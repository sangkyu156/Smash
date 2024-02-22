using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.IO;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 플레이어를 생성하고, 체크포인트를 처리하고 다시 생성합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Level Manager")]
	public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
	{
        /// 플레이어에게 원하는 조립식 건물
        [Header("Instantiate Characters")]
		[MMInformation("LevelManager는 스폰/리스폰, 체크포인트 관리, 레벨 경계 처리를 담당합니다. 여기에서 레벨에 대해 하나 이상의 플레이 가능한 캐릭터를 정의할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
        /// 플레이어 ID가 자동으로 부여되어야 하는지(일반적으로 yes)
        [Tooltip("플레이어 ID가 자동으로 부여되어야 하는지(일반적으로 yes)")]
		public bool AutoAttributePlayerIDs = true;
        /// 인스턴스화할 플레이어 프리팹 목록
        [Tooltip("이 레벨 관리자가 시작 시 인스턴스화할 플레이어 프리팹 목록")]
		public Character[] PlayerPrefabs ;

		[Header("이미 장면에 등장하는 캐릭터")]
		[MMInformation("LevelManager가 캐릭터를 인스턴스화하도록 하는 것이 권장되지만, 대신 장면에 이미 존재하도록 하려면 아래 목록에 바인딩하기만 하면 됩니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 런타임 전에 장면에 이미 존재하는 캐릭터 목록입니다. 이 목록이 채워지면 PlayerPrefabs가 무시됩니다.
        [Tooltip("런타임 전에 장면에 이미 존재하는 캐릭터 목록입니다. 이 목록이 채워지면 PlayerPrefabs가 무시됩니다.")]
		public List<Character> SceneCharacters;

		[Header("체크포인트")]
        /// 진입점이 지정되지 않은 경우 초기 스폰 지점으로 사용할 체크포인트
        [Tooltip("진입점이 지정되지 않은 경우 초기 스폰 지점으로 사용할 체크포인트")]
		public CheckPoint InitialSpawnPoint;
        /// 현재 활성화된 체크포인트(플레이어가 마지막으로 통과한 체크포인트)
        [Tooltip("현재 활성화된 체크포인트(플레이어가 마지막으로 통과한 체크포인트)")]
		public CheckPoint CurrentCheckpoint;

		[Header("진입점")]
        /// 다른 레벨에서 초기 목표로 사용할 수 있는 이 레벨의 진입점 목록입니다.
        [Tooltip("다른 레벨에서 초기 목표로 사용할 수 있는 이 레벨의 진입점 목록입니다.")]
		public Transform[] PointsOfEntry;
        				
		[Space(10)]
		[Header("인트로 및 아웃트로 기간")]
		[MMInformation("여기서 레벨의 시작과 끝에서 페이드 인 및 페이드 아웃 길이를 지정할 수 있습니다. 리스폰되기 전의 지연 시간을 결정할 수도 있습니다.", MMInformationAttribute.InformationType.Info,false)]
        /// 초기 페이드 인 기간(초)
        [Tooltip("초기 페이드 인 기간(초)")]
		public float IntroFadeDuration=1f;

		public float SpawnDelay = 0f;
        /// 레벨 끝에서 검은색으로 페이드되는 기간(초)
        [Tooltip("레벨 끝에서 검은색으로 페이드되는 지속 시간(초)")]
		public float OutroFadeDuration=1f;
        /// 이벤트를 트리거할 때 사용할 ID(사용하려는 페이더의 ID와 일치해야 함)
        [Tooltip("이벤트를 트리거할 때 사용할 ID(사용하려는 페이더의 ID와 일치해야 함)")]
		public int FaderID = 0;
        /// 인 및 아웃 페이드에 사용할 곡선
        [Tooltip("인 및 아웃 페이드에 사용할 곡선")]
		public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        /// 주인공의 죽음과 부활 사이의 시간
        [Tooltip("주인공의 죽음과 부활 사이의 시간")]
		public float RespawnDelay = 2f;

		[Header("부활 루프")]
        /// 플레이어가 죽었을 때 사망 화면이 표시되기까지의 지연 시간(초)
        [Tooltip("플레이어가 죽었을 때 사망 화면이 표시되기까지의 지연 시간(초)")]
		public float DelayBeforeDeathScreen = 1f;

		[Header("범위")]
        /// 이것이 true이면 이 레벨은 이 LevelManager에 정의된 레벨 경계를 사용합니다. 룸 시스템을 사용할 때는 false로 설정하세요.
        [Tooltip("이것이 true이면 이 레벨은 이 LevelManager에 정의된 레벨 경계를 사용합니다. 룸 시스템을 사용할 때는 false로 설정하세요.")]
		public bool UseLevelBounds = true;
        
		[Header("장면 로딩")]
        /// 대상 레벨을 로드하는 데 사용하는 방법
        [Tooltip("대상 레벨을 로드하는 데 사용하는 방법")]
		public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
        /// 사용하려는 MMSceneLoadingManager 장면의 이름
        [Tooltip("사용하려는 MMSceneLoadingManager 장면의 이름")]
		[MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
		public string LoadingSceneName = "LoadingScreen";
        /// 추가 모드에서 장면을 로드할 때 사용할 설정
        [Tooltip("추가 모드에서 장면을 로드할 때 사용할 설정")]
		[MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
		public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings; 
		
		[Header("Feedbacks")]
        /// 이것이 사실이라면 플레이어 인스턴스화 시 모든 피드백의 범위 목표를 설정하기 위해 이벤트가 트리거됩니다.
        [Tooltip("이것이 사실이라면 플레이어 인스턴스화 시 모든 피드백의 범위 목표를 설정하기 위해 이벤트가 트리거됩니다.")]
		public bool SetPlayerAsFeedbackRangeCenter = false;

        /// 레벨 제한, 카메라 및 플레이어는 이 지점을 넘지 않습니다.
        public Bounds LevelBounds {  get { return (_collider==null)? new Bounds(): _collider.bounds; } }
		public Collider BoundsCollider { get; protected set; }

        /// 레벨 시작 이후 경과된 시간
        public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}

        // 개인적인 물건
        public List<CheckPoint> Checkpoints { get; protected set; }
		public List<Character> Players { get; protected set; }

		protected DateTime _started;
		protected int _savedPoints;
		protected Collider _collider;
		protected Vector3 _initialSpawnPointPosition;

        /// <summary>
        /// Awake시 플레이어를 인스턴스화합니다.
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			_collider = this.GetComponent<Collider>();
			_initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;
        }

        /// <summary>
        /// 시작 시 종속성을 확보하고 스폰을 초기화합니다.
        /// </summary>
        protected virtual void Start()
		{
			StartCoroutine(InitializationCoroutine());
		}

		protected virtual IEnumerator InitializationCoroutine()
		{
			if (SpawnDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(SpawnDelay);    
			}

			BoundsCollider = _collider;
			InstantiatePlayableCharacters();

			if (UseLevelBounds)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider);
			}            
            
			if (Players == null || Players.Count == 0) { yield break; }

			Initialization();

			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);

            // 우리는 캐릭터의 스폰을 처리합니다
            if (Players.Count == 1)
			{
				SpawnSingleCharacter();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment();

            //우리는 페이드를 트리거합니다
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

            // 레벨 시작 이벤트를 트리거합니다
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			MMGameEvent.Trigger("CameraBound");
		}

        /// <summary>
        /// 캐릭터 생성 방법을 설명하기 위해 각 멀티플레이어 레벨 관리자가 재정의하는 방법입니다.
        /// </summary>
        protected virtual void SpawnMultipleCharacters()
		{

		}

        /// <summary>
        /// LevelManager 인스펙터의 PlayerPrefabs 목록에 지정된 캐릭터를 기반으로 재생 가능한 캐릭터를 인스턴스화합니다.
        /// </summary>
        protected virtual void InstantiatePlayableCharacters()
		{
			Players = new List<Character> ();

			if (GameManager.Instance.PersistentCharacter != null)
			{
				Players.Add(GameManager.Instance.PersistentCharacter);
				return;
			}

            // 게임 관리자에 인스턴스화해야 할 저장된 캐릭터가 있는지 확인합니다.
            if (GameManager.Instance.StoredCharacter != null)
			{
				Character newPlayer = Instantiate(GameManager.Instance.StoredCharacter, _initialSpawnPointPosition, Quaternion.identity);
				newPlayer.name = GameManager.Instance.StoredCharacter.name;
				Players.Add(newPlayer);
				return;
			}

			if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
			{
				foreach (Character character in SceneCharacters)
				{
					Players.Add(character);
				}
				return;
			}

			if (PlayerPrefabs == null) { return; }

            // 플레이어 인스턴스화
            if (PlayerPrefabs.Length != 0)
			{ 
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
					newPlayer.name = playerPrefab.name;
                    Players.Add(newPlayer);

                    GameManager.Instance.playerTypeChange(newPlayer);
					CreateManager.Instance.player = newPlayer.gameObject;

                    if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : TLevelManager에 설정한 캐릭터는 플레이어가 아닙니다. 이는 아마도 움직이지 않을 것임을 의미합니다. 프리팹의 캐릭터 구성 요소에서 이를 변경할 수 있습니다.");
					}
				}
			}
		}

        /// <summary>
        /// 장면의 모든 재생성 가능한 개체를 해당 체크포인트에 할당합니다.
        /// </summary>
        protected virtual void CheckpointAssignment()
		{
            //장면에서 재생성 가능한 모든 객체를 가져와 해당 체크포인트에 귀속시킵니다.
            IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>(true).OfType<Respawnable>();
			AutoRespawn autoRespawn;
			foreach (Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i >= 0; i--)
				{
					autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
					if (autoRespawn == null)
					{
						Checkpoints[i].AssignObjectToCheckPoint(listener);
						continue;
					}
					else
					{
						if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
						{
							Checkpoints[i].AssignObjectToCheckPoint(listener);
							continue;
						}
						else
						{
							if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
							{
								Checkpoints[i].AssignObjectToCheckPoint(listener);
								continue;
							}
							continue;
						}
					}
				}
			}
		}


        /// <summary>
        /// 현재 카메라, 포인트 번호, 시작 시간 등을 가져옵니다.
        /// </summary>
        protected virtual void Initialization()
		{
			Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
			_savedPoints =GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

        /// <summary>
        /// 장면에 플레이 가능한 캐릭터를 생성합니다.
        /// </summary>
        protected virtual void SpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], point.FacingDirection);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

			if (InitialSpawnPoint != null)
			{
				InitialSpawnPoint.SpawnPlayer(Players[0]);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

		}

        /// <summary>
        /// 플레이어를 지정된 레벨로 끌어옵니다.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public virtual void GotoLevel(string levelName)
		{
			TriggerEndLevelEvents();
			StartCoroutine(GotoLevelCo(levelName));
		}

        /// <summary>
        /// 레벨 종료 이벤트를 트리거합니다.
        /// </summary>
        public virtual void TriggerEndLevelEvents()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
			MMGameEvent.Trigger("Save");
		}

        /// <summary>
        /// 잠시 기다린 후 지정된 레벨을 로드합니다.
        /// </summary>
        /// <returns>The level co.</returns>
        /// <param name="levelName">Level name.</param>
        protected virtual IEnumerator GotoLevelCo(string levelName)
		{
			if (Players != null && Players.Count > 0)
			{ 
				foreach (Character player in Players)
				{
					player.Disable ();	
				}	    		
			}

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);
            
			if (Time.timeScale > 0.0f)
			{ 
				yield return new WaitForSeconds(OutroFadeDuration);
			}
            // GameManager(및 잠재적으로 다른 클래스)에 대해 unPause 이벤트를 트리거합니다.
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

			switch (LoadingSceneMode)
			{
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);			        
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}

        /// <summary>
        /// 플레이어를 죽입니다.
        /// </summary>
        public virtual void PlayerDead(Character playerCharacter)
		{
			if (Players.Count < 2)
			{
				StartCoroutine (PlayerDeadCo ());
			}
		}

        /// <summary>
        /// 짧은 지연 후 사망 화면 표시가 시작됩니다.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayerDeadCo()
		{
			yield return new WaitForSeconds(DelayBeforeDeathScreen);

			GUIManager.Instance.SetDeathScreen(true);
		}

        /// <summary>
        /// 리스폰을 시작합니다
        /// </summary>
        protected virtual void Respawn()
		{
			if (Players.Count < 2)
			{
				StartCoroutine(SoloModeRestart());
			}
		}

        /// <summary>
        /// 플레이어를 죽이고, 카메라를 멈추고, 포인트를 재설정하는 코루틴입니다.
        /// </summary>
        /// <returns>The player co.</returns>
        protected virtual IEnumerator SoloModeRestart()
		{
			if ((PlayerPrefabs.Length <= 0) && (SceneCharacters.Count <= 0))
			{
				yield break;
			}

            // 생명을 사용하도록 게임 관리자를 설정한 경우(최대 생명이 0보다 크다는 의미)
            if (GameManager.Instance.MaximumLives > 0)
			{
                // 우리는 생명을 잃습니다
                GameManager.Instance.LoseLife();
                // 수명이 다 되면 엑시트 신이 있는지 확인하고 그곳으로 이동합니다.
                if (GameManager.Instance.CurrentLives <= 0)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
					if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
					{
						MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
					}
				}
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
			yield return new WaitForSeconds(OutroFadeDuration);

			yield return new WaitForSeconds(RespawnDelay);
			GUIManager.Instance.SetPauseScreen(false);
			GUIManager.Instance.SetDeathScreen(false);
			MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = InitialSpawnPoint;
			}

			if (Players[0] == null)
			{
				InstantiatePlayableCharacters();
			}

			if (CurrentCheckpoint != null)
			{
				CurrentCheckpoint.SpawnPlayer(Players[0]);
			}
			else
			{
				Debug.LogWarning("LevelManager : 체크포인트나 초기 스폰 지점이 정의되지 않아 플레이어를 리스폰할 수 없습니다.");
			}

			_started = DateTime.UtcNow;
			
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            // GameManager가 포착할 수 있는 새로운 포인트 이벤트(및 이를 수신할 수 있는 다른 클래스)를 보냅니다.
            TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
			yield break;
		}


        /// <summary>
        /// 캐릭터 일시 정지를 전환합니다
        /// </summary>
        public virtual void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{
				CharacterPause characterPause = player.FindAbility<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}

        /// <summary>
        /// 캐릭터를 동결시킵니다.
        /// </summary>
        public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

        /// <summary>
        /// 캐릭터 고정을 해제합니다.
        /// </summary>
        public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

        /// <summary>
        /// 매개변수에 설정된 값으로 현재 체크포인트를 설정합니다. 이 체크포인트는 저장되어 플레이어가 죽을 경우 사용됩니다.
        /// </summary>
        /// <param name="newCheckPoint"></param>
        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			if (newCheckPoint.ForceAssignation)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}
			if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
			{
				CurrentCheckpoint = newCheckPoint;
			}
		}

        /// <summary>
        /// TopDownEngineEvents를 포착하고 그에 따라 작동하여 해당 사운드를 재생합니다.
        /// </summary>
        /// <param name="engineEvent">TopDownEngineEvent event.</param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					PlayerDead(engineEvent.OriginCharacter);
					break;
				case TopDownEngineEventTypes.RespawnStarted:
					Respawn();
					break;
			}
		}

        /// <summary>
        /// OnDisable을 사용하면 이벤트 수신이 시작됩니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// OnDisable을 사용하면 이벤트 수신이 중지됩니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}