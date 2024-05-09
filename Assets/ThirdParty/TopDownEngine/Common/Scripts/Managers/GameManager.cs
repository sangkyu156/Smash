using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;
using UnityEngine.SceneManagement;
using NPOI.POIFS.Properties;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 가능한 TopDown 엔진 기본 이벤트 목록
    /// LevelStart : 레벨이 시작될 때 LevelManager에 의해 트리거됩니다.
    ///	LevelComplete : 레벨 끝에 도달하면 트리거될 수 있습니다.
    /// LevelEnd : 같은 것
    ///	Pause : 일시정지가 시작될 때 트리거됨
    ///	UnPause : 일시정지가 끝나고 정상으로 돌아갈 때 트리거됩니다.
    ///	PlayerDeath : 플레이어 캐릭터가 죽을 때 발동
    ///	RespawnStarted : 플레이어 캐릭터 리스폰 시퀀스가 ​​시작될 때 트리거됩니다.
    ///	RespawnComplete : 플레이어 캐릭터 리스폰 시퀀스가 ​​종료되면 트리거됩니다.
    ///	StarPicked : 별 보너스가 선택되면 트리거됩니다.
    ///	GameOver : 모든 생명이 손실될 때 LevelManager에 의해 트리거됩니다.
    /// CharacterSwap : 캐릭터가 바뀔 때 발동
    /// CharacterSwitch : 캐릭터가 바뀔 때 발동
    /// Repaint : UI 새로 고침을 요청하도록 트리거됨
    /// TogglePause : 일시중지(또는 일시중지 해제)를 요청하도록 트리거됨
    /// </summary>
    public enum TopDownEngineEventTypes
	{
		SpawnCharacterStarts,
		LevelStart,
		LevelComplete,
		LevelEnd,
		Pause,
		UnPause,
		PlayerDeath,
		SpawnComplete,
		RespawnStarted,
		RespawnComplete,
		StarPicked,
		GameOver,
		CharacterSwap,
		CharacterSwitch,
		Repaint,
		TogglePause,
		LoadNextScene
	}

    /// <summary>
    /// 레벨 시작 및 종료 신호에 사용되는 이벤트 유형(현재)
    /// </summary>
    public struct TopDownEngineEvent
	{
		public TopDownEngineEventTypes EventType;
		public Character OriginCharacter;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>
		public TopDownEngineEvent(TopDownEngineEventTypes eventType, Character originCharacter)
		{
			EventType = eventType;
			OriginCharacter = originCharacter;
		}

		static TopDownEngineEvent e;
		public static void Trigger(TopDownEngineEventTypes eventType, Character originCharacter)
		{
			e.EventType = eventType;
			e.OriginCharacter = originCharacter;
			MMEventManager.TriggerEvent(e);
        }
	}

    /// <summary>
    /// 현재 점수를 변경하는 데 사용할 수 있는 방법 목록
    /// </summary>
    public enum PointsMethods
	{
		Add,
		Set
	}

    /// <summary>
    /// 현재 점수의 변경 사항을 알리는 데 사용되는 이벤트 유형
    /// </summary>
    public struct TopDownEnginePointEvent
	{
		public PointsMethods PointsMethod;
		public int Points;
		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEnginePointEvent"/> struct.
		/// </summary>
		/// <param name="pointsMethod">Points method.</param>
		/// <param name="points">Points.</param>
		public TopDownEnginePointEvent(PointsMethods pointsMethod, int points)
		{
			PointsMethod = pointsMethod;
			Points = points;
		}

		static TopDownEnginePointEvent e;
		public static void Trigger(PointsMethods pointsMethod, int points)
		{
			e.PointsMethod = pointsMethod;
			e.Points = points;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 가능한 일시 중지 방법 목록
    /// </summary>
    public enum PauseMethods
	{
		PauseMenu,
		NoPauseMenu
	}

    /// <summary>
    /// 레벨당 하나씩 레벨에 대한 진입점을 저장하는 클래스입니다.
    /// </summary>
    public class PointsOfEntryStorage
	{
		public string LevelName;
		public int PointOfEntryIndex;
		public Character.FacingDirections FacingDirection;

		public PointsOfEntryStorage(string levelName, int pointOfEntryIndex, Character.FacingDirections facingDirection)
		{
			LevelName = levelName;
			FacingDirection = facingDirection;
			PointOfEntryIndex = pointOfEntryIndex;
		}
	}

    /// <summary>
    /// 게임 관리자는 포인트와 시간을 처리하는 지속적인 싱글톤입니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Game Manager")]
	public class GameManager : 	MMPersistentSingleton<GameManager>, 
		MMEventListener<MMGameEvent>, 
		MMEventListener<TopDownEngineEvent>, 
		MMEventListener<TopDownEnginePointEvent>
	{
		/// the target frame rate for the game
		[Tooltip("게임의 목표 프레임 속도")]
		public int TargetFrameRate = 300;
		[Header("Lives")]
		/// the maximum amount of lives the character can currently have
		[Tooltip("캐릭터가 현재 가질 수 있는 최대 생명수")]
		public int MaximumLives = 0;
		/// the current number of lives 
		[Tooltip("현재 생명 수")]
		public int CurrentLives = 0;

		[Header("Bindings")]
		/// the name of the scene to redirect to when all lives are lost
		[Tooltip("모든 생명을 잃었을 때 방향을 바꿀 장면의 이름")]
		public string GameOverScene;

		[Header("Points")]
		/// the current number of game points
		[MMReadOnly]
		[Tooltip("현재 게임 포인트 수")]
		public int Points;

		[Header("Pause")]
        /// 이것이 사실이라면 인벤토리를 열 때 게임이 자동으로 일시 중지됩니다.
        [Tooltip("이것이 사실이라면 인벤토리를 열 때 게임이 자동으로 일시 중지됩니다.")]
		public bool PauseGameWhenInventoryOpens = true;
        /// 게임이 현재 일시 정지된 경우 true
        public bool Paused { get; set; } 
		// true if we've stored a map position at least once
		public bool StoredLevelMapPosition{ get; set; }
		/// the current player
		public Vector2 LevelMapPosition { get; set; }
        /// 저장된 선택된 캐릭터
        public Character PersistentCharacter { get; set; }
		/// the list of points of entry and exit
		[Tooltip("출입 지점 목록")]
		public List<PointsOfEntryStorage> PointsOfEntry;
        /// 저장된 선택된 캐릭터
        public Character StoredCharacter { get; set; }

		// storage
		protected bool _inventoryOpen = false;
		protected bool _pauseMenuOpen = false;
		protected InventoryInputManager _inventoryInputManager;
		protected int _initialMaximumLives;
		protected int _initialCurrentLives;

		//내가 만든 변수
		//public GameObject player_GameObject;
		//public Transform uiCanvas;
        public Define.Stage stage = Define.Stage.Stage00;

        //ResourceManager _resource = new ResourceManager();

        //public static ResourceManager Resource { get { return Instance._resource; } }

        /// <summary>
        /// On Awake we initialize our list of points of entry
        /// </summary>
        protected override void Awake()
		{
			base.Awake ();
			PointsOfEntry = new List<PointsOfEntryStorage> ();
        }

		/// <summary>
		/// On Start(), sets the target framerate to whatever's been specified
		/// </summary>
		protected virtual void Start()
		{
			Application.targetFrameRate = TargetFrameRate;
			_initialCurrentLives = CurrentLives;
			_initialMaximumLives = MaximumLives;
        }

        /// <summary>
        /// 이 방법은 전체 게임 관리자를 재설정합니다
        /// </summary>
        public virtual void Reset()
		{
			Points = 0;
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
			Paused = false;
		}
		/// <summary>
		/// Use this method to decrease the current number of lives
		/// </summary>
		public virtual void LoseLife()
		{
			CurrentLives--;
		}

		/// <summary>
		/// Use this method when a life (or more) is gained
		/// </summary>
		/// <param name="lives">Lives.</param>
		public virtual void GainLives(int lives)
		{
			CurrentLives += lives;
			if (CurrentLives > MaximumLives)
			{
				CurrentLives = MaximumLives;
			}
		}

		/// <summary>
		/// Use this method to increase the max amount of lives, and optionnally the current amount as well
		/// </summary>
		/// <param name="lives">Lives.</param>
		/// <param name="increaseCurrent">If set to <c>true</c> increase current.</param>
		public virtual void AddLives(int lives, bool increaseCurrent)
		{
			MaximumLives += lives;
			if (increaseCurrent)
			{
				CurrentLives += lives;
			}
		}

		/// <summary>
		/// Resets the number of lives to their initial values.
		/// </summary>
		public virtual void ResetLives()
		{
			CurrentLives = _initialCurrentLives;
			MaximumLives = _initialMaximumLives;
		}

		/// <summary>
		/// Adds the points in parameters to the current game points.
		/// </summary>
		/// <param name="pointsToAdd">Points to add.</param>
		public virtual void AddPoints(int pointsToAdd)
		{
			Points += pointsToAdd;
			GUIManager.Instance.RefreshPoints();
		}
		
		/// <summary>
		/// use this to set the current points to the one you pass as a parameter
		/// </summary>
		/// <param name="points">Points.</param>
		public virtual void SetPoints(int points)
		{
			Points = points;
			GUIManager.Instance.RefreshPoints();
		}
		
		/// <summary>
		/// Enables the inventory input manager if found
		/// </summary>
		/// <param name="status"></param>
		protected virtual void SetActiveInventoryInputManager(bool status)
		{
			_inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager> ();
			if (_inventoryInputManager != null)
			{
				_inventoryInputManager.enabled = status;
			}
		}

        /// <summary>
        /// 현재 상태에 따라 게임을 일시 중지하거나 일시 중지를 해제합니다.
        /// </summary>
        public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu, bool unpauseIfPaused = true)
		{	
			if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen && GUIManager.Instance.PlayerInventoryCanvas.alpha == 1 && GUIManager.Instance.StoreInventoryCanvas.alpha == 1)
			{
				return;
			}

            // 시간이 아직 멈추지 않았다면		
            if (Time.timeScale>0.0f)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
				Instance.Paused=true;
				if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
				{
					GUIManager.Instance.SetPauseScreen(true);
					_pauseMenuOpen = true;
					SetActiveInventoryInputManager (false);
				}
				if (pauseMethod == PauseMethods.NoPauseMenu)
				{
					_inventoryOpen = true;
				}
			}
			else
			{
				if (unpauseIfPaused)
				{
					UnPause(pauseMethod);	
				}
			}		
			LevelManager.Instance.ToggleCharacterPause();
		}
        
		/// <summary>
		/// Unpauses the game
		/// </summary>
		public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
		{
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			Instance.Paused = false;
			if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
			{ 
				GUIManager.Instance.SetPauseScreen(false);
				_pauseMenuOpen = false;
				SetActiveInventoryInputManager (true);
			}
			if (_inventoryOpen)
			{
				_inventoryOpen = false;
			}
			LevelManager.Instance.ToggleCharacterPause();
		}

        /// <summary>
        /// 매개변수로 전달한 이름의 레벨에 대한 진입점을 저장합니다.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="entryIndex">Entry index.</param>
        /// <param name="exitIndex">Exit index.</param>
        public virtual void StorePointsOfEntry(string levelName, int entryIndex, Character.FacingDirections facingDirection)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						point.PointOfEntryIndex = entryIndex;
						return;
					}
				}	
			}

			PointsOfEntry.Add (new PointsOfEntryStorage (levelName, entryIndex, facingDirection));
		}

        /// <summary>
        /// 매개변수로 전달한 씬 이름이 있는 레벨에 대한 진입점 정보를 가져옵니다.
        /// </summary>
        /// <returns>The points of entry.</returns>
        /// <param name="levelName">Level name.</param>
        public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						return point;
					}
				}
			}
			return null;
		}

        /// <summary>
        /// 이름을 매개변수로 전달한 레벨에 대한 저장된 진입점 정보를 지웁니다.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public virtual void ClearPointOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						PointsOfEntry.Remove (point);
					}
				}
			}
		}

		/// <summary>
		/// Clears all points of entry.
		/// </summary>
		public virtual void ClearAllPointsOfEntry()
		{
			PointsOfEntry.Clear ();
		}

		/// <summary>
		/// Deletes all save files
		/// </summary>
		public virtual void ResetAllSaves()
		{
			MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
			MMSaveLoadManager.DeleteSaveFolder("TopDownEngine");
			MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
		}

		/// <summary>
		/// Stores the selected character for use in upcoming levels
		/// </summary>
		/// <param name="selectedCharacter">Selected character.</param>
		public virtual void StoreSelectedCharacter(Character selectedCharacter)
		{
			StoredCharacter = selectedCharacter;
		}

		/// <summary>
		/// Clears the selected character.
		/// </summary>
		public virtual void ClearSelectedCharacter()
		{
			StoredCharacter = null;
		}
		
		/// <summary>
		/// Sets a new persistent character
		/// </summary>
		/// <param name="newCharacter"></param>
		public virtual void SetPersistentCharacter(Character newCharacter)
		{
			PersistentCharacter = newCharacter;
		}
		
		/// <summary>
		/// Destroys a persistent character if there's one
		/// </summary>
		public virtual void DestroyPersistentCharacter()
		{
			if (PersistentCharacter != null)
			{
				Destroy(PersistentCharacter.gameObject);
				SetPersistentCharacter(null);
			}
			

			if (LevelManager.Instance.Players[0] != null)
			{
				if (LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<CharacterPersistence>() != null)
				{
					Destroy(LevelManager.Instance.Players[0].gameObject);	
				}
			}
		}

		//스테이지에 따라 리워드 골드 세팅
		public int SetRewardGold()
		{
			int rewardGold = 0;

            switch (stage)
			{
				case Define.Stage.Stage00: rewardGold = 70; break;
                case Define.Stage.Stage01:rewardGold = 100; break;
                case Define.Stage.Stage02:rewardGold = 130; break;
                case Define.Stage.Stage03:rewardGold = 170; break;
                case Define.Stage.Stage04:rewardGold = 220; break;
                case Define.Stage.Stage05:rewardGold = 100; break;
                case Define.Stage.Stage06:rewardGold = 100; break;
                case Define.Stage.Stage07:rewardGold = 100; break;
                case Define.Stage.Stage08:rewardGold = 100; break;
                case Define.Stage.Stage09:rewardGold = 100; break;
                case Define.Stage.Stage10:rewardGold = 100; break;
                case Define.Stage.Stage11:rewardGold = 100; break;
                case Define.Stage.Stage12:rewardGold = 100; break;
                case Define.Stage.Stage13:rewardGold = 100; break;
                case Define.Stage.Stage14:rewardGold = 100; break;
                case Define.Stage.Stage15:rewardGold = 100; break;
                case Define.Stage.Stage16:rewardGold = 100; break;
                case Define.Stage.Stage17:rewardGold = 100; break;
                case Define.Stage.Stage18:rewardGold = 100; break;
                case Define.Stage.Stage19:rewardGold = 100; break;
                case Define.Stage.Stage20:rewardGold = 100; break;
                case Define.Stage.Stage21:rewardGold = 100; break;
                case Define.Stage.Stage22:rewardGold = 100; break;
                case Define.Stage.Stage23:rewardGold = 100; break;
                case Define.Stage.Stage24:rewardGold = 100; break;
                case Define.Stage.Stage25:rewardGold = 800; break;
                case Define.Stage.Stage26:rewardGold = 100; break;
                case Define.Stage.Stage27:rewardGold = 100; break;
                case Define.Stage.Stage28:rewardGold = 100; break;
                case Define.Stage.Stage29:rewardGold = 100; break;
                case Define.Stage.Stage30:rewardGold = 100; break;
                case Define.Stage.Stage31:rewardGold = 100; break;
                case Define.Stage.Stage32:rewardGold = 100; break;
                case Define.Stage.Stage33:rewardGold = 100; break;
                case Define.Stage.Stage34:rewardGold = 100; break;
                case Define.Stage.Stage35:rewardGold = 100; break;
                case Define.Stage.Stage36:rewardGold = 100; break;
                case Define.Stage.Stage37:rewardGold = 100; break;
                case Define.Stage.Stage38:rewardGold = 100; break;
                case Define.Stage.Stage39:rewardGold = 100; break;
                case Define.Stage.Stage40:rewardGold = 100; break;
                case Define.Stage.Stage41:rewardGold = 100; break;
                case Define.Stage.Stage42:rewardGold = 100; break;
                case Define.Stage.Stage43:rewardGold = 100; break;
                case Define.Stage.Stage44:rewardGold = 100; break;
                case Define.Stage.Stage45:rewardGold = 100; break;
                case Define.Stage.Stage46:rewardGold = 100; break;
                case Define.Stage.Stage47:rewardGold = 100; break;
                case Define.Stage.Stage48:rewardGold = 100; break;
                case Define.Stage.Stage49:rewardGold = 100; break;
                case Define.Stage.Stage50:rewardGold = 100;  break;
            }

			return rewardGold;
        }

        /// <summary>
        /// MMGameEvents를 포착하고 그에 따라 작동하여 해당 사운드를 재생합니다.
        /// </summary>
        /// <param name="gameEvent">MMGameEvent event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "inventoryOpens":
					if (PauseGameWhenInventoryOpens)
					{
                        Pause(PauseMethods.NoPauseMenu, false);
                    }					
					break;

				case "inventoryCloses":
					if (PauseGameWhenInventoryOpens)
					{
                        UnPause(PauseMethods.NoPauseMenu);
                    }
					break;
			}
		}

		/// <summary>
		/// Catches TopDownEngineEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="engineEvent">TopDownEngineEvent event.</param>
		public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.TogglePause:
					if (Paused)
					{
						TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
					}
					else
					{
						TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
					}
					break;
				case TopDownEngineEventTypes.Pause:
					if(GUIManager.Instance.PopupParents != null)
					{
						if (CheckChildActivation(GUIManager.Instance.PopupParents) == true)
						{
							if(LevelManager.Instance.HelperPopupIsOpen == true)
							{
								Time.timeScale = 1;
								LevelManager.Instance.HelperPopupIsOpen = false;
							}
						}
						else
							Pause();
                    }
					else
                        Pause();
                    break;

				case TopDownEngineEventTypes.UnPause:
                    if (GUIManager.Instance.PopupParents != null)
                    {
                        if (CheckChildActivation(GUIManager.Instance.PopupParents) == true)
                        {
                            if (LevelManager.Instance.HelperPopupIsOpen == true)
                            {
                                Time.timeScale = 1;
                                LevelManager.Instance.HelperPopupIsOpen = false;
                            }
                        }
                        else
                            UnPause();
                    }
                    else
                        UnPause();
                    break;
			}
		}

        //특정 오브젝트의 자식들이 활성화 된것이 있는지 확인하고 활성화된 자식이 있으면 비활성화 하는 함수
		bool CheckChildActivation(GameObject checkOb)
		{
			bool active = false;

			for (int i = 0; i < checkOb.transform.childCount; i++)
			{
                if(checkOb.transform.GetChild(i).gameObject.activeSelf == true)
				{
					active = true;
					checkOb.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

			return active;
		}

        /// <summary>
        /// Catches TopDownEnginePointsEvents and acts on them, playing the corresponding sounds
        /// </summary>
        /// <param name="pointEvent">TopDownEnginePointEvent event.</param>
        public virtual void OnMMEvent(TopDownEnginePointEvent pointEvent)
		{
			switch (pointEvent.PointsMethod)
			{
				case PointsMethods.Set:
					SetPoints(pointEvent.Points);
					break;

				case PointsMethods.Add:
					AddPoints(pointEvent.Points);
					break;
			}
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<TopDownEngineEvent> ();
			this.MMEventStartListening<TopDownEnginePointEvent> ();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<TopDownEngineEvent> ();
			this.MMEventStopListening<TopDownEnginePointEvent> ();
		}

		//레벨매니져한테 캐릭터형 받아와서 오브젝트형으로 바꾸기
		//public void playerTypeChange(Character player)
		//{
		//	player_GameObject = player.transform.gameObject;
		//	Transform ch1 = player.gameObject.transform.Find("DogModel");
  //      }

        //스킬,아이템 활성화 직전에 skillPostion 자리움겨놓기
		public void SetSkillPostion()
		{
            //skillPostion.position = player_GameObject.transform.position;
        }

		public void SetCurrentStage(Define.Stage stage_)
		{
            stage = stage_;
        }

		//플레이어 정보 저장하기
		void SetPlayerInfo()
		{

        }
    }
}