using UnityEngine;

public class BuildingBlockHelper
{
	public static void AddLayerShift(BaseBuildingBlock block, GameObject element)
	{
		RequiresLayerShift[] componentsInChildren = element.GetComponentsInChildren<RequiresLayerShift>();
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			if (componentsInChildren[num] != null)
			{
				Object.DestroyImmediate(componentsInChildren[num]);
				componentsInChildren[num] = null;
			}
		}
		if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Room || block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
		{
			return;
		}
		LevelSetup_RequiresLayerShift[] componentsInChildren2 = element.GetComponentsInChildren<LevelSetup_RequiresLayerShift>();
		int num2 = componentsInChildren2.Length;
		if (num2 == 0)
		{
			if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Wall || block.BlockType == BaseBuildingBlock.BuildingBlockType.Object)
			{
				LevelSetup_RequiresLayerShift levelSetup_RequiresLayerShift = element.AddComponent<LevelSetup_RequiresLayerShift>();
				levelSetup_RequiresLayerShift.ScanForContents();
			}
		}
		else
		{
			for (int i = 0; i < num2; i++)
			{
				componentsInChildren2[i].ScanForContents();
			}
		}
	}
}
