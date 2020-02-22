using System;
using System.Collections;
using System.Collections.Generic;

namespace PandeaGames.Services
{
    public abstract class ServiceRequest<TData> 
    {
        private TData _cache;

        protected void ClearCache()
        {
            _cache = default(TData);
        }

        private struct Request
        {
            public Action<TData> onComplete;
            public Action onError;
        }

        private List<Request> _requests { get; } = new List<Request>();

        public void AddRequest(Action<TData> onComplete, Action onError)
        {
            if (_cache != null)
            {
                onComplete(_cache);
            }
            else
            {
                TaskProvider.Instance.RunTask(MakeRequestCoroutine(OnRequestCompleted, OnRequestError));
            }
        }

        private void OnRequestError()
        {
            foreach (Request request in _requests)
            {
                request.onError();
            }

            _requests.Clear();
        }

        private void OnRequestCompleted(TData data)
        {
            _cache = data;
            foreach (Request request in _requests)
            {
                request.onComplete(data);
            }

            _requests.Clear();
        }
        
        protected abstract IEnumerator MakeRequestCoroutine(Action<TData> onComplete, Action onError);
    }
}