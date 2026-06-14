using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterStencilRenderer : MonoBehaviour
{
	private class CharacterSubMesh
	{
		public SkinnedMeshRenderer skin;

		public int subMeshIndex;

		public Material material;
	}

	public Shader m_Shader;

	public Shader m_AccessoryShader;

	public Mesh m_ClearPlane;

	public Material m_ClearMaterial;

	private Vector3 m_ClearQuadPos = new Vector3(0f, 0.353f, 0f);

	private Vector3 m_ClearQuadScale = new Vector3(1.8f, 1.4f, 1f);

	public bool m_isActive;

	private CameraManager m_CameraManager;

	private FloorManager m_FloorManager;

	public static int ms_CameraIndex = 0;

	public static int ms_CameraCount = 0;

	private Camera m_Camera;

	private CommandBuffer m_CommandBuffer;

	private RenderTexture m_StencilBuffer;

	private int m_StencilRenderTargetID;

	private int m_TextureVersion;

	private Vector2 m_StencilCamPos = Vector2.zero;

	private static Material m_Material = null;

	private static Material m_AccessoryMaterial = null;

	private static bool m_bMaterialsCreated = false;

	private static bool m_bInFullClearMode = true;

	private static int m_RendereredStencilCount = 0;

	private const int kLastCloseFrameCheckDelay = 5;

	public static StencilInterfaceComparer StencilTComparer = default(StencilInterfaceComparer);

	private static Dictionary<StencilInterface, FastList<MeshRenderer>> m_CharacterMappings = new Dictionary<StencilInterface, FastList<MeshRenderer>>(StencilTComparer);

	private static Dictionary<StencilInterface, FastList<MeshRenderer>> m_CharacterAccMappings = new Dictionary<StencilInterface, FastList<MeshRenderer>>(StencilTComparer);

	private static Dictionary<StencilInterface, FastList<CharacterSubMesh>> m_CharacterSkinnedMap = new Dictionary<StencilInterface, FastList<CharacterSubMesh>>(StencilTComparer);

	private static FastList<StencilInterface> m_CharacterKeys = new FastList<StencilInterface>();

	private static bool m_bInitedCharacterKeys = false;

	private static int m_LastFrameKeysWereSorted = -1;

	public bool m_bIsDirty { get; private set; }

	private void Awake()
	{
		m_LastFrameKeysWereSorted = -1;
		m_RendereredStencilCount = 0;
		ms_CameraIndex = 0;
		ms_CameraCount = 0;
		m_Camera = GetComponentInParent<Camera>();
		if (!m_bMaterialsCreated)
		{
			CreateMaterials();
		}
		if (m_ClearPlane == null)
		{
			m_ClearPlane = GlobalStart.GetInstance().m_StencilClearQuad;
		}
		if (m_ClearMaterial == null)
		{
			m_ClearMaterial = GlobalStart.GetInstance().m_StencilClearMaterial;
		}
		CreateCommandBuffer();
	}

	private void Start()
	{
		m_RendereredStencilCount = 0;
		m_CameraManager = CameraManager.GetInstance();
		m_FloorManager = FloorManager.GetInstance();
		CameraManager cameraManager = m_CameraManager;
		cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		if (!(m_Camera != null))
		{
			return;
		}
		if (m_Camera.name.Contains("PIP"))
		{
			m_Camera.gameObject.SetActive(value: false);
		}
		else if (m_Camera.targetTexture != null)
		{
			int width = m_Camera.targetTexture.width;
			int height = m_Camera.targetTexture.height;
			if (m_StencilBuffer != null)
			{
				RenderTargetManager.ReleaseRenderTarget(ref m_StencilRenderTargetID);
			}
			m_StencilBuffer = RenderTargetManager.RequestRenderTarget(width, height, 0, RenderTextureFormat.R8, ref m_StencilRenderTargetID, "CSR");
			if (!(m_StencilBuffer == null))
			{
				m_bIsDirty = true;
				m_CommandBuffer.SetRenderTarget(m_StencilBuffer);
				m_CommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
			}
		}
	}

	public static void InitKeys(int capacity)
	{
		if (!m_bInitedCharacterKeys)
		{
			m_CharacterKeys = new FastList<StencilInterface>(capacity);
			m_bInitedCharacterKeys = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_StencilBuffer != null)
		{
			RenderTargetManager.ReleaseRenderTarget(ref m_StencilRenderTargetID);
		}
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
			m_CameraManager = null;
		}
		if (m_FloorManager != null)
		{
			m_FloorManager = null;
		}
		if (m_CharacterKeys != null)
		{
			for (int i = 0; i < m_CharacterKeys.Count; i++)
			{
				StencilInterface stencilInterface = m_CharacterKeys[i];
				if (stencilInterface != null)
				{
					RemoveCharacterMeshRenderers(stencilInterface);
				}
			}
		}
		if (m_CharacterKeys != null)
		{
			m_CharacterKeys.Clear();
			m_bInitedCharacterKeys = false;
		}
		m_CharacterMappings.Clear();
		m_CharacterAccMappings.Clear();
		m_CharacterSkinnedMap.Clear();
	}

	private void OnDisable()
	{
		DestroyCommandBuffer();
	}

	private void OnEnable()
	{
		CreateCommandBuffer();
	}

	private void Update()
	{
		if (m_Camera != null)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f - m_Camera.transform.localPosition.z);
		}
	}

	private void OnWillRenderObject()
	{
		m_bIsDirty = false;
		if (m_Camera == null || m_Camera != Camera.current)
		{
			return;
		}
		m_CommandBuffer.Clear();
		if (!m_isActive)
		{
			return;
		}
		int width = Screen.width;
		int height = Screen.height;
		if (m_RendereredStencilCount >= 30)
		{
			m_bInFullClearMode = true;
		}
		else
		{
			m_bInFullClearMode = false;
		}
		m_RendereredStencilCount = 0;
		if (m_Camera.targetTexture != null)
		{
			width = m_Camera.targetTexture.width;
			height = m_Camera.targetTexture.height;
		}
		if (m_StencilBuffer == null || m_StencilBuffer.width != width || m_StencilBuffer.height != height)
		{
			if (m_StencilBuffer != null)
			{
				RenderTargetManager.ReleaseRenderTarget(ref m_StencilRenderTargetID);
			}
			m_StencilBuffer = RenderTargetManager.RequestRenderTarget(width, height, 0, RenderTextureFormat.R8, ref m_StencilRenderTargetID, "CSR");
			if (m_StencilBuffer == null)
			{
				return;
			}
			m_bIsDirty = true;
			m_CommandBuffer.SetRenderTarget(m_StencilBuffer);
			m_CommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
		}
		m_CommandBuffer.SetRenderTarget(m_StencilBuffer);
		if (m_bInFullClearMode)
		{
			m_CommandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
		}
		int frameCount = UpdateManager.frameCount;
		if (frameCount != m_LastFrameKeysWereSorted)
		{
			m_CharacterKeys.Sort(delegate(StencilInterface x, StencilInterface y)
			{
				int path = 0;
				return StencilInterfaceComparer.Compare(x, y, ref path);
			});
			m_LastFrameKeysWereSorted = frameCount;
		}
		int num = 0;
		CameraManager.CameraBinding cameraBinding = m_CameraManager.GetCameraBinding(m_Camera);
		if (cameraBinding != null)
		{
			Vector3 newTargetPosition = cameraBinding.m_NewTargetPosition;
			num = m_FloorManager.FindFloorIndexAtZ(newTargetPosition.z);
		}
		Vector2 currentCameraMinPos = CullingObjectCollector.GetInstance().CurrentCameraMinPos;
		Vector2 currentCameraMaxPos = CullingObjectCollector.GetInstance().CurrentCameraMaxPos;
		m_StencilCamPos = m_Camera.transform.position;
		int count = m_CharacterKeys.Count;
		if (!m_bInFullClearMode)
		{
			bool flag = m_CameraManager.GetCameraManagerOpMode() == CameraManager.CameraOpModes.Cutscene;
			for (int i = 0; i < count; i++)
			{
				StencilInterface stencilInterface = m_CharacterKeys[i];
				if (!flag)
				{
					if (stencilInterface.GetIsHiddenOrDisabled())
					{
						continue;
					}
					int floorIndex = stencilInterface.GetFloorIndex();
					if (floorIndex > num)
					{
						continue;
					}
				}
				Vector3 cachedCurrentPosition = stencilInterface.GetCachedCurrentPosition();
				if (!(cachedCurrentPosition.x > currentCameraMaxPos.x) && !(cachedCurrentPosition.x < currentCameraMinPos.x) && !(cachedCurrentPosition.y > currentCameraMaxPos.y) && !(cachedCurrentPosition.y < currentCameraMinPos.y))
				{
					Matrix4x4 identity = Matrix4x4.identity;
					identity.SetTRS(cachedCurrentPosition + m_ClearQuadPos, Quaternion.identity, m_ClearQuadScale);
					m_CommandBuffer.DrawMesh(m_ClearPlane, identity, m_ClearMaterial);
				}
			}
		}
		count = m_CharacterKeys.Count;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			StencilInterface stencilInterface2 = m_CharacterKeys[num2];
			if (!stencilInterface2.GetIsHiddenOrDisabled())
			{
				int floorIndex2 = stencilInterface2.GetFloorIndex();
				if (floorIndex2 <= num)
				{
					Vector3 cachedCurrentPosition2 = stencilInterface2.GetCachedCurrentPosition();
					if (!(cachedCurrentPosition2.x > currentCameraMaxPos.x) && !(cachedCurrentPosition2.x < currentCameraMinPos.x) && !(cachedCurrentPosition2.y > currentCameraMaxPos.y) && !(cachedCurrentPosition2.y < currentCameraMinPos.y))
					{
						bool flag2 = m_bIsDirty || frameCount - stencilInterface2.GetLastCloseFrame() < 5;
						if (!flag2 && stencilInterface2.ConsiderForCloseCheck())
						{
							for (int j = num2 + 1; j < count; j++)
							{
								StencilInterface stencilInterface3 = m_CharacterKeys[j];
								if (stencilInterface3.GetFloorIndex() <= floorIndex2)
								{
									Vector3 cachedCurrentPosition3 = stencilInterface3.GetCachedCurrentPosition();
									if ((double)(cachedCurrentPosition3.y - cachedCurrentPosition2.y) <= 1.1 && (double)Mathf.Abs(cachedCurrentPosition3.x - cachedCurrentPosition2.x) <= 0.8)
									{
										stencilInterface2.SetLastCloseFrame(frameCount);
										stencilInterface3.SetLastCloseFrame(frameCount);
										flag2 = true;
										break;
									}
								}
							}
							if (!flag2)
							{
								for (int num3 = num2 - 1; num3 >= 0; num3--)
								{
									StencilInterface stencilInterface4 = m_CharacterKeys[num3];
									if (stencilInterface4.GetFloorIndex() <= floorIndex2)
									{
										Vector3 cachedCurrentPosition4 = stencilInterface4.GetCachedCurrentPosition();
										if ((double)(cachedCurrentPosition2.y - cachedCurrentPosition4.y) <= 1.1 && (double)Mathf.Abs(cachedCurrentPosition4.x - cachedCurrentPosition2.x) <= 0.8)
										{
											stencilInterface2.SetLastCloseFrame(frameCount);
											stencilInterface4.SetLastCloseFrame(frameCount);
											flag2 = true;
											break;
										}
									}
								}
							}
						}
						if (flag2)
						{
							if (m_CharacterSkinnedMap.ContainsKey(stencilInterface2))
							{
								FastList<CharacterSubMesh> fastList = m_CharacterSkinnedMap[stencilInterface2];
								int count2 = fastList.Count;
								m_RendereredStencilCount++;
								for (int k = 0; k < count2; k++)
								{
									CharacterSubMesh characterSubMesh = fastList[k];
									m_CommandBuffer.DrawRenderer(characterSubMesh.skin, characterSubMesh.material, characterSubMesh.subMeshIndex);
								}
							}
							else
							{
								FastList<MeshRenderer> fastList2 = m_CharacterMappings[stencilInterface2];
								int count3 = fastList2.Count;
								m_RendereredStencilCount++;
								for (int l = 0; l < count3; l++)
								{
									m_CommandBuffer.DrawRenderer(fastList2[l], m_Material);
								}
								fastList2.Clear();
								fastList2 = m_CharacterAccMappings[stencilInterface2];
								count3 = fastList2.Count;
								for (int m = 0; m < count3; m++)
								{
									m_CommandBuffer.DrawRenderer(fastList2[m], m_AccessoryMaterial);
								}
								fastList2.Clear();
							}
						}
					}
				}
			}
		}
		m_TextureVersion++;
	}

	public static void StartFrame()
	{
		if (ms_CameraCount > 0)
		{
			ms_CameraIndex++;
			ms_CameraIndex %= ms_CameraCount;
		}
	}

	public bool GetCharacterMeshLists(StencilInterface character, out FastList<MeshRenderer> coreMeshes, out FastList<MeshRenderer> accMeshes)
	{
		coreMeshes = null;
		accMeshes = null;
		if (!m_isActive)
		{
			return false;
		}
		if (character != null)
		{
			if (!m_CharacterKeys.Contains(character))
			{
				m_CharacterMappings[character] = new FastList<MeshRenderer>(256);
				m_CharacterAccMappings[character] = new FastList<MeshRenderer>(256);
				m_CharacterKeys.Add(character);
			}
			coreMeshes = m_CharacterMappings[character];
			accMeshes = m_CharacterAccMappings[character];
		}
		return true;
	}

	public void AddCharacterMeshRenderer(StencilInterface character, MeshRenderer characterMeshRenderer)
	{
		if (character != null && characterMeshRenderer != null && characterMeshRenderer.material != null && characterMeshRenderer.material.mainTexture != null)
		{
			if (!m_CharacterMappings.ContainsKey(character))
			{
				m_CharacterMappings[character] = new FastList<MeshRenderer>(256);
				m_CharacterKeys.Add(character);
			}
			m_CharacterMappings[character].Add(characterMeshRenderer);
		}
	}

	public static void SetCharacterSkinnedMeshRenderer(StencilInterface character, SkinnedMeshRenderer characterSkinnedMeshRenderer)
	{
		if (character == null || !(characterSkinnedMeshRenderer != null))
		{
			return;
		}
		if (!m_CharacterKeys.Contains(character))
		{
			m_CharacterKeys.Add(character);
		}
		if (!m_CharacterSkinnedMap.ContainsKey(character))
		{
			m_CharacterSkinnedMap[character] = new FastList<CharacterSubMesh>(8);
		}
		Material[] sharedMaterials = characterSkinnedMeshRenderer.sharedMaterials;
		int num = sharedMaterials.Length;
		int subMeshCount = characterSkinnedMeshRenderer.sharedMesh.subMeshCount;
		for (int i = 0; i < subMeshCount; i++)
		{
			string text = sharedMaterials[i].name;
			if (!text.Contains("ITM") && !text.Contains("VFX") && !text.Contains("CFT") && !text.Contains("lense"))
			{
				CharacterSubMesh characterSubMesh = new CharacterSubMesh();
				characterSubMesh.skin = characterSkinnedMeshRenderer;
				characterSubMesh.subMeshIndex = i;
				if (i <= num && sharedMaterials[i].shader.name.Contains("Head"))
				{
					characterSubMesh.material = m_AccessoryMaterial;
				}
				else
				{
					characterSubMesh.material = m_Material;
				}
				if (characterSubMesh.material != null)
				{
					m_CharacterSkinnedMap[character].Add(characterSubMesh);
				}
			}
		}
	}

	public void AddCharacterAccMeshRenderer(StencilInterface character, MeshRenderer accMeshRenderer)
	{
		if (character != null && accMeshRenderer != null && accMeshRenderer.material != null && accMeshRenderer.material.mainTexture != null)
		{
			if (!m_CharacterKeys.Contains(character))
			{
				m_CharacterMappings[character] = new FastList<MeshRenderer>(256);
				m_CharacterAccMappings[character] = new FastList<MeshRenderer>(256);
				m_CharacterKeys.Add(character);
			}
			m_CharacterAccMappings[character].Add(accMeshRenderer);
		}
	}

	public static void RemoveCharacterMeshRenderers(StencilInterface character)
	{
		if (m_CharacterKeys != null && m_CharacterKeys.Contains(character))
		{
			m_CharacterKeys.Remove(character);
		}
		if (m_CharacterMappings.ContainsKey(character))
		{
			m_CharacterMappings[character].Clear();
			m_CharacterMappings.Remove(character);
		}
		if (m_CharacterAccMappings.ContainsKey(character))
		{
			m_CharacterAccMappings[character].Clear();
			m_CharacterAccMappings.Remove(character);
		}
		if (m_CharacterSkinnedMap.ContainsKey(character))
		{
			m_CharacterSkinnedMap[character] = null;
			m_CharacterSkinnedMap.Remove(character);
		}
	}

	public RenderTexture GetCharacterStencilTexture(ref Vector2 stencilOrigin)
	{
		if (m_isActive)
		{
			m_StencilCamPos = m_Camera.transform.position;
		}
		stencilOrigin = m_StencilCamPos;
		return m_StencilBuffer;
	}

	public int GetTextureVersion()
	{
		return m_TextureVersion;
	}

	private static void CreateMaterials()
	{
		if (m_Material == null)
		{
			Shader shader = Shader.Find("Unlit/CharacterStencil");
			if (shader != null && shader.isSupported)
			{
				m_Material = new Material(shader);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				Debug.LogError("FAILED TO FIND SHADER Unlit/CharacterStencil");
			}
		}
		if (m_AccessoryMaterial == null)
		{
			Shader shader2 = Shader.Find("Unlit/CharacterStencilAccessory");
			if (shader2 != null && shader2.isSupported)
			{
				m_AccessoryMaterial = new Material(shader2);
				m_AccessoryMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			else
			{
				Debug.LogError("FAILED TO FIND SHADER Unlit/CharacterStencilAccessory");
			}
		}
		m_bMaterialsCreated = true;
	}

	private void CreateCommandBuffer()
	{
		if (m_CommandBuffer == null && m_Camera != null)
		{
			m_CommandBuffer = new CommandBuffer();
			m_CommandBuffer.name = "Character Stencil Renderer";
			m_Camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, m_CommandBuffer);
		}
	}

	private void DestroyCommandBuffer()
	{
		if (m_CommandBuffer != null && m_Camera != null)
		{
			m_Camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, m_CommandBuffer);
			m_CommandBuffer = null;
		}
	}

	private void ActiveCamerasUpdated()
	{
		ms_CameraCount = ((!T17NetManager.NetOnlineMode) ? m_CameraManager.GetUsedCameraCount() : Gamer.GetNumLocalGamers());
	}
}
