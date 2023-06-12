using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Logger = Jotunn.Logger;
using Paths = BepInEx.Paths;

namespace ConfigurePlants
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ConfigurePlantsPlugin : BaseUnityPlugin
    {
        private const string PluginAuthor = "FixItFelix";
        private const string PluginName = "ConfigurePlants";
        private const string PluginVersion = "1.1.1";
        public const string PluginGuid = PluginAuthor + "." + PluginName;

        private static readonly ConfigSync ConfigSync = new(PluginGuid) { CurrentVersion = PluginVersion };

        private static readonly CustomSyncedValue<Dictionary<string, string>> PlantConfigFilesContent =
            new(ConfigSync, "PlantConfigFilesContent", new Dictionary<string, string>());

        private void Awake()
        {
            PlantConfigFilesContent.Value = PlantConfigFileAccess.ReadAllFiles();
            PrefabManager.OnPrefabsRegistered += ReplacePlants;
            CommandManager.Instance.AddConsoleCommand(new PlantsPrintController());
        }

        private static void ReplacePlants()
        {
            PlantsManager.RegisterPlantsFromCustomConfig(PlantConfigFilesContent.Value);
        }
    }

    public class PlantsPrintController : ConsoleCommand
    {
        public override void Run(string[] args)
        {
            PlantConfigFileAccess
                .WriteFile(PlantConfigFileAccess.DefaultFile, new SerializerBuilder()
                    .DisableAliases()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build()
                    .Serialize(PrefabManager.Cache.GetPrefabs(typeof(Plant))
                        .ToDictionary(pair => pair.Key, pair => PlantModel.FromPlant((Plant)pair.Value))));
        }

        public override string Name => "configure_plants_print_defaults";

        public override string Help =>
            "Write all prefabs loaded in-game into a YAML translations template file inside the BepInEx config.";
    }

    public static class PlantConfigFileAccess
    {
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ConfigurePlantsPlugin.PluginGuid}.defaults.yaml";
        private static readonly string ConfigNamePatterns = $"{ConfigurePlantsPlugin.PluginGuid}.custom.*.yaml";
        public static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        private static string ReadFile(string fileNameAndPath)
        {
            Logger.LogInfo($"reading from file '{fileNameAndPath}'");
            return File.ReadAllText(fileNameAndPath);
        }

        public static void WriteFile(string fileNameAndPath, string fileContent)
        {
            File.WriteAllText(fileNameAndPath, fileContent);
            Logger.LogInfo($"file '{fileNameAndPath}' successfully written");
        }

        private static List<string> SearchConfigFiles()
        {
            return Directory.GetFiles(Paths.ConfigPath, ConfigNamePatterns, SearchOption.AllDirectories).ToList();
        }

        public static Dictionary<string, string> ReadAllFiles()
        {
            return SearchConfigFiles().ToDictionary(fileName => fileName, ReadFile);
        }
    }

    public class PlantModel
    {
        [UsedImplicitly] public string Name;
        [UsedImplicitly] public float GrowTime;
        [UsedImplicitly] public float GrowRadius;
        [UsedImplicitly] public float MinScale;
        [UsedImplicitly] public float MaxScale;
        [UsedImplicitly] public bool NeedCultivatedGround;
        [UsedImplicitly] public bool DestroyIfCantGrow;

        public static PlantModel FromPlant(Plant fromPlant)
        {
            return new PlantModel
            {
                Name = fromPlant.m_name,
                GrowTime = fromPlant.m_growTime,
                GrowRadius = fromPlant.m_growRadius,
                MinScale = fromPlant.m_minScale,
                MaxScale = fromPlant.m_maxScale,
                NeedCultivatedGround = fromPlant.m_needCultivatedGround,
                DestroyIfCantGrow = fromPlant.m_destroyIfCantGrow
            };
        }

        public void ReconfigurePlant(Plant plant)
        {
            plant.m_name = Name;
            plant.m_growRadius = GrowRadius;
            plant.m_growTime = GrowTime;
            plant.m_minScale = MinScale;
            plant.m_maxScale = MaxScale;
            plant.m_needCultivatedGround = NeedCultivatedGround;
            plant.m_destroyIfCantGrow = DestroyIfCantGrow;
        }
    }

    public static class PlantsManager
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        public static void RegisterPlantsFromCustomConfig(Dictionary<string, string> fileContents)
        {
            Dictionary<string, PlantModel> plantModels = fileContents.Select(pair =>
                    {
                        try
                        {
                            return Deserializer.Deserialize<Dictionary<string, PlantModel>>(pair.Value);
                        }
                        catch (Exception e)
                        {
                            Logger.LogWarning(
                                $"Unable to parse config file '{pair.Key}' due to '{e.Message}' " +
                                $"because of '{e.GetBaseException().Message}', \n{e.StackTrace}");
                        }

                        return new Dictionary<string, PlantModel>();
                    }
                ).SelectMany(i => i)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (KeyValuePair<string, PlantModel> keyValuePair in plantModels)
            {
                GameObject plantObject = PrefabManager.Instance.GetPrefab(keyValuePair.Key);

                if (!plantObject.TryGetComponent(out Plant plant))
                {
                    Logger.LogError(
                        $"chosen original prefab '{keyValuePair.Key}' doesn't have a 'Plant' " +
                        $"component, cannot reconfigure it");
                    return;
                }

                keyValuePair.Value.ReconfigurePlant(plant);
                Logger.LogInfo($"reconfigured {keyValuePair.Key} successfully");
            }
        }
    }
}