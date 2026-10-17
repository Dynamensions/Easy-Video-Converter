using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dynamensions.EasyVideoConverter.Core.Messages;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels
{
    public partial class BootstrapViewModel(IFfmpegBootstrapService ffmpegBootstrapService) : ObservableObject
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        [ObservableProperty]
        private PrepareProgress? prepareProgress;


        [ObservableProperty]
        private string? statusMessage;

        [ObservableProperty]
        private bool isPercentVisible;

        [ObservableProperty]
        private double percent;

        [ObservableProperty]
        private string? percentMessage;

        [ObservableProperty]
        private bool isConfirmingPrepare = true;

        [RelayCommand]
        private async Task PrepareApplicationAsync()
        {
            IsConfirmingPrepare = false;

            var p = new Progress<PrepareProgress>();
            p.ProgressChanged += (s, e) =>
            {
                StatusMessage = e.StatusMessage;
                IsPercentVisible = e.ShowPercent;
                Percent = e.Percent;
                PercentMessage = e.PercentMessage;
            };

            await ffmpegBootstrapService.PrepareDependenciesAsync(p, cancellationTokenSource.Token);

            // file a message when the initialize completes.
            WeakReferenceMessenger.Default.Send(new ApplicationPrepareCompleteMessage(ffmpegBootstrapService.IsInitialized));
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await cancellationTokenSource.CancelAsync();
            await ffmpegBootstrapService.ExitAppAsync();
        }



    }
}
