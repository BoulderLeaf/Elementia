using Data;
using Terra.StaticData;
using UnityEditor;
using UnityEngine;
    
namespace PandeaGames.Data
{
    public class TerraGameResources : ScriptableObjectSingleton<TerraGameResources>, ILoadableObject
    {
#if UNITY_EDITOR
        public const string AssetPath = "Assets/Resources/Terra/TerraGameResources.asset";
        
        [MenuItem("Terra/Data/Generate Game Resources")]
        public static void CreateAsset()
        {
            TerraGameResources gameResources = ScriptableObjectFactory.CreateInstance<TerraGameResources>();
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
        
        [SerializeField] private Material _terrainMaterial;
        public Material TerrainMaterial
        {
            get => _terrainMaterial;
        }
        
        public void LoadAsync(LoadSuccess onLoadSuccess, LoadError onLoadFailed)
        {
            LoaderGroup loaderGroup = new LoaderGroup();
            loaderGroup.LoadAsync(onLoadSuccess, onLoadFailed);
        }

        public bool IsLoaded { get; }
    }
}