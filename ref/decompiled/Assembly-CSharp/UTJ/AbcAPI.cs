using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

public class AbcAPI
{
	public enum aiAspectRatioMode
	{
		CurrentResolution,
		DefaultResolution,
		CameraAperture
	}

	public enum aiAspectRatioModeOverride
	{
		InheritStreamSetting = -1,
		CurrentResolution,
		DefaultResolution,
		CameraAperture
	}

	public enum aiNormalsMode
	{
		ReadFromFile,
		ComputeIfMissing,
		AlwaysCompute,
		Ignore
	}

	public enum aiNormalsModeOverride
	{
		InheritStreamSetting = -1,
		ReadFromFile,
		ComputeIfMissing,
		AlwaysCompute,
		Ignore
	}

	public enum aiTangentsMode
	{
		None,
		Smooth,
		Split
	}

	public enum aiTangentsModeOverride
	{
		InheritStreamSetting = -1,
		None,
		Smooth,
		Split
	}

	public enum aiFaceWindingOverride
	{
		InheritStreamSetting = -1,
		Preserve,
		Swap
	}

	public enum aiTopologyVariance
	{
		Constant,
		Homogeneous,
		Heterogeneous
	}

	public enum aiPropertyType
	{
		Unknown = 0,
		Bool = 1,
		Int = 2,
		UInt = 3,
		Float = 4,
		Float2 = 5,
		Float3 = 6,
		Float4 = 7,
		Float4x4 = 8,
		BoolArray = 9,
		IntArray = 10,
		UIntArray = 11,
		FloatArray = 12,
		Float2Array = 13,
		Float3Array = 14,
		Float4Array = 15,
		Float4x4Array = 16,
		ScalarTypeBegin = 1,
		ScalarTypeEnd = 8,
		ArrayTypeBegin = 9,
		ArrayTypeEnd = 16
	}

	public enum aiTextureFormat
	{
		Unknown,
		ARGB32,
		ARGB2101010,
		RHalf,
		RGHalf,
		ARGBHalf,
		RFloat,
		RGFloat,
		ARGBFloat,
		RInt,
		RGInt,
		ARGBInt
	}

	public delegate void aiNodeEnumerator(aiObject obj, IntPtr userData);

	public delegate void aiConfigCallback(IntPtr _this, ref aiConfig config);

	public delegate void aiSampleCallback(IntPtr _this, aiSample sample, bool topologyChanged);

	public struct aiConfig
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool swapHandedness;

		[MarshalAs(UnmanagedType.U1)]
		public bool swapFaceWinding;

		[MarshalAs(UnmanagedType.U4)]
		public aiNormalsMode normalsMode;

		[MarshalAs(UnmanagedType.U4)]
		public aiTangentsMode tangentsMode;

		[MarshalAs(UnmanagedType.U1)]
		public bool cacheTangentsSplits;

		public float aspectRatio;

		[MarshalAs(UnmanagedType.U1)]
		public bool forceUpdate;

		[MarshalAs(UnmanagedType.U1)]
		public bool useThreads;

		[MarshalAs(UnmanagedType.U4)]
		public int cacheSamples;

		[MarshalAs(UnmanagedType.U1)]
		public bool submeshPerUVTile;

		public void SetDefaults()
		{
			swapHandedness = true;
			swapFaceWinding = false;
			normalsMode = aiNormalsMode.ComputeIfMissing;
			tangentsMode = aiTangentsMode.None;
			cacheTangentsSplits = true;
			aspectRatio = -1f;
			forceUpdate = false;
			useThreads = true;
			cacheSamples = 0;
			submeshPerUVTile = true;
		}
	}

	public struct aiSampleSelector
	{
		public ulong requestedIndex;

		public double requestedTime;

		public int requestedTimeIndexType;
	}

	public struct aiFacesets
	{
		[MarshalAs(UnmanagedType.U4)]
		public int count;

		public IntPtr faceCounts;

		public IntPtr faceIndices;
	}

	public struct aiMeshSummary
	{
		[MarshalAs(UnmanagedType.U4)]
		public aiTopologyVariance topologyVariance;

		public int peakVertexCount;

		public int peakIndexCount;

		public int peakTriangulatedIndexCount;

		public int peakSubmeshCount;
	}

	public struct aiMeshSampleSummary
	{
		[MarshalAs(UnmanagedType.U4)]
		public int splitCount;

		[MarshalAs(UnmanagedType.U1)]
		public bool hasNormals;

		[MarshalAs(UnmanagedType.U1)]
		public bool hasUVs;

		[MarshalAs(UnmanagedType.U1)]
		public bool hasTangents;
	}

	public struct aiPolyMeshData
	{
		public IntPtr positions;

		public IntPtr velocities;

		public IntPtr normals;

		public IntPtr uvs;

		public IntPtr tangents;

		public IntPtr indices;

		public IntPtr normalIndices;

		public IntPtr uvIndices;

		public IntPtr faces;

		public int positionCount;

		public int normalCount;

		public int uvCount;

		public int indexCount;

		public int normalIndexCount;

		public int uvIndexCount;

		public int faceCount;

		public int triangulatedIndexCount;

		public Vector3 center;

		public Vector3 size;
	}

	public struct aiSubmeshSummary
	{
		[MarshalAs(UnmanagedType.U4)]
		public int index;

		[MarshalAs(UnmanagedType.U4)]
		public int splitIndex;

		[MarshalAs(UnmanagedType.U4)]
		public int splitSubmeshIndex;

		[MarshalAs(UnmanagedType.U4)]
		public int facesetIndex;

		[MarshalAs(UnmanagedType.U4)]
		public int triangleCount;
	}

	public struct aiSubmeshData
	{
		public IntPtr indices;
	}

	public struct aiXFormData
	{
		public Vector3 translation;

		public Quaternion rotation;

		public Vector3 scale;

		[MarshalAs(UnmanagedType.U1)]
		public bool inherits;
	}

	public struct aiCameraData
	{
		public float nearClippingPlane;

		public float farClippingPlane;

		public float fieldOfView;

		public float aspectRatio;

		public float focusDistance;

		public float focalLength;

		public float aperture;
	}

	public struct aiContext
	{
		public IntPtr ptr;

		public static implicit operator bool(aiContext v)
		{
			return v.ptr != IntPtr.Zero;
		}
	}

	public struct aiObject
	{
		public IntPtr ptr;

		public static implicit operator bool(aiObject v)
		{
			return v.ptr != IntPtr.Zero;
		}
	}

	public struct aiSchema
	{
		public IntPtr ptr;

		public static implicit operator bool(aiSchema v)
		{
			return v.ptr != IntPtr.Zero;
		}
	}

	public struct aiProperty
	{
		public IntPtr ptr;

		public static implicit operator bool(aiProperty v)
		{
			return v.ptr != IntPtr.Zero;
		}
	}

	public struct aiSample
	{
		public IntPtr ptr;

		public static implicit operator bool(aiSample v)
		{
			return v.ptr != IntPtr.Zero;
		}
	}

	public struct aiPointsSummary
	{
		[MarshalAs(UnmanagedType.U1)]
		public bool hasVelocity;

		[MarshalAs(UnmanagedType.U1)]
		public bool positionIsConstant;

		[MarshalAs(UnmanagedType.U1)]
		public bool idIsConstant;

		public int peakCount;

		public ulong minID;

		public ulong maxID;

		public Vector3 boundsCenter;

		public Vector3 boundsExtents;
	}

	public struct aiPointsData
	{
		public IntPtr positions;

		public IntPtr velocities;

		public IntPtr ids;

		public int count;

		public Vector3 boundsCenter;

		public Vector3 boundsExtents;
	}

	private class ImportContext
	{
		public AlembicStream abcStream;

		public Transform parent;

		public aiSampleSelector ss;

		public bool createMissingNodes;

		public List<aiObject> objectsToDelete;
	}

	[DllImport("AlembicImporter")]
	public static extern aiSampleSelector aiTimeToSampleSelector(float time);

	[DllImport("AlembicImporter")]
	public static extern aiSampleSelector aiIndexToSampleSelector(int index);

	[DllImport("AlembicImporter")]
	public static extern void aiEnableFileLog(bool on, string path);

	[DllImport("AlembicImporter")]
	public static extern void aiCleanup();

	[DllImport("AlembicImporter")]
	public static extern aiContext aiCreateContext(int uid);

	[DllImport("AlembicImporter")]
	public static extern void aiDestroyContext(aiContext ctx);

	[DllImport("AlembicImporter")]
	public static extern bool aiLoad(aiContext ctx, string path);

	[DllImport("AlembicImporter")]
	public static extern void aiSetConfig(aiContext ctx, ref aiConfig conf);

	[DllImport("AlembicImporter")]
	public static extern float aiGetStartTime(aiContext ctx);

	[DllImport("AlembicImporter")]
	public static extern float aiGetEndTime(aiContext ctx);

	[DllImport("AlembicImporter")]
	public static extern aiObject aiGetTopObject(aiContext ctx);

	[DllImport("AlembicImporter")]
	public static extern void aiDestroyObject(aiContext ctx, aiObject obj);

	[DllImport("AlembicImporter")]
	public static extern void aiUpdateSamples(aiContext ctx, float time);

	[DllImport("AlembicImporter")]
	public static extern void aiUpdateSamplesBegin(aiContext ctx, float time);

	[DllImport("AlembicImporter")]
	public static extern void aiUpdateSamplesEnd(aiContext ctx);

	[DllImport("AlembicImporter")]
	public static extern void aiEnumerateChild(aiObject obj, aiNodeEnumerator e, IntPtr userData);

	[DllImport("AlembicImporter")]
	private static extern IntPtr aiGetNameS(aiObject obj);

	[DllImport("AlembicImporter")]
	private static extern IntPtr aiGetFullNameS(aiObject obj);

	public static string aiGetName(aiObject obj)
	{
		return Marshal.PtrToStringAnsi(aiGetNameS(obj));
	}

	public static string aiGetFullName(aiObject obj)
	{
		return Marshal.PtrToStringAnsi(aiGetFullNameS(obj));
	}

	[DllImport("AlembicImporter")]
	public static extern void aiSchemaSetSampleCallback(aiSchema schema, aiSampleCallback cb, IntPtr arg);

	[DllImport("AlembicImporter")]
	public static extern void aiSchemaSetConfigCallback(aiSchema schema, aiConfigCallback cb, IntPtr arg);

	[DllImport("AlembicImporter")]
	public static extern aiSample aiSchemaUpdateSample(aiSchema schema, ref aiSampleSelector ss);

	[DllImport("AlembicImporter")]
	public static extern aiSample aiSchemaGetSample(aiSchema schema, ref aiSampleSelector ss);

	[DllImport("AlembicImporter")]
	public static extern aiSchema aiGetXForm(aiObject obj);

	[DllImport("AlembicImporter")]
	public static extern bool aiXFormGetData(aiSample sample, ref aiXFormData data);

	[DllImport("AlembicImporter")]
	public static extern aiSchema aiGetPolyMesh(aiObject obj);

	[DllImport("AlembicImporter")]
	public static extern void aiPolyMeshGetSummary(aiSchema schema, ref aiMeshSummary summary);

	[DllImport("AlembicImporter")]
	public static extern void aiPolyMeshGetSampleSummary(aiSample sample, ref aiMeshSampleSummary summary, bool forceRefresh);

	[DllImport("AlembicImporter")]
	public static extern int aiPolyMeshGetVertexBufferLength(aiSample sample, int splitIndex);

	[DllImport("AlembicImporter")]
	public static extern void aiPolyMeshFillVertexBuffer(aiSample sample, int splitIndex, ref aiPolyMeshData data);

	[DllImport("AlembicImporter")]
	public static extern int aiPolyMeshPrepareSubmeshes(aiSample sample, ref aiFacesets facesets);

	[DllImport("AlembicImporter")]
	public static extern int aiPolyMeshGetSplitSubmeshCount(aiSample sample, int splitIndex);

	[DllImport("AlembicImporter")]
	public static extern bool aiPolyMeshGetNextSubmesh(aiSample sample, ref aiSubmeshSummary smi);

	[DllImport("AlembicImporter")]
	public static extern void aiPolyMeshFillSubmeshIndices(aiSample sample, ref aiSubmeshSummary smi, ref aiSubmeshData data);

	[DllImport("AlembicImporter")]
	public static extern aiSchema aiGetCamera(aiObject obj);

	[DllImport("AlembicImporter")]
	public static extern void aiCameraGetData(aiSample sample, ref aiCameraData data);

	[DllImport("AlembicImporter")]
	public static extern aiSchema aiGetPoints(aiObject obj);

	[DllImport("AlembicImporter")]
	public static extern void aiPointsGetSummary(aiSchema schema, ref aiPointsSummary summary);

	[DllImport("AlembicImporter")]
	public static extern void aiPointsCopyData(aiSample sample, ref aiPointsData data);

	public static float GetAspectRatio(aiAspectRatioMode mode)
	{
		return mode switch
		{
			aiAspectRatioMode.CameraAperture => 0f, 
			aiAspectRatioMode.CurrentResolution => (float)Screen.width / (float)Screen.height, 
			_ => (float)Screen.width / (float)Screen.height, 
		};
	}

	public static void UpdateAbcTree(aiContext ctx, Transform root, float time, bool createMissingNodes = false)
	{
		ImportContext importContext = new ImportContext();
		importContext.abcStream = root.GetComponent<AlembicStream>();
		importContext.parent = root;
		importContext.ss = aiTimeToSampleSelector(time);
		importContext.createMissingNodes = createMissingNodes;
		importContext.objectsToDelete = new List<aiObject>();
		GCHandle value = GCHandle.Alloc(importContext);
		aiObject obj = aiGetTopObject(ctx);
		if (!(obj.ptr != (IntPtr)0))
		{
			return;
		}
		aiEnumerateChild(obj, ImportEnumerator, GCHandle.ToIntPtr(value));
		foreach (aiObject item in importContext.objectsToDelete)
		{
			aiDestroyObject(ctx, item);
		}
	}

	private static void ImportEnumerator(aiObject obj, IntPtr userData)
	{
		ImportContext importContext = GCHandle.FromIntPtr(userData).Target as ImportContext;
		Transform parent = importContext.parent;
		string name = aiGetName(obj);
		Transform transform = parent.FindChild(name);
		if (transform == null)
		{
			if (!importContext.createMissingNodes)
			{
				importContext.objectsToDelete.Add(obj);
				return;
			}
			GameObject gameObject = new GameObject();
			gameObject.name = name;
			transform = gameObject.GetComponent<Transform>();
			transform.parent = parent;
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = Vector3.zero;
			transform.localScale = Vector3.one;
		}
		AlembicElement alembicElement = null;
		aiSchema aiSchema = default(aiSchema);
		if ((bool)aiGetXForm(obj))
		{
			alembicElement = GetOrAddComponent<AlembicXForm>(transform.gameObject);
			aiSchema = aiGetXForm(obj);
		}
		else if ((bool)aiGetPolyMesh(obj))
		{
			alembicElement = GetOrAddComponent<AlembicMesh>(transform.gameObject);
			aiSchema = aiGetPolyMesh(obj);
		}
		else if ((bool)aiGetCamera(obj))
		{
			alembicElement = GetOrAddComponent<AlembicCamera>(transform.gameObject);
			aiSchema = aiGetCamera(obj);
		}
		else if ((bool)aiGetPoints(obj))
		{
			alembicElement = GetOrAddComponent<AlembicPoints>(transform.gameObject);
			aiSchema = aiGetPoints(obj);
		}
		if ((bool)alembicElement)
		{
			alembicElement.AbcSetup(importContext.abcStream, obj, aiSchema);
			aiSchemaUpdateSample(aiSchema, ref importContext.ss);
			alembicElement.AbcUpdate();
			importContext.abcStream.AbcAddElement(alembicElement);
		}
		importContext.parent = transform;
		aiEnumerateChild(obj, ImportEnumerator, userData);
		importContext.parent = parent;
	}

	public static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}
}
