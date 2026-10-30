// IsExternalInit ships in .NET 5+ but not .NET Framework 4.8; declaring it lets `init` and
// `record` compile against net48.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
