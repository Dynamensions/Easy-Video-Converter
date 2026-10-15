using System.Diagnostics.CodeAnalysis;

[assembly: UnconditionalSuppressMessage(
    "Trimming",
    "IL2026",
    Scope = "module",
    Justification = "System.Management reflection is preserved.")]

namespace Dynamensions.EasyVideoConverter;

public class GlobalSuppressions
{
}
