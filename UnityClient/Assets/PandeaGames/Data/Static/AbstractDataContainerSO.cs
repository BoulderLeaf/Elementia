using UnityEngine;

namespace PandeaGames.Data.Static
{
    public abstract class AbstractDataContainerSO<TData>:ScriptableObject
    {
        [SerializeField] 
        private TData _data;
        public TData Data => _data;
    }
}