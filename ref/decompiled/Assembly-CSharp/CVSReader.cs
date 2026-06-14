using System;
using System.Text;

public class CVSReader
{
	private byte[] m_Data;

	private int m_Pos;

	public CVSReader(byte[] bytes)
	{
		m_Data = bytes;
		m_Pos = 0;
	}

	private string ReadLine()
	{
		string empty = string.Empty;
		int i;
		for (i = m_Pos; i < m_Data.Length && m_Data[i] != 10 && m_Data[i] != 13; i++)
		{
		}
		empty = Encoding.UTF8.GetString(m_Data, m_Pos, i - m_Pos);
		m_Pos = i + 1;
		if (m_Pos >= m_Data.Length)
		{
			m_Pos = ((m_Data.Length > 0) ? (m_Data.Length - 1) : 0);
		}
		if (m_Data[m_Pos] == 10 || m_Data[m_Pos] == 13)
		{
			m_Pos++;
		}
		return empty.Replace("\\n", "\n");
	}

	public string[] ReadRow()
	{
		string[] result = null;
		string[] separator = new string[1] { "\t" };
		if (m_Pos < m_Data.Length)
		{
			string text = ReadLine();
			result = text.Split(separator, StringSplitOptions.None);
		}
		return result;
	}
}
