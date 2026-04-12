using UnityEngine;
using UnityEngine.Rendering;


// MeshData Class
// essentially acts as a helper class to make mesh generation easier


public class MeshData
{
    Vector3[] vertices;
    int[] triangles;

    // border verts and tris act as a skirt around each side of the cube so that when normal vectors are calculated they match at the edges to prevent seams
    Vector3[] borderVertices;
    int[] borderTriangles;

    int triIndex;
    int borderTriIndex;

    public MeshData(int verticesPerRow)
    {
        vertices = new Vector3[verticesPerRow * verticesPerRow];
        triangles = new int[(verticesPerRow - 1) * (verticesPerRow - 1) * 6];


        borderVertices = new Vector3[verticesPerRow * 4 + 4]; // *4 for the sides and +4 for the corners
        borderTriangles = new int[24 * verticesPerRow];

    }

    public void AddVertex(Vector3 vertexPosition, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
        }
    }

    public void AddTriangle(int vertexA, int vertexB, int vertexC)
    {
        if (vertexA < 0 || vertexB < 0 || vertexC < 0)
        {
            borderTriangles[borderTriIndex] = vertexA;
            borderTriangles[borderTriIndex + 1] = vertexB;
            borderTriangles[borderTriIndex + 2] = vertexC;
            borderTriIndex += 3;
        }
        else
        {
            triangles[triIndex] = vertexA;
            triangles[triIndex + 1] = vertexB;
            triangles[triIndex + 2] = vertexC;
            triIndex += 3;
        }


    }

    // custom method used to calculate normals for the cube sphere as unity's built in function does not account for the vertices of other meshes.
    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triCount = triangles.Length / 3;
        for (int i = 0; i < triCount; ++i)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = CalculateSurfaceNormal(vertexIndexA, vertexIndexB, vertexIndexC, vertices);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriCount; ++i)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = CalculateSurfaceNormal(vertexIndexA, vertexIndexB, vertexIndexC, vertices);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }

        }


        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;

    }

    Vector3 CalculateSurfaceNormal(int verticeIndexA, int verticeIndexB, int verticeIndexC, Vector3[] vertices)
    {

        Vector3 pointA;
        Vector3 pointB;
        Vector3 pointC;


        if (verticeIndexA < 0)
        {
            pointA = borderVertices[-verticeIndexA - 1];
        } else {
            pointA = vertices[verticeIndexA];
        }

        if (verticeIndexB < 0)
        {
            pointB = borderVertices[-verticeIndexB - 1];
        }
        else
        {
            pointB = vertices[verticeIndexB];
        }

        if (verticeIndexC < 0)
        {
            pointC = borderVertices[-verticeIndexC - 1];
        }
        else
        {
            pointC = vertices[verticeIndexC];
        }


        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }


    public Mesh ConstructMesh(Mesh mesh)
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = CalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }


}
