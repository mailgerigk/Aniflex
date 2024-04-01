using System.IO;

namespace Aniflex.Core;

public static class EnsurePath
{
    public static void Exists(string path)
    {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Directory is DirectoryInfo directoryInfo && !directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
    }
}
