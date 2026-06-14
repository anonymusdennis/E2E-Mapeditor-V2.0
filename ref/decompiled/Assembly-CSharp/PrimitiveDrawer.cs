using System.Collections.Generic;
using UnityEngine;

public class PrimitiveDrawer : MonoBehaviour
{
	private struct LineData
	{
		public Vector3 m_from;

		public Vector3 m_to;

		public Color m_fromColour;

		public Color m_toColour;

		public float m_expirationTime;

		public LineData(Vector3 from, Vector3 to, Color fromColour, Color toColour, float duration)
		{
			m_from = from;
			m_to = to;
			m_fromColour = fromColour;
			m_toColour = toColour;
			m_expirationTime = Time.time + duration;
		}
	}

	private struct TriangleData
	{
		public Vector3 m_pos1;

		public Vector3 m_pos2;

		public Vector3 m_pos3;

		public Color m_colour;

		public float m_expirationTime;

		public TriangleData(Vector3 pos1, Vector3 pos2, Vector3 pos3, Color colour, float duration)
		{
			m_pos1 = pos1;
			m_pos2 = pos2;
			m_pos3 = pos3;
			m_colour = colour;
			m_expirationTime = Time.time + duration;
		}
	}

	public bool m_renderInGame = true;

	public bool m_renderInEditor = true;

	private Material m_lineMaterial;

	private List<LineData> m_lines = new List<LineData>();

	private List<LineData> m_linesMultiFrame = new List<LineData>();

	private List<TriangleData> m_triangles = new List<TriangleData>();

	private List<TriangleData> m_trianglesMultiFrame = new List<TriangleData>();

	private bool m_pendingClear;

	public void AddLine(Vector3 from, Vector3 to, Color colour, float duration = 0f)
	{
		ClearCheck();
		if (duration > 0f)
		{
			m_linesMultiFrame.Add(new LineData(from, to, colour, colour, duration));
		}
		else
		{
			m_lines.Add(new LineData(from, to, colour, colour, duration));
		}
	}

	public void AddLine(Vector3 from, Vector3 to, Color fromColour, Color toColour, float duration = 0f)
	{
		ClearCheck();
		if (duration > 0f)
		{
			m_linesMultiFrame.Add(new LineData(from, to, fromColour, toColour, duration));
		}
		else
		{
			m_lines.Add(new LineData(from, to, fromColour, toColour, duration));
		}
	}

	public void AddTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Color colour, float duration = 0f)
	{
		ClearCheck();
		if (duration > 0f)
		{
			m_trianglesMultiFrame.Add(new TriangleData(pos1, pos2, pos3, colour, duration));
		}
		else
		{
			m_triangles.Add(new TriangleData(pos1, pos2, pos3, colour, duration));
		}
	}

	public void Render()
	{
		m_lineMaterial.SetPass(0);
		GL.Begin(1);
		foreach (LineData line in m_lines)
		{
			GL.Color(line.m_fromColour);
			GL.Vertex(line.m_from);
			if (line.m_fromColour != line.m_toColour)
			{
				GL.Color(line.m_toColour);
			}
			GL.Vertex(line.m_to);
		}
		foreach (LineData item in m_linesMultiFrame)
		{
			GL.Color(item.m_fromColour);
			GL.Vertex(item.m_from);
			if (item.m_fromColour != item.m_toColour)
			{
				GL.Color(item.m_toColour);
			}
			GL.Vertex(item.m_to);
		}
		GL.End();
		GL.Begin(4);
		foreach (TriangleData triangle in m_triangles)
		{
			GL.Color(triangle.m_colour);
			GL.Vertex(triangle.m_pos1);
			GL.Vertex(triangle.m_pos2);
			GL.Vertex(triangle.m_pos3);
		}
		foreach (TriangleData item2 in m_trianglesMultiFrame)
		{
			GL.Color(item2.m_colour);
			GL.Vertex(item2.m_pos1);
			GL.Vertex(item2.m_pos2);
			GL.Vertex(item2.m_pos3);
		}
		GL.End();
	}

	private void Start()
	{
		CreateLineMaterial();
	}

	private void CreateLineMaterial()
	{
		m_lineMaterial = Object.Instantiate(Resources.Load<Material>("Materials/PrimitiveDrawerLine"));
		if (m_lineMaterial != null)
		{
			m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void ClearRequired()
	{
		m_pendingClear = true;
	}

	private void ClearCheck()
	{
		if (m_pendingClear)
		{
			Clear();
			m_pendingClear = false;
		}
	}

	private void Clear()
	{
		m_lines.Clear();
		m_triangles.Clear();
		if (m_linesMultiFrame.Count > 0)
		{
			List<LineData> list = new List<LineData>(m_linesMultiFrame.Count);
			float time = Time.time;
			foreach (LineData item in m_linesMultiFrame)
			{
				if (time < item.m_expirationTime)
				{
					list.Add(item);
				}
			}
			m_linesMultiFrame = list;
		}
		if (m_trianglesMultiFrame.Count <= 0)
		{
			return;
		}
		List<TriangleData> list2 = new List<TriangleData>(m_trianglesMultiFrame.Count);
		float time2 = Time.time;
		foreach (TriangleData item2 in m_trianglesMultiFrame)
		{
			if (time2 < item2.m_expirationTime)
			{
				list2.Add(item2);
			}
		}
		m_trianglesMultiFrame = list2;
	}

	private void Update()
	{
		ClearCheck();
	}
}
