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

[CreateAssetMenu]
public class WorldIndexGenerator : ScriptableObject {

    [SerializeField]
    private string _version;

    [SerializeField]
    private string _dataFileExtensions;

    [SerializeField]
    private string _indexFileName;

    [SerializeField]
    private string _areaFilenameFormatSource;

    [SerializeField]
    private string _areaDataRelativeDirectory;

    [SerializeField]
    private SerializationType _serializationType;

    [SerializeField]
    private int _seed;

    public string DataFileExtensions { get { return _dataFileExtensions; } }
    public string IndexFileName { get { return _indexFileName; } }

    public string dataRootDirectory
    {
        get
        {
            return Application.persistentDataPath + "/" + _version + "/";
        }
    }

    public string areaDataDirectoryPath
    {
        get
        {
            return dataRootDirectory + _areaDataRelativeDirectory;
        }
    }

    public string indexFilePath
    {
        get
        {
            return dataRootDirectory + IndexFileName + "." + DataFileExtensions;
        }
    }

    [Header("World")]

    [SerializeField]
    private WorldDimensions _dimensions;

    [SerializeField]
    private int _areaDimensions;

    [Header("Layers")]

    [SerializeField]
    private NoiseLayer _cloudLayer;

    [SerializeField]
    private WaterLayer _waterLayer;

    [SerializeField]
    private HeightLayer _heightLayer;

    public WorldIndex GenerateIndex()
    {
        Debug.Log("Saving World to " + indexFilePath);

        SharpSerializer serializer = new SharpSerializer();

        WorldIndex index = new WorldIndex();

        index.Dimensions = _dimensions;
        index.AreaFilenameFormatSource = _areaFilenameFormatSource;
        index.AreaRelativeDirectory = _areaDataRelativeDirectory;
        index.FileDataExtension = _dataFileExtensions;
        index.Version = _version;
        index.SerializationType = _serializationType;
        index.AreaDimensions = _areaDimensions;

        Directory.CreateDirectory(dataRootDirectory);
        Directory.CreateDirectory(areaDataDirectoryPath);
        FileStream fileStream = File.Open(indexFilePath, FileMode.OpenOrCreate);

        Debug.Log("Saving World to " + indexFilePath);

        using (var stream = fileStream)
        {
            serializer.Serialize(index, fileStream);
        }

        int horizontalAreaCount = _dimensions.Width / _areaDimensions;
        int verticalAreaCount = _dimensions.Width / _areaDimensions;

        for (int i = 0; i< horizontalAreaCount; i++)
        {
            for (int j = 0; j < horizontalAreaCount; j++)
            {
                AreaIndex area = new AreaIndex();

                area.AlphaDataLayer = new DataLayer();
                area.AlphaDataLayer.NoiseLayerData = _cloudLayer.GenerateData(_dimensions, _areaDimensions, i, j);
                area.AlphaDataLayer.WaterLayerData = _waterLayer.GenerateData(_dimensions, _areaDimensions, i, j);
                area.AlphaDataLayer.HeightLayerData = _heightLayer.GenerateData(_dimensions, _areaDimensions, i, j);

                area.BetaDataLayer = area.AlphaDataLayer.Clone();

                string filename = string.Format(_areaFilenameFormatSource, i, j, _dataFileExtensions);
                FileStream areaFileStream = File.Open(areaDataDirectoryPath + filename, FileMode.OpenOrCreate);
                
                switch(_serializationType)
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