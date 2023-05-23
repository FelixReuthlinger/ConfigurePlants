using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Jotunn;
using Jotunn.Managers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigurePlants
{

    public static class PlantsManager
    {
        private static readonly Dictionary<string, Plant> Plants = LoadPlants();
        
        private static readonly string DefaultConfigRootPath = Paths.ConfigPath;
        private static readonly string DefaultOutputFileName = $"{ConfigurePlantsPlugin.PluginGuid}.defaults.yaml";
        private static readonly string DefaultFile = Path.Combine(DefaultConfigRootPath, DefaultOutputFileName);

        private static Dictionary<string, Plant> LoadPlants()
        {
            return PrefabManager.Cache.GetPrefabs(typeof(Plant))
                .ToDictionary(pair => pair.Key, pair => (Plant)pair.Value);
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