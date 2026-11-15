using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Dynamensions.EasyVideoConverter.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AboutPage : Page
{
    private readonly AboutViewModel viewModel;

    public AboutPage()
    {
        InitializeComponent();
        viewModel = App.Current.Services.GetRequiredService<AboutViewModel>();

    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await viewModel.PrepareViewModelAsync(default);
    }
}
