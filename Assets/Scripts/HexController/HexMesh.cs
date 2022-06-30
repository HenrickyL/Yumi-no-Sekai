using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
    static List<Vector3> vertices = new List<Vector3>();
    static List<Color> colors = new List<Color>();
    static List<int> triangles = new List<int>();
    MeshCollider meshCollider;
    
    void Awake() {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        //We need to add a collider to the grid 
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
    }

    
    public void AddTriangleColor(Color c1,Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }
    public void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddTriangle(Vector3 v1,Vector3 v2, Vector3 v3){
        int vertexIndex = vertices.Count;
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v1): v1);
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v2): v2);
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v3): v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex+1);
        triangles.Add(vertexIndex+2);
    }
    public void AddTriangleUnperturbed (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

    public void AddQuad(Vector3 v1,Vector3 v2,Vector3 v3, Vector3 v4){
        int vertexIndex = vertices.Count;
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v1): v1);
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v2): v2);
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v3): v3);
        vertices.Add(HexGridChunk.GetWithIrregularity() ? HexMetrics.Perturb(v4): v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex+2);
        triangles.Add(vertexIndex+1);
        triangles.Add(vertexIndex+1);
        triangles.Add(vertexIndex+2);
        triangles.Add(vertexIndex+3);
    }
    public void AddQuadColor(Color c1,Color c2){
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }
    public void AddQuadColor(Color c1,Color c2,Color c3,Color c4){
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c3);
        colors.Add(c4);
    }
    public void AddQuadColor (Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}
    public void Clear () {
		hexMesh.Clear();
		vertices.Clear();
		colors.Clear();
		triangles.Clear();
	}

	public void Apply () {
		hexMesh.SetVertices(vertices);
		hexMesh.SetColors(colors);
		hexMesh.SetTriangles(triangles, 0);
		hexMesh.RecalculateNormals();
		meshCollider.sharedMesh = hexMesh;
	}

   
}
