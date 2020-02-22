using UnityEngine;

[CreateAssetMenu]
public class TerraShapeSO : ScriptableObject, ITerraShape
{
    public TerraVector[] _terraVectors;
    
    public TerraVector[] TerraVectors
    {
        get
        {
            return _terraVectors;
        }
    }
}
