using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Polenter.Serialization;

public class SaveAreaJob : ThreadedJob
{
    public class SaveAreaRequest
    {
        public AreaIndex area;
        public string filename;
        public SaveAreaRequest(AreaIndex area, string filename)
        {
            this.area = area;
            this.filename = filename;
        }
    }

    private List<SaveAreaRequest> _requests;
    private WorldIndex _worldIndex;
    public SaveAreaJob(List<SaveAreaRequest> requests, WorldIndex worldIndex)
    {
        _requests = requests;
        _worldIndex = worldIndex;
    }

    protected override void ThreadFunction()
    {
        foreach (SaveAreaRequest saveAreaRequest in _requests)
        {
            FileStream areaFileStream = File.Open(saveAreaRequest.filename, FileMode.Create);

            switch (_worldIndex.SerializationType)
            {
                case SerializationType.Binary:
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(areaFileStream, saveAreaRequest.area);
                    break;
                case SerializationType.SharpSerializer:
                    SharpSerializer serializer = new SharpSerializer();
                    using (var stream = areaFileStream)
                    {
                        serializer.Serialize(saveAreaRequest.area, areaFileStream);
                    }
                    break;
            }

            areaFileStream.Close();
        }
    }
}