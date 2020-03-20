using System;
using System.Collections.Generic;
using Terra.Services;

namespace Terra.SerializedData.World
{
    public class TerraDataPoint : GridDataPoint<TerraPoint>
    {
        public TerraDataPoint()
        {
            
        }
        public TerraDataPoint(TerraPoint data, TerraVector vector) : base(data, vector)
        {

        }
    }
    
    public class TerraWorldChunk : AbstractGridDataModel<TerraPoint, TerraDataPoint>
    {
        private TerraArea _area;
        private TerraDBService _db;

        public TerraWorldChunk(TerraPoint[,] points, TerraDBService db, TerraArea area) : base(points)
        {
            _db = db;
            _area = area;
        }

        public TerraPoint GetFromWorld(TerraVector vector)
        {
            return this[FindDifference(_area.x, vector.x), FindDifference(_area.y, vector.y)];
        }
        
        public void SetFromWorld(TerraVector vector, TerraPoint point)
        {
            this[FindDifference(_area.x, vector.x), FindDifference(_area.y, vector.y)] = point;
        }
        
        public int FindDifference(int nr1, int nr2)
        {
            return Math.Abs(nr1 - nr2);
        }

        public override TerraPoint this[int x, int y]
        {
            get {
                return base[Math.Min(x, Width - 1), Math.Min(y, Height - 1)]; 
            }
            set
            {
                DataHasChanged(new TerraDataPoint[]{new TerraDataPoint(value, new TerraVector(x, y))});
                _db.Write(value, TerraPoint.Serializer, TerraPoint.WherePrimaryKey);
                base[x, y] = value;
            }
        }
    }
}