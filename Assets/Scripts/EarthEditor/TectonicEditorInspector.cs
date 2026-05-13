
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TectonicEditor))]
public class TectonicEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TectonicEditor editor = (TectonicEditor)target;
        if (GUILayout.Button("Generate Blank Tectonic Map"))
        {
            editor.GenerateRenderTexture();
        }
        if (GUILayout.Button("Update Tectonic Texture"))
        {
            editor.UpdateTectonicLookupTexture();
        }
        if (GUILayout.Button("Update Plate Boundaries"))
        {
            editor.UpdatePlateBoundaries();
        }
        if (GUILayout.Button("Save Tectonic Map"))
        {
            
        }
    }
}

