using Aniflex.Core;
using Aniflex.MVVM.Models;

using System.ComponentModel;
using System.Linq;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class WatchingViewModel : INotifyPropertyChanged
{
    public CardCollectionViewModel Cards { get; set; } = new();

    public WatchingViewModel(SearchText searchText, ObservableList<Anime> animes)
    {
        Cards.Filter = searchText;

        animes.WhenAdded(anime =>
        {
            anime.PropertyChanged += Anime_PropertyChanged;
            if (!anime.IsWatching)
                return;

            Cards.Add(new CardViewModel
            {
                Anime = anime
            });
        });
    }

    private void Anime_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Anime anime)
        {
            CardViewModel? first = Cards.FirstOrDefault(card => card.Anime == anime);
            if (first is null && anime.IsWatching)
            {
                Cards.Add(new CardViewModel
                {
                    Anime = anime
                });
            }
            else if (first is not null && !anime.IsWatching)
            {
                Cards.Remove(first);
            }
        }
    }
}
