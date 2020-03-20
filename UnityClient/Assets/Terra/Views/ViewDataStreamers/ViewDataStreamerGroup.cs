using System.Collections.Generic;
using UnityEngine;

namespace Terra.Views.ViewDataStreamers
{
    public class ViewDataStreamerGroup : IDataStreamer
    {
        private IDataStreamer[] _dataStreamers;
        
        public ViewDataStreamerGroup(IDataStreamer[] dataStreamers)
        {
            _dataStreamers = dataStreamers;
        }

        public void Start()
        {
            foreach (IDataStreamer dataStreamer in _dataStreamers)
            {
                dataStreamer.Start();
            }
        }

        public void Update(float time)
        {
            foreach (IDataStreamer dataStreamer in _dataStreamers)
            {
                dataStreamer.Update(time);
            }
        }

        public void Stop()
        {
            foreach (IDataStreamer dataStreamer in _dataStreamers)
            {
                dataStreamer.Stop();
            }
        }
    }
}