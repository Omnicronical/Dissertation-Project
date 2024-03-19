using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "BEAGSNode", menuName = "BEAGS/Node")]
[System.Serializable]
public class BEAGSNode : ScriptableObject
{
    public string Name;
    public GameObject Prefab;
    public int Entropy;
    public bool Rotated90;
    public bool Rotated180;
    public bool Rotated270;
    public bool HasBeenCollapsed;
    public WFCConnection Top;
    public WFCConnection Bottom;
    public WFCConnection Left;
    public WFCConnection Right;
    public GameObject InitialisedModel;
    
    
}

[System.Serializable]
public class WFCConnection
{
    public List<BEAGSNode> CompatibleNodes = new List<BEAGSNode>();
}