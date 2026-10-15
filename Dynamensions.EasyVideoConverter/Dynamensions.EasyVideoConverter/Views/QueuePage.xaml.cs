using Dynamensions.EasyVideoConverter.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dynamensions.EasyVideoConverter.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QueuePage : Page
    {
        public QueuePage()
        {
            InitializeComponent();
            QueueViewModel viewModel;
            DataContext = viewModel = App.AppHost.Services.GetRequiredService<QueueViewModel>();

            DispatcherTimer timer = new() { Interval = System.TimeSpan.FromSeconds(1) };
            timer.Tick += (_, _) => viewModel.UpdateElapsedTime();
            timer.Start();

            viewModel.ProgressCallbackDispatcher = p =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    p.item.Progress = p.ProgressValue * 100;
                    p.item.EstimatedTimeRemaining = System.TimeSpan.FromSeconds(p.secondsRemaining);
                    p.item.CurrentFPS = p.CurrentFPS;
                    p.item.SpeedX = p.SpeedX;
                });
            };

            viewModel.SubtitleCallback = p =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    p.item.Status = $"Extracting external subtitles [{p.Current} of {p.Total}] - [{p.SubtitleTrack.Title}({p.SubtitleTrack.Codec})] ({p.Current}/{p.Total})";
                });
            };
        }
    }
}
