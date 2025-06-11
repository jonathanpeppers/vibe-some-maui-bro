using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;

namespace VibeSomeMauiBro.Views;

public partial class CollectionPage : ContentPage
{
    private readonly ICatService _catService;

    private List<Cat> _likedCats = [];
    public List<Cat> LikedCats
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
            LikedCats = await _catService.GetLikedCatsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load your cat collection: {ex.Message}", "OK");
        }
    }
}
