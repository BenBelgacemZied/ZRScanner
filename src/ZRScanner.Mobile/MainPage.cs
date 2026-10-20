using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using ZRScanner.Application.Ports;
using ZRScanner.Application.UseCases;

namespace ZRScanner.Mobile;

public sealed class MainPage : ContentPage
{
    private readonly ScanAndSaveDocument _scan;
    private readonly IDocumentStorage _storage;
    private readonly VerticalStackLayout _items = new() { Spacing = 10 };
    private readonly Label _status = new()
    {
        Text = "Vos documents scannés apparaîtront ici.",
        TextColor = Color.FromArgb("#697A74"),
        FontSize = 14
    };
    private readonly Border _emptyState = new()
    {
        Padding = 20,
        BackgroundColor = Colors.White,
        Stroke = Color.FromArgb("#E0E8E3"),
        StrokeShape = new RoundRectangle { CornerRadius = 16 },
        Content = new Label
        {
            Text = "Aucun document pour le moment.\nScannez une feuille pour la retrouver ici.",
            TextColor = Color.FromArgb("#697A74"),
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        }
    };

    public MainPage(ScanAndSaveDocument scan, IDocumentStorage storage)
    {
        _scan = scan;
        _storage = storage;
        Title = "ZRScanner";
        BackgroundColor = Color.FromArgb("#F5F7F5");

        var scanButton = new Button
        {
            Text = "＋  Scanner un document",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#174A3A"),
            TextColor = Colors.White,
            CornerRadius = 16,
            HeightRequest = 58
        };
        scanButton.Clicked += async (_, _) => await ScanAsync();

        var heading = new Label
        {
            Text = "Bienvenue sur\nZRScanner",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#17372E"),
            LineHeight = 1.05
        };

        var illustration = CreateScannerIllustration();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(22, 24),
                Spacing = 16,
                Children =
                {
                    heading,
                    new Label
                    {
                        Text = "Numérisez vos documents en quelques secondes.",
                        FontSize = 16,
                        TextColor = Color.FromArgb("#52665E")
                    },
                    illustration,
                    new Label
                    {
                        Text = "Scannez votre premier document",
                        FontSize = 19,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#17372E")
                    },
                    new Label
                    {
                        Text = "Transformez vos feuilles en PDF et retrouvez-les ici.",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#52665E")
                    },
                    scanButton,
                    _status,
                    new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Star),
                            new ColumnDefinition(GridLength.Auto)
                        },
                        Children =
                        {
                            new Label
                            {
                                Text = "Mes documents",
                                FontSize = 20,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#17372E"),
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = "Enregistrés sur cet appareil",
                                FontSize = 12,
                                TextColor = Color.FromArgb("#697A74"),
                                HorizontalOptions = LayoutOptions.End,
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    },
                    _emptyState,
                    _items
                }
            }
        };

        Loaded += async (_, _) => await RefreshAsync();
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

    private async Task ScanAsync()
    {
        try
        {
            _status.Text = "Ouverture du scanner…";
            var result = await _scan.ExecuteAsync();
            _status.Text = result is null
                ? "Aucune page scannée."
                : $"PDF enregistré : {result.FileName}";
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _status.Text = $"Erreur : {ex.Message}";
        }
    }

    private async Task OpenPdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null)
            {
                _status.Text = "Ce PDF n’est pas disponible dans le stockage local.";
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
            _status.Text = $"Impossible d’ouvrir le PDF : {ex.Message}";
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
                Text = "Ouvrir le PDF",
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#E7EFEA"),
                TextColor = Color.FromArgb("#174A3A"),
                CornerRadius = 10
            };
            openButton.Clicked += async (_, _) => await OpenPdfAsync(doc);

            var shareButton = new Button
            {
                Text = "Partager le PDF",
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#174A3A"),
                TextColor = Colors.White,
                CornerRadius = 10
            };
            shareButton.Clicked += async (_, _) => await SharePdfAsync(doc);

            var deleteButton = new Button
            {
                Text = "Supprimer le PDF",
                HeightRequest = 44,
                BackgroundColor = Color.FromArgb("#FCE8E6"),
                TextColor = Color.FromArgb("#B3261E"),
                CornerRadius = 10
            };
            deleteButton.Clicked += async (_, _) => await DeletePdfAsync(doc);

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
                            Text = $"{doc.CreatedAt.LocalDateTime:g} · {doc.SizeBytes / 1024} Ko",
                            FontSize = 12,
                            TextColor = Colors.Gray
                        },
                        openButton,
                        shareButton
                        deleteButton
                    }
                }
            });
        }
    }

    private async Task DeletePdfAsync(ZRScanner.Domain.Document document)
    {
        var confirmed = await DisplayAlert(
            "Supprimer le PDF ?",
            $"« {document.FileName} » sera supprimé définitivement de cet appareil.",
            "Supprimer",
            "Annuler");
        if (!confirmed) return;

        try
        {
            if (await _storage.DeleteAsync(document))
                _status.Text = $"PDF supprimé : {document.FileName}";
            else
                _status.Text = "Ce PDF n’existe déjà plus dans la liste.";

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _status.Text = $"Impossible de supprimer le PDF : {ex.Message}";
        }
    }

    private async Task SharePdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null)
            {
                _status.Text = "Ce PDF n’est pas disponible dans le stockage local.";
                return;
            }

            var sharePath = System.IO.Path.Combine(FileSystem.CacheDirectory, document.FileName);
            await using (var target = File.Create(sharePath))
                await source.CopyToAsync(target);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Partager {document.FileName}",
                File = new ShareFile(sharePath, "application/pdf")
            });
        }
        catch (Exception ex)
        {
            _status.Text = $"Impossible de partager le PDF : {ex.Message}";
        }
    }
}