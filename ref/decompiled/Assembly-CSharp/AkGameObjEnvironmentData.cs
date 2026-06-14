using System.Collections.Generic;
using UnityEngine;

public class AkGameObjEnvironmentData
{
	private List<AkEnvironment> activeEnvironmentsFromPortals = new List<AkEnvironment>();

	private List<AkEnvironment> activeEnvironments = new List<AkEnvironment>();

	private List<AkEnvironmentPortal> activePortals = new List<AkEnvironmentPortal>();

	private AkAuxSendArray auxSendValues = new AkAuxSendArray();

	private bool isDirty = true;

	private void AddHighestPriorityEnvironmentsFromPortals(Vector3 position)
	{
		for (int i = 0; i < activePortals.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				AkEnvironment akEnvironment = activePortals[i].environments[j];
				if (!(akEnvironment != null))
				{
					continue;
				}
				int num = activeEnvironmentsFromPortals.BinarySearch(akEnvironment, AkEnvironment.s_compareByPriority);
				if (num >= 0 && num < 4)
				{
					auxSendValues.Add(akEnvironment.GetAuxBusID(), activePortals[i].GetAuxSendValueForPosition(position, j));
					if (auxSendValues.isFull)
					{
						return;
					}
				}
			}
		}
	}

	private void AddHighestPriorityEnvironments(Vector3 position)
	{
		if (auxSendValues.isFull || auxSendValues.m_Count >= activeEnvironments.Count)
		{
			return;
		}
		for (int i = 0; i < activeEnvironments.Count; i++)
		{
			AkEnvironment akEnvironment = activeEnvironments[i];
			uint auxBusID = akEnvironment.GetAuxBusID();
			if ((!akEnvironment.isDefault || i == 0) && !auxSendValues.Contains(auxBusID))
			{
				auxSendValues.Add(auxBusID, akEnvironment.GetAuxSendValueForPosition(position));
				if (akEnvironment.excludeOthers || auxSendValues.isFull)
				{
					break;
				}
			}
		}
	}

	public void UpdateAuxSend(GameObject gameObject, Vector3 position)
	{
		if (isDirty)
		{
			auxSendValues.Reset();
			AddHighestPriorityEnvironmentsFromPortals(position);
			AddHighestPriorityEnvironments(position);
			AkSoundEngine.SetGameObjectAuxSendValues(gameObject, auxSendValues, auxSendValues.m_Count);
			isDirty = false;
		}
	}

	private void TryAddEnvironment(AkEnvironment env)
	{
		if (!(env != null))
		{
			return;
		}
		int num = activeEnvironmentsFromPortals.BinarySearch(env, AkEnvironment.s_compareByPriority);
		if (num < 0)
		{
			activeEnvironmentsFromPortals.Insert(~num, env);
			num = activeEnvironments.BinarySearch(env, AkEnvironment.s_compareBySelectionAlgorithm);
			if (num < 0)
			{
				activeEnvironments.Insert(~num, env);
			}
			isDirty = true;
		}
	}

	private void RemoveEnvironment(AkEnvironment env)
	{
		activeEnvironmentsFromPortals.Remove(env);
		activeEnvironments.Remove(env);
		isDirty = true;
	}

	public void AddAkEnvironment(GameObject in_AuxSendObject, GameObject gameObject)
	{
		AkEnvironmentPortal component = in_AuxSendObject.GetComponent<AkEnvironmentPortal>();
		if (component != null)
		{
			activePortals.Add(component);
			for (int i = 0; i < 2; i++)
			{
				TryAddEnvironment(component.environments[i]);
			}
		}
		else
		{
			AkEnvironment component2 = in_AuxSendObject.GetComponent<AkEnvironment>();
			TryAddEnvironment(component2);
		}
	}

	private bool AkEnvironmentBelongsToActivePortals(AkEnvironment env)
	{
		for (int i = 0; i < activePortals.Count; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (env == activePortals[i].environments[j])
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveAkEnvironment(GameObject in_AuxSendObject, GameObject gameObject, Collider collider)
	{
		AkEnvironmentPortal component = in_AuxSendObject.GetComponent<AkEnvironmentPortal>();
		if (component != null)
		{
			for (int i = 0; i < 2; i++)
			{
				AkEnvironment akEnvironment = component.environments[i];
				if (akEnvironment != null && !collider.bounds.Intersects(akEnvironment.GetCollider().bounds))
				{
					RemoveEnvironment(akEnvironment);
				}
			}
			activePortals.Remove(component);
			isDirty = true;
		}
		else
		{
			AkEnvironment component2 = in_AuxSendObject.GetComponent<AkEnvironment>();
			if (component2 != null && !AkEnvironmentBelongsToActivePortals(component2))
			{
				RemoveEnvironment(component2);
			}
		}
	}
}
