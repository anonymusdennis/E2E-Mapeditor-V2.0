using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check the state of a Wall Torch")]
[Category("★T17 Jobs")]
public class CheckWallTorchState : ConditionTask<AICharacter>
{
	public BBParameter<InteractiveObject> m_TargetWallTorch;

	public WallTorch.TorchState m_ExpectedState = WallTorch.TorchState.UnfueledTorch;

	protected override string info => "Wall Torch is " + m_ExpectedState;

	protected override bool OnCheck()
	{
		if (m_TargetWallTorch == null || m_TargetWallTorch.value == null)
		{
			return false;
		}
		WallTorch component = m_TargetWallTorch.value.GetComponent<WallTorch>();
		if (component == null)
		{
			return false;
		}
		return component.GetTorchState() == m_ExpectedState;
	}
}
