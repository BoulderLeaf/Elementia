
using System;


[Serializable]
public struct TerraVector
{
    public int x;
    public int y;
    
    public TerraVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode()
    {
        //https://math.stackexchange.com/questions/23503/create-unique-number-from-2-numbers
        return (x * x) * (y * y * y);
    }

    public static implicit operator TerraVector(int radius)
    {
        return new TerraVector(radius, radius);
    }
}

public interface ITerraShape
{
    TerraVector[] TerraVectors { get; }
}
