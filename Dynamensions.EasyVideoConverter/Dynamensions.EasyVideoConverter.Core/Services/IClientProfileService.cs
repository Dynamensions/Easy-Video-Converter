using Dynamensions.EasyVideoConverter.Core.Models.Profiles;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IClientProfileService
{
    public Dictionary<string, ClientProfile> Profiles { get; }


    /// <summary>
    /// Tasks the custom profiles to an external directory.
    /// </summary>
    /// <param name="profiles">The profiles to save.</param>
    public Task SaveCustomProfilesAsync(List<ClientProfile> profiles);

    /// <summary>
    /// Load the profiles into the <see cref="Profiles" /> object.
    /// </summary>
    public void LoadInternalProfiles();
}
