using System;
using System.Collections.Generic;
using UnityEngine;

public class CullingObjectCollector : T17MonoBehaviour
{
	public enum AMBIENT_LIGHT_MODE
	{
		ALM_INSIDE,
		ALM_OUTSIDE,
		ALM_DUAL
	}

	public class CharacterWrapper
	{
		public class MeshRendererTransform
		{
			public MeshRenderer m_MeshRenderer;

			public Transform m_Transform;

			public Texture[] m_Textures;

			public bool m_bCanRender;
		}

		public Character m_Character;

		public MeshRendererTransform[][] m_CoreMeshRenderers;

		public bool[] m_VisEnabled;

		public MeshRendererTransform[][] m_AccMeshRenderers;

		public bool[] m_AccVisEnabled;

		public MeshRendererTransform[] m_ActionMeshRenderers;

		public CharacterAnimator m_CharacterAnimator;

		public Animator m_Animator;

		public Light[] m_Lights;

		public int m_VisibilityMask;

		public bool m_bVis;

		public int m_CharacterStencilVer = -1;

		public bool m_isHiddenAlready;

		public bool m_bHasAccessoryTextures;

		public const int kMaxCameras = 8;

		public MaterialPropertyBlock m_propertyBlock = new MaterialPropertyBlock();

		public MaterialPropertyBlock m_accessoryPropertyBlock;

		public SkinnedMeshRenderer m_SkinnedMeshRenderer;

		public CharacterWrapper(Character character)
		{
			m_Character = character;
			m_CharacterAnimator = character.GetComponentInChildren<CharacterAnimator>();
			m_Lights = character.GetComponentsInChildren<Light>();
			m_Animator = character.GetComponentInChildren<Animator>();
			m_SkinnedMeshRenderer = character.GetComponentInChildren<SkinnedMeshRenderer>();
			m_VisEnabled = null;
			if (m_SkinnedMeshRenderer == null)
			{
				m_CoreMeshRenderers = new MeshRendererTransform[4][];
				m_AccMeshRenderers = new MeshRendererTransform[4][];
				CollectConstantCoreCharacterMeshRenderers();
				CollectConstantAccessoryCharacterMeshRenderers();
				CollectCurrentActionCharacterMeshRenderers();
			}
		}

		~CharacterWrapper()
		{
			m_Character = null;
			m_CharacterAnimator = null;
			m_Lights = null;
			m_Animator = null;
			m_SkinnedMeshRenderer = null;
			m_CoreMeshRenderers = null;
			m_AccMeshRenderers = null;
		}

		public void CollectConstantCoreCharacterMeshRenderers()
		{
			List<MeshRendererTransform>[] array = new List<MeshRendererTransform>[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = new List<MeshRendererTransform>();
			}
			m_CharacterAnimator.GetCoreDirectionMeshRenderers(array);
			for (int i = 0; i < 4; i++)
			{
				m_CoreMeshRenderers[i] = new MeshRendererTransform[array[i].Count];
				for (int j = 0; j < array[i].Count; j++)
				{
					m_CoreMeshRenderers[i][j] = array[i][j];
				}
			}
			if (m_VisEnabled == null)
			{
				m_VisEnabled = new bool[array[0].Count];
				for (int j = 0; j < array[0].Count; j++)
				{
					m_VisEnabled[j] = false;
				}
			}
		}

		public void CollectConstantAccessoryCharacterMeshRenderers()
		{
			List<MeshRendererTransform>[] array = new List<MeshRendererTransform>[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = new List<MeshRendererTransform>();
			}
			m_CharacterAnimator.GetAccessoryMeshRenderers(array);
			for (int i = 0; i < 4; i++)
			{
				m_AccMeshRenderers[i] = new MeshRendererTransform[array[i].Count];
				for (int j = 0; j < array[i].Count; j++)
				{
					m_AccMeshRenderers[i][j] = array[i][j];
				}
			}
			if (m_AccVisEnabled == null)
			{
				m_AccVisEnabled = new bool[array[0].Count];
				for (int j = 0; j < array[0].Count; j++)
				{
					m_AccVisEnabled[j] = false;
				}
			}
		}

		public void CollectCurrentActionCharacterMeshRenderers()
		{
			List<MeshRendererTransform> list = new List<MeshRendererTransform>();
			m_CharacterAnimator.GetActionMeshRenderers(list);
			m_ActionMeshRenderers = new MeshRendererTransform[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				m_ActionMeshRenderers[i] = list[i];
				m_ActionMeshRenderers[i].m_bCanRender = list[i].m_MeshRenderer.material != null;
				if (m_ActionMeshRenderers[i].m_bCanRender && list[i].m_Textures.Length > 0)
				{
					m_ActionMeshRenderers[i].m_Textures[0] = list[i].m_MeshRenderer.material.mainTexture;
				}
			}
			for (int j = 0; j < 4; j++)
			{
				MeshRendererTransform[] array = m_CoreMeshRenderers[j];
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					if (array[i].m_MeshRenderer.material != null && array[i].m_Textures.Length > 0)
					{
						array[i].m_Textures[0] = array[i].m_MeshRenderer.material.mainTexture;
						array[i].m_bCanRender = true;
					}
					else
					{
						array[i].m_bCanRender = false;
					}
				}
				MeshRendererTransform[] array2 = m_AccMeshRenderers[j];
				int num2 = array2.Length;
				for (int i = 0; i < num2; i++)
				{
					if (array2[i].m_MeshRenderer.material != null && array2[i].m_Textures.Length >= 5)
					{
						if (!array2[i].m_MeshRenderer.material.HasProperty(m_propID_CharacterTexture_HairMask))
						{
							array2[i].m_bCanRender = false;
							continue;
						}
						array2[i].m_Textures[0] = array2[i].m_MeshRenderer.material.GetTexture(m_propID_CharacterTexture_HairMask);
						array2[i].m_Textures[1] = array2[i].m_MeshRenderer.material.GetTexture(m_propID_CharacterTexture_Hair);
						array2[i].m_Textures[2] = array2[i].m_MeshRenderer.material.GetTexture(m_propID_CharacterTexture_Hat);
						array2[i].m_Textures[3] = array2[i].m_MeshRenderer.material.GetTexture(m_propID_CharacterTexture_UpperFace);
						array2[i].m_Textures[4] = array2[i].m_MeshRenderer.material.GetTexture(m_propID_CharacterTexture_LowerFace);
						array2[i].m_bCanRender = true;
					}
					else
					{
						array2[i].m_bCanRender = false;
					}
				}
			}
		}

		public void Init()
		{
			m_Character.EnableLayerAnimator(bEnable: false);
			m_Character.EnableTrackededElementRendering(bEnable: false);
			m_Animator.enabled = false;
			if (m_SkinnedMeshRenderer == null)
			{
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < m_CoreMeshRenderers[i].Length; j++)
					{
						m_CoreMeshRenderers[i][j].m_MeshRenderer.enabled = false;
						m_VisEnabled[j] = false;
					}
					for (int j = 0; j < m_AccMeshRenderers[i].Length; j++)
					{
						m_AccMeshRenderers[i][j].m_MeshRenderer.enabled = false;
						m_AccVisEnabled[j] = false;
					}
				}
				for (int i = 0; i < m_ActionMeshRenderers.Length; i++)
				{
					m_ActionMeshRenderers[i].m_MeshRenderer.enabled = false;
				}
			}
			m_CharacterAnimator.m_bRenderingDirty = true;
		}

		public void CreatePropertyBlocks()
		{
			m_propertyBlock.Clear();
			m_propertyBlock.SetColor(m_propId_Color, Color.white);
			m_propertyBlock.SetColor(m_propId_TileHeightTint, Color.white);
			m_propertyBlock.SetFloat(m_propID_CharacterID, m_Character.m_CharacterID / 255f);
			if (m_SkinnedMeshRenderer != null)
			{
				Material sharedMaterial = m_SkinnedMeshRenderer.sharedMaterial;
				if (sharedMaterial != null)
				{
					Texture mainTexture = sharedMaterial.mainTexture;
					if (mainTexture != null)
					{
						m_propertyBlock.SetTexture(m_propID_CharacterTexture, mainTexture);
					}
				}
			}
			else
			{
				m_accessoryPropertyBlock = new MaterialPropertyBlock();
			}
			MaterialPropertyBlock accPropertyBlock = m_propertyBlock;
			if (m_accessoryPropertyBlock != null)
			{
				m_accessoryPropertyBlock.Clear();
				m_accessoryPropertyBlock.SetColor(m_propId_Color, Color.white);
				m_accessoryPropertyBlock.SetColor(m_propId_TileHeightTint, Color.white);
				m_accessoryPropertyBlock.SetFloat(m_propID_CharacterID, m_Character.m_CharacterID / 255f);
				accPropertyBlock = m_accessoryPropertyBlock;
			}
			m_bHasAccessoryTextures = false;
			CharacterAnimator.AccessoryData currentAccessoryData = m_CharacterAnimator.GetCurrentAccessoryData();
			if (currentAccessoryData != null && currentAccessoryData.hairMask != null)
			{
				SetAccessoryTextureInMaterialBlock(currentAccessoryData.hairMask, m_propID_CharacterTexture_HairMask, accPropertyBlock);
				SetAccessoryTextureInMaterialBlock(currentAccessoryData.hairTex, m_propID_CharacterTexture_Hair, accPropertyBlock);
				SetAccessoryTextureInMaterialBlock(currentAccessoryData.hatTex, m_propID_CharacterTexture_Hat, accPropertyBlock);
				SetAccessoryTextureInMaterialBlock(currentAccessoryData.upperFaceTex, m_propID_CharacterTexture_UpperFace, accPropertyBlock);
				SetAccessoryTextureInMaterialBlock(currentAccessoryData.lowerFaceTex, m_propID_CharacterTexture_LowerFace, accPropertyBlock);
			}
			RoutineManager instance = RoutineManager.GetInstance();
			if (instance != null && !instance.IsTimeFrozen())
			{
				UpdateLightingImmediate();
			}
		}

		public void UpdateLightingImmediate()
		{
			bool inside = m_Character.m_isInside != 0;
			LightingManager instance = LightingManager.GetInstance();
			Color currentAmbientColor = instance.GetCurrentAmbientColor(inside);
			float currentAmbientIntensity = instance.GetCurrentAmbientIntensity(inside);
			MaterialPropertyBlock propertyBlock = m_propertyBlock;
			if (propertyBlock != null)
			{
				propertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
				propertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
				MaterialPropertyBlock accessoryPropertyBlock = m_accessoryPropertyBlock;
				if (accessoryPropertyBlock != null)
				{
					accessoryPropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
					accessoryPropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
				}
				if (m_SkinnedMeshRenderer != null)
				{
					m_SkinnedMeshRenderer.SetPropertyBlock(propertyBlock);
				}
			}
		}

		private void SetAccessoryTextureInMaterialBlock(Texture tex, int propertyID, MaterialPropertyBlock accPropertyBlock)
		{
			if (tex != null)
			{
				accPropertyBlock.SetTexture(propertyID, tex);
				m_bHasAccessoryTextures = true;
			}
		}

		public void GetMaterialPropertyBlocks(out MaterialPropertyBlock mainBlock, out MaterialPropertyBlock accBlock)
		{
			if (m_propertyBlock != null)
			{
				mainBlock = m_propertyBlock;
			}
			else
			{
				mainBlock = null;
			}
			if (m_accessoryPropertyBlock != null)
			{
				accBlock = m_accessoryPropertyBlock;
			}
			else
			{
				accBlock = null;
			}
		}

		public virtual void UpdateLogic(int newVisibilityMask, bool bVis, int cameraID, CharacterStencilRenderer stencilRenderer)
		{
			if (m_VisibilityMask != newVisibilityMask)
			{
				if (newVisibilityMask == 0)
				{
					m_Character.EnableLayerAnimator(bEnable: false);
					m_Character.EnableTrackededElementRendering(bEnable: false);
					m_Animator.enabled = false;
					int num = m_Lights.Length;
					for (int i = 0; i < num; i++)
					{
						m_Lights[i].enabled = false;
					}
				}
				else if (m_VisibilityMask == 0)
				{
					m_Character.EnableLayerAnimator(bEnable: true);
					m_Character.EnableTrackededElementRendering(bEnable: true);
					m_Animator.enabled = true;
					m_CharacterAnimator.OnAnimatorEnabled();
					m_CharacterAnimator.PostCullingObjetCollectorEnabled();
					if (m_CharacterAnimator.IsSpotlightActive())
					{
						int num2 = m_Lights.Length;
						for (int i = 0; i < num2; i++)
						{
							m_Lights[i].enabled = true;
						}
					}
				}
			}
			if ((m_bVis != bVis || m_CharacterAnimator.m_bRenderingDirty) && (bool)m_SkinnedMeshRenderer)
			{
				if (bVis && m_CharacterAnimator.m_bRenderingDirty)
				{
					m_CharacterAnimator.m_bRenderingDirty = false;
					CreatePropertyBlocks();
					m_SkinnedMeshRenderer.SetPropertyBlock(m_propertyBlock);
					m_SkinnedMeshRenderer.enabled = bVis;
				}
				else
				{
					m_SkinnedMeshRenderer.enabled = bVis;
					m_bVis = bVis;
				}
			}
			m_VisibilityMask = newVisibilityMask;
		}

		private void UpdateLogicNonSkinned(int newVisibilityMask, bool bVis, CharacterStencilRenderer stencilRenderer)
		{
			FastList<MeshRenderer> coreMeshes = null;
			FastList<MeshRenderer> accMeshes = null;
			if (stencilRenderer != null)
			{
				stencilRenderer.GetCharacterMeshLists(m_Character, out coreMeshes, out accMeshes);
			}
			Directionx4 animatorFacingDirection = m_Character.m_CharacterAnimator.GetAnimatorFacingDirection();
			RoomBlob currentLocation = m_Character.m_CurrentLocation;
			int num = (int)animatorFacingDirection >> 1;
			int num2 = m_CoreMeshRenderers[num].Length;
			if (m_CharacterAnimator.m_bAlteredAnimatorMaterials)
			{
				CollectCurrentActionCharacterMeshRenderers();
			}
			if (m_CharacterAnimator.m_bRenderingDirty)
			{
				CreatePropertyBlocks();
				m_CharacterAnimator.m_bRenderingDirty = false;
			}
			GetMaterialPropertyBlocks(out var mainBlock, out var accBlock);
			MeshRendererTransform[] array = m_CoreMeshRenderers[num];
			bool[] visEnabled = m_VisEnabled;
			for (int i = 0; i < num2; i++)
			{
				MeshRendererTransform meshRendererTransform = array[i];
				MeshRenderer meshRenderer = meshRendererTransform.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform.m_bCanRender && meshRendererTransform.m_Textures.Length > 0)
				{
					Texture texture = meshRendererTransform.m_Textures[0];
					if (texture != null)
					{
						float x = meshRendererTransform.m_Transform.localScale.x;
						if (x * x > 0.1f)
						{
							mainBlock.SetTexture(m_propID_CharacterTexture, texture);
							if (x < 0f)
							{
								mainBlock.SetFloat(m_propId_BumpFilp, -1f);
							}
							else
							{
								mainBlock.SetFloat(m_propId_BumpFilp, 1f);
							}
							meshRenderer.SetPropertyBlock(mainBlock);
							flag = true;
							coreMeshes?.Add(meshRenderer);
						}
					}
				}
				if (flag != visEnabled[i])
				{
					visEnabled[i] = flag;
					meshRenderer.enabled = flag;
				}
			}
			bool bHasAccessoryTextures = m_bHasAccessoryTextures;
			array = m_AccMeshRenderers[num];
			visEnabled = m_AccVisEnabled;
			int num3 = array.Length;
			for (int i = 0; i < num3; i++)
			{
				MeshRendererTransform meshRendererTransform2 = array[i];
				MeshRenderer meshRenderer2 = meshRendererTransform2.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform2.m_bCanRender && bHasAccessoryTextures)
				{
					float x2 = meshRendererTransform2.m_Transform.localScale.x;
					if (x2 * x2 > 0.1f)
					{
						if (x2 < 0f)
						{
							accBlock.SetFloat(m_propId_BumpFilp, -1f);
						}
						else
						{
							accBlock.SetFloat(m_propId_BumpFilp, 1f);
						}
						meshRenderer2.SetPropertyBlock(accBlock);
						accMeshes?.Add(meshRenderer2);
						flag = true;
					}
				}
				if (flag != visEnabled[i])
				{
					visEnabled[i] = flag;
					meshRenderer2.enabled = flag;
				}
			}
			if (!m_Character.m_bActionRenderersRequired)
			{
				return;
			}
			int num4 = m_ActionMeshRenderers.Length;
			for (int i = 0; i < num4; i++)
			{
				MeshRendererTransform meshRendererTransform3 = m_ActionMeshRenderers[i];
				MeshRenderer meshRenderer3 = meshRendererTransform3.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform3.m_bCanRender && meshRendererTransform3.m_Textures.Length > 0)
				{
					Texture texture2 = meshRendererTransform3.m_Textures[0];
					if (texture2 != null)
					{
						float x3 = meshRendererTransform3.m_Transform.localScale.x;
						if (x3 * x3 > 0.1f)
						{
							mainBlock.SetTexture(m_propID_CharacterTexture, texture2);
							meshRenderer3.SetPropertyBlock(mainBlock);
							coreMeshes?.Add(meshRenderer3);
							flag = true;
						}
					}
				}
				meshRenderer3.enabled = flag;
			}
		}
	}

	public class CrowdCharacterWrapper : CharacterWrapper
	{
		private AICharacter_CrowdNPC AICharacter_CrowdNPC;

		public CrowdCharacterWrapper(CharacterWrapper charWrapper)
			: base(charWrapper.m_Character)
		{
			AICharacter_CrowdNPC = m_Character.GetComponent<AICharacter_CrowdNPC>();
		}

		public override void UpdateLogic(int newVisibilityMask, bool bVis, int cameraID, CharacterStencilRenderer stencilRenderer)
		{
			if (m_VisibilityMask != newVisibilityMask)
			{
				if (newVisibilityMask == 0)
				{
					m_Character.EnableLayerAnimator(bEnable: false);
					m_Character.EnableTrackededElementRendering(bEnable: false);
					AICharacter_CrowdNPC.ControllAnimatorFromCulling(bEnable: false);
					int num = m_Lights.Length;
					for (int i = 0; i < num; i++)
					{
						m_Lights[i].enabled = false;
					}
				}
				else if (m_VisibilityMask == 0)
				{
					m_Character.EnableLayerAnimator(bEnable: true);
					m_Character.EnableTrackededElementRendering(bEnable: true);
					AICharacter_CrowdNPC.ControllAnimatorFromCulling(bEnable: true);
					if (m_CharacterAnimator.IsSpotlightActive())
					{
						int num2 = m_Lights.Length;
						for (int i = 0; i < num2; i++)
						{
							m_Lights[i].enabled = true;
						}
					}
				}
			}
			if ((m_bVis != bVis || m_CharacterAnimator.m_bRenderingDirty) && (bool)m_SkinnedMeshRenderer)
			{
				if (bVis && m_CharacterAnimator.m_bRenderingDirty)
				{
					m_CharacterAnimator.m_bRenderingDirty = false;
					CreatePropertyBlocks();
					m_SkinnedMeshRenderer.SetPropertyBlock(m_propertyBlock);
					m_SkinnedMeshRenderer.enabled = bVis;
				}
				else
				{
					m_SkinnedMeshRenderer.enabled = bVis;
					m_bVis = bVis;
				}
			}
			m_VisibilityMask = newVisibilityMask;
		}
	}

	public class LightWrapperContainer
	{
		public FastList<LightWrapper> m_NormalLightWrappers = new FastList<LightWrapper>();

		public FastList<LightWrapper> m_CustomLightWrappers = new FastList<LightWrapper>();

		public FastList<LightWrapper> m_CustomLightDynamicWrappers = new FastList<LightWrapper>();

		public void Clear()
		{
			m_NormalLightWrappers.Clear();
			m_CustomLightWrappers.Clear();
			m_CustomLightDynamicWrappers.Clear();
		}
	}

	public class LightWrapper
	{
		public Light m_Light;

		public CustomLight m_CustomLight;

		public Vector3 m_position = Vector3.zero;

		public LightControl m_control;

		public int m_floorIndex;

		public bool m_isStatic = true;

		public float m_range;

		private bool m_bIsEnabled = true;

		private CustomLightManager m_lightManager;

		public LightWrapper(Light light)
		{
			m_Light = light;
			if (light != null)
			{
				m_position = light.transform.position;
				m_control = light.GetComponent<LightControl>();
				m_Light.shadows = LightShadows.None;
				m_bIsEnabled = m_Light.enabled;
				m_range = m_Light.range;
				if (FloorManager.GetInstance() != null)
				{
					m_floorIndex = FloorManager.GetInstance().FindFloorIndexForRendererZ(m_position.z);
				}
			}
		}

		public LightWrapper(CustomLight customLight, bool isStatic = true)
		{
			m_CustomLight = customLight;
			m_isStatic = isStatic;
			if (customLight != null)
			{
				m_position = customLight.transform.position;
				m_control = customLight.GetComponent<LightControl>();
				m_range = m_CustomLight.m_Size;
				if (FloorManager.GetInstance() != null)
				{
					m_floorIndex = FloorManager.GetInstance().FindFloorIndexForRendererZ(m_position.z);
				}
			}
		}

		~LightWrapper()
		{
		}

		public void Init()
		{
			if ((bool)m_Light)
			{
				m_Light.enabled = false;
				m_bIsEnabled = false;
			}
			m_lightManager = CustomLightRenderer.customLightManager;
		}

		public void UpdateLogicNormal(bool inView)
		{
			bool flag = false;
			flag = inView && (m_control == null || m_control.isEnabled);
			if (m_bIsEnabled != flag)
			{
				m_Light.enabled = flag;
				m_bIsEnabled = flag;
			}
		}

		public bool UpdateLogicCustom(bool inView)
		{
			bool flag = false;
			flag = inView && (m_control == null || m_control.isEnabled);
			if (!m_isStatic && m_position != m_CustomLight.transform.position)
			{
				m_CustomLight.UpdateMatrix();
				m_position = m_CustomLight.transform.position;
			}
			if (flag)
			{
				if (m_isStatic)
				{
					m_lightManager.Add(m_CustomLight);
				}
				else
				{
					m_lightManager.AddDynamic(m_CustomLight);
				}
			}
			return flag && !m_isStatic;
		}
	}

	public class DeskWrapper
	{
		public GameObject m_GameObject;

		public Transform m_Transform;

		public MeshRenderer[] m_MeshRenderers;

		public Animator[] m_Animators;

		public ICullingWrapperListener[] m_WrapperListeners;

		public int m_VisibilityMask;

		public int m_Floor;

		public bool m_isEnabled;

		public MaterialPropertyBlock m_propertyBlock;

		private RoomManager m_RoomManager;

		public DeskWrapper(GameObject gameObject)
		{
			m_GameObject = gameObject;
			m_Transform = gameObject.transform;
			m_MeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			m_Animators = gameObject.GetComponentsInChildren<Animator>();
			m_Floor = FloorManager.GetInstance().FindFloorAtZ(gameObject.transform.position.z).m_FloorIndex;
			m_WrapperListeners = gameObject.GetComponentsInChildren<ICullingWrapperListener>(includeInactive: true);
			m_isEnabled = true;
		}

		public void Init()
		{
			for (int i = 0; i < m_Animators.Length; i++)
			{
				m_Animators[i].enabled = false;
			}
			for (int i = 0; i < m_MeshRenderers.Length; i++)
			{
				m_MeshRenderers[i].enabled = false;
			}
			m_isEnabled = false;
			m_RoomManager = RoomManager.GetInstance();
		}

		public void DisableLogic()
		{
			for (int i = 0; i < m_Animators.Length; i++)
			{
				m_Animators[i].enabled = false;
			}
		}

		public void EnableLogic()
		{
			for (int i = 0; i < m_Animators.Length; i++)
			{
				m_Animators[i].enabled = true;
			}
			if (m_WrapperListeners != null)
			{
				for (int i = 0; i < m_WrapperListeners.Length; i++)
				{
					m_WrapperListeners[i].OnCullingWrapperEnabled();
				}
			}
		}

		private RoomBlob GetGameObjectRoom(Transform tran, int floorIndex)
		{
			RoomFloor floorFromIndex = m_RoomManager.GetFloorFromIndex(floorIndex);
			Vector3 vector = RoomUtility.WorldToRoomGrid(tran.position, floorFromIndex);
			return m_RoomManager.LookUpRoom((int)vector.x, (int)vector.y, floorFromIndex);
		}

		public void UpdateLogic(int oldVisibility, int newVisibility)
		{
			if (oldVisibility != newVisibility)
			{
				if (newVisibility == 0)
				{
					DisableLogic();
				}
				else if (oldVisibility == 0)
				{
					EnableLogic();
				}
			}
		}
	}

	public class TagWrapper
	{
		public GameObject m_GameObject;

		public MeshRenderer[] m_MeshRenderers;

		public int m_VisibilityMask;

		public int m_CullingMask;

		public int m_Floor;

		public bool m_isEnabled;

		public TagWrapper(GameObject gameObject)
		{
			m_GameObject = gameObject;
			m_MeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			Tag component = gameObject.GetComponent<Tag>();
			m_CullingMask = component.cullingMask;
			m_Floor = component.tileFloor;
			m_isEnabled = true;
		}

		~TagWrapper()
		{
		}

		public void Init()
		{
			for (int i = 0; i < m_MeshRenderers.Length; i++)
			{
				m_MeshRenderers[i].enabled = false;
			}
			m_isEnabled = false;
		}

		public void UpdateLogic(int oldVisibility, int newVisibility)
		{
			m_VisibilityMask = newVisibility;
		}
	}

	[Serializable]
	public class LargeRendererWrapper
	{
		public class BucketData
		{
			public CullingBuckets.Bucket m_Bucket;

			public int m_bViz;

			public BucketData(CullingBuckets.Bucket bucket)
			{
				m_Bucket = bucket;
				m_bViz = 0;
			}
		}

		public GameObject m_GameObject;

		public Transform m_Transform;

		public MeshRenderer[] m_MeshRenderers;

		public int m_VisibilityMask;

		public int m_Floor;

		public bool m_isEnabled;

		public bool m_bNeedUpdate;

		public int m_ID;

		public FastList<BucketData> m_SpanningBuckets = new FastList<BucketData>();

		public LargeRendererWrapper(GameObject gameObject, FastList<CullingBuckets.Bucket> spanningBuckets, int id)
		{
			m_GameObject = gameObject;
			m_Transform = gameObject.transform;
			m_MeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			m_Floor = FloorManager.GetInstance().FindFloorAtZ(gameObject.transform.position.z).m_FloorIndex;
			for (int i = 0; i < spanningBuckets.Count; i++)
			{
				m_SpanningBuckets.Add(new BucketData(spanningBuckets[i]));
			}
			m_isEnabled = true;
			m_ID = id;
		}

		public LargeRendererWrapper(GameObject lRW, LevelScript.LRWData lRWData, int id)
		{
			m_GameObject = lRW;
			m_Transform = lRW.transform;
			m_MeshRenderers = lRW.GetComponentsInChildren<MeshRenderer>();
			m_Floor = FloorManager.GetInstance().FindFloorAtZ(lRW.transform.position.z).m_FloorIndex;
			for (int i = 0; i < lRWData.m_Buckets.Length; i++)
			{
				CullingBuckets.Bucket bucket = CullingBuckets.s_buckets[(int)lRWData.m_Buckets[i].x, (int)lRWData.m_Buckets[i].y, (int)lRWData.m_Buckets[i].z];
				m_SpanningBuckets.Add(new BucketData(bucket));
			}
			m_isEnabled = true;
			m_ID = id;
		}

		public void Init()
		{
			for (int i = 0; i < m_MeshRenderers.Length; i++)
			{
				m_MeshRenderers[i].enabled = false;
			}
			m_isEnabled = false;
		}

		public void ReSetBuckets(LevelScript.LRWData lRWData, int id)
		{
			m_SpanningBuckets.Clear();
			for (int i = 0; i < lRWData.m_Buckets.Length; i++)
			{
				CullingBuckets.Bucket bucket = CullingBuckets.s_buckets[(int)lRWData.m_Buckets[i].x, (int)lRWData.m_Buckets[i].y, (int)lRWData.m_Buckets[i].z];
				m_SpanningBuckets.Add(new BucketData(bucket));
			}
		}

		public void SetCameraVisibility(int cameraID, bool isVisible, CullingBuckets.Bucket bucket)
		{
			for (int i = 0; i < m_SpanningBuckets.Count; i++)
			{
				if (m_SpanningBuckets[i].m_Bucket.m_bucketX == bucket.m_bucketX && m_SpanningBuckets[i].m_Bucket.m_bucketY == bucket.m_bucketY && m_SpanningBuckets[i].m_Bucket.m_bucketZ <= bucket.m_bucketZ)
				{
					if (isVisible)
					{
						m_SpanningBuckets[i].m_bViz |= 1 << cameraID;
					}
					else
					{
						m_SpanningBuckets[i].m_bViz &= ~(1 << cameraID);
					}
					m_bNeedUpdate = true;
					break;
				}
			}
		}
	}

	[Serializable]
	public class AnimatedWrapper
	{
		public GameObject m_GameObject;

		public MeshRenderer[] m_MeshRenderers;

		public MaterialPropertyBlock[] m_MeshRenderersPropertyBlock;

		public List<Animator> m_Animators;

		public AnimatedInteraction[] m_WrapperListeners;

		public ParticleSystem[] m_Particles;

		public int m_VisibilityMask;

		public int m_oldVisibilityMask;

		public bool m_isEnabled;

		public int m_floorIndex;

		public AMBIENT_LIGHT_MODE m_AmbientLightMode;

		public int m_isInside;

		public bool m_bNeedUpdate;

		public bool m_bUpdateLightingProperties = true;

		public AnimatedWrapper(GameObject gameObject)
		{
			m_GameObject = gameObject;
			m_MeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			Animator[] componentsInChildren = gameObject.GetComponentsInChildren<Animator>();
			m_WrapperListeners = gameObject.transform.parent.GetComponentsInChildren<AnimatedInteraction>(includeInactive: true);
			m_Particles = gameObject.transform.parent.GetComponentsInChildren<ParticleSystem>();
			m_MeshRenderersPropertyBlock = new MaterialPropertyBlock[m_MeshRenderers.Length];
			m_floorIndex = FloorManager.GetInstance().FindFloorIndexAtZ(m_GameObject.transform.position.z);
			if (RoomManager.GetInstance() != null)
			{
				RoomFloor floorFromIndex = RoomManager.GetInstance().GetFloorFromIndex(m_floorIndex);
				if (floorFromIndex != null)
				{
					Vector3 vector = RoomUtility.WorldToRoomGrid(m_GameObject.transform.position, floorFromIndex);
					RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom((int)vector.x, (int)vector.y, floorFromIndex);
					if (roomBlob != null)
					{
						if (roomBlob != null && roomBlob.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors)
						{
							m_AmbientLightMode = AMBIENT_LIGHT_MODE.ALM_INSIDE;
							m_isInside = 1;
						}
						else
						{
							m_AmbientLightMode = AMBIENT_LIGHT_MODE.ALM_OUTSIDE;
							m_isInside = 0;
						}
					}
				}
			}
			m_Animators = new List<Animator>(componentsInChildren.Length);
			AnimSyncher instance = AnimSyncher.instance;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(instance != null) || !instance.DoesContain(componentsInChildren[i]))
				{
					componentsInChildren[i].ForceStateNormalizedTime(1f);
					componentsInChildren[i].enabled = false;
					m_Animators.Add(componentsInChildren[i]);
				}
			}
			for (int i = 0; i < m_Particles.Length; i++)
			{
				m_Particles[i].gameObject.SetActive(value: false);
			}
			for (int i = 0; i < m_MeshRenderers.Length; i++)
			{
				m_MeshRenderers[i].enabled = false;
			}
		}

		public void SetCameraVisibility(int cameraID, bool isVisible)
		{
			if (isVisible)
			{
				m_VisibilityMask |= 1 << cameraID;
			}
			else
			{
				m_VisibilityMask &= ~(1 << cameraID);
			}
			m_bNeedUpdate = true;
		}

		public void Init()
		{
			if (m_MeshRenderers != null && m_MeshRenderers.Length > 0)
			{
				if (m_MeshRenderersPropertyBlock == null)
				{
					m_MeshRenderersPropertyBlock = new MaterialPropertyBlock[m_MeshRenderers.Length];
				}
				for (int i = 0; i < m_MeshRenderers.Length; i++)
				{
					m_MeshRenderersPropertyBlock[i] = new MaterialPropertyBlock();
					m_MeshRenderers[i].GetPropertyBlock(m_MeshRenderersPropertyBlock[i]);
				}
				for (int i = 0; i < m_Animators.Count; i++)
				{
					m_Animators[i].ForceStateNormalizedTime(1f);
				}
			}
		}

		public void DisableLogic()
		{
			int num = m_Particles.Length;
			for (int i = 0; i < num; i++)
			{
				m_Particles[i].gameObject.SetActive(value: false);
			}
			num = m_Animators.Count;
			for (int i = 0; i < num; i++)
			{
				m_Animators[i].enabled = false;
			}
		}

		public void EnableLogic()
		{
			int num = m_Particles.Length;
			for (int i = 0; i < num; i++)
			{
				m_Particles[i].gameObject.SetActive(value: true);
			}
			num = m_Animators.Count;
			for (int i = 0; i < num; i++)
			{
				m_Animators[i].enabled = true;
				m_Animators[i].Update(0f);
			}
			if (m_WrapperListeners != null)
			{
				for (int i = 0; i < m_WrapperListeners.Length; i++)
				{
					m_WrapperListeners[i].OnCullingWrapperEnabled();
				}
			}
		}

		public void UpdateVisibility()
		{
			if (m_oldVisibilityMask != m_VisibilityMask)
			{
				if (m_VisibilityMask == 0 && m_oldVisibilityMask != 0)
				{
					DisableLogic();
				}
				else if (m_oldVisibilityMask == 0)
				{
					EnableLogic();
				}
				m_oldVisibilityMask = m_VisibilityMask;
			}
		}

		public void UpdateLighting(bool applyProperty, bool forceLight = false)
		{
			Color value = s_AmbientColours[m_isInside];
			float value2 = s_AmbientIntensities[m_isInside];
			int num = m_MeshRenderers.Length;
			if (m_bUpdateLightingProperties || forceLight)
			{
				for (int i = 0; i < num; i++)
				{
					m_MeshRenderersPropertyBlock[i].SetColor(m_propId_AmbientLight, value);
					m_MeshRenderersPropertyBlock[i].SetFloat(m_propId_AmbientIntensity, value2);
				}
				m_bUpdateLightingProperties = false;
			}
			if (applyProperty || forceLight)
			{
				for (int j = 0; j < num; j++)
				{
					m_MeshRenderers[j].SetPropertyBlock(m_MeshRenderersPropertyBlock[j]);
				}
				m_bUpdateLightingProperties = true;
			}
		}

		public void CullThisObjectsParticlesSystemsFromList(FastList<ParticleSystem> particleSystemList)
		{
			for (int i = 0; i < m_Particles.Length; i++)
			{
				if (particleSystemList.Contains(m_Particles[i]))
				{
					particleSystemList.Remove(m_Particles[i]);
				}
			}
		}

		public void LogRenderables()
		{
			Debug.LogFormat("animatedWrapper:{0}", m_GameObject.name);
		}

		public override string ToString()
		{
			return (!m_GameObject) ? ("NULL: " + base.ToString()) : m_GameObject.name;
		}
	}

	[Serializable]
	public class ParticleWrapper
	{
		public ParticleSystem m_ParticleSystem;

		public ParticleSystemRenderer m_renderer;

		public ParticleControl m_Control;

		public int m_VisibilityMask;

		public int m_PreviousVisibilityMask;

		public bool m_bRendererEnabled;

		private bool m_BlockControl;

		public ParticleWrapper(ParticleSystem pS)
		{
			m_ParticleSystem = pS;
			m_Control = pS.GetComponent<ParticleControl>();
			m_renderer = pS.GetComponent<ParticleSystemRenderer>();
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if (currentLevelInfo != null && currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Transport_Train && m_ParticleSystem.name == "Engine_Smoke")
			{
				m_BlockControl = true;
			}
		}

		~ParticleWrapper()
		{
		}

		public void UpdateVisibility(int cameraID)
		{
			if (m_BlockControl)
			{
				return;
			}
			if (m_PreviousVisibilityMask != m_VisibilityMask)
			{
				if (m_VisibilityMask == 0)
				{
					if (m_Control != null)
					{
						if (!m_Control.bPlaying)
						{
							m_ParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
						}
					}
					else if (m_ParticleSystem.isPlaying)
					{
						m_ParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
					}
				}
				else if (m_Control != null)
				{
					if (m_Control.bPlaying)
					{
						m_ParticleSystem.Play();
					}
				}
				else if (!m_ParticleSystem.isPlaying)
				{
					m_ParticleSystem.Play();
				}
			}
			bool flag = (m_VisibilityMask & (1 << cameraID)) != 0;
			if (flag != m_bRendererEnabled)
			{
				m_bRendererEnabled = flag;
				if (m_renderer != null)
				{
					m_renderer.enabled = flag;
				}
				if (m_Control != null)
				{
					m_Control.bVisible = flag;
				}
			}
			m_PreviousVisibilityMask = m_VisibilityMask;
		}

		public void SetInvisible()
		{
			m_PreviousVisibilityMask = int.MaxValue;
			m_VisibilityMask = 0;
			UpdateVisibility(0);
		}

		public void SetVisibilityToCamera(int cameraID, bool visible)
		{
			if (visible)
			{
				m_VisibilityMask |= 1 << cameraID;
			}
			else
			{
				m_VisibilityMask &= ~(1 << cameraID);
			}
		}

		public void LogRenderables()
		{
			Debug.LogFormat("particleWrapper:{0}", m_ParticleSystem.name);
		}
	}

	public class EffectManagerWrapper
	{
		public GameObject m_effectGameObject;

		private bool m_enabled;

		private bool m_firstToggle = true;

		private Character m_Character;

		private CharacterWrapper m_CharacterWrapper;

		private Renderer[] m_EffectRenderers;

		public EffectManagerWrapper(GameObject effectObj, Character character)
		{
			m_effectGameObject = effectObj;
			m_Character = character;
			m_CharacterWrapper = m_Instance.FindCharacterWrapper(m_Character);
			m_EffectRenderers = m_effectGameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		}

		~EffectManagerWrapper()
		{
		}

		public void UpdateEffectVisibility(FloorManager floorMan, int floorIndex)
		{
			bool flag;
			if (m_CharacterWrapper != null)
			{
				flag = m_CharacterWrapper.m_bVis;
			}
			else
			{
				FloorManager.Floor floor = floorMan.FindRealFloorAtZ(m_effectGameObject.transform.position.z);
				flag = floor.m_FloorIndex <= floorIndex;
			}
			if (m_enabled != flag || m_firstToggle)
			{
				m_enabled = flag;
				m_firstToggle = false;
				m_effectGameObject.GetComponent<IIngameEffect>()?.PrepareForCullerVisiblity(flag);
				for (int num = m_EffectRenderers.Length - 1; num >= 0; num--)
				{
					m_EffectRenderers[num].enabled = flag;
				}
			}
		}
	}

	private enum eDirtyLighting
	{
		All = 0,
		Tiles = 0,
		Character = 1,
		Animated = 2,
		Desk = 3,
		Background = 4,
		Wrappers = 5,
		None = 6
	}

	public class ViewReturnedData
	{
		public int m_LastStaticMeshCounts;

		public int m_LastTotalMeshCounts;

		public int m_LastCharacterMeshCounts;

		public int m_LastOtherMeshCounts;

		public int m_MinZ;

		public int m_MinX;

		public int m_MaxX;

		public int m_MinY;

		public int m_MaxY;

		public int m_LevelLights;

		public int m_CharLights;

		public void Reset()
		{
			m_LastStaticMeshCounts = 0;
			m_LastTotalMeshCounts = 0;
			m_LastCharacterMeshCounts = 0;
			m_LastOtherMeshCounts = 0;
			m_MinZ = 0;
			m_MinX = 10000;
			m_MaxX = 0;
			m_MinY = 10000;
			m_MaxY = 0;
			m_LevelLights = 0;
			m_CharLights = 0;
		}
	}

	private struct SlidingWindow
	{
		public int m_minX;

		public int m_minY;

		public int m_maxX;

		public int m_maxY;

		public int m_floorIndex;

		public CameraView m_CameraView;
	}

	private class CameraData
	{
		public Vector3 m_Position;

		public SlidingWindow m_oldWindow;

		public SlidingWindow m_window;

		public CameraView m_CameraView;
	}

	public const int SCENE_CAMERA = 666;

	public Vector3 m_Min;

	public Vector3 m_Max;

	private bool m_bInited;

	private const float SCALE_EPS = 0.1f;

	private const bool START_ENABLED = true;

	public const int TOTAL_LIGHT_WRAPPER_CONTAINERS = 21;

	public const int OUTDOOR_LIGHT_WRAPPER_CONTAINER_SLOT = 20;

	private static bool m_bEnabled = true;

	private static bool m_bJustEnabled = true;

	public bool m_bArraySizeChecking = true;

	private bool[] m_ForceHideAll = new bool[4];

	private static Vector2 m_bDebugCameraSizes = new Vector2(5f, 3f);

	private static bool m_bAlwaysUpdateBucket = false;

	private FastList<CharacterWrapper> m_CharacterWrappers = new FastList<CharacterWrapper>();

	private FastList<CrowdCharacterWrapper> m_CrowdCharacterWrappers = new FastList<CrowdCharacterWrapper>();

	private LightWrapperContainer[] m_LightWrapperContainers = new LightWrapperContainer[21];

	private FastList<DeskWrapper> m_DeskWrappers = new FastList<DeskWrapper>();

	private FastList<LargeRendererWrapper> m_LargeRendererWrappers = new FastList<LargeRendererWrapper>();

	private FastList<TagWrapper> m_TagWrappers = new FastList<TagWrapper>();

	private FastList<AnimatedWrapper> m_AnimatedWrappers = new FastList<AnimatedWrapper>();

	private FastList<ParticleWrapper> m_ParticleWrappers = new FastList<ParticleWrapper>();

	private FastList<EffectManagerWrapper> m_EffectManagerGameObjects = new FastList<EffectManagerWrapper>();

	private MaterialPropertyBlock editAlreadyPresentBlock;

	private int m_lightUpdateCount;

	private static Color[] s_AmbientColours = new Color[2];

	private static float[] s_AmbientIntensities = new float[2];

	private eDirtyLighting m_LightingDirty = eDirtyLighting.None;

	private bool m_LightingPendingDirty;

	private int m_animWrapperPreCalcIndex;

	private int m_animWrapperPreCalcBudget = 20;

	private const int m_animWrapperNumUpdateFrames = 6;

	private int m_animWrapperUpdateIndex;

	private ViewReturnedData[] m_ViewDatas = new ViewReturnedData[8];

	public static int m_propId_Color = 0;

	public static int m_propId_TileHeightTint = 0;

	public static int m_propId_AmbientLight = 0;

	public static int m_propId_AmbientIntensity = 0;

	public static int m_propId_AmbientLightOther = 0;

	public static int m_propId_AmbientIntensityOther = 0;

	public static int m_propId_BumpFilp = 0;

	public static int m_propID_CharacterID = 0;

	public static int m_propID_CharacterTexture = 0;

	public static int m_globalPropID_CharacterStencilTexture = 0;

	public static int m_propID_CharacterTexture_HairMask = 0;

	public static int m_propID_CharacterTexture_Hair = 0;

	public static int m_propID_CharacterTexture_Hat = 0;

	public static int m_propID_CharacterTexture_UpperFace = 0;

	public static int m_propID_CharacterTexture_LowerFace = 0;

	private Vector2 m_MinPos = default(Vector2);

	private Vector2 m_MaxPos = default(Vector2);

	private FloorManager m_FloorManager;

	private RoomManager m_RoomManager;

	private LightingManager m_LightingManager;

	private FacadesManager m_FacadesManager;

	private CameraManager m_CameraManager;

	private CullingBuckets.Bucket[,,] m_Buckets;

	private int m_BucketsMaxX;

	private int m_BucketsMaxY;

	private Dictionary<int, CameraData> m_cameraOldPostion = new Dictionary<int, CameraData>();

	private MaterialPropertyBlock m_insidePropertyBlock;

	private MaterialPropertyBlock m_outsidePropertyBlock;

	private MaterialPropertyBlock m_DualPropertyBlock;

	private MaterialPropertyBlock m_UndergroundPropertyBlock;

	private MaterialPropertyBlock m_insideDepthPropertyBlock;

	private MaterialPropertyBlock m_outsideDepthPropertyBlock;

	private MaterialPropertyBlock m_DualDepthPropertyBlock;

	private MaterialPropertyBlock m_ventAbovePropertyBlock;

	public Color m_LowerFloorColour = Color.grey;

	[SerializeField]
	private Color m_VentAboveColourTint = new Color(0.5f, 0.5f, 0.5f, 0.6f);

	private static bool m_bForcedFacadesOff = true;

	private static CullingObjectCollector m_Instance = null;

	public Vector2 CurrentCameraMinPos => m_MinPos;

	public Vector2 CurrentCameraMaxPos => m_MaxPos;

	public LargeRendererWrapper FindLRW(int id)
	{
		int count = m_LargeRendererWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_LargeRendererWrappers[i].m_ID == id)
			{
				return m_LargeRendererWrappers[i];
			}
		}
		return null;
	}

	private bool valueInRange(int value, int min, int max)
	{
		return value >= min && value <= max;
	}

	private bool IsInsideWindow(int x, int y, int f, SlidingWindow A)
	{
		int num = A.m_floorIndex;
		if (A.m_CameraView == CameraView.VentLayerAbove)
		{
			num++;
		}
		if (f == num)
		{
			bool flag = valueInRange(x, A.m_minX, A.m_maxX);
			bool flag2 = valueInRange(y, A.m_minY, A.m_maxY);
			return flag && flag2;
		}
		return false;
	}

	private bool SlidingWindowOverlap(SlidingWindow A, SlidingWindow B)
	{
		bool flag = valueInRange(A.m_minX, B.m_minX, B.m_maxX) || valueInRange(A.m_maxX, B.m_minX, B.m_maxX) || valueInRange(B.m_minX, A.m_minX, A.m_maxX) || valueInRange(B.m_maxX, A.m_minX, A.m_maxX);
		bool flag2 = valueInRange(A.m_minY, B.m_minY, B.m_maxY) || valueInRange(A.m_maxY, B.m_minY, B.m_maxY) || valueInRange(B.m_minY, A.m_minY, A.m_maxY) || valueInRange(B.m_maxY, A.m_minY, A.m_maxY);
		return flag && flag2;
	}

	private SlidingWindow MergeWindows(SlidingWindow A, SlidingWindow B)
	{
		SlidingWindow result = default(SlidingWindow);
		result.m_minX = Math.Min(A.m_minX, B.m_minX);
		result.m_maxX = Math.Max(A.m_maxX, B.m_maxX);
		result.m_minY = Math.Min(A.m_minY, B.m_minY);
		result.m_maxY = Math.Max(A.m_maxY, B.m_maxY);
		result.m_floorIndex = Math.Max(A.m_floorIndex, B.m_floorIndex);
		return result;
	}

	public static CullingObjectCollector GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	public void Init(int ZBucketCount, List<Transform> sceneRoots)
	{
		if (m_bInited)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			m_ForceHideAll[i] = false;
		}
		int capacity = 8;
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null && currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.DLC04)
		{
			capacity = 196;
		}
		CharacterStencilRenderer.InitKeys(capacity);
		GlobalStart.TimedNetworkService();
		m_propId_Color = Shader.PropertyToID("_Color");
		m_propId_TileHeightTint = Shader.PropertyToID("_TileHeightTint");
		m_propId_AmbientLight = Shader.PropertyToID("_AmbientLight");
		m_propId_AmbientIntensity = Shader.PropertyToID("_AmbientIntensity");
		m_propId_AmbientLightOther = Shader.PropertyToID("_AmbientLightOther");
		m_propId_AmbientIntensityOther = Shader.PropertyToID("_AmbientIntensityOther");
		m_propId_BumpFilp = Shader.PropertyToID("_BumpFlip");
		m_propID_CharacterID = Shader.PropertyToID("_CharacterID");
		m_propID_CharacterTexture = Shader.PropertyToID("_CharacterTexture");
		m_globalPropID_CharacterStencilTexture = Shader.PropertyToID("_CharacterStencilTexture");
		m_propID_CharacterTexture_HairMask = Shader.PropertyToID("_HairMaskTex");
		m_propID_CharacterTexture_Hair = Shader.PropertyToID("_HairTex");
		m_propID_CharacterTexture_Hat = Shader.PropertyToID("_HatTex");
		m_propID_CharacterTexture_UpperFace = Shader.PropertyToID("_UpperAccTex");
		m_propID_CharacterTexture_LowerFace = Shader.PropertyToID("_LowerAccTex");
		GlobalStart.TimedNetworkService();
		m_FloorManager = FloorManager.GetInstance();
		m_RoomManager = RoomManager.GetInstance();
		m_LightingManager = LightingManager.GetInstance();
		m_FacadesManager = FacadesManager.GetInstance();
		m_CameraManager = CameraManager.GetInstance();
		editAlreadyPresentBlock = new MaterialPropertyBlock();
		m_insidePropertyBlock = new MaterialPropertyBlock();
		m_outsidePropertyBlock = new MaterialPropertyBlock();
		m_DualPropertyBlock = new MaterialPropertyBlock();
		m_UndergroundPropertyBlock = new MaterialPropertyBlock();
		m_insideDepthPropertyBlock = new MaterialPropertyBlock();
		m_outsideDepthPropertyBlock = new MaterialPropertyBlock();
		m_DualDepthPropertyBlock = new MaterialPropertyBlock();
		m_ventAbovePropertyBlock = new MaterialPropertyBlock();
		GlobalStart.TimedNetworkService();
		MaterialPropertyBlock[] array = new MaterialPropertyBlock[7] { m_insidePropertyBlock, m_outsidePropertyBlock, m_DualPropertyBlock, m_UndergroundPropertyBlock, m_insideDepthPropertyBlock, m_outsideDepthPropertyBlock, m_DualDepthPropertyBlock };
		for (int j = 0; j < array.Length; j++)
		{
			array[j].SetColor(m_propId_Color, Color.white);
			array[j].SetColor(m_propId_TileHeightTint, Color.white);
			array[j].SetFloat(m_propId_BumpFilp, 1f);
		}
		m_ventAbovePropertyBlock.SetColor(m_propId_Color, m_VentAboveColourTint);
		m_ventAbovePropertyBlock.SetColor(m_propId_TileHeightTint, Color.white);
		GlobalStart.TimedNetworkService();
		m_ParticleWrappers.Clear();
		m_AnimatedWrappers.Clear();
		m_CharacterWrappers.Clear();
		m_CrowdCharacterWrappers.Clear();
		m_DeskWrappers.Clear();
		m_LargeRendererWrappers.Clear();
		for (int k = 0; k < m_LightWrapperContainers.Length; k++)
		{
			m_LightWrapperContainers[k] = new LightWrapperContainer();
		}
		GlobalStart.TimedNetworkService();
		CullingBuckets.CreateBuckets(sceneRoots, m_ParticleWrappers, m_AnimatedWrappers, m_CharacterWrappers, m_DeskWrappers, m_LightWrapperContainers, m_LargeRendererWrappers, LevelScript.GetInstance().m_PreBuildBuckets, bUpdateNetworkService: true);
		GlobalStart.TimedNetworkService();
		for (int num = m_CharacterWrappers.Count - 1; num >= 0; num--)
		{
			if (m_CharacterWrappers._items[num].m_Character.m_CharacterRole == CharacterRole.Crowd)
			{
				CrowdCharacterWrapper crowdCharacterWrapper = new CrowdCharacterWrapper(m_CharacterWrappers._items[num]);
				crowdCharacterWrapper.Init();
				m_CrowdCharacterWrappers.Add(crowdCharacterWrapper);
				m_CharacterWrappers.RemoveAt(num);
			}
		}
		GlobalStart.TimedNetworkService();
		for (int l = 0; l < 8; l++)
		{
			m_ViewDatas[l] = new ViewReturnedData();
			m_ViewDatas[l].Reset();
		}
		m_Buckets = CullingBuckets.s_buckets;
		m_BucketsMaxX = m_Buckets.GetLength(0) - 1;
		m_BucketsMaxY = m_Buckets.GetLength(1) - 1;
		if (m_LightingManager != null)
		{
			LightingManager lightingManager = m_LightingManager;
			lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Combine(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(OnLightingUpdated));
			LightingManager lightingManager2 = m_LightingManager;
			lightingManager2.OnLightingPreCalc = (LightingManager.LightingPreCalc)Delegate.Combine(lightingManager2.OnLightingPreCalc, new LightingManager.LightingPreCalc(PreCalcAnimatedLighting));
		}
		SlidingWindow camWindow = default(SlidingWindow);
		camWindow.m_minX = 0;
		camWindow.m_minY = 0;
		camWindow.m_maxX = m_BucketsMaxX;
		camWindow.m_maxY = m_BucketsMaxY;
		int num2 = m_Buckets.GetLength(2) - 1;
		for (int m = 0; m <= num2; m++)
		{
			camWindow.m_floorIndex = m;
			DisableTiles(camWindow, 0, CameraView.Normal, 0);
			GlobalStart.TimedNetworkService();
		}
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		}
		m_bInited = true;
		CullingBuckets.ProcessBucketsToUpdateAfterInitalise();
		GlobalStart.TimedNetworkService();
	}

	protected virtual void OnDestroy()
	{
		if (m_LightingManager != null)
		{
			LightingManager lightingManager = m_LightingManager;
			lightingManager.OnLightingUpdated = (LightingManager.LightingUpdated)Delegate.Remove(lightingManager.OnLightingUpdated, new LightingManager.LightingUpdated(OnLightingUpdated));
			LightingManager lightingManager2 = m_LightingManager;
			lightingManager2.OnLightingPreCalc = (LightingManager.LightingPreCalc)Delegate.Remove(lightingManager2.OnLightingPreCalc, new LightingManager.LightingPreCalc(PreCalcAnimatedLighting));
		}
		if (m_CameraManager != null)
		{
			CameraManager cameraManager = m_CameraManager;
			cameraManager.OnActiveCamerasUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(cameraManager.OnActiveCamerasUpdated, new CameraManager.CameraManagerHandler(ActiveCamerasUpdated));
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		OnDisable();
		CullingBuckets.CleanUp();
	}

	public void ForceCleanup()
	{
		OnDisable();
		CullingBuckets.CleanUp();
	}

	private void OnDisable()
	{
		m_CharacterWrappers.Clear();
		m_CrowdCharacterWrappers.Clear();
		for (int i = 0; i < m_LightWrapperContainers.Length; i++)
		{
			if (m_LightWrapperContainers[i] != null)
			{
				m_LightWrapperContainers[i].Clear();
			}
		}
		m_DeskWrappers.Clear();
		m_TagWrappers.Clear();
		m_AnimatedWrappers.Clear();
		m_ParticleWrappers.Clear();
		m_LargeRendererWrappers.Clear();
		m_bEnabled = true;
		m_bJustEnabled = true;
		m_bDebugCameraSizes = new Vector2(5f, 3f);
		m_bAlwaysUpdateBucket = false;
		m_cameraOldPostion.Clear();
	}

	private void ActiveCamerasUpdated()
	{
		Dictionary<int, CameraData>.Enumerator enumerator = m_cameraOldPostion.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			CameraData value = enumerator.Current.Value;
			SlidingWindow window = value.m_window;
			DisableTiles(window, key, value.m_CameraView, 0);
		}
		m_cameraOldPostion.Clear();
	}

	private void SetBakedFloorQuadVisibility(CullingBuckets.Bucket bucket, bool isVisible)
	{
		for (int i = 0; i < bucket.m_bakedFloorQuads.Length; i++)
		{
			MeshRenderer meshRenderer = bucket.m_bakedFloorQuads[i];
			if (meshRenderer != null)
			{
				meshRenderer.enabled = isVisible;
			}
		}
	}

	private void SetBakedFloorQuadColour(CullingBuckets.Bucket bucket, bool isUnderground, bool bColourInVentOnly, bool isInVent, CameraView cameraView)
	{
		for (int i = 0; i < bucket.m_bakedFloorQuads.Length; i++)
		{
			MeshRenderer meshRenderer = bucket.m_bakedFloorQuads[i];
			if (!(meshRenderer != null))
			{
				continue;
			}
			if (isUnderground)
			{
				meshRenderer.SetPropertyBlock(m_UndergroundPropertyBlock);
			}
			else if (cameraView == CameraView.VentLayerAbove)
			{
				if (i == 0)
				{
					meshRenderer.material.SetOverrideTag("RenderType", "Transparent");
					meshRenderer.material.SetInt("_SrcBlend", 5);
					meshRenderer.material.SetInt("_DstBlend", 10);
					meshRenderer.material.SetInt("_SrcFactorA", 0);
					meshRenderer.material.SetInt("_DstFactorA", 0);
					meshRenderer.material.renderQueue = 3000;
					meshRenderer.SetPropertyBlock(m_ventAbovePropertyBlock);
				}
				else
				{
					SetRendererColour(meshRenderer, AMBIENT_LIGHT_MODE.ALM_DUAL, (!bColourInVentOnly) ? (i - 1) : 0);
				}
			}
			else
			{
				if (meshRenderer.material.renderQueue == 3000)
				{
					meshRenderer.material.SetOverrideTag("RenderType", "TransparentCutout");
					meshRenderer.material.SetInt("_SrcBlend", 1);
					meshRenderer.material.SetInt("_DstBlend", 0);
					meshRenderer.material.SetInt("_SrcFactorA", 1);
					meshRenderer.material.SetInt("_DstFactorA", 0);
					meshRenderer.material.renderQueue = 2450;
				}
				SetRendererColour(meshRenderer, AMBIENT_LIGHT_MODE.ALM_DUAL, (!bColourInVentOnly || isInVent) ? i : 0);
			}
		}
	}

	private void SetBuildingVisibility(CullingBuckets.Bucket bucket, int cameraID, bool isVisible)
	{
		FastList<AnimatedWrapper> fastList = null;
		FastList<LargeRendererWrapper> fastList2 = null;
		Dictionary<int, CullingBuckets.BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CullingBuckets.BuildingRenderables value = enumerator.Current.Value;
			for (int i = 0; i < value.m_particles.Count; i++)
			{
				value.m_particles._items[i].SetVisibilityToCamera(cameraID, isVisible);
			}
			Dictionary<int, FastList<AnimatedWrapper>>.Enumerator enumerator2 = value.m_animated.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				fastList = enumerator2.Current.Value;
				if (fastList != null)
				{
					for (int j = 0; j < fastList.Count; j++)
					{
						fastList._items[j].SetCameraVisibility(cameraID, isVisible);
					}
				}
			}
			Dictionary<int, FastList<LargeRendererWrapper>>.Enumerator enumerator3 = value.m_LargeRenderer.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				fastList2 = enumerator3.Current.Value;
				if (fastList2 != null)
				{
					for (int k = 0; k < fastList2.Count; k++)
					{
						fastList2._items[k].SetCameraVisibility(cameraID, isVisible, bucket);
					}
				}
			}
			Dictionary<int, FastList<CullingBuckets.MeshInfo>>.Enumerator enumerator4 = value.m_RenderersWithCustomMaterialBlocks.GetEnumerator();
			FastList<CullingBuckets.MeshInfo> fastList3 = new FastList<CullingBuckets.MeshInfo>();
			while (enumerator4.MoveNext())
			{
				fastList3 = enumerator4.Current.Value;
				if (fastList3 != null)
				{
					for (int l = 0; l < fastList3.Count; l++)
					{
						fastList3._items[l].m_renderer.enabled = isVisible;
					}
				}
			}
			RenderMeshList(value.m_meshesIndi, isVisible);
			if (value.m_meshesDamagableCombined.Count == 0)
			{
				RenderMeshList(value.m_meshesDamagableOrig, isVisible);
			}
			else
			{
				RenderMeshList(value.m_meshesDamagableCombined, isVisible);
			}
		}
	}

	private void SetMeshListColour(Dictionary<int, FastList<CullingBuckets.MeshInfo>> meshList, CameraView cameraView, bool isInVent, bool isUnderground, bool bColourInVentOnly)
	{
		Dictionary<int, FastList<CullingBuckets.MeshInfo>>.Enumerator enumerator = meshList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			FastList<CullingBuckets.MeshInfo> value = enumerator.Current.Value;
			if (value == null)
			{
				continue;
			}
			for (int i = 0; i < value.Count; i++)
			{
				if (value._items[i] == null || value._items[i].m_renderer == null)
				{
					continue;
				}
				if (isUnderground)
				{
					value._items[i].m_renderer.SetPropertyBlock(m_UndergroundPropertyBlock);
				}
				else if (cameraView == CameraView.VentLayerAbove)
				{
					if (key == 0)
					{
						value._items[i].m_renderer.SetPropertyBlock(m_ventAbovePropertyBlock);
					}
					else
					{
						SetRendererColour(value._items[i].m_renderer, value._items[i].m_AmbientLightMode, (!bColourInVentOnly) ? (key - 1) : 0);
					}
				}
				else
				{
					SetRendererColour(value._items[i].m_renderer, value._items[i].m_AmbientLightMode, (!bColourInVentOnly || isInVent) ? key : 0);
				}
			}
		}
	}

	private void SetLargeRendererListColour(Dictionary<int, FastList<LargeRendererWrapper>> largeRendererList, CameraView cameraView, bool isInVent, bool isUnderground, bool bColourInVentOnly)
	{
		Dictionary<int, FastList<LargeRendererWrapper>>.Enumerator enumerator = largeRendererList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			FastList<LargeRendererWrapper> value = enumerator.Current.Value;
			if (value == null)
			{
				continue;
			}
			for (int i = 0; i < value.Count; i++)
			{
				if (value._items[i] == null || value._items[i].m_MeshRenderers == null)
				{
					continue;
				}
				for (int j = 0; j < value._items[i].m_MeshRenderers.Length; j++)
				{
					if (isUnderground)
					{
						value._items[i].m_MeshRenderers[j].SetPropertyBlock(m_UndergroundPropertyBlock);
					}
					else if (cameraView == CameraView.VentLayerAbove)
					{
						if (key == 0)
						{
							value._items[i].m_MeshRenderers[j].SetPropertyBlock(m_ventAbovePropertyBlock);
						}
						else
						{
							SetRendererColour(value._items[i].m_MeshRenderers[j], AMBIENT_LIGHT_MODE.ALM_OUTSIDE, (!bColourInVentOnly) ? (key - 1) : 0);
						}
					}
					else
					{
						SetRendererColour(value._items[i].m_MeshRenderers[j], AMBIENT_LIGHT_MODE.ALM_OUTSIDE, (!bColourInVentOnly || isInVent) ? key : 0);
					}
				}
			}
		}
	}

	private void OutputSlidingWindow(string name, SlidingWindow window)
	{
	}

	private void OutputWindow(string name, int x, int y, int floor, bool isVisible, int caller)
	{
	}

	private void UpdateTiles(int cameraID, CameraData camData, SlidingWindow newWindow, CameraView cameraView, bool force, int caller)
	{
		int num = Math.Min(newWindow.m_minX, camData.m_window.m_minX);
		int num2 = Math.Max(newWindow.m_maxX, camData.m_window.m_maxX);
		int num3 = Math.Min(newWindow.m_minY, camData.m_window.m_minY);
		int num4 = Math.Max(newWindow.m_maxY, camData.m_window.m_maxY);
		int num5 = newWindow.m_floorIndex;
		if (cameraView == CameraView.VentLayerAbove)
		{
			num5++;
		}
		bool isUnderground = false;
		bool isInVent = false;
		bool bColourInVentOnly = true;
		if (m_FloorManager != null)
		{
			FloorManager.Floor floor = m_FloorManager.FindFloorbyIndex(num5);
			isInVent = cameraView == CameraView.Normal && floor.IsVent();
			isUnderground = floor.IsUnderGround();
		}
		SlidingWindow window = camData.m_window;
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				CullingBuckets.Bucket bucket = m_Buckets[i, j, num5];
				if (bucket == null)
				{
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				if (force)
				{
					flag = true;
					flag2 = true;
				}
				else
				{
					bool flag4 = IsInsideWindow(i, j, num5, window);
					bool flag5 = IsInsideWindow(i, j, num5, newWindow);
					if (flag5 && bucket.m_forceUpdate)
					{
						flag = true;
						flag2 = true;
						flag3 = true;
						bucket.m_forceUpdate = false;
					}
					else if (flag5 && flag4)
					{
						flag = true;
						flag2 = true;
					}
					else if (flag4 && !flag5)
					{
						flag = true;
						flag2 = false;
					}
				}
				if (!flag)
				{
					continue;
				}
				bucket.SetCameraVisibility(cameraID, flag2);
				if (!m_bAlwaysUpdateBucket)
				{
					if (flag2 != bucket.m_isVisible || force)
					{
						flag3 = true;
					}
				}
				else
				{
					flag3 = true;
				}
				if (!flag3)
				{
					continue;
				}
				SetBakedFloorQuadVisibility(bucket, flag2);
				SetBuildingVisibility(bucket, cameraID, flag2);
				if (flag2)
				{
					UpdateBucketColours(bucket, isUnderground, bColourInVentOnly, isInVent, cameraID, cameraView, bTileAdded: true);
					if (bucket.m_RequiresBatching)
					{
						CullingBuckets.RequestBucketDamagableUpdate(bucket, bHighPriority: false);
						bucket.m_RequiresBatching = false;
					}
				}
				bucket.UpdateVisibilityState(flag2);
			}
		}
	}

	private void UpdateTileWindow(SlidingWindow camWindow, int cameraID, CameraView cameraView, bool bForceVis, int caller)
	{
		int num = camWindow.m_floorIndex;
		if (cameraView == CameraView.VentLayerAbove)
		{
			num++;
		}
		bool isUnderground = false;
		bool isInVent = false;
		bool bColourInVentOnly = true;
		if (m_FloorManager != null)
		{
			FloorManager.Floor floor = m_FloorManager.FindFloorbyIndex(num);
			isInVent = cameraView == CameraView.Normal && floor.IsVent();
			isUnderground = floor.IsUnderGround();
		}
		for (int i = camWindow.m_minX; i <= camWindow.m_maxX; i++)
		{
			for (int j = camWindow.m_minY; j <= camWindow.m_maxY; j++)
			{
				CullingBuckets.Bucket bucket = m_Buckets[i, j, num];
				if (bucket == null)
				{
					continue;
				}
				bool flag = false;
				if (bucket.m_forceUpdate)
				{
					flag = true;
					bucket.m_forceUpdate = false;
				}
				else if ((bucket.m_cameraVisMask & (1 << cameraID)) > 0)
				{
					if (!bucket.m_isVisible || bForceVis)
					{
						flag = true;
					}
				}
				else
				{
					bucket.SetCameraVisibility(cameraID, isVisible: true);
					if (!bucket.m_isVisible || bForceVis)
					{
						flag = true;
					}
				}
				if (flag)
				{
					SetBakedFloorQuadVisibility(bucket, isVisible: true);
					SetBuildingVisibility(bucket, cameraID, isVisible: true);
					UpdateBucketColours(bucket, isUnderground, bColourInVentOnly, isInVent, cameraID, cameraView, bTileAdded: true);
					bucket.UpdateVisibilityState(isVisible: true);
					if (bucket.m_RequiresBatching)
					{
						CullingBuckets.RequestBucketDamagableUpdate(bucket, bHighPriority: false);
						bucket.m_RequiresBatching = false;
					}
				}
			}
		}
	}

	private void DisableTiles(SlidingWindow camWindow, int cameraID, CameraView cameraView, int caller)
	{
		int length = m_Buckets.GetLength(2);
		for (int i = camWindow.m_minX; i <= camWindow.m_maxX; i++)
		{
			for (int j = camWindow.m_minY; j <= camWindow.m_maxY; j++)
			{
				for (int k = 0; k < length; k++)
				{
					CullingBuckets.Bucket bucket = m_Buckets[i, j, k];
					if (bucket != null)
					{
						SetBakedFloorQuadVisibility(bucket, isVisible: false);
						SetBuildingVisibility(bucket, cameraID, isVisible: false);
						bucket.UpdateVisibilityState(isVisible: false);
					}
				}
			}
		}
	}

	private void UpdateTileLighting()
	{
		m_lightUpdateCount++;
		UpdateLightingPropertyBlocks();
		Dictionary<int, CameraData>.Enumerator enumerator = m_cameraOldPostion.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			CameraData value = enumerator.Current.Value;
			SlidingWindow window = value.m_window;
			CameraView cameraView = value.m_CameraView;
			int num = window.m_floorIndex;
			if (cameraView == CameraView.VentLayerAbove)
			{
				num++;
			}
			bool isUnderground = false;
			bool isInVent = false;
			bool bColourInVentOnly = true;
			if (m_FloorManager != null)
			{
				FloorManager.Floor floor = m_FloorManager.FindFloorbyIndex(num);
				isInVent = cameraView == CameraView.Normal && floor.IsVent();
				isUnderground = floor.IsUnderGround();
			}
			for (int i = window.m_minX; i <= window.m_maxX; i++)
			{
				for (int j = window.m_minY; j <= window.m_maxY; j++)
				{
					CullingBuckets.Bucket bucket = m_Buckets[i, j, num];
					if (bucket != null)
					{
						UpdateBucketColours(bucket, isUnderground, bColourInVentOnly, isInVent, key, cameraView);
					}
				}
			}
		}
	}

	private void UpdateBucketColours(CullingBuckets.Bucket bucket, bool isUnderground, bool bColourInVentOnly, bool isInVent, int camID, CameraView camView, bool bTileAdded = false)
	{
		if (!bTileAdded && bucket.m_lightingLastTouched == m_lightUpdateCount)
		{
			return;
		}
		SetBakedFloorQuadColour(bucket, isUnderground, bColourInVentOnly, isInVent, camView);
		Dictionary<int, CullingBuckets.BuildingRenderables>.Enumerator enumerator = bucket.m_renderables.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CullingBuckets.BuildingRenderables value = enumerator.Current.Value;
			SetMeshListColour(value.m_meshesIndi, camView, isInVent, isUnderground, bColourInVentOnly);
			if (value.m_meshesDamagableCombined.Count == 0)
			{
				SetMeshListColour(value.m_meshesDamagableOrig, camView, isInVent, isUnderground, bColourInVentOnly);
			}
			else
			{
				SetMeshListColour(value.m_meshesDamagableCombined, camView, isInVent, isUnderground, bColourInVentOnly);
			}
			SetLargeRendererListColour(value.m_LargeRenderer, camView, isInVent, isUnderground, bColourInVentOnly);
		}
		bucket.m_lightingLastTouched = m_lightUpdateCount;
	}

	private void UpdateLightingPropertyBlocks()
	{
		Color currentAmbientColor = m_LightingManager.GetCurrentAmbientColor(inside: true);
		Color currentAmbientColor2 = m_LightingManager.GetCurrentAmbientColor(inside: false);
		float currentAmbientIntensity = m_LightingManager.GetCurrentAmbientIntensity(inside: true);
		float currentAmbientIntensity2 = m_LightingManager.GetCurrentAmbientIntensity(inside: false);
		float currentAmbientDepthIntensityFactor = m_LightingManager.GetCurrentAmbientDepthIntensityFactor(inside: true);
		float currentAmbientDepthIntensityFactor2 = m_LightingManager.GetCurrentAmbientDepthIntensityFactor(inside: false);
		m_insidePropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
		m_insidePropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
		m_outsidePropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor2);
		m_outsidePropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity2);
		m_DualPropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
		m_DualPropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
		m_DualPropertyBlock.SetColor(m_propId_AmbientLightOther, currentAmbientColor2);
		m_DualPropertyBlock.SetFloat(m_propId_AmbientIntensityOther, currentAmbientIntensity2);
		m_DualDepthPropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
		m_DualDepthPropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity * currentAmbientDepthIntensityFactor);
		m_DualDepthPropertyBlock.SetColor(m_propId_AmbientLightOther, currentAmbientColor2);
		m_DualDepthPropertyBlock.SetFloat(m_propId_AmbientIntensityOther, currentAmbientIntensity2 * currentAmbientDepthIntensityFactor2);
		Color underGroundAmbientLightColour = m_LightingManager.GetUnderGroundAmbientLightColour();
		float underGroundAmbientLightIntensity = m_LightingManager.GetUnderGroundAmbientLightIntensity();
		m_UndergroundPropertyBlock.SetColor(m_propId_AmbientLight, underGroundAmbientLightColour);
		m_UndergroundPropertyBlock.SetFloat(m_propId_AmbientIntensity, underGroundAmbientLightIntensity);
		m_UndergroundPropertyBlock.SetColor(m_propId_AmbientLightOther, underGroundAmbientLightColour);
		m_UndergroundPropertyBlock.SetFloat(m_propId_AmbientIntensityOther, underGroundAmbientLightIntensity);
		m_insideDepthPropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
		m_insideDepthPropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity * currentAmbientDepthIntensityFactor);
		m_outsideDepthPropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor2);
		m_outsideDepthPropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity2 * currentAmbientDepthIntensityFactor2);
		m_ventAbovePropertyBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
		m_ventAbovePropertyBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
	}

	private void RenderMeshList(Dictionary<int, FastList<CullingBuckets.MeshInfo>> meshList, bool isVisible)
	{
		Dictionary<int, FastList<CullingBuckets.MeshInfo>>.Enumerator enumerator = meshList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			int key = enumerator.Current.Key;
			FastList<CullingBuckets.MeshInfo> value = enumerator.Current.Value;
			if (value == null)
			{
				continue;
			}
			for (int i = 0; i < value.Count; i++)
			{
				if (value._items[i] != null && !(value._items[i].m_renderer == null))
				{
					value._items[i].m_renderer.enabled = isVisible;
				}
			}
		}
	}

	private bool CollectMeshRenders(Vector3 position, int cameraID, Vector3 cameraViewSize, CameraView cameraView, int floorIndexOfPosition)
	{
		bool bJustEnabled = m_bJustEnabled;
		if (m_bEnabled && !m_bJustEnabled)
		{
			CameraData cameraData = null;
			SlidingWindow slidingWindow = default(SlidingWindow);
			slidingWindow.m_minX = 0;
			slidingWindow.m_minY = 0;
			slidingWindow.m_maxX = m_BucketsMaxX;
			slidingWindow.m_maxY = m_BucketsMaxY;
			slidingWindow.m_CameraView = cameraView;
			if (!m_cameraOldPostion.ContainsKey(cameraID))
			{
				cameraData = new CameraData();
				CullingBuckets.GetBucketCoordsForWorldPosition(position - cameraViewSize, ref slidingWindow.m_minX, ref slidingWindow.m_maxY);
				CullingBuckets.GetBucketCoordsForWorldPosition(position + cameraViewSize, ref slidingWindow.m_maxX, ref slidingWindow.m_minY);
				slidingWindow.m_floorIndex = floorIndexOfPosition;
				cameraData.m_Position = position;
				cameraData.m_window = slidingWindow;
				cameraData.m_oldWindow = slidingWindow;
				cameraData.m_CameraView = cameraView;
				m_cameraOldPostion.Add(cameraID, cameraData);
				UpdateTiles(cameraID, cameraData, slidingWindow, cameraView, force: true, 0);
			}
			else
			{
				cameraData = m_cameraOldPostion[cameraID];
				bool force = false;
				cameraData.m_oldWindow = cameraData.m_window;
				CullingBuckets.GetBucketCoordsForWorldPosition(position - cameraViewSize, ref slidingWindow.m_minX, ref slidingWindow.m_maxY);
				CullingBuckets.GetBucketCoordsForWorldPosition(position + cameraViewSize, ref slidingWindow.m_maxX, ref slidingWindow.m_minY);
				slidingWindow.m_floorIndex = floorIndexOfPosition;
				bool flag = false;
				Dictionary<int, CameraData>.Enumerator enumerator = m_cameraOldPostion.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Key == cameraID)
					{
						continue;
					}
					CameraData value = enumerator.Current.Value;
					SlidingWindow a = MergeWindows(value.m_oldWindow, value.m_window);
					flag = false;
					if ((cameraView == CameraView.VentLayerAbove || value.m_CameraView == CameraView.VentLayerAbove) && cameraView != value.m_CameraView)
					{
						flag = true;
					}
					if (cameraView == CameraView.Facade && slidingWindow.m_floorIndex < a.m_floorIndex)
					{
						flag = true;
					}
					if (SlidingWindowOverlap(a, slidingWindow))
					{
						SlidingWindow camWindow = default(SlidingWindow);
						camWindow.m_minX = Math.Max(a.m_minX, slidingWindow.m_minX);
						camWindow.m_maxX = Math.Min(a.m_maxX, slidingWindow.m_maxX);
						camWindow.m_minY = Math.Max(a.m_minY, slidingWindow.m_minY);
						camWindow.m_maxY = Math.Min(a.m_maxY, slidingWindow.m_maxY);
						camWindow.m_floorIndex = a.m_floorIndex;
						if (slidingWindow.m_floorIndex < a.m_floorIndex || flag)
						{
							DisableTiles(camWindow, cameraID, value.m_CameraView, 0);
							camWindow.m_floorIndex = slidingWindow.m_floorIndex;
							UpdateTileWindow(camWindow, cameraID, cameraView, bForceVis: true, 0);
						}
						else
						{
							camWindow.m_floorIndex = slidingWindow.m_floorIndex;
							bool bForceVis = slidingWindow.m_floorIndex > a.m_floorIndex;
							UpdateTileWindow(camWindow, cameraID, cameraView, bForceVis, 0);
						}
					}
				}
				flag = false;
				if ((cameraView == CameraView.VentLayerAbove || cameraData.m_CameraView == CameraView.VentLayerAbove) && cameraView != cameraData.m_CameraView)
				{
					flag = true;
				}
				bool flag2 = SlidingWindowOverlap(cameraData.m_oldWindow, slidingWindow);
				if (slidingWindow.m_floorIndex != cameraData.m_window.m_floorIndex || !flag2 || flag)
				{
					DisableTiles(cameraData.m_window, cameraID, cameraData.m_CameraView, 1);
					cameraData.m_Position = position;
					cameraData.m_window = slidingWindow;
					cameraData.m_CameraView = cameraView;
					UpdateTiles(cameraID, cameraData, slidingWindow, cameraView, force: true, 2);
				}
				else
				{
					UpdateTiles(cameraID, cameraData, slidingWindow, cameraView, force, 3);
					cameraData.m_window = slidingWindow;
					cameraData.m_Position = position;
					cameraData.m_CameraView = cameraView;
				}
			}
			return true;
		}
		return m_bJustEnabled;
	}

	private RoomBlob GetRendererRoom(MeshRenderer renderer, int floorIndex)
	{
		RoomFloor floorFromIndex = m_RoomManager.GetFloorFromIndex(floorIndex);
		Vector3 vector = RoomUtility.WorldToRoomGrid(renderer.transform.position, floorFromIndex);
		return m_RoomManager.LookUpRoom((int)vector.x, (int)vector.y, floorFromIndex);
	}

	private RoomBlob GetGameObjectRoom(Transform tran, int floorIndex)
	{
		RoomFloor floorFromIndex = m_RoomManager.GetFloorFromIndex(floorIndex);
		Vector3 vector = RoomUtility.WorldToRoomGrid(tran.position, floorFromIndex);
		return m_RoomManager.LookUpRoom((int)vector.x, (int)vector.y, floorFromIndex);
	}

	private void SetRendererColour(MeshRenderer tile, AMBIENT_LIGHT_MODE ambientLightMode, int depth = 0, bool bIsAnimatedWarpper = false)
	{
		MaterialPropertyBlock propertyBlock = null;
		if (bIsAnimatedWarpper)
		{
			tile.GetPropertyBlock(editAlreadyPresentBlock);
			switch (ambientLightMode)
			{
			case AMBIENT_LIGHT_MODE.ALM_INSIDE:
			{
				Color currentAmbientColor2 = m_LightingManager.GetCurrentAmbientColor(inside: true);
				float currentAmbientIntensity2 = m_LightingManager.GetCurrentAmbientIntensity(inside: true);
				if (depth == 0)
				{
					editAlreadyPresentBlock.SetColor(m_propId_AmbientLight, currentAmbientColor2);
					editAlreadyPresentBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity2);
				}
				else
				{
					float currentAmbientDepthIntensityFactor2 = m_LightingManager.GetCurrentAmbientDepthIntensityFactor(inside: true);
					editAlreadyPresentBlock.SetColor(m_propId_AmbientLight, currentAmbientColor2);
					editAlreadyPresentBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity2 * currentAmbientDepthIntensityFactor2);
				}
				break;
			}
			case AMBIENT_LIGHT_MODE.ALM_OUTSIDE:
			{
				Color currentAmbientColor = m_LightingManager.GetCurrentAmbientColor(inside: false);
				float currentAmbientIntensity = m_LightingManager.GetCurrentAmbientIntensity(inside: false);
				if (depth == 0)
				{
					editAlreadyPresentBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
					editAlreadyPresentBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity);
				}
				else
				{
					float currentAmbientDepthIntensityFactor = m_LightingManager.GetCurrentAmbientDepthIntensityFactor(inside: false);
					editAlreadyPresentBlock.SetColor(m_propId_AmbientLight, currentAmbientColor);
					editAlreadyPresentBlock.SetFloat(m_propId_AmbientIntensity, currentAmbientIntensity * currentAmbientDepthIntensityFactor);
				}
				break;
			}
			}
			tile.SetPropertyBlock(editAlreadyPresentBlock);
		}
		else
		{
			switch (ambientLightMode)
			{
			case AMBIENT_LIGHT_MODE.ALM_INSIDE:
				propertyBlock = ((depth != 0) ? m_insideDepthPropertyBlock : m_insidePropertyBlock);
				break;
			case AMBIENT_LIGHT_MODE.ALM_OUTSIDE:
				propertyBlock = ((depth != 0) ? m_outsideDepthPropertyBlock : m_outsidePropertyBlock);
				break;
			case AMBIENT_LIGHT_MODE.ALM_DUAL:
				propertyBlock = ((depth != 0) ? m_DualDepthPropertyBlock : m_DualPropertyBlock);
				break;
			}
			tile.SetPropertyBlock(propertyBlock);
		}
	}

	public void InGameAddDynamic(GameObject obj)
	{
		if (obj.GetComponent<Tag>() != null)
		{
			TagWrapper tagWrapper = new TagWrapper(obj);
			tagWrapper.UpdateLogic(1, 0);
			tagWrapper.Init();
			m_TagWrappers.Add(tagWrapper);
		}
	}

	public void InGameRemoveDynamic(GameObject obj)
	{
		if (!(obj.GetComponent<Tag>() != null))
		{
			return;
		}
		for (int num = m_TagWrappers.Count - 1; num >= 0; num--)
		{
			if (m_TagWrappers._items[num].m_GameObject == obj)
			{
				m_TagWrappers.RemoveAt(num);
				break;
			}
		}
	}

	public void RemoveCharacter(Character character)
	{
		for (int num = m_CharacterWrappers.Count - 1; num >= 0; num--)
		{
			if (m_CharacterWrappers._items[num].m_Character == character)
			{
				m_CharacterWrappers.RemoveAt(num);
				break;
			}
		}
	}

	public CharacterWrapper FindCharacterWrapper(Character character)
	{
		for (int num = m_CharacterWrappers.Count - 1; num >= 0; num--)
		{
			if (m_CharacterWrappers._items[num].m_Character == character)
			{
				return m_CharacterWrappers._items[num];
			}
		}
		return null;
	}

	public void Runtime_RemoveFromBucket(MeshRenderer meshRenderer, bool bCheckForMaterialBlock = false)
	{
		CullingBuckets.RemoveFromBucketAtRuntime(meshRenderer, bCheckForMaterialBlock);
	}

	public void Runtime_AddToBucket(MeshRenderer meshRenderer, bool bCheckForMaterialBlock = false, bool bAlsoFloorsAbove = false, int forceBuildingID = -1)
	{
		bool bCheckForMaterialBlock2 = bCheckForMaterialBlock;
		bool bAlsoFloorsAbove2 = bAlsoFloorsAbove;
		CullingBuckets.AddToBucketAtRuntime(meshRenderer, visibleThroughFacade: true, visisbleFromFloorsAbove: true, bCheckForMaterialBlock2, bCombined: false, bAlsoFloorsAbove2, forceBuildingID);
	}

	public void Runtime_AddAnimWrapper(AnimatedWrapper animWrapper)
	{
		CullingBuckets.AddAnimWrapperAtRuntime(animWrapper);
		m_AnimatedWrappers.Add(animWrapper);
		animWrapper.m_bNeedUpdate = true;
	}

	public void AddRuntimeEffect(GameObject obj, Character character)
	{
		if (!m_EffectManagerGameObjects.Exists((EffectManagerWrapper x) => x.m_effectGameObject == obj))
		{
			m_EffectManagerGameObjects.Add(new EffectManagerWrapper(obj, character));
		}
	}

	public void RemoveRuntimeEffect(GameObject obj)
	{
		EffectManagerWrapper effectManagerWrapper = m_EffectManagerGameObjects.Find((EffectManagerWrapper x) => x.m_effectGameObject == obj);
		if (effectManagerWrapper != null)
		{
			m_EffectManagerGameObjects.Remove(effectManagerWrapper);
		}
	}

	public void HideAllMode(bool bHide, CameraManager.PlayerBindingID bindingID)
	{
		int cameraIDFromBinding = CameraManager.GetInstance().GetCameraIDFromBinding(bindingID);
		if (cameraIDFromBinding >= 0 && cameraIDFromBinding < 4)
		{
			m_ForceHideAll[cameraIDFromBinding] = bHide;
		}
	}

	public bool GetHideAll(ref bool[] each)
	{
		bool flag = false;
		for (int i = 0; i < 4; i++)
		{
			each[i] = m_ForceHideAll[i];
			flag |= m_ForceHideAll[i];
		}
		return flag;
	}

	public void SetHideAll(bool[] each)
	{
		for (int i = 0; i < 4; i++)
		{
			m_ForceHideAll[i] = each[i];
		}
	}

	public void SetHideAll(bool all)
	{
		for (int i = 0; i < 4; i++)
		{
			m_ForceHideAll[i] = all;
		}
	}

	public bool IsEnabled()
	{
		return m_bEnabled;
	}

	public bool GetMeshRenderers(Vector3 position, int cameraID, Vector3 cameraViewSize, bool updateMR, Character characterTarget, CameraView cameraView, CharacterStencilRenderer characterStencilRenderer, Vector3 facadeTargetPos)
	{
		bool result = true;
		if (m_bInited && FloorManager.GetInstance() != null)
		{
			if (m_ForceHideAll[cameraID])
			{
				cameraViewSize.x = 0f;
				cameraViewSize.y = 0f;
			}
			if (m_FacadesManager != null)
			{
				m_FacadesManager.ShouldShowFacade(cameraID, facadeTargetPos, bAdvanceTimers: false);
			}
			int floorIndex = FloorManager.GetInstance().FindFloorAtZ(position.z).m_FloorIndex;
			result = CollectMeshRenders(position, cameraID, cameraViewSize, cameraView, floorIndex);
			m_MinPos.x = position.x - cameraViewSize.x;
			m_MinPos.y = position.y - cameraViewSize.y;
			m_MaxPos.x = position.x + cameraViewSize.x;
			m_MaxPos.y = position.y + cameraViewSize.y;
			UpdateCharacterWrappers(floorIndex, position, cameraID, cameraViewSize, characterStencilRenderer);
			UpdateLightWrappers(floorIndex, position, cameraID, cameraViewSize);
			UpdateDeskWrappers(floorIndex, position, cameraID, cameraViewSize);
			UpdateTagWrappers(floorIndex, position, cameraID, cameraViewSize);
			UpdateAnimatedWrappers(floorIndex, cameraID);
			UpdateParticleWrappers(floorIndex, cameraID);
			UpdateEffectManagerGameObjects(floorIndex, cameraID);
			UpdateLargeRendererWrappers(floorIndex, cameraID);
			m_bJustEnabled = false;
		}
		return result;
	}

	private void UpdateAnimatedWrappers(int floor, int cameraID)
	{
		int count = m_AnimatedWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			AnimatedWrapper animatedWrapper = m_AnimatedWrappers._items[i];
			if (!animatedWrapper.m_bNeedUpdate)
			{
				continue;
			}
			int num = 1 << cameraID;
			int visibilityMask = animatedWrapper.m_VisibilityMask;
			if ((visibilityMask & num) > 0)
			{
				if (!animatedWrapper.m_isEnabled)
				{
					int num2 = animatedWrapper.m_MeshRenderers.Length;
					for (int j = 0; j < num2; j++)
					{
						MeshRenderer meshRenderer = animatedWrapper.m_MeshRenderers[j];
						meshRenderer.enabled = true;
					}
					animatedWrapper.m_isEnabled = true;
				}
			}
			else if (animatedWrapper.m_isEnabled)
			{
				int num3 = animatedWrapper.m_MeshRenderers.Length;
				for (int j = 0; j < num3; j++)
				{
					MeshRenderer meshRenderer2 = animatedWrapper.m_MeshRenderers[j];
					meshRenderer2.enabled = false;
				}
				animatedWrapper.m_isEnabled = false;
			}
			animatedWrapper.UpdateVisibility();
			animatedWrapper.m_bNeedUpdate = false;
		}
	}

	private void UpdateLargeRendererWrappers(int floor, int cameraID)
	{
		int count = m_LargeRendererWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			LargeRendererWrapper largeRendererWrapper = m_LargeRendererWrappers._items[i];
			if (!largeRendererWrapper.m_bNeedUpdate)
			{
				continue;
			}
			int num = 1 << cameraID;
			bool flag = false;
			for (int j = 0; j < largeRendererWrapper.m_SpanningBuckets.Count; j++)
			{
				if ((largeRendererWrapper.m_SpanningBuckets[j].m_bViz & num) > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (!largeRendererWrapper.m_isEnabled)
				{
					int num2 = largeRendererWrapper.m_MeshRenderers.Length;
					for (int j = 0; j < num2; j++)
					{
						MeshRenderer meshRenderer = largeRendererWrapper.m_MeshRenderers[j];
						meshRenderer.enabled = true;
					}
					largeRendererWrapper.m_isEnabled = true;
				}
			}
			else if (largeRendererWrapper.m_isEnabled)
			{
				int num3 = largeRendererWrapper.m_MeshRenderers.Length;
				for (int j = 0; j < num3; j++)
				{
					MeshRenderer meshRenderer2 = largeRendererWrapper.m_MeshRenderers[j];
					meshRenderer2.enabled = false;
				}
				largeRendererWrapper.m_isEnabled = false;
			}
			largeRendererWrapper.m_bNeedUpdate = false;
		}
	}

	private bool MaterialReady(Material mat)
	{
		if (mat != null && mat.mainTexture != null)
		{
			return true;
		}
		return false;
	}

	private void UpdateCharacterWrappers(int floor, Vector3 position, int cameraID, Vector3 cameraViewSize, CharacterStencilRenderer characterStencilRenderer)
	{
		if (!m_bJustEnabled)
		{
			Vector2 stencilOrigin = Vector2.zero;
			RenderTexture renderTexture = ((!(characterStencilRenderer != null)) ? null : characterStencilRenderer.GetCharacterStencilTexture(ref stencilOrigin));
			if (renderTexture != null)
			{
				Shader.SetGlobalTexture(m_globalPropID_CharacterStencilTexture, renderTexture);
			}
			int count = m_CharacterWrappers.Count;
			for (int i = 0; i < count; i++)
			{
				CharacterWrapper characterWrapper = m_CharacterWrappers._items[i];
				Character character = characterWrapper.m_Character;
				if (character != null)
				{
					float num = character.CurrentFloor.m_FloorIndex;
					bool flag = num == (float)floor || (num < (float)floor && !character.CurrentFloor.IsUnderGround());
					Vector3 cachedCurrentPosition = character.m_CachedCurrentPosition;
					bool flag2 = !character.m_bIsHidden && flag && cachedCurrentPosition.x > m_MinPos.x && cachedCurrentPosition.x < m_MaxPos.x && cachedCurrentPosition.y > m_MinPos.y && cachedCurrentPosition.y < m_MaxPos.y;
					int visibilityMask = characterWrapper.m_VisibilityMask;
					visibilityMask = ((!flag2) ? (visibilityMask & ~(1 << cameraID)) : (visibilityMask | (1 << cameraID)));
					characterWrapper.UpdateLogic(visibilityMask, flag2, cameraID, characterStencilRenderer);
				}
			}
			count = m_CrowdCharacterWrappers.Count;
			for (int i = 0; i < count; i++)
			{
				CrowdCharacterWrapper crowdCharacterWrapper = m_CrowdCharacterWrappers._items[i];
				Character character2 = crowdCharacterWrapper.m_Character;
				if (character2 != null)
				{
					float num2 = character2.CurrentFloor.m_FloorIndex;
					bool flag3 = num2 == (float)floor || (num2 < (float)floor && !character2.CurrentFloor.IsUnderGround());
					Vector3 cachedCurrentPosition2 = character2.m_CachedCurrentPosition;
					bool flag4 = !character2.m_bIsHidden && flag3 && cachedCurrentPosition2.x > m_MinPos.x && cachedCurrentPosition2.x < m_MaxPos.x && cachedCurrentPosition2.y > m_MinPos.y && cachedCurrentPosition2.y < m_MaxPos.y;
					int visibilityMask2 = crowdCharacterWrapper.m_VisibilityMask;
					visibilityMask2 = ((!flag4) ? (visibilityMask2 & ~(1 << cameraID)) : (visibilityMask2 | (1 << cameraID)));
					crowdCharacterWrapper.UpdateLogic(visibilityMask2, flag4, cameraID, characterStencilRenderer);
				}
			}
			CS_HijackIngameCharacter.RunStencilForHiJackedCharacters(renderTexture);
		}
		else
		{
			InitialCharacterWrappersUpdate(floor, position, cameraID, cameraViewSize, characterStencilRenderer);
		}
	}

	private void InitialCharacterWrappersUpdate(int floor, Vector3 position, int cameraID, Vector3 cameraViewSize, CharacterStencilRenderer characterStencilRenderer)
	{
		Vector2 stencilOrigin = Vector2.zero;
		if (characterStencilRenderer != null)
		{
			characterStencilRenderer.GetCharacterStencilTexture(ref stencilOrigin);
		}
		int count = m_CharacterWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			CharacterWrapper characterWrapper = m_CharacterWrappers._items[i];
			Character character = characterWrapper.m_Character;
			if (!(character != null))
			{
				continue;
			}
			character.m_CharacterID = 1f + (float)i;
			if (characterWrapper.m_SkinnedMeshRenderer != null)
			{
				if (character.m_CharacterRole != CharacterRole.Ghost)
				{
					CharacterStencilRenderer.SetCharacterSkinnedMeshRenderer(characterWrapper.m_Character, characterWrapper.m_SkinnedMeshRenderer);
				}
				continue;
			}
			FastList<MeshRenderer> coreMeshes = null;
			FastList<MeshRenderer> accMeshes = null;
			if (characterStencilRenderer != null)
			{
				characterStencilRenderer.GetCharacterMeshLists(character, out coreMeshes, out accMeshes);
			}
			MeshRenderer meshRenderer = null;
			RoomBlob currentLocation = character.m_CurrentLocation;
			int num = 0;
			characterWrapper.m_VisibilityMask = 15;
			characterWrapper.UpdateLogic(0, bVis: false, 0, null);
			characterWrapper.GetMaterialPropertyBlocks(out var mainBlock, out var accBlock);
			CharacterWrapper.MeshRendererTransform[] array = characterWrapper.m_CoreMeshRenderers[num];
			bool[] visEnabled = characterWrapper.m_VisEnabled;
			int num2 = array.Length;
			for (int j = 0; j < num2; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform = array[j];
				meshRenderer = meshRendererTransform.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform.m_bCanRender && meshRendererTransform.m_Textures.Length > 0)
				{
					Texture texture = meshRendererTransform.m_Textures[0];
					if (texture != null)
					{
						mainBlock.SetTexture(m_propID_CharacterTexture, texture);
						meshRenderer.SetPropertyBlock(mainBlock);
						flag = true;
					}
				}
				if (visEnabled[j] != flag)
				{
					visEnabled[j] = flag;
					meshRenderer.enabled = flag;
				}
				if (flag)
				{
					coreMeshes?.Add(meshRenderer);
				}
			}
			bool bHasAccessoryTextures = characterWrapper.m_bHasAccessoryTextures;
			array = characterWrapper.m_AccMeshRenderers[num];
			visEnabled = characterWrapper.m_AccVisEnabled;
			int num3 = array.Length;
			for (int j = 0; j < num3; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform2 = array[j];
				meshRenderer = meshRendererTransform2.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform2.m_bCanRender && bHasAccessoryTextures)
				{
					meshRenderer.SetPropertyBlock(accBlock);
					accMeshes?.Add(meshRenderer);
					flag = true;
				}
				if (visEnabled[j] != flag)
				{
					visEnabled[j] = flag;
					meshRenderer.enabled = flag;
				}
			}
			for (int j = 0; j < characterWrapper.m_ActionMeshRenderers.Length; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform3 = characterWrapper.m_ActionMeshRenderers[j];
				meshRenderer = meshRendererTransform3.m_MeshRenderer;
				bool flag = false;
				if (meshRendererTransform3.m_bCanRender && meshRendererTransform3.m_Textures.Length > 0)
				{
					Texture texture2 = meshRendererTransform3.m_Textures[0];
					if (texture2 != null)
					{
						mainBlock.SetTexture(m_propID_CharacterTexture, texture2);
						meshRenderer.SetPropertyBlock(mainBlock);
						coreMeshes?.Add(meshRenderer);
						flag = true;
					}
				}
				meshRenderer.enabled = flag;
			}
		}
		count = m_CrowdCharacterWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			CharacterWrapper characterWrapper2 = m_CrowdCharacterWrappers._items[i];
			Character character2 = characterWrapper2.m_Character;
			if (!(character2 != null))
			{
				continue;
			}
			character2.m_CharacterID = 1f + (float)i;
			if (characterWrapper2.m_SkinnedMeshRenderer != null)
			{
				if (character2.m_CharacterRole != CharacterRole.Ghost)
				{
					CharacterStencilRenderer.SetCharacterSkinnedMeshRenderer(characterWrapper2.m_Character, characterWrapper2.m_SkinnedMeshRenderer);
				}
				continue;
			}
			FastList<MeshRenderer> coreMeshes2 = null;
			FastList<MeshRenderer> accMeshes2 = null;
			if (characterStencilRenderer != null)
			{
				characterStencilRenderer.GetCharacterMeshLists(character2, out coreMeshes2, out accMeshes2);
			}
			MeshRenderer meshRenderer2 = null;
			RoomBlob currentLocation2 = character2.m_CurrentLocation;
			int num4 = 0;
			characterWrapper2.m_VisibilityMask = 15;
			characterWrapper2.UpdateLogic(0, bVis: false, 0, null);
			characterWrapper2.GetMaterialPropertyBlocks(out var mainBlock2, out var accBlock2);
			CharacterWrapper.MeshRendererTransform[] array2 = characterWrapper2.m_CoreMeshRenderers[num4];
			bool[] visEnabled2 = characterWrapper2.m_VisEnabled;
			int num5 = array2.Length;
			for (int j = 0; j < num5; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform4 = array2[j];
				meshRenderer2 = meshRendererTransform4.m_MeshRenderer;
				bool flag2 = false;
				if (meshRendererTransform4.m_bCanRender && meshRendererTransform4.m_Textures.Length > 0)
				{
					Texture texture3 = meshRendererTransform4.m_Textures[0];
					if (texture3 != null)
					{
						mainBlock2.SetTexture(m_propID_CharacterTexture, texture3);
						meshRenderer2.SetPropertyBlock(mainBlock2);
						flag2 = true;
					}
				}
				if (visEnabled2[j] != flag2)
				{
					visEnabled2[j] = flag2;
					meshRenderer2.enabled = flag2;
				}
				if (flag2)
				{
					coreMeshes2?.Add(meshRenderer2);
				}
			}
			bool bHasAccessoryTextures2 = characterWrapper2.m_bHasAccessoryTextures;
			array2 = characterWrapper2.m_AccMeshRenderers[num4];
			visEnabled2 = characterWrapper2.m_AccVisEnabled;
			int num6 = array2.Length;
			for (int j = 0; j < num6; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform5 = array2[j];
				meshRenderer2 = meshRendererTransform5.m_MeshRenderer;
				bool flag2 = false;
				if (meshRendererTransform5.m_bCanRender && bHasAccessoryTextures2)
				{
					meshRenderer2.SetPropertyBlock(accBlock2);
					accMeshes2?.Add(meshRenderer2);
					flag2 = true;
				}
				if (visEnabled2[j] != flag2)
				{
					visEnabled2[j] = flag2;
					meshRenderer2.enabled = flag2;
				}
			}
			for (int j = 0; j < characterWrapper2.m_ActionMeshRenderers.Length; j++)
			{
				CharacterWrapper.MeshRendererTransform meshRendererTransform6 = characterWrapper2.m_ActionMeshRenderers[j];
				meshRenderer2 = meshRendererTransform6.m_MeshRenderer;
				bool flag2 = false;
				if (meshRendererTransform6.m_bCanRender && meshRendererTransform6.m_Textures.Length > 0)
				{
					Texture texture4 = meshRendererTransform6.m_Textures[0];
					if (texture4 != null)
					{
						mainBlock2.SetTexture(m_propID_CharacterTexture, texture4);
						meshRenderer2.SetPropertyBlock(mainBlock2);
						coreMeshes2?.Add(meshRenderer2);
						flag2 = true;
					}
				}
				meshRenderer2.enabled = flag2;
			}
		}
	}

	private void UpdateParticleWrappers(int floor, int cameraID)
	{
		for (int i = 0; i < m_ParticleWrappers.Count; i++)
		{
			ParticleWrapper particleWrapper = m_ParticleWrappers._items[i];
			particleWrapper.UpdateVisibility(cameraID);
		}
	}

	private void UpdateEffectManagerGameObjects(int floorIndex, int cameraID)
	{
		for (int i = 0; i < m_EffectManagerGameObjects.Count; i++)
		{
			EffectManagerWrapper effectManagerWrapper = m_EffectManagerGameObjects._items[i];
			effectManagerWrapper.UpdateEffectVisibility(m_FloorManager, floorIndex);
		}
	}

	private void UpdateLightWrappers(int floor, Vector3 position, int cameraID, Vector3 cameraViewSize)
	{
		if (m_bEnabled && !m_bJustEnabled)
		{
			int currentMaxFloor = m_FloorManager.currentMaxFloor;
			int currentFloor = CustomLightRenderer.customLightManager.currentFloor;
			int num = ((floor >= currentMaxFloor) ? currentMaxFloor : floor);
			CustomLightManager customLightManager = CustomLightRenderer.customLightManager;
			bool flag = customLightManager.ShouldUpdateLights(num, position);
			if (flag)
			{
				customLightManager.UpdateCameraPos(num, position);
				customLightManager.RemoveAllLights();
			}
			else
			{
				customLightManager.RemoveAllDynamicLights();
			}
			float num2 = 3f;
			bool flag2 = false;
			FastList<LightWrapper> fastList = null;
			int count;
			if (flag)
			{
				fastList = m_LightWrapperContainers[num].m_NormalLightWrappers;
				count = fastList.Count;
				for (int i = 0; i < count; i++)
				{
					LightWrapper lightWrapper = fastList._items[i];
					Vector3 position2 = lightWrapper.m_position;
					float num3 = lightWrapper.m_range + num2;
					bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
					lightWrapper.UpdateLogicNormal(inView);
				}
				if (currentFloor > num)
				{
					fastList = m_LightWrapperContainers[currentFloor].m_NormalLightWrappers;
					count = fastList.Count;
					for (int i = 0; i < count; i++)
					{
						LightWrapper lightWrapper = fastList._items[i];
						lightWrapper.UpdateLogicNormal(inView: false);
					}
				}
				fastList = m_LightWrapperContainers[num].m_CustomLightWrappers;
				count = fastList.Count;
				for (int i = 0; i < count; i++)
				{
					LightWrapper lightWrapper = fastList._items[i];
					Vector3 position2 = lightWrapper.m_position;
					float num3 = lightWrapper.m_range + num2;
					bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
					flag2 |= lightWrapper.UpdateLogicCustom(inView);
				}
				fastList = m_LightWrapperContainers[20].m_NormalLightWrappers;
				count = fastList.Count;
				for (int i = 0; i < count; i++)
				{
					LightWrapper lightWrapper = fastList._items[i];
					Vector3 position2 = lightWrapper.m_position;
					float num3 = lightWrapper.m_range + num2;
					bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
					lightWrapper.UpdateLogicNormal(inView);
				}
				fastList = m_LightWrapperContainers[20].m_CustomLightWrappers;
				count = fastList.Count;
				for (int i = 0; i < count; i++)
				{
					LightWrapper lightWrapper = fastList._items[i];
					Vector3 position2 = lightWrapper.m_position;
					float num3 = lightWrapper.m_range + num2;
					bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
					flag2 |= lightWrapper.UpdateLogicCustom(inView);
				}
			}
			fastList = m_LightWrapperContainers[num].m_CustomLightDynamicWrappers;
			count = fastList.Count;
			for (int i = 0; i < count; i++)
			{
				LightWrapper lightWrapper = fastList._items[i];
				Vector3 position2 = lightWrapper.m_position;
				float num3 = lightWrapper.m_range + num2;
				bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
				flag2 |= lightWrapper.UpdateLogicCustom(inView);
			}
			fastList = m_LightWrapperContainers[20].m_CustomLightDynamicWrappers;
			count = fastList.Count;
			for (int i = 0; i < count; i++)
			{
				LightWrapper lightWrapper = fastList._items[i];
				Vector3 position2 = lightWrapper.m_position;
				float num3 = lightWrapper.m_range + num2;
				bool inView = position2.x + num3 > m_MinPos.x && position2.x - num3 < m_MaxPos.x && position2.y + num3 > m_MinPos.y && position2.y - num3 < m_MaxPos.y;
				flag2 |= lightWrapper.UpdateLogicCustom(inView);
			}
			if (flag2)
			{
				customLightManager.ForceUpdate();
			}
			return;
		}
		for (int j = 0; j < m_LightWrapperContainers.Length; j++)
		{
			FastList<LightWrapper> normalLightWrappers = m_LightWrapperContainers[j].m_NormalLightWrappers;
			FastList<LightWrapper> customLightWrappers = m_LightWrapperContainers[j].m_CustomLightWrappers;
			FastList<LightWrapper> customLightDynamicWrappers = m_LightWrapperContainers[j].m_CustomLightDynamicWrappers;
			for (int i = 0; i < normalLightWrappers.Count; i++)
			{
				normalLightWrappers._items[i].UpdateLogicNormal(inView: true);
			}
			for (int i = 0; i < customLightWrappers.Count; i++)
			{
				customLightWrappers._items[i].UpdateLogicCustom(inView: true);
			}
			for (int i = 0; i < customLightDynamicWrappers.Count; i++)
			{
				customLightDynamicWrappers._items[i].UpdateLogicCustom(inView: true);
			}
		}
	}

	private void UpdateDeskWrappers(int floor, Vector3 position, int cameraID, Vector3 cameraViewSize)
	{
		if (m_bEnabled && !m_bJustEnabled)
		{
			int num = 1 << cameraID;
			DeskWrapper[] items = m_DeskWrappers._items;
			int count = m_DeskWrappers.Count;
			for (int i = 0; i < count; i++)
			{
				DeskWrapper deskWrapper = items[i];
				if (deskWrapper.m_Transform == null)
				{
					if (deskWrapper.m_GameObject == null)
					{
						continue;
					}
					deskWrapper.m_Transform = deskWrapper.m_GameObject.transform;
				}
				int visibilityMask = deskWrapper.m_VisibilityMask;
				Vector3 position2 = deskWrapper.m_Transform.position;
				int floor2 = deskWrapper.m_Floor;
				if (false || (floor2 <= floor && floor2 >= floor - 1 && position2.x > m_MinPos.x && position2.x < m_MaxPos.x && position2.y > m_MinPos.y && position2.y < m_MaxPos.y))
				{
					deskWrapper.m_VisibilityMask |= num;
					if (!deskWrapper.m_isEnabled)
					{
						int num2 = deskWrapper.m_MeshRenderers.Length;
						for (int j = 0; j < num2; j++)
						{
							MeshRenderer meshRenderer = deskWrapper.m_MeshRenderers[j];
							meshRenderer.enabled = true;
						}
					}
					deskWrapper.m_isEnabled = true;
				}
				else
				{
					deskWrapper.m_VisibilityMask &= ~num;
					if (deskWrapper.m_isEnabled)
					{
						int num3 = deskWrapper.m_MeshRenderers.Length;
						for (int j = 0; j < num3; j++)
						{
							MeshRenderer meshRenderer2 = deskWrapper.m_MeshRenderers[j];
							meshRenderer2.enabled = false;
						}
						deskWrapper.m_isEnabled = false;
					}
				}
				deskWrapper.UpdateLogic(visibilityMask, deskWrapper.m_VisibilityMask);
			}
			return;
		}
		DeskWrapper[] items2 = m_DeskWrappers._items;
		int count2 = m_DeskWrappers.Count;
		for (int i = 0; i < count2; i++)
		{
			DeskWrapper deskWrapper2 = items2[i];
			int num4 = deskWrapper2.m_MeshRenderers.Length;
			for (int j = 0; j < num4; j++)
			{
				MeshRenderer meshRenderer3 = deskWrapper2.m_MeshRenderers[j];
				meshRenderer3.enabled = true;
			}
			deskWrapper2.m_isEnabled = true;
			deskWrapper2.UpdateLogic(0, 1);
			deskWrapper2.m_VisibilityMask = 1;
		}
	}

	private void UpdateTagWrappers(int floor, Vector3 position, int cameraID, Vector3 cameraViewSize)
	{
		if (m_bEnabled && !m_bJustEnabled)
		{
			for (int i = 0; i < m_TagWrappers.Count; i++)
			{
				if (m_TagWrappers._items[i] == null || !(m_TagWrappers._items[i].m_GameObject != null))
				{
					continue;
				}
				int visibilityMask = m_TagWrappers._items[i].m_VisibilityMask;
				int num = visibilityMask & ~(1 << cameraID);
				int num2 = m_TagWrappers._items[i].m_CullingMask & (1 << cameraID);
				Vector3 position2 = m_TagWrappers._items[i].m_GameObject.transform.position;
				int floor2 = m_TagWrappers._items[i].m_Floor;
				if (false || (num2 != 0 && floor2 == floor && position2.x > m_MinPos.x && position2.x < m_MaxPos.x && position2.y > m_MinPos.y && position2.y < m_MaxPos.y))
				{
					num |= 1 << cameraID;
					int num3 = m_TagWrappers._items[i].m_MeshRenderers.Length;
					for (int j = 0; j < num3; j++)
					{
						MeshRenderer meshRenderer = m_TagWrappers._items[i].m_MeshRenderers[j];
						if (meshRenderer != null)
						{
							meshRenderer.enabled = true;
						}
					}
				}
				else
				{
					int num4 = m_TagWrappers._items[i].m_MeshRenderers.Length;
					for (int j = 0; j < num4; j++)
					{
						MeshRenderer meshRenderer2 = m_TagWrappers._items[i].m_MeshRenderers[j];
						if (meshRenderer2 != null)
						{
							meshRenderer2.enabled = false;
						}
					}
				}
				m_TagWrappers._items[i].UpdateLogic(visibilityMask, num);
			}
			return;
		}
		for (int i = 0; i < m_TagWrappers.Count; i++)
		{
			if (m_TagWrappers._items[i] == null)
			{
				continue;
			}
			int num5 = m_TagWrappers._items[i].m_MeshRenderers.Length;
			for (int j = 0; j < num5; j++)
			{
				MeshRenderer meshRenderer3 = m_TagWrappers._items[i].m_MeshRenderers[j];
				if (meshRenderer3 != null)
				{
					meshRenderer3.enabled = true;
				}
			}
			m_TagWrappers._items[i].UpdateLogic(0, 1);
			m_TagWrappers._items[i].m_VisibilityMask = 1;
		}
	}

	private void PreCalcAnimatedLighting(ref Color[] ambientColours, ref float[] ambientIntensitie)
	{
		int count = m_AnimatedWrappers.Count;
		if (count == 0)
		{
			return;
		}
		ref Color reference = ref s_AmbientColours[0];
		reference = ambientColours[0];
		ref Color reference2 = ref s_AmbientColours[1];
		reference2 = ambientColours[1];
		s_AmbientIntensities[0] = ambientIntensitie[0];
		s_AmbientIntensities[1] = ambientIntensitie[1];
		int num = 0;
		int animWrapperPreCalcIndex = m_animWrapperPreCalcIndex;
		do
		{
			if (m_animWrapperPreCalcIndex >= count)
			{
				m_animWrapperPreCalcIndex = 0;
			}
			m_AnimatedWrappers[m_animWrapperPreCalcIndex].UpdateLighting(applyProperty: false);
			m_animWrapperPreCalcIndex++;
			num++;
		}
		while (num < m_animWrapperPreCalcBudget && m_animWrapperPreCalcIndex != animWrapperPreCalcIndex);
		if (UpdateManager.deltaTime == 0f)
		{
			return;
		}
		float num2 = 1f / UpdateManager.deltaTime;
		int num3 = 1 + (int)((float)count / num2);
		if (num3 > m_animWrapperPreCalcBudget)
		{
			if (m_animWrapperPreCalcBudget < count)
			{
				m_animWrapperPreCalcBudget++;
			}
		}
		else if (num3 < m_animWrapperPreCalcBudget && m_animWrapperPreCalcBudget > 1)
		{
			m_animWrapperPreCalcBudget--;
		}
	}

	private bool UpdateAnimatedLighting()
	{
		int count = m_AnimatedWrappers.Count;
		int num = 1 + count / 6;
		int num2 = 0;
		RoutineManager instance = RoutineManager.GetInstance();
		bool forceLight = instance.IsTimeFrozen();
		if (instance.IsTimeSpedUp())
		{
			num = count;
			forceLight = true;
		}
		do
		{
			if (m_animWrapperUpdateIndex >= count)
			{
				m_animWrapperUpdateIndex = 0;
				return true;
			}
			m_AnimatedWrappers[m_animWrapperUpdateIndex].UpdateLighting(applyProperty: true, forceLight);
			m_animWrapperUpdateIndex++;
			num2++;
		}
		while (num2 < num);
		return false;
	}

	private void UpdateDeskLighting()
	{
		int count = m_DeskWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			DeskWrapper deskWrapper = m_DeskWrappers[i];
			RoomBlob gameObjectRoom = GetGameObjectRoom(deskWrapper.m_Transform.transform, deskWrapper.m_Floor);
			int num = ((gameObjectRoom != null && gameObjectRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors) ? 1 : 0);
			Color value = s_AmbientColours[num];
			float value2 = s_AmbientIntensities[num];
			MaterialPropertyBlock materialPropertyBlock = deskWrapper.m_propertyBlock;
			if (materialPropertyBlock == null)
			{
				materialPropertyBlock = new MaterialPropertyBlock();
				materialPropertyBlock.SetColor(m_propId_Color, Color.white);
				materialPropertyBlock.SetColor(m_propId_TileHeightTint, Color.white);
				deskWrapper.m_propertyBlock = materialPropertyBlock;
			}
			materialPropertyBlock.SetColor(m_propId_AmbientLight, value);
			materialPropertyBlock.SetFloat(m_propId_AmbientIntensity, value2);
			int num2 = deskWrapper.m_MeshRenderers.Length;
			for (int j = 0; j < num2; j++)
			{
				deskWrapper.m_MeshRenderers[j].SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	private void UpdateBackgroundLighting()
	{
		if (CullingBuckets.s_backGround == null)
		{
			return;
		}
		int count = CullingBuckets.s_backGround.Count;
		if (count <= 0)
		{
			return;
		}
		Color value = s_AmbientColours[0];
		float value2 = s_AmbientIntensities[0];
		for (int i = 0; i < count; i++)
		{
			if (CullingBuckets.s_backGround[i] != null)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				CullingBuckets.s_backGround[i].GetPropertyBlock(materialPropertyBlock);
				if (materialPropertyBlock != null)
				{
					materialPropertyBlock.SetColor(m_propId_AmbientLight, value);
					materialPropertyBlock.SetFloat(m_propId_AmbientIntensity, value2);
					CullingBuckets.s_backGround[i].SetPropertyBlock(materialPropertyBlock);
				}
			}
		}
	}

	private void UpdateCharacterLighting()
	{
		int count = m_CharacterWrappers.Count;
		for (int i = 0; i < count; i++)
		{
			CharacterWrapper characterWrapper = m_CharacterWrappers[i];
			int isInside = characterWrapper.m_Character.m_isInside;
			Color value = s_AmbientColours[isInside];
			float value2 = s_AmbientIntensities[isInside];
			MaterialPropertyBlock propertyBlock = characterWrapper.m_propertyBlock;
			if (propertyBlock != null)
			{
				propertyBlock.SetColor(m_propId_AmbientLight, value);
				propertyBlock.SetFloat(m_propId_AmbientIntensity, value2);
				MaterialPropertyBlock accessoryPropertyBlock = characterWrapper.m_accessoryPropertyBlock;
				if (accessoryPropertyBlock != null)
				{
					accessoryPropertyBlock.SetColor(m_propId_AmbientLight, value);
					accessoryPropertyBlock.SetFloat(m_propId_AmbientIntensity, value2);
				}
				if (characterWrapper.m_SkinnedMeshRenderer != null)
				{
					characterWrapper.m_SkinnedMeshRenderer.SetPropertyBlock(propertyBlock);
				}
			}
		}
		count = m_CrowdCharacterWrappers.Count;
		for (int j = 0; j < count; j++)
		{
			CharacterWrapper characterWrapper2 = m_CrowdCharacterWrappers[j];
			int isInside2 = characterWrapper2.m_Character.m_isInside;
			Color value3 = s_AmbientColours[isInside2];
			float value4 = s_AmbientIntensities[isInside2];
			MaterialPropertyBlock propertyBlock2 = characterWrapper2.m_propertyBlock;
			if (propertyBlock2 != null)
			{
				propertyBlock2.SetColor(m_propId_AmbientLight, value3);
				propertyBlock2.SetFloat(m_propId_AmbientIntensity, value4);
				MaterialPropertyBlock accessoryPropertyBlock2 = characterWrapper2.m_accessoryPropertyBlock;
				if (accessoryPropertyBlock2 != null)
				{
					accessoryPropertyBlock2.SetColor(m_propId_AmbientLight, value3);
					accessoryPropertyBlock2.SetFloat(m_propId_AmbientIntensity, value4);
				}
				if (characterWrapper2.m_SkinnedMeshRenderer != null)
				{
					characterWrapper2.m_SkinnedMeshRenderer.SetPropertyBlock(propertyBlock2);
				}
			}
		}
	}

	private void OnLightingUpdated()
	{
		if (m_LightingDirty != eDirtyLighting.None)
		{
			m_LightingPendingDirty = true;
		}
		else
		{
			m_LightingDirty = eDirtyLighting.All;
		}
	}

	private void UpdateAllLighting()
	{
		if (m_LightingDirty == eDirtyLighting.None || !(m_LightingManager != null) || !UpdateManager.AquireHeavyCpuLock())
		{
			return;
		}
		ref Color reference = ref s_AmbientColours[0];
		reference = m_LightingManager.GetCurrentAmbientColor(inside: false);
		ref Color reference2 = ref s_AmbientColours[1];
		reference2 = m_LightingManager.GetCurrentAmbientColor(inside: true);
		s_AmbientIntensities[0] = m_LightingManager.GetCurrentAmbientIntensity(inside: false);
		s_AmbientIntensities[1] = m_LightingManager.GetCurrentAmbientIntensity(inside: true);
		bool flag = true;
		switch (m_LightingDirty)
		{
		case eDirtyLighting.All:
			UpdateTileLighting();
			break;
		case eDirtyLighting.Animated:
			flag = UpdateAnimatedLighting();
			break;
		case eDirtyLighting.Character:
			UpdateCharacterLighting();
			break;
		case eDirtyLighting.Desk:
			UpdateDeskLighting();
			break;
		case eDirtyLighting.Background:
			UpdateBackgroundLighting();
			break;
		case eDirtyLighting.Wrappers:
			CustomLightRenderer.customLightManager.ForceUpdate();
			break;
		}
		if (flag)
		{
			m_LightingDirty++;
			if (m_LightingDirty == eDirtyLighting.None && m_LightingPendingDirty)
			{
				m_LightingDirty = eDirtyLighting.All;
				m_LightingPendingDirty = false;
			}
		}
	}

	public int GetMeshCount()
	{
		return 0;
	}

	public int GetCharactersCount()
	{
		return m_CharacterWrappers.Count + m_CrowdCharacterWrappers.Count;
	}

	public int GetVisibleCharacterCount()
	{
		int num = 0;
		for (int i = 0; i < m_CharacterWrappers.Count; i++)
		{
			if (m_CharacterWrappers._items[i].m_VisibilityMask != 0)
			{
				num++;
			}
		}
		for (int j = 0; j < m_CrowdCharacterWrappers.Count; j++)
		{
			if (m_CrowdCharacterWrappers._items[j].m_VisibilityMask != 0)
			{
				num++;
			}
		}
		return num;
	}

	public string GetVisibleCharacterNames()
	{
		string text = string.Empty;
		for (int i = 0; i < m_CharacterWrappers.Count; i++)
		{
			if (m_CharacterWrappers._items[i].m_VisibilityMask != 0)
			{
				if (text != string.Empty)
				{
					text += ",";
				}
				text += m_CharacterWrappers._items[i].m_Character.name;
			}
		}
		return text;
	}

	public ViewReturnedData GetViewData(int ID)
	{
		return m_ViewDatas[ID];
	}

	public bool IsCharacterVisibleToCamera(int cameraID, Character targetCharacter)
	{
		for (int i = 0; i < m_CharacterWrappers.Count; i++)
		{
			if (m_CharacterWrappers._items[i].m_Character.gameObject == targetCharacter.gameObject)
			{
				if ((m_CharacterWrappers._items[i].m_VisibilityMask & (1 << cameraID)) > 0)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private void Update()
	{
		UpdateAllLighting();
	}

	public static bool DebugToggleFacade(bool bEnable, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bForcedFacadesOff = !m_bForcedFacadesOff;
		}
		return !m_bForcedFacadesOff;
	}
}
