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

            LoadAreaJob loadAreaJob = new LoadAreaJob(loadRequests, index, _areaCache, "FILL IN LATER");
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