namespace PiDarkPhotosUtilities;

public static class LocationTools
{
    public static DirectoryInfo PhotoDirectory()
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var dataDirectory = new DirectoryInfo(Path.Combine(baseDirectory.Parent!.FullName, "PiDarkPhotosStorage"));

        if (!dataDirectory.Exists) dataDirectory.Create();

        return dataDirectory;
    }
}