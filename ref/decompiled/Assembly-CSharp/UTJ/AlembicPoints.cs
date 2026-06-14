using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicPoints : AlembicElement
{
	private AbcAPI.aiPointsData m_abcData;

	private Vector3[] m_abcPositions;

	private Vector3[] m_abcVelocities;

	private ulong[] m_abcIDs;

	private AbcAPI.aiPointsSummary m_summary;

	public AbcAPI.aiPointsData abcData => m_abcData;

	public Vector3[] abcPositions => m_abcPositions;

	public ulong[] abcIDs => m_abcIDs;

	public int abcPeakVertexCount
	{
		get
		{
			if (m_summary.peakCount == 0)
			{
				AbcAPI.aiPointsGetSummary(m_abcSchema, ref m_summary);
			}
			return m_summary.peakCount;
		}
	}

	public override void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged)
	{
		if (m_abcPositions == null)
		{
			AbcAPI.aiPointsGetSummary(m_abcSchema, ref m_summary);
			m_abcPositions = new Vector3[m_summary.peakCount];
			m_abcIDs = new ulong[m_summary.peakCount];
			m_abcData.positions = Marshal.UnsafeAddrOfPinnedArrayElement(m_abcPositions, 0);
			m_abcData.ids = Marshal.UnsafeAddrOfPinnedArrayElement(m_abcIDs, 0);
			if (m_summary.hasVelocity)
			{
				m_abcVelocities = new Vector3[m_summary.peakCount];
				m_abcData.velocities = Marshal.UnsafeAddrOfPinnedArrayElement(m_abcVelocities, 0);
			}
		}
		AbcAPI.aiPointsCopyData(sample, ref m_abcData);
		AbcDirty();
	}

	public override void AbcUpdate()
	{
		if (AbcIsDirty())
		{
			AbcClean();
		}
	}

	private void Reset()
	{
		AlembicPointsRenderer component = base.gameObject.GetComponent<AlembicPointsRenderer>();
		if (component == null)
		{
			component = base.gameObject.AddComponent<AlembicPointsRenderer>();
		}
	}
}
