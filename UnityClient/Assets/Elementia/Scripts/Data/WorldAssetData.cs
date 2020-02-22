using UnityEngine;
using System;


[Serializable]
public struct WorldPosition
{
    [SerializeField]
    private int _x;
    [SerializeField]
    private int _y;

    public int X { get { return _x; } set { _x = value; } }
    public int Y { get { return _y; } set { _y = value; } }

    public override string ToString()
    {
        return string.Format("[x:{0}, y:{1}]", _x, _y);
    }
}

[Serializable]
public struct WorldDimensions
{
    [SerializeField]
    private int _width;
    [SerializeField]
    private int _height;

    public int Width { get { return _width; } set { _width = value; } }
    public int Height { get { return _height; } set { _height = value; } }
}

//In order to maintain backwards compatability, do not remove or swap items around in this enum
public enum SerializationType
{
    SharpSerializer,
    Binary
}

public interface IWorldIndexGenerator
{
    string uid { get; }
    bool Exists(string persistentDataPath);
    WorldIndex Generate(string persistentDataPath);
    void Generate(string persistentDataPath, Action<WorldIndex> onComplete, Action onError);
    WorldIndex Load(string persistentDataPath);
}

public class WorldLayer
{

}

[Serializable]
public class WaterLayer : WorldLayer
{
    public ByteDataLater GenerateData(WorldDimensions worldDimensions, int areaDimensions, int x, int y)
    {
        ByteDataLater layer = new ByteDataLater();

        layer.data = new byte[areaDimensions, areaDimensions];

        for (int i = 0; i < areaDimensions; i++)
        {
            for (int j = 0; j < areaDimensions; j++)
            {
                layer.data[i, j] = 0;
            }
        }

        return layer;
    }
}

[Serializable]
public class HeightLayer : WorldLayer
{
    public UshortDataLater GenerateData(WorldDimensions worldDimensions, int areaDimensions, int x, int y)
    {
        UshortDataLater layer = new UshortDataLater();

        layer.data = new ushort[areaDimensions, areaDimensions];

        for (int i = 0; i < areaDimensions; i++)
        {
            for (int j = 0; j < areaDimensions; j++)
            {
                layer.data[i, j] = (ushort) UnityEngine.Random.Range(50, 52);
            }
        }

        return layer;
    }
}

[Serializable]
public class NoiseLayer : WorldLayer
{
    [SerializeField][Range(0, 1)]
    private float _depth;

    [SerializeField]
    public Sprite _noiseImageSource;

    public IntDataLater GenerateData(WorldDimensions worldDimensions, int areaDimensions, int x, int y)
    {
        IntDataLater layer = new IntDataLater();

        layer.data = new int[areaDimensions, areaDimensions];

        for(int i=0;i < areaDimensions; i++)
        {
            for (int j = 0; j < areaDimensions; j++)
            {
                layer.data[i, j] = UnityEngine.Random.Range(0, 100);
            }
        }

        return layer;
    }
}