using System;
using System.Collections.Generic;
using System.Linq;
using PandeaGames.Data;
using Terra.SerializedData.GameData;
using Terra.Services;

namespace Terra.SerializedData.Entities
{
    public partial class RuntimeTerraEntity : ITerraEntity
    {
        public event Action<RuntimeTerraEntity, string> OnLabelAdded;
        public event Action<RuntimeTerraEntity, string> OnLabelRemoved;
            
        event Action<TerraEntity, string> ITerraEntity.OnLabelRemoved
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event Action<TerraEntity, string> ITerraEntity.OnLabelAdded
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        
        public TerraEntity Entity { private set; get; }
        public TerraDBService DB { private set; get; }

        public TerraPosition3DComponent Position;
        public TerraEntityTypeData EntityTypeData { get; private set; }

        public override int GetHashCode()
        {
            return Entity.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == this.GetHashCode();
        }

        public RuntimeTerraEntity(AssembledEntity entity, TerraDBService db)
        {
            entity.TerraPosition3D.InstanceId = entity.TerraEntity.InstanceId;
            Position = new TerraPosition3DComponent(db,entity.TerraPosition3D);
            Entity = entity.TerraEntity;
            
            Entity.OnLabelAdded += (labelEntity, label) => OnLabelAdded?.Invoke(this, label);
            Entity.OnLabelRemoved += (labelEntity, label) => OnLabelRemoved?.Invoke(this, label);
            
            EntityTypeData = TerraGameResources.Instance.TerraEntityPrefabConfig.GetEntityConfig(this);
            DB = db;
        }

        public int InstanceId
        {
            get => Entity.InstanceId;
            set => Entity.InstanceId = value;
        }
        public HashSet<string> Labels
        {
            get => Entity.Labels;
            set => Entity.Labels = value;
        }

        public string EntityID
        {
            get => Entity.EntityID;
            set => Entity.EntityID = value;
        }
    }
    
    

    public struct AssembledEntity : IDBSerializable
    {
        public TerraEntity TerraEntity;
        public TerraPosition3D TerraPosition3D;

        public AssembledEntity(TerraEntity entity)
        {
            TerraEntity = entity;
            TerraPosition3D = new TerraPosition3D();
        }
    }

    //Use LEFT OUTER JOIN to GET this
    public class AssembledEntitySerializer : IDBSerializer<AssembledEntity>
    {
        public static TerraEntitySerializer TerraEntitySerializer = new TerraEntitySerializer();
        public static TerraPosition3DSerializer TerraPosition3DSerializer = new TerraPosition3DSerializer();

        private IDBSchema[] Schemas { get; } = new IDBSchema[]
        {
            TerraEntitySerializer,
            TerraPosition3DSerializer
        };

        public string Table => TerraEntitySerializer.Table;
        public IDBColumn[] Columns => TerraEntitySerializer.Columns.Concat(TerraPosition3DSerializer.Columns).ToArray();

        public int PrimaryKeyColumnIndex => TerraEntitySerializer.PrimaryKeyColumnIndex;
        
        public AssembledEntity Instantiate()
        {
            return new AssembledEntity()
            {
                TerraEntity = new TerraEntity(), TerraPosition3D = new TerraPosition3D()
            };
        }

        private struct SerializerScrubber
        {
            public int SerializerIndex;
            public int SerializerColumnIndex;
        }
        
        private SerializerScrubber GetSerializerIndex(int columnIndex)
        {
            SerializerScrubber scrubber = new SerializerScrubber();
            int totalSerializerColumnIndex = 0;
            for (scrubber.SerializerIndex = 0; scrubber.SerializerIndex < Schemas.Length; scrubber.SerializerIndex++)
            {
                scrubber.SerializerColumnIndex = columnIndex - totalSerializerColumnIndex;
                if (columnIndex < totalSerializerColumnIndex + Schemas[scrubber.SerializerIndex].Columns.Length)
                {
                    break;
                }
                
                totalSerializerColumnIndex += Schemas[scrubber.SerializerIndex].Columns.Length;
            }

            return scrubber;
        }

        public void ParseStringResult(ref AssembledEntity serializable, int columnIndex, string value)
        {
            SerializerScrubber scrubber = GetSerializerIndex(columnIndex);

            if (scrubber.SerializerIndex == 0)
            {
                TerraEntitySerializer.ParseStringResult(ref serializable.TerraEntity, scrubber.SerializerColumnIndex, value);
            } 
            else if (scrubber.SerializerIndex == 1)
            {
                TerraPosition3DSerializer.ParseStringResult(ref serializable.TerraPosition3D, scrubber.SerializerColumnIndex, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ParseIntegerResult(ref AssembledEntity serializable, int columnIndex, int value)
        {
            SerializerScrubber scrubber = GetSerializerIndex(columnIndex);

            if (scrubber.SerializerIndex == 0)
            {
                TerraEntitySerializer.ParseIntegerResult(ref  serializable.TerraEntity, scrubber.SerializerColumnIndex, value);
            } 
            else if (scrubber.SerializerIndex == 1)
            {
                TerraPosition3DSerializer.ParseIntegerResult(ref  serializable.TerraPosition3D, scrubber.SerializerColumnIndex, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ParseNumericResult(ref AssembledEntity serializable, int columnIndex, float value)
        {
            SerializerScrubber scrubber = GetSerializerIndex(columnIndex);

            if (scrubber.SerializerIndex == 0)
            {
                TerraEntitySerializer.ParseNumericResult(ref serializable.TerraEntity, scrubber.SerializerColumnIndex, value);
            } 
            else if (scrubber.SerializerIndex == 1)
            {
                TerraPosition3DSerializer.ParseNumericResult(ref serializable.TerraPosition3D, scrubber.SerializerColumnIndex, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string GetValue(AssembledEntity serializable, int columnIndex)
        {
            SerializerScrubber scrubber = GetSerializerIndex(columnIndex);

            if (scrubber.SerializerIndex == 0)
            {
                return TerraEntitySerializer.GetValue(serializable.TerraEntity, scrubber.SerializerColumnIndex);
            } 
            else if (scrubber.SerializerIndex == 1)
            {
                return TerraPosition3DSerializer.GetValue(serializable.TerraPosition3D, scrubber.SerializerColumnIndex);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}