using System;
using System.Collections;
using System.Collections.Generic;
using PandeaGames;
using UnityEngine;

public class WorldRendererLoader : MonoBehaviour, ITerraWorld {

    [SerializeField]
    private ServiceManager _serviceManager;  

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private int _radius;

    [SerializeField]
    private float _scale = 1;
    public float Scale
    {
        get { return _scale; }
    }

    [SerializeField]
    private Material _material;
    
    [SerializeField]
    private Texture _texture;

    [SerializeField] 
    private GameObject _grassSprite;

    [SerializeField] 
    private LayerMask _layerMaskForMousePosition;
    
    [SerializeField] 
    private int _geometryLayer;

    private WorldDataToken _currentToken;

    private GameObject _renderingPlane;

    private WorldDataAccessService _worldDataAccessService;
    private int RedrawCountdown = 0;
    private HashSet<string> _grassLocations;
    private Vector3 _mousePosition;
    public Vector3 MousePosition => _mousePosition;
    private TerraVector _mousePositionOnTerra;
    public TerraVector MousePositionOnTerra => _mousePositionOnTerra;
    private Vector2 _mousePositionOnGrid;
    public Vector2 MousePositionOnGrid => _mousePositionOnGrid;
    private Vector3 _mousePosition3OnGrid;
    public Vector3 MousePosition3OnGrid => _mousePosition3OnGrid;

    private GameObject _grassContainer;
    private void Start()
    {
        _grassContainer = new GameObject();
        _grassLocations = new HashSet<string>();
        _worldDataAccessService = Game.Instance.GetService<WorldDataAccessService>();
        DrawGround();
    }

    // Update is called once per frame
    void Update ()
    {
		if(--RedrawCountdown == 0)
        {
            DrawGround();
        }

        if (_currentToken != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100f, _layerMaskForMousePosition, QueryTriggerInteraction.Collide)) {
                _mousePosition = hit.point;
                _mousePositionOnGrid = new Vector2(MathUtils.Round(_mousePosition.x, _scale), MathUtils.Round(_mousePosition.z, _scale));
                int x = (int) Math.Round(_mousePositionOnGrid.x);
                int y = (int) Math.Round(_mousePositionOnGrid.y);
                _mousePositionOnTerra = new TerraVector() {x = x, y = y};
                TerraVector mousePositionOnToken = new TerraVector() {x = x - _currentToken.Request.left, y = y - _currentToken.Request.top};
                _mousePosition3OnGrid = GetVertice(_currentToken, mousePositionOnToken.x, mousePositionOnToken.y);
                // Do something with the object that was hit by the raycast.
            }
        }
	}
    
    

    private void DrawGround()
    {
        _worldDataAccessService.RequestAccess((worldDataAccess) =>
            {
                StartCoroutine(DrawGroundCoroutine(worldDataAccess));
            }, () => { });
    }

    private IEnumerator DrawGroundCoroutine(WorldDataAccess worldDataAccess)
    {
        Vector3 targetPosition = _target.position;

        LoadTokenJob loadTokenJob =
            new LoadTokenJob(worldDataAccess, targetPosition, _radius, Application.persistentDataPath);

        loadTokenJob.Start();

        while (loadTokenJob.Output == null)
        {
            yield return 0;
        }


        OnGetTokenComplete(loadTokenJob.Output);
        worldDataAccess.ReturnToken(loadTokenJob.Output);
    }

    private void OnGetTokenComplete(WorldDataToken token)
    {
        StartCoroutine(RenderGround(token));
        
    }

    private Mesh _renderMesh;
    private MeshFilter _renderMeshFilter;
    private Texture2D _heightTexture;

    public Renderer _dataPlaneRenderer; 
    private IEnumerator RenderGround(WorldDataToken token)
    {
        _heightTexture = new Texture2D(token.Request.width, token.Request.height);
        _currentToken = token;
        MeshCollider collider = null;

        if(_renderingPlane == null)
        {
            _renderingPlane = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Plane));
            collider = _renderingPlane.AddComponent<MeshCollider>();
            Rigidbody rb = _renderingPlane.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        else
        {
            collider = _renderingPlane.GetComponent<MeshCollider>();
        }

        GameObject plane = _renderingPlane;
        plane.layer = _geometryLayer;
        plane.GetComponent<Renderer>().material = _material;

        MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        _renderMeshFilter = meshFilter;
        Mesh mesh = meshFilter.sharedMesh;
        _renderMesh = mesh;
        collider.sharedMesh = mesh;

        int verticiesLength = (token.Request.width + 1) * (token.Request.height + 1);

        Vector3[] vertices = new Vector3[verticiesLength];
        Color[] colors = new Color[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        float[] grass = new float[vertices.Length];
        // Vector2[] triangles = new Vector2[(int)(dimensions.Area * 2)];

        //for every point, there is 2 triangles, equaling 6 total vertices
        int[] triangles = new int[(int)((token.Request.width * token.Request.height) * 6)];

        float totalWidth = token.Request.width * _scale;
        float totalHeight = token.Request.height * _scale;

        //Create Vertices
        for (int x = 0; x < token.Request.width + 1; x++)
        {
            for (int y = 0; y < token.Request.height + 1; y++)
            {
                int position = (x * (token.Request.width + 1)) + y;
                
                if (x > 0 && y > 0 && x < token.Request.width && y < token.Request.height)
                {
                    vertices[position] = GetVertice(token, x, y);
                }
                else
                {
                    vertices[position] = new Vector3((token.Request.left + x) * _scale, token.GetUshort(x, y, UshortDataID.HeightLayerData) * 0.5f, (token.Request.top + y) * _scale);
                }
                
                colors[position] = new Color(0.5f, 0.5f, 0.5f);
                uvs[position] = new Vector2((float)x / (float)token.Request.width, (float)y / (float)token.Request.height);
                normals[position] = Vector3.up;
                grass[position] = 0.5f;

                string grassId = x + "_" + y;

               /* if (!_grassLocations.Contains(grassId))
                {
                    
                    float scaleFactor = 1-(25.5f - vertices[position].y) / 0.5f;
                    Vector3 scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                    Vector3 grassPosition = new Vector3(vertices[position].x, (vertices[position].y - 1) + (scaleFactor * 2f), vertices[position].z);
                    GameObject grassInstance = Instantiate(_grassSprite, grassPosition, _grassSprite.transform.rotation, _grassContainer.transform);
                    grassInstance.transform.localScale = scale;
                    _grassLocations.Add(grassId);
                }*/
            }

           //yield return 0;
        }

        List<Vector3> vectorTriangles = new List<Vector3>();

        //Create Triangles
        for (int x = 0; x < token.Request.width; x++)
        {
            for (int y = 0; y < token.Request.height; y++)
            {
              //  Debug.Log("height "+token.GetUshort(x, y, UshortDataID.HeightLayerData));
                //_heightTexture.SetPixel(x, y, new Color(0.5f, 1, 1));
                _heightTexture.SetPixel(x, y, new Color(0.5f, (float)token.GetUshort(x, y, UshortDataID.HeightLayerData) / (float)100, 0.5f));
                SetTriangles(token, x, y, triangles, vectorTriangles);
            }
        }

        mesh.Clear();
        _heightTexture.Apply();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.normals = normals;

        RedrawCountdown = 100;
        plane.SetActive(true);
        
       // _material.SetInt("_Width", token.Request.width + 1);
       // _material.SetInt("_Height", token.Request.height + 1);

        for (int i = 0; i < grass.Length / 1023; i++)
        {
            float[] grassInput = new float[1023];
            for (int j = 0; j < 1023; j++)
            {
                grassInput[j] = grass[i * 1023 + j];
            }

          //  _material.SetFloatArray("_Grass" + i, grassInput);
        }
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        _dataPlaneRenderer.material.mainTexture =_heightTexture;
        yield return 0;
    }

    private void SetTriangles(WorldDataToken token, int x, int y, int[] triangles, List<Vector3> vectorTriangles)
    {
        //we are making 2 triangles per loop. so offset goes up by 6 each time
        int triangleOffset = (x * token.Request.height + y) * 6;
        int verticeX = token.Request.width + 1;
        int verticeY = token.Request.height + 1;
                
        //triangle 1
        triangles[triangleOffset] = x * verticeY + y;
        triangles[1 + triangleOffset] = x * verticeY + y + 1;
        triangles[2 + triangleOffset] = x * verticeY + y + verticeY;

        vectorTriangles.Add(new Vector3(triangles[triangleOffset], triangles[1 + triangleOffset], triangles[2 + triangleOffset]));

        //triangle 2
        triangles[3 + triangleOffset] = x * verticeY + y + verticeY;
        triangles[4 + triangleOffset] = x * verticeY + y + 1;
        triangles[5 + triangleOffset] = x * verticeY + y + verticeY + 1;

        vectorTriangles.Add(new Vector3(triangles[3 + triangleOffset], triangles[4 + triangleOffset], triangles[5 + triangleOffset]));
    }

    private Vector3 GetVertice(WorldDataToken token, int x, int y)
    {
        return new Vector3(
            (token.Request.left + x) * _scale, 
            ((token.GetUshort(x, y, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y, UshortDataID.HeightLayerData)) / 9f) * 0.5f,
            (token.Request.top + y) * _scale);
    }

    public void ChangeHeight(TerraVector position, int delta)
    {
        if (_currentToken == null)
        {
            throw new InvalidOperationException(string.Format("There is no current {0}", typeof(WorldDataToken)));
        }
        else
        {
            Mesh mesh = _renderMeshFilter.sharedMesh;
            List<Vector3> vectorTriangles = new List<Vector3>();
            var triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Color[] colors = mesh.colors;
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;

            
            ushort currentValue = _currentToken.GetUshort(position.x - _currentToken.Request.left, position.y - _currentToken.Request.top, UshortDataID.HeightLayerData);
            TerraVector localVector = _currentToken.ToLocal(position);
            int vertPosition = (localVector.x * (_currentToken.Request.width + 1)) + localVector.y;
            _currentToken.SetUshort(localVector.x, localVector.y, (ushort)( (int) currentValue + delta), UshortDataID.HeightLayerData);
            //vertices[vertPosition] = GetVertice(_currentToken, localVector.x, localVector.y);
            UpdateSurroundingHights(localVector, vertices, _currentToken);
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            /*vertices[localVector.x * localVector.y] =
                GetVertice(_currentToken, localVector.x, localVector.y);
            _renderMeshFilter.sharedMesh.vertices = _renderMesh.vertices;
            


            //SetTriangles(_currentToken, localVector.x, localVector.y, triangles, vectorTriangles);
            
            mesh.Clear();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.normals = normals;
            
            _renderMeshFilter.sharedMesh.RecalculateNormals();
            _renderMeshFilter.sharedMesh.RecalculateBounds();
            _renderMeshFilter.sharedMesh.RecalculateTangents();*/
            //RedrawCountdown = 1;
            // RenderGround(_currentToken);
        }
    }
/*
 * ((token.GetUshort(x, y, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y -1, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y, UshortDataID.HeightLayerData) +
              token.GetUshort(x + 1, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y + 1, UshortDataID.HeightLayerData) +
              token.GetUshort(x - 1, y, UshortDataID.HeightLayerData)) / 9f) * 0.5f,*/
 
    private void UpdateSurroundingHights(TerraVector vector, Vector3[] vertices, WorldDataToken token)
    {
        UpdateHights(vector, vertices, token);
        
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y - 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x, y = vector.y - 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y - 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y + 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x, y = vector.y + 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y + 1}, vertices, token);
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y}, vertices, token);
    }
    
    private void UpdateHights(TerraVector vector, Vector3[] vertices, WorldDataToken token)
    {
        int vertPosition = (vector.x * (token.Request.width + 1)) + vector.y;
        vertices[vertPosition] = GetVertice(_currentToken, vector.x, vector.y);
    }
}

public class LoadTokenJob : ThreadedJob
{
    public WorldDataToken Output;
    private WorldDataAccess _worldDataAccess;
    private Vector3 _position;
    private int _radius;
    private string _persistentDataPath;
    public LoadTokenJob(WorldDataAccess worldDataAccess, Vector3 position, int raduis, string persistentDataPath)
    {
        _worldDataAccess = worldDataAccess;
        _position = position;
        _radius = raduis;
        _persistentDataPath = persistentDataPath;
    }

    protected override void ThreadFunction()
    {
        _worldDataAccess.GetToken(new TokenRequest((int)_position.x - _radius, (int)_position.x + _radius, (int)_position.z + _radius, (int)_position.z - _radius), _persistentDataPath,
            token =>
            {
                Debug.Log("LoadTokenJob.ThreadFunction token: "+token);
                Output = token;
            });
        
    }
}