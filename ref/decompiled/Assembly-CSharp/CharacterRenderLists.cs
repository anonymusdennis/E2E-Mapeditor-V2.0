using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRenderLists : MonoBehaviour
{
	private enum AVATAR_MATERIAL_CHANNELS
	{
		AMC_GENERIC,
		AMC_FACE_ACC,
		AMC_HAT,
		AMC_SHOVEL,
		AMC_HAMMER,
		AMC_MOP,
		AMC_TOOL,
		AMC_SWOOSH,
		AMC_SHADOW,
		AMC_NUM
	}

	[Serializable]
	public enum RENDER_TYPE
	{
		NOT_SET,
		CORE,
		ACTION,
		NUM
	}

	[Serializable]
	public enum RENDER_DIRECTION
	{
		ALL = -1,
		NOT_SET,
		UP,
		LEFT,
		DOWN,
		RIGHT
	}

	[Serializable]
	public class RenderersWithSettings
	{
		public MeshRenderer m_Renderer;

		public RENDER_TYPE m_AreaType;

		public int m_DirectionalMask = -1;
	}

	[Serializable]
	public class MaterialRenderers
	{
		public Material material;

		public Material preFabMaterial;

		public List<RenderersWithSettings> renderers;

		public MaterialRenderers()
		{
			renderers = new List<RenderersWithSettings>();
		}
	}

	public List<MaterialRenderers> m_MaterialRenderers = new List<MaterialRenderers>();

	private bool m_bInitedMaterials;

	public Dictionary<string, RENDER_TYPE> m_RenderTypes = new Dictionary<string, RENDER_TYPE>();

	public Dictionary<string, int> m_RenderDirs = new Dictionary<string, int>();

	[SerializeField]
	private string[] m_SaveDictKeys;

	[SerializeField]
	private RENDER_TYPE[] m_SaveDictValues;

	[SerializeField]
	private int[] m_SaveDictDirectionalMasks;

	public void RestoreData()
	{
		if (m_SaveDictKeys != null)
		{
			m_RenderTypes.Clear();
			m_RenderDirs.Clear();
			int num = m_SaveDictKeys.Length;
			for (int i = 0; i < num; i++)
			{
				m_RenderTypes[m_SaveDictKeys[i]] = m_SaveDictValues[i];
				m_RenderDirs[m_SaveDictKeys[i]] = m_SaveDictDirectionalMasks[i];
			}
		}
	}

	public void UpdateSave()
	{
		int count = m_RenderTypes.Count;
		m_SaveDictKeys = new string[count];
		m_SaveDictValues = new RENDER_TYPE[count];
		m_SaveDictDirectionalMasks = new int[count];
		int num = 0;
		foreach (string key in m_RenderTypes.Keys)
		{
			m_SaveDictKeys[num] = key;
			m_SaveDictValues[num] = m_RenderTypes[key];
			m_SaveDictDirectionalMasks[num] = m_RenderDirs[key];
			num++;
		}
	}

	public void InitMaterialSetting()
	{
		RestoreData();
		m_MaterialRenderers.Clear();
		MeshRenderer[] componentsInChildren = base.gameObject.transform.GetComponentsInChildren<MeshRenderer>();
		Dictionary<Material, List<MeshRenderer>> dictionary = new Dictionary<Material, List<MeshRenderer>>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material sharedMaterial = componentsInChildren[i].sharedMaterial;
			List<MeshRenderer> value = null;
			if (!(sharedMaterial == null))
			{
				dictionary.TryGetValue(sharedMaterial, out value);
				if (value == null)
				{
					value = new List<MeshRenderer>();
				}
				value.Add(componentsInChildren[i]);
				dictionary[sharedMaterial] = value;
			}
		}
		foreach (Material key in dictionary.Keys)
		{
			MaterialRenderers materialRenderers = new MaterialRenderers();
			materialRenderers.material = key;
			for (int j = 0; j < dictionary[key].Count; j++)
			{
				RenderersWithSettings renderersWithSettings = new RenderersWithSettings();
				renderersWithSettings.m_Renderer = dictionary[key][j];
				renderersWithSettings.m_AreaType = RENDER_TYPE.NOT_SET;
				if (m_RenderTypes.ContainsKey(renderersWithSettings.m_Renderer.name))
				{
					renderersWithSettings.m_AreaType = m_RenderTypes[renderersWithSettings.m_Renderer.name];
				}
				materialRenderers.renderers.Add(renderersWithSettings);
			}
			m_MaterialRenderers.Add(materialRenderers);
		}
		m_bInitedMaterials = true;
	}

	public void GetMeshRenderers(List<MeshRenderer> coreRenderers, List<MeshRenderer> actionRenderers)
	{
		MaterialRenderers materialRenderers = null;
		if (!m_bInitedMaterials)
		{
			InitMaterialSetting();
		}
		if (m_MaterialRenderers.Count < 9)
		{
			for (int i = 0; i < m_MaterialRenderers.Count; i++)
			{
				materialRenderers = m_MaterialRenderers[i];
				foreach (RenderersWithSettings renderer3 in materialRenderers.renderers)
				{
					MeshRenderer renderer = renderer3.m_Renderer;
					coreRenderers.Add(renderer);
				}
			}
			return;
		}
		for (int i = 0; i < 9; i++)
		{
			materialRenderers = m_MaterialRenderers[i];
			foreach (RenderersWithSettings renderer4 in materialRenderers.renderers)
			{
				MeshRenderer renderer2 = renderer4.m_Renderer;
				switch ((AVATAR_MATERIAL_CHANNELS)i)
				{
				case AVATAR_MATERIAL_CHANNELS.AMC_GENERIC:
				case AVATAR_MATERIAL_CHANNELS.AMC_FACE_ACC:
				case AVATAR_MATERIAL_CHANNELS.AMC_HAT:
				case AVATAR_MATERIAL_CHANNELS.AMC_SHADOW:
					coreRenderers.Add(renderer2);
					break;
				case AVATAR_MATERIAL_CHANNELS.AMC_SHOVEL:
				case AVATAR_MATERIAL_CHANNELS.AMC_HAMMER:
				case AVATAR_MATERIAL_CHANNELS.AMC_MOP:
				case AVATAR_MATERIAL_CHANNELS.AMC_TOOL:
				case AVATAR_MATERIAL_CHANNELS.AMC_SWOOSH:
					actionRenderers.Add(renderer2);
					break;
				}
			}
		}
	}
}
