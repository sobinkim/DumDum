using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace SB.App.Editor
{
    public static class ProjectFontApplier
    {
        private const string SourceFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Ownglyph_ParkDaHyun.ttf";
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Ownglyph_ParkDaHyun SDF.asset";

        [MenuItem("Tools/DumDum/Apply Park DaHyun Font")]
        public static void Apply()
        {
            AssetDatabase.ImportAsset(SourceFontPath, ImportAssetOptions.ForceSynchronousImport);

            UnityEngine.Font sourceFont = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(SourceFontPath);
            if (sourceFont == null)
                throw new System.InvalidOperationException("Unable to load source font: " + SourceFontPath);

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    2048,
                    2048,
                    TMPro.AtlasPopulationMode.Dynamic,
                    true);
                fontAsset.name = "Ownglyph_ParkDaHyun SDF";
                fontAsset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
                AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
                AssetDatabase.SaveAssets();
            }

            ApplyTmpSettings(fontAsset);
            ApplyToPrefabs(fontAsset);
            ApplyToScenes(fontAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Applied project font: " + fontAsset.name);
        }

        private static void ApplyTmpSettings(TMP_FontAsset fontAsset)
        {
            TMP_Settings settings = TMP_Settings.instance;
            if (settings == null)
                return;

            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;

            SerializedProperty fallbackAssets = serializedSettings.FindProperty("m_fallbackFontAssets");
            fallbackAssets.arraySize = 1;
            fallbackAssets.GetArrayElementAtIndex(0).objectReferenceValue = fontAsset;

            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static void ApplyToPrefabs(TMP_FontAsset fontAsset)
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/SB" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            foreach (string prefabPath in prefabPaths)
            {
                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                bool changed = ApplyToTexts(prefab, fontAsset);
                if (changed)
                    PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void ApplyToScenes(TMP_FontAsset fontAsset)
        {
            string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/SB", "Assets/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            foreach (string scenePath in scenePaths)
            {
                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool changed = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                    changed |= ApplyToTexts(root, fontAsset);

                if (changed)
                    EditorSceneManager.SaveScene(scene);
            }
        }

        private static bool ApplyToTexts(GameObject root, TMP_FontAsset fontAsset)
        {
            bool changed = false;
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text.font == fontAsset && text.fontSharedMaterial == fontAsset.material)
                    continue;

                text.font = fontAsset;
                text.fontSharedMaterial = fontAsset.material;
                EditorUtility.SetDirty(text);
                changed = true;
            }

            return changed;
        }
    }
}
