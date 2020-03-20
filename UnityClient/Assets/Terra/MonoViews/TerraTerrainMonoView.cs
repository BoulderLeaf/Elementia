using System;
using System.Collections.Generic;
using PandeaGames;
using PandeaGames.Data;
using Terra.ViewModels;
using UnityEngine;

namespace Terra.MonoViews
{
    public class TerraTerrainMonoView : MonoBehaviour
    {
        private TerraTerrainGeometryDataModel _renderingChunk;
        private TerraChunksViewModel _vm;
        public void Start()
        {
            _vm = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
           // _vm.OnChunkAdded += VmOnChunkAdded;
            Game.Instance.GetViewModel<TerraViewModel>(0).OnGeometryUpdate += GeometryUpdate;
        }

        private void GeometryUpdate(TerraTerrainGeometryDataModel geom)
        {
            if (_renderingChunk != null)
            {
                _renderingChunk.OnDataHasChanged -= OnDataHasChanged;
            }
            _renderingChunk = geom;
            _renderingChunk.OnDataHasChanged += OnDataHasChanged;
            
            RenderGround(geom);
        }

        private GameObject _renderingPlane;

    [SerializeField] private Vector3 _planeOffset;
    [SerializeField] private string _generatedGameObjectName;
    [SerializeField] private bool _debugView;

    [Serializable]
    private struct FogLayerConfig
    {
        public Color color;
    }

    [SerializeField] private FogLayerConfig[] _fogConfig;
    
    [SerializeField]
    private float _scale = 1;
    public float Scale
    {
        get { return _scale; }
    }


    private Mesh mesh;
    private MeshFilter meshFilter = null;

    private void OnDataHasChanged(IEnumerable<TerraTerrainGeometryDataPoint> data)
    {
        Color[] colors = meshFilter.sharedMesh.colors;
        Vector3[] vertices = meshFilter.sharedMesh.vertices;
        
        foreach (TerraTerrainGeometryDataPoint dataPoint in data)
        {
            UpdateData(dataPoint, colors, vertices);
        }

        meshFilter.sharedMesh.colors = colors;
        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateTangents();
        MeshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    
    private void UpdateData(TerraTerrainGeometryDataPoint vector, Color[] colors, Vector3[] vertices)
    {
        int vertPosition = (vector.Vector.x * (_renderingChunk.Width + 1)) + vector.Vector.y;
        vertices[vertPosition] = vector.Data;
    }

    private MeshCollider MeshCollider;
    private void RenderGround(TerraTerrainGeometryDataModel chunk)
    {
        if(_renderingPlane == null)
        {
            _renderingPlane = Instantiate(new GameObject(string.IsNullOrEmpty(_generatedGameObjectName) ? "FogOfWar":_generatedGameObjectName), transform);
            _renderingPlane.transform.position = _planeOffset;
            _renderingPlane.AddComponent<MeshRenderer>();
            _renderingPlane.AddComponent<MeshFilter>();
            MeshCollider = _renderingPlane.AddComponent<MeshCollider>();
            ///_renderingPlane.AddComponent<Rigidbody>();
            meshFilter = _renderingPlane.GetComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            Rigidbody rb = _renderingPlane.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
        }

        GameObject plane = _renderingPlane;
        plane.SetActive(false);
        plane.GetComponent<Renderer>().material = TerraGameResources.Instance.TerrainMaterial;

        meshFilter = plane.GetComponent<MeshFilter>();
        mesh = meshFilter.sharedMesh;
        MeshCollider.sharedMesh = meshFilter.sharedMesh;
        int verticiesLength = (chunk.Width + 1) * (chunk.Height + 1);

        Vector3[] vertices = new Vector3[verticiesLength];
        Color[] colors = new Color[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector3[] normals = new Vector3[vertices.Length];
        float[] grass = new float[vertices.Length];
        // Vector2[] triangles = new Vector2[(int)(dimensions.Area * 2)];

        //for every point, there is 2 triangles, equaling 6 total vertices
        int[] triangles = new int[(int)((chunk.Width * chunk.Height) * 6)];

        //Create Vertices
        for (int x = 0; x < chunk.Width; x++)
        {
            for (int y = 0; y < chunk.Height; y++)
            {
                Color color = GetColor(x, y);
                
                int position = (x * (chunk.Width + 1)) + y;
                vertices[position] = chunk[x, y];
                colors[position] = color;
                uvs[position] = new Vector2((float)x / (float)chunk.Width, (float)y / (float)chunk.Height);
                normals[position] = Vector3.up;
                grass[position] = 0.5f;
            }
        }

        List<Vector3> vectorTriangles = new List<Vector3>();

        //Create Triangles
        for (int x = 0; x < chunk.Width; x++)
        {
            for (int y = 0; y < chunk.Height; y++)
            {
                SetTriangles(chunk, x, y, triangles, vectorTriangles);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.normals = normals;

        plane.SetActive(true);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        plane.SetActive(false);
        plane.SetActive(true);
    }

    private Color GetColor(int x, int y)
    {
        return Color.cyan;
    }

    private void SetTriangles(TerraTerrainGeometryDataModel chunk, int x, int y, int[] triangles, List<Vector3> vectorTriangles)
    {
        //we are making 2 triangles per loop. so offset goes up by 6 each time
        int triangleOffset = (x * chunk.Height + y) * 6;
        int verticeX = chunk.Width + 1;
        int verticeY = chunk.Height + 1;
                
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

    public void OnDrawGizmos()
    {
        if (_debugView)
        {
            MeshFilter MeshFilter = _renderingPlane.GetComponent<MeshFilter>();
            for (int i = 0; i < MeshFilter.sharedMesh.vertexCount; i++)
            {
                Gizmos.DrawLine(MeshFilter.sharedMesh.vertices[i], MeshFilter.sharedMesh.vertices[i] + MeshFilter.sharedMesh.normals[i] * 1);
            }
        }
        
    }

    /* private void UpdateSurroundingHights(TerraVector vector, Vector3[] vertices, TerraWorldChunk chunk)
    {
        UpdateHights(vector, vertices, chunk);
        
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y - 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x, y = vector.y - 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y - 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x + 1, y = vector.y + 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x, y = vector.y + 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y + 1}, vertices, chunk);
        UpdateHights(new TerraVector(){x = vector.x - 1, y = vector.y}, vertices, chunk);
    }*/
    
   /* private void UpdateHights(TerraVector vector, Vector3[] vertices,  TerraWorldChunk chunk)
    {
        int vertPosition = (vector.x * (chunk.Width + 1)) + vector.y;
        vertices[vertPosition] = GetVertice(_currentToken, vector.x, vector.y);
    }*/
    
   /* private Vector3 GetVertice(TerraWorldChunk chunk, int x, int y)
    {
        return new Vector3(
            (chunk..left + x), 
            ((chunk[x, y].Height) +
             chunk[x - 1, y -1].Height +
            chunk[x, y -1].Height +
               chunk[x + 1, y -1].Height +
            chunk[x + 1, y].Height +
            chunk[x + 1, y + 1].Height +
            chunk[x, y + 1].Height +
            chunk[x - 1, y + 1].Height +
                  chunk[x - 1, y].Height) / 9f) * 0.5f,
            (token.Request.top + y));
    }*/
    }
}