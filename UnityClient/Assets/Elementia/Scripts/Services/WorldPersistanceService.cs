using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using PandeaGames.Services;
using Polenter.Serialization;
using UnityEditor;

public class WorldPersistanceService : AbstractService<WorldPersistanceService> {

    private class WorldIndexRequest : ServiceRequest<WorldIndex>
    {
        private IWorldIndexGenerator _indexGenerator;
        private string _persistentDataPath;

        public WorldIndexRequest(WorldPersistanceService worldPersistanceService, IWorldIndexGenerator indexGenerator, string persistentDataPath) : base()
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
    
    private WorldAsset _indexGenerator;

    private WorldIndex _index;
    private Coroutine _loadCoroutine;
    private Coroutine _saveCoroutine;
    private SharpSerializer _serializer;
    private WorldIndexRequest _worldIndexRequest { get; }

    public WorldAsset IndexGenerator
    {
        get
        {
            return null;
            //return AssetDatabase.LoadAssetAtPath<WorldAsset>("Assets/Elementia/Config/MainWorld.asset");
            return _indexGenerator;
        }
    }

    public WorldPersistanceService()
    {
        _worldIndexRequest = new WorldIndexRequest(this, IndexGenerator, Application.persistentDataPath);
    }

    public void Load(Action<WorldIndex> onComplete, Action onError)
    {
        _worldIndexRequest.AddRequest(onComplete, onError);
    }
}