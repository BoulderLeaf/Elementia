using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ServiceRequest<T>
{
    private T _cache;
    private List<Action<T>> _onCompleteListeners;
    private List<Action> _onErrorListeners;
    private Coroutine _requestCoroutine;
    private Service _service;

    public ServiceRequest(Service service)
    {
        _onCompleteListeners = new List<Action<T>>();
        _onErrorListeners = new List<Action>();
        _service = service;
    }

    public void ClearCache()
    {
        _cache = default(T);
    }

    public void AddRequest(Action<T> onComplete, Action onError)
    {
        _onCompleteListeners.Add(onComplete);
        _onErrorListeners.Add(onError);

        if (_requestCoroutine == null)
        {
            BeginRequest();
        }
    }

    private void ClearRequest()
    {
        _onCompleteListeners.Clear();
        _onErrorListeners.Clear();
        _requestCoroutine = null;
    }

    private void CompleteRequest(T data)
    {
        _cache = data;

        foreach (Action<T> handler in _onCompleteListeners)
        {
            handler(data);
        }

        ClearRequest();
    }

    private void CompleteRequest(string error)
    {
        foreach (Action handler in _onErrorListeners)
        {
            handler();
        }

        ClearRequest();
    }

    private void BeginRequest()
    {
        _requestCoroutine = _service.StartCoroutine(BeginRequestCoroutine());
    }

    private IEnumerator BeginRequestCoroutine()
    {
        yield return 0;

        if (_cache != null)
        {
            CompleteRequest(_cache);
        }
        else
        {
            yield return MakeRequestCoroutine((data) =>
            {
                CompleteRequest(data);
            }, () =>
            {
                CompleteRequest("error");
            });
        }
    }

    protected abstract IEnumerator MakeRequestCoroutine(Action<T> onComplete, Action onError);
}

[Serializable]
public abstract class Service : MonoBehaviour {

    public delegate void serviceStatusDelegate();

    public event serviceStatusDelegate OnServiceStart;
    public event serviceStatusDelegate OnServiceEnd;

    [SerializeField]
    private bool _isRunning;

    public bool IsRunning { get { return _isRunning; } }

    public virtual void StartService(ServiceManager serviceManager)
    {
        _isRunning = true;

        if (OnServiceStart != null)
        {
            OnServiceStart();
        }
    }

    public virtual void EndService(ServiceManager serviceManager)
    {
        _isRunning = false;

        if (OnServiceEnd != null)
        {
            OnServiceEnd();
        }
    }

    public virtual IEnumerator Preload()
    {
        yield break;
    }
}
