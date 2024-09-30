using CommandLine;

namespace PiDarkPhotos;

internal class Options
{
    [Option('c', "cameradirectives", Required = false, HelpText = "Added to the libcamera command")]
    public string CameraDirectives { get; set; } = string.Empty;

    [Option('e', "errordetails", Required = false,
        HelpText =
            "If set to true this will write all available exception information to error images, this may reveal sensitive information",
        Default = true)]
    public bool ErrorDetails { get; set; }

    [Option('f', "fileidentifier", Required = false,
        HelpText =
            "Text added to the save photo filenames to help identify the file.", Default = "Vibration Detected")]
    public string FileIdentifier { get; set; } = "Vibration Detected";

    [Option('p', "gpiopin", Required = false,
        HelpText = "The gpio pin the light, or lights, are connected to.", Default = 17)]
    public int GpioPin { get; set; }

    [Option('n', "numberofphotosperday", Required = false, HelpText = "The number of photos per day", Default = 2)]
    public int NumberOfPhotosPerDay { get; set; }
}