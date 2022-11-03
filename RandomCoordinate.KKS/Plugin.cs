//
// KKS entry point
//
using BepInEx;

using KKAPI;


namespace IDHIPlugins
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(IDHIUtils.Info.GUID, IDHIUtils.Info.Version)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    public partial class RandomCoordinatePlugin : BaseUnityPlugin
    {
    }
}
