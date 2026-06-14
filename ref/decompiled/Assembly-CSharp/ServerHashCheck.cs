using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class ServerHashCheck
{
	private string m_EncryptedServerURL = string.Empty;

	private bool m_bCheckResult;

	private WWW m_WWW;

	private string m_FoundString = "1.1";

	public ServerHashCheck(string serverURL)
	{
		m_EncryptedServerURL = serverURL;
	}

	public IEnumerator MakeRequest()
	{
		m_WWW = new WWW(m_EncryptedServerURL);
		yield return m_WWW;
		string returnedValue = Encryption.Decrypt(m_WWW.text, "testpassword", "testsalt");
		m_WWW = null;
		int space = returnedValue.IndexOf(' ');
		if (space > 0)
		{
			string text = returnedValue.Substring(0, space);
			m_bCheckResult = text == GetDLLHash();
			if (m_bCheckResult)
			{
				m_FoundString = returnedValue.Substring(space + 1, returnedValue.Length - space - 1);
			}
		}
	}

	private string GetDLLHash()
	{
		string[] files = Directory.GetFiles(".\\TheEscapists2_Data\\Managed");
		foreach (string text in files)
		{
			if (text.GetHashCode() == 1771445216)
			{
				FileStream fileStream = File.OpenRead(text);
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				string result = string.Empty;
				using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
				{
					result = Convert.ToBase64String(sHA1CryptoServiceProvider.ComputeHash(array));
				}
				fileStream.Close();
				return result;
			}
		}
		return "error";
	}

	public string GetCheckResult()
	{
		return m_FoundString;
	}
}
