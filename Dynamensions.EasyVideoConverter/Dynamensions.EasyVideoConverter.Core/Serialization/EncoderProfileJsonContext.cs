using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Serialization;

[JsonSerializable(typeof(EncoderProfile))]
[JsonSerializable(typeof(List<EncoderProfile>))]

//
// Nested types used inside EncoderProfile
//
[JsonSerializable(typeof(Resolution))]
[JsonSerializable(typeof(List<Resolution>))]

//
// Client profiles also serialize Resolution[] and codec lists
//
[JsonSerializable(typeof(ClientProfile))]
[JsonSerializable(typeof(List<ClientProfile>))]

[JsonSerializable(typeof(FfprobeRoot))]

[JsonSerializable(typeof(AppSettings))]

//
// Primitive collections used throughout your model graph
//
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(string[]))]

public partial class EncoderProfileJsonContext : JsonSerializerContext
{
}


