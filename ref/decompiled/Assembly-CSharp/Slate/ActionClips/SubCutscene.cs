using UnityEngine;

namespace Slate.ActionClips;

[Description("SubCutscenes are used for organization. Notice that the CameraTrack of the SubCutscene is ignored if this Cutscene already has an active CameraTrack.")]
[Category("Composition")]
public class SubCutscene : DirectorActionClip
{
	[Required]
	public Cutscene cutscene;

	private bool wasCamTrackActive;

	public override string info
	{
		get
		{
			if (object.ReferenceEquals(cutscene, base.root))
			{
				return "        SubCutscene can't be same as this cutscene";
			}
			return (!(cutscene != null)) ? "No Cutscene Selected" : $"        SubCutscene\n        '{cutscene.name}'";
		}
	}

	public override bool isValid => cutscene != null && !object.ReferenceEquals(cutscene, base.root);

	public override float length => (!isValid) ? 0f : cutscene.length;

	public new GameObject actor => (!isValid) ? base.actor : cutscene.gameObject;

	protected override void OnEnter()
	{
		if (cutscene.cameraTrack != null)
		{
			wasCamTrackActive = cutscene.cameraTrack.isActive;
			cutscene.cameraTrack.isActive = false;
		}
	}

	protected override void OnReverseEnter()
	{
		if (cutscene.cameraTrack != null)
		{
			wasCamTrackActive = cutscene.cameraTrack.isActive;
			cutscene.cameraTrack.isActive = false;
		}
	}

	protected override void OnExit()
	{
		if (cutscene.cameraTrack != null)
		{
			cutscene.cameraTrack.isActive = wasCamTrackActive;
		}
		cutscene.SkipAll();
	}

	protected override void OnReverse()
	{
		if (cutscene.cameraTrack != null)
		{
			cutscene.cameraTrack.isActive = wasCamTrackActive;
		}
		cutscene.Rewind();
	}

	protected override void OnUpdate(float time)
	{
		cutscene.Sample(time);
	}
}
