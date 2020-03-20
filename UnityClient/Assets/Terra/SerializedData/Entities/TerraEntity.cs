using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using Terra.SerializedData.GameData;
using Terra.Services;
using UnityEditor;
using UnityEngine;

namespace Terra.SerializedData.Entities
{
    public class TerraEntitySerializer : IDBSerializer<TerraEntity>
    {
        public const string TABLE = "TerraEntities";

        public const string COLUMN_INSTNACE_ID = "instanceId";
        public const string COLUMN_ENTITY_ID = "entityId";
        
        public string Table
        {
            get => TABLE;
        }

        public IDBColumn[] Columns
        {
            get
            {
                return new[]
                {
                    new IDBColumn() {ColumnName = COLUMN_INSTNACE_ID, DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = COLUMN_ENTITY_ID, DataType = DBDataType.TEXT}
                };
            }
        }

        public int PrimaryKeyColumnIndex {get{return 0;}}
        
        public string GetValue(TerraEntity terraEntity, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    return terraEntity.InstanceId.ToString();
                    break;
                }
                case 1:
                {
                    return terraEntity.EntityID;
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }

        public void ParseIntegerResult(ref TerraEntity terraEntity, int columnIndex, int value)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    terraEntity.InstanceId = value;
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }

        public void ParseNumericResult(ref TerraEntity terraEntity, int columnIndex, float value)
        {
            throw new ArgumentException();
        }

        public TerraEntity Instantiate()
        {
            return new TerraEntity();
        }

        public void ParseStringResult(ref TerraEntity terraEntity, int columnIndex, string value)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    throw new ArgumentException();
                }
                case 1:
                {
                    terraEntity.EntityID = value;
                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }
    }
    
    public class TerraEntity : ITerraEntity, IDBSerializable
/*, IDBSerializableCollection<TerraEntitySerializer, TerraEntity>*/
    {
        public event Action<TerraEntity, string> OnLabelRemoved;
        public event Action<TerraEntity, string> OnLabelAdded;
        
        public static TerraEntitySerializer Serializer { get; } = new TerraEntitySerializer();

        public static TerraDBService.DBPrimaryKeyWhereClause<TerraEntity, TerraEntitySerializer> WherePrimaryKey
        {
            get;
        } = new TerraDBService.DBPrimaryKeyWhereClause<TerraEntity, TerraEntitySerializer>(Serializer);
        
        public Dictionary<EntityComponent, IEntityComponent> Components { get; } = new Dictionary<EntityComponent, IEntityComponent>();
        
        public int InstanceId { get; set; } = System.Guid.NewGuid().GetHashCode();
        public HashSet<string> Labels { get; set; } = new HashSet<string>();
        public string EntityID { get; set; } = "";
        public long RowID = 0;

        public TerraEntity() : this(0, string.Empty)
        {
        }
        
        public TerraEntity(long rowId, string entityId)
        {
            EntityID = entityId;
            RowID = rowId;
        }

        public IEnumerator<TerraEntity> GetEnumerator()
        {
           /* foreach (KeyValuePair<EntityComponent, IEntityComponent> kvp in Components)
            {
                yield return kvp.Value;
            }*/
           return null;
        }

        public override int GetHashCode()
        {
            return InstanceId;
        }

        /*IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }*/

        public void AddLabel(string label)
        {
            Labels.Add(label);
            OnLabelAdded?.Invoke(this, label);
        }

        public void RemoveLabel(string label)
        {
            Labels.Remove(label);
            OnLabelRemoved?.Invoke(this, label);
        }

        public bool HasLabel(string label)
        {
            return Labels.Contains(label);
        }


        public TerraDBService.TerraDBRequest WriteRequest()
        {
            return default(TerraDBService.TerraDBRequest);
            /*
            return new TerraDBService.TerraDBRequest()
            {
                CommandText =
                    $"UPSERT INTO {TABLE} WHERE instanceId = {InstanceId} (instanceId, entityId) Values(@instanceId, @entityId)",
                Values = new string[] {InstanceId.ToString(), EntityID}
            };*/
        }
    }
}