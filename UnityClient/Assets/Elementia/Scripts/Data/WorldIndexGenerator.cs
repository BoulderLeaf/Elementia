using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Polenter.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

[CreateAssetMenu]
public class WorldAsset : ScriptableObject, IWorldIndexGenerator
{
    [Header("Config")]

    [SerializeField]
    private DataConfig _dataConfig;
    
    [Header("World")]

    [SerializeField]
    private WorldDimensions _dimensions;
    
    [SerializeField]
    private int _seed;
    
    [SerializeField]
    private string _uid;
    
    public string RootPath(string persistentDataPath)
    {
        return string.Join(DataConfig.DirectoryDelimiter,
            new string[] {persistentDataPath, _dataConfig.GetRelativeWorldIndexPath(_uid)});
    }
    
    public string uid
    {
        get { return _uid; }
    }
    
    public bool Exists(string persistentDataPath)
    {
        string worldDirectory = RootPath(persistentDataPath);
        string areaDirectory = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.AreaDataRelativeDirectory});
        string indexFilePath = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.IndexFilename});
        
        return File.Exists(indexFilePath);
    }
    
    [Header("Layers")]

    [SerializeField]
    private NoiseLayer _cloudLayer;

    [SerializeField]
    private WaterLayer _waterLayer;

    [SerializeField]
    private HeightLayer _heightLayer;

    public WorldIndex Load(string persistentDataPath)
    {
        SharpSerializer serializer = new SharpSerializer();
        WorldIndex index = null;
        
        string worldDirectory = string.Join(DataConfig.DirectoryDelimiter, new string[]{persistentDataPath, _dataConfig.GetRelativeWorldIndexPath(_uid)});
        string areaDirectory = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.AreaDataRelativeDirectory});
        string indexFilePath = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.IndexFilename});
        
        FileStream fileStream = File.Open(indexFilePath, FileMode.Open);

        Debug.Log("Loading World from " + indexFilePath);

        using (var stream = fileStream)
        {
            index = serializer.Deserialize(stream) as WorldIndex;
        }

        index.SetGenerator(this);

        return index;
    }
    
    public WorldIndex Generate(string persistentDataPath)
    {
        SharpSerializer serializer = new SharpSerializer();

        WorldIndex index = new WorldIndex();

        index.Dimensions = _dimensions;
        index.AreaFilenameFormatSource = _dataConfig.AreaFilenameFormatSource;
        index.AreaRelativeDirectory = _dataConfig.AreaDataRelativeDirectory;
        index.FileDataExtension = _dataConfig.DataFileExtensions;
        index.Version = _dataConfig.Version;
        index.SerializationType = _dataConfig.AreaSerializationType;
        index.AreaDimensions = _dataConfig.AreaDimensions;

        string worldDirectory = string.Join(DataConfig.DirectoryDelimiter, new string[]{persistentDataPath, _dataConfig.GetRelativeWorldIndexPath(_uid)});
        string areaDirectory = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.AreaDataRelativeDirectory});
        string indexFilePath = string.Join(DataConfig.DirectoryDelimiter, new string[]{worldDirectory, _dataConfig.IndexFilename});

        int areaDimensions = _dataConfig.AreaDimensions;
        
        Debug.Log("Saving World to " + indexFilePath);
        
        Directory.CreateDirectory(worldDirectory);
        Directory.CreateDirectory(areaDirectory);
        FileStream fileStream = File.Open(indexFilePath, FileMode.CreateNew);

        using (var stream = fileStream)
        {
            serializer.Serialize(index, fileStream);
        }

        int horizontalAreaCount = _dimensions.Width / areaDimensions;
        int verticalAreaCount = _dimensions.Width / areaDimensions;

        for (int i = 0; i< horizontalAreaCount; i++)
        {
            for (int j = 0; j < horizontalAreaCount; j++)
            {
                AreaIndex area = new AreaIndex();

                area.AlphaDataLayer = new DataLayer();
                area.AlphaDataLayer.NoiseLayerData = _cloudLayer.GenerateData(_dimensions, areaDimensions, i, j);
                area.AlphaDataLayer.WaterLayerData = _waterLayer.GenerateData(_dimensions, areaDimensions, i, j);
                area.AlphaDataLayer.HeightLayerData = _heightLayer.GenerateData(_dimensions, areaDimensions, i, j);

                area.BetaDataLayer = area.AlphaDataLayer.Clone();

                string filename = string.Format(_dataConfig.AreaFilenameFormatSource, i, j, _dataConfig.DataFileExtensions);
                FileStream areaFileStream = File.Open(String.Join(DataConfig.DirectoryDelimiter, new string[]{areaDirectory, filename}), FileMode.CreateNew);
                
                switch(_dataConfig.AreaSerializationType)
                {
                    case SerializationType.Binary:
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(areaFileStream, area);
                        break;
                    case SerializationType.SharpSerializer:
                        using (var stream = areaFileStream)
                        {  
                            serializer.Serialize(area, areaFileStream);
                        }
                        
                        break;
                }

                areaFileStream.Close();
            }
        }

        return index;
    }

    public void Generate(string persistentDataPath, Action<WorldIndex> onComplete, Action onError)
    {
        
    }
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