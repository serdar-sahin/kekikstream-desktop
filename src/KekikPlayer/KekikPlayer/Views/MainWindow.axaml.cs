using FluentAvalonia.UI.Windowing;

namespace KekikPlayer.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        this.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen;
    }
}
