using UnityEngine;

namespace Terra.Views.ViewDataStreamers
{
    public interface IDataStreamer
    {
        void Start();
        void Update(float time);
        void Stop();
    }
}