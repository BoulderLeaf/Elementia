using System;
using UnityEngine;

namespace PandeaGames.Data
{
    [Serializable]
    public abstract class DataSO<TData> : ScriptableObject
    {
        [SerializeField] 
        private TData _data;
        public TData Data
        {
            get => _data;
        }
    }
}