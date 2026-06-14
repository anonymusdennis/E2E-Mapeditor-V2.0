using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterAnimator))]
[RequireComponent(typeof(CutsceneFlooredMonobehaviour))]
public abstract class CS_HijackIngameCharacter : T17MonoBehaviour, ICutsceneStartEndResponder, IControlledUpdate, StencilInterface
{
	private Character m_HijackedObject;

	private CharacterAnimator m_ActorAnimator;

	private CharacterCustomisation m_ActorCustomisation;

	private CutsceneFlooredMonobehaviour m_ActorFloor;

	private bool m_LayerAnimator = true;

	private bool m_bHijacked_IsDisabled;

	private bool m_bHijacked_IsHidden;

	private Directionx4 m_HijackedCharDirection;

	private float m_HiJackedCharacterID;

	private bool m_bIntoStencilRenderer;

	private MaterialPropertyBlock m_PropertyBlock;

	private int m_HijackedCharacterIndex;

	private bool m_bAwakeDone;

	private static List<CS_HijackIngameCharacter> m_HiJackedCharacters = new List<CS_HijackIngameCharacter>();

	private int m_LastCloseFrame;

	public abstract Character GetCharacterToHijack();

	protected override void Awake()
	{
		base.Awake();
		if (!m_bAwakeDone)
		{
			m_ActorAnimator = GetComponent<CharacterAnimator>();
			m_ActorCustomisation = GetComponent<CharacterCustomisation>();
			m_ActorFloor = GetComponent<CutsceneFlooredMonobehaviour>();
			m_ActorAnimator.SetCharacterAnimatorType(CharacterAnimator.ANIMATOR_TYPE.AT_CLIVE);
			m_bAwakeDone = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_HijackedObject != null)
		{
			ReleaseHijackedPlayer();
		}
	}

	public void CutsceneStarted()
	{
		Awake();
		HijackCharacter();
	}

	public void CutsceneEnded()
	{
		ReleaseHijackedPlayer();
	}

	public void HijackCharacter()
	{
		Character characterToHijack = GetCharacterToHijack();
		if (characterToHijack != null)
		{
			if (m_ActorAnimator != null)
			{
				SetActorEnabled(state: true);
			}
			CopyAndDisableCharacter(characterToHijack);
		}
		else
		{
			SetActorEnabled(state: false);
		}
	}

	private void SetActorEnabled(bool state)
	{
		if (m_ActorAnimator != null)
		{
			m_ActorAnimator.gameObject.SetActive(state);
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(state);
		}
	}

	protected void CopyAndDisableCharacter(Character character)
	{
		if (m_HijackedObject != null)
		{
			ReleaseHijackedPlayer();
		}
		if (m_ActorAnimator != null)
		{
			m_ActorAnimator.SetCharacterAnimatorType(character.m_CharacterAnimator.GetCharacterAnimatorType());
			if (m_ActorCustomisation != null)
			{
				m_ActorCustomisation.SetCustomisation(character.m_CharacterCustomisation);
				m_ActorCustomisation.SetForceNaked(naked: false);
			}
		}
		if (m_PropertyBlock == null)
		{
			m_PropertyBlock = new MaterialPropertyBlock();
		}
		m_bIntoStencilRenderer = false;
		m_HijackedObject = character;
		m_HiJackedCharacterID = character.m_CharacterID;
		m_bHijacked_IsDisabled = m_HijackedObject.GetIsDisabled();
		m_HijackedObject.SetIsDisabled(disabled: true);
		m_HiJackedCharacters.Add(this);
		m_HijackedCharacterIndex = m_HiJackedCharacters.Count;
		m_bHijacked_IsHidden = m_HijackedObject.m_bIsHidden;
		m_HijackedObject.m_bIsHidden = true;
		CharacterStencilRenderer.RemoveCharacterMeshRenderers(m_HijackedObject);
		m_HijackedCharDirection = m_HijackedObject.m_CharacterAnimator.GetDirectionx4();
		m_HijackedObject.Cutscenes_SetHijacked(isHijacked: true);
	}

	public void SetCharacterAppearance(Customisation appearance, Customisation overrides = null)
	{
		if (!(m_ActorAnimator != null))
		{
			return;
		}
		m_ActorAnimator.SetSkipStencilCheck(skip: false);
		if (m_ActorCustomisation != null)
		{
			m_ActorCustomisation.SetCustomisation(appearance);
			m_ActorCustomisation.SetOutfit(appearance.defaultOutfit);
			if (overrides != null)
			{
				m_ActorCustomisation.SetCustomisationOverrides(overrides);
				m_ActorCustomisation.SetOutfit(overrides.defaultOutfit);
			}
			m_ActorCustomisation.SetForceNaked(naked: false);
		}
	}

	public void SetCharacterNaked(bool isNaked)
	{
		if (m_ActorAnimator != null)
		{
			m_ActorAnimator.SetSkipStencilCheck(skip: false);
			m_ActorCustomisation.SetForceNaked(isNaked);
		}
	}

	public void ReleaseHijackedPlayer()
	{
		if (!(m_HijackedObject == null))
		{
			CharacterStencilRenderer.RemoveCharacterMeshRenderers(this);
			m_HiJackedCharacters.Remove(this);
			m_HijackedObject.SetIsDisabled(m_bHijacked_IsDisabled);
			m_HijackedObject.m_bIsHidden = m_bHijacked_IsHidden;
			SkinnedMeshRenderer componentInChildren = m_HijackedObject.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				CharacterStencilRenderer.SetCharacterSkinnedMeshRenderer(m_HijackedObject, componentInChildren);
			}
			m_HijackedObject.Cutscenes_SetHijacked(isHijacked: false);
			m_HijackedObject.m_CharacterAnimator.ForceStateUpdate();
			m_HijackedObject.m_CharacterAnimator.HeadAndBodyFaceDirection(m_HijackedCharDirection, force: true);
			m_HijackedObject = null;
		}
	}

	public void EnableLayerAnimator(bool bEnable)
	{
		m_LayerAnimator = bEnable;
	}

	public void ControlledUpdate()
	{
		LayerAnimator();
	}

	public void ControlledFixedUpdate()
	{
	}

	private void LayerAnimator()
	{
		Vector3 defaultAnimatorPosition = Character.m_DefaultAnimatorPosition;
		if (LevelScript.GetInstance().m_Processed && m_LayerAnimator && m_ActorFloor != null)
		{
			FloorManager.GetInstance().GetTileCentrePosition(m_ActorFloor.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, base.transform.position, out var centredPosition);
			float zOffset = LayerHelper.GetZOffset(centredPosition.y);
			defaultAnimatorPosition.z = zOffset;
			defaultAnimatorPosition.z += -0.0035f;
		}
		if (m_ActorAnimator != null && m_ActorAnimator.m_CharacterAnimator != null)
		{
			m_ActorAnimator.m_CharacterAnimator.gameObject.transform.localPosition = defaultAnimatorPosition;
			Vector3 localScale = m_ActorAnimator.m_CharacterAnimator.gameObject.transform.localScale;
			localScale.z = 20f;
			m_ActorAnimator.m_CharacterAnimator.gameObject.transform.localScale = localScale;
		}
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public StencilInterface GetPickedUpBy()
	{
		return null;
	}

	public StencilInterface GetCarrying()
	{
		return null;
	}

	public Directionx4 GetFacingDirectionEnum()
	{
		return Directionx4.Down;
	}

	public Vector3 GetCachedCurrentPosition()
	{
		if (base.gameObject != null)
		{
			Transform transform = base.gameObject.transform;
			if (transform != null)
			{
				return transform.position;
			}
		}
		return new Vector3(0f, -10000f, 0f);
	}

	public float GetCharacterID()
	{
		return m_HiJackedCharacterID;
	}

	public int GetFloorIndex()
	{
		return FloorManager.GetInstance().FindFloorAtZ(GetCachedCurrentPosition().z).m_FloorIndex;
	}

	public int GetCharacterListIndex()
	{
		return CullingObjectCollector.GetInstance().GetCharactersCount() + m_HijackedCharacterIndex;
	}

	public bool GetIsHiddenOrDisabled()
	{
		return false;
	}

	public bool ConsiderForCloseCheck()
	{
		return true;
	}

	public void SetLastCloseFrame(int framenum)
	{
		m_LastCloseFrame = 0;
	}

	public int GetLastCloseFrame()
	{
		return m_LastCloseFrame;
	}

	public static void RunStencilForHiJackedCharacters(RenderTexture characterStencilTexture)
	{
		if (characterStencilTexture == null)
		{
			return;
		}
		int count = m_HiJackedCharacters.Count;
		for (int i = 0; i < count; i++)
		{
			SkinnedMeshRenderer componentInChildren = m_HiJackedCharacters[i].m_ActorAnimator.GetComponentInChildren<SkinnedMeshRenderer>();
			if (!(componentInChildren != null))
			{
				continue;
			}
			m_HiJackedCharacters[i].m_HijackedCharacterIndex = 1 + i;
			m_HiJackedCharacters[i].m_HiJackedCharacterID = 1f + (float)CullingObjectCollector.GetInstance().GetCharactersCount() + (float)i;
			m_HiJackedCharacters[i].m_PropertyBlock.SetFloat(CullingObjectCollector.m_propID_CharacterID, m_HiJackedCharacters[i].m_HiJackedCharacterID / 255f);
			Material sharedMaterial = componentInChildren.sharedMaterial;
			if (sharedMaterial != null)
			{
				Texture mainTexture = sharedMaterial.mainTexture;
				if (mainTexture != null)
				{
					m_HiJackedCharacters[i].m_PropertyBlock.SetTexture(CullingObjectCollector.m_propID_CharacterTexture, mainTexture);
				}
			}
			componentInChildren.SetPropertyBlock(m_HiJackedCharacters[i].m_PropertyBlock);
			if (!m_HiJackedCharacters[i].m_bIntoStencilRenderer)
			{
				CharacterStencilRenderer.SetCharacterSkinnedMeshRenderer(m_HiJackedCharacters[i], componentInChildren);
				m_HiJackedCharacters[i].m_bIntoStencilRenderer = true;
			}
		}
	}
}
