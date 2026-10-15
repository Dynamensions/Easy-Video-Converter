using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Dynamensions.EasyVideoConverter.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = App.AppHost.Services.GetRequiredService<MainViewModel>();
    }
}
