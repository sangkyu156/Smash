using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.IO;
using MoreMountains.InventoryEngine;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �÷��̾ �����ϰ�, üũ����Ʈ�� ó���ϰ� �ٽ� �����մϴ�.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Level Manager")]
	public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
	{
        /// �÷��̾�� ���ϴ� ������ �ǹ�
        [Header("Instantiate Characters")]
		[MMInformation("LevelManager�� ����/������, üũ����Ʈ ����, ���� ��� ó���� ����մϴ�. ���⿡�� ������ ���� �ϳ� �̻��� �÷��� ������ ĳ���͸� ������ �� �ֽ��ϴ�.", MMInformationAttribute.InformationType.Info,false)]
        /// �÷��̾� ID�� �ڵ����� �ο��Ǿ�� �ϴ���(�Ϲ������� yes)
        [Tooltip("�÷��̾� ID�� �ڵ����� �ο��Ǿ�� �ϴ���(�Ϲ������� yes)")]
		public bool AutoAttributePlayerIDs = true;
        /// �ν��Ͻ�ȭ�� �÷��̾� ������ ���
        [Tooltip("�� ���� �����ڰ� ���� �� �ν��Ͻ�ȭ�� �÷��̾� ������ ���")]
		public Character[] PlayerPrefabs;//������ �ΰ��� �ȿ� �ִ� �÷��̾� �ƴ�! �������ϻ�

        [Header("�̹� ��鿡 �����ϴ� ĳ����")]
		[MMInformation("LevelManager�� ĳ���͸� �ν��Ͻ�ȭ�ϵ��� �ϴ� ���� ���������, ��� ��鿡 �̹� �����ϵ��� �Ϸ��� �Ʒ� ��Ͽ� ���ε��ϱ⸸ �ϸ� �˴ϴ�.", MMInformationAttribute.InformationType.Info, false)]
        /// ��Ÿ�� ���� ��鿡 �̹� �����ϴ� ĳ���� ����Դϴ�. �� ����� ä������ PlayerPrefabs�� ���õ˴ϴ�.
        [Tooltip("��Ÿ�� ���� ��鿡 �̹� �����ϴ� ĳ���� ����Դϴ�. �� ����� ä������ PlayerPrefabs�� ���õ˴ϴ�.")]
		public List<Character> SceneCharacters;

		[Header("üũ����Ʈ")]
        /// �������� �������� ���� ��� �ʱ� ���� �������� ����� üũ����Ʈ
        [Tooltip("�������� �������� ���� ��� �ʱ� ���� �������� ����� üũ����Ʈ")]
		public CheckPoint InitialSpawnPoint;
        /// ���� Ȱ��ȭ�� üũ����Ʈ(�÷��̾ ���������� ����� üũ����Ʈ)
        [Tooltip("���� Ȱ��ȭ�� üũ����Ʈ(�÷��̾ ���������� ����� üũ����Ʈ)")]
		public CheckPoint CurrentCheckpoint;

		[Header("������")]
        /// �ٸ� �������� �ʱ� ��ǥ�� ����� �� �ִ� �� ������ ������ ����Դϴ�.
        [Tooltip("�ٸ� �������� �ʱ� ��ǥ�� ����� �� �ִ� �� ������ ������ ����Դϴ�.")]
		public Transform[] PointsOfEntry;
        				
		[Space(10)]
		[Header("��Ʈ�� �� �ƿ�Ʈ�� �Ⱓ")]
		[MMInformation("���⼭ ������ ���۰� ������ ���̵� �� �� ���̵� �ƿ� ���̸� ������ �� �ֽ��ϴ�. �������Ǳ� ���� ���� �ð��� ������ ���� �ֽ��ϴ�.", MMInformationAttribute.InformationType.Info,false)]
        /// �ʱ� ���̵� �� �Ⱓ(��)
        [Tooltip("�ʱ� ���̵� �� �Ⱓ(��)")]
		public float IntroFadeDuration=1f;

		public float SpawnDelay = 0f;
        /// ���� ������ ���������� ���̵�Ǵ� �Ⱓ(��)
        [Tooltip("���� ������ ���������� ���̵�Ǵ� ���� �ð�(��)")]
		public float OutroFadeDuration=1f;
        /// �̺�Ʈ�� Ʈ������ �� ����� ID(����Ϸ��� ���̴��� ID�� ��ġ�ؾ� ��)
        [Tooltip("�̺�Ʈ�� Ʈ������ �� ����� ID(����Ϸ��� ���̴��� ID�� ��ġ�ؾ� ��)")]
		public int FaderID = 0;
        /// �� �� �ƿ� ���̵忡 ����� �
        [Tooltip("�� �� �ƿ� ���̵忡 ����� �")]
		public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        /// ���ΰ��� ������ ��Ȱ ������ �ð�
        [Tooltip("���ΰ��� ������ ��Ȱ ������ �ð�")]
		public float RespawnDelay = 2f;

		[Header("��Ȱ ����")]
        /// �÷��̾ �׾��� �� ��� ȭ���� ǥ�õǱ������ ���� �ð�(��)
        [Tooltip("�÷��̾ �׾��� �� ��� ȭ���� ǥ�õǱ������ ���� �ð�(��)")]
		public float DelayBeforeDeathScreen = 1f;

		[Header("����")]
        /// �̰��� true�̸� �� ������ �� LevelManager�� ���ǵ� ���� ��踦 ����մϴ�. �� �ý����� ����� ���� false�� �����ϼ���.
        [Tooltip("�̰��� true�̸� �� ������ �� LevelManager�� ���ǵ� ���� ��踦 ����մϴ�. �� �ý����� ����� ���� false�� �����ϼ���.")]
		public bool UseLevelBounds = true;
        
		[Header("��� �ε�")]
        /// ��� ������ �ε��ϴ� �� ����ϴ� ���
        [Tooltip("��� ������ �ε��ϴ� �� ����ϴ� ���")]
		public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
        /// ����Ϸ��� MMSceneLoadingManager ����� �̸�
        [Tooltip("����Ϸ��� MMSceneLoadingManager ����� �̸�")]
		[MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
		public string LoadingSceneName = "LoadingScreen";
        /// �߰� ��忡�� ����� �ε��� �� ����� ����
        [Tooltip("�߰� ��忡�� ����� �ε��� �� ����� ����")]
		[MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
		public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings; 
		
		[Header("Feedbacks")]
        /// �̰��� ����̶�� �÷��̾� �ν��Ͻ�ȭ �� ��� �ǵ���� ���� ��ǥ�� �����ϱ� ���� �̺�Ʈ�� Ʈ���ŵ˴ϴ�.
        [Tooltip("�̰��� ����̶�� �÷��̾� �ν��Ͻ�ȭ �� ��� �ǵ���� ���� ��ǥ�� �����ϱ� ���� �̺�Ʈ�� Ʈ���ŵ˴ϴ�.")]
		public bool SetPlayerAsFeedbackRangeCenter = false;

        /// ���� ����, ī�޶� �� �÷��̾�� �� ������ ���� �ʽ��ϴ�.
        public Bounds LevelBounds {  get { return (_collider==null)? new Bounds(): _collider.bounds; } }
		public Collider BoundsCollider { get; protected set; }

        /// ���� ���� ���� ����� �ð�
        public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}

        // �������� ����
        public List<CheckPoint> Checkpoints { get; protected set; }
		public List<Character> Players { get; protected set; }//������ �ΰ��� �ȿ� �ִ� �÷��̾�

		protected DateTime _started;
		protected int _savedPoints;
		protected Collider _collider;
		protected Vector3 _initialSpawnPointPosition;

		//�������� ����
		public bool isClear = false;
		public GameObject startGuide;
        public bool HelperPopupIsOpen = false;

        /// <summary>
        /// Awake�� �÷��̾ �ν��Ͻ�ȭ�մϴ�.
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			_collider = this.GetComponent<Collider>();
			_initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;
        }

        /// <summary>
        /// ���� �� ���Ӽ��� Ȯ���ϰ� ������ �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void Start()
		{
			isClear = false;
            StartCoroutine(InitializationCoroutine());

            if (SceneManager.GetActiveScene().name == "Village" && DataManager.Instance.datas.firstVillage == false)
            {
                InputManager.Instance.InputDetectionActive = false;
                startGuide.SetActive(true);
				HelperPopupIsOpen = true;
                DataManager.Instance.datas.firstVillage = true;
                DataManager.Instance.DataSave();
				//�ʱ� ������ �־��ִ� �̺�Ʈ �߻�
				MMGameEvent.Trigger("StartItemAdd");
            }
            else if (SceneManager.GetActiveScene().name == "LevelSelect2" && DataManager.Instance.datas.firstLevelSelect == false)
            {
                InputManager.Instance.InputDetectionActive = false;
                startGuide.SetActive(true);
                DataManager.Instance.datas.firstLevelSelect = true;
                DataManager.Instance.DataSave();
            }
            else if (SceneManager.GetActiveScene().name == "Battlefield01" && DataManager.Instance.datas.firstBattlefield == false)
            {
                InputManager.Instance.InputDetectionActive = false;
                startGuide.SetActive(true);
                HelperPopupIsOpen = true;
                DataManager.Instance.datas.firstBattlefield = true;
                DataManager.Instance.DataSave();
            }
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

            // �츮�� ĳ������ ������ ó���մϴ�
            if (Players.Count == 1)
			{
				SpawnSingleCharacter();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment();

            //�츮�� ���̵带 Ʈ�����մϴ�
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

            // ���� ���� �̺�Ʈ�� Ʈ�����մϴ�
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			MMGameEvent.Trigger("CameraBound");

            if (SceneManager.GetActiveScene().name != "LevelSelect2")
                DataManager.Instance.PlayerRefresh();
        }

        /// <summary>
        /// ĳ���� ���� ����� �����ϱ� ���� �� ��Ƽ�÷��̾� ���� �����ڰ� �������ϴ� ����Դϴ�.
        /// </summary>
        protected virtual void SpawnMultipleCharacters()
		{

		}

        /// <summary>
        /// LevelManager �ν������� PlayerPrefabs ��Ͽ� ������ ĳ���͸� ������� ��� ������ ĳ���͸� �ν��Ͻ�ȭ�մϴ�.
        /// </summary>
        protected virtual void InstantiatePlayableCharacters()
		{
			Players = new List<Character> ();

			if (GameManager.Instance.PersistentCharacter != null)
			{
				Players.Add(GameManager.Instance.PersistentCharacter);
				return;
			}

            // ���� �Ŵ����� �ν��Ͻ�ȭ�ؾ� �� ����� ĳ���Ͱ� �ִ��� Ȯ���մϴ�.
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

            // �÷��̾� �ν��Ͻ�ȭ
            if (PlayerPrefabs.Length != 0)
			{ 
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
					newPlayer.name = playerPrefab.name;
                    Players.Add(newPlayer);

                    if (SceneManager.GetActiveScene().name != "LevelSelect2")
					{
                        //GameManager.Instance.playerTypeChange(newPlayer);
                        CreateManager.Instance.player = newPlayer.gameObject;
                    }

                    if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : TLevelManager�� ������ ĳ���ʹ� �÷��̾ �ƴմϴ�. �̴� �Ƹ��� �������� ���� ������ �ǹ��մϴ�. �������� ĳ���� ���� ��ҿ��� �̸� ������ �� �ֽ��ϴ�.");
					}
				}
			}
		}

        /// <summary>
        /// ����� ��� ����� ������ ��ü�� �ش� üũ����Ʈ�� �Ҵ��մϴ�.
        /// </summary>
        protected virtual void CheckpointAssignment()
		{
            //��鿡�� ����� ������ ��� ��ü�� ������ �ش� üũ����Ʈ�� �ͼӽ�ŵ�ϴ�.
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
        /// ���� ī�޶�, ����Ʈ ��ȣ, ���� �ð� ���� �����ɴϴ�.
        /// </summary>
        protected virtual void Initialization()
		{
			Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
			_savedPoints =GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

        /// <summary>
        /// ��鿡 �÷��� ������ ĳ���͸� �����մϴ�.
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

		//�������� Ŭ���� ����Ȯ���� ���������ε��� ��ȯ �ϴ� �Լ�
		int SetPointOfEntryIndex()
		{
			int index = 0;
			bool[] stage = new bool[51];
			stage[0] = DataManager.Instance.datas.stage0;
            stage[1] = DataManager.Instance.datas.stage1;
            stage[2] = DataManager.Instance.datas.stage2;
            stage[3] = DataManager.Instance.datas.stage3;
            stage[4] = DataManager.Instance.datas.stage4;
            stage[5] = DataManager.Instance.datas.stage5;
            stage[6] = DataManager.Instance.datas.stage6;
            stage[7] = DataManager.Instance.datas.stage7;
            stage[8] = DataManager.Instance.datas.stage8;
            stage[9] = DataManager.Instance.datas.stage9;
            stage[10] = DataManager.Instance.datas.stage10;
            stage[11] = DataManager.Instance.datas.stage11;
            stage[12] = DataManager.Instance.datas.stage12;
            stage[13] = DataManager.Instance.datas.stage13;
            stage[14] = DataManager.Instance.datas.stage14;
            stage[15] = DataManager.Instance.datas.stage15;
            stage[16] = DataManager.Instance.datas.stage16;
            stage[17] = DataManager.Instance.datas.stage17;
            stage[18] = DataManager.Instance.datas.stage18;
            stage[19] = DataManager.Instance.datas.stage19;
            stage[20] = DataManager.Instance.datas.stage20;
            stage[21] = DataManager.Instance.datas.stage21;
            stage[22] = DataManager.Instance.datas.stage22;
            stage[23] = DataManager.Instance.datas.stage23;
            stage[24] = DataManager.Instance.datas.stage24;
            stage[25] = DataManager.Instance.datas.stage25;
            stage[26] = DataManager.Instance.datas.stage26;
            stage[27] = DataManager.Instance.datas.stage27;
            stage[28] = DataManager.Instance.datas.stage28;
            stage[29] = DataManager.Instance.datas.stage29;
            stage[30] = DataManager.Instance.datas.stage30;
            stage[31] = DataManager.Instance.datas.stage31;
            stage[32] = DataManager.Instance.datas.stage32;
            stage[33] = DataManager.Instance.datas.stage33;
            stage[34] = DataManager.Instance.datas.stage34;
            stage[35] = DataManager.Instance.datas.stage35;
            stage[36] = DataManager.Instance.datas.stage36;
            stage[37] = DataManager.Instance.datas.stage37;
            stage[38] = DataManager.Instance.datas.stage38;
            stage[39] = DataManager.Instance.datas.stage39;
            stage[40] = DataManager.Instance.datas.stage40;
            stage[41] = DataManager.Instance.datas.stage41;
            stage[42] = DataManager.Instance.datas.stage42;
            stage[43] = DataManager.Instance.datas.stage43;
            stage[44] = DataManager.Instance.datas.stage44;
            stage[45] = DataManager.Instance.datas.stage45;
            stage[46] = DataManager.Instance.datas.stage46;
            stage[47] = DataManager.Instance.datas.stage47;
            stage[48] = DataManager.Instance.datas.stage48;
            stage[49] = DataManager.Instance.datas.stage49;
            stage[50] = DataManager.Instance.datas.stage50;

			for (int i = 1; i <= stage.Length; i++)
			{
				if (stage[i] == true)
					index = i;
				else
					return index;
			}

            return index;
		}

        /// <summary>
        /// �÷��̾ ������ ������ ����ɴϴ�.
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public virtual void GotoLevel(string levelName)
		{
			TriggerEndLevelEvents();
			StartCoroutine(GotoLevelCo(levelName));
		}

        /// <summary>
        /// ���� ���� �̺�Ʈ�� Ʈ�����մϴ�.
        /// </summary>
        public virtual void TriggerEndLevelEvents()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
			if(isClear == true)
                MMGameEvent.Trigger("Save");
        }

        /// <summary>
        /// ��� ��ٸ� �� ������ ������ �ε��մϴ�.
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
            // GameManager(�� ���������� �ٸ� Ŭ����)�� ���� unPause �̺�Ʈ�� Ʈ�����մϴ�.
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
        /// �÷��̾ ���Դϴ�.
        /// </summary>
        public virtual void PlayerDead(Character playerCharacter)
		{
			if (Players.Count < 2)
			{
				StartCoroutine (PlayerDeadCo ());
			}
		}

        /// <summary>
        /// ª�� ���� �� ��� ȭ�� ǥ�ð� ���۵˴ϴ�.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayerDeadCo()
		{
			yield return new WaitForSeconds(DelayBeforeDeathScreen);

			GUIManager.Instance.SetDeathScreen(true);
			Time.timeScale = 0;
		}

        /// <summary>
        /// �������� �����մϴ�
        /// </summary>
        protected virtual void Respawn()
		{
			if (Players.Count < 2)
			{
				StartCoroutine(SoloModeRestart());
			}
		}

        /// <summary>
        /// �÷��̾ ���̰�, ī�޶� ���߰�, ����Ʈ�� �缳���ϴ� �ڷ�ƾ�Դϴ�.
        /// </summary>
        /// <returns>The player co.</returns>
        protected virtual IEnumerator SoloModeRestart()
		{
			if ((PlayerPrefabs.Length <= 0) && (SceneCharacters.Count <= 0))
			{
				yield break;
			}

            // ������ ����ϵ��� ���� �����ڸ� ������ ���(�ִ� ������ 0���� ũ�ٴ� �ǹ�)
            if (GameManager.Instance.MaximumLives > 0)
			{
                // �츮�� ������ �ҽ��ϴ�
                GameManager.Instance.LoseLife();
                // ������ �� �Ǹ� ����Ʈ ���� �ִ��� Ȯ���ϰ� �װ����� �̵��մϴ�.
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
				Debug.LogWarning("LevelManager : üũ����Ʈ�� �ʱ� ���� ������ ���ǵ��� �ʾ� �÷��̾ �������� �� �����ϴ�.");
			}

			_started = DateTime.UtcNow;
			
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            // GameManager�� ������ �� �ִ� ���ο� ����Ʈ �̺�Ʈ(�� �̸� ������ �� �ִ� �ٸ� Ŭ����)�� �����ϴ�.
            TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
			yield break;
		}


        /// <summary>
        /// ĳ���� �Ͻ� ������ ��ȯ�մϴ�
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
        /// ĳ���͸� �����ŵ�ϴ�.
        /// </summary>
        public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

        /// <summary>
        /// ĳ���� ������ �����մϴ�.
        /// </summary>
        public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

        /// <summary>
        /// �Ű������� ������ ������ ���� üũ����Ʈ�� �����մϴ�. �� üũ����Ʈ�� ����Ǿ� �÷��̾ ���� ��� ���˴ϴ�.
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
        /// TopDownEngineEvents�� �����ϰ� �׿� ���� �۵��Ͽ� �ش� ���带 ����մϴ�.
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
        /// OnDisable�� ����ϸ� �̺�Ʈ ������ ���۵˴ϴ�.
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
            if (SceneManager.GetActiveScene().name == "Village")
			{
				DataManager.Instance.FindSetStore();
            }

        }

        /// <summary>
        /// OnDisable�� ����ϸ� �̺�Ʈ ������ �����˴ϴ�.
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}