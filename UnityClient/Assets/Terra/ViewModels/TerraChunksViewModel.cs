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
        
        public delegate void ChunkDelegate(TerraVector position, TerraWorldChunk chunk);

        public event ChunkDelegate OnChunkAdded;
        public event ChunkDelegate OnChunkRemoved;
        
        private Dictionary<TerraVector, TerraWorldChunk> _chunks = new Dictionary<TerraVector, TerraWorldChunk>();
        
        public void AddChunk(TerraVector position, TerraWorldChunk chunk)
        {
            _chunks[position] = chunk;
            OnChunkAdded?.Invoke(position, chunk);
        }
        
        public void RemoveChunk(TerraVector position)
        {
            _chunks.TryGetValue(position, out TerraWorldChunk chunk);
            _chunks.Remove(position);
            OnChunkRemoved?.Invoke(position, chunk);
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
            foreach (KeyValuePair<TerraVector, TerraWorldChunk> kvp in _chunks)
            {
                foreach (RuntimeTerraEntity entity in kvp.Value)
                {
                    yield return entity;
                }
            }
        }
        
        public IEnumerable<TerraEntity> GetEntities()
        {
            foreach (KeyValuePair<TerraVector, TerraWorldChunk> kvp in _chunks)
            {
                foreach (RuntimeTerraEntity entity in kvp.Value)
                {
                    yield return entity.Entity;
                }
            }
        }

        public IEnumerator<Chunk> GetChunks()
        {
            foreach (KeyValuePair<TerraVector, TerraWorldChunk> kvp in _chunks)
            {
                yield return new Chunk(){ vector = kvp.Key, chunk = kvp.Value};
            }
        }

        public void Reset()
        {
            
        }
    }
}