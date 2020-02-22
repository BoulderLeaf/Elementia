using UnityEngine;

public class TerraShape
{
    public interface ITerraShapeConfig
    {
        GameObject EditorCude { get; }
    }
    
    private GameObject _shapeGameObject;

    public GameObject ShapeGameObject => _shapeGameObject;

    private TerraShape(GameObject shapeGameObject)
    {
        _shapeGameObject = shapeGameObject;
    }

    public static TerraShape Generate(ITerraShape shape, ITerraWorld world, ITerraShapeConfig config)
    {
        GameObject go =  GameObject.Instantiate(config.EditorCude);
        go.SetActive(true);
        go.name = "TerraShapePlane";
        TerraShape terraShape = new TerraShape(go);
        return terraShape;
    }

    public void Destroy()
    {
        GameObject.Destroy(_shapeGameObject);       
    }
}
