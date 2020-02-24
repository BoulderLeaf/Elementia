using System;
using System.Collections.Generic;
using PandeaGames.Data.Static;
using UnityEngine;

namespace Terra.StaticData
{
    [CreateAssetMenu(menuName = "Terra/TerraEntityPrefabConfig")]
    public class TerraEntityPrefabConfigSO : AbstractDataContainerSO<TerraEntityPrefabConfig>
    {
        
    }

    [Serializable]
    public class TerraEntityPrefabConfig
    {
        public List<GameObject> _config;

        public GameObject GetGameObject(string id)
        {
            GameObject config = null;

            foreach (GameObject go in _config)
            {
                if (go.name.Equals(id))
                {
                    config = go;
                    break;
                }
            }

            return config;
        }
    }
}