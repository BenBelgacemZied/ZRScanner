namespace ZRScanner.Mobile;

public partial class App : global::Microsoft.Maui.Controls.Application
{
    private readonly StartupPage _startupPage;

    public App(StartupPage startupPage)
    {
        InitializeComponent();
        _startupPage = startupPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_startupPage));
    }
}