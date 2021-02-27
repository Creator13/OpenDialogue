using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace OpenDialogue.Editor
{
public static class DialogueAssetUtility
{
    [OnOpenAsset(0)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (EditorUtility.InstanceIDToObject(instanceID) is DialogueContainer)
        {
            ShowEditorWindow(AssetDatabase.GetAssetPath(instanceID));
            return true;
        }

        return false;
    }

    private static void ShowEditorWindow(string assetPath)
    {
        var guid = AssetDatabase.GUIDFromAssetPath(assetPath);

        // Check if there already is a DialogueEditor for this asset and focus on it if that's the case.
        foreach (var w in Resources.FindObjectsOfTypeAll<DialogueEditor>())
        {
            if (w.SelectedGraphGuid == guid)
            {
                w.Focus();
                return;
            }
        }

        // Otherwise create a new window.
        var window = EditorWindow.CreateWindow<DialogueEditor>(typeof(DialogueEditor), typeof(SceneView));
        window.titleContent = new GUIContent("Dialogue Graph");
        window.Init(guid);
    }
}
}
