using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils.Editor;
using Object = UnityEngine.Object;

namespace UI.Editor.Setup
{
    public class UiElementsSetupManager : ScriptableObjectSingleton<UiElementsSetupManager>
    {
        [SerializeField] private Canvas _canvasPrefab;
        [SerializeField] private TextMeshProUGUI _textPrefab;
        [SerializeField] private Button _textButtonPrefab;
        [SerializeField] private Button _iconButtonPrefab;
        [SerializeField] private GameObject _loadingIcon;
        [SerializeField, HideInInspector] private Button _baseButtonPrefab;
        
        [MenuItem("Tools/Facticus.UI/Trigger initial ui setup")]
        private static void InitialSetup()
        {
            var directory = EditorUtility.OpenFolderPanel("Select Directory", "Assets", "");
            
            if (string.IsNullOrEmpty(directory))
                return;
            
            directory = Path.GetRelativePath(".", directory);
            if (!Directory.Exists(directory) || !directory.StartsWith("Assets"))
            {
                Debug.LogWarning($"Cannot create ui setup, directory is not valid: {directory}");
                return;
            }
                
            var settings = UiElementsSettingsEditor.GetOrCreate();

            Undo.SetCurrentGroupName("Setup ui prefabs");
            var group = Undo.GetCurrentGroup();
            Undo.RecordObject(settings, "Set ui prefabs into settings");
            
            // create new container
            var container = CreateInstance<UiElementsSetupManager>();
            
            settings.canvasPrefab = CopyAssetIntoDirectory(container._canvasPrefab, directory);
            settings.textPrefab = CopyAssetIntoDirectory(container._textPrefab, directory);
            
            // buttons
            var newBaseButtonPrefab = CopyAssetIntoDirectory(container._baseButtonPrefab, directory);
            settings.textButtonPrefab = CopyAssetIntoDirectory(container._textButtonPrefab, directory);
            settings.iconButtonPrefab = CopyAssetIntoDirectory(container._iconButtonPrefab, directory);
            ChangeParentOfVariant(settings.textButtonPrefab.gameObject, newBaseButtonPrefab.gameObject);
            ChangeParentOfVariant(settings.iconButtonPrefab.gameObject, newBaseButtonPrefab.gameObject);
            
            // loading icon
            settings.loadingIcon = CopyAssetIntoDirectory(container._loadingIcon, directory);
            var guidsMap = DuplicateDependenciesInSameDirectory(container._loadingIcon, directory);
            ReplaceGuidsReferencesInAsset(settings.loadingIcon, guidsMap);

            EditorUtility.SetDirty(settings.textButtonPrefab);
            EditorUtility.SetDirty(settings.iconButtonPrefab);
            EditorUtility.SetDirty(settings.loadingIcon);
            EditorUtility.SetDirty(settings);
            Undo.CollapseUndoOperations(group);
        }

        private static T CopyAssetIntoDirectory<T>(T asset, string directory) where T : Object
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var assetName = asset.name;
            assetName = assetName.Replace("Template", "");
            var extension = Path.GetExtension(assetPath);
            var newPath = Path.Combine(directory, $"{assetName}{extension}");
            AssetDatabase.CopyAsset(assetPath, newPath);
            var newAsset = AssetDatabase.LoadAssetAtPath<T>(newPath);
            
            // Undo.RegisterCreatedObjectUndo(newAsset, $"Created new {assetName}");
            return newAsset;
        }

        private static void ChangeParentOfVariant(GameObject variant, GameObject newParent)
        {
            var instance = Instantiate(variant);  // create plane instance of variant
            PrefabUtility.ConvertToPrefabInstance(  // convert to an instance of the new parent keeping overrides
                instance.gameObject,
                newParent.gameObject,
                new ConvertToPrefabInstanceSettings()
                {
                    changeRootNameToAssetName = false,
                    logInfo = false,
                    componentsNotMatchedBecomesOverride = true,
                    gameObjectsNotMatchedBecomesOverride = true,
                    recordPropertyOverridesOfMatches = true,
                },
                InteractionMode.AutomatedAction);
            
            // save as variant of parent in the same path as the variant to keep guid
            PrefabUtility.SaveAsPrefabAsset(instance.gameObject, AssetDatabase.GetAssetPath(variant));
            DestroyImmediate(instance);
        }
        
        private static Dictionary<string, string> DuplicateDependenciesInSameDirectory(Object asset, string newDirectory)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var directory = Path.GetDirectoryName(path);
            directory = directory.Replace("\\", "/");
            var dependencies = AssetDatabase.GetDependencies(path, true);
            
            var newOldGuidsMap = new Dictionary<string, string>();
            foreach (var dependency in dependencies)
            {
                var isSelf = dependency == path;
                var isLocal = dependency.StartsWith(directory);
                if (isLocal && !isSelf)
                {
                    var dependencyAsset = AssetDatabase.LoadAssetAtPath<Object>(dependency);
                    var newDependencyAsset = CopyAssetIntoDirectory(dependencyAsset, newDirectory);
                    var newDependencyPath = AssetDatabase.GetAssetPath(newDependencyAsset);
                    var oldGuid = AssetDatabase.AssetPathToGUID(dependency);
                    var newGuid = AssetDatabase.AssetPathToGUID(newDependencyPath);
                    newOldGuidsMap.Add(oldGuid, newGuid);
                }
            }

            return newOldGuidsMap;
        }

        private static void ReplaceGuidsReferencesInAsset(Object asset, Dictionary<string,string> newOldGuidsMap)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var assetFileContent = File.ReadAllText(assetPath);
            foreach (var (oldGuid, newGuid) in newOldGuidsMap)
            {
                assetFileContent = assetFileContent.Replace(oldGuid, newGuid);
            }
            File.WriteAllText(assetPath, assetFileContent);
            
            AssetDatabase.Refresh();
        }
    }
}