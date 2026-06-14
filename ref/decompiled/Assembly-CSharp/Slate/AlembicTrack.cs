using System;
using System.Linq;

namespace Slate;

[Icon("AlembicIcon")]
[Attachable(new Type[] { typeof(DirectorGroup) })]
[Description("The Alembic Track can sample imported Alembic (.abc) files. This track does not accept any clips. Instead a virtual clip will represent the active exported frame range of the alembic file, plus any extra offset set bellow.\n\n*Alembic files should be placed under 'Assets/StreamingAssets' folder*")]
public class AlembicTrack : CutsceneTrack
{
	public AlembicStreamRoot alembicStream;

	private float abcLinkStartTime => (!(alembicStream != null)) ? float.NegativeInfinity : (alembicStream.m_startTime + alembicStream.m_timeOffset);

	private float abcLinkEndTime => (!(alembicStream != null)) ? float.NegativeInfinity : (alembicStream.m_endTime + alembicStream.m_timeOffset);

	public override string info => (!(alembicStream != null)) ? "NONE" : alembicStream.m_pathToAbc.Split('/').LastOrDefault();

	protected override void OnAfterValidate()
	{
		if (alembicStream != null)
		{
			alembicStream.Validate();
		}
	}

	protected override bool OnInitialize()
	{
		if (alembicStream != null)
		{
			alembicStream.Initialize();
			return true;
		}
		return false;
	}

	protected override void OnUpdate(float deltaTime, float previousTime)
	{
		if (alembicStream != null)
		{
			alembicStream.Sample(deltaTime);
		}
	}
}
