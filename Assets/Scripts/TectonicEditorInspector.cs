
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TectonicEditor))]
public class TectonicEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TectonicEditor editor = (TectonicEditor)target;
        if (GUILayout.Button("Generate Render Texture"))
        {
            editor.GenerateRenderTexture();
        }
        if (GUILayout.Button("Save Tectonic Map"))
        {
            
        }
    }
}

