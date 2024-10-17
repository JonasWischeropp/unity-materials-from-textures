using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.Import {
    [FilePath("ProjectSettings/Packages/wischeropp.jonas.materials-from-textures/Settings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class MaterialImporterSettings : ScriptableSingleton<MaterialImporterSettings> {
        [SerializeField]    
        public List<PropertyMapping> Mappings = new () {
            new PropertyMapping("albedo", "diffuse|diff|albedo|(base)?.?color"),
            new PropertyMapping("metallic", "metallic|gloss"),
            new PropertyMapping("normal", "normal(_gl)?|bump"),
            new PropertyMapping("occlusion", "(ambient)?.?occlusion|ao"),
            new PropertyMapping("emission", "emission|emissive"),
            new PropertyMapping("height", "height"),
            new PropertyMapping("roughness", "roughness")
            // Roughness
            // Detail maps
        };

        [SerializeField]
        public string CommonFileNameSuffix = "(_[0-9]k)?$";

        [SerializeField]
        public string LastSelectedShaderName = "Standard";

        public void SaveSettings() {
            Save(true);
        }
        
        public (Regex fileNameRegex, Regex propertyRegex)[] GetRegexFileNamePropertyMappings() {
            return Mappings
                .Select(mapping => (
                    new Regex($"({mapping.FileNamePattern})({CommonFileNameSuffix})", RegexOptions.IgnoreCase),
                    new Regex(mapping.PropertyPattern, RegexOptions.IgnoreCase)
                ))
                .ToArray();
        }
    }

    [Serializable]
    internal class PropertyMapping {
        [SerializeField]
        public string PropertyPattern;

        [SerializeField]
        public string FileNamePattern;

        public PropertyMapping(string propertyPattern, string fileNamePattern) {
            PropertyPattern = propertyPattern;
            FileNamePattern = fileNamePattern;
        }
    }
}
