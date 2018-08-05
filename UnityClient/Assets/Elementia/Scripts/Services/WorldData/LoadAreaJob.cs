using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LoadAreaJob: ThreadedJob
{
    public struct AreaRequest
    {
        private int _areaX;
        private int _areaY;

        public int areaX { get { return _areaX; } }
        public int areaY { get { return _areaY; } }

        public AreaRequest(int areaX, int areaY)
        {
            _areaX = areaX;
            _areaY = areaY;
        }

        public static bool operator ==(AreaRequest a1, AreaRequest a2)
        {
            return a1.areaX == a2.areaX && a1.areaY == a2.areaY;
        }

        public static bool operator !=(AreaRequest a1, AreaRequest a2)
        {
            return a1.areaX != a2.areaX || a1.areaY != a2.areaY;
        }

        public override string ToString()
        {
            return string.Format("[areaX:{0}, areaY:{1}]", _areaX, _areaY);
        }

        public string GetAreaKey()
        {
            return String.Join("_", new string[]{areaX.ToString(), areaY.ToString()});
        }
    }

    public struct AreaRequestResult
    {
        private AreaRequest _request;
        private AreaIndex _result;
        private string _filepath;

        public AreaRequest Request { get { return _request; } }
        public AreaIndex Result { get { return _result; } }
        public string Filepath { get { return _filepath; } }

        public AreaRequestResult(AreaRequest request, AreaIndex result, string filepath)
        {
            _request = request;
            _result = result;
            _filepath = filepath;
        }
    }

    private AreaRequest _areaRequest;  // arbitary job data
    private WorldIndex _worldIndex;
    public AreaRequestResult OutData; // arbitary job data
    public WorldDataToken token;
    private DataConfig _dataConfig;
    private string _areaDataDirectoryPath;
    BinaryFormatter bf = new BinaryFormatter();

    public LoadAreaJob(AreaRequest areaRequest, WorldIndex index, DataConfig dataConfig, string areaDataDirectoryPath)
    {
        _areaRequest = areaRequest;
        _worldIndex = index;
        _areaDataDirectoryPath = areaDataDirectoryPath;
        _dataConfig = dataConfig;
    }

    protected override void ThreadFunction()
    {
        AreaIndex areaIndex = null;
        Debug.Log("LoadAreaJob: "+GetFilePath());
        if(File.Exists(GetFilePath()))
        {
            try
            {
                string allfilesString = string.Empty;
                FileStream areaFileStream = File.Open(GetFilePath(), FileMode.OpenOrCreate);
                areaIndex = (AreaIndex)bf.Deserialize(areaFileStream);
                areaFileStream.Close();
                allfilesString = string.Empty;
                Debug.Log("LoadAreaJob DONE: "+GetFilePath());
            }
            catch(Exception e)
            {
                Debug.LogError("Error while loading area files: \n"+e);
            }
        }
        else
        {
            Debug.LogErrorFormat("File does not exist {0}", GetFilePath());
        }
        
        OutData = new AreaRequestResult(_areaRequest, areaIndex, GetFilePath());
    }
    
    public string GetFilePath()
    {
        return String.Join(DataConfig.DirectoryDelimiter,
            new string[]
            {
                _areaDataDirectoryPath,
                _dataConfig.GetRelativeWorldIndexPath(_worldIndex.GetGenerator()),
                _dataConfig.AreaDataRelativeDirectory,
                string.Format(_worldIndex.AreaFilenameFormatSource, _areaRequest.areaX, _areaRequest.areaY,
                    _worldIndex.FileDataExtension)
            });
    }

    public string GetAreaKey()
    {
        return _areaRequest.GetAreaKey();
    }
}