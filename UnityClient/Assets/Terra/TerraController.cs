using UnityEngine;

public class TerraController : MonoBehaviour/*, TerraShape.ITerraShapeConfig*/
{

   /* [SerializeField] 
    private TerraShapeSO _terraShapeSo;
    
    [SerializeField] 
    private WorldRendererLoader _worldRendererLoader;

    [SerializeField] private GameObject _editorCude;

    private bool _altKeyDown;
    private bool _mouseButtonDown;
    private TerraShape _terraShape;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_mouseButtonDown)
            {
                MouseDown();
            }
            
            _mouseButtonDown = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_mouseButtonDown)
            {
                MouseUp();
            }
            _mouseButtonDown = false;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _altKeyDown = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            _altKeyDown = false;
        }

        if (_terraShape != null)
        {
            _terraShape.ShapeGameObject.transform.position = _worldRendererLoader.MousePosition3OnGrid;
            _worldRendererLoader.ChangeHeight(_worldRendererLoader.MousePositionOnTerra, _altKeyDown ? -1:1);
        }
    }

    private void MouseUp()
    {
        if (_terraShape != null)
        {
            _terraShape.Destroy();
            _terraShape = null;
        }
    }
    
    private void MouseDown()
    {
        _terraShape = TerraShape.Generate(_terraShapeSo, _worldRendererLoader, this);
    }

    public GameObject EditorCude => _editorCude;*/
}
