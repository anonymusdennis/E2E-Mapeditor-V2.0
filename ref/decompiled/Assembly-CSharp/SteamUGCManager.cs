using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class SteamUGCManager : MonoBehaviour
{
	private class UGCItem
	{
		public PublishedFileId_t m_publishedID;

		public EItemState m_ItemState;

		public Platform.UGCType m_UGCType;

		public UGCItem(PublishedFileId_t id, EItemState state)
		{
			m_publishedID = id;
			m_ItemState = state;
			m_UGCType = Platform.UGCType.eNone;
		}
	}

	private static CallResult<CreateItemResult_t> m_CreateItemResult = new CallResult<CreateItemResult_t>();

	private static CallResult<SubmitItemUpdateResult_t> m_SubmitUpdateItemResult = new CallResult<SubmitItemUpdateResult_t>();

	private static CallResult<SteamUGCQueryCompleted_t> m_MetadataQueryResultCallback = new CallResult<SteamUGCQueryCompleted_t>();

	private static CallResult<SteamUGCQueryCompleted_t> m_PublishedContentQueryResultCallback = new CallResult<SteamUGCQueryCompleted_t>();

	private static CallResult<RemoteStorageUnsubscribePublishedFileResult_t> m_OnUnsubscribedCallback = new CallResult<RemoteStorageUnsubscribePublishedFileResult_t>();

	private static Callback<DownloadItemResult_t> m_DownloadItemCallback = null;

	private static Callback<ItemInstalled_t> m_InstalledItemCallback = null;

	private List<UGCItem> m_UGCList = new List<UGCItem>();

	private Queue<Platform.UGCUploadData> m_PendingUploads = new Queue<Platform.UGCUploadData>();

	private bool m_bWaitingForMetaData;

	private bool m_bUploadingUGCItem;

	private UGCUpdateHandle_t m_CurrentUpdateHandle = UGCUpdateHandle_t.Invalid;

	private int m_UploadIDIndex;

	private int m_CurrentUploadID = -1;

	private Platform.UGCType m_PublishQueryFilter;

	private bool m_bUnSubscribing;

	private float m_fContentCheckTimer;

	private const float m_fContentTimerInterval = 5f;

	private event Platform.OnUserGeneratedContentUpdated m_OnUGCChanged;

	private event Platform.UserGeneratedContentUploadCallback m_UGCUploadCallback;

	private event Platform.OnPublishedItemsRecieved m_PublishedItemsRecievedCallback;

	private void Start()
	{
		m_DownloadItemCallback = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
		m_InstalledItemCallback = Callback<ItemInstalled_t>.Create(OnItemInstalled);
		QueryPublishedContent();
	}

	private bool QueryPublishedContent()
	{
		CSteamID steamID = SteamUser.GetSteamID();
		if (steamID.IsValid() && !m_PublishedContentQueryResultCallback.IsActive())
		{
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(steamID.GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_All, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, SteamPlatform.GetAppId(), SteamPlatform.GetAppId(), 1u);
			SteamUGC.SetReturnMetadata(handle, bReturnMetadata: true);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
			m_PublishedContentQueryResultCallback.Set(hAPICall, OnPublishedQueryCallback);
			return true;
		}
		return false;
	}

	private void Update()
	{
		int count = m_PendingUploads.Count;
		if (count > 0 && !m_bUploadingUGCItem && m_CurrentUpdateHandle == UGCUpdateHandle_t.Invalid)
		{
			InternalUploadUGCItem(m_PendingUploads.Dequeue());
		}
		if (m_CurrentUpdateHandle != UGCUpdateHandle_t.Invalid)
		{
			Platform.UGCUploadState uGCUploadState = new Platform.UGCUploadState();
			uGCUploadState.m_UploadID = m_CurrentUploadID;
			EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(m_CurrentUpdateHandle, out uGCUploadState.m_ulBytesProcessed, out uGCUploadState.m_ulBytesTotal);
			if (this.m_UGCUploadCallback != null)
			{
				this.m_UGCUploadCallback(uGCUploadState);
			}
		}
		if (m_bWaitingForMetaData || m_bUnSubscribing || !(Time.realtimeSinceStartup >= m_fContentCheckTimer))
		{
			return;
		}
		m_fContentCheckTimer = Time.realtimeSinceStartup + 5f;
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		if (numSubscribedItems != 0)
		{
			List<PublishedFileId_t> list = new List<PublishedFileId_t>();
			PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
			SteamUGC.GetSubscribedItems(array, numSubscribedItems);
			bool flag = false;
			for (int i = 0; i < numSubscribedItems; i++)
			{
				bool flag2 = false;
				for (int j = 0; j < m_UGCList.Count; j++)
				{
					if (m_UGCList[j].m_publishedID == array[i])
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					EItemState itemState = (EItemState)SteamUGC.GetItemState(array[i]);
					m_UGCList.Add(new UGCItem(array[i], itemState));
					flag = true;
					list.Add(array[i]);
				}
			}
			for (int num = m_UGCList.Count - 1; num >= 0; num--)
			{
				bool flag3 = false;
				for (int k = 0; k < numSubscribedItems; k++)
				{
					if (m_UGCList[num].m_publishedID == array[k])
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					m_UGCList.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				if (list.Count > 0)
				{
					UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(list.ToArray(), (uint)list.Count);
					SteamUGC.SetReturnMetadata(handle, bReturnMetadata: true);
					SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
					m_MetadataQueryResultCallback.Set(hAPICall, OnUGCMetadataQuery);
					m_bWaitingForMetaData = true;
				}
				if (this.m_OnUGCChanged != null)
				{
					this.m_OnUGCChanged();
				}
			}
		}
		else if (m_UGCList.Count != 0)
		{
			m_UGCList.Clear();
		}
	}

	private void UploadWorkshopItem(Platform.UGCUploadData uploadData, PublishedFileId_t publishID)
	{
		m_CurrentUpdateHandle = SteamUGC.StartItemUpdate(SteamPlatform.GetAppId(), publishID);
		SteamUGC.SetItemTitle(m_CurrentUpdateHandle, uploadData.m_strName);
		SteamUGC.SetItemDescription(m_CurrentUpdateHandle, uploadData.m_strDescription);
		SteamUGC.SetItemMetadata(m_CurrentUpdateHandle, uploadData.m_ugcType.ToString());
		SteamUGC.SetItemVisibility(m_CurrentUpdateHandle, (ERemoteStoragePublishedFileVisibility)uploadData.m_eVisibility);
		SteamUGC.SetItemContent(m_CurrentUpdateHandle, uploadData.m_strContentPath);
		SteamUGC.SetItemPreview(m_CurrentUpdateHandle, uploadData.m_strPreviewPath);
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(m_CurrentUpdateHandle, null);
		m_SubmitUpdateItemResult.Set(hAPICall, delegate(SubmitItemUpdateResult_t pSubmitCallback, bool submitFailure)
		{
			m_bUploadingUGCItem = false;
			Platform.UGCUploadState uGCUploadState = new Platform.UGCUploadState
			{
				m_bError = false,
				m_bCompleted = true,
				m_UploadID = uploadData.m_UploadID
			};
			SteamUGC.GetItemUpdateProgress(m_CurrentUpdateHandle, out uGCUploadState.m_ulBytesProcessed, out uGCUploadState.m_ulBytesTotal);
			if (pSubmitCallback.m_eResult == EResult.k_EResultOK && !submitFailure)
			{
				Debug.Log("SteamUGCManager: Upload Complete!");
				Steamworks.SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + publishID);
				uGCUploadState.m_ulFinalPublishedID = publishID.m_PublishedFileId;
			}
			else
			{
				uGCUploadState.m_bError = true;
				Debug.LogError("SteamUGCManager: Failed to update Steam UGC Item! Result: " + pSubmitCallback.m_eResult);
			}
			if (this.m_UGCUploadCallback != null)
			{
				this.m_UGCUploadCallback(uGCUploadState);
			}
			m_CurrentUploadID = -1;
			m_CurrentUpdateHandle = UGCUpdateHandle_t.Invalid;
		});
	}

	private void InternalUploadUGCItem(Platform.UGCUploadData uploadData)
	{
		m_CurrentUploadID = uploadData.m_UploadID;
		if (uploadData.m_PublishID != 0)
		{
			UploadWorkshopItem(uploadData, new PublishedFileId_t(uploadData.m_PublishID));
			return;
		}
		SteamAPICall_t hAPICall = SteamUGC.CreateItem(SteamPlatform.GetAppId(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
		m_CreateItemResult.Set(hAPICall, delegate(CreateItemResult_t pCallback, bool bFailure)
		{
			if (bFailure || pCallback.m_eResult != EResult.k_EResultOK)
			{
				int currentUploadID = m_CurrentUploadID;
				Debug.LogError("SteamUGCManager: Failed to create UGC item for upload! Result: " + pCallback.m_eResult);
				m_bUploadingUGCItem = false;
				m_CurrentUploadID = -1;
				if (this.m_UGCUploadCallback != null)
				{
					Platform.UGCUploadState uploadData2 = new Platform.UGCUploadState
					{
						m_UploadID = currentUploadID,
						m_bCompleted = true,
						m_bError = true
					};
					this.m_UGCUploadCallback(uploadData2);
				}
			}
			else
			{
				if (pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
				{
					Debug.Log("SteamUGCManager: Need to accept workshop agreement!");
					Steamworks.SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + pCallback.m_nPublishedFileId);
				}
				if (pCallback.m_eResult == EResult.k_EResultOK)
				{
					UploadWorkshopItem(uploadData, pCallback.m_nPublishedFileId);
				}
			}
		});
	}

	public void RegisterOnUGCChange(Platform.OnUserGeneratedContentUpdated ugcEvent)
	{
		if (ugcEvent != null)
		{
			m_OnUGCChanged += ugcEvent;
		}
	}

	public void UnRegisterOnUGCChange(Platform.OnUserGeneratedContentUpdated ugcEvent)
	{
		if (ugcEvent != null)
		{
			m_OnUGCChanged -= ugcEvent;
		}
	}

	public void EnumerateUGCItems(ref List<Platform.UGCItem> ugcList, Platform.UGCType filter = Platform.UGCType.eNone)
	{
		for (int i = 0; i < m_UGCList.Count; i++)
		{
			UGCItem uGCItem = m_UGCList[i];
			if (uGCItem != null && (uGCItem.m_ItemState & EItemState.k_EItemStateInstalled) == EItemState.k_EItemStateInstalled && (uGCItem.m_ItemState & EItemState.k_EItemStateSubscribed) == EItemState.k_EItemStateSubscribed && (filter == Platform.UGCType.eNone || uGCItem.m_UGCType == filter))
			{
				ulong punSizeOnDisk = 0uL;
				string pchFolder = string.Empty;
				uint punTimeStamp = 0u;
				if (SteamUGC.GetItemInstallInfo(uGCItem.m_publishedID, out punSizeOnDisk, out pchFolder, 1024u, out punTimeStamp))
				{
					Platform.UGCItem uGCItem2 = new Platform.UGCItem();
					uGCItem2.m_ID = uGCItem.m_publishedID.m_PublishedFileId;
					uGCItem2.m_Type = uGCItem.m_UGCType;
					uGCItem2.m_AbsolutePath = pchFolder;
					ugcList.Add(uGCItem2);
				}
			}
		}
	}

	public bool EnumeratePublishedUGCItems(Platform.UGCType filter = Platform.UGCType.eNone, Platform.OnPublishedItemsRecieved callback = null)
	{
		m_PublishQueryFilter = filter;
		if (callback != null)
		{
			m_PublishedItemsRecievedCallback -= callback;
			m_PublishedItemsRecievedCallback += callback;
		}
		return QueryPublishedContent();
	}

	public int UploadUGCItem(Platform.UGCUploadData uploadData)
	{
		uploadData.m_UploadID = m_UploadIDIndex++;
		m_PendingUploads.Enqueue(uploadData);
		return uploadData.m_UploadID;
	}

	public void RegisterUGCUploadCallback(Platform.UserGeneratedContentUploadCallback callback)
	{
		if (callback != null)
		{
			m_UGCUploadCallback += callback;
		}
	}

	public void UnRegisterUGCUploadCallback(Platform.UserGeneratedContentUploadCallback callback)
	{
		if (callback != null)
		{
			m_UGCUploadCallback -= callback;
		}
	}

	public void RequestShowUGCTerms()
	{
		Steamworks.SteamFriends.ActivateGameOverlayToWebPage("steam://url/SSA/");
	}

	public void RequestShowUGCHomePage()
	{
		Steamworks.SteamFriends.ActivateGameOverlayToWebPage("steam://url/SteamWorkshopPage/" + SteamPlatform.GetAppId().ToString());
	}

	private void OnItemDownloaded(DownloadItemResult_t result)
	{
		if (!(result.m_unAppID == SteamPlatform.GetAppId()))
		{
			return;
		}
		if (result.m_eResult == EResult.k_EResultOK)
		{
			UGCItem uGCItem = m_UGCList.Find((UGCItem x) => x.m_publishedID == result.m_nPublishedFileId);
			if (uGCItem != null)
			{
				uGCItem.m_ItemState = (EItemState)SteamUGC.GetItemState(uGCItem.m_publishedID);
				if (this.m_OnUGCChanged != null)
				{
					this.m_OnUGCChanged();
				}
			}
			else
			{
				Debug.LogError("SteamUGCManager: Downloaded a file that we don't know about!");
			}
		}
		else
		{
			Debug.LogError("SteamUGCManager: Failed to download UGC item! Result: " + result.m_eResult);
		}
	}

	private void OnItemInstalled(ItemInstalled_t result)
	{
		if (!(result.m_unAppID == SteamPlatform.GetAppId()))
		{
			return;
		}
		UGCItem uGCItem = m_UGCList.Find((UGCItem x) => x.m_publishedID == result.m_nPublishedFileId);
		if (uGCItem != null)
		{
			uGCItem.m_ItemState = (EItemState)SteamUGC.GetItemState(uGCItem.m_publishedID);
			if (this.m_OnUGCChanged != null)
			{
				this.m_OnUGCChanged();
			}
		}
		else
		{
			Debug.LogError("SteamUGCManager: Downloaded a file that we don't know about!");
		}
	}

	private void OnUGCMetadataQuery(SteamUGCQueryCompleted_t result, bool bFailure)
	{
		m_bWaitingForMetaData = false;
		if (result.m_eResult == EResult.k_EResultOK && !bFailure)
		{
			for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
			{
				if (!SteamUGC.GetQueryUGCResult(result.m_handle, num, out var pDetails))
				{
					continue;
				}
				PublishedFileId_t publishID = pDetails.m_nPublishedFileId;
				try
				{
					UGCItem uGCItem = m_UGCList.Find((UGCItem x) => x.m_publishedID == publishID);
					SteamUGC.GetQueryUGCMetadata(result.m_handle, num, out var pchMetadata, 1024u);
					uGCItem.m_UGCType = (Platform.UGCType)Enum.Parse(typeof(Platform.UGCType), pchMetadata);
					if (this.m_OnUGCChanged != null)
					{
						this.m_OnUGCChanged();
					}
				}
				catch (ArgumentException)
				{
					Debug.LogError("SteamUGCManager: Failed to query metadata.");
				}
			}
		}
		else
		{
			Debug.LogError("SteamUGCManager: Failed to query UGC items! Result: " + result.m_eResult);
		}
		SteamUGC.ReleaseQueryUGCRequest(result.m_handle);
	}

	private void OnPublishedQueryCallback(SteamUGCQueryCompleted_t result, bool bFailure)
	{
		List<Platform.UGCItem> list = new List<Platform.UGCItem>();
		if (result.m_eResult == EResult.k_EResultOK && !bFailure)
		{
			for (uint num = 0u; num < result.m_unNumResultsReturned; num++)
			{
				if (!SteamUGC.GetQueryUGCResult(result.m_handle, num, out var pDetails))
				{
					continue;
				}
				PublishedFileId_t nPublishedFileId = pDetails.m_nPublishedFileId;
				try
				{
					Platform.UGCItem uGCItem = new Platform.UGCItem();
					uGCItem.m_ID = nPublishedFileId.m_PublishedFileId;
					uGCItem.m_AbsolutePath = pDetails.m_rgchTitle;
					SteamUGC.GetQueryUGCMetadata(result.m_handle, num, out var pchMetadata, 1024u);
					uGCItem.m_Type = (Platform.UGCType)Enum.Parse(typeof(Platform.UGCType), pchMetadata);
					if (m_PublishQueryFilter == Platform.UGCType.eNone || uGCItem.m_Type == m_PublishQueryFilter)
					{
						list.Add(uGCItem);
					}
				}
				catch (ArgumentException)
				{
					Debug.LogError("SteamUGCManager: Failed to query metadata.");
				}
			}
		}
		else
		{
			Debug.LogError("SteamUGCManager: Failed to query UGC items! Result: " + result.m_eResult);
		}
		SteamUGC.ReleaseQueryUGCRequest(result.m_handle);
		if (this.m_PublishedItemsRecievedCallback != null)
		{
			this.m_PublishedItemsRecievedCallback(list);
		}
		this.m_PublishedItemsRecievedCallback = null;
		m_PublishQueryFilter = Platform.UGCType.eNone;
	}

	public void UnsubscribedFromContent(Platform.UGCItem ugcToRemove)
	{
		if (ugcToRemove == null)
		{
			return;
		}
		for (int i = 0; i < m_UGCList.Count; i++)
		{
			if (m_UGCList[i].m_publishedID.m_PublishedFileId != ugcToRemove.m_ID)
			{
				continue;
			}
			m_UGCList.RemoveAt(i);
			if (this.m_OnUGCChanged != null)
			{
				this.m_OnUGCChanged();
			}
			m_bUnSubscribing = true;
			SteamAPICall_t hAPICall = SteamUGC.UnsubscribeItem(new PublishedFileId_t(ugcToRemove.m_ID));
			m_OnUnsubscribedCallback.Set(hAPICall, delegate(RemoteStorageUnsubscribePublishedFileResult_t result, bool failure)
			{
				for (int j = 0; j < m_UGCList.Count; j++)
				{
					if (m_UGCList[j].m_publishedID == result.m_nPublishedFileId)
					{
						m_UGCList.RemoveAt(j);
					}
				}
				m_bUnSubscribing = false;
				if (this.m_OnUGCChanged != null)
				{
					this.m_OnUGCChanged();
				}
				m_fContentCheckTimer = Time.realtimeSinceStartup + 5f;
			});
			break;
		}
	}
}
