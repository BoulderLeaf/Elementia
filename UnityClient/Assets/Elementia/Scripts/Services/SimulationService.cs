using UnityEngine;
using System.Collections;
using System.IO;
using Polenter.Serialization;
using System;
using System.Threading;
using System.Collections.Generic;
using PandeaGames;
using UnityEditor;

public class SimulationService : Service
{
    private WorldDataAccessService _worldDataAccessService;
    private WorldSimulationState _state;
    private SharpSerializer _serializer;
    private WorldSimulationStateService _worldSimulationStateService;
    private WorldDataAccess _worldDataAccess;
    private SimulationJob _job;

    public SimulationService() : base()
    {
        _serializer = new SharpSerializer();
        _worldSimulationStateService = Game.Instance.GetService<WorldSimulationStateService>();
        _worldDataAccessService =  Game.Instance.GetService<WorldDataAccessService>();
        _worldSimulationStateService.Load(OnSimulationStateLoaded, () => { });
    }
    
    public override void StartService(ServiceManager serviceManager)
    {
        
    }

    private void OnSimulationStateLoaded(WorldSimulationState simulationState)
    {
        _state = simulationState;
        _worldDataAccessService.RequestAccess(OnDataAccessRequested, ()=>{
        });
    }
    
    private void OnDataAccessRequested(WorldDataAccess worldDataAccess)
    {
        _worldDataAccess = worldDataAccess;
        StartCoroutine(SimulationCoroutine());
    }

    private IEnumerator SimulationCoroutine()
    {
        yield return 0;
        
        bool simulating = true;
        _job = new SimulationJob(_worldDataAccess, _state, _worldSimulationStateService, Application.persistentDataPath);
        _job.Start();
        
        while(!_job.IsDone)
        {
            yield return 0;
        }
    }

    private void OnDestroy()
    {
        if (_job != null)
        {
            _job.Abort();
            _job = null;
        }
    }
}

public class SimulationJob : ThreadedJob
{
    private WorldSimulationStateService _worldSimulationStateService;
    private WorldSimulationState _worldSimulationState;
    private WorldDataAccess _worldDataAccess;
    private string _persistentDataPath;

    public SimulationJob(WorldDataAccess worldDataAccess, WorldSimulationState worldSimulationState, WorldSimulationStateService worldSimulationStateService, string persistentDataPath)
    {
        _worldSimulationState = worldSimulationState;
        _worldSimulationStateService = worldSimulationStateService;
        _worldDataAccess = worldDataAccess;
        _persistentDataPath = persistentDataPath;
    }

    protected override void ThreadFunction()
    {
        try
        {
            bool running = true;
    
            List<SimulateAreaJob> jobs = new List<SimulateAreaJob>();

            jobs.Clear();
            
            uint totalDevisions = _worldSimulationState.SimulationDevisions;
            uint stepsCompleted = 0;

            for (uint i = 0; i<totalDevisions;i++)
            {
                jobs.Add(SimulateStep(i, totalDevisions, () =>
                {
                    if (++stepsCompleted >= jobs.Count)
                    {
                        //TODO SImulate again
                        _worldSimulationState.StepSimulationState();
                        _worldSimulationStateService.SaveState(_worldSimulationState, _persistentDataPath);
                        
                        SimulationJob _job = new SimulationJob(_worldDataAccess, _worldSimulationState, _worldSimulationStateService, _persistentDataPath);
                        _job.Start();
                    }
                }));
            }
            
        }
        catch(Exception e)
        {
            Debug.LogError("There was an error running SimulationJob \n"+e);
        }
    }
    
    private SimulateAreaJob SimulateStep(uint offset, uint totalDevisions, Action onComplete)
    {
        SimulationArea simulationArea = _worldSimulationState.GetCurrentSimulationArea(offset, totalDevisions);
        TokenRequest tokenRequest = new TokenRequest(simulationArea.Left, simulationArea.Right, simulationArea.Bottom, simulationArea.Top);
        SimulateAreaJob simulateAreaJob = new SimulateAreaJob(_worldDataAccess, tokenRequest, _worldSimulationState, _persistentDataPath, onComplete);
        simulateAreaJob.Start();
        return simulateAreaJob;
    }
}

public class SimulateAreaJob:ThreadedJob
{
    private TokenRequest _tokenRequest;
    private WorldSimulationState _state;
    private WorldDataAccess _worldDataAccess;
    private string _persistentDataPath;
    private Action _onComplete;
    
    public SimulateAreaJob(WorldDataAccess worldDataAccess, TokenRequest tokenRequest, WorldSimulationState state, string persistentDataPath, Action onComplete)
    {
        _tokenRequest = tokenRequest;
        _state = state;
        _worldDataAccess = worldDataAccess;
        _persistentDataPath = persistentDataPath;
        _onComplete = onComplete;
    }

    protected override void ThreadFunction()
    {
        try
        {
            _worldDataAccess.GetToken(_tokenRequest, _persistentDataPath, (dataToken) =>
            {
                for (int x = _state.Radius; x < dataToken.Request.width - _state.Radius; x++)
                {
                    for (int y = _state.Radius; y < dataToken.Request.height - _state.Radius; y++)
                    {
                        SimulateAreaWithRadius(dataToken, x, y);
                    }
                }

                //_worldDataAccess.SaveToken(_tokenRequest);
                _onComplete();
            });
        }
        catch (Exception e)
        {
            Debug.LogError("SimulateAreaJob Error: \n"+e);
            throw;
        }
    }
    
     

    private void SimulateAreaWithRadius(WorldDataToken token, int x, int y)
    {
        //test for frame rate

        byte[,] waterValues = new byte[3, 3];
        ushort[,] depthValues = new ushort[3, 3];

        for (int i = x - 1; i < x + 1; i++)
        {
            for (int j = y - 1; j < y + 1; j++)
            {
                int valuesX = i - (x - 1);
                int valuesY = j - (y - 1);

                waterValues[valuesX, valuesY] = token.GetByte(i, j, ByteDataLyerID.WaterLayerData);
                depthValues[valuesX, valuesY] = token.GetUshort(i, j, UshortDataID.HeightLayerData);
            }
        }

        byte baseWaterValue = waterValues[1, 1];
        ushort baseDepthValue = depthValues[1, 1];

        ApplyWaterValues(waterValues, depthValues, 0, 0);
        ApplyWaterValues(waterValues, depthValues, 1, 0);
        ApplyWaterValues(waterValues, depthValues, 2, 0);
        ApplyWaterValues(waterValues, depthValues, 2, 1);
        ApplyWaterValues(waterValues, depthValues, 2, 2);
        ApplyWaterValues(waterValues, depthValues, 1, 2);
        ApplyWaterValues(waterValues, depthValues, 0, 2);
        ApplyWaterValues(waterValues, depthValues, 0, 1);

        for (int i = x - 1; i < x + 1; i++)
        {
            for (int j = y - 1; j < y + 1; j++)
            {
                int valuesX = i - (x - 1);
                int valuesY = j - (y - 1);

                token.SetByte(i, j, waterValues[valuesX, valuesY], ByteDataLyerID.WaterLayerData);
            }
        }
    }

    private void ApplyWaterValues(byte[,] waterValues, ushort[,] depthValues, int otherX, int otherY)
    {
        byte waterValue = waterValues[1, 1];
        byte otherWaterValue = waterValues[otherX, otherY];

        ushort depthValue = depthValues[1, 1];
        ushort otherDepthValue = depthValues[1, 1];

        if (depthValue == otherDepthValue)
        {
            byte avg = (byte)((waterValue + otherWaterValue) / 2);
            waterValues[1, 1] = avg;
            waterValues[otherX, otherY] = avg;
        }
        else if (depthValue < otherDepthValue)
        {
            waterValues[1, 1] += otherWaterValue;
            waterValues[otherX, otherY] = 0;
        }
        else
        {
            waterValues[otherX, otherY] += waterValue;
            waterValues[1, 1] = 0;
        }
    }
}