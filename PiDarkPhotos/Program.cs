using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PiDarkPhotos;
using PiDarkPhotosUtilities;
using Serilog;

var parseResult = new PiDarkPhotoOptions();

if (args.Length > 0)
{
    var parser = PiDarkPhotoOptions.CreateParser();
    var parserResults = parser.Parse(CommandLineTools.QuotePreservingParseForCommandLine(Environment.CommandLine)
        .Skip(1).ToArray());
    CommandLineTools.CleanStringProperties(parserResults);
    if (parserResults is not null) parseResult = parserResults;
}

LogTools.StandardStaticLoggerForProgramDirectory("PiDarkPhotos");

Console.WriteLine($"Startup Options -> File Identifier: {parseResult.FileIdentifier}");
Console.WriteLine($"Startup Options -> Gpio Pin: {parseResult.GpioPin}");
Console.WriteLine($"Startup Options -> Number of Photos per Day: {parseResult.NumberOfPhotosPerDay}");
Console.WriteLine($"Startup Options -> Camera Directives: {parseResult.CameraDirectives}");
Console.WriteLine($"Startup Options -> Show All Error Details: {parseResult.ErrorDetails}");

Log.ForContext(nameof(parseResult), parseResult.SafeObjectDump()).Debug(
    "Command Line Options: File Identifier {0}, Gpio Pin: {1}, Number of Photos per Day: {2}, Camera Directives: {3}",
    parseResult.FileIdentifier, parseResult.GpioPin, parseResult.NumberOfPhotosPerDay,
    parseResult.CameraDirectives);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSystemd();
builder.Services.AddHostedService<PiDarkPhotosWorker>(_ => new PiDarkPhotosWorker
{
    FileIdentifier = parseResult.FileIdentifier,
    GpioPin = parseResult.GpioPin,
    NumberOfPhotosPerDay = parseResult.NumberOfPhotosPerDay,
    CameraDirectives = parseResult.CameraDirectives,
    ErrorDetails = parseResult.ErrorDetails
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