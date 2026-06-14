using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class PlatformIO : MonoBehaviour
{
	public enum IOResultEnum
	{
		IOSuccessful,
		IOFailed,
		IOFailedCorrupt,
		IOFailedDoesNotExist,
		IOFailedNotEnoughRoom,
		IOFailedTimeOut,
		IOFailedInvalidRequest,
		IOFailedBusy,
		IOCanceled
	}

	public enum IORequestResultEnum
	{
		IORequestSuccessful,
		IORequestFailedInvalidRequest,
		IORequestFailedBusy
	}

	public delegate void RequestResult(IOResultEnum eResult, int iHandle);

	public class IORequest
	{
		public enum IORequestType
		{
			INVALID,
			LoadFile,
			SaveFile,
			DeleteFile,
			DeleteDirectory,
			GetFileList,
			GetDirectoryList,
			FileExistCheck,
			InvalidRequest
		}

		public enum IORequestPriority
		{
			Now,
			Next,
			High,
			Normal,
			Low
		}

		public enum IORequestStatus
		{
			Waiting,
			Processing,
			Canceled,
			TimedOut,
			Complete,
			Remove
		}

		public IORequestType m_Type;

		public IORequestStatus m_Status;

		public IOResultEnum m_Result = IOResultEnum.IOFailedInvalidRequest;

		public object m_Data;

		public object m_PlatformData;

		public string m_strOwner = string.Empty;

		public string m_strFileName = string.Empty;

		public float m_fTimeout = 4f;

		public RequestResult m_Callback;

		public int m_Handle = -1;

		public IORequestPriority m_Priority = IORequestPriority.Low;

		public bool m_bUserInputRequired;
	}

	private static PlatformIO m_TheInstance;

	private int m_iRequestHandleID = 1;

	protected const int m_iInvalidHandle = -1;

	protected char m_SeperationChar = '/';

	protected string m_strSavePath = string.Empty;

	private int m_SaveCount;

	private int m_NumSavesDoneSinceLastPoll;

	private object m_ChachedData;

	private List<IORequest> m_IORequests = new List<IORequest>();

	protected virtual void Awake()
	{
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
			Object.DontDestroyOnLoad(base.transform.gameObject);
		}
		else
		{
			Object.Destroy(this);
		}
		m_SeperationChar = Path.DirectorySeparatorChar;
	}

	public static PlatformIO GetInstance()
	{
		return m_TheInstance;
	}

	protected virtual void Update()
	{
		UpdateIORequests();
	}

	public virtual string GetPath(string stringDirectory)
	{
		if (string.IsNullOrEmpty(stringDirectory))
		{
			return m_strSavePath;
		}
		return m_strSavePath + stringDirectory + m_SeperationChar;
	}

	public char GetSeperationCharacter()
	{
		return m_SeperationChar;
	}

	public object GetCache()
	{
		return m_ChachedData;
	}

	protected void SetCache(object cacheObject)
	{
		m_ChachedData = cacheObject;
	}

	public bool IsIOBusy(bool bSaveOnly)
	{
		if (bSaveOnly)
		{
			int numSavesDoneSinceLastPoll = m_NumSavesDoneSinceLastPoll;
			m_NumSavesDoneSinceLastPoll = 0;
			return m_SaveCount > 0 || numSavesDoneSinceLastPoll > 0;
		}
		return m_IORequests.Count > 0;
	}

	private int AddIORequest(IORequest ioRequest)
	{
		if (ioRequest.m_Priority == IORequest.IORequestPriority.Low)
		{
			m_IORequests.Add(ioRequest);
		}
		else
		{
			int count = m_IORequests.Count;
			int num = (int)(ioRequest.m_Priority + 1);
			int num2 = 0;
			for (num2 = 0; num2 < count; num2++)
			{
				if (m_IORequests[num2].m_Priority == (IORequest.IORequestPriority)num)
				{
					m_IORequests.Insert(num2, ioRequest);
					break;
				}
			}
			if (num2 == count)
			{
				m_IORequests.Add(ioRequest);
			}
			if (ioRequest.m_Priority == IORequest.IORequestPriority.Next)
			{
				ioRequest.m_Priority = IORequest.IORequestPriority.High;
			}
		}
		if (ioRequest.m_Type == IORequest.IORequestType.SaveFile)
		{
			m_SaveCount++;
		}
		ioRequest.m_Handle = m_iRequestHandleID++;
		return ioRequest.m_Handle;
	}

	protected void RespondToRequest(int iHandle, IOResultEnum eResult, object objData)
	{
		int num = FindRequestIndexByHandle(iHandle);
		if (num >= 0)
		{
			switch (m_IORequests[num].m_Status)
			{
			case IORequest.IORequestStatus.Canceled:
				m_IORequests[num].m_Status = IORequest.IORequestStatus.Remove;
				break;
			case IORequest.IORequestStatus.Complete:
				break;
			case IORequest.IORequestStatus.Waiting:
			case IORequest.IORequestStatus.Processing:
				m_IORequests[num].m_Data = objData;
				m_IORequests[num].m_Result = eResult;
				m_IORequests[num].m_Status = IORequest.IORequestStatus.Complete;
				break;
			case IORequest.IORequestStatus.TimedOut:
				m_IORequests[num].m_Data = null;
				m_IORequests[num].m_Result = IOResultEnum.IOFailedTimeOut;
				m_IORequests[num].m_Status = IORequest.IORequestStatus.Complete;
				break;
			case IORequest.IORequestStatus.Remove:
				break;
			}
		}
	}

	private int FindRequestIndexByHandle(int iHandle)
	{
		for (int num = m_IORequests.Count - 1; num >= 0; num--)
		{
			if (m_IORequests[num].m_Handle == iHandle)
			{
				return num;
			}
		}
		return -1;
	}

	public void MarkRequestAsNeedingNativeUserInput(int iHandle)
	{
		int num = FindRequestIndexByHandle(iHandle);
		if (num >= 0)
		{
			m_IORequests[num].m_bUserInputRequired = true;
		}
	}

	private int RequestIORequest(IORequest.IORequestType eType, IORequest.IORequestPriority ePriority, string strOwner, string filename, object objectData, float fTimeout, RequestResult callback, object platformData)
	{
		IORequest iORequest = new IORequest();
		iORequest.m_Priority = ePriority;
		iORequest.m_strOwner = strOwner;
		iORequest.m_strFileName = filename;
		iORequest.m_Data = objectData;
		iORequest.m_PlatformData = platformData;
		iORequest.m_fTimeout = fTimeout;
		iORequest.m_Callback = callback;
		iORequest.m_Type = eType;
		return AddIORequest(iORequest);
	}

	public void CancelAllIORequests()
	{
		for (int num = m_IORequests.Count - 1; num >= 0; num--)
		{
			if (m_IORequests[num].m_Type == IORequest.IORequestType.SaveFile)
			{
				m_SaveCount--;
				m_NumSavesDoneSinceLastPoll++;
			}
			CancelIORequest(m_IORequests[num].m_Handle);
		}
	}

	public void CancelIORequest(int iHandle)
	{
		int num = FindRequestIndexByHandle(iHandle);
		if (num <= -1)
		{
			return;
		}
		switch (m_IORequests[num].m_Status)
		{
		case IORequest.IORequestStatus.Waiting:
		case IORequest.IORequestStatus.Complete:
			m_IORequests[num].m_Status = IORequest.IORequestStatus.Remove;
			if (m_IORequests[num].m_Type == IORequest.IORequestType.SaveFile)
			{
				m_SaveCount--;
				m_NumSavesDoneSinceLastPoll++;
			}
			break;
		case IORequest.IORequestStatus.Remove:
			break;
		case IORequest.IORequestStatus.Canceled:
			break;
		case IORequest.IORequestStatus.Processing:
		case IORequest.IORequestStatus.TimedOut:
			m_IORequests[num].m_Status = IORequest.IORequestStatus.Canceled;
			break;
		}
	}

	public int RequestSaveFile(IORequest.IORequestPriority ePriority, string strOwner, string strFilename, object objectData, float fTimeout, RequestResult callback, object platformData)
	{
		if (string.IsNullOrEmpty(strFilename) || objectData == null || fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.SaveFile, ePriority, strOwner, strFilename, objectData, fTimeout, callback, platformData);
	}

	public int RequestLoadFile(IORequest.IORequestPriority ePriority, string strOwner, string strFilename, float fTimeout, RequestResult callback, object platformData)
	{
		if (string.IsNullOrEmpty(strFilename) || fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.LoadFile, ePriority, strOwner, strFilename, null, fTimeout, callback, platformData);
	}

	public int RequestDeleteFile(IORequest.IORequestPriority ePriority, string strOwner, string strFilename, float fTimeout, RequestResult callback, object platformData)
	{
		if (fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.DeleteFile, ePriority, strOwner, strFilename, null, fTimeout, callback, platformData);
	}

	public int RequestDeleteDirectory(IORequest.IORequestPriority ePriority, string strDirectory, float fTimeout, RequestResult callback, object platformData)
	{
		if (fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now || string.IsNullOrEmpty(strDirectory))
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.DeleteDirectory, ePriority, strDirectory, null, null, fTimeout, callback, platformData);
	}

	public int RequestFileList(IORequest.IORequestPriority ePriority, string strOwner, string strFilter, float fTimeout, RequestResult callback, object platformData)
	{
		if (fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.GetFileList, ePriority, strOwner, strFilter, null, fTimeout, callback, platformData);
	}

	public int RequestDirectoryList(IORequest.IORequestPriority ePriority, string strOwner, string strFilter, float fTimeout, RequestResult callback, object platformData)
	{
		if (fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.GetDirectoryList, ePriority, strOwner, strFilter, null, fTimeout, callback, platformData);
	}

	public int RequestFileExistsCheck(IORequest.IORequestPriority ePriority, string strOwner, string strFilename, float fTimeout, RequestResult callback, object platformData)
	{
		if (string.IsNullOrEmpty(strFilename) || fTimeout <= 0f || ePriority == IORequest.IORequestPriority.Now)
		{
			return RequestIORequest(IORequest.IORequestType.InvalidRequest, ePriority, string.Empty, string.Empty, null, 0f, callback, platformData);
		}
		return RequestIORequest(IORequest.IORequestType.FileExistCheck, ePriority, strOwner, strFilename, null, fTimeout, callback, platformData);
	}

	private void UpdateIORequests()
	{
		bool flag = true;
		bool flag2 = true;
		while (flag && m_IORequests.Count > 0 && flag2)
		{
			int count = m_IORequests.Count;
			flag2 = false;
			for (int i = 0; i < count; i++)
			{
				if (!flag)
				{
					break;
				}
				IORequest iORequest = m_IORequests[i];
				switch (iORequest.m_Status)
				{
				case IORequest.IORequestStatus.Processing:
					if (!iORequest.m_bUserInputRequired && iORequest.m_fTimeout < Time.realtimeSinceStartup)
					{
						iORequest.m_Status = IORequest.IORequestStatus.TimedOut;
						SetCache(null);
						if (iORequest.m_Callback != null)
						{
							iORequest.m_Callback(IOResultEnum.IOFailedTimeOut, iORequest.m_Handle);
						}
					}
					flag = false;
					break;
				case IORequest.IORequestStatus.Canceled:
				case IORequest.IORequestStatus.TimedOut:
					flag = false;
					break;
				case IORequest.IORequestStatus.Complete:
					iORequest.m_Status = IORequest.IORequestStatus.Remove;
					if (iORequest.m_Callback != null)
					{
						SetCache(iORequest.m_Data);
						iORequest.m_Callback(iORequest.m_Result, iORequest.m_Handle);
					}
					if (iORequest.m_Type == IORequest.IORequestType.SaveFile)
					{
						m_SaveCount--;
						m_NumSavesDoneSinceLastPoll++;
					}
					count = m_IORequests.Count;
					i = -1;
					break;
				case IORequest.IORequestStatus.Waiting:
				{
					flag2 = true;
					IORequestResultEnum iORequestResultEnum = IORequestResultEnum.IORequestFailedInvalidRequest;
					switch (iORequest.m_Type)
					{
					case IORequest.IORequestType.INVALID:
					case IORequest.IORequestType.InvalidRequest:
						iORequestResultEnum = IORequestResultEnum.IORequestFailedInvalidRequest;
						break;
					case IORequest.IORequestType.LoadFile:
						iORequestResultEnum = PlatformLoadFile(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.DeleteFile:
						iORequestResultEnum = PlatformDeleteFile(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.DeleteDirectory:
						iORequestResultEnum = PlatformDeleteDirectory(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.FileExistCheck:
						iORequestResultEnum = PlatformDoesFileExists(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.GetFileList:
						iORequestResultEnum = PlatformGetFileList(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.GetDirectoryList:
						iORequestResultEnum = PlatformGetDirectoryList(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_PlatformData);
						break;
					case IORequest.IORequestType.SaveFile:
						iORequestResultEnum = PlatformSaveFile(iORequest.m_Handle, iORequest.m_strOwner, iORequest.m_strFileName, iORequest.m_Data, iORequest.m_PlatformData);
						break;
					}
					switch (iORequestResultEnum)
					{
					case IORequestResultEnum.IORequestFailedBusy:
						flag = false;
						break;
					case IORequestResultEnum.IORequestFailedInvalidRequest:
						iORequest.m_Status = IORequest.IORequestStatus.Complete;
						iORequest.m_Data = null;
						iORequest.m_Result = IOResultEnum.IOFailedInvalidRequest;
						i = -1;
						break;
					case IORequestResultEnum.IORequestSuccessful:
						if (iORequest.m_Status == IORequest.IORequestStatus.Waiting)
						{
							iORequest.m_Status = IORequest.IORequestStatus.Processing;
							if (iORequest.m_fTimeout > 0f)
							{
								iORequest.m_fTimeout += Time.realtimeSinceStartup;
							}
							iORequest.m_Priority = IORequest.IORequestPriority.Now;
							m_IORequests.RemoveAt(i);
							m_IORequests.Insert(0, iORequest);
							flag = false;
						}
						else
						{
							count = m_IORequests.Count;
							i = -1;
						}
						break;
					default:
						iORequest.m_Status = IORequest.IORequestStatus.Complete;
						iORequest.m_Data = null;
						iORequest.m_Result = IOResultEnum.IOFailedInvalidRequest;
						flag = false;
						break;
					}
					break;
				}
				}
			}
		}
		if (m_IORequests.Count > 0)
		{
			m_IORequests.RemoveAll((IORequest x) => x.m_Status == IORequest.IORequestStatus.Remove);
		}
	}

	protected abstract IORequestResultEnum PlatformSaveFile(int iHandle, string strDirectory, string strFilename, object objData, object platformData);

	protected abstract IORequestResultEnum PlatformLoadFile(int iHandle, string strDirectory, string strFilename, object platformData);

	protected abstract IORequestResultEnum PlatformDeleteFile(int iHandle, string strDirectory, string strFilename, object platformData);

	protected abstract IORequestResultEnum PlatformDeleteDirectory(int iHandle, string strDirectory, object platformData);

	protected abstract IORequestResultEnum PlatformDoesFileExists(int iHandle, string strDirectory, string strFilename, object platformData);

	protected abstract IORequestResultEnum PlatformGetFileList(int iHandle, string strDirectory, string strFilter, object platformData);

	protected abstract IORequestResultEnum PlatformGetDirectoryList(int iHandle, string strDirectory, string strFilter, object platformData);

	public abstract string GetSanitisedString(string strSanitizerThis);

	public abstract bool IsPlatformReady();
}
