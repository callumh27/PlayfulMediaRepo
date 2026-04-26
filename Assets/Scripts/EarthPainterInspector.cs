using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EarthLandPainter))]
public class EarthPainterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EarthLandPainter painter = (EarthLandPainter)target;
        if (GUILayout.Button("Create New Earth Map"))
        {
            painter.CreateNewRenderTexture();
        }
        if (GUILayout.Button("Save Current Earth Map"))
        {
            painter.Save3DTexture();
        }
        if (GUILayout.Button("Generate Blank Sphere Map"))
        {
            painter.GenerateBlankSphereRenderTexture();
        }
        if (GUILayout.Button("Save Blank Earth Map"))
        {
            painter.SaveRenderTexture();
        }
    }
}
