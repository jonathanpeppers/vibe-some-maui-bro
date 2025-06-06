using System.Collections.ObjectModel;
using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;

namespace VibeSomeMauiBro.Views;

public partial class CollectionPage : ContentPage
{
    private readonly ICatService _catService;
    
    private ObservableCollection<Cat> _likedCats = new();
    public ObservableCollection<Cat> LikedCats 
    { 
        get => _likedCats;
        set 
        {
            _likedCats = value;
            OnPropertyChanged();
        }
    }

    public CollectionPage(ICatService catService)
    {
        _catService = catService;
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadLikedCatsAsync();
    }

    private async Task LoadLikedCatsAsync()
    {
        try
        {
            var likedCats = await _catService.GetLikedCatsAsync();
            LikedCats = new ObservableCollection<Cat>(likedCats);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load your cat collection: {ex.Message}", "OK");
        }
    }
}