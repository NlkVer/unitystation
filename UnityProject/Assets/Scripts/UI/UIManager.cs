﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private static UIManager uiManager;
	public ControlAction actionControl;
	public DragAndDrop dragAndDrop;
	public ControlDisplays displayControl;
	public DisplayManager displayManager;
	public GameObject bottomBar;
	public Hands hands;
	public ControlIntent intentControl;
	public InventorySlotCache inventorySlotCache;
	public PlayerHealthUI playerHealthUI;
	public PlayerListUI playerListUIControl;
	public Text toolTip;
	public ControlWalkRun walkRunControl;
	public UI_StorageHandler storageHandler;
	public Toggle ttsToggle;
	[HideInInspector]
	public ProgressBar progressBar;

	///Global flag for focused input field. Movement keystrokes are ignored if true.
	/// <see cref="InputFieldFocus"/> handles this flag automatically
	public static bool IsInputFocus
	{
		get
		{
			return Instance && Instance.isInputFocus;
		}
		set
		{
			if (!Instance)
			{
				return;
			}
			Instance.isInputFocus = value;
		}
	}

	public bool isInputFocus;

	public static UIManager Instance
	{
		get
		{
			if (!uiManager)
			{
				uiManager = FindObjectOfType<UIManager>();
			}

			return uiManager;
		}
	}

	//		public static ControlChat Chat => Instance.chatControl; //Use ChatRelay.Instance.AddToChatLog instead!
	public static ProgressBar ProgressBar => Instance.progressBar;
	public static PlayerHealthUI PlayerHealthUI => Instance.playerHealthUI;

	public static Hands Hands => Instance.hands;

	public static ControlIntent Intent => Instance.intentControl;

	public static ControlAction Action => Instance.actionControl;

	public static DragAndDrop DragAndDrop => Instance.dragAndDrop;

	public static ControlWalkRun WalkRun => Instance.walkRunControl;

	public static ControlDisplays Display => Instance.displayControl;

	public static PlayerListUI PlayerListUI => Instance.playerListUIControl;

	public static DisplayManager DisplayManager => Instance.displayManager;
	public static UI_StorageHandler StorageHandler => Instance.storageHandler;

	public static string SetToolTip
	{
		set { Instance.toolTip.text = value; }
	}

	public static InventorySlotCache InventorySlots => Instance.inventorySlotCache;

	/// <summary>
	///     Current Intent status
	/// </summary>
	public static Intent CurrentIntent { get; set; }

	/// <summary>
	///     What is DamageZoneSeclector currently set at
	/// </summary>
	public static BodyPartType DamageZone { get; set; }

	/// <summary>
	///     Is throw selected?
	/// </summary>
	public static bool IsThrow { get; set; }

	/// <summary>
	///     Is Oxygen On?
	/// </summary>
	public static bool IsOxygen { get; set; }

	public static void ResetAllUI()
	{
		UI_ItemSlot[] slots = Instance.GetComponentsInChildren<UI_ItemSlot>(true);
		foreach (UI_ItemSlot slot in slots)
		{
			slot.Reset();
		}
		foreach (DamageMonitorListener listener in Instance.GetComponentsInChildren<DamageMonitorListener>())
		{
			listener.Reset();
		}
		Camera2DFollow.followControl.ZeroStars();
	}

	/// <summary>
	///     use this for client UI mangling attepts
	/// </summary>
	public static bool TryUpdateSlot(UISlotObject slotInfo)
	{
		if (!CanPutItemToSlot(slotInfo))
		{
			return false;
		}
		InventoryInteractMessage.Send(slotInfo.SlotUUID, slotInfo.FromSlotUUID, slotInfo.SlotContents, true);
		UpdateSlot(slotInfo);
		return true;
	}

	/// <summary>
	///     rather direct method that doesn't check anything.
	///     probably should check if you CanPutItemToSlot before using it
	/// </summary>
	public static void UpdateSlot(UISlotObject slotInfo)
	{
		Debug.Log("UPDATESLOT: " + slotInfo.ToString());
		if (string.IsNullOrEmpty(slotInfo.SlotUUID) && !string.IsNullOrEmpty(slotInfo.FromSlotUUID))
		{
			Debug.Log("ALRIGHT DROP");
			//Dropping updates:
			var _fromSlot = InventorySlots.GetSlotByUUID(slotInfo.FromSlotUUID);
			if (_fromSlot != null)
			{
				Debug.Log("Ffound slot");
				Debug.Log("Clear From slot of Item: " + _fromSlot.Item?.name);
				_fromSlot.Clear();
				return;

			}
		}
		Logger.LogTraceFormat("Updating slots: {0}", Category.UI, slotInfo);
		//			InputTrigger.Touch(slotInfo.SlotContents);
		var slot = InventorySlots.GetSlotByUUID(slotInfo.SlotUUID);
		slot.SetItem(slotInfo.SlotContents);
		Debug.Log("Set Slot: " + slot.eventName);

		var fromSlot = InventorySlots.GetSlotByUUID(slotInfo.FromSlotUUID);
		if (fromSlot?.Item == slotInfo.SlotContents)
		{
			fromSlot.Clear();
			Debug.Log("Empty FromSlot: " + fromSlot.inventorySlot.SlotName);
		}
	}

	public static bool CanPutItemToSlot(UISlotObject proposedSlotInfo)
	{
		if (proposedSlotInfo.IsEmpty() || !SendUpdateAllowed(proposedSlotInfo.SlotContents))
		{
			return false;
		}
		InventorySlot invSlot = InventoryManager.GetSlotFromUUID(proposedSlotInfo.SlotUUID, false);
		PlayerScript lps = PlayerManager.LocalPlayerScript;

		if (!lps || lps.canNotInteract() || invSlot.Item != null)
		{
			return false;
		}

		UI_ItemSlot uiItemSlot = InventorySlots.GetSlotByUUID(invSlot.UUID);
		if (uiItemSlot == null)
		{
			return false;
		}

		if (!uiItemSlot.CheckItemFit(proposedSlotInfo.SlotContents))
		{
			return false;
		}
		return true;
	}

	public static string FindEmptySlotForItem(GameObject itemToPlace)
	{
		foreach (UI_ItemSlot slot in InventorySlotCache.InventorySlots)
		{
			UISlotObject slottingAttempt = new UISlotObject(slot.inventorySlot.UUID, itemToPlace);
			if (CanPutItemToSlot(slottingAttempt))
			{
				return slot.eventName;
			}
		}

		return null;
	}

	/// Checks if player received transform update after sending interact message
	/// (Anti-blinking protection)
	public static bool SendUpdateAllowed(GameObject item)
	{
		//			if ( CustomNetworkManager.Instance._isServer ) return true;
		//			var netId = item.GetComponent<NetworkIdentity>().netId;
		//			var lastReceive = item.GetComponent<NetworkTransform>().lastSyncTime;
		//			var lastSend = InputTrigger.interactCache.ContainsKey(netId) ? InputTrigger.interactCache[netId] : 0f;
		//			if ( lastReceive < lastSend )
		//			{
		//				return CanTrySendAgain(lastSend, lastReceive);
		//			}
		//			Logger.LogTraceFormat("ItemAction allowed! {2} msgcache {0} {1}", Category.UI, InputTrigger.interactCache.Count, lastSend, item.name);
		return true;
	}

	private static bool CanTrySendAgain(float lastSend, float lastReceive)
	{
		float f = Time.time - lastSend;
		float d = lastSend - lastReceive;
		bool canTrySendAgain = f >= d || f >= 1.5;
		Logger.LogTraceFormat("canTrySendAgain = {0} {1}>={2} ", Category.UI, canTrySendAgain, f, d);
		return canTrySendAgain;
	}

	public static void SetDeathVisibility(bool vis)
	{
		//			Logger.Log("I was activated!");
		foreach (Transform child in Display.hudRight.GetComponentsInChildren<Transform>(true))
		{
			if (child.gameObject.name != "OxygenSelector" && child.gameObject.name != "PlayerHealth_UI_Hud")
			{
				child.gameObject.SetActive(vis);
			}
		}

		foreach (Transform child in Display.hudBottom.GetComponentsInChildren<Transform>(true))
		{
			Transform eh = Display.hudBottom.transform.Find("Equip-Hands");
			if (child.gameObject.name != "Panel_Hud_Bottom" && !child.transform.IsChildOf(eh) && child.gameObject.name != "Equip-Hands")
			{
				child.gameObject.SetActive(vis);
			}
		}
	}
}