using System;
using UnityEngine;


[CreateAssetMenu]
public class DataConfig : ScriptableObject
{
    public const string DirectoryDelimiter = "/";
    
    [SerializeField]
    private string _version;

    [SerializeField]
    private string _dataFileExtensions;
    
    [SerializeField]
    private string _areaFilenameFormatSource;
    
    [SerializeField]
    private string _indexFilename;

    [SerializeField]
    private string _areaDataRelativeDirectory;
    
    [SerializeField]
    private string _worldIndexRelativeDirectory;
    
    [SerializeField]
    private SerializationType _areaSerializationType;
    
    [SerializeField]
    private int _areaDimensions;
    
    public string Version
    {
        get { return _version; }
    }
    
    public string DataFileExtensions
    {
        get { return _dataFileExtensions; }
    }
    
    public string AreaFilenameFormatSource
    {
        get { return _areaFilenameFormatSource; }
    }
    
    public string IndexFilename
    {
        get { return String.Join(".", new string[] {_indexFilename, _dataFileExtensions}); }
    }
    
    public string AreaDataRelativeDirectory
    {
        get { return _areaDataRelativeDirectory; }
    }
    
    public SerializationType AreaSerializationType
    {
        get { return _areaSerializationType; }
    }
    
    public int AreaDimensions
    {
        get { return _areaDimensions; }
    }

    public string GetRelativeWorldIndexPath(string uid)
    {
        return String.Join(DirectoryDelimiter, new string[]
        {
            _worldIndexRelativeDirectory,
            uid
        });
    }
    
    public string GetRelativeWorldIndexPath(IWorldIndexGenerator generator)
    {
        return GetRelativeWorldIndexPath(generator.uid);
    }
}