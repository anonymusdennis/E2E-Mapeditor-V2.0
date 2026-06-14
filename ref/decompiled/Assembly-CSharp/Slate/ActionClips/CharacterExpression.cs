using UnityEngine;

namespace Slate.ActionClips;

[Category("Character")]
[Description("Sets a collection of BlendShapes (an expression), which you can create in the Character Component Inspector of the actor.")]
public class CharacterExpression : ActorActionClip<Character>
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.25f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.25f;

	[HideInInspector]
	public string expressionName;

	[HideInInspector]
	public string expressionUID;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	private float originalWeight;

	private BlendShapeGroup expression;

	public override string info
	{
		get
		{
			BlendShapeGroup blendShapeGroup = ((!(base.actor != null)) ? null : ResolveExpression());
			return string.Format("Expression '{0}'", (blendShapeGroup == null) ? "NONE" : blendShapeGroup.name);
		}
	}

	public override bool isValid => base.actor != null;

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

	private BlendShapeGroup ResolveExpression()
	{
		if (!string.IsNullOrEmpty(expressionUID))
		{
			return base.actor.FindExpressionByUID(expressionUID);
		}
		return base.actor.FindExpressionByName(expressionName);
	}

	protected override void OnEnter()
	{
		expression = ResolveExpression();
		if (expression != null)
		{
			originalWeight = expression.weight;
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (expression != null)
		{
			float num = Easing.Ease(EaseType.QuadraticInOut, originalWeight, weight, GetClipWeight(deltaTime));
			expression.weight = num;
		}
	}

	protected override void OnReverse()
	{
		if (expression != null)
		{
			expression.weight = originalWeight;
			expression = null;
		}
	}
}
