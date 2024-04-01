using Aniflex.Core;
using Aniflex.Services;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Aniflex.MVVM.Models;
[DataContract]
public sealed partial class Thumbnail : INotifyPropertyChanged
{
    private const string PathPart = "/.cache/.images/";
    private static readonly string Path = $"{AppContext.BaseDirectory}{PathPart}";

    [DataMember] public string? Uri { get; set; }
    public string? LocalFile
    {
        get
        {
            if (Uri is null)
                return null;

            string uri = Uri;
            string extension = System.IO.Path.GetExtension(uri);
            ulong hash = StringHash.ToHash(uri);
            return $"{Path}{hash}{extension}";
        }
    }
    public bool IsDownloaded => LocalFile is not null && File.Exists(LocalFile) && new FileInfo(LocalFile).Length > 0;

    private int sourceLock;
    [JsonIgnore]
    public BitmapSource? Source { get; set; }

    private void DeleteLocalFile()
    {
        try
        {
            if (LocalFile is not null)
            {
                if (File.Exists(LocalFile))
                {
                    File.Delete(LocalFile);
                }
            }
        }
        catch
        {
        }
    }

    public async Task<BitmapSource> LoadSource(WebContext webContext)
    {
        if (Source is not null)
            return Source;

        if (Interlocked.CompareExchange(ref sourceLock, 1, 0) == 0)
        {
            if (!IsDownloaded)
            {
                Debug.Assert(Uri is not null);
                Debug.Assert(LocalFile is not null);
                await webContext.DownloadFile(Uri, LocalFile).ConfigureAwait(false);
            }
            Source = new BitmapImage(new Uri(LocalFile!));
            Source.Freeze();
            return Source;
        }
        return await LoadSource().ConfigureAwait(false);
    }
    public async Task<BitmapSource> LoadSource()
    {
        if (Source is not null)
            return Source;

        do
        {
            await Task.Delay(100).ConfigureAwait(false);
        } while (Source is null);

        return Source;
    }

    public bool UpdateFrom(Thumbnail thumbnail)
    {
        if (Uri == thumbnail.Uri)
        {
            DeleteLocalFile();
            Uri = thumbnail.Uri;
        }
        return true;
    }
}
