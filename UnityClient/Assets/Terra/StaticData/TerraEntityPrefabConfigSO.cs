using System;
using System.Collections.Generic;
using PandeaGames.Data.Static;
using Terra.SerializedData.Entities;
using Terra.SerializedData.GameData;
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
        public TerraEntityTypeSO PlayerConfig;
        public List<TerraEntityTypeSO> DataConfig;
        
        public List<GameObject> _config;
        public GameObject GetGameObject(ITerraEntity entity)
        {
            return GetGameObject(entity.EntityID);
        }
        
        public TerraEntityTypeData GetEntityConfig(ITerraEntityType type)
        {
            TerraEntityTypeData config = null;

            foreach (TerraEntityTypeSO go in DataConfig)
            {
                if (go.Data.EntityID == type.EntityID)
                {
                    config = go.Data;
                    break;
                }
            }

            return config;
        }
        
        public GameObject GetGameObject(string entityId)
        {
            GameObject config = null;

            foreach (GameObject go in _config)
            {
                if (go.name.Equals(entityId))
                {
                    config = go;
                    break;
                }
            }

            return config;
        }
    }
}