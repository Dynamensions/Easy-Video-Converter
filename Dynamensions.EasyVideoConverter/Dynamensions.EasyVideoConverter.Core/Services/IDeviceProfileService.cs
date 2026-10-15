using Dynamensions.EasyVideoConverter.Core.Models.Profiles;

namespace Dynamensions.EasyVideoConverter.Core.Services
{
    public interface IDeviceProfileService
    {
        List<EncoderProfile> LoadCustomProfiles();
        void SaveCustomProfiles(List<EncoderProfile> profiles);
        List<EncoderProfile> GetAll();
    }
}
