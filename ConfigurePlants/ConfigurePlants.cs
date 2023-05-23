using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace ConfigurePlants
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ConfigurePlantsPlugin : BaseUnityPlugin
    {
        private const string PluginAuthor = "FixItFelix";
        private const string PluginName = "ConfigurePlants";
        private const string PluginVersion = "1.0.0";
        public const string PluginGuid = PluginAuthor + "." + PluginName;

        private void Awake()
        {
            CommandManager.Instance.AddConsoleCommand(new PlantsPrintController());
        }
    }

    public class PlantsPrintController : ConsoleCommand
    {
        public override void Run(string[] args)
        {
            Jotunn.Logger.LogInfo($"PlantsPrintController called");
            PlantsManager.WritePlants();
        }

        public override string Name => "configure_plants_print_defaults";

        public override string Help =>
            "Write all prefabs loaded in-game into a YAML translations template file inside the BepInEx config.";
    }
}