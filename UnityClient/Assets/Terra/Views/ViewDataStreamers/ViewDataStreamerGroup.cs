using System.Collections.Generic;

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

        public void Stop()
        {
            foreach (IDataStreamer dataStreamer in _dataStreamers)
            {
                dataStreamer.Stop();
            }
        }
    }
}