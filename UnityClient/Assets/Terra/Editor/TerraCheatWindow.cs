using System;
using PandeaGames;
using PandeaGames.Data;
using Terra.SerializedData.Entities;
using Terra.SerializedData.GameData;
using Terra.Services;
using Terra.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Terra.Editor
{
    public class TerraCheatWindow : EditorWindow
    {
        [MenuItem("Terra/CheatWindow")]
        public static void CreateWindow()
        {
            GetWindow<TerraCheatWindow>();
        }

        public void OnGUI()
        {
            foreach (TerraEntityTypeSO entityTypeSO in TerraGameResources.Instance.TerraEntityPrefabConfig.DataConfig)
            {
                ITerraEntityType entityType = entityTypeSO.Data;
                if (GUILayout.Button($"Create {entityType.EntityID}"))
                {
                    CreateEntity(entityType);
                }
            }
        }

        private void CreateEntity(ITerraEntityType type)
        {
            RuntimeTerraEntity entity = Game.Instance.GetService<TerraEntitesService>().CreateEntity(type);
            TerraEntitiesViewModel vm = Game.Instance.GetViewModel<TerraEntitiesViewModel>(0);
            RuntimeTerraEntity player = vm.GetEntity(TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig.Data);

            if (player != null)
            {
                entity.Position.Set(new Vector3(player.Position.Data.x, player.Position.Data.y + 2,
                    player.Position.Data.z));
            }

            vm.AddEntity(entity);
        }
    }
}