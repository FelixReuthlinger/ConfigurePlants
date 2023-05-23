using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace ConfigurePlants
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ConfigurePlants : BaseUnityPlugin
    {
        private const string PluginAuthor = "FixItFelix";
        private const string PluginName = "ConfigurePlants";
        private const string PluginVersion = "1.0.0";
        public const string PluginGuid = PluginAuthor + "." + PluginName;

        private void Awake()
        {
            Jotunn.Logger.LogInfo("ConfigurePlants has landed");
        }
    }
}

