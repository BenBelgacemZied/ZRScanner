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
    private readonly Label _status = new() { Text = "Les documents restent dans l’espace privé de l’application.", TextColor = Colors.Gray };

    public MainPage(ScanAndSaveDocument scan, IDocumentStorage storage)
    {
        _scan = scan; _storage = storage;
        Title = "ZRScanner";
        BackgroundColor = Color.FromArgb("#F5F7F5");
        var button = new Button { Text = "Scanner un document", BackgroundColor = Color.FromArgb("#174A3A"), TextColor = Colors.White, CornerRadius = 14, HeightRequest = 54 };
        button.Clicked += async (_, _) => await ScanAsync();
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 24, Spacing = 18, Children = {
            new Label { Text = "Documents comptables", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17372E") },
            new Label { Text = "Scannez vos justificatifs et conservez-les en PDF.", FontSize = 15, TextColor = Colors.DarkSlateGray }, button, _status,
            new Label { Text = "Mes documents", FontSize = 19, FontAttributes = FontAttributes.Bold }, _items
        }}};
        Loaded += async (_, _) => await RefreshAsync();
    }

    private async Task ScanAsync()
    {
        try { _status.Text = "Ouverture du scanner…"; var result = await _scan.ExecuteAsync(); _status.Text = result is null ? "Aucune page scannée." : $"PDF enregistré : {result.FileName}"; await RefreshAsync(); }
        catch (Exception ex) { _status.Text = $"Erreur : {ex.Message}"; }
    }

    private async Task OpenPdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null) { _status.Text = "Ce PDF n’est pas disponible dans le stockage local."; return; }
            var tempPath = System.IO.Path.Combine(FileSystem.CacheDirectory, $"{document.Id:N}.pdf");
            await using (var target = File.Create(tempPath)) await source.CopyToAsync(target);
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                Title = document.FileName,
                File = new ReadOnlyFile(tempPath, "application/pdf")
            });
        }
        catch (Exception ex) { _status.Text = $"Impossible d’ouvrir le PDF : {ex.Message}"; }
    }

    private async Task RefreshAsync()
    {
        _items.Children.Clear();
        foreach (var doc in await _storage.ListAsync())
        {
            var openButton = new Button { Text = "Ouvrir le PDF", HeightRequest = 44, BackgroundColor = Color.FromArgb("#E7EFEA"), TextColor = Color.FromArgb("#174A3A"), CornerRadius = 10 };
            openButton.Clicked += async (_, _) => await OpenPdfAsync(doc);
            var shareButton = new Button { Text = "Partager le PDF", HeightRequest = 44, BackgroundColor = Color.FromArgb("#174A3A"), TextColor = Colors.White, CornerRadius = 10 };
            shareButton.Clicked += async (_, _) => await SharePdfAsync(doc);
            _items.Children.Add(new Border
            {
                Padding = 14,
                Stroke = Color.FromArgb("#DEE5E0"),
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = new VerticalStackLayout { Spacing = 8, Children = {
                    new Label { Text = doc.FileName, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#17372E") },
                    new Label { Text = $"{doc.CreatedAt.LocalDateTime:g} · {doc.SizeBytes / 1024} Ko", FontSize = 12, TextColor = Colors.Gray },
                    openButton,
                    shareButton
                }}
            });
        }
    }

    private async Task SharePdfAsync(ZRScanner.Domain.Document document)
    {
        try
        {
            await using var source = await _storage.OpenReadAsync(document);
            if (source is null) { _status.Text = "Ce PDF n’est pas disponible dans le stockage local."; return; }
            var sharePath = System.IO.Path.Combine(FileSystem.CacheDirectory, document.FileName);
            await using (var target = File.Create(sharePath)) await source.CopyToAsync(target);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Partager {document.FileName}",
                File = new ShareFile(sharePath, "application/pdf")
            });
        }
        catch (Exception ex) { _status.Text = $"Impossible de partager le PDF : {ex.Message}"; }
    }
}
