using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Dynamensions.EasyVideoConverter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.IO;


namespace Dynamensions.EasyVideoConverter;

public partial class App : Application
{
    public event Action<bool>? BootstrapComplete;
    public static IHost AppHost { get; private set; } = default!;
    public static Window MainWindow { get; private set; } = default!;

    public App()
    {
        InitializeComponent();



        var appFolder = AppContext.BaseDirectory;

        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        var logFolder = Path.Combine(localFolder.Path,
            "Dynamensions.EasyVideoConverter", "logs");

        var logFolderCreated = Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logFolder, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();


        AppHost = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();

                logging.AddDebug();     // <-- This adds Microsoft.Extensions.Logging.Debug
                logging.AddSerilog();   // <-- This keeps your Serilog file logging
            })
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IClientProfileService, ClientProfileService>();
                //services.AddSingleton<IDeviceProfileService, DeviceProfileService>();
                services.AddSingleton<IEncoderProfileService, EncoderProfileService>();
                services.AddSingleton<IFfmpegBootstrapService, FfmpegBootstrapService>();
                services.AddSingleton<IMediaFileAnalyzerService, MediaFileAnalyzerService>();
                services.AddSingleton<IPerformanceBucketService, PerformanceBucketService>();
                services.AddSingleton<IPickerService, WinUIPickerService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ISmartDecisionService, SmartDecisionService>();
                services.AddSingleton<ISubtitleExtractorService, SubtitleExtractorService>();
                services.AddSingleton<ISystemService, SystemService>();
                services.AddSingleton<ITranscodeQueueService, TranscodeQueueService>();
                services.AddSingleton<ITranscodeService, TranscodeService>();


                // helpers
                services.AddSingleton<EncodeTimeEstimator>();
                services.AddSingleton<SmartDecisionComplianceValidator>();

                // ViewModels
                services.AddTransient<BootstrapViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<ProfileEditorViewModel>();
                services.AddTransient<QueueViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddTransient<Views.BootstrapPage>();
                services.AddTransient<Views.MainPage>();
                services.AddTransient<Views.QueuePage>();
                services.AddTransient<Views.SettingsPage>();
                services.AddTransient<Views.ProfileEditorPage>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // load the profiles.
        var clientProfileService = App.AppHost.Services.GetRequiredService<IClientProfileService>();
        clientProfileService.LoadInternalProfiles();


        // Set the FFMPEG path
        //var baseLocal = ApplicationData.Current.LocalFolder.Path;
        //settingsService.FfMpegDirectory = Path.Combine(
        //    baseLocal,
        //    "Dynamensions",
        //    "EasyVideoConverter",
        //    "ffmpeg"
        //);

        base.OnLaunched(args);

        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

    public void InvokeBootstrapComplete(bool isSuccessful)
    {
        BootstrapComplete?.Invoke(isSuccessful);
    }

}
