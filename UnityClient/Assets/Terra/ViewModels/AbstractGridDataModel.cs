
using System;
using System.Collections.Generic;

public class GridDataPoint<TData>
{
    public TData Data;
    public TerraVector Vector;

    public GridDataPoint()
    {
        
    }
    
    public GridDataPoint(TData data, TerraVector vector)
    {
        Data = data;
        Vector = vector;
    }
}

public abstract class AbstractGridDataModel<TData, TGridDataPoint> where TGridDataPoint : GridDataPoint<TData>, new()
{
    public delegate void DataHasChangedDelegate(IEnumerable<TGridDataPoint> data);

    public event DataHasChangedDelegate OnDataHasChanged;
    
    protected TData[,] _data {private set; get; }

    public readonly int Width;
    public readonly int Height;

    public AbstractGridDataModel(TData[,] data)
    {
        _data = data;
        Width = data.GetLength(0);
        Height = data.GetLength(1);
    }
    
    public AbstractGridDataModel(uint width, uint height)
    {
        _data = new TData[width, height];
    }

    public virtual TData this[TerraVector vector]
    {
        get { return _data[vector.x, vector.y]; }
        set { _data[vector.x, vector.y] = value; }
    }
    
    public virtual TData this[int x, int y]
    {
        get { return _data[x, y]; }
        set { _data[x, y] = value; }
    }

    public IEnumerable<TGridDataPoint> AllData()
    {
        for (int x = 0; x < _data.GetLength(0); x++)
        {
            for (int y = 0; y < _data.GetLength(1); y++)
            {
                yield return new TGridDataPoint() {Data = _data[x, y], Vector = new TerraVector(x, y)};
            }
        }
    }

    public virtual void UpdateData(TerraVector vector)
    {
        
    }

    protected virtual void DataHasChanged(IEnumerable<TGridDataPoint> data)
    {
        OnDataHasChanged?.Invoke(data);
    }

    protected virtual IEnumerable<TGridDataPoint> GenerateVectorGrid(TerraVector center, TerraVector dimensions)
    {
        return GenerateVectorGrid(
            left: Math.Max(0, center.x - dimensions.x),
            top: Math.Max(0, center.y - dimensions.y),
            right: Math.Min(Width - 1, center.x + dimensions.x),
            bottom: Math.Min(Height - 1, center.y + dimensions.y));
    }
    
    protected virtual IEnumerable<TGridDataPoint> GenerateVectorGrid(int left, int top, int right, int bottom)
    {
        for (int x = left; x <= right; x++)
        {
            for (int y = top; y <= bottom; y++)
            {
                yield return new TGridDataPoint() {Data = _data[x, y], Vector = new TerraVector(x, y)};
            }
        }
    }
}
