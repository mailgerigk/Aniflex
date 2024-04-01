﻿using Aniflex.Core;
using Aniflex.MVVM.Models;

using System.ComponentModel;
using System.Linq;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class FavoritesViewModel : INotifyPropertyChanged
{
    public CardCollectionViewModel Cards { get; set; } = new();

    public FavoritesViewModel(SearchText searchText, ObservableList<Anime> animes)
    {
        Cards.Filter = searchText;

        animes.WhenAdded(anime =>
        {
            anime.PropertyChanged += Anime_PropertyChanged;
            if (!anime.IsFavorite)
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
            if (first is null && anime.IsFavorite)
            {
                Cards.Add(new CardViewModel
                {
                    Anime = anime
                });
            }
            else if (first is not null && !anime.IsFavorite)
            {
                Cards.Remove(first);
            }
        }
    }
}
