using System;
using System.Collections.Generic;
using Terra.SerializedData.Entities;

namespace Terra.ViewModels
{
    public interface ITerraEntityViewModel<TTerraEntity> : IEnumerable<RuntimeTerraEntity> where TTerraEntity : ITerraEntity
    {
        event Action<TTerraEntity> OnAddEntity;
        event Action<TTerraEntity> OnRemoveEntity;

        bool AddEntity(TTerraEntity entity);
        bool RemoveEntity(TTerraEntity entity);
    }
}