using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slate.ActionClips;

[Description("Instantiates an object with optional popup animation if BlendIn is higher than zero. You can optionaly 'popdown' and destroy the object after a period of time, if you also set a BlendOut value higher than zero.")]
[Category("Control")]
public class InstantiateObject : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 2f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.8f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut;

	[Required]
	public GameObject targetObject;

	public Transform optionalParent;

	public Vector3 targetPosition;

	public Vector3 targetRotation;

	public MiniTransformSpace space;

	public EaseType popupInterpolation = EaseType.ElasticInOut;

	private GameObject instance;

	private Vector3 originalScale;

	public override bool isValid => targetObject != null;

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	public override float blendIn
	{
		get
		{
			return _blendIn;
		}
		set
		{
			_blendIn = value;
		}
	}

	public override float blendOut
	{
		get
		{
			return _blendOut;
		}
		set
		{
			_blendOut = value;
		}
	}

	public override string info => string.Format("Instantiate\n{0}", (!(targetObject != null)) ? "NULL" : targetObject.name);

	protected override void OnEnter()
	{
		originalScale = targetObject.transform.localScale;
		instance = Object.Instantiate(targetObject);
		SceneManager.MoveGameObjectToScene(instance, base.root.context.scene);
		instance.transform.parent = optionalParent;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (instance != null)
		{
			instance.transform.position = TransformPoint(targetPosition, (TransformSpace)space);
			instance.transform.localEulerAngles = targetRotation;
			instance.transform.localScale = Easing.Ease(popupInterpolation, Vector3.zero, originalScale, GetClipWeight(deltaTime));
		}
	}

	protected override void OnExit()
	{
		if (blendOut > 0f)
		{
			Object.DestroyImmediate(instance, allowDestroyingAssets: false);
		}
	}

	protected override void OnReverseEnter()
	{
		if (blendOut > 0f)
		{
			OnEnter();
		}
	}

	protected override void OnReverse()
	{
		Object.DestroyImmediate(instance, allowDestroyingAssets: false);
	}
}
