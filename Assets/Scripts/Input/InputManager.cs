using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class PlayerJoinEvent : UnityEvent<int, string> { }

[RequireComponent(typeof(StandaloneInputModule))]
public class InputManager : MonoBehaviour {
	private static InputManager instance;
	public static InputManager Instance {
		get {
			if (instance == null) instance = FindObjectOfType<InputManager>();
			return instance;
		}
	}

	public readonly static string[] ControllerCodeNames = Enum.GetNames(typeof(ControllerCode));

	[SerializeField]
	private bool paused = false;
	public static bool Paused { get { return Instance.paused; } set { Instance.paused = value; } }

	[SerializeField]
	private List<string> playerRoster = new List<string>();
	private int usedSlots = 0;
	
	public static List<int> AssignedPlayers {
		get {
			var list = new List<int>(Instance.usedSlots);
			for (var i = 0; i < Instance.playerRoster.Count; i++)
				if (Instance.playerRoster[i] != null) list.Add(i);
			return list;
		}
	}

	public static string GetPlayerControllerCode(int player) {
		if (player < 0 || player >= Instance.playerRoster.Count) {
			Debug.LogWarning("Tried to get a controller code for player " + player + " but no code has been assigned.");
			return null;
		}
		return Instance.playerRoster[player];
	}

	public static HashSet<string> ActiveControllers { get { return new HashSet<string>(Instance.playerRoster); } }

	public static int PlayerCount { get { return Instance.usedSlots; } }

	[SerializeField]
	private PlayerJoinEvent onPlayerJoin = new PlayerJoinEvent();
	public static PlayerJoinEvent OnPlayerJoin { get { return Instance.onPlayerJoin; } }
	[SerializeField]
	private PlayerJoinEvent onPlayerLeft = new PlayerJoinEvent();
	public static PlayerJoinEvent OnPlayerLeft { get { return Instance.onPlayerLeft; } }

	/// <summary>
	/// This field is assumed to be in the same order as the ControllerCode enum
	/// </summary>
	[SerializeField]
	private List<StandaloneInputModule> availableInputModules = new List<StandaloneInputModule>();
	[SerializeField]
	private List<StandaloneInputModule> playerInputModules = new List<StandaloneInputModule>();

	private int activePlayer = -1;
	public static int ActivePlayer { get { return Instance.activePlayer; } private set { Instance.activePlayer = value; } }
	public static string ActiveController { get { return Instance.playerRoster[ActivePlayer]; } }

	private static void SetPlayerControllerModule(int moduleIndex, int player) {
		if (moduleIndex < 0 || player < 0 || moduleIndex >= Instance.availableInputModules.Count) return;

		var inputModule = Instance.availableInputModules[moduleIndex];
		if (player >= Instance.playerInputModules.Count) Instance.playerInputModules.Insert(player, inputModule);
		else Instance.playerInputModules[player] = inputModule;

		if (ActivePlayer < 0) SetActivePlayer(player);
	}

	private static void ClearPlayerControllerModule(int player) {
		if (player < 0 || player >= Instance.playerInputModules.Count) return;

		if (ActivePlayer == player) {
			var firstAvailable = Instance.playerRoster.FirstNonNullIndex();
			SetActivePlayer(firstAvailable);
		}

		Instance.playerInputModules[player] = null;
	}

	/// <summary>Given a player index, get the corresponding input module and activate it.</summary>
	/// <param name="player">Roster index of the player.</param>
	public static void SetActivePlayer(int player) {
		if (player >= Instance.playerInputModules.Count) return;
		if (player >= 0 && !Instance.playerInputModules[player]) return;
		
		// Try to find target module
		var activeModule = player >= 0 ? Instance.playerInputModules[player] 
			: Instance.availableInputModules.Count > 0 ? Instance.availableInputModules[0] : null;

		if (activeModule != null) {
			// Disable previous module
			if (ActivePlayer >= 0) Instance.playerInputModules[ActivePlayer].enabled = false;
			else if (Instance.availableInputModules.Count > 0) Instance.availableInputModules[0].enabled = false;
			// Set new player as active
			ActivePlayer = player;
			// Force the module to activate
			activeModule.enabled = true;
			activeModule.forceModuleActive = true;
		}
	}

	public static bool GetButtonFromAny(string button) {
		if (Paused) return false;
		foreach (var code in ControllerCodeNames) if (Input.GetButton(code + "_" + button)) return true;
		return false;
	}

	public static bool GetButtonDownFromAny(string button) {
		if (Paused) return false;
		foreach (var code in ControllerCodeNames) if (Input.GetButtonDown(code + "_" + button)) return true;
		return false;
	}

	public static bool GetButtonUpFromAny(string button) {
		if (Paused) return false;
		foreach (var code in ControllerCodeNames) if (Input.GetButtonUp(code + "_" + button)) return true;
		return false;
	}

	public static bool GetButtonFromActive(string button) {
		if (Paused) return false;
		if (Instance.activePlayer < 0) return GetButtonFromAny(button);
		if (Input.GetButton(ActiveController + "_" + button)) return true;
		return false;
	}

	public static bool GetButtonDownFromActive(string button) {
		if (Paused) return false;
		if (Instance.activePlayer < 0) return GetButtonDownFromAny(button);
		if (Input.GetButtonDown(ActiveController + "_" + button)) return true;
		return false;
	}

	public static bool GetButtonUpFromActive(string button) {
		if (Paused) return false;
		if (Instance.activePlayer < 0) return GetButtonUpFromAny(button);
		if (Input.GetButtonUp(ActiveController + "_" + button)) return true;
		return false;
	}

	public static bool GetPlayerButton(int player, string button) {
		if (Paused) return false;
		if (player < 0 || Instance.playerRoster.Count <= player) return false;
		return Input.GetButton(Instance.playerRoster[player] + "_" + button);
	}

	public static bool GetPlayerButtonDown(int player, string button) {
		if (Paused) return false;
		if (player < 0 || Instance.playerRoster.Count <= player) return false;
		return Input.GetButtonDown(Instance.playerRoster[player] + "_" + button);
	}

	public static bool GetPlayerButtonUp(int player, string button) {
		if (Paused) return false;
		if (player < 0 || Instance.playerRoster.Count <= player) return false;
		return Input.GetButtonUp(Instance.playerRoster[player] + "_" + button);
	}

	public static float GetPlayerAxis(int player, string axis) {
		if (Paused) return 0.0f;
		if (player < 0 || Instance.playerRoster.Count <= player) return 0;
		return Input.GetAxis(Instance.playerRoster[player] + "_" + axis);
	}

	public static int GetPlayerNumber(ControllerCode controllerCode) {
		return Instance.playerRoster.FindIndex(value => value == controllerCode.ToString());
	}

	public static int AddPlayer(string controllerCode) {
		var controllerCodeIndex = GetControllerCodeIndexByString(controllerCode);
		if (controllerCodeIndex < 0) return -1;

		var position = Instance.playerRoster.FindIndex(value => value == null);
		if (position < 0) {
			Instance.playerRoster.Add(controllerCode);
			position = Instance.playerRoster.Count - 1;

		}
		else {
			Instance.playerRoster[position] = controllerCode;
		}

		SetPlayerControllerModule(controllerCodeIndex, position);

		Instance.usedSlots++;
		OnPlayerJoin.Invoke(position, controllerCode);
		return position;
	}

	public static bool RemovePlayer(int playerNumber) {
		if (playerNumber < 0 || playerNumber >= Instance.playerRoster.Count) return false;
		var controllerCode = Instance.playerRoster[playerNumber];

		Instance.playerRoster[playerNumber] = null;
		ClearPlayerControllerModule(playerNumber);

		Instance.usedSlots--;
		OnPlayerLeft.Invoke(playerNumber, controllerCode);
		return true;
	}

	public static bool RemoveActivePlayer() {
		return RemovePlayer(Instance.activePlayer);
	}

	public static void ClearRoster() {
		Debug.Log("Clearing Roster");
		for (var i = 0; i < Instance.playerRoster.Count; i++) {
			if (!string.IsNullOrEmpty(Instance.playerRoster[i])) {
				Debug.Log("Removing Player " + i);
				RemovePlayer(i);
			}
		}
	}

	private void Awake() {
		DontDestroyOnLoad(gameObject);
	}

	private void Start() {
		if (Instance != this) {
			Destroy(gameObject);
			return;
		}
		foreach (var inputModule in availableInputModules) inputModule.enabled = false;
		SetActivePlayer(playerInputModules.FirstNonNullIndex());
	}

	public static int GetControllerCodeIndexByString(string controllerCode) {
		var codeEnum = Enum.Parse(typeof(ControllerCode), controllerCode, true);
		if (codeEnum is ControllerCode) return (int)codeEnum;
		else return -1;
	}

	public static string GetControllerName(ControllerCode code) {
		switch (code) {
			case ControllerCode.KB:
				return "Keyboard and Mouse";
			case ControllerCode.C1:
				return "Controller 1";
			case ControllerCode.C2:
				return "Controller 2";
			case ControllerCode.C3:
				return "Controller 3";
			case ControllerCode.C4:
				return "Controller 4";
			default:
				return "";
		}
	}

	public static string GetControllerName(string code) {
		return GetControllerName((ControllerCode)Enum.Parse(typeof(ControllerCode), code, true));
	}

	public enum ControllerCode {
		KB,
		C1,
		C2,
		C3,
		C4
	};
}
