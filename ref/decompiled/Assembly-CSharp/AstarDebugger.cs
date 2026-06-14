using System;
using System.Text;
using Pathfinding;
using UnityEngine;

[HelpURL("http://arongranberg.com/astar/docs/class_astar_debugger.php")]
[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/Debugger")]
public class AstarDebugger : MonoBehaviour
{
	private struct GraphPoint
	{
		public float fps;

		public float memory;

		public bool collectEvent;
	}

	private struct PathTypeDebug
	{
		private string name;

		private Func<int> getSize;

		private Func<int> getTotalCreated;

		public PathTypeDebug(string name, Func<int> getSize, Func<int> getTotalCreated)
		{
			this.name = name;
			this.getSize = getSize;
			this.getTotalCreated = getTotalCreated;
		}

		public void Print(StringBuilder text)
		{
			int num = getTotalCreated();
			if (num > 0)
			{
				text.Append("\n").Append(("  " + name).PadRight(25)).Append(getSize())
					.Append("/")
					.Append(num);
			}
		}
	}

	public int yOffset = 5;

	public bool show = true;

	public bool showInEditor;

	public bool showFPS;

	public bool showPathProfile;

	public bool showMemProfile;

	public bool showGraph;

	public int graphBufferSize = 200;

	public Font font;

	public int fontSize = 12;

	private GraphPoint[] graph;

	private int allocMem;

	private int peakAlloc;

	private int fpsDropCounterSize = 200;

	private float[] fpsDrops;

	private Camera cam;

	private float graphWidth = 100f;

	private float graphHeight = 100f;

	private float graphOffset = 50f;

	public void Start()
	{
		base.useGUILayout = false;
		fpsDrops = new float[fpsDropCounterSize];
		cam = GetComponent<Camera>();
		if (cam == null)
		{
			cam = Camera.main;
		}
		graph = new GraphPoint[graphBufferSize];
		for (int i = 0; i < fpsDrops.Length; i++)
		{
			fpsDrops[i] = 1f / UpdateManager.deltaTime;
		}
	}

	public void Update()
	{
		if (!show || (!Application.isPlaying && !showInEditor))
		{
			return;
		}
		allocMem = (int)GC.GetTotalMemory(forceFullCollection: false);
		bool flag = allocMem < peakAlloc;
		peakAlloc = (flag ? peakAlloc : allocMem);
		if (Application.isPlaying)
		{
			int frameCount = UpdateManager.frameCount;
			fpsDrops[frameCount % fpsDrops.Length] = ((UpdateManager.deltaTime == 0f) ? float.PositiveInfinity : (1f / UpdateManager.deltaTime));
			int num = frameCount % graph.Length;
			graph[num].fps = ((!(UpdateManager.deltaTime < Mathf.Epsilon)) ? (1f / UpdateManager.deltaTime) : 0f);
			graph[num].collectEvent = flag;
			graph[num].memory = allocMem;
		}
		if (!Application.isPlaying || !(cam != null) || !showGraph)
		{
			return;
		}
		graphWidth = (float)cam.pixelWidth * 0.8f;
		float num2 = float.PositiveInfinity;
		float num3 = 0f;
		float num4 = float.PositiveInfinity;
		float num5 = 0f;
		for (int i = 0; i < graph.Length; i++)
		{
			num2 = Mathf.Min(graph[i].memory, num2);
			num3 = Mathf.Max(graph[i].memory, num3);
			num4 = Mathf.Min(graph[i].fps, num4);
			num5 = Mathf.Max(graph[i].fps, num5);
		}
		int num6 = UpdateManager.frameCount % graph.Length;
		Matrix4x4 m = Matrix4x4.TRS(new Vector3(((float)cam.pixelWidth - graphWidth) / 2f, graphOffset, 1f), Quaternion.identity, new Vector3(graphWidth, graphHeight, 1f));
		for (int j = 0; j < graph.Length - 1; j++)
		{
			if (j != num6)
			{
				DrawGraphLine(j, m, (float)j / (float)graph.Length, (float)(j + 1) / (float)graph.Length, AstarMath.MapTo(num2, num3, graph[j].memory), AstarMath.MapTo(num2, num3, graph[j + 1].memory), Color.blue);
				DrawGraphLine(j, m, (float)j / (float)graph.Length, (float)(j + 1) / (float)graph.Length, AstarMath.MapTo(num4, num5, graph[j].fps), AstarMath.MapTo(num4, num5, graph[j + 1].fps), Color.green);
			}
		}
	}

	public void DrawGraphLine(int index, Matrix4x4 m, float x1, float x2, float y1, float y2, Color col)
	{
		Debug.DrawLine(cam.ScreenToWorldPoint(m.MultiplyPoint3x4(new Vector3(x1, y1))), cam.ScreenToWorldPoint(m.MultiplyPoint3x4(new Vector3(x2, y2))), col);
	}

	public void Cross(Vector3 p)
	{
		p = cam.cameraToWorldMatrix.MultiplyPoint(p);
		Debug.DrawLine(p - Vector3.up * 0.2f, p + Vector3.up * 0.2f, Color.red);
		Debug.DrawLine(p - Vector3.right * 0.2f, p + Vector3.right * 0.2f, Color.red);
	}
}
