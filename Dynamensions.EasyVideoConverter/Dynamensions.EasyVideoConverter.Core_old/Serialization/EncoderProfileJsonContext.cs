using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Serialization;

[JsonSerializable(typeof(EncoderProfile))]
[JsonSerializable(typeof(List<EncoderProfile>))]
[JsonSerializable(typeof(Resolution))]
[JsonSerializable(typeof(Resolution[]))]
[JsonSerializable(typeof(List<Resolution>))]

[JsonSerializable(typeof(AudioTrackSelectionMode))]
[JsonSerializable(typeof(SubtitleMode))]
[JsonSerializable(typeof(ProfileType))]

[JsonSerializable(typeof(ClientProfile))]
[JsonSerializable(typeof(List<ClientProfile>))]

[JsonSerializable(typeof(FfprobeRoot))]
[JsonSerializable(typeof(FfprobeStream))]
[JsonSerializable(typeof(FfprobeFormat))]
[JsonSerializable(typeof(FfprobeDisposition))]
[JsonSerializable(typeof(FfprobeTags))]

[JsonSerializable(typeof(AppSettings))]
public partial class EncoderProfileJsonContext : JsonSerializerContext
{
}
