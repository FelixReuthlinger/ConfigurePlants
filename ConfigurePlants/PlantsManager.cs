using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Managers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        private static readonly Dictionary<string, PlantModel> Plants = LoadPlants();

        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ConfigurePlantsPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        private static Dictionary<string, PlantModel> LoadPlants()
        {
            return PrefabManager.Cache.GetPrefabs(typeof(Plant))
                .ToDictionary(pair => pair.Key, pair => new PlantModel((Plant)pair.Value));
        }

        public static void WritePlants()
        {
            var yamlContent = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Serialize(Plants);
            File.WriteAllText(DefaultFile, yamlContent);
            Logger.LogInfo($"wrote yaml content to file '{DefaultFile}'");
        }
    }
}