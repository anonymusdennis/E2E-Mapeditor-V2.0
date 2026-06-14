using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageDebug : MonoBehaviour
{
	public T17Text m_DirectoryList;

	public T17Text m_FilesInTest;

	public T17Button m_Does1ExistButton;

	public T17Button m_Does2ExistButton;

	public T17Button m_Does3ExistButton;

	private PlatformIO m_PlatformIO;

	private int m_ExistHandle1 = -1;

	private int m_ExistHandle2 = -1;

	private int m_ExistHandle3 = -1;

	private void Start()
	{
		m_PlatformIO = PlatformIO.GetInstance();
	}

	private void Update()
	{
	}

	public void LoadAllDirectories()
	{
		m_PlatformIO.RequestDirectoryList(PlatformIO.IORequest.IORequestPriority.Next, string.Empty, "*", 4f, GetPrisonDirectoryResult, null);
	}

	private void GetPrisonDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
			if (m_DirectoryList != null)
			{
				m_DirectoryList.text = "GetPrisonDirectoryResult(" + eResult.ToString() + ")";
			}
			return;
		}
		List<string> list = m_PlatformIO.GetCache() as List<string>;
		string text = "Directories Found:\n";
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				text = text + list[i] + "\n";
			}
		}
		else
		{
			text += "None\n";
		}
		if (m_DirectoryList != null)
		{
			m_DirectoryList.text = text;
		}
	}

	public void LoadFileFromTestDir()
	{
		m_PlatformIO.RequestFileList(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "*", 4f, GetScanPrisonResult, null);
	}

	private void GetScanPrisonResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (eResult != 0)
		{
			if (m_FilesInTest != null)
			{
				m_FilesInTest.text = "GetScanPrisonResult(" + eResult.ToString() + ")";
			}
			return;
		}
		List<string> list = m_PlatformIO.GetCache() as List<string>;
		string text = "Files In TestDir Found:\n";
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				text = text + list[i] + "\n";
			}
		}
		else
		{
			text += "None\n";
		}
		if (m_FilesInTest != null)
		{
			m_FilesInTest.text = text;
		}
	}

	public void SaveFile(int index)
	{
		switch (index)
		{
		case 0:
			m_PlatformIO.RequestSaveFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 1", "I am savefile 1", 4f, SaveDirectoryResult, null);
			break;
		case 1:
			m_PlatformIO.RequestSaveFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 2", "I am savefile 2", 4f, SaveDirectoryResult, null);
			break;
		case 2:
			m_PlatformIO.RequestSaveFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 3", "I am savefile 3", 4f, SaveDirectoryResult, null);
			break;
		}
	}

	private void SaveDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
	}

	public void LoadFile(int index)
	{
		switch (index)
		{
		case 0:
			m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 1", 4f, RequestDirectoryFileResult, null);
			break;
		case 1:
			m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 2", 4f, RequestDirectoryFileResult, null);
			break;
		case 2:
			m_PlatformIO.RequestLoadFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 3", 4f, RequestDirectoryFileResult, null);
			break;
		}
	}

	private void RequestDirectoryFileResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
	}

	public void DeleteFile(int index)
	{
		switch (index)
		{
		case 0:
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 1", 4f, DeleteDirectoryResult, null);
			break;
		case 1:
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 2", 4f, DeleteDirectoryResult, null);
			break;
		case 2:
			m_PlatformIO.RequestDeleteFile(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 3", 4f, DeleteDirectoryResult, null);
			break;
		}
	}

	private void DeleteDirectoryResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
	}

	public void DoesFileExist(int index)
	{
		switch (index)
		{
		case 0:
			m_ExistHandle1 = m_PlatformIO.RequestFileExistsCheck(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 1", 4f, FileExistsResult, null);
			break;
		case 1:
			m_ExistHandle2 = m_PlatformIO.RequestFileExistsCheck(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 2", 4f, FileExistsResult, null);
			break;
		case 2:
			m_ExistHandle3 = m_PlatformIO.RequestFileExistsCheck(PlatformIO.IORequest.IORequestPriority.Next, "TestDir", "SaveFile 3", 4f, FileExistsResult, null);
			break;
		}
	}

	private void FileExistsResult(PlatformIO.IOResultEnum eResult, int iHandle)
	{
		if (iHandle == m_ExistHandle1)
		{
			ColorBlock colors = m_Does1ExistButton.colors;
			if (eResult == PlatformIO.IOResultEnum.IOSuccessful)
			{
				colors.normalColor = Color.green * 0.7f;
				colors.highlightedColor = Color.green;
			}
			else
			{
				colors.normalColor = Color.red * 0.7f;
				colors.highlightedColor = Color.red;
			}
			m_Does1ExistButton.colors = colors;
		}
		else if (iHandle == m_ExistHandle2)
		{
			ColorBlock colors2 = m_Does2ExistButton.colors;
			if (eResult == PlatformIO.IOResultEnum.IOSuccessful)
			{
				colors2.normalColor = Color.green * 0.7f;
				colors2.highlightedColor = Color.green;
			}
			else
			{
				colors2.normalColor = Color.red * 0.7f;
				colors2.highlightedColor = Color.red;
			}
			m_Does2ExistButton.colors = colors2;
		}
		else if (iHandle == m_ExistHandle3)
		{
			ColorBlock colors3 = m_Does3ExistButton.colors;
			if (eResult == PlatformIO.IOResultEnum.IOSuccessful)
			{
				colors3.normalColor = Color.green * 0.7f;
				colors3.highlightedColor = Color.green;
			}
			else
			{
				colors3.normalColor = Color.red * 0.7f;
				colors3.highlightedColor = Color.red;
			}
			m_Does3ExistButton.colors = colors3;
		}
	}
}
