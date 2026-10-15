namespace Dynamensions.EasyVideoConverter.Core.Models.Profiles;

public class ClientProfileGroup
{
    public required string Key { get; set; }
    public List<ClientProfile> Profiles { get; set; } = [];
}
