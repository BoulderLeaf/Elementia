using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class AreaIndex
{
    public DataLayer AlphaDataLayer { get; set; }
    public DataLayer BetaDataLayer { get; set; }
    public List<Occupant> Occupant { get; set; }

    public AreaIndex()
    {
        AlphaDataLayer = new DataLayer();
        BetaDataLayer = new DataLayer();
        Occupant = new List<Occupant>();
    }

    public void Destroy()
    {
        AlphaDataLayer.Destroy();
        BetaDataLayer.Destroy();

        if(Occupant != null)
            Occupant.Clear();

        Occupant = null;
        AlphaDataLayer = null;
        BetaDataLayer = null;
    }
}

public enum IntDataID
{
    NoiseLayerData
}

public enum UshortDataID
{
    HeightLayerData
}

public enum ByteDataLyerID
{
    WaterLayerData
}

[Serializable]
public class DataLayer
{
    public IntDataLater NoiseLayerData { get; set; }
    public UshortDataLater HeightLayerData { get; set; }
    public ByteDataLater WaterLayerData { get; set; }

    public DataLayer()
    {
        NoiseLayerData = new IntDataLater();
        HeightLayerData = new UshortDataLater();
        WaterLayerData = new ByteDataLater();
    }

    public DataLayer Clone()
    {
        DataLayer data = new DataLayer();

        data.NoiseLayerData = NoiseLayerData.Clone();
        data.WaterLayerData = WaterLayerData.Clone();
        data.HeightLayerData = HeightLayerData.Clone();

        return data;
    }

    public void Destroy()
    {
        NoiseLayerData = null;
        HeightLayerData = null;
        WaterLayerData = null;
    }
}

[Serializable]
public class Occupant
{

}

[Serializable]
public class IntDataLater
{
    public int[,] data { get; set; }

    public IntDataLater Clone()
    {
        IntDataLater clonedData = new IntDataLater();
        clonedData.data = (int[,])data.Clone();
        return clonedData;
    }
}

[Serializable]
public class UshortDataLater
{
    public ushort[,] data { get; set; }

    public UshortDataLater Clone()
    {
        UshortDataLater clonedData = new UshortDataLater();
        clonedData.data = (ushort[,])data.Clone();
        return clonedData;
    }
}

[Serializable]
public class ByteDataLater
{
    public byte[,] data { get; set; }

    public ByteDataLater Clone()
    {
        ByteDataLater clonedData = new ByteDataLater();
        clonedData.data = (byte[,])data.Clone();
        return clonedData;
    }
}