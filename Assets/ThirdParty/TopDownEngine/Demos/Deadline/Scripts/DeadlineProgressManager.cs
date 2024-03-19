using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{	
	[System.Serializable]
    /// <summary>
    /// 완료 여부, 잠금 해제 여부, 수집할 수 있고 수집한 별 수 등 마감 시연 장면을 저장하는 직렬화 가능한 엔터티입니다.
    /// </summary>
    public class DeadlineScene
	{
		public string SceneName;
		public bool LevelComplete = false;
		public bool LevelUnlocked = false;
		public int MaxStars;
		public bool[] CollectedStars;
	}

	[System.Serializable]
    /// <summary>
    /// 진행 상황을 저장하는 데 사용되는 직렬화 가능한 엔터티: 내부 상태가 포함된 장면 목록(위 참조), 남은 생명 수 및 얻을 수 있는 양
    /// </summary>
    public class DeadlineProgress
	{
		public string StoredCharacterName;
		public int InitialMaximumLives = 0;
		public int InitialCurrentLives = 0;
		public int MaximumLives = 0;
		public int CurrentLives = 0;
		public DeadlineScene[] Scenes;
		public string[] Collectibles;
	}

    /// <summary>
    /// DeadlineProgressManager 클래스는 게임에서 진행 관리를 구현하는 방법의 예 역할을 합니다.
    /// 엔진에는 이에 대한 일반 클래스가 없습니다. 두 게임 모두 똑같은 것을 저장하려고 하지 않기 때문입니다.
    /// 하지만 이것이 어떻게 완료되었는지 보여줄 것이며, 그런 다음 이를 복사하여 자신의 클래스에 붙여넣을 수 있습니다(또는 원하는 대로 이 클래스를 확장할 수 있습니다).
    /// </summary>
    public class DeadlineProgressManager : MMSingleton<DeadlineProgressManager>, MMEventListener<TopDownEngineEvent>, MMEventListener<TopDownEngineStarEvent>
	{
		public int InitialMaximumLives { get; set; }
		public int InitialCurrentLives { get; set; }

		[Header("Characters")] 
		public Character Naomi;
		public Character Jules;

		/// the list of scenes that we'll want to consider for our game
		[Tooltip("게임에서 고려하고 싶은 장면 목록")]
		public DeadlineScene[] Scenes;

		[MMInspectorButton("CreateSaveGame")]
		/// A test button to test creating the save file
		public bool CreateSaveGameBtn;

		/// the current amount of collected stars
		public int CurrentStars { get; protected set; }
		
		public List<string> FoundCollectibles { get; protected set; }

		protected const string _saveFolderName = "DeadlineProgressData";
		protected const string _saveFileName = "Progress.data";

		/// <summary>
		/// On awake, we load our progress and initialize our stars counter
		/// </summary>
		protected override void Awake()
		{
			base.Awake ();
			LoadSavedProgress ();
			InitializeStars ();
			if (FoundCollectibles == null)
			{
				FoundCollectibles = new List<string> ();
			}
		}

		/// <summary>
		/// When a level is completed, we update our progress
		/// </summary>
		protected virtual void LevelComplete()
		{
			for (int i = 0; i < Scenes.Length; i++)
			{
				if (Scenes[i].SceneName == SceneManager.GetActiveScene().name)
				{
					Scenes[i].LevelComplete = true;
					Scenes[i].LevelUnlocked = true;
					if (i < Scenes.Length - 1)
					{
						Scenes [i + 1].LevelUnlocked = true;
					}
				}
			}
		}

		/// <summary>
		/// Goes through all the scenes in our progress list, and updates the collected stars counter
		/// </summary>
		protected virtual void InitializeStars()
		{
			foreach (DeadlineScene scene in Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					int stars = 0;
					foreach (bool star in scene.CollectedStars)
					{
						if (star) { stars++; }
					}
					CurrentStars = stars;
				}
			}
		}

		/// <summary>
		/// Saves the progress to a file
		/// </summary>
		protected virtual void SaveProgress()
		{
			DeadlineProgress progress = new DeadlineProgress ();
			progress.StoredCharacterName = GameManager.Instance.StoredCharacter.name;
			progress.MaximumLives = GameManager.Instance.MaximumLives;
			progress.CurrentLives = GameManager.Instance.CurrentLives;
			progress.InitialMaximumLives = InitialMaximumLives;
			progress.InitialCurrentLives = InitialCurrentLives;
			progress.Scenes = Scenes;
			if (FoundCollectibles != null)
			{
				progress.Collectibles = FoundCollectibles.ToArray();	
			}

			MMSaveLoadManager.Save(progress, _saveFileName, _saveFolderName);
		}

		/// <summary>
		/// A test method to create a test save file at any time from the inspector
		/// </summary>
		protected virtual void CreateSaveGame()
		{
			SaveProgress();
		}

		/// <summary>
		/// Loads the saved progress into memory
		/// </summary>
		protected virtual void LoadSavedProgress()
		{
			DeadlineProgress progress = (DeadlineProgress)MMSaveLoadManager.Load(typeof(DeadlineProgress), _saveFileName, _saveFolderName);
			if (progress != null)
			{
				GameManager.Instance.StoredCharacter = (progress.StoredCharacterName == Jules.name) ? Jules : Naomi;
				GameManager.Instance.MaximumLives = progress.MaximumLives;
				GameManager.Instance.CurrentLives = progress.CurrentLives;
				InitialMaximumLives = progress.InitialMaximumLives;
				InitialCurrentLives = progress.InitialCurrentLives;
				Scenes = progress.Scenes;
				if (progress.Collectibles != null)
				{
					FoundCollectibles = new List<string>(progress.Collectibles);	
				}
			}
			else
			{
				InitialMaximumLives = GameManager.Instance.MaximumLives;
				InitialCurrentLives = GameManager.Instance.CurrentLives;
			}
		}

		public virtual void FindCollectible(string collectibleName)
		{
			FoundCollectibles.Add(collectibleName);
		}

		/// <summary>
		/// When we grab a star event, we update our scene status accordingly
		/// </summary>
		/// <param name="deadlineStarEvent">Deadline star event.</param>
		public virtual void OnMMEvent(TopDownEngineStarEvent deadlineStarEvent)
		{
			foreach (DeadlineScene scene in Scenes)
			{
				if (scene.SceneName == deadlineStarEvent.SceneName)
				{
					scene.CollectedStars [deadlineStarEvent.StarID] = true;
					CurrentStars++;
				}
			}
		}

		/// <summary>
		/// When we grab a level complete event, we update our status, and save our progress to file
		/// </summary>
		/// <param name="gameEvent">Game event.</param>
		public virtual void OnMMEvent(TopDownEngineEvent gameEvent)
		{
			switch (gameEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelComplete:
					LevelComplete ();
					SaveProgress ();
					break;
				case TopDownEngineEventTypes.GameOver:
					GameOver ();
					break;
			}
		} 

		/// <summary>
		/// This method describes what happens when the player loses all lives. In this case, we reset its progress and all lives will be reset.
		/// </summary>
		protected virtual void GameOver()
		{
			ResetProgress ();
			ResetLives ();
		}

		/// <summary>
		/// Resets the number of lives to its initial values
		/// </summary>
		protected virtual void ResetLives()
		{
			GameManager.Instance.MaximumLives = InitialMaximumLives;
			GameManager.Instance.CurrentLives = InitialCurrentLives;
		}

		/// <summary>
		/// A method used to remove all save files associated to progress
		/// </summary>
		public virtual void ResetProgress()
		{
			MMSaveLoadManager.DeleteSaveFolder (_saveFolderName);			
		}

		/// <summary>
		/// OnEnable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineStarEvent> ();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineStarEvent> ();
			this.MMEventStopListening<TopDownEngineEvent>();
		}		
	}
}