﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Polenter.Serialization;

public class WorldPersistanceService : Service {

    private class WorldIndexRequest : ServiceRequest<WorldIndex>
    {
        private IWorldIndexGenerator _indexGenerator;
        private string _persistentDataPath;

        public WorldIndexRequest(WorldPersistanceService worldPersistanceService, IWorldIndexGenerator indexGenerator, string persistentDataPath) : base(worldPersistanceService)
        {
            _indexGenerator = indexGenerator;
            _persistentDataPath = persistentDataPath;
        }

        protected override IEnumerator MakeRequestCoroutine(Action<WorldIndex> onComplete, Action onError)
        {
            yield return 0;

            if (_indexGenerator.Exists((_persistentDataPath)))
            {
                onComplete(_indexGenerator.Load(_persistentDataPath));
            }
            else
            {
                onComplete(_indexGenerator.Generate(_persistentDataPath));
            }
        }
    }

    [SerializeField]
    private WorldAsset _indexGenerator;

    private WorldIndex _index;
    private Coroutine _loadCoroutine;
    private Coroutine _saveCoroutine;
    private SharpSerializer _serializer;
    private WorldIndexRequest _worldIndexRequest;

    public WorldAsset IndexGenerator
    {
        get { return _indexGenerator; }
    }

    public override void StartService(ServiceManager serviceManager)
    {
        _worldIndexRequest = new WorldIndexRequest(this, _indexGenerator, Application.persistentDataPath);
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