using UnityEngine;

// EarthGenerator Class
// Handles the generation of the Earth mesh by generating 6 seperate sides of a cube and then spherising it by normalising the position of each vertex

public class TectonicPlate
{
    public int plateID;
    public Color displayColour;
    public Vector3 plateDirection;
    public bool isOceanic;
}

public class EarthGenerator : MonoBehaviour
{
    [Range(2, 256)]
    public int earthResolution = 10;

    public Material material;

    public int earthRadius = 20;

    [Header("Climate Settings")]
    public int temperature = 30;

    [Header("Tectonic Settings")]
    public int amountOfPlates = 9;
    public TectonicPlate[] tectonicPlates;
    public RenderTexture renderTexture;
    public ComputeShader computeShader;
    public int renderTextureSize = 256;

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
        CalculateTectonicPoints(out Vector4[] colours, out Vector4[] points);
        GenerateMesh();
        transform.localScale = Vector3.one * earthRadius;
        GenerateColours();
    }

    void GenerateMesh()
    {
        foreach (EarthFace face in earthFaces)
        {
            MeshData faceMeshData = face.ConstructEarthFaceMesh();

            faceMeshData.ConstructMesh(face.GetMesh());
        }

    }

    void GenerateColours()
    {
        foreach (MeshFilter m in meshFilters)
        {
            m.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TectonicTexture", renderTexture);
            m.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_PlanetRadius", earthRadius);
        }
    }

    public void CalculateTectonicPoints(out Vector4[] colours, out Vector4[] points)
    {

        renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = renderTextureSize;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();


        Vector4[] tectonicPlates = new Vector4[amountOfPlates];
        Vector4[] tectonicPoints = new Vector4[amountOfPlates];


        for (int i = 0; i < amountOfPlates; i++)
        {
            tectonicPoints[i] = UnityEngine.Random.onUnitSphere * 10f;
            tectonicPlates[i] = new Vector4(UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f), UnityEngine.Random.Range(0f, 1.0f), 1);
            
        }

        

        colours = tectonicPlates;
        points = tectonicPoints;

        computeShader.SetTexture(0, "TectonicLookupTexture", renderTexture); // can use .FindKernel() method if using multiple kernels
        computeShader.SetInt("textureSize", renderTextureSize);
        computeShader.SetInt("amountOfPlates", amountOfPlates);
        computeShader.SetFloat("planetRadius", 10);
        computeShader.SetVectorArray("tectonicPoints", points);
        computeShader.SetVectorArray("tectonicColours", colours);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);



    }

}
