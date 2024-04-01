using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class DownloadViewModel : INotifyPropertyChanged
{
    public ulong Current { get; set; }
    public ulong Total { get; set; }

    public string ProgressText => Current == Total ? ValueToText(Total) : $"{ValueToText(Current)} / {ValueToText(Total)}";

    public BitmapSource? Image { get; set; }
    public string? AnimeName { get; set; }
    public string? EpisodeId { get; set; }

    static string ValueToText(ulong byteCount)
    {
        double value = byteCount;
        if (value < 1024)
            return value.ToString();
        value /= 1024;
        if (value < 1024)
            return $"{value:0.00}KiB";
        value /= 1024;
        if (value < 1024)
            return $"{value:0.00}MiB";
        value /= 1024;
        return $"{value:0.00}GiB";
    }
}
