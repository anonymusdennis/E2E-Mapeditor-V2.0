using System.Collections.Generic;
using UnityEngine;

public class UI_DrawCharacterToRenderTexture : MonoBehaviour
{
	public bool m_bHeadOnly;

	private Transform m_HeadTransform;

	private CustomisationData.BodyData m_LastBodyTextureData;

	private CustomisationData.BodyType m_LastBodyType = CustomisationData.BodyType.NULL;

	private CustomisationData.SkinColour m_LastSkinType = CustomisationData.SkinColour.NULL;

	private bool m_bInitialiseAnimator;

	private List<GameObject> m_Activators = new List<GameObject>();

	private CharacterAnimator m_CharacterAnimator;

	private Camera m_Camera;

	private int m_propID_Highlight = -1;

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	private float m_TimeOfLastAnimUpdate;

	private void Awake()
	{
		m_Camera = GetComponent<Camera>();
		m_CharacterAnimator = GetComponentInChildren<CharacterAnimator>();
		m_CharacterAnimator.SetCharacterAnimatorType(CharacterAnimator.ANIMATOR_TYPE.AT_CLIVE);
		if (m_CharacterAnimator != null)
		{
			m_HeadTransform = m_CharacterAnimator.transform.Find("Animation/Clive1");
		}
		m_CharacterAnimator.ChangeToUIMaterails();
		m_MaterialPropertyBlock = new MaterialPropertyBlock();
		m_propID_Highlight = Shader.PropertyToID("_Highlight");
	}

	private void Start()
	{
		if (m_Activators.Count <= 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		m_bInitialiseAnimator = true;
	}

	public void RegisterActivationRequest(GameObject activator)
	{
		m_Activators.Add(activator);
		base.gameObject.SetActive(value: true);
	}

	public void UnregisterActivationRequest(GameObject activator)
	{
		int num = m_Activators.IndexOf(activator);
		if (num >= 0)
		{
			m_Activators.RemoveAt(num);
		}
		if (m_Activators.Count <= 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public RenderTexture CreateRenderTexture(int width, int height, ref int ID)
	{
		return RenderTargetManager.RequestRenderTarget(width, height, 24, RenderTextureFormat.ARGB32, ref ID, "DCTRT");
	}

	public bool CleanupRenderTexture(ref int textureID)
	{
		RenderTargetManager.ReleaseRenderTarget(ref textureID);
		return true;
	}

	public bool SetCustomisation(CharacterCustomisation customisation)
	{
		if (customisation != null && m_CharacterAnimator != null)
		{
			bool flag = false;
			flag = customisation.SetupCharacterAnimator(m_CharacterAnimator);
			m_CharacterAnimator.SetMaterialHandHeld(null);
			m_CharacterAnimator.SetMaterialTorch(null);
			m_CharacterAnimator.ChangeToUIMaterails();
			return flag;
		}
		return false;
	}

	public void SetCustomisation(Customisation customisation, CustomisationData.Outfit outfitOverride = CustomisationData.Outfit.NULL)
	{
		if (customisation != null)
		{
			CustomisationData.Outfit outfit = ((outfitOverride == CustomisationData.Outfit.NULL) ? customisation.defaultOutfit : outfitOverride);
			SetCustomisation(customisation.body, customisation.skin, outfit, customisation.hair, customisation.hat, customisation.upperFace, customisation.lowerFace);
		}
	}

	public void SetCustomisation(CustomisationData.BodyType body, CustomisationData.SkinColour skin, CustomisationData.Outfit outfit, CustomisationData.Hair hair, CustomisationData.Hat hat, CustomisationData.UpperFaceAccessory upperFace, CustomisationData.LowerFaceAccessory lowerFace)
	{
		if (body != m_LastBodyType || skin != m_LastSkinType)
		{
			m_LastBodyTextureData = CustomisationData.GetInstance().GetCustomisationData(body, skin);
			m_LastBodyType = body;
			m_LastSkinType = skin;
		}
		Material bodyMaterial = ((m_LastBodyTextureData == null) ? null : m_LastBodyTextureData.GetMaterialForOutfit((outfit != CustomisationData.Outfit.NULL) ? outfit : CustomisationData.Outfit.NONE));
		Material materialForHair = CustomisationData.GetInstance().GetMaterialForHair(hair);
		Material materialForHat = CustomisationData.GetInstance().GetMaterialForHat(hat);
		Material materialForUpperFace = CustomisationData.GetInstance().GetMaterialForUpperFace(upperFace);
		Material materialForLowerFace = CustomisationData.GetInstance().GetMaterialForLowerFace(lowerFace);
		if (m_CharacterAnimator != null)
		{
			m_CharacterAnimator.SetMaterialAppearance(bodyMaterial, materialForHair, materialForHat, materialForUpperFace, materialForLowerFace);
			m_CharacterAnimator.SetMaterialHandHeld(null);
			m_CharacterAnimator.SetMaterialTorch(null);
			m_CharacterAnimator.ChangeToUIMaterails();
		}
	}

	public void DrawCharacter(RenderTexture texture, bool bAllowSkipRender = false)
	{
		if (texture == null)
		{
			return;
		}
		bool flag = false;
		if (!texture.IsCreated())
		{
			texture.Create();
		}
		if (bAllowSkipRender)
		{
			if (UpdateManager.frameCount % 10 != 0)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (!(m_Camera != null))
		{
			return;
		}
		m_Camera.targetTexture = texture;
		float width = (float)texture.width / (float)texture.height;
		m_Camera.rect.Set(0f, 0f, width, 1f);
		if (m_bInitialiseAnimator)
		{
			SetHeadOnlyMode(m_bHeadOnly);
			if (m_CharacterAnimator != null && m_CharacterAnimator.m_CharacterAnimator != null)
			{
				m_CharacterAnimator.m_CharacterAnimator.gameObject.SetActive(value: true);
				m_CharacterAnimator.m_CharacterAnimator.enabled = true;
				m_CharacterAnimator.HeadAndBodyFaceDirection(Directionx4.Down, force: true);
				m_CharacterAnimator.OnAnimatorEnabled();
				m_CharacterAnimator.m_CharacterAnimator.Update(0f);
				m_CharacterAnimator.m_CharacterAnimator.enabled = false;
			}
			m_bInitialiseAnimator = false;
		}
		if (flag)
		{
			m_Camera.Render();
		}
		m_Camera.targetTexture = null;
	}

	public void UpdateAnimation(float delta)
	{
		if (!(m_CharacterAnimator != null) || !(m_CharacterAnimator.m_CharacterAnimator != null))
		{
			return;
		}
		bool flag = true;
		if (m_Activators.Count > 1)
		{
			if (Time.time == m_TimeOfLastAnimUpdate)
			{
				flag = false;
			}
			else
			{
				m_TimeOfLastAnimUpdate = Time.time;
			}
		}
		if (flag)
		{
			m_CharacterAnimator.m_CharacterAnimator.Update(delta);
		}
	}

	public bool SetAndDrawCharacter(CharacterCustomisation customisation, RenderTexture texture, float deltaTime = 0f, bool bAllowSkipRender = false)
	{
		if (SetCustomisation(customisation))
		{
			UpdateAnimation(deltaTime);
			DrawCharacter(texture, bAllowSkipRender);
			return true;
		}
		return false;
	}

	public void SetHighlighted(bool bIsHighlighted)
	{
		if (bIsHighlighted)
		{
			m_MaterialPropertyBlock.SetFloat(m_propID_Highlight, 1f);
		}
		else
		{
			m_MaterialPropertyBlock.SetFloat(m_propID_Highlight, 0f);
		}
		m_CharacterAnimator.SetMatBlock(m_MaterialPropertyBlock);
	}

	public void SetHeadOnlyMode(bool headOnly)
	{
		if (m_HeadTransform == null)
		{
			return;
		}
		Transform parent = m_HeadTransform.parent;
		if (!(parent != null))
		{
			return;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child != m_HeadTransform)
			{
				child.gameObject.SetActive(!headOnly);
			}
		}
	}
}
