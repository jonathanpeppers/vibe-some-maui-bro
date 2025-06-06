using System.Collections.ObjectModel;
using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;
using VibeSomeMauiBro.Views;

namespace VibeSomeMauiBro;

public partial class MainPage : ContentPage
{
	private readonly ICatService _catService;
	private readonly List<Cat> _currentCats = new();
	private int _currentCatIndex = 0;
	private ContentView? _currentCatCard;

	public MainPage(ICatService catService)
	{
		InitializeComponent();
		_catService = catService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadCatsAsync();
	}

	private async Task LoadCatsAsync()
	{
		try
		{
			LoadingIndicator.IsVisible = true;
			NoMoreCatsMessage.IsVisible = false;

			var cats = await _catService.GetCatsAsync(10);
			_currentCats.AddRange(cats);

			if (_currentCats.Count > 0)
			{
				ShowCurrentCat();
			}
			else
			{
				ShowNoMoreCats();
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to load cats: {ex.Message}", "OK");
		}
		finally
		{
			LoadingIndicator.IsVisible = false;
		}
	}

	private void ShowCurrentCat()
	{
		if (_currentCatIndex >= _currentCats.Count)
		{
			ShowNoMoreCats();
			return;
		}

		var cat = _currentCats[_currentCatIndex];
		
		// Remove previous cat card if exists
		if (_currentCatCard != null)
		{
			CardContainer.Children.Remove(_currentCatCard);
		}

		// Create new cat card
		_currentCatCard = CreateCatCard(cat);
		CardContainer.Children.Add(_currentCatCard);

		LoadingIndicator.IsVisible = false;
		NoMoreCatsMessage.IsVisible = false;
	}

	private ContentView CreateCatCard(Cat cat)
	{
		var contentView = new ContentView
		{
			BackgroundColor = Colors.White
		};

		AbsoluteLayout.SetLayoutBounds(contentView, new Rect(0.5, 0.5, 0.8, 0.7));
		AbsoluteLayout.SetLayoutFlags(contentView, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);

		var grid = new Grid();
		grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		// Cat image
		var image = new Image
		{
			Source = cat.ImageUrl,
			Aspect = Aspect.AspectFill,
			BackgroundColor = Colors.LightGray
		};
		Grid.SetRow(image, 0);

		// Cat info
		var infoStack = new StackLayout
		{
			Padding = new Thickness(20),
			BackgroundColor = Colors.White
		};

		var breedLabel = new Label
		{
			Text = cat.Breed ?? "Unknown Breed",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.Black
		};

		var descriptionLabel = new Label
		{
			Text = cat.Description ?? "A beautiful cat",
			FontSize = 16,
			TextColor = Colors.Gray,
			MaxLines = 2
		};

		infoStack.Children.Add(breedLabel);
		infoStack.Children.Add(descriptionLabel);
		Grid.SetRow(infoStack, 1);

		grid.Children.Add(image);
		grid.Children.Add(infoStack);
		contentView.Content = grid;

		return contentView;
	}

	private void ShowNoMoreCats()
	{
		if (_currentCatCard != null)
		{
			CardContainer.Children.Remove(_currentCatCard);
			_currentCatCard = null;
		}
		
		LoadingIndicator.IsVisible = false;
		NoMoreCatsMessage.IsVisible = true;
	}

	private async void OnLikeClicked(object? sender, EventArgs e)
	{
		if (_currentCatIndex < _currentCats.Count)
		{
			var cat = _currentCats[_currentCatIndex];
			await _catService.LikeCatAsync(cat);
			
			// Animate card out (simplified)
			if (_currentCatCard != null)
			{
				await _currentCatCard.TranslateTo(300, 0, 250);
			}
			
			NextCat();
		}
	}

	private async void OnDislikeClicked(object? sender, EventArgs e)
	{
		if (_currentCatIndex < _currentCats.Count)
		{
			var cat = _currentCats[_currentCatIndex];
			await _catService.DislikeCatAsync(cat);
			
			// Animate card out (simplified)
			if (_currentCatCard != null)
			{
				await _currentCatCard.TranslateTo(-300, 0, 250);
			}
			
			NextCat();
		}
	}

	private void NextCat()
	{
		_currentCatIndex++;
		ShowCurrentCat();
	}

	private async void OnCollectionClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("collection");
	}

	private async void OnLoadMoreCatsClicked(object? sender, EventArgs e)
	{
		await LoadCatsAsync();
	}
}
