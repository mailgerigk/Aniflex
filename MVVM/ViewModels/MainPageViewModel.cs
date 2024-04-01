using Aniflex.Core;
using Aniflex.Diagnostic;
using Aniflex.MVVM.Models;
using Aniflex.Provider;
using Aniflex.Services;

using Prism.Commands;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Aniflex.MVVM.ViewModels;
public sealed partial class MainPageViewModel : INotifyPropertyChanged
{
    private const string PathPart = "/.cache/settings.json";
    private static readonly string Path = $"{AppContext.BaseDirectory}{PathPart}";
    private readonly WebContext webContext;
    private readonly IProvider provider;
    private readonly DownloadManager downloadManager;
    private readonly Dispatcher dispatcher;
    private readonly HomeViewModel homeViewModel;
    private readonly WatchingViewModel watchingViewModel;
    private readonly FavoritesViewModel favoritesViewModel;
    private readonly SeasonViewModel seasonViewModel;
    private readonly ShowsViewModel showsViewModel;
    private readonly DownloadsViewModel downloadsViewModel;
    private readonly LogViewModel logViewModel;

    public ICommand ReloadCommand { get; set; }
    public ICommand ExitCommand { get; set; }

    public SearchText SearchText { get; set; } = new();
    public object? CurrentView { get; set; }
    public ICommand HomeCommand { get; set; }
    public ICommand WatchingCommand { get; set; }
    public ICommand FavoritesCommand { get; set; }
    public ICommand SeasonCommand { get; set; }
    public ICommand ShowsCommand { get; set; }
    public ICommand DownloadsCommand { get; set; }
    public ICommand LogCommand { get; set; }

    public ObservableList<Anime> Animes { get; set; } = new();

    public MainPageViewModel()
    {
        webContext = new();
        provider = new Subsplease();
        downloadManager = new();

        dispatcher = Dispatcher.CurrentDispatcher;

        homeViewModel = new();
        watchingViewModel = new(SearchText, Animes);
        favoritesViewModel = new(SearchText, Animes);
        seasonViewModel = new(SearchText, Animes);
        showsViewModel = new(SearchText, Animes);
        downloadsViewModel = new(downloadManager, Animes);
        logViewModel = new();

        ReloadCommand = new DelegateCommand(() =>
        {
            webContext.ClearCache();
            BeginAnimeUpdate();
        });
        ExitCommand = new DelegateCommand(() => { Application.Current.MainWindow.Close(); });

        HomeCommand = new DelegateCommand(() => { CurrentView = homeViewModel; });
        WatchingCommand = new DelegateCommand(() => { CurrentView = watchingViewModel; });
        FavoritesCommand = new DelegateCommand(() => { CurrentView = favoritesViewModel; });
        SeasonCommand = new DelegateCommand(() => { CurrentView = seasonViewModel; });
        ShowsCommand = new DelegateCommand(() => { CurrentView = showsViewModel; });
        DownloadsCommand = new DelegateCommand(() => { CurrentView = downloadsViewModel; });
        LogCommand = new DelegateCommand(() => { CurrentView = logViewModel; });

        if (File.Exists(Path))
        {
            try
            {
                string fileContent = File.ReadAllText(Path);
                Anime[]? animes = JsonSerializer.Deserialize<Anime[]>(fileContent);
                if (animes is not null)
                {
                    foreach (Anime anime in animes)
                    {
                        if (anime.Thumbnail is not null)
                        {
                            _ = anime.Thumbnail.LoadSource(webContext);
                        }
                        Animes.Add(anime);
                    }
                }
            }
            catch
            {
            }
        }

        BeginAnimeUpdate();

        OnExit.Do += () =>
            {
                EnsurePath.Exists(Path);
                string fileContent = JsonSerializer.Serialize(Animes.ToArray());
                File.WriteAllText(Path, fileContent);
            };
    }

    private async Task AnimeUpdate()
    {
        var progress = new ProviderProgress();
        progress.OnProgress += AnimeUpdate_OnProgress;
        await provider.GetShows(webContext, progress);

        downloadManager.SetMagnets(Animes.SelectMany(anime => anime.Episodes.SelectMany(episode => episode.Magnets)));
    }

    private void AnimeUpdate_OnProgress(ProviderProgress progress, int current, Anime anime)
    {
        Log.Default.Write($"AnimeUpdate: {current}/{progress.Total} {anime.Name}");

        // load thumbnail
        if (anime.Thumbnail is not null)
        {
            _ = anime.Thumbnail.LoadSource(webContext);
        }

        dispatcher.BeginInvoke(() =>
        {
            if (!Animes.Any(a => a.UpdateFrom(anime)))
            {
                Animes.Add(anime);
            }
        }, DispatcherPriority.Background);
    }

    private void BeginAnimeUpdate()
    {
        FreeStorageSpace();
        _ = AnimeUpdate();
    }

    private void FreeStorageSpace()
    {
        ulong totalSize = 0;
        var allFiles = new List<FileInfo>();
        foreach (var anime in Animes)
        {
            foreach (var episode in anime.Episodes)
            {
                if (episode.KeepAlive) continue;
                foreach (var magnet in episode.Magnets)
                {
                    if(magnet.DownloadRequested && magnet.IsWatched)
                    {
                        magnet.DownloadRequested = false;
                    }

                    foreach (var localFile in magnet.LocalFiles)
                    {
                        var info = new FileInfo(localFile);
                        if (!info.Exists)
                        {
                            continue;
                        }
                        if (info.Length >= 0)
                        {
                            totalSize += (ulong)info.Length;
                        }
                        if (!magnet.KeepAlive)
                        {
                            allFiles.Add(info);
                        }
                    }
                }
            }
        }

        const ulong kilobyte = 1024;
        const ulong megabyte = 1024 * kilobyte;
        const ulong gigabyte = 1024 * megabyte;
        const ulong storageMax = 128 * gigabyte;

        if (totalSize > storageMax)
        {
            allFiles.Sort((a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
            while (totalSize > storageMax)
            {
                if (!allFiles.Any())
                {
                    MessageBox.Show($"More than {storageMax / gigabyte}gb of episodes are keep alive, can't delete anything.");
                    break;
                }
                var first = allFiles.First();
                allFiles.RemoveAt(0);
                Log.Default.Write($"Deleting {first.Name}");
                totalSize -= (ulong)first.Length;
                first.Delete();
            }
        }
    }
}
