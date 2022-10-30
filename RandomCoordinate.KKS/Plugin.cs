//
// KKS entry point
//
using BepInEx;

using KKAPI;

namespace IDHIPlugins
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    public partial class RandomCoordinatePlugin : BaseUnityPlugin
    {
    }
}
