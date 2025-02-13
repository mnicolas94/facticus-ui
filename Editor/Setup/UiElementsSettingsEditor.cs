using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils.Editor;

namespace UI.Editor.Setup
{
    public class UiElementsSettingsEditor : ScriptableObjectSingleton<UiElementsSettingsEditor>
    {
        [SerializeField] public Canvas canvasPrefab;
        [SerializeField] public TextMeshProUGUI textPrefab;
        [SerializeField] public Button textButtonPrefab;
        [SerializeField] public Button iconButtonPrefab;
        [SerializeField] public GameObject loadingIcon;

        public static UiElementsSettingsEditor GetOrCreate()
        {
            if (Instance == null)
            {
                // create directory
                var dir = "Assets/Editor/FacticusUI";
                Directory.CreateDirectory(dir);

                // create asset
                var settings = CreateInstance<UiElementsSettingsEditor>();
                var path = Path.Combine(dir, "UiElementsSettingsEditor.asset");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssetIfDirty(settings);
            }

            return Instance;
        }

        [MenuItem("GameObject/UI/Prefabs/Canvas")]
        private static void CreateCanvas()
        {
            InstantiatePrefab(GetOrCreate().canvasPrefab.gameObject);
        }
        
        [MenuItem("GameObject/UI/Prefabs/Text")]
        private static void CreateText()
        {
            InstantiatePrefab(GetOrCreate().textPrefab.gameObject);
        }
        
        [MenuItem("GameObject/UI/Prefabs/Button-Text")]
        private static void CreateTextButton()
        {
            InstantiatePrefab(GetOrCreate().textButtonPrefab.gameObject);
        }
        
        [MenuItem("GameObject/UI/Prefabs/Button-Icon")]
        private static void CreateIconButton()
        {
            InstantiatePrefab(GetOrCreate().iconButtonPrefab.gameObject);
        }
        
        [MenuItem("GameObject/UI/Prefabs/LoadingIcon")]
        private static void CreateLoadingIcon()
        {
            InstantiatePrefab(GetOrCreate().loadingIcon.gameObject);
        }
        
        private static void InstantiatePrefab(GameObject prefab)
        {
            var selectedObject = Selection.activeGameObject;
            var parent = selectedObject ? selectedObject.transform : null;
            PrefabUtility.InstantiatePrefab(prefab, parent);
        }
    }
}