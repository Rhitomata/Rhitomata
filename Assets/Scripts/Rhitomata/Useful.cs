using System;
using System.IO;

public static class Useful {
    public static string GetRelativePath(string filePath, string rootDirPath) {
        var pathUri = new Uri(filePath);
        // Folders must end in a slash
        if (!rootDirPath.EndsWith(Path.DirectorySeparatorChar)) {
            rootDirPath += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(rootDirPath);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)).Replace('\\', '/');
    }
}
