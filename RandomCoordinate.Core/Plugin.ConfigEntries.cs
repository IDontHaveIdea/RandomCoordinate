//
// Configuration entries RandomCoordinate
//
using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI.Utilities;

namespace IDHIPlugins
{
    public partial class RandomCoordinatePlugin
    {
        internal static ConfigEntry<bool> DebugInfo { get; set; }
        internal static ConfigEntry<bool> OnlyChangingRoom { get; set; }

        internal void ConfigEntries()
        {
            var sectionDescription = "Debug";

            DebugInfo = Config.Bind(
                section: sectionDescription,
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes
                        { Order = 40, IsAdvanced = true }));
            DebugInfo.SettingChanged += (_sender, _args) =>
            {
                _Log.Enabled = DebugInfo.Value;
#if DEBUG
                _Log.Level(LogLevel.Info, $"[Configuration] Log.Enabled set " +
                    $"to {_Log.Enabled}");
#endif
            };

            sectionDescription = "Options";

            OnlyChangingRoom = Config.Bind(
                section: sectionDescription,
                key: "Random Coordinates Change Room Only",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Random coordinate selection will be " +
                        "effective only when the character is in a " +
                        "changing room.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 30 }));
        }
    }
}
