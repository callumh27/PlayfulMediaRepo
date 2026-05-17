using System.Collections.Generic;
using UnityEngine;

// need to bake the plate rotations between these two timeline points
// calculate the centre point of each plate and then calculate the rotation on the surface of the sphere to get from 1 to 2
public class TimelineTransition
{
    public EarthTimelinePoint point1;
    public EarthTimelinePoint point2;

    private Dictionary<int, Quaternion> plateRotations = new();

    private Vector4[] rotatedTectonicPoints;


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

    public void Bake()
    {
        plateRotations.Clear();

        var tectonicPoints1 = GroupPointsByPlate(point1.tectonicPoints);
        var tectonicPoints2 = GroupPointsByPlate(point2.tectonicPoints);

        foreach(var plate in tectonicPoints1)
        {
            int plateId = plate.Key;

            Vector3 center1 = GetCenterOfMass(plate.Value);
            if (!tectonicPoints2.TryGetValue(plateId, out List<Vector3> destPoints))
            {
                plateRotations[plateId] = Quaternion.identity;
                continue;
            }

            Vector3 center2 = GetCenterOfMass(destPoints);

            plateRotations[plateId] = Quaternion.FromToRotation(center1, center2);
            
        }
        
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
            sum += p;
        }
        return (sum / points.Count); 
    }

}
