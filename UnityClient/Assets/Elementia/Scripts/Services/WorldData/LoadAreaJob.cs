using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LoadAreaJob: ThreadedJob
{
    public class AreaRequest
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

        public override string ToString()
        {
            return string.Format("[areaX:{0}, areaY:{1}]", _areaX, _areaY);
        }

        public string GetAreaKey()
        {
            return String.Join("_", new string[]{areaX.ToString(), areaY.ToString()});
        }
    }

    public class AreaRequestResult
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
    public bool IsRunning;
    public WorldDataToken token;
    private DataConfig _dataConfig;
    private LoadedArea _loadedArea;
    private string _areaDataDirectoryPath;
    BinaryFormatter bf = new BinaryFormatter();

    public LoadAreaJob(WorldIndex index, DataConfig dataConfig, string areaDataDirectoryPath)
    {
        _worldIndex = index;
        _areaDataDirectoryPath = areaDataDirectoryPath;
        _dataConfig = dataConfig;
    }

    public void SetJob(LoadedArea loadedArea)
    {
        _areaRequest = loadedArea.Request;
        _loadedArea = loadedArea;
    }

    protected override void ThreadFunction()
    {
        IsRunning = true;
        while (IsRunning)
        {
            try
            {
                if (_areaRequest != null)
                {
                    AreaIndex areaIndex = null;
                    if(File.Exists(GetFilePath()))
                    {
                        string allfilesString = string.Empty;
                        FileStream areaFileStream = File.Open(GetFilePath(), FileMode.OpenOrCreate);
                        areaIndex = (AreaIndex)bf.Deserialize(areaFileStream);
                        areaFileStream.Close();
                        allfilesString = string.Empty;
                    }
                    else
                    {
                        Debug.LogErrorFormat("File does not exist {0}", GetFilePath());
                    }
                
                    OutData = new AreaRequestResult(_areaRequest, areaIndex, GetFilePath());
                    _loadedArea.SetResult(OutData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while loading area files: \n"+e);
                throw;
            }

            _areaRequest = null;
            Thread.Sleep(100);
        }
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