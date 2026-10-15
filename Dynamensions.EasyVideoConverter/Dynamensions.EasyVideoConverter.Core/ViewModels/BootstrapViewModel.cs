using CommunityToolkit.Mvvm.ComponentModel;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels
{
    public partial class BootstrapViewModel(IFfmpegBootstrapService ffmpegBootstrapService) : ObservableObject
    {
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



        public async Task PrepareApplicationAsync(CancellationToken cancellationToken)
        {
            var p = new Progress<PrepareProgress>();
            p.ProgressChanged += (s, e) =>
            {
                StatusMessage = e.StatusMessage;
                IsPercentVisible = e.ShowPercent;
                Percent = e.Percent;
                PercentMessage = e.PercentMessage;
            };


            await ffmpegBootstrapService.InitializeAsync(p, cancellationToken);
        }



    }
}
