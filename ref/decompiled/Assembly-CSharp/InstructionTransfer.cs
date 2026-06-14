using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InstructionTransfer
{
	[SerializeField]
	public List<BaseBuildInstruction> m_Instructions = new List<BaseBuildInstruction>();
}
