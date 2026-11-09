using CommunityToolkit.Mvvm.Messaging;
using Dynamensions.EasyVideoConverter.Core.Messages;
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
        App.LogEntry("MainWindow ctor started");

        InitializeComponent();
        App.LogEntry("InitializeComponent complete");

        // 1. Get the Window Handle (HWND)
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        App.LogEntry("MainWindow handle created");

        // 2. Get the WindowId from the HWND
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        App.LogEntry("MainWindow id created");

        // 3. Get the AppWindow instance
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            // 4. Combine the base directory with the local path to your .ico file
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");

            // 5. Set the window title bar icon
            appWindow.SetIcon(iconPath);
            App.LogEntry("MainWindow icon set");
        }

        // subscribe to the message bus
        WeakReferenceMessenger.Default.Register<ApplicationPrepareCompleteMessage>(this, (reciever, message) =>
        {
            App.LogEntry($"MainWindow ApplicationPreparedMessage received: {message.isSuccessful}");

            if (message.isSuccessful)
            {
                NavView.IsPaneVisible = true;
                ContentFrame.Navigate(App.Current.Services.GetRequiredService<QueuePage>().GetType());
                App.LogEntry("Navigation to Queue completed");
            }
        });

        NavView.SelectionChanged += OnNavigationChanged;
        NavView.IsPaneVisible = false;
        ContentFrame.Navigate(App.Current.Services.GetRequiredService<BootstrapPage>().GetType());
        App.LogEntry("Navigation to Bootstrap complete");


    }

    private void OnNavigationChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var services = App.Current.Services;

            switch (item.Tag)
            {
                //case "MainPage":
                //    ContentFrame.Navigate(services.GetRequiredService<MainPage>().GetType());
                //    break;

                case "QueuePage":
                    ContentFrame.Navigate(services.GetRequiredService<QueuePage>().GetType());
                    App.LogEntry("Click navigation to Queue complete");
                    break;

                case "SettingsPage":
                    ContentFrame.Navigate(services.GetRequiredService<SettingsPage>().GetType());
                    App.LogEntry("Click navigation to Settings complete");
                    break;

                case "Profiles":
                    ContentFrame.Navigate(services.GetRequiredService<ProfileEditorPage>().GetType());
                    App.LogEntry("Click navigation to Profiles complete");
                    break;

                case "LogsPage":
                    ContentFrame.Navigate(services.GetRequiredService<LogView>().GetType());
                    App.LogEntry("Click navigation to Logs complete");
                    break;
            }
        }
    }
}
