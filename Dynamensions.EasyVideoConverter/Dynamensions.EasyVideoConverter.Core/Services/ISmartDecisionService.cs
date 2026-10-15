using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISmartDecisionService
{
    SmartDecision Decide(EncoderProfile encoder, ClientProfile client, MediaInfo info, SmartDecision? d = null);
}
