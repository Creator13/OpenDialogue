using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenDialogue.Editor
{
public class DialogueEditor : EditorWindow
{
    private DialogueGraphView graphView;

    public GUID SelectedGraphGuid { get; private set; }

    public void Init(GUID guid)
    {
        SelectedGraphGuid = guid;
        RequestDataOperation(false);
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
        GenerateBlackboard();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        toolbar.Add(new ToolbarButton(() => RequestDataOperation(true)) {text = "Save"});
        toolbar.Add(new Label($"{SelectedGraphGuid.ToString()}") {name = "currentInstanceId"});

        rootVisualElement.Add(toolbar);
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

    private void ConstructGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };

        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void RequestDataOperation(bool save)
    {
        var saveUtility = GraphSaveUtility.GetInstance(graphView);

        if (save)
        {
            saveUtility.SaveGraph(SelectedGraphGuid);
        }
        else
        {
            Debug.Log($"Loading {SelectedGraphGuid}");
            rootVisualElement.Q<Label>("currentInstanceId").text = $"{SelectedGraphGuid.ToString()}";
            saveUtility.LoadGraph(SelectedGraphGuid);
        }
    }
}
}
