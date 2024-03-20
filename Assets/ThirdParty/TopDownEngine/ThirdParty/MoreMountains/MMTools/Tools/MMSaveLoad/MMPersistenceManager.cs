using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성요소를 장면에 추가하면 IMMPersible 인터페이스를 구현하는 객체의 상태를 저장하고 로드할 수 있습니다.
    /// 이 인터페이스를 구현하는 고유한 클래스를 만들거나 이 패키지와 함께 제공되는 MMPercious 클래스를 사용할 수 있습니다.
    /// 변환 데이터(위치, 회전, 크기 조정) 및 활성 상태를 저장합니다.
    /// 저장 및 로드 트리거는 이벤트를 통해 수행되며 관리자는 데이터가 로드되거나 저장될 때마다 이벤트를 내보냅니다.
    /// </summary>
    public class MMPersistenceManager : MMPersistentSingleton<MMPersistenceManager>, MMEventListener<MMGameEvent>
	{
		[Header("Persistence")]
		/// A persistence ID used to identify the data associated to this manager.
		/// Usually you'll want to leave this to its default value.
		[Tooltip("이 관리자와 연관된 데이터를 식별하는 데 사용되는 지속성 ID입니다. 일반적으로 이를 기본값으로 두는 것이 좋습니다.")]
		public string PersistenceID = "MMPersistency";

		[Header("Events")]
		/// whether or not this manager should listen for save events. If you set this to false, you'll have to call SaveToMemory or SaveFromMemoryToFile manually
		[Tooltip("이 관리자가 저장 이벤트를 수신해야 하는지 여부입니다. 이를 false로 설정하면 SaveToMemory 또는 SaveFromMemoryToFile을 수동으로 호출해야 합니다.")] 
		public bool ListenForSaveEvents = true;
		/// whether or not this manager should listen for load events. If you set this to false, you'll have to call LoadFromMemory or LoadFromFileToMemory manually
		[Tooltip("이 관리자가 로드 이벤트를 수신해야 하는지 여부입니다. 이를 false로 설정하면 LoadFromMemory 또는 LoadFromFileToMemory를 수동으로 호출해야 합니다.")] 
		public bool ListenForLoadEvents = true;
		/// whether or not this manager should listen for save to memory events. If you set this to false, you'll have to call SaveToMemory manually
		[Tooltip("이 관리자가 메모리 저장 이벤트를 수신해야 하는지 여부입니다. 이를 false로 설정하면 SaveToMemory를 수동으로 호출해야 합니다.")]
		public bool ListenForSaveToMemoryEvents = true;
		/// whether or not this manager should listen for load from memory events. If you set this to false, you'll have to call LoadFromMemory manually
		[Tooltip("이 관리자가 메모리 이벤트에서 로드를 수신해야 하는지 여부입니다. 이를 false로 설정하면 LoadFromMemory를 수동으로 호출해야 합니다.")]
		public bool ListenForLoadFromMemoryEvents = true;
		/// whether or not this manager should listen for save to file events. If you set this to false, you'll have to call SaveFromMemoryToFile manually
		[Tooltip("이 관리자가 파일 이벤트 저장을 수신해야 하는지 여부입니다. 이를 false로 설정하면 SaveFromMemoryToFile을 수동으로 호출해야 합니다.")]
		public bool ListenForSaveToFileEvents = true;
		/// whether or not this manager should listen for load from file events. If you set this to false, you'll have to call LoadFromFileToMemory manually
		[Tooltip("이 관리자가 파일 이벤트에서 로드를 수신해야 하는지 여부입니다. 이를 false로 설정하면 LoadFromFileToMemory를 수동으로 호출해야 합니다.")]
		public bool ListenForLoadFromFileEvents = true;
		/// whether or not this manager should save data to file on save events
		[Tooltip("이 관리자가 저장 이벤트 시 데이터를 파일에 저장해야 하는지 여부")]
		public bool SaveToFileOnSaveEvents = true;
		/// whether or not this manager should load data from file on load events
		[Tooltip("이 관리자가 로드 이벤트 시 파일에서 데이터를 로드해야 하는지 여부")]
		public bool LoadFromFileOnLoadEvents = true;
		
		/// the debug buttons below are only meant to be used at runtime
		[Header("Debug Buttons (Only at Runtime)")]
		[MMInspectorButton("SaveToMemory")]
		public bool SaveToMemoryButton;
		[MMInspectorButton("LoadFromMemory")]
		public bool LoadFromMemoryButton;
		[MMInspectorButton("SaveFromMemoryToFile")]
		public bool SaveToFileButton;
		[MMInspectorButton("LoadFromFileToMemory")]
		public bool LoadFromFileButton;
		[MMInspectorButton("DeletePersistencyFile")]
		public bool DeletePersistencyFileButton;

		public DictionaryStringSceneData SceneDatas;
		
		public static string _resourceItemPath = "Persistence/";
		public static string _saveFolderName = "MMTools/";
		public static string _saveFileExtension = ".persistence";

		protected string _currentSceneName;

		#region INITIALIZATION

			/// <summary>
			/// On Awake we initialize our dictionary
			/// </summary>
			protected override void Awake()
			{
				base.Awake();
				SceneDatas = new DictionaryStringSceneData();
			}

		#endregion

		#region SAVE_AND_LOAD

			/// <summary>
			/// Saves data from objects that need saving to memory
			/// </summary>
			public virtual void SaveToMemory()
			{
				ComputeCurrentSceneName();

				SceneDatas.Remove(_currentSceneName);
				
				MMPersistenceSceneData sceneData = new MMPersistenceSceneData();
				sceneData.ObjectDatas = new DictionaryStringString();
				
				IMMPersistent[] persistents = FindAllPersistentObjects();
				
				foreach (IMMPersistent persistent in persistents)
				{
					if (persistent.ShouldBeSaved())
					{
						sceneData.ObjectDatas.Add(persistent.GetGuid(), persistent.OnSave());	
					}
				}

				SceneDatas.Add(_currentSceneName, sceneData);

				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataSavedToMemory, PersistenceID);
			}

			/// <summary>
			/// Loads data from memory and applies it to all objects that need it
			/// </summary>
			public virtual void LoadFromMemory()
			{
				ComputeCurrentSceneName();
				
				if (!SceneDatas.TryGetValue(_currentSceneName, out MMPersistenceSceneData sceneData))
				{
					return;
				}
				
				if (sceneData.ObjectDatas == null)
				{
					return;
				}
				
				IMMPersistent[] persistents = FindAllPersistentObjects();
				foreach (IMMPersistent persistent in persistents)
				{
					if (sceneData.ObjectDatas.TryGetValue(persistent.GetGuid(), out string data))
					{
						persistent.OnLoad(sceneData.ObjectDatas[persistent.GetGuid()]);
					}
				}
				
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataLoadedFromMemory, PersistenceID);
			}

			/// <summary>
			/// Saves data from memory to a file
			/// </summary>
			public virtual void SaveFromMemoryToFile()
			{
				MMPersistenceManagerData saveData = new MMPersistenceManagerData();
				saveData.PersistenceID = PersistenceID;
				saveData.SaveDate = DateTime.Now.ToString();
				saveData.SceneDatas = SceneDatas;
				MMSaveLoadManager.Save(saveData, DetermineSaveName(), _saveFolderName);
				
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataSavedFromMemoryToFile, PersistenceID);
			}

			/// <summary>
			/// Loads data from file and stores it in memory
			/// </summary>
			public virtual void LoadFromFileToMemory()
			{
				MMPersistenceManagerData saveData = (MMPersistenceManagerData)MMSaveLoadManager.Load(typeof(MMPersistenceManagerData), DetermineSaveName(), _saveFolderName);
				if ((saveData != null) && (saveData.SceneDatas != null))
				{
					SceneDatas = new DictionaryStringSceneData();
					SceneDatas = saveData.SceneDatas;	
				}
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataLoadedFromFileToMemory, PersistenceID);
			}
			
			/// <summary>
			/// On Save, we save to memory and to file if needed
			/// </summary>
			public virtual void Save()
			{
				SaveToMemory();
				if (SaveToFileOnSaveEvents)
				{
					SaveFromMemoryToFile();
				}
			}

			/// <summary>
			/// On Load, we load from memory and from file if needed
			/// </summary>
			public virtual void Load()
			{
				if (LoadFromFileOnLoadEvents)
				{
					LoadFromFileToMemory();
				}
				LoadFromMemory();
			}

		#endregion

		#region RESET

			/// <summary>
			/// Deletes all persistence data for the specified scene
			/// </summary>
			/// <param name="sceneName"></param>
			public virtual void DeletePersistencyMemoryForScene(string sceneName)
			{
				if (!SceneDatas.TryGetValue(_currentSceneName, out MMPersistenceSceneData sceneData))
				{
					return;
				}
				SceneDatas.Remove(sceneName);
			}

			/// <summary>
			/// Deletes persistence data from memory and on file for this persistence manager
			/// </summary>
			public virtual void ResetPersistence()
			{
				DeletePersistenceMemory();
				DeletePersistenceFile();
			}

			/// <summary>
			/// Deletes all persistence data stored in this persistence manager's memory
			/// </summary>
			public virtual void DeletePersistenceMemory()
			{
				SceneDatas = new DictionaryStringSceneData();
			}
				
			/// <summary>
			/// Deletes the save file for this persistence manager
			/// </summary>
			public virtual void DeletePersistenceFile()
			{
				MMSaveLoadManager.DeleteSave(DetermineSaveName(), _saveFolderName);
				Debug.LogFormat("Persistence save file deleted");
			}

		#endregion

		#region HELPERS

			/// <summary>
			/// Finds all objects in the scene that implement IMMPersistent and may need saving
			/// </summary>
			/// <returns></returns>
			protected virtual IMMPersistent[] FindAllPersistentObjects()
			{
				return FindObjectsOfType<MonoBehaviour>(true).OfType<IMMPersistent>().ToArray();
			}

			/// <summary>
			/// Grabs the current scene's name and stores it 
			/// </summary>
			protected virtual void ComputeCurrentSceneName()
			{
				_currentSceneName = SceneManager.GetActiveScene().name;
			}

			/// <summary>
			/// Determines the name of the file to write to store persistence data
			/// </summary>
			/// <returns></returns>
			protected virtual string DetermineSaveName()
			{
				return gameObject.name + "_" + PersistenceID + _saveFileExtension;
			}

		#endregion

		#region EVENTS

			/// <summary>
			/// When we get a MMEvent, we filter on its name and invoke the appropriate methods if needed
			/// </summary>
			/// <param name="gameEvent"></param>
			public virtual void OnMMEvent(MMGameEvent gameEvent)
			{
				if ((gameEvent.EventName == "Save") && ListenForSaveEvents)
				{
					Save();
				}
				if ((gameEvent.EventName == "Load") && ListenForLoadEvents)
				{
					Load();
				}
				if ((gameEvent.EventName == "SaveToMemory") && ListenForSaveToMemoryEvents)
				{
					SaveToMemory();
				}
				if ((gameEvent.EventName == "LoadFromMemory") && ListenForLoadFromMemoryEvents)
				{
					LoadFromMemory();
				}
				if ((gameEvent.EventName == "SaveToFile") && ListenForSaveToFileEvents)
				{
					SaveFromMemoryToFile();
				}
				if ((gameEvent.EventName == "LoadFromFile") && ListenForLoadFromFileEvents)
				{
					LoadFromFileToMemory();
				}
			}
			
			/// <summary>
			/// On enable, we start listening for MMGameEvents
			/// </summary>
			protected virtual void OnEnable()
			{
				this.MMEventStartListening<MMGameEvent>();
			}
			
			/// <summary>
			/// On enable, we stop listening for MMGameEvents
			/// </summary>
			protected virtual void OnDisable()
			{
				this.MMEventStopListening<MMGameEvent>();
			}

		#endregion
	}	
}

