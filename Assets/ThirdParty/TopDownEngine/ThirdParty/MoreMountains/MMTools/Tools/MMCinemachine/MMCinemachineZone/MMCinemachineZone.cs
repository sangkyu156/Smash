using System.Collections;
using System.Collections.Generic;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 입력 시 가상 카메라를 활성화하고 모든 상용구 설정을 처리하는 영역을 정의할 수 있는 추상 클래스
    /// </summary>
    [AddComponentMenu("")]
	[ExecuteAlways]
	public abstract class MMCinemachineZone : MonoBehaviour
	{
		public enum Modes { Enable, Priority }
        
		[Header("Virtual Camera")]
		/// whether to enable/disable virtual cameras, or to play on their priority for transitions
		[Tooltip("가상 카메라를 활성화/비활성화할지 또는 전환 우선 순위에 따라 재생할지 여부")]
		public Modes Mode = Modes.Enable;
		/// whether or not the camera in this zone should start active
		[Tooltip("이 영역의 카메라가 활성화되어야 하는지 여부")]
		public bool CameraStartsActive = false;
		#if MM_CINEMACHINE
		/// the virtual camera associated to this zone (will try to grab one in children if none is set) 
		[Tooltip("이 영역과 연결된 가상 카메라(아무 것도 설정되지 않은 경우 어린이에서 하나를 잡으려고 시도합니다)")]
		public CinemachineVirtualCamera VirtualCamera;
		#endif

		/// when in priority mode, the priority this camera should have when the zone is active
		[Tooltip("우선순위 모드에서는 영역이 활성화되었을 때 이 카메라가 가져야 하는 우선순위입니다.")]
		[MMEnumCondition("Mode", (int)Modes.Priority)]
		public int EnabledPriority = 10;
		/// when in priority mode, the priority this camera should have when the zone is inactive
		[Tooltip("우선순위 모드에 있을 때 구역이 비활성일 때 이 카메라가 가져야 하는 우선순위입니다.")]
		[MMEnumCondition("Mode", (int)Modes.Priority)]
		public int DisabledPriority = 0;

		[Header("Collisions")] 
		/// a layermask containing all the layers that should activate this zone
		[Tooltip("이 영역을 활성화해야 하는 모든 레이어를 포함하는 레이어 마스크")]
		public LayerMask TriggerMask;
        
		[Header("Confiner Setup")] 
		/// whether or not the zone should auto setup its camera's confiner on start - alternative is to manually click the ManualSetupConfiner, or do your own setup
		[Tooltip("영역이 시작 시 카메라 제한을 자동으로 설정해야 하는지 여부 - 대안은 ManualSetupConfiner를 수동으로 클릭하거나 직접 설정하는 것입니다.")]
		public bool SetupConfinerOnStart = false;

		/// a debug button used to setup the confiner on click
		[MMInspectorButton("ManualSetupConfiner")]
		public bool GenerateConfinerSetup;
		
		[Header("State")]
		/// whether this room is the current room or not
		[Tooltip("이 방이 현재 방인지 아닌지")]
		[MMReadOnly]
		public bool CurrentRoom = false;
		/// whether this room has already been visited or not
		[Tooltip("이 방을 이미 방문한 적이 있는지 여부")]
		public bool RoomVisited = false;

		[Header("Events")] 
		/// a UnityEvent to trigger when entering the zone for the first time
		[Tooltip("처음으로 영역에 들어갈 때 트리거할 UnityEvent")]
		public UnityEvent OnEnterZoneForTheFirstTimeEvent;
		/// a UnityEvent to trigger when entering the zone
		[Tooltip("영역에 들어갈 때 트리거할 UnityEvent")]
		public UnityEvent OnEnterZoneEvent;
		/// a UnityEvent to trigger when exiting the zone
		[Tooltip("영역을 나갈 때 트리거할 UnityEvent")]
		public UnityEvent OnExitZoneEvent;

		[Header("Activation")]

		/// a list of gameobjects to enable when entering the zone, and disable when exiting it
		[Tooltip("영역에 들어갈 때 활성화하고 나갈 때 비활성화할 게임 객체 목록")]
		public List<GameObject> ActivationList;

		[Header("Debug")] 
		/// whether or not to draw shape gizmos to help visualize the zone's bounds
		[Tooltip("영역 경계를 시각화하는 데 도움이 되는 모양 기즈모를 그릴지 여부")]
		public bool DrawGizmos = true;
		/// the color of the gizmos to draw in edit mode
		[Tooltip("편집 모드에서 그릴 기즈모의 색상")] 
		public Color GizmosColor;
        
		protected GameObject _confinerGameObject;
		protected Vector3 _gizmoSize;

		#if MM_CINEMACHINE
        
		/// <summary>
		/// On Awake we proceed to init if app is playing
		/// </summary>
		protected virtual void Awake()
		{
			AlwaysInitialization();
			if (!Application.isPlaying)
			{
				return;
			}
			Initialization();
		}

		/// <summary>
		/// On Awake we initialize our collider
		/// </summary>
		protected virtual void AlwaysInitialization()
		{
			InitializeCollider();
		}

		/// <summary>
		/// On init we grab our virtual camera 
		/// </summary>
		protected virtual void Initialization()
		{
			if (VirtualCamera == null)
			{
				VirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
			}

			if (VirtualCamera == null)
			{
				Debug.LogWarning("[MMCinemachineZone2D] " + this.name + " : no virtual camera is attached to this zone. Set one in its inspector.");
			}

			if (SetupConfinerOnStart)
			{
				SetupConfinerGameObject();	
			}
            
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// On Start we setup the confiner
		/// </summary>
		protected virtual void Start()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (SetupConfinerOnStart)
			{
				SetupConfiner();	
			}
			
			StartCoroutine(EnableCamera(CameraStartsActive, 1));
		}

		/// <summary>
		/// Describes what happens when initializing the collider
		/// </summary>
		protected abstract void InitializeCollider();

		/// <summary>
		/// Describes what happens when setting up the confiner
		/// </summary>
		protected abstract void SetupConfiner();

		/// <summary>
		/// A method used to manually create a confiner
		/// </summary>
		protected virtual void ManualSetupConfiner()
		{
			Initialization();
			SetupConfiner();
		}

		/// <summary>
		/// Creates an object to host the confiner
		/// </summary>
		protected virtual void SetupConfinerGameObject()
		{
			// we remove the object if needed
			Transform child = this.transform.Find("Confiner");
			if (child != null)
			{
				DestroyImmediate(child.gameObject);
			}
            
			// we create an empty child object
			_confinerGameObject = new GameObject();
			_confinerGameObject.transform.localPosition = Vector3.zero;
			_confinerGameObject.transform.SetParent(this.transform);
			_confinerGameObject.name = "Confiner";
		}

		/// <summary>
		/// An extra test you can override to add extra collider conditions
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestCollidingGameObject(GameObject collider)
		{
			return true;
		}
        
		/// <summary>
		/// Enables the camera, either via enabled state or priority
		/// </summary>
		/// <param name="state"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		protected virtual IEnumerator EnableCamera(bool state, int frames)
		{
			if (VirtualCamera == null)
			{
				yield break;
			}

			if (frames > 0)
			{
				yield return MMCoroutine.WaitForFrames(frames);    
			}

			if (Mode == Modes.Enable)
			{
				VirtualCamera.enabled = state;
			}
			else if (Mode == Modes.Priority)
			{
				VirtualCamera.Priority = state ? EnabledPriority : DisabledPriority;
			}
		}

		protected virtual void EnterZone()
		{
			if (!RoomVisited)
			{
				OnEnterZoneForTheFirstTimeEvent.Invoke();	
			}
			
			CurrentRoom = true;
			RoomVisited = true;

			OnEnterZoneEvent.Invoke();
			StartCoroutine(EnableCamera(true, 0));
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

		protected virtual void ExitZone()
		{
			CurrentRoom = false;
			OnExitZoneEvent.Invoke();
			StartCoroutine(EnableCamera(false, 0));
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

		/// <summary>
		/// On Reset we initialize our gizmo color
		/// </summary>
		protected virtual void Reset()
		{
			GizmosColor = MMColors.RandomColor();
			GizmosColor.a = 0.2f;
		}

		#endif
	}    
}