using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class JobRoom_ContentsMarker : MonoBehaviour
{
	public enum ContentsType
	{
		Nothing,
		Dispencer,
		Processors,
		Collectors,
		Objects,
		Behaviour
	}

	public ContentsType m_ContentsType;

	public InteractiveObject m_InteractiveObject;

	public BehaviourTree m_Behaviour;
}
