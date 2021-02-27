using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenDialogue
{
[Serializable]
[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation")]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<DialogueNodeData> dialogueNodeData = new List<DialogueNodeData>();
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();

    public void Clear()
    {
        nodeLinks.Clear();
        dialogueNodeData.Clear();
        exposedProperties.Clear();
    }
}
}
