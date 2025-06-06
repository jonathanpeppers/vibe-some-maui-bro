using System.Collections.ObjectModel;
using System.ComponentModel;
using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;

namespace VibeSomeMauiBro.Views;

public partial class CollectionPage(ICatService catService) : ContentPage, INotifyPropertyChanged
{
    private readonly ICatService _catService = catService;
    
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeComponent();
        BindingContext = this;
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}