using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dynamensions.EasyVideoConverter.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BootstrapPage : Page
{
    // TODO: convert this navigation to Inversion Of Control
    private readonly BootstrapViewModel viewModel;

    public BootstrapPage()
    {
        InitializeComponent();

        DataContext = viewModel = App.AppHost.Services.GetRequiredService<BootstrapViewModel>();
    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await viewModel.PrepareApplicationAsync(CancellationToken.None);
        ((App)App.Current).InvokeBootstrapComplete(true);
    }
}
