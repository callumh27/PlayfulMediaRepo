using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EarthLandPainter))]
public class EarthPainterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EarthLandPainter painter = (EarthLandPainter)target;
        
        if (GUILayout.Button("Generate Blank Sphere Map"))
        {
            painter.GenerateBlankSphereRenderTexture();
        }
        if (GUILayout.Button("Convert To Asset"))
        {
            painter.ConvertToAsset();
        }
    }
}
