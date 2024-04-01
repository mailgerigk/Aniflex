using Aniflex.MVVM.Models;

using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class AnimeViewModel : INotifyPropertyChanged
{
    private readonly Anime anime;

    public string? Name => anime.Name;
    public string? Description => anime.Description;
    public BitmapSource? Thumbnail => anime.Thumbnail?.Source;

    public ICollectionView? Episodes
    {
        get
        {
            var view = CollectionViewSource.GetDefaultView(anime.Episodes.Select(episode => new EpisodeViewModel(episode)));
            view.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
            return view;
        }
    }

    public AnimeViewModel(Anime anime)
    {
        this.anime = anime;
        anime.PropertyChanged += Anime_PropertyChanged;
        anime.Episodes.CollectionChanged += Episodes_CollectionChanged;
    }

    private void Episodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Episodes));
    }

    private void Anime_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Episodes));
    }
}
