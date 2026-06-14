using System;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "ObjectiveBehaviourRefs", menuName = "Team17/Objectives/RuntimeBehaviours")]
public class RuntimeBehaviours : ScriptableObject
{
	public List<BehaviourTree> m_Behaviours = new List<BehaviourTree>();
}
