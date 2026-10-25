using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.Globalization;
using ZRScanner.Application.Ports;
using ZRScanner.Application.UseCases;

namespace ZRScanner.Mobile;

public sealed class MainPage : ContentPage
{
    private const string LanguagePreferenceKey = "zrscanner-language";
    private readonly ScanAndSaveDocument _scan;
    private readonly IDocumentStorage _storage;
    private readonly VerticalStackLayout _items = new() { Spacing = 10 };
    private readonly Label _status = new() { TextColor = Color.FromArgb("#697A74"), FontSize = 14 };
    private readonly Border _emptyState;
    private readonly Picker _languagePicker = new() { WidthRequest = 150, HorizontalOptions = LayoutOptions.End };
    private readonly Label _languageLabel = new() { VerticalOptions = LayoutOptions.Center };
    private readonly Label _heading;
    private readonly Label _intro;
    private readonly Label _scanTitle;
    private readonly Label _scanDescription;
    private readonly Label _documentsTitle;
    private readonly Label _localNote;
    private readonly Button _scanButton;
    private string _language;
    private string? _statusMessageKey;

    public MainPage(ScanAndSaveDocument scan, IDocumentStorage storage)
    {
        _scan = scan;
        _storage = storage;
        _language = GetInitialLanguage();

        _languagePicker.Items.Add("Français");
        _languagePicker.Items.Add("Nederlands");
        _languagePicker.Items.Add("English");
        _languagePicker.SelectedIndex = _language switch { "nl" => 1, "en" => 2, _ => 0 };

        _emptyState = new Border
        {
            Padding = 20,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E0E8E3"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Content = new Label
            {
                TextColor = Color.FromArgb("#697A74"),
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        _heading = new Label
        {
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#17372E"),
            LineHeight = 1.05
        };
        _intro = new Label { FontSize = 16, TextColor = Color.FromArgb("#52665E") };
        _scanTitle = new Label
        {
            FontSize = 19,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#17372E")
        };
        _scanDescription = new Label { FontSize = 14, TextColor = Color.FromArgb("#52665E") };
        _documentsTitle = new Label
        {
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#17372E"),
            VerticalOptions = LayoutOptions.Center
        };
        _localNote = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#697A74"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center
        };

        _scanButton = new Button
        {
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#174A3A"),
            TextColor = Colors.White,
            CornerRadius = 16,
            HeightRequest = 58
        };
        _scanButton.Clicked += async (_, _) => await ScanAsync();

        Title = "ZRScanner";
        BackgroundColor = Color.FromArgb("#F5F7F5");
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(22, 24),
                Spacing = 16,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Children =
                        {
                            _languageLabel,
                            _languagePicker
                        }
                    },
                    _heading,
                    _intro,
                    CreateScannerIllustration(),
                    _scanTitle,
                    _scanDescription,
                    _scanButton,
                    _status,
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Children = { _documentsTitle, _localNote }
                    },
                    _emptyState,
                    _items
                }
            }
        };

        _languagePicker.SelectedIndexChanged += (_, _) =>
        {
            if (_languagePicker.SelectedIndex < 0) return;
            _language = _languagePicker.SelectedIndex switch { 1 => "nl", 2 => "en", _ => "fr" };
            Preferences.Default.Set(LanguagePreferenceKey, _language);
            _statusMessageKey = null;
            ApplyLanguage();
        };

        ApplyLanguage();
        Loaded += async (_, _) => await RefreshAsync();
    }

    private static string GetInitialLanguage()
    {
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, string.Empty);
        if (savedLanguage is "fr" or "nl" or "en") return savedLanguage;

        var deviceLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return deviceLanguage is "nl" or "en" ? deviceLanguage : "fr";
    }

    private string T(string key) => AppText.Get(_language, key);

    private void ApplyLanguage()
    {
        _languageLabel.Text = T("language");
        _languagePicker.Title = T("language");
        _heading.Text = T("welcome");
        _intro.Text = T("intro");
        _scanTitle.Text = T("first_scan");
        _scanDescription.Text = T("scan_description");
        _scanButton.Text = "＋  " + T("scan_button");
        _documentsTitle.Text = T("my_documents");
        _localNote.Text = T("saved_on_device");
        ((Label)_emptyState.Content).Text = T("empty_state");
        _status.Text = _statusMessageKey is null ? T("ready") : T(_statusMessageKey);
    }

    private static View CreateScannerIllustration()
    {
        var paper = new Border
        {
            WidthRequest = 112,
            HeightRequest = 142,
            Padding = new Thickness(12, 14),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#DDE7E1"),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#26071B55"),
                Offset = new Point(0, 5),
                Radius = 12,
                Opacity = 0.18f
            },
            Content = new VerticalStackLayout
            {
                Spacing = 7,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "ZR",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.FromArgb("#17372E")
                    },
                    new BoxView
                    {
                        HeightRequest = 4,
                        CornerRadius = 2,
                        Color = Color.FromArgb("#00BFAF")
                    },
                    new BoxView
                    {
                        HeightRequest = 4,
                        WidthRequest = 52,
                        HorizontalOptions = LayoutOptions.Center,
                        CornerRadius = 2,
                        Color = Color.FromArgb("#D0DDD6")
                    }
                }
            }
        };

        return new Border
        {
            HeightRequest = 190,
            Padding = 16,
            BackgroundColor = Color.FromArgb("#E6F0EB"),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            Content = new Grid
            {
                Children =
                {
                    paper,
                    new Border
                    {
                        WidthRequest = 136,
                        HeightRequest = 4,
                        BackgroundColor = Color.FromArgb("#00BFAF"),
                        Stroke = Colors.Transparent,
                        StrokeShape = new RoundRectangle { CornerRadius = 2 },
                        VerticalOptions = LayoutOptions.Center,
                        TranslationY = 28,
                        Opacity = 0.9
                    }
                }
            }
        };
    }

    private void SetStatus(string key, params object[] arguments)
    {
        _statusMessageKey = key;
        _status.Text = arguments.Length == 0
            ? T(key)
            : string.Format(CultureInfo.GetCultureInfo(_language), T(key), arguments);
    }

    private async Task ScanAsync()
    {
        try
        {
            SetStatus("opening_scanner");
            var result = await _scan.ExecuteAsync();
            if (result is null)
                SetStatus("no_scan");
            else
                SetStatus("pdf_saved", result.FileName);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            SetStatus("error", ex.Message);
        }
    }

    private async Task OpenPdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null)
            {
                SetStatus("pdf_unavailable");
                return;
            }

            var tempPath = System.IO.Path.Combine(FileSystem.CacheDirectory, $"{document.Id:N}.pdf");
            await using (var target = File.Create(tempPath))
                await source.CopyToAsync(target);

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                Title = document.FileName,
                File = new ReadOnlyFile(tempPath, "application/pdf")
            });
        }
        catch (Exception ex)
        {
            SetStatus("open_error", ex.Message);
        }
    }

    private async Task RefreshAsync()
    {
        _items.Children.Clear();
        var documents = await _storage.ListAsync();
        _emptyState.IsVisible = documents.Count == 0;

        foreach (var doc in documents)
        {
            var openButton = new Button
            {
                Text = T("open_pdf"),
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#E7EFEA"),
                TextColor = Color.FromArgb("#174A3A"),
                CornerRadius = 10
            };
            openButton.Clicked += async (_, _) => await OpenPdfAsync(doc);

            var shareButton = new Button
            {
                Text = T("share_pdf"),
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#174A3A"),
                TextColor = Colors.White,
                CornerRadius = 10
            };
            shareButton.Clicked += async (_, _) => await SharePdfAsync(doc);

            var deleteButton = new Button
            {
                Text = T("delete_pdf"),
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#FCE8E6"),
                TextColor = Color.FromArgb("#B3261E"),
                CornerRadius = 10
            };
            deleteButton.Clicked += async (_, _) => await DeletePdfAsync(doc);

            var dateCulture = CultureInfo.GetCultureInfo(_language);
            var sizeUnit = _language == "fr" ? "Ko" : "KB";
            _items.Children.Add(new Border
            {
                Padding = 14,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#DEE5E0"),
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label
                        {
                            Text = doc.FileName,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#17372E")
                        },
                        new Label
                        {
                            Text = $"{doc.CreatedAt.LocalDateTime.ToString("g", dateCulture)} · {doc.SizeBytes / 1024} {sizeUnit}",
                            FontSize = 12,
                            TextColor = Colors.Gray
                        },
                        openButton,
                        shareButton,
                        deleteButton
                    }
                }
            });
        }
    }

    private async Task DeletePdfAsync(ZRScanner.Domain.Document document)
    {
        var confirmed = await DisplayAlert(
            T("delete_title"),
            string.Format(CultureInfo.GetCultureInfo(_language), T("delete_confirm"), document.FileName),
            T("delete_pdf"),
            T("cancel"));
        if (!confirmed) return;

        try
        {
            if (await _storage.DeleteAsync(document))
                SetStatus("deleted", document.FileName);
            else
                SetStatus("already_deleted");

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            SetStatus("delete_error", ex.Message);
        }
    }

    private async Task SharePdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null)
            {
                SetStatus("pdf_unavailable");
                return;
            }

            var sharePath = System.IO.Path.Combine(FileSystem.CacheDirectory, document.FileName);
            await using (var target = File.Create(sharePath))
                await source.CopyToAsync(target);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = string.Format(T("share_title"), document.FileName),
                File = new ShareFile(sharePath, "application/pdf")
            });
        }
        catch (Exception ex)
        {
            SetStatus("share_error", ex.Message);
        }
    }
}
