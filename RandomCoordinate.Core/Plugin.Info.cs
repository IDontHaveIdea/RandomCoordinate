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
[assembly: AssemblyTitle(Constants.Prefix + "_" + RandomCoordinatePlugIn.PluginName + " (" + RandomCoordinatePlugIn.GUID + ")")]
[assembly: AssemblyProduct(Constants.Prefix + "_" + RandomCoordinatePlugIn.PluginName)]
[assembly: AssemblyVersion(RandomCoordinatePlugIn.Version)]
[assembly: AssemblyFileVersion(RandomCoordinatePlugIn.Version)]

#endregion Assembly attributes

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugIn
    {
        public const string GUID = "com.ihavenoidea.randomcoordinate";
#if DEBUG
        public const string PluginDisplayName = "Random Coordinates Plug-in (Debug)";
#else
        public const string PluginDisplayName = "Random Coordinates Plug-in";
#endif
        public const string PluginName = "RandomCoordinate";
        public const string Version = "1.1.1.0";
    }
}
