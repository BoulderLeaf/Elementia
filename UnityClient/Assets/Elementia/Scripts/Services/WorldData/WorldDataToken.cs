using UnityEngine;
using System.Collections.Generic;
using System;

public class WorldDataToken
{
    private struct PixelInformation
    {
        public int areaX;
        public int areaY;
        public int areaPixelX;
        public int areaPixelY;

        public override string ToString()
        {
            return string.Format("[areaX:{0}, areaY:{1}, areaPixelX:{2}, areaPixelY:{3}]", areaX, areaY, areaPixelX, areaPixelY);
        }
    }

    public bool Loaded = false;
    private AreaIndex[,] _areas;
    private LoadedArea[,] _loadedAreas;
    private Dictionary<AreaIndex, string> _filepaths;
    private WorldIndex _index;
    private TokenRequest _request;

    public TokenRequest Request { get { return _request; } }
    public WorldIndex WorldIndex { get { return _index; } }
    
    public AreaIndex[,] Areas
    {
        get
        {
            return _areas;
        }
        set { _areas = value; }
    }

    public WorldDataToken(TokenRequest request, WorldIndex index, LoadedArea[,] loadedAreas)
    {
        _areas = new AreaIndex[loadedAreas.GetLength(0),loadedAreas.GetLength(1)];

        for (int i=0; i< loadedAreas.GetLength(0); i++)
        {
            for (int j=0; j < loadedAreas.GetLength(1); j++)
            {
                _areas[i, j] = loadedAreas[i, j].Result.Result;
            }
        }
        
        _request = request;
        _index = index;
        _loadedAreas = loadedAreas;
    }

    private PixelInformation GetPixelInformation(int x, int y)
    {
        PixelInformation info = default(PixelInformation);

        int areaDim = _index.AreaDimensions;
        int trueAreaX = (int)((_request.left + x) / areaDim);
        int requestedAreaX = (int)(_request.left / areaDim);
        int trueAreaY = (int)((_request.top + y) / areaDim);
        int requestedAreaY = (int)(_request.top / areaDim);

        info.areaX = trueAreaX - requestedAreaX;
        info.areaY = trueAreaY - requestedAreaY;
        info.areaPixelX = (_request.left + x) % areaDim;
        info.areaPixelY = (_request.top + y) % areaDim;

        return info;
    }

    private bool AreCoordinatesInvalid(int x, int y)
    {
        return x < 0 || x > _request.width || y < 0 || y > _request.height;
    }

    private bool IsPixelInformationInvalid(PixelInformation info)
    {
        return info.areaX >= _areas.Length || info.areaY >= _areas.GetLongLength(1);
    }

    public TerraVector ToLocal(TerraVector worldVector)
    {
        return new TerraVector() { x = worldVector.x - _request.left, y = worldVector.y - _request.top };
    }

    public ushort GetUshort(int x, int y, UshortDataID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("Thigns went BAD " + info);
        }

        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case UshortDataID.HeightLayerData:
                return area.DataLayer.HeightLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetUshort(int x, int y, ushort value, UshortDataID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case UshortDataID.HeightLayerData:
                area.DataLayer.HeightLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }

    public int GetInt(int x, int y, IntDataID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("THigns went BAD "+ info);
        }

        AreaIndex area = null;

        try
        {
            area = _areas[info.areaX, info.areaY];
        }
        catch (Exception e)
        {
            Debug.LogWarning("THigns went BAD " + e);
            info = GetPixelInformation(x, y);
        }

        switch(id)
        {
            case IntDataID.NoiseLayerData:
                return area.DataLayer.NoiseLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetInt(int x, int y, int value, IntDataID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case IntDataID.NoiseLayerData:
                area.DataLayer.NoiseLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }

    public byte GetByte(int x, int y, ByteDataLyerID id)
    {
        if (AreCoordinatesInvalid(x, y))
        {
            return 1;
        }

        PixelInformation info = GetPixelInformation(x, y);

        if (IsPixelInformationInvalid(info))
        {
            Debug.LogWarning("THigns went BAD " + info);
        }

        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case ByteDataLyerID.WaterLayerData:
                return area.DataLayer.WaterLayerData.data[info.areaPixelX, info.areaPixelY];
        }

        return 0;
    }

    public void SetByte(int x, int y, byte value, ByteDataLyerID id)
    {
        PixelInformation info = GetPixelInformation(x, y);
        AreaIndex area = _areas[info.areaX, info.areaY];

        switch (id)
        {
            case ByteDataLyerID.WaterLayerData:
                area.DataLayer.WaterLayerData.data[info.areaPixelX, info.areaPixelY] = value;
                break;
        }
    }
}