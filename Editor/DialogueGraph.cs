using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace OpenDialogue.Editor
{
public class DialogueGraph : EditorWindow
{
    private DialogueGraphView graphView;
    private string filename = "New narrative";

    // [MenuItem("Graph/Dialogue")]
    // public static void OpenDialogueGraphWindow()
    // {
    //     var window = GetWindow<DialogueGraph>();
    //     window.titleContent = new GUIContent("Dialogue Graph");
    // }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (EditorUtility.InstanceIDToObject(instanceID) is DialogueContainer)
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
            window.graphView.graphAssetInstanceID = instanceID;

            window.RequestDataOperation(false);

            return true;
        }

        return false;
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
        GenerateBlackboard();
    }

    private void GenerateBlackboard()
    {
        var blackboard = new Blackboard(graphView);
        blackboard.Add(new BlackboardSection
        {
            title = "Exposed properties"
        });
        blackboard.SetPosition(new Rect(10, 30, 200, 300));

        blackboard.addItemRequested = thisBlackboard => { graphView.AddPropertyToBlackBoard(new ExposedProperty()); };
        blackboard.editTextRequested = (thisBlackboard, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField) element).text;
            if (graphView.exposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property already exists", "Ok");
                return;
            }

            var propertyIndex = graphView.exposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            graphView.exposedProperties[propertyIndex].PropertyName = newValue;
            ((BlackboardField) element).text = newValue;
        };

        graphView.Add(blackboard);
        graphView.blackboard = blackboard;
    }

    private void GenerateMinimap()
    {
        var miniMap = new MiniMap();
        const int width = 200;
        var coords = graphView.contentViewContainer.WorldToLocal(new Vector2(this.position.width - width - 10, 30));
        Debug.Log(coords);
        miniMap.SetPosition(new Rect(coords.x, coords.y, width, 140));
        graphView.Add(miniMap);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        toolbar.Add(new ToolbarButton(() => RequestDataOperation(true)) {text = "Save"});
        toolbar.Add(new Label($"{graphView.graphAssetInstanceID}") {name = "currentInstanceId"});

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(filename))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid filename.", "Ok");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(graphView);

        if (save)
        {
            saveUtility.SaveGraph(graphView.graphAssetInstanceID);
        }
        else
        {
            saveUtility.LoadGraph(graphView.graphAssetInstanceID);
            rootVisualElement.Q<Label>("currentInstanceId").text = $"{graphView.graphAssetInstanceID}";
        }
    }


    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
}
}
