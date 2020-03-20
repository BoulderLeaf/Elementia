using System.Collections.Generic;
using PandeaGames;
using PandeaGames.ViewModels;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;

namespace Terra.ViewModels
{
    public class TerraChunksViewModel : IViewModel
    {
        public struct Chunk
        {
            public TerraVector vector;
            public TerraWorldChunk chunk;
        }
        
        public delegate void ChunkDelegate(TerraWorldChunk chunk);

        public event ChunkDelegate OnChunkAdded;
        public event ChunkDelegate OnChunkRemoved;
        public TerraWorldChunk CurrentChunk;
        private Dictionary<TerraVector, TerraWorldChunk> _chunks = new Dictionary<TerraVector, TerraWorldChunk>();
        
        public void AddChunk(TerraWorldChunk chunk)
        {
            CurrentChunk = chunk;
            OnChunkAdded?.Invoke(chunk);
        }
        
        public void RemoveChunk(TerraVector position)
        {
            _chunks.TryGetValue(position, out TerraWorldChunk chunk);
            _chunks.Remove(position);
            OnChunkRemoved?.Invoke(chunk);
        }
        
        public TerraWorldChunk this[TerraVector vector]
        {
            get
            {
                if (_chunks.ContainsKey(vector))
                {
                    return _chunks[vector];
                }

                return null;
            }
        }
        
        public IEnumerable<RuntimeTerraEntity> GetRuntimeEntities()
        {
            return null;
        }
        
        public IEnumerable<TerraEntity> GetEntities()
        {
            return null;
        }

        public IEnumerator<Chunk> GetChunks()
        {
            return null;
        }

        public void Reset()
        {
            
        }
    }
}