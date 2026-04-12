using UnityEngine;

// EarthFace Class
// generates a flat square mesh that acts a side for the earth

public class EarthFace
{
    Mesh mesh;
    int resolution;
    int meshSize;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    int[,] vertexIndicesMap;

    public MeshData meshData;

    public EarthFace(Mesh mesh, int resolution, Vector3 localUp)
    { 
        this.mesh = mesh;
        this.resolution = resolution;
        this.meshSize = resolution - 2;
        this.localUp = localUp;


        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    // converts vertex position into spherical positions.
    public static Vector3 PointOnCubeToPointOnSphere(Vector3 p)
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;
        float x = p.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        float y = p.y * Mathf.Sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
        float z = p.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);
        return new Vector3(x, y, z);
    }

    public Vector3 GetVertexPosition(int x, int y)
    {
        return mesh.vertices[vertexIndicesMap[x, y]];
    }

    public Vector3 GetLocalUp()
    {
        return localUp;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    public MeshData ConstructEarthFaceMesh()
    {
        int borderedSize = resolution;
        int meshSize = resolution - 2;


        int verticesPerLine = (meshSize - 1) / 1 + 1;

        meshData = new MeshData(verticesPerLine);

        vertexIndicesMap = new int[resolution, resolution];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;


        for (int y = 0; y < resolution; y += 1)
        {
            for (int x = 0; x < resolution; x += 1)
            {
                bool isBorderVertex = y == 0 || y == resolution - 1 || x == 0 || x == resolution - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < resolution; y += 1)
        {
            for (int x = 0; x < resolution; x += 1)
            {
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2(x - 1, y - 1) / (meshSize - 1);
                // when x == 0, percent.x == 0
                // when x == max, percent.x == 1
                // tells us how close to complete each loop is
                // used to define where the vertex should be along the face

                Vector3 pointOnUnitCube = localUp + ((percent.x - 0.5f) * 2 * axisA) + ((percent.y - 0.5f) * 2 * axisB);
                Vector3 pointOnUnitSphere = PointOnCubeToPointOnSphere(pointOnUnitCube);//pointOnUnitCube.normalized;
                meshData.AddVertex(pointOnUnitSphere, vertexIndex); // x == 0 is out of the mesh as a border vertex


                if (x < resolution - 1 && y < resolution - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + 1, y];
                    int c = vertexIndicesMap[x, y + 1];
                    int d = vertexIndicesMap[x + 1, y + 1];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;


            }
        }



        return meshData;
    }


}
