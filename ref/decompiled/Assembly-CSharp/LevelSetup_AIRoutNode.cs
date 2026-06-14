using System.Collections.Generic;
using UnityEngine;

public class LevelSetup_AIRoutNode : BaseComponentSetup
{
	public enum Directions
	{
		Down,
		Up,
		Right,
		Left
	}

	public Directions m_Direction;

	public static bool m_IsGuardWhoPatrols = true;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_6;
	}

	public override SetupReturnState Setup()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		if (instance.m_WaypointParent == null)
		{
			return FinishedAndRemove();
		}
		AIPatrols component = GetComponent<AIPatrols>();
		if (component == null)
		{
			return FinishedAndRemove();
		}
		SetupLightsOutPatrol();
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.GroundFloor;
		if (GetLayerAndPosition(ref X, ref Y, ref layer, FloorManager.TileSystem_Type.TileSystem_Ground))
		{
			Y = 119 - Y;
			int iIndex = X + Y * 120;
			int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref instance.m_BuildingLayers[(uint)layer], iIndex);
			if (roomNumberFromProperty != 0)
			{
				int blockIDFromComplexAllocation = instance.GetBlockIDFromComplexAllocation(roomNumberFromProperty);
				if (blockIDFromComplexAllocation != -1)
				{
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
					if (block != null && block.m_LimitationGroup != -1)
					{
						BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(block.m_LimitationGroup);
						if (limitationGroup != null)
						{
							Routines routines = Routines.UNASSIGNED;
							if (limitationGroup.m_GroupName.CompareTo(BuildingBlockManager.DefaultLimitationGroups.RollCall.ToString()) == 0)
							{
								routines = Routines.RollCall;
							}
							else if (limitationGroup.m_GroupName.CompareTo(BuildingBlockManager.DefaultLimitationGroups.MealHall.ToString()) == 0)
							{
								routines = Routines.MealTime;
							}
							else if (limitationGroup.m_GroupName.CompareTo(BuildingBlockManager.DefaultLimitationGroups.Gym.ToString()) == 0)
							{
								routines = Routines.Exercise;
							}
							else if (limitationGroup.m_GroupName.CompareTo(BuildingBlockManager.DefaultLimitationGroups.Shower.ToString()) == 0)
							{
								routines = Routines.ShowerTime;
							}
							if (RoutineHelper.IsValid(routines))
							{
								GameObject gameObject = new GameObject(routines.ToString() + "_WayPoint");
								gameObject.transform.parent = instance.m_WaypointParent.transform;
								gameObject.transform.localPosition = Vector3.zero;
								PatrolPath patrolPath = gameObject.AddComponent<PatrolPath>();
								patrolPath.m_Floor = (int)layer;
								if (patrolPath.m_vPathNodes.Length == 0)
								{
									patrolPath.m_vPathNodes = new PatrolPath.PathNode[1];
								}
								if (patrolPath.m_vPathNodes[0] == null)
								{
									patrolPath.m_vPathNodes[0] = new PatrolPath.PathNode();
								}
								Directionx4 direction = m_Direction switch
								{
									Directions.Down => Directionx4.Down, 
									Directions.Up => Directionx4.Up, 
									Directions.Right => Directionx4.Right, 
									Directions.Left => Directionx4.Left, 
									_ => Directionx4.Down, 
								};
								patrolPath.m_vPathNodes[0].m_bSetDirection = true;
								patrolPath.m_vPathNodes[0].m_bRunToNode = false;
								patrolPath.m_vPathNodes[0].m_vNodePos = base.transform.position;
								patrolPath.m_vPathNodes[0].m_FacingDirection = Direction.DirectionToRotation(direction);
								patrolPath.m_vPathNodes[0].m_iIndex = 0;
								for (int num = component.m_RoutinePatrols.Count - 1; num >= 0; num--)
								{
									if (component.m_RoutinePatrols[num] != null && component.m_RoutinePatrols[num].routine == routines)
									{
										component.m_RoutinePatrols[num].patrol = new List<PatrolPath>();
										component.m_RoutinePatrols[num].patrol.Add(patrolPath);
										return FinishedAndRemove();
									}
								}
								AIPatrols.RoutinePatrol routinePatrol = new AIPatrols.RoutinePatrol();
								routinePatrol.routine = routines;
								routinePatrol.patrol = new List<PatrolPath>();
								routinePatrol.patrol.Add(patrolPath);
								component.m_RoutinePatrols.Add(routinePatrol);
								return FinishedAndRemove();
							}
						}
					}
				}
			}
			return FinishedAndRemove();
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		if (instance.m_WaypointParent == null)
		{
			return FinishedAndRemove();
		}
		AIPatrols component = GetComponent<AIPatrols>();
		if (component == null)
		{
			return FinishedAndRemove();
		}
		LevelEditor_ZoneManager instance2 = LevelEditor_ZoneManager.GetInstance();
		if (instance2 == null)
		{
			return FinishedAndRemove();
		}
		SetupLightsOutPatrol();
		int iIndex = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL;
		if (GetLayerAndZoneMapIndex(ref iIndex, ref layer, FloorManager.TileSystem_Type.TileSystem_ObjectPlops))
		{
			LevelEditor_ZoneManager.ZoneMap zoneMap = instance2.GetZoneMap(layer);
			int num = zoneMap.m_Map[iIndex];
			if (num != -1)
			{
				LevelEditor_ZoneManager.Zone zone = instance2.GetZone(num);
				if (zone != null)
				{
					Routines routines = Routines.UNASSIGNED;
					switch (zone.m_ZoneType)
					{
					case ZoneDetailsManager.ZoneTypes.RollCall:
						routines = Routines.RollCall;
						break;
					case ZoneDetailsManager.ZoneTypes.MealHall:
						routines = Routines.MealTime;
						break;
					case ZoneDetailsManager.ZoneTypes.Gym:
						routines = Routines.Exercise;
						break;
					case ZoneDetailsManager.ZoneTypes.Shower:
						routines = Routines.ShowerTime;
						break;
					}
					if (RoutineHelper.IsValid(routines))
					{
						GameObject gameObject = new GameObject(routines.ToString() + "_WayPoint");
						gameObject.transform.parent = instance.m_WaypointParent.transform;
						gameObject.transform.localPosition = Vector3.zero;
						PatrolPath patrolPath = gameObject.AddComponent<PatrolPath>();
						patrolPath.m_Floor = (int)layer;
						if (patrolPath.m_vPathNodes.Length == 0)
						{
							patrolPath.m_vPathNodes = new PatrolPath.PathNode[1];
						}
						if (patrolPath.m_vPathNodes[0] == null)
						{
							patrolPath.m_vPathNodes[0] = new PatrolPath.PathNode();
						}
						Directionx4 direction = m_Direction switch
						{
							Directions.Down => Directionx4.Down, 
							Directions.Up => Directionx4.Up, 
							Directions.Right => Directionx4.Right, 
							Directions.Left => Directionx4.Left, 
							_ => Directionx4.Down, 
						};
						patrolPath.m_vPathNodes[0].m_bSetDirection = true;
						patrolPath.m_vPathNodes[0].m_bRunToNode = false;
						patrolPath.m_vPathNodes[0].m_vNodePos = base.transform.position;
						patrolPath.m_vPathNodes[0].m_FacingDirection = Direction.DirectionToRotation(direction);
						patrolPath.m_vPathNodes[0].m_iIndex = 0;
						for (int num2 = component.m_RoutinePatrols.Count - 1; num2 >= 0; num2--)
						{
							if (component.m_RoutinePatrols[num2] != null && component.m_RoutinePatrols[num2].routine == routines)
							{
								component.m_RoutinePatrols[num2].patrol = new List<PatrolPath>();
								component.m_RoutinePatrols[num2].patrol.Add(patrolPath);
								return FinishedAndRemove();
							}
						}
						AIPatrols.RoutinePatrol routinePatrol = new AIPatrols.RoutinePatrol();
						routinePatrol.routine = routines;
						routinePatrol.patrol = new List<PatrolPath>();
						routinePatrol.patrol.Add(patrolPath);
						component.m_RoutinePatrols.Add(routinePatrol);
						return FinishedAndRemove();
					}
				}
			}
			return FinishedAndRemove();
		}
		return FinishedAndRemove();
	}

	private void SetupLightsOutPatrol()
	{
		if (!m_IsGuardWhoPatrols)
		{
			return;
		}
		m_IsGuardWhoPatrols = false;
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		List<BaseLevelManager.InterestingLocations> interestingLocationsList = instance.GetInterestingLocationsList();
		List<BaseLevelManager.InterestingLocations>[] array = new List<BaseLevelManager.InterestingLocations>[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = new List<BaseLevelManager.InterestingLocations>();
		}
		int count = interestingLocationsList.Count;
		for (int j = 0; j < count; j++)
		{
			if (interestingLocationsList[j].m_Type == BaseLevelManager.InterestingLocations.LocationType.OutsideDoor)
			{
				array[(uint)interestingLocationsList[j].m_Layer].Add(interestingLocationsList[j]);
			}
		}
		List<BaseLevelManager.InterestingLocations> list = new List<BaseLevelManager.InterestingLocations>();
		for (int k = 0; k < 6; k++)
		{
			if ((byte)k == 4 || (byte)k == 2 || (byte)k == 0 || array[k].Count <= 0)
			{
				continue;
			}
			BaseLevelManager.InterestingLocations interestingLocations = array[k][0];
			array[k].RemoveAt(0);
			list.Add(interestingLocations);
			while (array[k].Count > 0)
			{
				BaseLevelManager.InterestingLocations interestingLocations2 = null;
				for (int l = 0; l < array[k].Count; l++)
				{
					if (interestingLocations2 == null)
					{
						interestingLocations2 = array[k][l];
						continue;
					}
					Vector3 vector = interestingLocations2.m_Position - interestingLocations.m_Position;
					if ((array[k][l].m_Position - interestingLocations.m_Position).magnitude < vector.magnitude)
					{
						interestingLocations2 = array[k][l];
					}
				}
				if (interestingLocations2 != null)
				{
					array[k].Remove(interestingLocations2);
					list.Add(interestingLocations2);
					interestingLocations = interestingLocations2;
				}
			}
		}
		if (list.Count > 0)
		{
			GameObject gameObject = new GameObject("AutoLightsOut_WayPoint");
			gameObject.transform.parent = instance.m_WaypointParent.transform;
			gameObject.transform.localPosition = Vector3.zero;
			PatrolPath patrolPath = gameObject.AddComponent<PatrolPath>();
			patrolPath.m_vPathNodes = new PatrolPath.PathNode[list.Count];
			for (int m = 0; m < list.Count; m++)
			{
				patrolPath.m_vPathNodes[m] = new PatrolPath.PathNode();
				patrolPath.m_vPathNodes[m].m_bSetDirection = false;
				patrolPath.m_vPathNodes[m].m_bRunToNode = false;
				patrolPath.m_vPathNodes[m].m_vNodePos = list[m].m_Position;
				patrolPath.m_vPathNodes[m].m_iIndex = m;
				patrolPath.m_vPathNodes[m]._m_fWaitTimer = 1.5f;
			}
			AIPatrols component = GetComponent<AIPatrols>();
			AIPatrols.RoutinePatrol routinePatrol = new AIPatrols.RoutinePatrol();
			routinePatrol.routine = Routines.LightsOut;
			routinePatrol.patrol = new List<PatrolPath>();
			routinePatrol.patrol.Add(patrolPath);
			component.m_RoutinePatrols.Add(routinePatrol);
		}
	}
}
