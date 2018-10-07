﻿using DatabaseAPI;
using UnityEngine;

namespace Lobby
{
	public class LobbyManager : MonoBehaviour
	{
		public static LobbyManager Instance;
		public AccountLogin accountLogin;
		public CharacterCustomization characterCustomization;

		public GameObject lobbyDialogue;

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(this);
			}
		}

		void OnEnable()
		{
			EventManager.AddHandler(EVENT.LoggedOut, SetOnLogOut);
		}

		void OnDisable()
		{
			EventManager.RemoveHandler(EVENT.LoggedOut, SetOnLogOut);
		}

		private void SetOnLogOut()
		{
			characterCustomization.gameObject.SetActive(false);
			accountLogin.gameObject.SetActive(true);
		}

		public void CheckIfFirstTime(){
			if(PlayerManager.CurrentCharacterSettings.username == null){
				//is First time, show the character settings screen
				lobbyDialogue.SetActive(false);
				characterCustomization.gameObject.SetActive(true);
			} else {
				//show something else
				lobbyDialogue.gameObject.SetActive(false);
			}
		}
	}
}