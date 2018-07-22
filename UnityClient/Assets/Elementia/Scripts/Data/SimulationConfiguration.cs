using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class SimulationConfiguration : ScriptableObject
{
    public const string IndexFilename = "simulation.dat";

    [SerializeField]
    private WorldDimensions _dimemsions;

    [SerializeField]
    private int _radius;

    [SerializeField]
    private uint _simulationDevisions = 1;

    public WorldDimensions Dimemsions { get { return _dimemsions; } }
    public int Radius { get { return _radius; } }
    public uint SimulationDevisions { get { return _simulationDevisions; } }

    public WorldSimulationState GenerateSimulationState(WorldIndex worldIndex)
    {
        WorldSimulationState state = new WorldSimulationState();

        state.Dimensions = Dimemsions;
        state.WorldDimensions = worldIndex.Dimensions;
        state.Radius = Radius;
        state.SimulationDevisions = SimulationDevisions;

        return state;
    }
}