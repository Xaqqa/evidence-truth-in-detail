using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC")]
public class NPC : ScriptableObject
{
    public string name;
    public bool guilty;
    public bool dead;
    public bool talkedTo;
    public string evidenceIdentifier;
    [TextArea(3, 20)] public string[] dialogue;
    [TextArea(3, 20)] public string[] playerDialogue;
}
