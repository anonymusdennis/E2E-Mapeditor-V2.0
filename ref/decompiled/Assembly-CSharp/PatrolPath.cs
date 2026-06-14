using System;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
	[Serializable]
	public class PathNode
	{
		[SerializeField]
		public Vector3 m_vNodePos;

		[SerializeField]
		public Quaternion m_FacingDirection = Quaternion.Euler(new Vector3(0f, 90f, 270f));

		[SerializeField]
		public bool m_bSetDirection;

		[SerializeField]
		public bool m_bRunToNode;

		[SerializeField]
		public float _m_fWaitVariance;

		[SerializeField]
		public float _m_fWaitTimer;

		[SerializeField]
		public SpeechPODO m_CharacterSpeech;

		public int m_iIndex = -1;

		public float m_fWaitVariance
		{
			get
			{
				return _m_fWaitVariance;
			}
			set
			{
				_m_fWaitVariance = Mathf.Max(0f, value);
				_m_fWaitVariance = Mathf.Min(_m_fWaitVariance, _m_fWaitTimer);
			}
		}

		public float m_fWaitTimer
		{
			get
			{
				return _m_fWaitTimer;
			}
			set
			{
				_m_fWaitTimer = Mathf.Max(0f, value);
				_m_fWaitVariance = Mathf.Min(_m_fWaitVariance, _m_fWaitTimer);
			}
		}

		public PathNode Clone()
		{
			PathNode pathNode = new PathNode();
			pathNode.m_vNodePos = new Vector3(m_vNodePos.x, m_vNodePos.y, m_vNodePos.z);
			pathNode.m_FacingDirection = new Quaternion(m_FacingDirection.x, m_FacingDirection.y, m_FacingDirection.z, m_FacingDirection.w);
			pathNode.m_bSetDirection = m_bSetDirection;
			pathNode.m_bRunToNode = m_bRunToNode;
			pathNode._m_fWaitVariance = _m_fWaitVariance;
			pathNode._m_fWaitTimer = _m_fWaitTimer;
			pathNode.m_iIndex = -1;
			pathNode.m_CharacterSpeech = new SpeechPODO(m_CharacterSpeech);
			return pathNode;
		}
	}

	[SerializeField]
	public PathNode[] m_vPathNodes = new PathNode[1];

	[SerializeField]
	public bool m_bBidirectional;

	[SerializeField]
	public bool m_bStartAtFirstWaypoint;

	[SerializeField]
	public Color m_DebugColor = Color.white;

	[SerializeField]
	public int m_Floor;

	[SerializeField]
	public bool m_bUseNavMesh = true;
}
