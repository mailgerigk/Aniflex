using Aniflex.Core;
using Aniflex.MVVM.Models;

using Prism.Commands;

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class EpisodeViewModel : INotifyPropertyChanged
{
    private readonly Episode episode;

    public string? Id => episode.Id;
    public bool IsWatched
    {
        get => episode.IsWatched;
        set => episode.IsWatched = value;
    }
    public bool KeepAlive
    {
        get => episode.KeepAlive;
        set => episode.KeepAlive = value;
    }
    public ICommand RequestDownload { get; set; }
    public ICommand Watch { get; set; }
    public bool CanRequestDownload => !episode.IsDownloadingOrDownloaded;
    public bool CanWatch => episode.Magnets.Any(magnet => magnet.IsDownloaded);

    public EpisodeViewModel(Episode episode)
    {
        this.episode = episode;
        RequestDownload = new DelegateCommand(() =>
        {
            this.episode.RequestBestQuality();
        });
        Watch = new DelegateCommand(() =>
        {
            Magnet? magnet = this.episode.Magnets.LastOrDefault(magnet => magnet.IsDownloaded);
            if (magnet is null)
                return;

            foreach (string localFile in magnet.LocalFiles)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = localFile,
                    UseShellExecute = true,
                });
            }
        });
        episode.Magnets.ForEach(magnet => magnet.PropertyChanged += Magnet_PropertyChanged);
    }

    private void Magnet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanRequestDownload));
        OnPropertyChanged(nameof(CanWatch));
    }
}
