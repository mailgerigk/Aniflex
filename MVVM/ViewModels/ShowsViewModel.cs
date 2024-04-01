using Aniflex.Core;
using Aniflex.MVVM.Models;

using System.ComponentModel;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class ShowsViewModel : INotifyPropertyChanged
{
    public CardCollectionViewModel Cards { get; set; } = new();

    public ShowsViewModel(SearchText searchText, ObservableList<Anime> animes)
    {
        Cards.Filter = searchText;

        animes.WhenAdded(anime =>
        {
            Cards.Add(new CardViewModel
            {
                Anime = anime,
            });
        });
    }
}
