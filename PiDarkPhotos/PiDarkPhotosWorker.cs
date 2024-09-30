using System.Device.Gpio;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using PiDarkPhotosUtilities;
using Serilog;

namespace PiDarkPhotos;

public class PiDarkPhotosWorker : BackgroundService
{
    private ScheduledPhotoDateTime _nextDateTime = new()
        { PhotoSeries = "DefaultPhotoSeries", ScheduledTime = new DateTime(2012, 1, 1, 0, 0, 0) };

    public required string CameraDirectives { get; set; }
    public required bool ErrorDetails { get; set; }
    public required string FileIdentifier { get; set; }
    public required int GpioPin { get; set; }
    public required int NumberOfPhotosPerDay { get; set; }

    private string ErrorImageFileName()
    {
        return Path.Combine(LocationTools.PhotoDirectory().FullName,
            $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-Error{(string.IsNullOrWhiteSpace(FileIdentifier.SanitizeForFileName()) ? "" : "-")}{FileIdentifier.SanitizeForFileName()}.jpg");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Timer? heartBeatWatchDogTimer = null;

        Timer? mainLoop = null;

        async void TakePhotographExceptionTrapWrapperCallback(object? state)
        {
            try
            {
                await TakePhotograph();
            }
            catch (Exception e)
            {
                Log.Error(e, "[General] Main Photograph Loop Error");

                await ExceptionTools.WriteExceptionToImage(string.Empty, e, ErrorImageFileName(), ErrorDetails);
            }
        }

        async Task TakePhotograph()
        {
            //Give the PhotoTakerCallback a chance to run without being interrupted by the watchdog
            heartBeatWatchDogTimer!.Change(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(1));

            Console.WriteLine();

            var currentPhotoDateTime = _nextDateTime;

            var seriesName = FileIdentifier.SanitizeForFileName();
            if (string.IsNullOrWhiteSpace(seriesName)) seriesName = Environment.MachineName.SanitizeForFileName();
            if (string.IsNullOrWhiteSpace(seriesName)) seriesName = "UnknownSeries".SanitizeForFileName();

            var fileName = Path.Combine(LocationTools.PhotoDirectory().FullName,
                $"{DateTime.Now:yyyy-MM-dd-HH-mm}---{currentPhotoDateTime.PhotoSeries}--{seriesName}.jpg");

            var photoExecutable = "libcamera-still";
            var photoArguments = $"-o {fileName} {CameraDirectives}".Trim();

            Log.Verbose($"Taking Photo at {DateTime.Now:O} - {photoExecutable} {photoArguments}");

            var photoDataList = new List<string>();

            try
            {
                using (var gpioController = new GpioController())
                {
                    gpioController.OpenPin(GpioPin, PinMode.Output);
                    gpioController.Write(GpioPin, PinValue.High);
                }

                await Task.Delay(500, stoppingToken);

                var process = new Process();
                process.StartInfo.FileName = photoExecutable;
                process.StartInfo.Arguments = photoArguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (_, eventArgs) =>
                {
                    Console.WriteLine(eventArgs.Data);
                    photoDataList.Add(eventArgs.Data ?? string.Empty);
                };
                process.ErrorDataReceived += (_, eventArgs) =>
                {
                    Console.WriteLine(eventArgs.Data);
                    photoDataList.Add(eventArgs.Data ?? string.Empty);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync(stoppingToken);

                if (process.ExitCode == 0)
                    Log.ForContext("libcamera-output", string.Join(Environment.NewLine, photoDataList))
                        .ForContext("settings", this.SafeObjectDump())
                        .Information("[Photograph] Photograph Taken - {executable} {arguments}", photoExecutable,
                            photoArguments);
                else
                    throw new Exception($"libcamera-still exited with code {process.ExitCode}");
            }
            catch (Exception e)
            {
                Log.ForContext("libcamera-output", string.Join(Environment.NewLine, photoDataList))
                    .ForContext("settings", this.SafeObjectDump()).Error(e,
                        "[Photograph] Problem Running libcamera-still - {executable} {arguments}", photoExecutable,
                        photoArguments);

                await ExceptionTools.WriteExceptionToImage(
                    $"[Photograph] Problem Running libcamera-still - {photoExecutable} {photoArguments}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, photoDataList)}",
                    e, ErrorImageFileName(),
                    ErrorDetails);
            }
            finally
            {
                using var gpioController = new GpioController();
                gpioController.OpenPin(GpioPin, PinMode.Output);
                gpioController.Write(GpioPin, PinValue.Low);
            }

            var frozenNow = DateTime.Now;
            _nextDateTime = ScheduledPhotoDateTimeTools.NextPhotoDateTime(frozenNow, NumberOfPhotosPerDay);

            Log.Information("[Timing] Next Photograph Time Set {@NextTime}", _nextDateTime);

            if (currentPhotoDateTime.ScheduledTime.Day != _nextDateTime.ScheduledTime.Day)
            {
                var upcomingSchedule = ScheduledPhotoDateTimeTools.NextPhotoDateTimes(frozenNow, NumberOfPhotosPerDay);

                var scheduleDayGroup = upcomingSchedule.GroupBy(x => x.ScheduledTime.Date).ToList();

                scheduleDayGroup.ForEach(x =>
                    Console.WriteLine(
                        $"Schedule for {x.First().ScheduledTime:M/d}: {string.Join(", ", x.Select(y => y.ScheduledTime.ToString("h:mm tt")))}"));
            }

            mainLoop.Change(_nextDateTime.ScheduledTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);

            Console.WriteLine();

            //There could be a slightly awkward interaction with the timing change at the top of this method TO
            //delay the watch dog until after the photo run - but in general the assumption is that the delay
            //should be more than enough time to execute the photo taking process.
            heartBeatWatchDogTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        var frozenNow = DateTime.Now;

        _nextDateTime = ScheduledPhotoDateTimeTools.NextPhotoDateTime(frozenNow, NumberOfPhotosPerDay);

        Log.Information("[Timing] Next Photograph Time Set {@NextTime}", _nextDateTime);

        var upcomingSchedule = ScheduledPhotoDateTimeTools.NextPhotoDateTimes(frozenNow, NumberOfPhotosPerDay);

        var scheduleDayGroup = upcomingSchedule.GroupBy(x => x.ScheduledTime.Date).ToList();

        scheduleDayGroup.ForEach(x =>
            Console.WriteLine(
                $"Schedule for {x.First().ScheduledTime:M/d}: {string.Join(", ", x.Select(y => y.ScheduledTime.ToString("h:mm tt")))}"));

        mainLoop = new Timer(TakePhotographExceptionTrapWrapperCallback, null,
            _nextDateTime.ScheduledTime.Subtract(DateTime.Now),
            Timeout.InfiniteTimeSpan);

        Console.WriteLine(
            $"Next Scheduled Photo: {_nextDateTime.ScheduledTime:O} - {_nextDateTime.ScheduledTime.Subtract(DateTime.Now):g}");

        heartBeatWatchDogTimer = new Timer(_ =>
            {
                var timeUntilNextPhoto = _nextDateTime.ScheduledTime.Subtract(DateTime.Now);

                if (timeUntilNextPhoto.TotalSeconds >= 0)
                {
                    Console.WriteLine($"Photo in {_nextDateTime.ScheduledTime.Subtract(DateTime.Now):c}");
                    return;
                }

                if (timeUntilNextPhoto.TotalMinutes <= -5)
                {
                    var frozenNow = DateTime.Now;
                    var nextTime = ScheduledPhotoDateTimeTools.NextPhotoDateTime(frozenNow, NumberOfPhotosPerDay);

                    Log.ForContext("hint",
                            "A past next photo time can result from errors in the main photo loop - there should be Log entries prior to this entry that help diagnose any problems.")
                        .Warning(
                            $"[Timing] Photo time of {_nextDateTime.ScheduledTime} is in the Past (current time {frozenNow.ToShortOutput()}) - resetting Next Time to {nextTime.ScheduledTime.ToShortOutput()}");

                    _nextDateTime = nextTime;
                    mainLoop.Change(_nextDateTime.ScheduledTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);
                }
                else
                {
                    Log.ForContext("hint",
                            "Just in case a negative Next Photo time will resolve due to delays processing the main loop (or error conditions keeping this heartbeat/watchdog loop from running as on the expected schedule negative values less than 5 minutes are logged but tolerated...")
                        .Information(
                            $"[Timing] Logging Past Photo time of {_nextDateTime.ScheduledTime.ToShortOutput()} (current time {DateTime.Now.ToShortOutput()}) - Not Taking Any Action At This Time");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        await stoppingToken.WhenCancelled();

        await Log.CloseAndFlushAsync();
    }
}