//
// Plug-in Meta-data
//
using System.Reflection;

using IDHIPlugins;
using IDHIUtils;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(Constants.Prefix + "_" + RandomCoordinatePlugin.PluginName + " (" + RandomCoordinatePlugin.GUID + ")")]
[assembly: AssemblyProduct(Constants.Prefix + "_" + RandomCoordinatePlugin.PluginName)]
[assembly: AssemblyVersion(RandomCoordinatePlugin.Version)]
[assembly: AssemblyFileVersion(RandomCoordinatePlugin.Version)]

#endregion Assembly attributes

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        public const string GUID = "com.ihavenoidea.randomcoordinate";
#if DEBUG
        public const string PluginDisplayName = "Random Coordinates Plug-in (Debug)";
#else
        public const string PluginDisplayName = "Random Coordinates Plug-in";
#endif
        public const string PluginName = "RandomCoordinate";
        public const string Version = "1.0.6.0";
    }
}
