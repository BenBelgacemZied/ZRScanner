using Plugin.Maui.DocumentScanner;
using ZRScanner.Application.UseCases;
using ZRScanner.Infrastructure.Pdf;
using ZRScanner.Infrastructure.Storage;

namespace ZRScanner.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseDocumentScanner();
        builder.Services.AddSingleton<ZRScanner.Application.Ports.IDocumentScanner, Platforms.Android.AndroidDocumentScanner>();
        builder.Services.AddSingleton<ZRScanner.Application.Ports.IPdfDocumentWriter, PdfDocumentWriter>();
        builder.Services.AddSingleton<ZRScanner.Application.Ports.IDocumentStorage>(_ => new LocalDocumentStorage(FileSystem.AppDataDirectory));
        builder.Services.AddTransient<ScanAndSaveDocument>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}