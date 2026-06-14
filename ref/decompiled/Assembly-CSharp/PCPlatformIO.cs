using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PCPlatformIO : PlatformIO
{
	private void Start()
	{
		m_strSavePath = Application.persistentDataPath + m_SeperationChar + "Saves" + m_SeperationChar + Platform.GetInstance().GetUniqueID() + m_SeperationChar;
		if (!Directory.Exists(m_strSavePath))
		{
			Directory.CreateDirectory(m_strSavePath);
		}
	}

	protected override IORequestResultEnum PlatformSaveFile(int iHandle, string strDirectory, string strFilename, object objData, object platformData)
	{
		if (objData != null && !string.IsNullOrEmpty(strFilename))
		{
			string path = GetPath(strDirectory);
			string text = path + strFilename;
			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				bool flag = false;
				if (File.Exists(text))
				{
					flag = true;
					File.SetAttributes(text, FileAttributes.Normal);
				}
				if (File.Exists(text + "_Backup"))
				{
					File.Delete(text + "_Backup");
				}
				if (File.Exists(text + "_Temp"))
				{
					File.Delete(text + "_Temp");
				}
				StreamWriter streamWriter = File.CreateText(text + "_Temp");
				streamWriter.WriteLine(Encryption.Encrypt(objData, "of all the flavours you choose to be salty", "SHA1", 2, 256, string.Empty));
				streamWriter.Close();
				if (flag)
				{
					File.Move(text, text + "_Backup");
				}
				File.Move(text + "_Temp", text);
				RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
			}
			catch
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
			}
			return IORequestResultEnum.IORequestSuccessful;
		}
		return IORequestResultEnum.IORequestFailedInvalidRequest;
	}

	protected override IORequestResultEnum PlatformLoadFile(int iHandle, string strDirectory, string strFilename, object platformData)
	{
		if (!string.IsNullOrEmpty(strFilename))
		{
			string path = GetPath(strDirectory);
			string text = path + strFilename;
			string text2 = path + strFilename + "_Temp";
			string text3 = path + strFilename + "_Backup";
			if (!Directory.Exists(path))
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailedDoesNotExist, null);
			}
			bool flag = false;
			bool flag2 = false;
			if (File.Exists(text2))
			{
				flag2 = true;
				flag = true;
				try
				{
					StreamReader streamReader = File.OpenText(text2);
					string text4 = streamReader.ReadToEnd();
					streamReader.Close();
					if (string.IsNullOrEmpty(text4))
					{
						throw new Exception(text2 + " is empty!");
					}
					string text5 = Encryption.Decrypt(text4, string.Empty);
					if (string.IsNullOrEmpty(text5))
					{
						throw new Exception(text2 + " failed to decrypt!");
					}
					try
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
					}
					catch
					{
					}
					try
					{
						File.Move(text2, text);
					}
					catch
					{
					}
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, text5);
					return IORequestResultEnum.IORequestSuccessful;
				}
				catch
				{
					Debug.LogError("PlatformLoadFile: Tried to load " + text2 + " but something went wrong.. trying current and backup.");
				}
			}
			if (File.Exists(text))
			{
				flag = true;
				try
				{
					StreamReader streamReader2 = File.OpenText(text);
					string text6 = streamReader2.ReadToEnd();
					streamReader2.Close();
					if (string.IsNullOrEmpty(text6))
					{
						throw new Exception(text + " is empty!");
					}
					string text7 = Encryption.Decrypt(text6, string.Empty);
					if (string.IsNullOrEmpty(text7))
					{
						throw new Exception(text + " failed to decrypt!");
					}
					if (flag2)
					{
						try
						{
							if (File.Exists(text3))
							{
								File.Delete(text3);
							}
							File.Copy(text, text3);
						}
						catch
						{
							Debug.LogError("PlatformLoadFile: Failed to restore backup!");
						}
					}
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, text7);
					return IORequestResultEnum.IORequestSuccessful;
				}
				catch
				{
					Debug.LogError("PlatformLoadFile: Tried to load " + text + " but something went wrong.. trying backup.");
				}
			}
			if (File.Exists(text + "_Backup"))
			{
				flag = true;
				try
				{
					StreamReader streamReader3 = File.OpenText(text3);
					string text8 = streamReader3.ReadToEnd();
					streamReader3.Close();
					if (string.IsNullOrEmpty(text8))
					{
						throw new Exception(text3 + " is empty!");
					}
					string text9 = Encryption.Decrypt(text8, string.Empty);
					if (string.IsNullOrEmpty(text9))
					{
						throw new Exception(text3 + " failed to decrypt!");
					}
					try
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
					}
					catch
					{
					}
					try
					{
						File.Copy(text3, text);
					}
					catch
					{
					}
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, text9);
					return IORequestResultEnum.IORequestSuccessful;
				}
				catch
				{
					Debug.LogError("PlatformLoadFile: Tried to load " + text + " but something went wrong.. trying backup.");
				}
			}
			if (flag)
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
				return IORequestResultEnum.IORequestFailedInvalidRequest;
			}
			RespondToRequest(iHandle, IOResultEnum.IOFailedDoesNotExist, null);
			return IORequestResultEnum.IORequestSuccessful;
		}
		return IORequestResultEnum.IORequestFailedInvalidRequest;
	}

	protected override IORequestResultEnum PlatformDeleteFile(int iHandle, string strDirectory, string strFilename, object platformData)
	{
		if (!string.IsNullOrEmpty(strFilename))
		{
			string path = GetPath(strDirectory);
			if (!Directory.Exists(path) || !File.Exists(path + strFilename))
			{
				RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
			}
			else
			{
				try
				{
					File.SetAttributes(path + strFilename, FileAttributes.Normal);
					File.Delete(path + strFilename);
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
				}
				catch
				{
					RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
					return IORequestResultEnum.IORequestSuccessful;
				}
			}
			return IORequestResultEnum.IORequestSuccessful;
		}
		return IORequestResultEnum.IORequestFailedInvalidRequest;
	}

	protected override IORequestResultEnum PlatformDeleteDirectory(int iHandle, string strDirectory, object platformData)
	{
		if (!string.IsNullOrEmpty(strDirectory))
		{
			string path = GetPath(strDirectory);
			if (!Directory.Exists(path))
			{
				RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
			}
			else
			{
				try
				{
					Directory.Delete(path, recursive: true);
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
				}
				catch (Exception)
				{
					RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
					return IORequestResultEnum.IORequestSuccessful;
				}
			}
			return IORequestResultEnum.IORequestSuccessful;
		}
		return IORequestResultEnum.IORequestFailedInvalidRequest;
	}

	protected override IORequestResultEnum PlatformDoesFileExists(int iHandle, string strDirectory, string strFilename, object platformData)
	{
		if (!string.IsNullOrEmpty(strFilename))
		{
			string path = GetPath(strDirectory);
			try
			{
				if (!Directory.Exists(path) || !File.Exists(path + strFilename))
				{
					RespondToRequest(iHandle, IOResultEnum.IOFailedDoesNotExist, null);
				}
				else
				{
					RespondToRequest(iHandle, IOResultEnum.IOSuccessful, null);
				}
			}
			catch
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
			}
			return IORequestResultEnum.IORequestSuccessful;
		}
		return IORequestResultEnum.IORequestFailedInvalidRequest;
	}

	protected override IORequestResultEnum PlatformGetFileList(int iHandle, string strDirectory, string strFilter, object platformData)
	{
		string path = GetPath(strDirectory);
		if (string.IsNullOrEmpty(strFilter))
		{
			strFilter = "*.*";
		}
		try
		{
			if (!Directory.Exists(path))
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailedDoesNotExist, null);
			}
			else
			{
				string[] files = Directory.GetFiles(path, strFilter, SearchOption.TopDirectoryOnly);
				List<string> list = new List<string>(files);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					list[num] = list[num].Replace(path, string.Empty);
				}
				RespondToRequest(iHandle, IOResultEnum.IOSuccessful, list);
			}
		}
		catch
		{
			RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
		}
		return IORequestResultEnum.IORequestSuccessful;
	}

	protected override IORequestResultEnum PlatformGetDirectoryList(int iHandle, string strDirectory, string strFilter, object platformData)
	{
		string path = GetPath(strDirectory);
		if (string.IsNullOrEmpty(strFilter))
		{
			strFilter = "*";
		}
		try
		{
			if (!Directory.Exists(path))
			{
				RespondToRequest(iHandle, IOResultEnum.IOFailedDoesNotExist, null);
			}
			else
			{
				string[] directories = Directory.GetDirectories(path, strFilter, SearchOption.TopDirectoryOnly);
				List<string> list = new List<string>(directories);
				for (int num = list.Count - 1; num >= 0; num--)
				{
					list[num] = list[num].Replace(path, string.Empty);
				}
				RespondToRequest(iHandle, IOResultEnum.IOSuccessful, list);
			}
		}
		catch
		{
			RespondToRequest(iHandle, IOResultEnum.IOFailed, null);
		}
		return IORequestResultEnum.IORequestSuccessful;
	}

	public override string GetSanitisedString(string strSanitizerThis)
	{
		return strSanitizerThis.Replace("_", string.Empty);
	}

	public override bool IsPlatformReady()
	{
		return true;
	}
}
