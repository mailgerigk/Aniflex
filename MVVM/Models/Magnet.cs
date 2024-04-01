using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Aniflex.MVVM.Models;
[DataContract]
public sealed partial class Magnet : INotifyPropertyChanged
{
    [DataMember] public MagnetKind Kind { get; set; }
    [DataMember] public VideoQuality Quality { get; set; }
    [DataMember] public string Uri { get; set; } = string.Empty;
    [DataMember] public List<string> LocalFiles { get; set; } = new();
    [DataMember] public bool DownloadRequested { get; set; }
    [DataMember] public bool KeepAlive { get; set; }
    [DataMember] public bool IsWatched { get; set; }
    public Download? Download { get; set; }

    public bool CanBeAutoDeleted => !KeepAlive && IsWatched;
    public bool IsDownloaded => LocalFiles.Any();

    private void DeleteLocalFiles()
    {
        foreach (string localFile in LocalFiles)
        {
            try
            {
                File.Delete(localFile);
            }
            catch
            {
            }
        }
        LocalFiles.Clear();
    }

    public bool UpdateFrom(Magnet magnet)
    {
        if (Kind != magnet.Kind || Quality != magnet.Quality)
            return false;

        if (Uri != magnet.Uri)
        {
            Uri = magnet.Uri;
            if (IsDownloaded)
            {
                DeleteLocalFiles();
                DownloadRequested = true;
            }
        }
        CheckLocalFiles();
        return true;
    }

    public void CheckLocalFiles()
    {
        if (IsDownloaded)
        {
            LocalFiles.RemoveAll(filePath => !File.Exists(filePath));
            if(LocalFiles.Any(filePath => filePath.EndsWith("aria2")))
            {
                LocalFiles.Clear();
            }
        }
        OnPropertyChanged(nameof(IsDownloaded));
    }

    public void FreeSpace()
    {
        if (CanBeAutoDeleted)
        {
            DeleteLocalFiles();
            OnPropertyChanged(nameof(IsDownloaded));
        }
    }
}
