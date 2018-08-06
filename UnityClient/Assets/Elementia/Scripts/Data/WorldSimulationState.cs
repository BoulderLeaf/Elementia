using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSimulationState {

    public WorldDimensions Dimensions { get; set; }
    public WorldDimensions WorldDimensions { get; set; }
    public int Radius { get; set; }
    public uint SimulationDevisions { get; set; }
    public uint SimulationStep { get; set; }

    public struct StepInformation
    {
        public uint totalStepsForWorld;
        public int worldSteps;
        public int minorSteps;
        public int simulationPosition;
    }

    private StepInformation GetStepInformation()
    {
        return GetStepInformation(0, 0);
    }

    private StepInformation GetStepInformation(uint offset, uint totalDevisions)
    {
        StepInformation result = default(StepInformation);

        result.totalStepsForWorld = (uint)((WorldDimensions.Width / Dimensions.Width) * (WorldDimensions.Height / Dimensions.Height));

        uint devidedStepsForWorld = result.totalStepsForWorld / totalDevisions;
        uint offsetSteps = devidedStepsForWorld * offset;

        uint simulationStepWithOffset = SimulationStep + offsetSteps;

        result.worldSteps = (int)(simulationStepWithOffset / result.totalStepsForWorld);
        result.minorSteps = (int)(result.worldSteps * result.totalStepsForWorld);
        result.simulationPosition = (int)simulationStepWithOffset - result.minorSteps;

        return result;
    }

    public SimulationArea GetCurrentSimulationArea()
    {
        return GetCurrentSimulationArea(0, 1);
    }

    public SimulationArea GetCurrentSimulationArea(uint offset, uint totalDevisions)
    {
        StepInformation stepInformation = GetStepInformation(offset, totalDevisions);

        int columnCount = WorldDimensions.Width / Dimensions.Width;
        int rowCount = WorldDimensions.Height / Dimensions.Height;

        WorldPosition position = new WorldPosition();

        position.X = (stepInformation.simulationPosition - ((int)(stepInformation.simulationPosition / columnCount) * columnCount)) * Dimensions.Width;
        position.Y = (stepInformation.simulationPosition / columnCount) * Dimensions.Height;

        WorldDimensions dimensions = new WorldDimensions();

        dimensions.Width = Math.Min(WorldDimensions.Width - position.X, Dimensions.Width);
        dimensions.Height = Math.Min(WorldDimensions.Height - position.Y, Dimensions.Height);

        position.X -= Radius;
        position.Y -= Radius;
        dimensions.Width += Radius * 2;
        dimensions.Height += Radius * 2;

        if (position.X < 0)
        {
            int diff = position.X * -1;
            position.X += diff;
            dimensions.Width -= diff;
        }

        if (position.X + dimensions.Width > WorldDimensions.Width)
        {
            int diff = (position.X + dimensions.Width) - WorldDimensions.Width;
            dimensions.Width -= diff;
        }

        if (position.Y < 0)
        {
            int diff = position.Y * -1;
            position.Y += diff;
            dimensions.Height -= diff;
        }

        if (position.Y + dimensions.Height > WorldDimensions.Height)
        {
            int diff = (position.Y + dimensions.Height) - WorldDimensions.Height;
            dimensions.Height -= diff;
        }

        return new SimulationArea(dimensions, position, stepInformation.simulationPosition);
    }

    private DateTime _lastStep = DateTime.UtcNow;
    private uint _total = uint.MinValue;
    private uint _steps = 1;
    public void StepSimulationState()
    {
        TimeSpan span = DateTime.UtcNow - _lastStep;
        _total += (uint)span.Milliseconds;
        uint totalStepsForWorld = (uint)((WorldDimensions.Width / Dimensions.Width) * (WorldDimensions.Height / Dimensions.Height));
        SimulationStep++;
        _steps++;
        _lastStep = DateTime.UtcNow;
    }
}

public struct SimulationArea
{  
    private WorldDimensions _dimensions;
    private WorldPosition _position;
    private int _positionStep;

    public WorldDimensions Dimensions { get { return _dimensions; } }
    public WorldPosition Position { get { return _position; } }
    public int Left { get { return Position.X; } }
    public int Right { get { return Position.X + Dimensions.Width; } }
    public int Top { get { return Position.Y; } }
    public int Bottom { get { return Position.Y + Dimensions.Height; } }
    public int PositionStep { get { return _positionStep; } }

    public SimulationArea(WorldDimensions dimensions, WorldPosition position, int positionStep)
    {
        _dimensions = dimensions;
        _position = position;
        _positionStep = positionStep;
    }

    public override string ToString()
    {
        return base.ToString();
    }
}