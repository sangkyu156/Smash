using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
	public class GrasslandHUD : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// The playerID associated to this HUD
		[Tooltip("이 HUD에 연결된 플레이어 ID")]
		public string PlayerID = "Player1";
		/// the progress bar to use to show the healthbar
		[Tooltip("체력바를 표시하는 데 사용할 진행률 표시줄")]
		public MMProgressBar HealthBar;
		/// the Text comp to use to display the player name
		[Tooltip("플레이어 이름을 표시하는 데 사용할 텍스트 구성 요소")]
		public Text PlayerName;
		/// the radial progress bar to put around the avatar
		[Tooltip("아바타 주위에 배치할 방사형 진행률 표시줄")]
		public MMRadialProgressBar AvatarBar;
		/// the counter used to display coin amounts
		[Tooltip("코인 금액을 표시하는 데 사용되는 카운터")]
		public Text CoinCounter;
		/// the mask to use when the target player dies
		[Tooltip("대상 플레이어가 죽을 때 사용할 마스크")]
		public CanvasGroup DeadMask;
		/// the screen to display if the target player wins
		[Tooltip("대상 플레이어가 승리할 경우 표시되는 화면")]
		public CanvasGroup WinnerScreen;

		protected virtual void Start()
		{
			CoinCounter.text = "0";
			DeadMask.gameObject.SetActive(false);
			WinnerScreen.gameObject.SetActive(false);
		}

		public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			switch (tdEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					if (tdEvent.OriginCharacter.PlayerID == PlayerID)
					{
						DeadMask.gameObject.SetActive(true);
						DeadMask.alpha = 0f;
						StartCoroutine(MMFade.FadeCanvasGroup(DeadMask, 0.5f, 0.8f, true));
					}
					break;
				case TopDownEngineEventTypes.Repaint:
					foreach (GrasslandsMultiplayerLevelManager.GrasslandPoints points in (LevelManager.Instance as GrasslandsMultiplayerLevelManager).Points)
					{
						if (points.PlayerID == PlayerID)
						{
							CoinCounter.text = points.Points.ToString();
						}
					}
					break;
				case TopDownEngineEventTypes.GameOver:
					if (PlayerID == (LevelManager.Instance as GrasslandsMultiplayerLevelManager).WinnerID)
					{
						WinnerScreen.gameObject.SetActive(true);
						WinnerScreen.alpha = 0f;
						StartCoroutine(MMFade.FadeCanvasGroup(WinnerScreen, 0.5f, 0.8f, true));
					}
					break;
			}
            
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}