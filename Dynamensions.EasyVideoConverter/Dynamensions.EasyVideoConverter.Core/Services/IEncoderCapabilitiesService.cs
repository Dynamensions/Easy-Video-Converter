using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public sealed partial class EncoderCapabilities : ObservableObject
{
    [ObservableProperty] private bool hasCpu = true;
    [ObservableProperty] private bool hasNvidia;
    [ObservableProperty] private bool hasIntelQsv;
    [ObservableProperty] private bool hasAmdAmf;
}

public interface IEncoderCapabilitiesService
{
    public EncoderCapabilities EncoderCapabilities { get; }
    public Task<EncoderCapabilities> GetEncoderCapabilitiesAsync(IProgress<(double Percent, string Status)> progress, CancellationToken cancellationToken);

}
