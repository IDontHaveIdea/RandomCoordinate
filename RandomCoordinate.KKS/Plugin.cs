//
// KKS entry point
//
using BepInEx;

using KKAPI;

//[BepInDependency(IDHIUtils.IDHIUtilsInfo.GUID, IDHIUtils.IDHIUtilsInfo.Version)]


namespace IDHIPlugins
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(IDHIUtils.IDHIUtilsInfo.GUID, "1.0.6.0")]
    [BepInDependency("com.deathweasel.bepinex.moreoutfits",
        BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    public partial class RandomCoordinatePlugin : BaseUnityPlugin
    {
    }
}
