using CommunityToolkit.Mvvm.Messaging;
using Dynamensions.EasyVideoConverter.Core.Messages;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using WinRT.Interop;

namespace Dynamensions.EasyVideoConverter;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 1. Get the Window Handle (HWND)
        IntPtr hWnd = WindowNative.GetWindowHandle(this);

        // 2. Get the WindowId from the HWND
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

        // 3. Get the AppWindow instance
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            // 4. Combine the base directory with the local path to your .ico file
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");

            // 5. Set the window title bar icon
            appWindow.SetIcon(iconPath);
        }

        // subscribe to the message bus
        WeakReferenceMessenger.Default.Register<ApplicationPrepareCompleteMessage>(this, (reciever, message) =>
        {
            if (message.isSuccessful)
            {
                NavView.IsPaneVisible = true;
                ContentFrame.Navigate(App.AppHost.Services.GetRequiredService<QueuePage>().GetType());
            }
        });


        NavView.SelectionChanged += OnNavigationChanged;


        var ffmpegService = App.AppHost.Services.GetRequiredService<IFfmpegBootstrapService>();
        var isValidated = ffmpegService.Verify();

        if (isValidated)
        {
            ContentFrame.Navigate(App.AppHost.Services.GetRequiredService<QueuePage>().GetType());
        }
        else
        {
            NavView.IsPaneVisible = false;
            ContentFrame.Navigate(App.AppHost.Services.GetRequiredService<BootstrapPage>().GetType());
        }
    }

    private void OnNavigationChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var services = App.AppHost.Services;

            switch (item.Tag)
            {
                //case "MainPage":
                //    ContentFrame.Navigate(services.GetRequiredService<MainPage>().GetType());
                //    break;

                case "QueuePage":
                    ContentFrame.Navigate(services.GetRequiredService<QueuePage>().GetType());
                    break;

                case "SettingsPage":
                    ContentFrame.Navigate(services.GetRequiredService<SettingsPage>().GetType());
                    break;

                case "Profiles":
                    ContentFrame.Navigate(services.GetRequiredService<ProfileEditorPage>().GetType());
                    break;
            }
        }
    }
}
