public class T17Regex
{
	public class StringMatch
	{
		public string value;

		public int position;
	}

	private string _tokenStart;

	private string _tokenEnd;

	private static StringMatch[] _stringlist = new StringMatch[128];

	public T17Regex(string tokenStart, string tokenEnd)
	{
		_tokenStart = tokenStart;
		_tokenEnd = tokenEnd;
	}

	~T17Regex()
	{
		_tokenStart = null;
		_tokenEnd = null;
	}

	public StringMatch[] Matches(string s, out int count)
	{
		count = 0;
		int startIndex = 0;
		int num = 0;
		int length = _tokenEnd.Length;
		while (true)
		{
			startIndex = s.IndexOf(_tokenStart, startIndex);
			if (startIndex >= 0)
			{
				num = s.IndexOf(_tokenEnd, startIndex + 1);
				if (num >= 0)
				{
					string value = s.Substring(startIndex, num - startIndex + length);
					StringMatch stringMatch = _stringlist[count];
					if (stringMatch == null)
					{
						stringMatch = new StringMatch();
						_stringlist[count] = stringMatch;
					}
					stringMatch.position = startIndex;
					stringMatch.value = value;
					count++;
					startIndex = num + 1;
					continue;
				}
				break;
			}
			break;
		}
		return _stringlist;
	}
}
