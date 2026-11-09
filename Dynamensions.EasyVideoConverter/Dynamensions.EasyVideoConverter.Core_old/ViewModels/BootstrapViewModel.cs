using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dynamensions.EasyVideoConverter.Core.Messages;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels
{
    public partial class BootstrapViewModel(IBootstrapService ffmpegBootstrapService, IEncoderCapabilitiesService encoderCapabilitiesService) : ObservableObject
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        [ObservableProperty]
        private bool isVerified;

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

            await PrepareDependenciesAsync();
            await GetEncoderCapabilitiesAsync();

            // file a message when the initialize completes.
            WeakReferenceMessenger.Default.Send(new ApplicationPrepareCompleteMessage(ffmpegBootstrapService.IsInitialized));
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await cancellationTokenSource.CancelAsync();
            await ffmpegBootstrapService.ExitAppAsync();
        }

        public async Task PrepareViewModelAsync()
        {
            // verify dependencies are in place.
            IsVerified = ffmpegBootstrapService.Verify();

            // if dependencies are in place, get the encoder's capabilities.
            if (IsVerified)
            {
                IsConfirmingPrepare = false;
                await GetEncoderCapabilitiesAsync();
                WeakReferenceMessenger.Default.Send(new ApplicationPrepareCompleteMessage(true));
            }
        }


        private async Task PrepareDependenciesAsync()
        {
            var bootStrapProgress = new Progress<PrepareProgress>();
            bootStrapProgress.ProgressChanged += (s, e) =>
            {
                StatusMessage = e.StatusMessage;
                IsPercentVisible = e.ShowPercent;
                Percent = e.Percent;
                PercentMessage = e.PercentMessage;
            };
            await ffmpegBootstrapService.PrepareDependenciesAsync(bootStrapProgress, cancellationTokenSource.Token);
        }

        private async Task GetEncoderCapabilitiesAsync()
        {
            var encoderDetectionProgress = new Progress<(double Percent, string Status)>();
            encoderDetectionProgress.ProgressChanged += (s, e) =>
            {
                StatusMessage = e.Status;
                Percent = e.Percent;
            };

            IsPercentVisible = true;
            await encoderCapabilitiesService.GetEncoderCapabilitiesAsync(encoderDetectionProgress, cancellationTokenSource.Token);
            IsPercentVisible = false;
        }

    }
}
