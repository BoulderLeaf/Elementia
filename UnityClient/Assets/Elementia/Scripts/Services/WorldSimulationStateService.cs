using UnityEngine;
using System.Collections;
using Polenter.Serialization;
using System;
using System.IO;
using PandeaGames;
using PandeaGames.Services;

public class WorldSimulationStateService : AbstractService<WorldSimulationStateService>
{
    private class WorldSimulateStateRequest : ServiceRequest<WorldSimulationState>
    {
        private WorldAsset _indexGenerator;
        private SharpSerializer _serializer;
        private WorldPersistanceService _worldPersistanceService;
        private SimulationConfiguration _simulationConfiguration;

        public WorldSimulateStateRequest(
            WorldSimulationStateService worldSimulationStateService, 
            WorldPersistanceService worldPersistanceService, 
            SimulationConfiguration simulationConfiguration, 
            WorldAsset indexGenerator) : base()
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

            string filepath = String.Join(DataConfig.DirectoryDelimiter, _indexGenerator.RootPath(Application.persistentDataPath), SimulationConfiguration.IndexFilename);

            var request = new LoadSimulationStateJob.LoadSimulationStateJobRequest(filepath);
            LoadSimulationStateJob loadJob = new LoadSimulationStateJob(request);
            loadJob.Start();

            yield return new WaitUntil(() => loadJob.IsDone);

            if (loadJob.Output == null)
            {
                state = _simulationConfiguration.GenerateSimulationState(worldIndex);
            }
            else
            {
                state = loadJob.Output;
            }

            onComplete(state);
        }
    }

    private class WorldSimulateSaveRequest : ServiceRequest<WorldSimulationState>
    {
        private WorldAsset _indexGenerator;
        private SharpSerializer _serializer;
        private WorldSimulateStateRequest _getRequest;

        public WorldSimulateSaveRequest(WorldSimulationStateService worldSimulationStateService, WorldSimulateStateRequest getRequest, WorldAsset indexGenerator) : base()
        {
            _serializer = new SharpSerializer();
            _indexGenerator = indexGenerator;
            _getRequest = getRequest;
        }

        protected override IEnumerator MakeRequestCoroutine(Action<WorldSimulationState> onComplete, Action onError)
        {
            yield return 0;

            WorldSimulationState state = null;

            _getRequest.AddRequest((worldSimulationState) =>
            {
                state = worldSimulationState;
            }, () => { });

            yield return new WaitUntil(() => state != null);

            string filepath = String.Join(DataConfig.DirectoryDelimiter, _indexGenerator.RootPath(Application.persistentDataPath), SimulationConfiguration.IndexFilename);

            var request = new SaveSimulationStateJob.SaveSimulationStateJobRequest(filepath, state);
            SaveSimulationStateJob savejob = new SaveSimulationStateJob(request);
            savejob.Start();

            yield return new WaitUntil(() => savejob.IsDone);

            onComplete(state);

            ClearCache();
        }
    }

    [SerializeField]
    private SimulationConfiguration _simulationConfiguration;

    private WorldSimulateStateRequest _worldSimulateStateRequest;
    private WorldSimulateSaveRequest _worldSimulateSaveRequest;
    private WorldPersistanceService _worldPersistanceService;

    public WorldSimulationStateService()
    {
        _worldPersistanceService = Game.Instance.GetService<WorldPersistanceService>();
        _worldSimulateStateRequest = new WorldSimulateStateRequest(
            this, 
            _worldPersistanceService, _simulationConfiguration, _worldPersistanceService.IndexGenerator);
        _worldSimulateSaveRequest = new WorldSimulateSaveRequest(this, _worldSimulateStateRequest, _worldPersistanceService.IndexGenerator);
    }

    public void Load(Action<WorldSimulationState> onComplete, Action onError)
    {
        _worldSimulateStateRequest.AddRequest(onComplete, onError);
    }

    public void SaveState(WorldSimulationState state, string persistentDataPath)
    {
        SharpSerializer serializer = new SharpSerializer();

        string filePath = String.Join(DataConfig.DirectoryDelimiter, new string[]
        {
            _worldPersistanceService.IndexGenerator.RootPath(persistentDataPath),
            SimulationConfiguration.IndexFilename
        });
        
        FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate);

        using (var stream = fileStream)
        {
            serializer.Serialize(state, fileStream);
        }
    }

    private class LoadSimulationStateJob : ThreadedJob
    {
        public class LoadSimulationStateJobRequest
        {
            private string _filepath;
            public string Filepath { get
                {
                    return _filepath;
                } }
            public LoadSimulationStateJobRequest(string filepath)
            {
                _filepath = filepath;
            }
        }

        public WorldSimulationState Output;
        private LoadSimulationStateJobRequest _request;
        private SharpSerializer _serializer;

        public LoadSimulationStateJob(LoadSimulationStateJobRequest request)
        {
            _request = request;
            _serializer = new SharpSerializer();
        }

        protected override void ThreadFunction()
        {
            string filepath = _request.Filepath;
            if (File.Exists(filepath))
            {
                FileStream fileStream = File.Open(filepath, FileMode.Open);

                using (var stream = fileStream)
                {
                    Output = _serializer.Deserialize(stream) as WorldSimulationState;
                }
            }
        }
    }

    private class SaveSimulationStateJob : ThreadedJob
    {
        public class SaveSimulationStateJobRequest
        {
            private WorldSimulationState _state;
            public WorldSimulationState State
            {
                get
                {
                    return _state;
                }
            }
            private string _filepath;
            public string Filepath
            {
                get
                {
                    return _filepath;
                }
            }
            public SaveSimulationStateJobRequest(string filepath, WorldSimulationState state)
            {
                _filepath = filepath;
                _state = state;
            }
        }

        public WorldSimulationState Output;
        private SaveSimulationStateJobRequest _request;
        private SharpSerializer _serializer;

        public SaveSimulationStateJob(SaveSimulationStateJobRequest request)
        {
            _request = request;
            _serializer = new SharpSerializer();
        }

        protected override void ThreadFunction()
        {
            FileStream fileStream = File.Open(_request.Filepath, FileMode.OpenOrCreate);

            using (var stream = fileStream)
            {
                _serializer.Serialize(_request.State, fileStream);
            }
        }
    }
}


