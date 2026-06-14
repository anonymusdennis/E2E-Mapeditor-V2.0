using System.Collections.Generic;
using UnityEngine;

public class T17DialogBoxManager : T17MonoBehaviour
{
	public class DialogQueueElement
	{
		public T17DialogBox Dialog;

		public AllowedToShowEvent ShowEvent;
	}

	public delegate void AllowedToShowEvent();

	public static T17DialogBoxManager Instance;

	public GameObject m_DialogBoxPrefab;

	public Canvas m_GlobalDialogBoxCanvas;

	private List<T17DialogBox> m_DialogBoxPool;

	private const int POOL_SIZE = 15;

	private Dictionary<Gamer, List<T17DialogBox>> m_Bindings;

	private Dictionary<Gamer, bool> m_BindingStates;

	private List<T17DialogBox> m_GlobalDialogs;

	private bool m_bHasGlobalDiags;

	private List<DialogQueueElement> m_DialogBoxQueue = new List<DialogQueueElement>();

	protected override void Awake()
	{
		base.Awake();
		if (Instance != null)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		if (!(m_DialogBoxPrefab != null))
		{
			return;
		}
		m_DialogBoxPool = new List<T17DialogBox>();
		m_Bindings = new Dictionary<Gamer, List<T17DialogBox>>();
		m_BindingStates = new Dictionary<Gamer, bool>();
		m_GlobalDialogs = new List<T17DialogBox>();
		for (int i = 0; i < 15; i++)
		{
			GameObject gameObject = Object.Instantiate(m_DialogBoxPrefab);
			gameObject.transform.SetParent(m_GlobalDialogBoxCanvas.transform, worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			T17DialogBox component = gameObject.GetComponent<T17DialogBox>();
			if (component != null)
			{
				gameObject.SetActive(value: false);
				m_DialogBoxPool.Add(component);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		int count = m_DialogBoxPool.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_DialogBoxPool[i] != null)
			{
				GameObject gameobject = m_DialogBoxPool[i].GetGameobject();
				m_DialogBoxPool[i] = null;
				Object.Destroy(gameobject);
			}
		}
		m_DialogBoxPool = null;
		m_DialogBoxPrefab = null;
		m_GlobalDialogBoxCanvas = null;
		m_Bindings.Clear();
		if (Instance != null)
		{
			Instance = null;
		}
	}

	public static T17DialogBox GetDialog(bool forSingleUser, Player playerToShowFor = null, bool showOverPauseMenu = false)
	{
		if (Instance == null)
		{
			return null;
		}
		for (int i = 0; i < 15; i++)
		{
			T17DialogBox box = Instance.m_DialogBoxPool[i];
			if (box.IsActive || Instance.m_DialogBoxQueue.FindIndex((DialogQueueElement x) => x != null && x.Dialog == box) != -1)
			{
				continue;
			}
			GameObject gameObject = null;
			Gamer gamer = null;
			if (forSingleUser && playerToShowFor != null && InGameMenuFlow.Instance != null)
			{
				gamer = playerToShowFor.m_Gamer;
				InGameMenuFlow.PlayerIGMData data = null;
				InGameMenuFlow.Instance.GetCorrectIGMData(playerToShowFor.m_PlayerCameraManagerBindingID, out data);
				if (data != null)
				{
					gameObject = ((!showOverPauseMenu || !playerToShowFor.IsBrowsingPauseMenu) ? data.m_DialogBoxParentObject : Instance.m_GlobalDialogBoxCanvas.gameObject);
				}
				if (Instance.m_Bindings.ContainsKey(playerToShowFor.m_Gamer))
				{
					Instance.m_Bindings[playerToShowFor.m_Gamer].Add(box);
				}
				else
				{
					Instance.m_Bindings.Add(playerToShowFor.m_Gamer, new List<T17DialogBox> { box });
				}
				Instance.m_BindingStates[playerToShowFor.m_Gamer] = true;
			}
			else
			{
				gamer = ((!(playerToShowFor != null)) ? Gamer.GetPrimaryGamer() : playerToShowFor.m_Gamer);
				T17EventSystemsManager.Instance.DisableAllEventSystemsExceptFor(gamer);
				gameObject = Instance.m_GlobalDialogBoxCanvas.gameObject;
				Instance.m_GlobalDialogs.Add(box);
				Instance.m_bHasGlobalDiags = true;
			}
			gameObject.SetActive(value: true);
			box.transform.SetParent(gameObject.transform, worldPositionStays: false);
			box.transform.localScale = Vector3.one;
			box.transform.localPosition = Vector3.zero;
			box.ReparentToOnHide(Instance.m_GlobalDialogBoxCanvas.transform);
			box.SetGamerForEventSystem(gamer, T17EventSystemsManager.Instance.GetEventSystemForGamer(gamer));
			return box;
		}
		return null;
	}

	public static void RequestDialogShow(T17DialogBox dialog, AllowedToShowEvent callOnAllowed)
	{
		if (!(Instance == null))
		{
			DialogQueueElement dialogQueueElement = new DialogQueueElement();
			dialogQueueElement.Dialog = dialog;
			dialogQueueElement.ShowEvent = callOnAllowed;
			if (!Instance.m_DialogBoxQueue.Exists((DialogQueueElement q) => q.Dialog == dialog))
			{
				Instance.m_DialogBoxQueue.Add(dialogQueueElement);
			}
		}
	}

	public static bool HasGlobalDialogs()
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.m_bHasGlobalDiags;
	}

	public static bool HasDialogsForPlayer(Player player)
	{
		if ((player == null && player.m_Gamer == null) || player.m_Gamer.m_RewiredPlayer == null)
		{
			return false;
		}
		return HasDialogsForGamer(player.m_Gamer);
	}

	public static bool HasDialogsForGamer(Gamer gamer)
	{
		if (gamer == null)
		{
			return false;
		}
		if (Instance == null)
		{
			return false;
		}
		if (Instance.m_bHasGlobalDiags)
		{
			return true;
		}
		return Instance.m_BindingStates.ContainsKey(gamer) && Instance.m_BindingStates[gamer];
	}

	public static bool HasAnyOpenDialogs()
	{
		if (Instance == null)
		{
			return false;
		}
		for (int i = 0; i < 15; i++)
		{
			if (Instance.m_DialogBoxPool[i].IsActive)
			{
				return true;
			}
		}
		return false;
	}

	private void EnsureFocus()
	{
		if (Instance.m_bHasGlobalDiags)
		{
			if (m_GlobalDialogs != null && m_GlobalDialogs.Count > 0 && T17RewiredStandaloneInputModule.IsControllerDrivingInput(Gamer.GetPrimaryGamer()))
			{
				T17DialogBox t17DialogBox = m_GlobalDialogs[m_GlobalDialogs.Count - 1];
				if (!t17DialogBox.HasFocus())
				{
					t17DialogBox.Focus();
				}
			}
			return;
		}
		foreach (Gamer key in Instance.m_Bindings.Keys)
		{
			if (!T17RewiredStandaloneInputModule.IsControllerDrivingInput(key))
			{
				continue;
			}
			List<T17DialogBox> list = Instance.m_Bindings[key];
			if (list != null && list.Count > 0)
			{
				T17DialogBox t17DialogBox2 = list[list.Count - 1];
				if (!t17DialogBox2.HasFocus())
				{
					t17DialogBox2.Focus();
				}
			}
		}
	}

	public void Update()
	{
		switch (GlobalStart.GetInstance().CurrentGlobalStartMode)
		{
		case GlobalStart.GLOBALSTART_MODE.SHOW_BOOT:
		case GlobalStart.GLOBALSTART_MODE.CHECK_INVITES:
		case GlobalStart.GLOBALSTART_MODE.PROCESSING_INVITE:
		case GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.IN_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS:
		case GlobalStart.GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR:
			if (m_DialogBoxQueue.Count > 0)
			{
				DialogQueueElement dialogQueueElement = m_DialogBoxQueue[0];
				m_DialogBoxQueue.RemoveAt(0);
				dialogQueueElement.ShowEvent();
			}
			break;
		}
		EnsureFocus();
	}

	public static void ReleaseAll()
	{
		if (Instance == null)
		{
			return;
		}
		for (int i = 0; i < 15; i++)
		{
			if (Instance.m_DialogBoxPool[i].IsActive)
			{
				Instance.m_DialogBoxPool[i].Hide();
			}
		}
		Instance.m_DialogBoxQueue.Clear();
		Instance.m_bHasGlobalDiags = false;
		Instance.m_GlobalDialogs.Clear();
		Instance.m_Bindings.Clear();
		Instance.m_BindingStates.Clear();
	}

	public static void ReleaseMe(T17DialogBox box)
	{
		int num = Instance.m_DialogBoxQueue.Count - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			if (Instance.m_DialogBoxQueue[num2].Dialog == box)
			{
				Instance.m_DialogBoxQueue.RemoveAt(num2);
				break;
			}
		}
		if (Instance.m_GlobalDialogs.Contains(box))
		{
			Instance.m_GlobalDialogs.Remove(box);
			Instance.m_bHasGlobalDiags = Instance.m_GlobalDialogs.Count > 0;
			T17EventSystemsManager.Instance.EnableAllEventSystems();
			return;
		}
		foreach (KeyValuePair<Gamer, List<T17DialogBox>> binding in Instance.m_Bindings)
		{
			if (binding.Value.Contains(box))
			{
				binding.Value.Remove(box);
				Instance.m_BindingStates[binding.Key] = binding.Value.Count > 0;
				break;
			}
		}
	}
}
