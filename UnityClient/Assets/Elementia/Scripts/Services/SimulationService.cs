using UnityEngine;
using System.Collections;
using System.IO;
using Polenter.Serialization;
using System;

public class SimulationService : Service
{
    private WorldDataAccessService _worldDataAccessService;
    private WorldSimulationState _state;
    private SharpSerializer _serializer;
    private WorldSimulationStateService _worldSimulationStateService;

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);
        _serializer = new SharpSerializer();
        _worldSimulationStateService = serviceManager.GetService<WorldSimulationStateService>();
        _worldDataAccessService = serviceManager.GetService<WorldDataAccessService>();
        _worldSimulationStateService.Load(OnSimulationStateLoaded, () => { });
    }

    private void OnSimulationStateLoaded(WorldSimulationState simulationState)
    {
        _state = simulationState;
        StartCoroutine(SimulationCoroutine());
    }

    private IEnumerator SimulationCoroutine()
    {
        yield return 0;

        bool simulating = true;

        while(simulating)
        {
            uint totalDevisions = _state.SimulationDevisions;
            uint stepsCompleted = 0;
            Debug.Log("Step Simulation" + _state.SimulationStep);

            for (uint i = 0; i<totalDevisions;i++)
            {
                StartCoroutine(SimulateStep(i, totalDevisions, () => stepsCompleted++));
            }

            yield return new WaitUntil(() => stepsCompleted >= totalDevisions);

            _state.StepSimulationState();
            _worldSimulationStateService.SaveState();
        }
    }
    int tokenRequests = 0;
    private IEnumerator SimulateStep(uint offset, uint totalDevisions,Action onComplete)
    {
        SimulationArea simulationArea = _state.GetCurrentSimulationArea(offset, totalDevisions);
        TokenRequest tokenRequest = new TokenRequest(simulationArea.Left, simulationArea.Right, simulationArea.Bottom, simulationArea.Top);
        WorldDataToken token = null;
        int myTokenRequest = tokenRequests++;
        Debug.Log("Step 1`: loading "+ (myTokenRequest));
        _worldDataAccessService.GetToken(tokenRequest, (recievedToken) => {
            token = recievedToken;
        }, () => {
            Debug.LogError("There was an error attempthing to recieve a data token while simulating a step.");
        });
        DateTime startWorldIndexLoad = DateTime.UtcNow;
        yield return new WaitUntil(() => {
            return token != null || (DateTime.UtcNow - startWorldIndexLoad).TotalMilliseconds > 10000;
        });
        Debug.Log("Step 2");
        if ((DateTime.UtcNow - startWorldIndexLoad).Milliseconds > 10000)
        {
            Debug.Log("TIMEOUT ERROR LOADING WORLD INDEX");
        }
        //SimulateAreaJob simulateAreaJob = new SimulateAreaJob(token, _state);
        //simulateAreaJob.Start();

        //yield return new WaitUntil(() => simulateAreaJob.IsDone);

        //bool tokenSaved = false;
        //_worldDataAccessService.SaveAndReturnToken(token, () => tokenSaved = true);
        //yield return new WaitUntil(() => tokenSaved == true);
        Debug.Log("Step 3: Done loading: "+ myTokenRequest);
        onComplete();
    }
}

public class SimulateAreaJob:ThreadedJob
{
    private WorldDataToken _token;
    private WorldSimulationState _state;

    public SimulateAreaJob(WorldDataToken token, WorldSimulationState state)
    {
        _token = token;
        _state = state;
    }

    protected override void ThreadFunction()
    {
        for (int x = _state.Radius; x < _token.Request.width - _state.Radius; x++)
        {
            for (int y = _state.Radius; y < _token.Request.height - _state.Radius; y++)
            {
                SimulateAreaWithRadius(_token, x, y);
            }
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