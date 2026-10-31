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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SystemUnhandledExceptionEventArgs = System.UnhandledExceptionEventArgs;
using XamlUnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;


namespace Dynamensions.EasyVideoConverter;

public partial class App : Application
{
    public IServiceProvider Services { get; }
    public Window Window { get; private set; } = default!;

    public new static App Current => (App)Application.Current;

    public App()
    {
        InitializeComponent();

        // Exception Handling
        RegisterExceptionHandlers();

        // Set the base folder
        var appFolder = AppContext.BaseDirectory;
        // Set the local folder
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        // Set the log folder
        var logFolder = Path.Combine(localFolder.Path, "Dynamensions.EasyVideoConverter", "logs");
        // Create directory
        var logFolderCreated = Directory.CreateDirectory(logFolder);

        // Configure log
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logFolder, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true)
            .CreateLogger();

        // Configure services
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
        services.AddSingleton<IEncoderProfileService, EncoderProfileService>();
        services.AddSingleton<IEncoderCapabilitiesService, EncoderCapabilitiesService>();
        services.AddSingleton<IBootstrapService, BootstrapService>();
        services.AddSingleton<IMediaFileAnalyzerService, MediaFileAnalyzerService>();
        services.AddSingleton<IPickerService, WinUIPickerService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISubtitleExtractorService, SubtitleExtractorService>();
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
        services.AddTransient<ProfileEditorViewModel>();
        services.AddTransient<QueueViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<Views.BootstrapPage>();
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

        base.OnLaunched(args);

        App.Current.Window = new MainWindow();
        App.Current.Window.Activate();
    }

    private void RegisterExceptionHandlers()
    {
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        this.UnhandledException += App_UnhandledException;
    }


    private void App_UnhandledException(object sender, XamlUnhandledExceptionEventArgs e)
    {
        // Log only the important ones to keep from filling the log.
        if (e.Exception is NullReferenceException || e.Exception is FileNotFoundException || e.Exception is UnauthorizedAccessException)
            LogException($"FirstChance", e.Exception);

        // Consider false while diagnosing
        e.Handled = true;
    }

    private void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        LogException("FirstChanceException", e.Exception);
    }

    private void CurrentDomain_UnhandledException(object sender, SystemUnhandledExceptionEventArgs e)
    {
        LogException("AppDomain UnhandledException", e.ExceptionObject as Exception);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException("TaskScheduler UnobservedTaskException", e.Exception);
    }

    private static void LogException(string source, Exception? ex)
    {
        try
        {
            var text =
                $"{DateTime.Now}\r\n" +
                $"Source: {source}\r\n" +
                $"{ex}\r\n\r\n";

            var file = Path.Combine(ApplicationData.Current.LocalFolder.Path, "crash.log");

            File.AppendAllText(file, text);

            EventLog.WriteEntry("Application", text, EventLogEntryType.Error);
        }
        catch
        {
        }
    }

}
