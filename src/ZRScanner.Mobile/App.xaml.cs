namespace ZRScanner.Mobile;
public partial class App : global::Microsoft.Maui.Controls.Application
{
    public App(MainPage page) { InitializeComponent(); MainPage = new NavigationPage(page); }
}