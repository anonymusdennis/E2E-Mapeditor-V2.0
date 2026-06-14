using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LevelSetup_PostProcess : BaseComponentSetup
{
	public enum PostProcessStageEnum
	{
		Start,
		LightSettings,
		LinkTileTools,
		PrisonLayer,
		MapTextureGenerator,
		CutsceneAnimationRelinker,
		PlopAtlasTool,
		MaterialMergeTool,
		AmbientLightMarkerTool,
		FloorBakeTool,
		FloorPostProcessInit,
		FloorPostProcess,
		EnableLevelScript,
		Customisations,
		Finished
	}

	private enum StageResult
	{
		Finished,
		NeedMoreTime
	}

	private Stopwatch m_StopWatch = new Stopwatch();

	private PostProcessStageEnum m_State;

	private long m_TimeOut = 300L;

	public GameObject[] m_LayerRoots = new GameObject[6];

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_ReservedForPostprocess;
	}

	public override SetupReturnState Setup()
	{
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			switch (m_State)
			{
			case PostProcessStageEnum.Start:
				m_State = PostProcessStageEnum.LightSettings;
				break;
			case PostProcessStageEnum.LightSettings:
				switch (LightSettingsStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.LinkTileTools;
					break;
				}
				break;
			case PostProcessStageEnum.LinkTileTools:
				switch (LinkTileToolsStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.PrisonLayer;
					break;
				}
				break;
			case PostProcessStageEnum.PrisonLayer:
				switch (PrisonLayerStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.MapTextureGenerator;
					break;
				}
				break;
			case PostProcessStageEnum.MapTextureGenerator:
				switch (MapTextureGeneratorStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.CutsceneAnimationRelinker;
					break;
				}
				break;
			case PostProcessStageEnum.CutsceneAnimationRelinker:
				switch (CutsceneAnimationRelinkerStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.PlopAtlasTool;
					break;
				}
				break;
			case PostProcessStageEnum.PlopAtlasTool:
				switch (PlopAtlasToolStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.MaterialMergeTool;
					break;
				}
				break;
			case PostProcessStageEnum.MaterialMergeTool:
				switch (MaterialMergeToolStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.AmbientLightMarkerTool;
					break;
				}
				break;
			case PostProcessStageEnum.AmbientLightMarkerTool:
				switch (AmbientLightMarkerToolStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.FloorBakeTool;
					break;
				}
				break;
			case PostProcessStageEnum.FloorBakeTool:
				switch (FloorBakeToolStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.Customisations;
					break;
				}
				break;
			case PostProcessStageEnum.Customisations:
				switch (CustomisationProcessStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.FloorPostProcessInit;
					break;
				}
				break;
			case PostProcessStageEnum.FloorPostProcessInit:
				switch (FloorPostProcessStageInit())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.FloorPostProcess;
					break;
				}
				break;
			case PostProcessStageEnum.FloorPostProcess:
				switch (FloorPostProcessStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = PostProcessStageEnum.EnableLevelScript;
					break;
				}
				break;
			case PostProcessStageEnum.EnableLevelScript:
				if (LevelScript.GetInstance() != null)
				{
					LevelScript.GetInstance().m_Processed = true;
				}
				m_State = PostProcessStageEnum.Finished;
				break;
			case PostProcessStageEnum.Finished:
				LevelSetup_AIRoutNode.m_IsGuardWhoPatrols = true;
				CleanUp();
				m_StopWatch.Stop();
				return FinishedAndRemove();
			}
		}
		m_StopWatch.Stop();
		return TakeABreak();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}

	private StageResult LightSettingsStage()
	{
		LightingManager classInstance = GetClassInstance<LightingManager>();
		if (classInstance != null)
		{
			classInstance.ForceSettings();
		}
		return StageResult.Finished;
	}

	private StageResult LinkTileToolsStage()
	{
		return StageResult.Finished;
	}

	private StageResult PrisonLayerStage()
	{
		return StageResult.Finished;
	}

	private StageResult MapTextureGeneratorStage()
	{
		return StageResult.Finished;
	}

	private StageResult CutsceneAnimationRelinkerStage()
	{
		return StageResult.Finished;
	}

	private StageResult PlopAtlasToolStage()
	{
		return StageResult.Finished;
	}

	private StageResult MaterialMergeToolStage()
	{
		return StageResult.Finished;
	}

	private StageResult AmbientLightMarkerToolStage()
	{
		return StageResult.Finished;
	}

	private StageResult FloorBakeToolStage()
	{
		return StageResult.Finished;
	}

	private StageResult FloorPostProcessStageInit()
	{
		RoomProcessingTool.Init(bForRunTime: true);
		return StageResult.Finished;
	}

	private StageResult FloorPostProcessStage()
	{
		if (!RoomProcessingTool.StartProcessing())
		{
			return StageResult.NeedMoreTime;
		}
		if (LevelDetailsManager.GetInstance() != null)
		{
			string blockSceneName = LevelDetailsManager.GetInstance().GetBlockSceneName();
			GameObject[] array = GameObject.FindGameObjectsWithTag("EditorOnly");
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (!(array[i].scene.name == blockSceneName))
				{
					Object.DestroyImmediate(array[i]);
				}
			}
		}
		RoomProcessingTool.Finished();
		return StageResult.Finished;
	}

	private StageResult CustomisationProcessStage()
	{
		List<Character> characters = CustomisationGeneratorTools.FindAllCharacters();
		CustomisationGeneratorTools.AssignCharacterIdentifiers(characters);
		return StageResult.Finished;
	}
}
