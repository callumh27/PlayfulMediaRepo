using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// need to bake the plate rotations between these two timeline points
// calculate the centre point of each plate and then calculate the rotation on the surface of the sphere to get from 1 to 2
public class TimelineTransition
{
    public EarthTimelinePoint point1;
    public EarthTimelinePoint point2;

    private Dictionary<int, Quaternion> plateRotations = new();

    private Vector4[] rotatedTectonicPoints;

    public RenderTexture bakedTectonicMap1;
    public RenderTexture bakedHeightMap1;
    public RenderTexture bakedHeightMap2;


    public Vector4[] GetRotatedTectonicPoints(float t)
    {
        Vector4[] points1 = point1.tectonicPoints;
        Vector4[] result = new Vector4[points1.Length];

        for (int i = 0; i < points1.Length; i++)
        {
            Vector4 point = points1[i];
            int plateId = Mathf.RoundToInt(point.w);

            if (plateRotations.TryGetValue(plateId, out Quaternion rot))
            {
                Quaternion slerped = Quaternion.Slerp(Quaternion.identity, rot, t);
                Vector3 rotated = slerped * new Vector3(point.x, point.y, point.z);
                result[i] = new Vector4(rotated.x, rotated.y, rotated.z, point.w);
            }
            else
            {
                result[i] = point;
            }
        }
        return result;
    }

    public Vector4[] GetSlerpedRotations(float t)
    {
        Vector4[] result = new Vector4[25];

        for (int i = 0; i < 25; i++)
        {
            result[i] = new Vector4(0, 0, 0, 1);
        }

        foreach (var rotation in plateRotations)
        {
            if (rotation.Key >= 25) continue;
            Quaternion slerped = Quaternion.Slerp(Quaternion.identity, rotation.Value, t);
            result[rotation.Key] = new Vector4(slerped.x, slerped.y, slerped.z, slerped.w);
        }
        return result;
    }


    public void Bake()
    {
        plateRotations.Clear();

        var tectonicPoints1 = GroupPointsByPlate(point1.tectonicPoints);
        var tectonicPoints2 = GroupPointsByPlate(point2.tectonicPoints);

        bakedHeightMap1 = ConvertToRenderTexture(point1.heightMap);
        bakedHeightMap2 = ConvertToRenderTexture(point2.heightMap);
        bakedTectonicMap1 = ConvertToRenderTexture(point1.tectonicMap);

        foreach(var plate in tectonicPoints1)
        {
            int plateId = plate.Key;

            Vector3 center1 = GetCenterOfMass(plate.Value).normalized;
            if (!tectonicPoints2.TryGetValue(plateId, out List<Vector3> destPoints))
            {
                plateRotations[plateId] = Quaternion.identity;
                continue;
            }

            Vector3 center2 = GetCenterOfMass(destPoints).normalized;

            plateRotations[plateId] = Quaternion.FromToRotation(center1, center2);
            
        }
        
    }

    RenderTexture ConvertToRenderTexture(Texture3D source)
    {
        RenderTexture renderTexture = new RenderTexture(source.width, source.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = source.depth;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
        AssetDatabase.CreateAsset(renderTexture, CreateCorrectPathName("Assets/Pre-Compute/Cache/", "BlankEarthRenderTexture"));

        Graphics.CopyTexture(source, renderTexture);
        return renderTexture;
    }
    string CreateCorrectPathName(string path, string assetName)
    {
        int fileNumber = 0;
        string fileName;
        string fullPath;

        do
        {
            fileName = $"{assetName}_{fileNumber}.asset";
            fullPath = path + fileName;
            fileNumber++;
        }
        while (File.Exists(fullPath));

        return fullPath;
    }
    private Dictionary<int, List<Vector3>> GroupPointsByPlate(Vector4[] points)
    {
        var tectonicPointGroups = new Dictionary<int, List<Vector3>>();
        foreach (var point in points)
        {
            int id = Mathf.RoundToInt(point.w);
            if (!tectonicPointGroups.ContainsKey(id))
            {
                tectonicPointGroups[id] = new List<Vector3>();
            }
            tectonicPointGroups[id].Add(new Vector3(point.x, point.y, point.z));
        }
        return tectonicPointGroups;
    }

    private Vector3 GetCenterOfMass(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach(var p in points)
        {
            sum += p.normalized;
        }
        return (sum / points.Count); 
    }

}
