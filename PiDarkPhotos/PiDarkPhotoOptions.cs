using System.ComponentModel;
using Ookii.CommandLine;

namespace PiDarkPhotos;

[GeneratedParser]
[ParseOptions(Mode = ParsingMode.LongShort,
    CaseSensitive = true,
    ArgumentNameTransform = NameTransform.None,ValueDescriptionTransform = NameTransform.None)]
public partial class PiDarkPhotoOptions
{
    [CommandLineArgument(ShortName = 'c', IsRequired = false, IsPositional = false)]
    [Description("Added to the libcamera command")]
    public string CameraDirectives { get; set; } = string.Empty;

    [CommandLineArgument(ShortName = 'e', IncludeDefaultInUsageHelp = true)]
    [Description("If set to true this will write all available exception information to error images, this may reveal sensitive information")]
    public bool ErrorDetails { get; set; }

    [CommandLineArgument(ShortName = 'f', IncludeDefaultInUsageHelp = true)]
    [Description("Text added to the save photo filenames to help identify the file.")]
    public string FileIdentifier { get; set; } = "DarkPhoto";

    [CommandLineArgument(ShortName = 'p', IncludeDefaultInUsageHelp = true)]
    [Description("The gpio pin the light, or lights, are connected to.")]
    public int GpioPin { get; set; } = 17;

    [CommandLineArgument(ShortName = 'n', IncludeDefaultInUsageHelp = true)]
    [Description("The number of photos per day")]
    public int NumberOfPhotosPerDay { get; set; } = 2;
}