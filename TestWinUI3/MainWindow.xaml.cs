using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TestWinUI3;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.Content = "Clicked!";
    }
}
