using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PiDarkPhotos;
using PiDarkPhotosUtilities;
using Serilog;

var parseResult = Parser.Default.ParseArguments<Options>(args);

if (parseResult.Errors.Any())
{
    foreach (var resultError in parseResult.Errors)
    {
        if (resultError.Tag is ErrorType.HelpRequestedError or ErrorType.HelpVerbRequestedError
            or ErrorType.VersionRequestedError) continue;

        Console.WriteLine($"Error: {resultError}");
    }

    return;
}

LogTools.StandardStaticLoggerForProgramDirectory("PiDarkPhotos");

Console.WriteLine($"Startup Options -> File Identifier: {parseResult.Value.FileIdentifier}");
Console.WriteLine($"Startup Options -> Gpio Pin: {parseResult.Value.GpioPin}");
Console.WriteLine($"Startup Options -> Number of Photos per Day: {parseResult.Value.NumberOfPhotosPerDay}");
Console.WriteLine($"Startup Options -> Camera Directives: {parseResult.Value.CameraDirectives}");

Log.ForContext(nameof(parseResult), parseResult.SafeObjectDump()).Debug(
    "Command Line Options: File Identifier {0}, Gpio Pin: {1}, Number of Photos per Day: {2}, Camera Directives: {3}",
    parseResult.Value.FileIdentifier, parseResult.Value.GpioPin, parseResult.Value.NumberOfPhotosPerDay,
    parseResult.Value.CameraDirectives);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSystemd();
builder.Services.AddHostedService<PiDarkPhotosWorker>(_ => new PiDarkPhotosWorker
{
    FileIdentifier = parseResult.Value.FileIdentifier,
    GpioPin = parseResult.Value.GpioPin,
    NumberOfPhotosPerDay = parseResult.Value.NumberOfPhotosPerDay,
    CameraDirectives = parseResult.Value.CameraDirectives,
    ErrorDetails = parseResult.Value.ErrorDetails
});

var host = builder.Build();

try
{
    host.Run();
}
catch (Exception e)
{
    Log.Error(e, "Exception with host.Run");
}
finally
{
    Log.CloseAndFlush();
}