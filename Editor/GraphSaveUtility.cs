using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenDialogue.Editor
{
public class GraphSaveUtility
{
    private DialogueGraphView targetGraphView;
    private DialogueContainer containerCache;

    private List<Edge> Edges => targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(int instanceID)
    {
        var filename = AssetDatabase.GetAssetPath(instanceID);
        var asset = AssetDatabase.LoadAssetAtPath<DialogueContainer>(filename);
        asset.Clear();

        if (asset != null)
        {
            EditorUtility.SetDirty(asset);
        }
        else
        {
            asset = ScriptableObject.CreateInstance<DialogueContainer>();
        }

        if (!SaveNodes(asset)) return;
        SaveExposedProperties(asset);

        AssetDatabase.SaveAssets();
    }

    private void SaveExposedProperties(DialogueContainer dialogueContainer)
    {
        dialogueContainer.exposedProperties.AddRange(targetGraphView.exposedProperties);
    }

    private bool SaveNodes(DialogueContainer dialogueContainer)
    {
        if (!Edges.Any()) return false;

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

        for (var i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.nodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = outputNode.guid,
                PortName = connectedPorts[i].output.portName,
                TargetNodeGuid = inputNode.guid
            });
        }

        foreach (var dialogueNode in Nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogueNodeData
            {
                Guid = dialogueNode.guid,
                DialogueText = dialogueNode.dialogueText,
                Position = dialogueNode.GetPosition().position
            });
        }

        return true;
    }

    public void LoadGraph(int instanceID)
    {
        var filename = AssetDatabase.GetAssetPath(instanceID);
        Debug.Log(filename);
        LoadGraph(AssetDatabase.LoadAssetAtPath<DialogueContainer>(filename));
    }

    public void LoadGraph(DialogueContainer container)
    {
        containerCache = container;

        if (containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found!", "Target dialogue graph file does not exist!", "Ok");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        targetGraphView.ClearBlackboardAndExposedProperties();

        foreach (var exposedProperty in containerCache.exposedProperties)
        {
            targetGraphView.AddPropertyToBlackBoard(exposedProperty);
        }
    }

    private void ConnectNodes()
    {
        for (var i = 0; i < Nodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].guid).ToList();
            for (var j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.guid == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(
                    containerCache.dialogueNodeData.First(x => x.Guid == targetNodeGuid).Position,
                    targetGraphView.defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var nodeData in containerCache.dialogueNodeData)
        {
            var tempNode = targetGraphView.CreateDialogueNode(nodeData.DialogueText, Vector2.zero);
            tempNode.guid = nodeData.Guid;
            targetGraphView.AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(x => x.BaseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => targetGraphView.AddChoicePort(tempNode, x.PortName));
        }
    }

    private void ClearGraph()
    {
        //Set entry point guid back from the save. Discard existing guid.
        Nodes.Find(x => x.entryPoint).guid = containerCache.nodeLinks[0].BaseNodeGuid;

        foreach (var node in Nodes)
        {
            if (node.entryPoint) continue;

            // Remove edges that connected to this node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => targetGraphView.RemoveElement(edge));

            // Then remove the node
            targetGraphView.RemoveElement(node);
        }
    }
}
}
