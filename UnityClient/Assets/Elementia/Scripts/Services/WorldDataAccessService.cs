using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Polenter.Serialization;

public class WorldDataAccessService : Service
{
    private WorldPersistanceService _worldPersistanceService;
    private WorldSimulationStateService _worldSimulationStateService;

    private List<WorldDataToken> _tokens;
    private Dictionary<string, AreaIndex> _areaCache;
    private WorldSimulationState _worldSimulationState;
    private List<LoadAreaJob> _jobCache = new List<LoadAreaJob>();

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);

        _areaCache = new Dictionary<string, AreaIndex>();
        _tokens = new List<WorldDataToken>();
        _worldPersistanceService = serviceManager.GetService<WorldPersistanceService>();
        _worldSimulationStateService = serviceManager.GetService<WorldSimulationStateService>();
        _worldSimulationStateService.Load((simulationState) => _worldSimulationState = simulationState, () => { });
        _jobCache.Clear();
    }

    public void ReturnToken(WorldDataToken token)
    {
        return;
        if (_tokens.Contains(token))
        {
            _tokens.Remove(token);

            HashSet<string> _removeCachedAreas = new HashSet<string>();

            foreach (KeyValuePair<AreaIndex, string> keyValuePair in token.Filepaths)
            {
                _removeCachedAreas.Add(keyValuePair.Value);
            }

            foreach (WorldDataToken cachedToken in _tokens)
            {
                foreach (KeyValuePair<AreaIndex, string> keyValuePair in cachedToken.Filepaths)
                {
                    if (_removeCachedAreas.Contains(keyValuePair.Value))
                    {
                        _removeCachedAreas.Remove(keyValuePair.Value);
                    }
                }
            }

            string[] removeResult = new string[_removeCachedAreas.Count];
            _removeCachedAreas.CopyTo(removeResult);

            foreach (string removeCache in removeResult)
            {
                _areaCache[removeCache].Destroy();
                _areaCache.Remove(removeCache);
            }
        }
    }

    public void SaveAndReturnToken(WorldDataToken token, Action onComplete)
    {
        SaveToken(token, (savedToken) => {
            ReturnToken(savedToken);
            onComplete();
        });
    }

    public void SaveToken(WorldDataToken token, Action<WorldDataToken> onComplete)
    {
        StartCoroutine(SaveTokenCoroutine(token, onComplete));
    }
    private HashSet<string> savingAreas = new HashSet<string>();
    public IEnumerator SaveTokenCoroutine(WorldDataToken token, Action<WorldDataToken> onComplete)
    {
        List<SaveAreaJob.SaveAreaRequest> saveRequests = new List<SaveAreaJob.SaveAreaRequest>();
        foreach (KeyValuePair<AreaIndex, string> tokenFile in token.Filepaths)
        {
            if(!savingAreas.Contains(tokenFile.Value))
            {
                savingAreas.Add(tokenFile.Value);
                saveRequests.Add(new SaveAreaJob.SaveAreaRequest(tokenFile.Key, tokenFile.Value));
            }
        }

        SaveAreaJob job = new SaveAreaJob(saveRequests, token.WorldIndex);
        job.Start();
        yield return new WaitUntil(() => job.IsDone);
        onComplete(token);
        saveRequests.ForEach((request) => savingAreas.Remove(request.filename));
    }

    public void GetToken(TokenRequest request, Action<WorldDataToken> onComplete, Action onError)
    {
        StartCoroutine(GetTokenCoroutine(request, onComplete, onError));
    }

    private int testInt = 0;
    private int remainingJobs = 0;
    private IEnumerator GetTokenCoroutine(TokenRequest request, Action<WorldDataToken> onComplete, Action onError)
    {
        DateTime timeCheck = DateTime.UtcNow;
        
        WorldIndex index = null;
        bool hasError = false;
        _worldPersistanceService.Load((worldIndex) => { index = worldIndex; }, () => { hasError = true; });
        DateTime startWorldIndexLoad = DateTime.UtcNow;
        yield return new WaitUntil(() => index != null || hasError || (DateTime.UtcNow - startWorldIndexLoad).TotalMilliseconds > 10000);
        timeCheck = DateTime.UtcNow;

        if ((DateTime.UtcNow - startWorldIndexLoad).TotalMilliseconds > 10000)
        {
            Debug.Log("TIMEOUT ERROR LOADING WORLD INDEX");
        }

        if (!hasError)
        {
            int leftArea = request.left / index.AreaDimensions;
            int rightArea = (request.right - 1) / index.AreaDimensions;
            int topArea = request.top / index.AreaDimensions;
            int bottomArea = (request.bottom - 1) / index.AreaDimensions;

            int horizontalAreaCount = (rightArea - leftArea) + 1;
            int verticalAreaCount = (bottomArea - topArea) + 1;

            AreaIndex[,] areas = new AreaIndex[horizontalAreaCount, verticalAreaCount];
            Dictionary<AreaIndex, string> filepaths = new Dictionary<AreaIndex, string>();

            List<LoadAreaJob.AreaRequest> requests = new List<LoadAreaJob.AreaRequest>();
            List<LoadAreaJob.AreaRequest> loadRequests = new List<LoadAreaJob.AreaRequest>();
            //List<string> 

            List<LoadAreaJob> collaborators = new List<LoadAreaJob>();

            for (int i = 0; i < horizontalAreaCount; i++)
            {
                for (int j = 0; j < verticalAreaCount; j++)
                {
                    int areaX = i + leftArea;
                    int areaY = j + topArea;
                    LoadAreaJob.AreaRequest newRequest = new LoadAreaJob.AreaRequest(areaX, areaY);
                    bool hasMatching = false;
                    foreach (LoadAreaJob job in _jobCache)
                    {
                        if(job.ContainsMatchingAreaRequest(newRequest))
                        {
                            hasMatching = true;
                            collaborators.Add(job);
                            continue;
                        }
                    }
                    
                    if (!hasMatching)
                    {
                        requests.Add(newRequest);
                        loadRequests.Add(newRequest);
                    }
                    else
                    {
                        requests.Add(newRequest);
                    }
                }
            }

            LoadAreaJob loadAreaJob = new LoadAreaJob(loadRequests, index, _areaCache, _worldPersistanceService.areaDataDirectoryPath);
            _jobCache.Add(loadAreaJob);
            loadAreaJob.Start();
            int jobNumber = testInt++;
            DateTime startLoad = DateTime.UtcNow;
            bool timeout = false;

            yield return new WaitUntil(() =>
            {
                bool isDone = true;
                isDone &= loadAreaJob.IsDone;
                collaborators.ForEach((collaborator) => isDone &= collaborator.IsDone);
                timeout = (DateTime.UtcNow - startLoad).TotalMilliseconds > 10000;
                isDone |= timeout;
                return isDone;
            });
            
            timeCheck = DateTime.UtcNow;
            if (!timeout)
            {
                requests.ForEach((loadedRequest) =>
                {
                    LoadAreaJob.AreaRequestResult areaResult = default(LoadAreaJob.AreaRequestResult);
                    loadAreaJob.OutDataTable.TryGetValue(loadedRequest.areaX.ToString() + loadedRequest.areaY.ToString(), out areaResult);

                    if(areaResult.Result != null)
                    {
                        filepaths[areaResult.Result] = areaResult.Filepath;
                        _areaCache[areaResult.Filepath] = areaResult.Result;
                        areas[areaResult.Request.areaX - leftArea, areaResult.Request.areaY - topArea] = areaResult.Result;
                    }

                    collaborators.ForEach((collaborator) =>
                    {
                        areaResult = default(LoadAreaJob.AreaRequestResult);
                        loadAreaJob.OutDataTable.TryGetValue(loadedRequest.areaX.ToString() + loadedRequest.areaY.ToString(), out areaResult);

                        if (areaResult.Result != null)
                        {
                            filepaths[areaResult.Result] = areaResult.Filepath;
                            areas[areaResult.Request.areaX - leftArea, areaResult.Request.areaY - topArea] = areaResult.Result;
                        }
                    });
                });

                _jobCache.Remove(loadAreaJob);

                WorldDataToken token = new WorldDataToken(request, index, areas, filepaths);
                _tokens.Add(token);
                Debug.Log("timeCheck3: " + (DateTime.UtcNow - timeCheck).TotalMilliseconds);
                onComplete(token);
            }
            else
            {
                Debug.LogError("Timeout error occured attempting to load area files.");
                onError();
            }
        }
        else
        {
            Debug.LogError("There was an error loading the World Index");
            onError();
        }
    }
}

public class SaveAreaJob : ThreadedJob
{
    public class SaveAreaRequest
    {
        public AreaIndex area;
        public string filename;
        public SaveAreaRequest(AreaIndex area, string filename)
        {
            this.area = area;
            this.filename = filename;
        }
    }

    private List<SaveAreaRequest> _requests;
    private WorldIndex _worldIndex;
    public SaveAreaJob(List<SaveAreaRequest> requests, WorldIndex worldIndex)
    {
        _requests = requests;
        _worldIndex = worldIndex;
    }

    protected override void ThreadFunction()
    {
        foreach (SaveAreaRequest saveAreaRequest in _requests)
        {
            FileStream areaFileStream = File.Open(saveAreaRequest.filename, FileMode.Create);

            switch (_worldIndex.SerializationType)
            {
                case SerializationType.Binary:
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(areaFileStream, saveAreaRequest.area);
                    break;
                case SerializationType.SharpSerializer:
                    SharpSerializer serializer = new SharpSerializer();
                    using (var stream = areaFileStream)
                    {
                        serializer.Serialize(saveAreaRequest.area, areaFileStream);
                    }
                    break;
            }

            areaFileStream.Close();
        }
    }
}

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

public class WorldDataToken
{
    private struct PixelInformation
    {
        public int areaX;
        public int areaY;
        public int areaPixelX;
        public int areaPixelY;

        public override string ToString()
        {
            return string.Format("[areaX:{0}, areaY:{1}, areaPixelX:{2}, areaPixelY:{3}]", areaX, areaY, areaPixelX, areaPixelY);
        }
    }

    private AreaIndex[,] _areas;
    private Dictionary<AreaIndex, string> _filepaths;
    private WorldIndex _index;
    private TokenRequest _request;

    public TokenRequest Request { get { return _request; } }
    public WorldIndex WorldIndex { get { return _index; } }
    public Dictionary<AreaIndex, string> Filepaths { get { return _filepaths; } }

    public AreaIndex[,] Areas
    {
        get
        {
            return _areas;
        }
    }

    public WorldDataToken(TokenRequest request, WorldIndex index, AreaIndex[,] areas, Dictionary<AreaIndex, string> filepaths)
    {
        _request = request;
        _index = index;
        _areas = areas;
        _filepaths = filepaths;
    }

    private PixelInformation GetPixelInformation(int x, int y)
    {
        PixelInformation info = default(PixelInformation);

        int areaDim = _index.AreaDimensions;
        int trueAreaX = (int)((_request.left + x) / areaDim);
        int requestedAreaX = (int)(_request.left / areaDim);
        int trueAreaY = (int)((_request.top + y) / areaDim);
        int requestedAreaY = (int)(_request.top / areaDim);

        info.areaX = trueAreaX - requestedAreaX;
        info.areaY = trueAreaY - requestedAreaY;
        info.areaPixelX = (_request.left + x) % areaDim;
        info.areaPixelY = (_request.top + y) % areaDim;

        return info;
    }

    private bool AreCoordinatesInvalid(int x, int y)
    {
        return x < 0 || x > _request.width || y < 0 || y > _request.height;
    }

    private bool IsPixelInformationInvalid(PixelInformation info)
    {
        return info.areaX >= _areas.Length || info.areaY >= _areas.GetLongLength(1);
    }

    public ushort GetUshort(int x, int y, UshortDataID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("Thigns went BAD " + info);
        }

        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case UshortDataID.HeightLayerData:
                return area.AlphaDataLayer.HeightLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetUshort(int x, int y, ushort value, UshortDataID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case UshortDataID.HeightLayerData:
                area.AlphaDataLayer.HeightLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }

    public int GetInt(int x, int y, IntDataID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("THigns went BAD "+ info);
        }

        AreaIndex area = null;

        try
        {
            area = _areas[info.areaX, info.areaY];
        }
        catch (Exception e)
        {
            Debug.LogWarning("THigns went BAD " + e);
            info = GetPixelInformation(x, y);
        }

        switch(id)
        {
            case IntDataID.NoiseLayerData:
                return area.AlphaDataLayer.NoiseLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetInt(int x, int y, int value, IntDataID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case IntDataID.NoiseLayerData:
                area.AlphaDataLayer.NoiseLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }

    public byte GetByte(int x, int y, ByteDataLyerID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("THigns went BAD " + info);
        }

        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case ByteDataLyerID.WaterLayerData:
                return area.AlphaDataLayer.WaterLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetByte(int x, int y, byte value, ByteDataLyerID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case ByteDataLyerID.WaterLayerData:
                area.AlphaDataLayer.WaterLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }
}

public struct TokenRequest
{
    private int _left;
    private int _right;
    private int _bottom;
    private int _top;

    public int left { get { return _left; } }
    public int right { get { return _right; } }
    public int bottom { get { return _bottom; } }
    public int top { get { return _top; } }
    public int width { get { return _right - _left; } }
    public int height { get { return _bottom - top; } }

    public TokenRequest(int left, int right, int bottom, int top)
    {
        _left = left;
        _right = right;
        _bottom = bottom;
        _top = top;
    }
}