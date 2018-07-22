using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Polenter.Serialization;

public class WorldPersistanceService : Service {

    private class WorldIndexRequest : ServiceRequest<WorldIndex>
    {
        private WorldIndexGenerator _indexGenerator;
        private SharpSerializer _serializer;

        public WorldIndexRequest(WorldPersistanceService worldPersistanceService, WorldIndexGenerator indexGenerator) : base(worldPersistanceService)
        {
            _indexGenerator = indexGenerator;
            _serializer = new SharpSerializer();
        }

        protected override IEnumerator MakeRequestCoroutine(Action<WorldIndex> onComplete, Action onError)
        {
            yield return 0;

            WorldIndex worldIndex = null;

            if (File.Exists(_indexGenerator.indexFilePath))
            {
                FileStream fileStream = File.Open(_indexGenerator.indexFilePath, FileMode.Open);

                Debug.Log("Loading World from " + _indexGenerator.indexFilePath);

                using (var stream = fileStream)
                {
                    worldIndex = _serializer.Deserialize(stream) as WorldIndex;
                }

                onComplete(worldIndex);
            }
            else
            {
                onComplete(_indexGenerator.GenerateIndex());
            }
        }
    }

    [SerializeField]
    private WorldIndexGenerator _indexGenerator;

    private WorldIndex _index;
    private Coroutine _loadCoroutine;
    private Coroutine _saveCoroutine;
    private SharpSerializer _serializer;
    private WorldIndexRequest _worldIndexRequest;

    public string areaDataDirectoryPath
    {
        get
        {
            return _indexGenerator.areaDataDirectoryPath;
        }
    }

    public override void StartService(ServiceManager serviceManager)
    {
        _worldIndexRequest = new WorldIndexRequest(this, _indexGenerator);
    }

    public void Load(Action<WorldIndex> onComplete, Action onError)
    {
        _worldIndexRequest.AddRequest(onComplete, onError);
    }

    public override IEnumerator Preload()
    {
        yield return 0;
        _worldIndexRequest.AddRequest((index) => { }, () => { });
    }
}