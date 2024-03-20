using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 사용하면 레벨에 있는 방의 경계를 정의할 수 있습니다.
    /// 룸은 레벨을 여러 부분으로 나누고 싶을 때 유용합니다(예를 들어 Super Metroid 또는 Hollow Knight를 생각해 보세요).
    /// 이 방에는 자체 가상 카메라와 크기를 정의하는 제한 장치가 필요합니다.
    /// 제한자는 방을 정의하는 충돌체와 다르다는 점에 유의하세요.
    /// KoalaRooms 데모 장면에서 실행 중인 방의 예를 볼 수 있습니다.
    /// </summary>
    public class Room : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		public enum Modes { TwoD, ThreeD }

		/// the collider for this room
		public Vector3 RoomColliderCenter
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.center;
				}
				else
				{
					return _roomCollider.bounds.center;
				}
			}
		}
        
		public Vector3 RoomColliderSize
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.size;
				}
				else
				{
					return _roomCollider.bounds.size;
				}
			}
		}

		public Bounds RoomBounds
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds;
				}
				else
				{
					return _roomCollider.bounds;
				}
			}
		}

		[Header("Mode")]
		/// whether this room is intended to work in 2D or 3D mode
		[Tooltip("이 방이 2D 또는 3D 모드에서 작동하도록 되어 있는지 여부")]
		public Modes Mode = Modes.TwoD;

		#if MM_CINEMACHINE
		[Header("Camera")]
		/// the virtual camera associated to this room
		[Tooltip("이 방에 연결된 가상 카메라")]
		public CinemachineVirtualCamera VirtualCamera;
		/// the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room
		[Tooltip("가상 카메라를 제한하는 이 방의 제한 장치로 일반적으로 Room의 하위 객체에 배치됩니다.")]
		public BoxCollider Confiner;
		/// the confiner component of the virtual camera
		[Tooltip("가상 카메라의 제한 구성요소")]
		public CinemachineConfiner CinemachineCameraConfiner;
		#endif
		/// whether or not the confiner should be auto resized on start to match the camera's size and ratio
		[Tooltip("카메라의 크기와 비율에 맞게 시작 시 제한자의 크기를 자동으로 조정해야 하는지 여부")]
		public bool ResizeConfinerAutomatically = true;
		/// whether or not this Room should look at the level's start position and declare itself the current room on start or not
		[Tooltip("이 룸이 레벨의 시작 위치를 확인하고 시작 시 자신을 현재 룸으로 선언해야 하는지 여부")]
		public bool AutoDetectFirstRoomOnStart = true;
		/// the depth of the room (used to resize the z value of the confiner
		[MMEnumCondition("Mode", (int)Modes.TwoD)]
		[Tooltip("방의 깊이(제한자의 z 값 크기를 조정하는 데 사용됨)")]
		public float RoomDepth = 100f;

		[Header("State")]
		/// whether this room is the current room or not
		[Tooltip("이 방이 현재 방인지 아닌지")]
		public bool CurrentRoom = false;
		/// whether this room has already been visited or not
		[Tooltip("이 방을 이미 방문한 적이 있는지 여부")]
		public bool RoomVisited = false;

		[Header("Actions")]
		/// the event to trigger when the player enters the room for the first time
		[Tooltip("플레이어가 처음으로 방에 들어갈 때 트리거되는 이벤트")]
		public UnityEvent OnPlayerEntersRoomForTheFirstTime;
		/// the event to trigger everytime the player enters the room
		[Tooltip("플레이어가 방에 들어갈 때마다 트리거되는 이벤트")]
		public UnityEvent OnPlayerEntersRoom;
		/// the event to trigger everytime the player exits the room
		[Tooltip("플레이어가 방을 나갈 때마다 트리거되는 이벤트")]
		public UnityEvent OnPlayerExitsRoom;

		[Header("Activation")]
		/// a list of gameobjects to enable when entering the room, and disable when exiting it
		[Tooltip("방에 들어갈 때 활성화하고 나갈 때 비활성화할 게임 개체 목록")]
		public List<GameObject> ActivationList;

		protected Collider _roomCollider;
		protected Collider2D _roomCollider2D;
		protected Camera _mainCamera;
		protected Vector2 _cameraSize;
		protected bool _initialized = false;

		/// <summary>
		/// On Awake we reset our camera's priority
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			if (VirtualCamera != null)
			{
				VirtualCamera.Priority = 0;	
			}
			#endif
		}
        
		/// <summary>
		/// On Start we initialize our room
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs our Room collider, our main camera, and starts the confiner resize
		/// </summary>
		protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			_roomCollider = this.gameObject.GetComponent<Collider>();
			_roomCollider2D = this.gameObject.GetComponent<Collider2D>();
			_mainCamera = Camera.main;          
			StartCoroutine(ResizeConfiner());
			_initialized = true;
		}

		/// <summary>
		/// Resizes the confiner 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResizeConfiner()
		{
			#if MM_CINEMACHINE
			if ((VirtualCamera == null) || (Confiner == null) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

			// we wait two more frame for Unity's pixel perfect camera component to be ready because apparently sending events is not a thing.
			yield return null;
			yield return null;

			Confiner.transform.position = RoomColliderCenter;
			Vector3 size = RoomColliderSize;

			switch (Mode)
			{
				case Modes.TwoD:
					size.z = RoomDepth;
					Confiner.size = size;
					_cameraSize.y = 2 * _mainCamera.orthographicSize;
					_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

					Vector3 newSize = Confiner.size;

					if (Confiner.size.x < _cameraSize.x)
					{
						newSize.x = _cameraSize.x;
					}
					if (Confiner.size.y < _cameraSize.y)
					{
						newSize.y = _cameraSize.y;
					}

					Confiner.size = newSize;
					break;
				case Modes.ThreeD:
					Confiner.size = size;
					break;
			}
            
			CinemachineCameraConfiner.InvalidatePathCache();

			//HandleLevelStartDetection();
			#else
            yield return null;
			#endif
		}   
        
		/// <summary>
		/// Looks for the level start position and if it's inside the room, makes this room the current one
		/// </summary>
		protected virtual void HandleLevelStartDetection()
		{
			#if MM_CINEMACHINE
			if (!_initialized)
			{
				Initialization();
			}

			if (AutoDetectFirstRoomOnStart)
			{
				if (LevelManager.HasInstance)
				{
					if (RoomBounds.Contains(LevelManager.Instance.Players[0].transform.position))
					{
						MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
						MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, 0f);

						MMSpriteMaskEvent.Trigger(MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition,
							RoomColliderCenter,
							RoomColliderSize,
							0f, MMTween.MMTweenCurve.LinearTween);

						PlayerEntersRoom();
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 10;
							VirtualCamera.enabled = true;	
						}
					}
					else
					{
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 0;
							VirtualCamera.enabled = false;	
						}
					}
				}
			}
			#endif
		}

		/// <summary>
		/// Call this to let the room know a player entered
		/// </summary>
		public virtual void PlayerEntersRoom()
		{
			CurrentRoom = true;
			if (RoomVisited)
			{
				OnPlayerEntersRoom?.Invoke();
			}
			else
			{
				RoomVisited = true;
				OnPlayerEntersRoomForTheFirstTime?.Invoke();
			}  
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

		/// <summary>
		/// Call this to let this room know a player exited
		/// </summary>
		public virtual void PlayerExitsRoom()
		{
			CurrentRoom = false;
			OnPlayerExitsRoom?.Invoke();
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// When we get a respawn event, we ask for a camera reposition
		/// </summary>
		/// <param name="topDownEngineEvent"></param>
		public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
		{
			if ((topDownEngineEvent.EventType == TopDownEngineEventTypes.RespawnComplete)
			    || (topDownEngineEvent.EventType == TopDownEngineEventTypes.LevelStart))
			{
				HandleLevelStartDetection();
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On enable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}