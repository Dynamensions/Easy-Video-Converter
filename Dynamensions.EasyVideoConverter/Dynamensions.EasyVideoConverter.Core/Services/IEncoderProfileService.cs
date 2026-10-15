using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IEncoderProfileService
{
    /// <summary>
    /// GET ALL SERVER PROFILES
    /// </summary>
    /// <returns>A <see cref="IList{EncoderProfile}"/> containing all the internal and custom profiles.</returns>
    List<EncoderProfile> GetAll();

    /// <summary>
    /// Gets the avaialbe profiles accoring to encoder system capabilities.
    /// </summary>
    /// <param name="encoderCapabilities">The <see cref="EncoderCapabilities"/> object containing all the capabilities in the encoder's system.</param>
    /// <returns></returns>
    IEnumerable<EncoderProfile> GetAvailableProfiles(EncoderCapabilities encoderCapabilities);
}
