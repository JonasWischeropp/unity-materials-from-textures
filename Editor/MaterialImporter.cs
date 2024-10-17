using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

namespace JonasWischeropp.Unity.EditorTools.Import {
    public class MaterialImporter {
        public static void MaterialsFromDirectories(
            IEnumerable<string> directories,
            Shader shader,
            (Regex fileNameRegex, Regex propertyRegex)[] fileNamePropertyMappings
        ) {
            (string displayName, string name)[] textureProperties = Enumerable.Range(0, shader.GetPropertyCount())
                .Where(i => shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                .Select(i => (shader.GetPropertyDescription(i), shader.GetPropertyName(i)))
                .ToArray();

            foreach (var directory in directories) {
                Material material = MaterialFromDirectoryTextures(directory, shader,
                    textureProperties, fileNamePropertyMappings);
                string directoryName = Path.GetFileName(directory);
                string parentPath = Path.GetDirectoryName(directory);
                AssetDatabase.CreateAsset(material, $"{parentPath}/{directoryName}.mat");
            }
        }

        static Material MaterialFromDirectoryTextures(
            string directory,
            Shader shader,
            (string displayName, string name)[] textureProperties,
            (Regex fileNameRegex, Regex propertyRegex)[] fileNamePropertyMappings
        ) {
            var mappings = Directory.GetFiles(directory)
                .Select(file => AssetDatabase.LoadAssetAtPath<Texture2D>(file))
                .Where(texture => texture != null)
                .Select(texture => {
                    foreach (var mapping in fileNamePropertyMappings) {
                        if (mapping.fileNameRegex.IsMatch(texture.name)) {
                            return (texture, mapping.propertyRegex);
                        }
                    }
                    return (texture, null);
                })
                .Where(e => e.propertyRegex != null)
                .Select(e => {
                    foreach (var property in textureProperties) {
                        if (e.propertyRegex.IsMatch(property.displayName)) {
                            return (e.texture, property.name);
                        }
                    }
                    return (e.texture, null);
                })
                .Where(e => e.name != null);

            Material material = new Material(shader);
            foreach (var mapping in mappings) {
                material.SetTexture(mapping.name, mapping.texture);
            }
            return material;
        }
    }

    public class MyEditorWindow : EditorWindow {
        private const string MENU_ITEM_PATH = "Assets/Create/Material(s) from Textures";

        private static IEnumerable<string> directories;

        [MenuItem(MENU_ITEM_PATH, priority = 302)]
        static void ShowDialogWindow() {
            directories = Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath);
            MyEditorWindow window = GetWindow<MyEditorWindow>();
        }

        [MenuItem(MENU_ITEM_PATH, true)]
        static bool ShowDialogWindowValidator() {
            return Selection.assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .All(path => AssetDatabase.IsValidFolder(path));
        }

        private void CreateGUI() {
            titleContent = new GUIContent("Textures to Material");

            var settings = MaterialImporterSettings.instance;

            List<string> shaderNames = ShaderUtil.GetAllShaderInfo()
                .Select(info => info.name)
                .Where(name => !name.StartsWith("Hidden/"))
                .ToList();

            VisualElement root = rootVisualElement;
            var list = new MultiColumnListView() {
                showAddRemoveFooter = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                itemsSource = settings.Mappings,
                selectionType = SelectionType.Multiple,
            };
            list.columns.Add(new Column {
                title = "Shader Property Name Regex",
                makeCell = () => new TextField(),
                bindCell = (element, index) => {
                    TextField textField = element as TextField;
                    textField.value = settings.Mappings[index].PropertyPattern;
                    textField.RegisterValueChangedCallback(evt
                        => settings.Mappings[index].PropertyPattern = evt.newValue);
                },
                stretchable = true,
            });
            list.columns.Add(new Column {
                title = "File Name Regex",
                makeCell = () => new TextField(),
                bindCell = (element, index) => {
                    TextField textField = element as TextField;
                    textField.value = settings.Mappings[index].FileNamePattern;
                    textField.RegisterValueChangedCallback(evt
                        => settings.Mappings[index].FileNamePattern = evt.newValue);
                },
                stretchable = true,
            });
            list.itemsAdded += (addedIndices) => {
                foreach (int index in addedIndices) {
                    settings.Mappings[index] = new PropertyMapping("", "");
                }
            };

            var suffixInput = new TextField("Common File Name Suffix") {
                value = settings.CommonFileNameSuffix,
            };
            suffixInput.RegisterValueChangedCallback(evt
                => settings.CommonFileNameSuffix = evt.newValue);

            int index = shaderNames.IndexOf(settings.LastSelectedShaderName);
            var shaderDropdown = new DropdownField("Shader", shaderNames, index);
            shaderDropdown.RegisterValueChangedCallback(evt
                => settings.LastSelectedShaderName = evt.newValue);

            var importButton = new Button(() => {
                Shader shader = Shader.Find(settings.LastSelectedShaderName);
                MaterialImporter.MaterialsFromDirectories(directories, shader,
                    settings.GetRegexFileNamePropertyMappings());
                Close();
            }) {
                text = "Create Material(s)",
            };

            root.Add(list);
            root.Add(suffixInput);

            root.Add(shaderDropdown);
            root.Add(importButton);
        }

        private void OnDisable() {
            MaterialImporterSettings.instance.SaveSettings();
        }
    }
}
// var suffixParent = new VisualElement {
//     style = {
//         flexDirection = FlexDirection.Row,
//     },
// };
// var suffixInput = new TextField("Common suffix") {
//     value = settings.CommonSuffix,
//     style = {
//         flexGrow = 2,
//     },
// };
// var suffixOptionalToggle = new Toggle("Optional") {
//     style = {
//         flexGrow = 1,
//     },
// };

// root.Add(suffixParent);
// suffixParent.Add(suffixInput);
// suffixParent.Add(suffixOptionalToggle);

// Identifying the MIME type would be better but this is simpler.
// static HashSet<string> validExtensions = new HashSet<string>{
//     ".bmp", ".dib",
//     ".exr",
//     ".gif",
//     ".hdr",
//     ".iff",
//     ".pict", ".pct", ".pic",
//     ".png",
//     ".jpg", ".jpeg",
//     ".psd",
//     ".tga", ".icb", ".vda", ".vst",
//     ".tiff", ".tif"
// };
//
// static RegexOptions regexOptions = RegexOptions.IgnoreCase;
// static string optionalCommonSuffix = "_[0-9]k";

// static Regex MyRegex(string pattern) => new Regex($"({pattern})({optionalCommonSuffix})?$", regexOptions);
// // static List<(Regex propertyRegex, Regex fileRegex)> map = new () {
// //     (new Regex("_MainTex", regexOptions), new Regex("base.?color(_[0-9]k)?", regexOptions)),
// //     (new Regex("_MetallicGlossMap", regexOptions), new Regex("metallic|gloss", regexOptions)),
// //     (new Regex("_BumpMap", regexOptions), new Regex("normal|bump", regexOptions)),
// //     (new Regex("_OcclusionMap", regexOptions), new Regex("occlusion", regexOptions)),
// //     (new Regex("_EmissionMap", regexOptions), new Regex("emission", regexOptions)),
// // };
// static List<(Regex propertyRegex, Regex fileRegex)> map = new () {
//     (new Regex("_MainTex", regexOptions), MyRegex("base.?color")),
//     (new Regex("_MetallicGlossMap", regexOptions), MyRegex("metallic|gloss")),
//     (new Regex("_BumpMap", regexOptions), MyRegex("normal(_gl)|bump")),
//     (new Regex("_OcclusionMap", regexOptions), MyRegex("(ambient)?.?occlusion")),
//     (new Regex("_EmissionMap", regexOptions), MyRegex("emission|emissive")),
//     (new Regex("_ParallaxMap", regexOptions), MyRegex("height")),
//     // Roughness
//     // Detail maps
// };
