using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerJoinEvent : UnityEvent<int, string> { }

public class InputManager : MonoBehaviour {
	private static InputManager instance;
	public static InputManager Instance {
		get {
			if (instance == null) instance = FindObjectOfType<InputManager>();
			return instance;
		}
	}

	public enum ControllerCode {
		KB,
		C1,
		C2,
		C3,
		C4
	};
	public readonly static string[] ControllerCodeNames = Enum.GetNames(typeof(ControllerCode));
	
	private string[] playerRoster = new string[ControllerCodeNames.Length];
	private int usedSlots = 0;

	public static string GetPlayerControllerCode(int player) {
		if (player < 0 || player >= Instance.playerRoster.Length) return null;
		return Instance.playerRoster[player];
	}
	
	public static HashSet<string> ActivePlayers { get { return new HashSet<string>(Instance.playerRoster); } }

	public static bool RosterFull { get { return Instance.usedSlots >= Instance.playerRoster.Length; } }

	[SerializeField]
	private PlayerJoinEvent onPlayerJoin = new PlayerJoinEvent();
	public static PlayerJoinEvent OnPlayerJoin { get { return Instance.onPlayerJoin; } }
	[SerializeField]
	private PlayerJoinEvent onPlayerLeft = new PlayerJoinEvent();
	public static PlayerJoinEvent OnPlayerLeft { get { return Instance.onPlayerLeft; } }

	public static bool GetPlayerButton(int player, string button) {
		if (player < 0 || Instance.playerRoster.Length <= player) return false;
		return Input.GetButton(Instance.playerRoster[player] + "_" + button);
	}

	public static bool GetPlayerButtonDown(int player, string button) {
		if (player < 0 || Instance.playerRoster.Length <= player) return false;
		return Input.GetButtonDown(Instance.playerRoster[player] + "_" + button);
	}

	public static bool GetPlayerButtonUp(int player, string button) {
		if (player < 0 || Instance.playerRoster.Length <= player) return false;
		return Input.GetButtonUp(Instance.playerRoster[player] + "_" + button);
	}

	public static float GetPlayerAxis(int player, string axis) {
		if (player < 0 || Instance.playerRoster.Length <= player) return 0;
		return Input.GetAxis(Instance.playerRoster[player] + "_" + axis);
	}

	public static int GetPlayerNumber(ControllerCode controllerCode) {
		return Array.IndexOf(Instance.playerRoster, controllerCode.ToString());
	}

	public static int AddPlayer(string controllerCode) {
		var firstAvailable = Array.IndexOf(Instance.playerRoster, null);
		if (firstAvailable < 0) return -1;

		Instance.playerRoster[firstAvailable] = controllerCode;
		Instance.usedSlots++;
		OnPlayerJoin.Invoke(firstAvailable, controllerCode);
		return firstAvailable;
	}

	public static bool RemovePlayer(int playerNumber) {
		if (playerNumber < 0 || playerNumber > Instance.playerRoster.Length) return false;
		Instance.playerRoster[playerNumber] = null;
		Instance.usedSlots--;
		OnPlayerLeft.Invoke(playerNumber, GetPlayerControllerCode(playerNumber));
		return true;
	}

	private void Awake() {
		DontDestroyOnLoad(gameObject);
	}
}
