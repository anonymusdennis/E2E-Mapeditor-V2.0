using UnityEngine;

namespace Slate.ActionClips;

[Category("Sprites")]
public class SetSprite : ActorActionClip<SpriteRenderer>
{
	[Header("Basic")]
	public Sprite sprite;

	public Color color = Color.white;

	public bool flipX;

	public bool flipY;

	[Header("Sorting")]
	public bool changeSorting;

	[SortingLayer]
	public int sortingLayerID;

	public int sortingOrder;

	private Sprite lastSprite;

	private Color lastColor;

	private bool lastFlipX;

	private bool lastFlipY;

	private int lastSortingLayerID;

	private int lastSortingOrder;

	public override string info => (!(sprite == null)) ? string.Empty : "No Sprite Set";

	protected override void OnEnter()
	{
		lastSprite = base.actor.sprite;
		lastColor = base.actor.color;
		lastFlipX = base.actor.flipX;
		lastFlipY = base.actor.flipY;
		lastSortingLayerID = base.actor.sortingLayerID;
		lastSortingOrder = base.actor.sortingOrder;
		base.actor.sprite = sprite;
		base.actor.color = color;
		base.actor.flipX = flipX;
		base.actor.flipY = flipY;
		if (changeSorting)
		{
			base.actor.sortingLayerID = sortingLayerID;
			base.actor.sortingOrder = sortingOrder;
		}
	}

	protected override void OnReverse()
	{
		base.actor.sprite = lastSprite;
		base.actor.color = lastColor;
		base.actor.flipX = lastFlipX;
		base.actor.flipY = lastFlipY;
		base.actor.sortingLayerID = lastSortingLayerID;
		base.actor.sortingOrder = lastSortingOrder;
	}
}
