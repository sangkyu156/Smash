using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 Explodude 데모 장면에서 승자 화면 표시를 처리합니다.
    /// </summary>
    public class ExplodudesWinnerScreen : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// the ID of the player we want this screen to appear for
		[Tooltip("이 화면을 표시하려는 플레이어의 ID")]
		public string PlayerID = "Player1";
		/// the canvas group containing the winner screen
		[Tooltip("승자 화면이 포함된 캔버스 그룹")]
		public CanvasGroup WinnerScreen;

		/// <summary>
		/// On Start we make sure our screen is disabled
		/// </summary>
		protected virtual void Start()
		{
			WinnerScreen.gameObject.SetActive(false);
		}

		/// <summary>
		/// On game over we display our winner screen if needed
		/// </summary>
		/// <param name="tdEvent"></param>
		public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			switch (tdEvent.EventType)
			{
				case TopDownEngineEventTypes.GameOver:
					if (PlayerID == (LevelManager.Instance as ExplodudesMultiplayerLevelManager).WinnerID)
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