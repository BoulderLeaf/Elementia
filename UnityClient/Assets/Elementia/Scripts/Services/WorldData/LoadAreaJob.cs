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

    private WorldIndex _worldIndex;
    public bool IsRunning;
    private DataConfig _dataConfig;
    private List<LoadedArea> _loadedAreasRequests;
    private string _areaDataDirectoryPath;
    BinaryFormatter bf = new BinaryFormatter();
    public event Action<LoadAreaJob> OnComplete;

    public LoadAreaJob(WorldIndex index, DataConfig dataConfig, List<LoadedArea> loadedAreas, string areaDataDirectoryPath)
    {
        _worldIndex = index;
        _areaDataDirectoryPath = areaDataDirectoryPath;
        _dataConfig = dataConfig;
        _loadedAreasRequests = new List<LoadedArea>();
        SetJob(loadedAreas);
    }

    public void SetJob(LoadedArea loadedArea)
    {
        _loadedAreasRequests.Add(loadedArea);
    }
    
    public void SetJob(List<LoadedArea> loadedAreas)
    {
        _loadedAreasRequests.AddRange(loadedAreas);
    }

    protected override void ThreadFunction()
    {
        IsRunning = true;
        //while (IsRunning)
        //{
            try
            {
                bool hasRequest = _loadedAreasRequests.Count != 0;
                
                if (hasRequest)
                {
                    _loadedAreasRequests.ForEach((loadedArea) =>
                    {
                        AreaIndex areaIndex = null;
                        if(File.Exists(GetFilePath(loadedArea)))
                        {
                            string allfilesString = string.Empty;
                            FileStream areaFileStream = File.Open(GetFilePath(loadedArea), FileMode.OpenOrCreate);
                            areaIndex = (AreaIndex)bf.Deserialize(areaFileStream);
                            areaFileStream.Close();
                            allfilesString = string.Empty;
                        }
                        else
                        {
                            Debug.LogErrorFormat("File does not exist {0}", GetFilePath(loadedArea));
                        }
                
                        loadedArea.SetResult(new AreaRequestResult(loadedArea.Request, areaIndex, GetFilePath(loadedArea)));
                        Thread.Sleep(100);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while loading area files: \n"+e);
                throw;
            }

            _loadedAreasRequests.Clear();
            if (OnComplete != null)
                OnComplete(this);
        //}
    }
    
    public string GetFilePath(LoadedArea loadedArea)
    {
        return String.Join(DataConfig.DirectoryDelimiter,
            new string[]
            {
                _areaDataDirectoryPath,
                _dataConfig.GetRelativeWorldIndexPath(_worldIndex.GetGenerator()),
                _dataConfig.AreaDataRelativeDirectory,
                string.Format(_worldIndex.AreaFilenameFormatSource, loadedArea.Request.areaX, loadedArea.Request.areaY,
                    _worldIndex.FileDataExtension)
            });
    }
}