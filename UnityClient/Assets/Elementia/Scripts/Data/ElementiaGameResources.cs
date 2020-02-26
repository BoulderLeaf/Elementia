using Data;
using Terra.StaticData;
using UnityEditor;
using UnityEngine;
    
namespace PandeaGames.Data
{
    public class ElementiaGameResources : ScriptableObjectSingleton<ElementiaGameResources>, ILoadableObject
    {
#if UNITY_EDITOR
        public const string AssetPath = "Assets/Resources/PandeaGames/ElementiaGameResources.asset";
        
        [MenuItem("Elementia/Data/Generate Game Resources")]
        public static void CreateAsset()
        {
            ElementiaGameResources gameResources = ScriptableObjectFactory.CreateInstance<ElementiaGameResources>();
            AssetDatabase.CreateAsset(gameResources, AssetPath);
            EditorUtility.DisplayDialog("Asset Created", string.Format("Asset has been added at '{0}'", AssetPath),
                "OK");
        }
#endif
        [SerializeField] private TerraEntityPrefabConfigSO _terraEntityPrefabConfigSO;
        public TerraEntityPrefabConfig TerraEntityPrefabConfig
        {
            get => _terraEntityPrefabConfigSO.Data;
        }
        
        public void LoadAsync(LoadSuccess onLoadSuccess, LoadError onLoadFailed)
        {
            LoaderGroup loaderGroup = new LoaderGroup();
            loaderGroup.LoadAsync(onLoadSuccess, onLoadFailed);
        }

        public bool IsLoaded { get; }
    }
}