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

    private Dictionary<string, AreaIndex> _areaCache;
    private List<AreaRequest> _areaRequests;  // arbitary job data
    private WorldIndex _worldIndex;
    public List<AreaRequestResult> OutData; // arbitary job data
    public Dictionary<string, AreaRequestResult> OutDataTable; // arbitary job data
    public WorldDataToken token;
    private string _areaDataDirectoryPath;
    BinaryFormatter bf = new BinaryFormatter();

    public LoadAreaJob(List<AreaRequest> areaRequests, WorldIndex index, Dictionary<string, AreaIndex> areaCache, string areaDataDirectoryPath)
    {
        _areaRequests = areaRequests;
        _worldIndex = index;
        _areaCache = areaCache;
        _areaDataDirectoryPath = areaDataDirectoryPath;
        OutData = new List<AreaRequestResult>();
        OutDataTable = new Dictionary<string, AreaRequestResult>();
    }

    public bool ContainsMatchingAreaRequest(AreaRequest areaRequest)
    {
        foreach(AreaRequest matchingAreaRequest in _areaRequests)
        {
            if(areaRequest == matchingAreaRequest)
            {
                return true;
            }
        }

        return false;
    }

    protected override void ThreadFunction()
    {
        foreach (AreaRequest request in _areaRequests)
        {
            string filename = string.Format(_worldIndex.AreaFilenameFormatSource, request.areaX, request.areaY, _worldIndex.FileDataExtension);
            AreaIndex areaIndex = null;
            if (_areaCache.ContainsKey(filename))
            {
                _areaCache.TryGetValue(filename, out areaIndex);
            }
            else
            { 
                if(File.Exists(_areaDataDirectoryPath + filename))
                {
                    try
                    {
                        string allfilesString = string.Empty;
                        FileStream areaFileStream = File.Open(_areaDataDirectoryPath + filename, FileMode.Open);
                        areaIndex = (AreaIndex)bf.Deserialize(areaFileStream);
                        areaFileStream.Close();
                        allfilesString = string.Empty;
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Error while loading area files: \n"+e);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("File does not exist {0}", _areaDataDirectoryPath + filename);
                } 
            }
            AreaRequestResult areaRequestResult = new AreaRequestResult(request, areaIndex, _areaDataDirectoryPath + filename);
            OutDataTable.Add(request.areaX.ToString() + request.areaY.ToString(), areaRequestResult);
            OutData.Add(areaRequestResult);
        }
    }
    protected override void OnFinished()
    {
        
    }
}