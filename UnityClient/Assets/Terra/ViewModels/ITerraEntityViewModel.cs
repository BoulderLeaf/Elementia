using System;
using Terra.Entities;

namespace Terra.ViewModels
{
    public interface ITerraEntityViewModel<TTerraEntity> where TTerraEntity : TerraEntity
    {
        event Action<TTerraEntity> OnAddEntity;
        event Action<TTerraEntity> OnRemoveEntity;

        bool AddEntity(TTerraEntity entity);
        bool RemoveEntity(TTerraEntity entity);
    }
}