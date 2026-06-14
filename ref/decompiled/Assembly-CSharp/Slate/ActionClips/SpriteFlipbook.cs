using System.Collections.Generic;
using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate a sprite object by altering between different sprites in order")]
[Category("Sprites")]
public class SpriteFlipbook : ActorActionClip<SpriteRenderer>
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[Min(1f)]
	public int loops = 1;

	public List<Sprite> sprites = new List<Sprite>();

	public bool endWithPrevious = true;

	private Sprite lastSprite;

	public override string info => isValid ? string.Empty : ((!(base.actor == null)) ? "No Sprites Set" : "No SpriteRenderer on Actor");

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

	public override bool isValid => base.actor != null && sprites.Count > 0;

	protected override void OnEnter()
	{
		lastSprite = base.actor.sprite;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (length == 0f)
		{
			return;
		}
		if (deltaTime >= length)
		{
			base.actor.sprite = sprites[sprites.Count - 1];
			return;
		}
		float t = deltaTime / length * (float)loops;
		t = Mathf.Repeat(t, 1f);
		int num = Mathf.FloorToInt(Mathf.Lerp(0f, sprites.Count, t));
		if (num < sprites.Count)
		{
			base.actor.sprite = sprites[num];
		}
	}

	protected override void OnExit()
	{
		if (endWithPrevious)
		{
			base.actor.sprite = lastSprite;
		}
	}

	protected override void OnReverse()
	{
		base.actor.sprite = lastSprite;
	}
}
