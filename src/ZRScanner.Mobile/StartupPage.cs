using Microsoft.Maui.Storage;
using System.Globalization;

namespace ZRScanner.Mobile;

public sealed class StartupPage : ContentPage
{
    private const string LanguagePreferenceKey = "zrscanner-language";
    private readonly MainPage _homePage;
    private readonly ProgressBar _progress = new()
    {
        Progress = 0,
        ProgressColor = Color.FromArgb("#00BFAF"),
        BackgroundColor = Color.FromArgb("#DDE8E2"),
        HeightRequest = 8
    };
    private readonly VerticalStackLayout _loadingArea;
    private readonly VerticalStackLayout _languageArea;
    private readonly Label _loadingText;
    private readonly Label _languageTitle;
    private readonly Label _languageHint;
    private readonly Button _continueButton;
    private readonly Dictionary<string, Button> _languageButtons = new();
    private string _language;
    private string? _selectedLanguage;
    private bool _hasStarted;

    public StartupPage(MainPage homePage)
    {
        _homePage = homePage;
        _language = GetPreferredLanguage();

        BackgroundColor = Color.FromArgb("#F5F7F5");
        Title = "ZRScanner";

        var logo = new Image
        {
            Source = "zrscanner-logo.svg",
            WidthRequest = 184,
            HeightRequest = 184,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center
        };

        var brand = new Label
        {
            Text = "ZRScanner",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#071B55"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        _loadingText = new Label
        {
            Text = AppText.Get(_language, "splash_loading"),
            FontSize = 15,
            TextColor = Color.FromArgb("#52665E"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        _progress.WidthRequest = 240;
        _progress.HorizontalOptions = LayoutOptions.Center;

        _loadingArea = new VerticalStackLayout
        {
            Spacing = 16,
            HorizontalOptions = LayoutOptions.Center,
            Children = { _loadingText, _progress }
        };

        _languageTitle = new Label
        {
            Text = AppText.Get(_language, "choose_language"),
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#17372E"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        _languageHint = new Label
        {
            Text = AppText.Get(_language, "language_helper"),
            FontSize = 14,
            TextColor = Color.FromArgb("#52665E"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        var french = CreateLanguageButton("🇫🇷   Français", "fr");
        var dutch = CreateLanguageButton("🇳🇱   Nederlands", "nl");
        var english = CreateLanguageButton("🇬🇧   English", "en");

        _continueButton = new Button
        {
            Text = AppText.Get(_language, "continue"),
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#174A3A"),
            TextColor = Colors.White,
            CornerRadius = 14,
            HeightRequest = 54,
            IsEnabled = false,
            Opacity = 0.55
        };
        _continueButton.Clicked += async (_, _) => await ContinueAsync();

        _languageArea = new VerticalStackLayout
        {
            Spacing = 11,
            IsVisible = false,
            Children =
            {
                _languageTitle,
                _languageHint,
                french,
                dutch,
                english,
                _continueButton
            }
        };

        Content = new Grid
        {
            Padding = new Thickness(26, 34),
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 18,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        logo,
                        brand,
                        _loadingArea,
                        _languageArea
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_hasStarted) return;
        _hasStarted = true;

        await _progress.ProgressTo(1, 3000, Easing.CubicInOut);

        if (Preferences.Default.ContainsKey(LanguagePreferenceKey))
        {
            await OpenHomeAsync();
            return;
        }

        _loadingArea.IsVisible = false;
        _languageArea.IsVisible = true;
    }

    private static string GetPreferredLanguage()
    {
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, string.Empty);
        if (savedLanguage is "fr" or "nl" or "en") return savedLanguage;

        var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return deviceLanguage is "nl" or "en" ? deviceLanguage : "fr";
    }

    private Button CreateLanguageButton(string label, string language)
    {
        var button = new Button
        {
            Text = label,
            FontSize = 16,
            HeightRequest = 54,
            CornerRadius = 14,
            BorderWidth = 1.5,
            BorderColor = Color.FromArgb("#DDE5E0"),
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#17372E"),
            Padding = new Thickness(18, 0)
        };
        button.Clicked += (_, _) => SelectLanguage(language);
        _languageButtons.Add(language, button);
        return button;
    }

    private void SelectLanguage(string language)
    {
        _selectedLanguage = language;
        _language = language;

        foreach (var pair in _languageButtons)
        {
            var isSelected = pair.Key == language;
            pair.Value.BackgroundColor = isSelected
                ? Color.FromArgb("#E5F2EC")
                : Colors.White;
            pair.Value.BorderColor = isSelected
                ? Color.FromArgb("#00A99D")
                : Color.FromArgb("#DDE5E0");
            pair.Value.BorderWidth = isSelected ? 2 : 1;
            pair.Value.TextColor = Color.FromArgb("#17372E");
        }

        _languageTitle.Text = AppText.Get(language, "choose_language");
        _languageHint.Text = AppText.Get(language, "language_helper");
        _continueButton.Text = AppText.Get(language, "continue");
        _continueButton.IsEnabled = true;
        _continueButton.Opacity = 1;
    }

    private async Task ContinueAsync()
    {
        if (_selectedLanguage is null) return;
        await _homePage.SetLanguageAsync(_selectedLanguage);
        await OpenHomeAsync();
    }

    private async Task OpenHomeAsync()
    {
        await Navigation.PushAsync(_homePage, animated: false);
        Navigation.RemovePage(this);
    }
}
