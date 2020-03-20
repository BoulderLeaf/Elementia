using System;
using Terra.Services;
using UnityEngine;

namespace Terra.SerializedData.Entities
{
    public class TerraPosition3DSerializer : IDBSerializer<TerraPosition3D>
    {
        public const string TABLE = "TerraPosition3D";
        
        public string Table
        {
            get { return TABLE; }
        }

        public IDBColumn[] Columns
        {
            get
            {
                return new IDBColumn[]
                {
                    new IDBColumn() {ColumnName = TerraEntitySerializer.COLUMN_INSTNACE_ID, DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = "x", DataType = DBDataType.NUMERIC},
                    new IDBColumn() {ColumnName = "y", DataType = DBDataType.NUMERIC},
                    new IDBColumn() {ColumnName = "z", DataType = DBDataType.NUMERIC}
                };
            }
        }

        public int PrimaryKeyColumnIndex
        {
            get { return 0; }
        }

        public TerraPosition3D Instantiate()
        {
            return new TerraPosition3D();
        }

        public void ParseStringResult(ref TerraPosition3D serializable, int columnIndex, string value)
        {
            throw new System.NotImplementedException();
        }

        public void ParseIntegerResult(ref TerraPosition3D serializable, int columnIndex, int value)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    serializable.InstanceId = value;
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }
        
        public void ParseNumericResult(ref TerraPosition3D serializable, int columnIndex, float value)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    throw new ArgumentException();
                }
                case 1:
                {
                    serializable.x = value;
                    break;
                }
                case 2:
                {
                    serializable.y= value;
                    break;
                }
                case 3:
                {
                    serializable.z = value;
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }

        public string GetValue(TerraPosition3D serializable, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    return serializable.InstanceId.ToString();
                }
                case 1:
                {
                    return serializable.x.ToString();
                }
                case 2:
                {
                    return serializable.y.ToString();
                }
                case 3:
                {
                    return serializable.z.ToString();
                }
            }

            throw new ArgumentException();
        }
    }

    public class TerraPosition3D : IDBSerializable
    {
        public static TerraPosition3DSerializer Serializer { get; } = new TerraPosition3DSerializer();
        public static TerraDBService.DBPrimaryKeyWhereClause<TerraPosition3D, TerraPosition3DSerializer> WherePrimaryKey
        {
            get;
        } = new TerraDBService.DBPrimaryKeyWhereClause<TerraPosition3D, TerraPosition3DSerializer>(Serializer);
        
        public int InstanceId { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        
        public static bool operator ==(TerraPosition3D a, TerraPosition3D b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.InstanceId == b.InstanceId;
        }

        public override bool Equals(object obj)
        {
            if (obj is TerraPosition3D)
            {
                return obj as TerraPosition3D == this;
            }

            return base.Equals(obj);
        }

        public static bool operator !=(TerraPosition3D a, TerraPosition3D b)
        {
            return a != b;
        }

        public TerraPosition3D Set(Vector3 position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
            return this;
        }
        
        public static implicit operator Vector3(TerraPosition3D unityVector)
        {
            return new Vector3()
            {
                x = unityVector.x, y = unityVector.y, z = unityVector.z
            };
        }
    }
}