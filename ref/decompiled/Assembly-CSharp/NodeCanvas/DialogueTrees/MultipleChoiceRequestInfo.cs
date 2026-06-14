using System;
using System.Collections.Generic;

namespace NodeCanvas.DialogueTrees;

public class MultipleChoiceRequestInfo
{
	public Dictionary<IStatement, int> options;

	public float availableTime;

	public bool showLastStatement;

	public Action<int> SelectOption;

	public MultipleChoiceRequestInfo(Dictionary<IStatement, int> options, float availableTime, bool showLastStatement, Action<int> callback)
	{
		this.options = options;
		this.availableTime = availableTime;
		this.showLastStatement = showLastStatement;
		SelectOption = callback;
	}

	public MultipleChoiceRequestInfo(Dictionary<IStatement, int> options, float availableTime, Action<int> callback)
	{
		this.options = options;
		this.availableTime = availableTime;
		SelectOption = callback;
	}
}
