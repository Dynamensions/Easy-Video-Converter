using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Processes;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Dynamensions.EasyVideoConverter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;
using System;
using System.IO;


namespace Dynamensions.EasyVideoConverter;

public partial class App : Application
{
    public IServiceProvider Services { get; }
    public Window Window { get; private set; } = default!;

    public new static App Current => (App)Application.Current;

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

        Services = ConfigureServices();

    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddDebug();
            config.AddSerilog();
        });

        // Services
        services.AddSingleton<IClientProfileService, ClientProfileService>();
        //services.AddSingleton<IDeviceProfileService, DeviceProfileService>();
        services.AddSingleton<IEncoderProfileService, EncoderProfileService>();
        services.AddSingleton<IBootstrapService, BootstrapService>();
        services.AddSingleton<IMediaFileAnalyzerService, MediaFileAnalyzerService>();
        services.AddSingleton<IPickerService, WinUIPickerService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISubtitleExtractorService, SubtitleExtractorService>();
        services.AddSingleton<ISystemService, SystemService>();
        services.AddSingleton<ITranscodeQueueService, TranscodeQueueService>();
        services.AddSingleton<ITranscodeService, TranscodeService>();

        // processes
        services.AddSingleton<SmartDecisionProcess>();
        services.AddSingleton<EncodeTimeEstimatorProcess>();


        // helpers
        services.AddSingleton<EncodeTimeEstimatorProcess>();
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


        return services.BuildServiceProvider();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // load the profiles.
        var clientProfileService = App.Current.Services.GetRequiredService<IClientProfileService>();
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

        App.Current.Window = new MainWindow();
        App.Current.Window.Activate();
    }


}
