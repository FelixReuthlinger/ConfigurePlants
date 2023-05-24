using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn.Managers;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Logger = Jotunn.Logger;

namespace ConfigurePlants
{
    public class PlantModel
    {
        public PlantModel(Plant fromPlant)
        {
            Name = fromPlant.m_name;
            GrowTime = fromPlant.m_growTime;
            GrowRadius = fromPlant.m_growRadius;
            MinScale = fromPlant.m_minScale;
            MaxScale = fromPlant.m_maxScale;
            NeedCultivatedGround = fromPlant.m_needCultivatedGround;
            DestroyIfCantGrow = fromPlant.m_destroyIfCantGrow;
        }

        public PlantModel(string name, float growTime, float growRadius, float minScale, float maxScale,
            bool needCultivatedGround, bool destroyIfCantGrow)
        {
            Name = name;
            GrowTime = growTime;
            GrowRadius = growRadius;
            MinScale = minScale;
            MaxScale = maxScale;
            NeedCultivatedGround = needCultivatedGround;
            DestroyIfCantGrow = destroyIfCantGrow;
        }

        [UsedImplicitly] public readonly string Name;
        [UsedImplicitly] public readonly float GrowTime;
        [UsedImplicitly] public readonly float GrowRadius;
        [UsedImplicitly] public readonly float MinScale;
        [UsedImplicitly] public readonly float MaxScale;
        [UsedImplicitly] public readonly bool NeedCultivatedGround;
        [UsedImplicitly] public readonly bool DestroyIfCantGrow;
    }

    public static class PlantsManager
    {
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ConfigurePlantsPlugin.PluginGuid}.defaults.yaml";
        private static readonly string ConfigNamePatterns = $"{ConfigurePlantsPlugin.PluginGuid}.custom.*.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        private static readonly ISerializer Serializer = new SerializerBuilder()
            .DisableAliases()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        private static Dictionary<string, PlantModel> LoadPlants()
        {
            return PrefabManager.Cache.GetPrefabs(typeof(Plant))
                .ToDictionary(pair => pair.Key, pair => new PlantModel((Plant)pair.Value));
        }

        public static void WritePlants()
        {
            var yamlContent = Serializer.Serialize(LoadPlants());
            File.WriteAllText(DefaultFile, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }

        public static void RegisterPlantsFromCustomConfig()
        {
            List<string> configPaths =
                Directory.GetFiles(Paths.ConfigPath, ConfigNamePatterns, SearchOption.AllDirectories).ToList();
            if (!configPaths.Any())
            {
                Logger.LogInfo(
                    $"no config file found inside {Paths.ConfigPath} matching to pattern {ConfigNamePatterns}");
                return;
            }

            foreach (var configPath in configPaths)
            {
                Logger.LogInfo($"config file found: {configPath}");
            }

            Dictionary<string, PlantModel> configs = configPaths.Select(ReadFromFile).SelectMany(i => i)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (KeyValuePair<string, PlantModel> keyValuePair in configs)
            {
                GameObject plantObject = PrefabManager.Instance.GetPrefab(keyValuePair.Key);

                if (!plantObject.TryGetComponent(out Plant plant))
                {
                    Logger.LogError(
                        $"chosen original prefab '{keyValuePair.Key}' for creature doesn't have a 'Plant' " +
                        $"component, cannot reconfigure it");
                    return;
                }

                PlantModel configuredPlant = keyValuePair.Value;
                plant.m_name = configuredPlant.Name;
                plant.m_growRadius = configuredPlant.GrowRadius;
                plant.m_growTime = configuredPlant.GrowTime;
                plant.m_minScale = configuredPlant.MinScale;
                plant.m_maxScale = configuredPlant.MaxScale;
                plant.m_needCultivatedGround = configuredPlant.NeedCultivatedGround;
                plant.m_destroyIfCantGrow = configuredPlant.DestroyIfCantGrow;
                PrefabManager.Instance.DestroyPrefab(keyValuePair.Key);
                PrefabManager.Instance.AddPrefab(plantObject);
            }
        }

        private static Dictionary<string, PlantModel> ReadFromFile(string file)
        {
            try
            {
                var yamlContent = File.ReadAllText(file);
                return Deserializer.Deserialize<Dictionary<string, PlantModel>>(yamlContent);
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Unable to parse config file '{file}' due to {e.Message}");
            }

            return new Dictionary<string, PlantModel>();
        }
    }
}