using System;
using System.Collections.Generic;
using Terra.Entities;

namespace Terra.ViewModels
{
    public interface ITerraEntityViewModel<TTerraEntity> : IEnumerable<TerraEntity> where TTerraEntity : TerraEntity
    {
        event Action<TTerraEntity> OnAddEntity;
        event Action<TTerraEntity> OnRemoveEntity;

        bool AddEntity(TTerraEntity entity);
        bool RemoveEntity(TTerraEntity entity);
    }
}