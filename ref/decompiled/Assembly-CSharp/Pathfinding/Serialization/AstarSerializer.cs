using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pathfinding.Ionic.Zip;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.Serialization;

public class AstarSerializer
{
	private AstarData data;

	private ZipFile zip;

	private MemoryStream zipStream;

	private GraphMeta meta;

	private SerializeSettings settings;

	private NavGraph[] graphs;

	private Dictionary<NavGraph, int> graphIndexInZip;

	private int graphIndexOffset;

	private const string binaryExt = ".binary";

	private const string jsonExt = ".json";

	private uint checksum = uint.MaxValue;

	private UTF8Encoding encoding = new UTF8Encoding();

	private static StringBuilder _stringBuilder = new StringBuilder();

	public AstarSerializer(AstarData data)
	{
		this.data = data;
		settings = SerializeSettings.Settings;
	}

	public AstarSerializer(AstarData data, SerializeSettings settings)
	{
		this.data = data;
		this.settings = settings;
	}

	private static StringBuilder GetStringBuilder()
	{
		_stringBuilder.Length = 0;
		return _stringBuilder;
	}

	private static void CloseOrDispose(BinaryWriter writer)
	{
		writer.Close();
	}

	public void SetGraphIndexOffset(int offset)
	{
		graphIndexOffset = offset;
	}

	private void AddChecksum(byte[] bytes)
	{
		checksum = Checksum.GetChecksum(bytes, checksum);
	}

	public uint GetChecksum()
	{
		return checksum;
	}

	public void OpenSerialize()
	{
		zip = new ZipFile();
		zip.AlternateEncoding = Encoding.UTF8;
		zip.AlternateEncodingUsage = ZipOption.Always;
		meta = new GraphMeta();
	}

	public byte[] CloseSerialize()
	{
		byte[] array = SerializeMeta();
		AddChecksum(array);
		zip.AddEntry("meta.json", array);
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		foreach (ZipEntry entry in zip.Entries)
		{
			entry.AccessedTime = dateTime;
			entry.CreationTime = dateTime;
			entry.LastModified = dateTime;
			entry.ModifiedTime = dateTime;
		}
		MemoryStream memoryStream = new MemoryStream();
		zip.Save(memoryStream);
		array = memoryStream.ToArray();
		memoryStream.Dispose();
		zip.Dispose();
		zip = null;
		return array;
	}

	public void SerializeGraphs(NavGraph[] _graphs)
	{
		if (graphs != null)
		{
			throw new InvalidOperationException("Cannot serialize graphs multiple times.");
		}
		graphs = _graphs;
		if (zip == null)
		{
			throw new NullReferenceException("You must not call CloseSerialize before a call to this function");
		}
		if (graphs == null)
		{
			graphs = new NavGraph[0];
		}
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				byte[] array = Serialize(graphs[i]);
				AddChecksum(array);
				zip.AddEntry("graph" + i + ".json", array);
			}
		}
	}

	private byte[] SerializeMeta()
	{
		if (graphs == null)
		{
			throw new Exception("No call to SerializeGraphs has been done");
		}
		meta.version = AstarPath.Version;
		meta.graphs = graphs.Length;
		meta.guids = new List<string>();
		meta.typeNames = new List<string>();
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				meta.guids.Add(graphs[i].guid.ToString());
				meta.typeNames.Add(graphs[i].GetType().FullName);
			}
			else
			{
				meta.guids.Add(null);
				meta.typeNames.Add(null);
			}
		}
		StringBuilder stringBuilder = GetStringBuilder();
		TinyJsonSerializer.Serialize(meta, stringBuilder);
		return encoding.GetBytes(stringBuilder.ToString());
	}

	public byte[] Serialize(NavGraph graph)
	{
		StringBuilder stringBuilder = GetStringBuilder();
		TinyJsonSerializer.Serialize(graph, stringBuilder);
		return encoding.GetBytes(stringBuilder.ToString());
	}

	[Obsolete("Not used anymore. You can safely remove the call to this function.")]
	public void SerializeNodes()
	{
	}

	private static int GetMaxNodeIndexInAllGraphs(NavGraph[] graphs)
	{
		int maxIndex = 0;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] == null)
			{
				continue;
			}
			graphs[i].GetNodes(delegate(GraphNode node)
			{
				maxIndex = Math.Max(node.NodeIndex, maxIndex);
				if (node.NodeIndex == -1)
				{
					Debug.LogError("Graph contains destroyed nodes. This is a bug.");
				}
				return true;
			});
		}
		return maxIndex;
	}

	private static byte[] SerializeNodeIndices(NavGraph[] graphs)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter wr = new BinaryWriter(memoryStream);
		int maxNodeIndexInAllGraphs = GetMaxNodeIndexInAllGraphs(graphs);
		wr.Write(maxNodeIndexInAllGraphs);
		int maxNodeIndex2 = 0;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				graphs[i].GetNodes(delegate(GraphNode node)
				{
					maxNodeIndex2 = Math.Max(node.NodeIndex, maxNodeIndex2);
					wr.Write(node.NodeIndex);
					return true;
				});
			}
		}
		if (maxNodeIndex2 != maxNodeIndexInAllGraphs)
		{
			throw new Exception("Some graphs are not consistent in their GetNodes calls, sequential calls give different results.");
		}
		byte[] result = memoryStream.ToArray();
		CloseOrDispose(wr);
		return result;
	}

	private static byte[] SerializeGraphExtraInfo(NavGraph graph)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		GraphSerializationContext ctx = new GraphSerializationContext(writer);
		graph.SerializeExtraInfo(ctx);
		byte[] result = memoryStream.ToArray();
		CloseOrDispose(writer);
		return result;
	}

	private static byte[] SerializeGraphNodeReferences(NavGraph graph)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		GraphSerializationContext ctx = new GraphSerializationContext(writer);
		graph.GetNodes(delegate(GraphNode node)
		{
			node.SerializeReferences(ctx);
			return true;
		});
		CloseOrDispose(writer);
		return memoryStream.ToArray();
	}

	public void SerializeExtraInfo()
	{
		if (!settings.nodes)
		{
			return;
		}
		if (graphs == null)
		{
			throw new InvalidOperationException("Cannot serialize extra info with no serialized graphs (call SerializeGraphs first)");
		}
		byte[] array = SerializeNodeIndices(graphs);
		AddChecksum(array);
		zip.AddEntry("graph_references.binary", array);
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] != null)
			{
				array = SerializeGraphExtraInfo(graphs[i]);
				AddChecksum(array);
				zip.AddEntry("graph" + i + "_extra.binary", array);
				array = SerializeGraphNodeReferences(graphs[i]);
				AddChecksum(array);
				zip.AddEntry("graph" + i + "_references.binary", array);
			}
		}
		array = SerializeNodeLinks();
		AddChecksum(array);
		zip.AddEntry("node_link2.binary", array);
	}

	private byte[] SerializeNodeLinks()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		GraphSerializationContext ctx = new GraphSerializationContext(writer);
		NodeLink2.SerializeReferences(ctx);
		return memoryStream.ToArray();
	}

	public void SerializeEditorSettings(GraphEditorBase[] editors)
	{
		if (editors == null || !settings.editorSettings)
		{
			return;
		}
		for (int i = 0; i < editors.Length && editors[i] != null; i++)
		{
			StringBuilder stringBuilder = GetStringBuilder();
			TinyJsonSerializer.Serialize(editors[i], stringBuilder);
			byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
			if (bytes.Length > 2)
			{
				AddChecksum(bytes);
				zip.AddEntry("graph" + i + "_editor.json", bytes);
			}
		}
	}

	public bool OpenDeserialize(byte[] bytes)
	{
		zipStream = new MemoryStream();
		zipStream.Write(bytes, 0, bytes.Length);
		zipStream.Position = 0L;
		try
		{
			zip = ZipFile.Read(zipStream);
		}
		catch (Exception ex)
		{
			Debug.LogError("Caught exception when loading from zip\n" + ex);
			zipStream.Dispose();
			return false;
		}
		if (zip.ContainsEntry("meta.json"))
		{
			meta = DeserializeMeta(zip["meta.json"]);
		}
		else
		{
			if (!zip.ContainsEntry("meta.binary"))
			{
				throw new Exception("No metadata found in serialized data.");
			}
			meta = DeserializeBinaryMeta(zip["meta.binary"]);
		}
		if (FullyDefinedVersion(meta.version) > FullyDefinedVersion(AstarPath.Version))
		{
			Debug.LogWarning(string.Concat("Trying to load data from a newer version of the A* Pathfinding Project\nCurrent version: ", AstarPath.Version, " Data version: ", meta.version, "\nThis is usually fine as the stored data is usually backwards and forwards compatible.\nHowever node data (not settings) can get corrupted between versions (even though I try my best to keep compatibility), so it is recommended to recalculate any caches (those for faster startup) and resave any files. Even if it seems to load fine, it might cause subtle bugs.\n"));
		}
		else if (FullyDefinedVersion(meta.version) < FullyDefinedVersion(AstarPath.Version))
		{
			Debug.LogWarning(string.Concat("Trying to load data from an older version of the A* Pathfinding Project\nCurrent version: ", AstarPath.Version, " Data version: ", meta.version, "\nThis is usually fine, it just means you have upgraded to a new version.\nHowever node data (not settings) can get corrupted between versions (even though I try my best to keep compatibility), so it is recommended to recalculate any caches (those for faster startup) and resave any files. Even if it seems to load fine, it might cause subtle bugs.\n"));
		}
		return true;
	}

	private static Version FullyDefinedVersion(Version v)
	{
		return new Version(Mathf.Max(v.Major, 0), Mathf.Max(v.Minor, 0), Mathf.Max(v.Build, 0), Mathf.Max(v.Revision, 0));
	}

	public void CloseDeserialize()
	{
		zipStream.Dispose();
		zip.Dispose();
		zip = null;
		zipStream = null;
	}

	private NavGraph DeserializeGraph(int zipIndex, int graphIndex)
	{
		Type graphType = meta.GetGraphType(zipIndex);
		if (object.Equals(graphType, null))
		{
			return null;
		}
		NavGraph navGraph = data.CreateGraph(graphType);
		navGraph.graphIndex = (uint)graphIndex;
		string text = "graph" + zipIndex + ".json";
		string text2 = "graph" + zipIndex + ".binary";
		if (zip.ContainsEntry(text))
		{
			TinyJsonDeserializer.Deserialize(GetString(zip[text]), graphType, navGraph);
		}
		else
		{
			if (!zip.ContainsEntry(text2))
			{
				throw new FileNotFoundException("Could not find data for graph " + zipIndex + " in zip. Entry 'graph" + zipIndex + ".json' does not exist");
			}
			BinaryReader binaryReader = GetBinaryReader(zip[text2]);
			GraphSerializationContext ctx = new GraphSerializationContext(binaryReader, null, navGraph.graphIndex, meta);
			navGraph.DeserializeSettingsCompatibility(ctx);
		}
		if (navGraph.guid.ToString() != meta.guids[zipIndex])
		{
			throw new Exception(string.Concat("Guid in graph file not equal to guid defined in meta file. Have you edited the data manually?\n", navGraph.guid, " != ", meta.guids[zipIndex]));
		}
		return navGraph;
	}

	public NavGraph[] DeserializeGraphs()
	{
		List<NavGraph> list = new List<NavGraph>();
		graphIndexInZip = new Dictionary<NavGraph, int>();
		for (int i = 0; i < meta.graphs; i++)
		{
			int graphIndex = list.Count + graphIndexOffset;
			NavGraph navGraph = DeserializeGraph(i, graphIndex);
			if (navGraph != null)
			{
				list.Add(navGraph);
				graphIndexInZip[navGraph] = i;
			}
		}
		graphs = list.ToArray();
		return graphs;
	}

	private bool DeserializeExtraInfo(NavGraph graph)
	{
		int num = graphIndexInZip[graph];
		ZipEntry zipEntry = zip["graph" + num + "_extra.binary"];
		if (zipEntry == null)
		{
			return false;
		}
		BinaryReader binaryReader = GetBinaryReader(zipEntry);
		GraphSerializationContext ctx = new GraphSerializationContext(binaryReader, null, graph.graphIndex, meta);
		graph.DeserializeExtraInfo(ctx);
		return true;
	}

	private bool AnyDestroyedNodesInGraphs()
	{
		bool result = false;
		for (int i = 0; i < graphs.Length; i++)
		{
			graphs[i].GetNodes(delegate(GraphNode node)
			{
				if (node.Destroyed)
				{
					result = true;
				}
				return true;
			});
		}
		return result;
	}

	private GraphNode[] DeserializeNodeReferenceMap()
	{
		ZipEntry zipEntry = zip["graph_references.binary"];
		if (zipEntry == null)
		{
			throw new Exception("Node references not found in the data. Was this loaded from an older version of the A* Pathfinding Project?");
		}
		BinaryReader reader = GetBinaryReader(zipEntry);
		int num = reader.ReadInt32();
		GraphNode[] int2Node = new GraphNode[num + 1];
		try
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				graphs[i].GetNodes(delegate(GraphNode node)
				{
					int num2 = reader.ReadInt32();
					int2Node[num2] = node;
					return true;
				});
			}
		}
		catch (Exception innerException)
		{
			throw new Exception("Some graph(s) has thrown an exception during GetNodes, or some graph(s) have deserialized more or fewer nodes than were serialized", innerException);
		}
		if (reader.BaseStream.Position != reader.BaseStream.Length)
		{
			throw new Exception(reader.BaseStream.Length / 4 + " nodes were serialized, but only data for " + reader.BaseStream.Position / 4 + " nodes was found. The data looks corrupt.");
		}
		reader.Close();
		return int2Node;
	}

	private void DeserializeNodeReferences(NavGraph graph, GraphNode[] int2Node)
	{
		int num = graphIndexInZip[graph];
		ZipEntry zipEntry = zip["graph" + num + "_references.binary"];
		if (zipEntry == null)
		{
			throw new Exception("Node references for graph " + num + " not found in the data. Was this loaded from an older version of the A* Pathfinding Project?");
		}
		BinaryReader binaryReader = GetBinaryReader(zipEntry);
		GraphSerializationContext ctx = new GraphSerializationContext(binaryReader, int2Node, graph.graphIndex, meta);
		graph.GetNodes(delegate(GraphNode node)
		{
			node.DeserializeReferences(ctx);
			return true;
		});
	}

	public void DeserializeExtraInfo()
	{
		bool flag = false;
		for (int i = 0; i < graphs.Length; i++)
		{
			flag |= DeserializeExtraInfo(graphs[i]);
		}
		if (flag)
		{
			if (AnyDestroyedNodesInGraphs())
			{
				Debug.LogError("Graph contains destroyed nodes. This is a bug.");
			}
			GraphNode[] int2Node = DeserializeNodeReferenceMap();
			for (int j = 0; j < graphs.Length; j++)
			{
				DeserializeNodeReferences(graphs[j], int2Node);
			}
			DeserializeNodeLinks(int2Node);
		}
	}

	private void DeserializeNodeLinks(GraphNode[] int2Node)
	{
		ZipEntry zipEntry = zip["node_link2.binary"];
		if (zipEntry != null)
		{
			BinaryReader binaryReader = GetBinaryReader(zipEntry);
			GraphSerializationContext ctx = new GraphSerializationContext(binaryReader, int2Node, 0u, meta);
			NodeLink2.DeserializeReferences(ctx);
		}
	}

	public void PostDeserialization()
	{
		for (int i = 0; i < graphs.Length; i++)
		{
			graphs[i].PostDeserialization();
		}
	}

	public void DeserializeEditorSettings(GraphEditorBase[] graphEditors)
	{
		if (graphEditors == null)
		{
			return;
		}
		for (int i = 0; i < graphEditors.Length; i++)
		{
			if (graphEditors[i] == null)
			{
				continue;
			}
			for (int j = 0; j < graphs.Length; j++)
			{
				if (graphEditors[i].target == graphs[j])
				{
					int num = graphIndexInZip[graphs[j]];
					ZipEntry zipEntry = zip["graph" + num + "_editor.json"];
					if (zipEntry != null)
					{
						TinyJsonDeserializer.Deserialize(GetString(zipEntry), graphEditors[i].GetType(), graphEditors[i]);
						break;
					}
				}
			}
		}
	}

	private static BinaryReader GetBinaryReader(ZipEntry entry)
	{
		MemoryStream memoryStream = new MemoryStream();
		entry.Extract(memoryStream);
		memoryStream.Position = 0L;
		return new BinaryReader(memoryStream);
	}

	private static string GetString(ZipEntry entry)
	{
		MemoryStream memoryStream = new MemoryStream();
		entry.Extract(memoryStream);
		memoryStream.Position = 0L;
		StreamReader streamReader = new StreamReader(memoryStream);
		string result = streamReader.ReadToEnd();
		memoryStream.Position = 0L;
		streamReader.Dispose();
		return result;
	}

	private GraphMeta DeserializeMeta(ZipEntry entry)
	{
		return TinyJsonDeserializer.Deserialize(GetString(entry), typeof(GraphMeta)) as GraphMeta;
	}

	private GraphMeta DeserializeBinaryMeta(ZipEntry entry)
	{
		GraphMeta graphMeta = new GraphMeta();
		BinaryReader binaryReader = GetBinaryReader(entry);
		if (binaryReader.ReadString() != "A*")
		{
			throw new Exception("Invalid magic number in saved data");
		}
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		int num3 = binaryReader.ReadInt32();
		int num4 = binaryReader.ReadInt32();
		if (num < 0)
		{
			graphMeta.version = new Version(0, 0);
		}
		else if (num2 < 0)
		{
			graphMeta.version = new Version(num, 0);
		}
		else if (num3 < 0)
		{
			graphMeta.version = new Version(num, num2);
		}
		else if (num4 < 0)
		{
			graphMeta.version = new Version(num, num2, num3);
		}
		else
		{
			graphMeta.version = new Version(num, num2, num3, num4);
		}
		graphMeta.graphs = binaryReader.ReadInt32();
		graphMeta.guids = new List<string>();
		int num5 = binaryReader.ReadInt32();
		for (int i = 0; i < num5; i++)
		{
			graphMeta.guids.Add(binaryReader.ReadString());
		}
		graphMeta.typeNames = new List<string>();
		num5 = binaryReader.ReadInt32();
		for (int j = 0; j < num5; j++)
		{
			graphMeta.typeNames.Add(binaryReader.ReadString());
		}
		return graphMeta;
	}

	public static void SaveToFile(string path, byte[] data)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Create);
		fileStream.Write(data, 0, data.Length);
	}

	public static byte[] LoadFromFile(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open);
		byte[] array = new byte[(int)fileStream.Length];
		fileStream.Read(array, 0, (int)fileStream.Length);
		return array;
	}
}
