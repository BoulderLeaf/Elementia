using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldIndex
{
    private IWorldIndexGenerator _generator;

    public void SetGenerator(IWorldIndexGenerator generator)
    {
        _generator = generator;
    }

    public IWorldIndexGenerator GetGenerator()
    {
        return _generator;
    }
    
    public string Version { get; set; }
    public int Width { get { return Dimensions.Width; } }
    public int Height { get { return Dimensions.Height; } }
    public WorldDimensions Dimensions { get; set; }
    public int AreaDimensions { get; set; }
    public string FileDataExtension { get; set; }
    public string AreaFilenameFormatSource { get; set; }
    public string AreaRelativeDirectory { get; set; }
    public SerializationType SerializationType { get; set; }
}