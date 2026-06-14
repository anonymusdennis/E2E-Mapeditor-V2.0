using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class DropCarryableObject : ActionTask<AICharacter>
{
	public BBParameter<GameObject> m_Consumer;

	public BBParameter<InteractiveObject> m_Processor;

	private CarryableObjectConsumer m_CarryableObjectConsumer;

	protected override void OnExecute()
	{
		Character character = base.agent.m_Character;
		if (character != null)
		{
			CarryableObjectConsumer carryableObjectConsumer = null;
			if (m_Consumer.value != null)
			{
				carryableObjectConsumer = m_Consumer.value.GetComponent<CarryableObjectConsumer>();
			}
			else if (m_Processor.value != null)
			{
				carryableObjectConsumer = m_Processor.value.GetComponent<CarryableObjectConsumer>();
			}
			if (carryableObjectConsumer != null)
			{
				m_CarryableObjectConsumer = carryableObjectConsumer;
			}
			else
			{
				EndAction(false);
			}
		}
		else
		{
			EndAction(false);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!m_CarryableObjectConsumer.IsProcessing())
		{
			Vector2 directionVector = m_CarryableObjectConsumer.transform.position - base.agent.m_Character.GetCachedCurrentPosition();
			Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(directionVector);
			base.agent.m_Character.SetFaceDirection(headAndBodyDirection);
			base.agent.m_Character.RequestStopInteraction();
			EndAction(true);
		}
	}
}
