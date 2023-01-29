//
// KKS entry point
//
using BepInEx;

using KKAPI;


namespace IDHIPlugins
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(IDHIUtils.Utilities.GUID, IDHIUtils.Utilities.Version)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    public partial class RandomCoordinatePlugin : BaseUnityPlugin
    {
    }
}
