using UnityEngine;
using System;
using System.IO;
using Polenter.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[CreateAssetMenu, Serializable]
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

                area.DataLayer = new DataLayer();
                area.DataLayer.NoiseLayerData = _cloudLayer.GenerateData(_dimensions, areaDimensions, i, j);
                area.DataLayer.WaterLayerData = _waterLayer.GenerateData(_dimensions, areaDimensions, i, j);
                area.DataLayer.HeightLayerData = _heightLayer.GenerateData(_dimensions, areaDimensions, i, j);

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
