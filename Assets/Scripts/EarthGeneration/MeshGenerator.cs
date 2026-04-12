using UnityEngine;
using UnityEngine.Rendering;


// MeshData Class
// essentially acts as a helper class to make mesh generation easier


public class MeshData
{
    Vector3[] vertices;
    int[] triangles;

    int triIndex;

    public MeshData(int verticesPerRow)
    {
        vertices = new Vector3[verticesPerRow * verticesPerRow];
        triangles = new int[verticesPerRow * verticesPerRow];

    }

    public void AddVertex(Vector3 vertexPosition, int vertexIndex)
    {
        vertices[vertexIndex] = vertexPosition;
    }

    public Mesh ConstructMesh(Mesh mesh)
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


}
