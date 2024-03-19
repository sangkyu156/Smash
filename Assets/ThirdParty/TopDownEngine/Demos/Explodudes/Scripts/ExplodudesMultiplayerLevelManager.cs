using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// MultiplayerLevelManager를 확장하여 자신만의 특정 규칙을 구현하는 방법에 대한 예제 클래스입니다.
    /// 이놈의 규칙은 다음과 같습니다 :
    /// - 모든 플레이어(한 명 제외)가 죽으면 게임이 중단되고 가장 많은 코인을 얻은 사람이 승리합니다.
    /// - 게임이 끝나면 승자 화면이 표시되고 아무데나 점프를 누르면 게임이 다시 시작됩니다.
    /// </summary>
    public class ExplodudesMultiplayerLevelManager : MultiplayerLevelManager
	{
		[Header("Explodudes Settings")]
		/// the duration of the game, in seconds
		[Tooltip("게임 시간(초)")]
		public int GameDuration = 99;
		/// the ID of the winner
		public string WinnerID { get; set; }

		protected string _playerID;
		protected bool _gameOver = false;

		/// <summary>
		/// On init, we initialize our points and countdowns
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			WinnerID = "";
		}

		/// <summary>
		/// Whenever a player dies, we check if we only have one left alive, in which case we trigger our game over routine
		/// </summary>
		/// <param name="playerCharacter"></param>
		protected override void OnPlayerDeath(Character playerCharacter)
		{
			base.OnPlayerDeath(playerCharacter);
			int aliveCharacters = 0;
			int i = 0;

			foreach (Character character in LevelManager.Instance.Players)
			{
				if (character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
				{
					WinnerID = character.PlayerID;
					aliveCharacters++;
				}
				i++;
			}

			if (aliveCharacters <= 1)
			{
				StartCoroutine(GameOver());
			}
		}

		/// <summary>
		/// On game over, freezes time and displays the game over screen
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator GameOver()
		{
			yield return new WaitForSeconds(2f);
			if (WinnerID == "")
			{
				WinnerID = "Player1";
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			_gameOver = true;
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
		}

		/// <summary>
		/// On update, we update our countdowns and check for input if we're in game over state
		/// </summary>
		public virtual void Update()
		{
			CheckForGameOver();
		}

		/// <summary>
		/// If we're in game over state, checks for input and restarts the game if needed
		/// </summary>
		protected virtual void CheckForGameOver()
		{
			if (_gameOver)
			{
				if ((Input.GetButton("Player1_Jump"))
				    || (Input.GetButton("Player2_Jump"))
				    || (Input.GetButton("Player3_Jump"))
				    || (Input.GetButton("Player4_Jump")))
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
					MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
				}
			}
		}
        
		/// <summary>
		/// Starts listening for pickable item events
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
		}

		/// <summary>
		/// Stops listening for pickable item events
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
		}
	}
}