using UnityEngine;
using System.Collections;
using Polenter.Serialization;
using System;
using System.IO;

public class WorldSimulationStateService : Service
{
    private class WorldSimulateStateRequest : ServiceRequest<WorldSimulationState>
    {
        private WorldIndexGenerator _indexGenerator;
        private SharpSerializer _serializer;
        private WorldPersistanceService _worldPersistanceService;
        private SimulationConfiguration _simulationConfiguration;

        public WorldSimulateStateRequest(WorldSimulationStateService worldSimulationStateService, WorldPersistanceService worldPersistanceService, SimulationConfiguration simulationConfiguration, WorldIndexGenerator indexGenerator) : base(worldSimulationStateService)
        {
            _serializer = new SharpSerializer();
            _worldPersistanceService = worldPersistanceService;
            _simulationConfiguration = simulationConfiguration;
            _indexGenerator = indexGenerator;
        }

        protected override IEnumerator MakeRequestCoroutine(Action<WorldSimulationState> onComplete, Action onError)
        {
            WorldSimulationState state = null;
            WorldIndex worldIndex = null;

            _worldPersistanceService.Load((index) => worldIndex = index, () => { });

            yield return new WaitUntil(() => worldIndex != null);

            string filepath = _indexGenerator.dataRootDirectory + SimulationConfiguration.IndexFilename;

            if (File.Exists(filepath))
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open);

                Debug.Log("Loading Simulation State from " + filepath);

                using (var stream = fileStream)
                {
                    state = _serializer.Deserialize(stream) as WorldSimulationState;
                }
            }
            else
            {
                FileStream fileStream = File.Open(filepath, FileMode.OpenOrCreate);

                Debug.Log("Generating new simulation state at " + filepath);

                state = _simulationConfiguration.GenerateSimulationState(worldIndex);

                using (var stream = fileStream)
                {
                    _serializer.Serialize(state, fileStream);
                }
            }

            onComplete(state);
        }
    }

    private class WorldSimulateSaveRequest : ServiceRequest<WorldSimulationState>
    {
        private WorldIndexGenerator _indexGenerator;
        private SharpSerializer _serializer;
        private WorldSimulateStateRequest _getRequest;

        public WorldSimulateSaveRequest(WorldSimulationStateService worldSimulationStateService, WorldSimulateStateRequest getRequest, WorldIndexGenerator indexGenerator) : base(worldSimulationStateService)
        {
            _serializer = new SharpSerializer();
            _indexGenerator = indexGenerator;
            _getRequest = getRequest;
        }

        protected override IEnumerator MakeRequestCoroutine(Action<WorldSimulationState> onComplete, Action onError)
        {
            yield return 0;

            _getRequest.AddRequest((worldSimulationState) =>
            {
                string filepath = _indexGenerator.dataRootDirectory + SimulationConfiguration.IndexFilename;
                FileStream fileStream = File.Open(filepath, FileMode.OpenOrCreate);

                using (var stream = fileStream)
                {
                    _serializer.Serialize(worldSimulationState, fileStream);
                }

                onComplete(worldSimulationState);

                ClearCache();

            }, () => { });
        }
    }

    [SerializeField]
    private SimulationConfiguration _simulationConfiguration;

    [SerializeField]
    private WorldIndexGenerator _indexGenerator;

    private WorldSimulateStateRequest _worldSimulateStateRequest;
    private WorldSimulateSaveRequest _worldSimulateSaveRequest;
    private WorldPersistanceService _worldPersistanceService;

    public override void StartService(ServiceManager serviceManager)
    {
        _worldPersistanceService = serviceManager.GetService<WorldPersistanceService>();
        _worldSimulateStateRequest = new WorldSimulateStateRequest(this, _worldPersistanceService, _simulationConfiguration, _indexGenerator);
        _worldSimulateSaveRequest = new WorldSimulateSaveRequest(this, _worldSimulateStateRequest, _indexGenerator);
    }

    public void Load(Action<WorldSimulationState> onComplete, Action onError)
    {
        _worldSimulateStateRequest.AddRequest(onComplete, onError);
    }

    public override IEnumerator Preload()
    {
        yield return 0;
        _worldSimulateStateRequest.AddRequest((index) => { }, () => { });
    }

    public void SaveState()
    {
        _worldSimulateSaveRequest.AddRequest((worldSimulationState) => {}, () => { });
    }
}
