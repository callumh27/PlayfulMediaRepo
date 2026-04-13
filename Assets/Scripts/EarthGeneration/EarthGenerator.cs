using UnityEngine;

// EarthGenerator Class
// Handles the generation of the Earth mesh by generating 6 seperate sides of a cube and then spherising it by normalising the position of each vertex

public class EarthGenerator : MonoBehaviour
{
    [Range(2, 256)]
    public int earthResolution = 10;

    public int earthRadius = 20;

    [Header("Climate Settings")]
    public int temperature = 30;

    MeshFilter[] meshFilters;
    EarthFace[] earthFaces;


    // create the base mesh data
    void Initialise()
    {
        meshFilters = new MeshFilter[6];
        earthFaces = new EarthFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        // Generate the 6 side's mesh objects
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("PlanetSide" + i.ToString());
                meshObject.transform.parent = transform;

                Material material = new Material(Shader.Find("Shader Graphs/EarthTerrain"));
                material.color = new Color(0.2f, 1, 0.2f);
                material.SetFloat("Smoothness", 0);
                meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            
            earthFaces[i] = new EarthFace(meshFilters[i].sharedMesh, earthResolution, directions[i]);
        }

    }

    public void Start()
    {
        Initialise();
        GenerateMesh();
        transform.localScale = Vector3.one * earthRadius;
    }

    void GenerateMesh()
    {
        foreach (EarthFace face in earthFaces)
        {
            MeshData faceMeshData = face.ConstructEarthFaceMesh();

            faceMeshData.ConstructMesh(face.GetMesh());
        }

    }

}
