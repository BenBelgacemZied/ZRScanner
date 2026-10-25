namespace ZRScanner.Mobile;
public partial class App : global::Microsoft.Maui.Controls.Application
{
    public App(StartupPage startupPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(startupPage);
    }
}