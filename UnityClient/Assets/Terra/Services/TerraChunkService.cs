using System;
using PandeaGames;
using PandeaGames.Services;
using Terra.SerializedData.World;
using Terra.ViewModels;

namespace Terra.Services
{
    public class TerraPointSerializer : IDBSerializer<TerraPoint>
    {
        public const string TABLE = "TerraPoint";
        
        public string Table
        {
            get => TABLE;
        }

        public IDBColumn[] Columns
        {
            get
            {
                return new IDBColumn[]
                {
                    new IDBColumn() {ColumnName = "InstanceId", DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = "x", DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = "y", DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = "height", DataType = DBDataType.INTEGER},
                    new IDBColumn() {ColumnName = "soil", DataType = DBDataType.INTEGER}
                };
            }
        }

        public int PrimaryKeyColumnIndex
        {
            get { return 0; }
        }
        public TerraPoint Instantiate()
        {
            return default(TerraPoint);
        }

        public void ParseStringResult(ref TerraPoint serializable, int columnIndex, string value)
        {
            throw new ArgumentException();
        }

        public void ParseIntegerResult(ref TerraPoint serializable, int columnIndex, int value)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    //do nothing
                    break;
                }
                case 1:
                {
                    serializable.Position = new TerraVector(value, serializable.Position.y);
                    break;
                }
                case 2:
                {
                    serializable.Position = new TerraVector(serializable.Position.x, value);
                    break;
                }
                case 3:
                {
                    serializable.Height = value;
                    break;
                }
                case 4:
                {
                    serializable.Soil = value;
                    break;
                }
            }
        }

        public void ParseNumericResult(ref TerraPoint serializable, int columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        public string GetValue(TerraPoint serializable, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                {
                    return serializable.InstanceID.ToString();
                }
                case 1:
                {
                    return serializable.Position.x.ToString();
                }
                case 2:
                {
                    return serializable.Position.y.ToString();
                }
                case 3:
                {
                    return serializable.Height.ToString();
                }
                case 4:
                {
                    return serializable.Soil.ToString();
                }
            }
            
            throw new ArgumentException();
        }
    }
    
    public class TerraPointWhereClause : TerraDBService.IDBWhereClause<TerraPoint>
    {
        private TerraArea _area;
        public TerraPointWhereClause(TerraArea area)
        {
            _area = area;
        }
        public string Where(TerraPoint serializable)
        {
            return
                $"WHERE x > {_area.x} AND x < {_area.x + _area.width} AND y > {_area.y} AND y < {_area.y + _area.height}";
        }
    }
    
    public class TerraPointUpdateWhereClause : TerraDBService.IDBWhereClause<TerraPoint>
    {
        private TerraPoint _point;
        public TerraPointUpdateWhereClause(TerraPoint point)
        {
            _point = point;
        }
        public string Where(TerraPoint serializable)
        {
            return $"WHERE x = {_point.Position.x} AND y = {_point.Position.y}";
        }
    }

    public struct TerraPoint : IDBSerializable
    {
        public static TerraPointSerializer Serializer { get; } = new TerraPointSerializer();
        
        public static TerraDBService.DBPrimaryKeyWhereClause<TerraPoint, TerraPointSerializer> WherePrimaryKey
        {
            get;
        } = new TerraDBService.DBPrimaryKeyWhereClause<TerraPoint, TerraPointSerializer>(Serializer);

        public int InstanceID
        {
            get { return Position.GetHashCode(); }
        }

        public TerraVector Position;
        public int Height;
        public int Soil;

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
    
    public class TerraChunkService : AbstractService<TerraChunkService>
    {
        public delegate void TerraWorldChunkDelegate(TerraWorldChunk chunk);
        public delegate void TerraWorldChunkErrorDelegate(Exception exception);

        private TerraChunksViewModel _viewModel;
        private TerraDBService _db;
        
        public TerraChunkService()
        {
            _db = Game.Instance.GetService<TerraDBService>();
            _viewModel = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
        }
        public int FindDifference(int nr1, int nr2)
        {
            return Math.Abs(nr1 - nr2);
        }
        public void GetChunk(
            TerraArea area,
            TerraWorldChunkDelegate onComplete,
            TerraWorldChunkErrorDelegate onError)
        {
            TerraPoint[] terraPoints =_db.Get<TerraPointSerializer, TerraPoint>(TerraPoint.Serializer,
                $"WHERE x > {area.x} AND x < {area.x + area.width} AND y > {area.y} AND y < {area.y + area.height}");
            
            TerraPoint[,] grid = new TerraPoint[area.width,area.height];

            foreach (TerraPoint terraPoint in terraPoints)
            {
                int x = FindDifference(area.x, terraPoint.Position.x);
                int y = FindDifference(area.y, terraPoint.Position.y);
                grid[x,y] = terraPoint;
            }

            for (int x = 0; x < grid.GetLongLength(0); x++)
            {
                for (int y = 0; y < grid.GetLongLength(1); y++)
                {
                    TerraPoint point = grid[x, y];
                    point.Position = new TerraVector(area.x + x, area.y + y);
                    grid[x, y] = point;
                }
            }
            
            TerraWorldChunk chunk = new TerraWorldChunk(grid, _db, area);
            onComplete(chunk);
        }

        public void Save(Action onComplete, TerraWorldChunkErrorDelegate onError)
        {
            //TODO: save everything in _viewModel
        }
    }
}