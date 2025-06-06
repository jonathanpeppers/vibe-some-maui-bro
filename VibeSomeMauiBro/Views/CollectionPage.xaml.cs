using System.Collections.ObjectModel;
using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;

namespace VibeSomeMauiBro.Views;

public partial class CollectionPage : ContentPage
{
    private readonly ICatService _catService;
    
    public ObservableCollection<Cat> LikedCats { get; } = new();

    public CollectionPage(ICatService catService)
    {
        InitializeComponent();
        _catService = catService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLikedCatsAsync();
    }

    private async Task LoadLikedCatsAsync()
    {
        try
        {
            var likedCats = await _catService.GetLikedCatsAsync();
            LikedCats.Clear();
            foreach (var cat in likedCats)
            {
                LikedCats.Add(cat);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load your cat collection: {ex.Message}", "OK");
        }
    }
}