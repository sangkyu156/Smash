using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
	/// A class that handles camera follow for Cinemachine powered cameras
    /// 시네머신 기반 카메라의 카메라 팔로우를 처리하는 클래스
    /// </summary>
    public class CinemachineCameraController : TopDownMonoBehaviour, MMEventListener<MMCameraEvent>, MMEventListener<TopDownEngineEvent>
	{
        /// 카메라가 플레이어를 따라가야 한다면 True입니다.
        public bool FollowsPlayer { get; set; }
        /// 이 카메라가 플레이어를 따라가야 하는지 여부
        [Tooltip("이 카메라가 플레이어를 따라가야 하는지 여부")]
		public bool FollowsAPlayer = true;
        /// LevelManager에 정의된 대로 이 카메라를 레벨 경계에 제한할지 여부
        [Tooltip("LevelManager에 정의된 대로 이 카메라를 레벨 경계에 제한할지 여부")]
		public bool ConfineCameraToLevelBounds = true;
        /// 이것이 사실이라면, 이 제한자는 설정된 제한자 이벤트를 수신합니다.
        [Tooltip("이것이 사실이라면, 이 제한자는 설정된 제한자 이벤트를 수신합니다.")]
		public bool ListenToSetConfinerEvents = true;
		[MMReadOnly]
        /// 이 카메라가 따라야 하는 대상 캐릭터
        [Tooltip("이 카메라가 따라야 하는 대상 캐릭터")]
		public Character TargetCharacter;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _virtualCamera;
		protected CinemachineConfiner _confiner;
#endif

        /// <summary>
        /// Awake에서는 구성 요소를 가져옵니다.
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = GetComponent<CinemachineVirtualCamera>();
			_confiner = GetComponent<CinemachineConfiner>();
			#endif
		}

        /// <summary>
        /// 시작 시 경계 볼륨을 할당합니다.
        /// </summary>
        protected virtual void Start()
		{
			#if MM_CINEMACHINE
			if ((_confiner != null) && ConfineCameraToLevelBounds && LevelManager.HasInstance)
			{
				_confiner.m_BoundingVolume = LevelManager.Instance.BoundsCollider;
			}
			#endif
		}

		public virtual void SetTarget(Character character)
		{
			TargetCharacter = character;
		}

        /// <summary>
        /// LevelManager의 메인 플레이어를 따라가기 시작합니다.
        /// </summary>
        public virtual void StartFollowing()
		{
			if (!FollowsAPlayer) { return; }
			FollowsPlayer = true;
			#if MM_CINEMACHINE
			_virtualCamera.Follow = TargetCharacter.CameraTarget.transform;
			_virtualCamera.LookAt = TargetCharacter.CameraTarget.transform;
			_virtualCamera.enabled = true;
			#endif
		}

		/// <summary>
		/// Stops following any target
		/// </summary>
		public virtual void StopFollowing()
		{
			if (!FollowsAPlayer) { return; }
			FollowsPlayer = false;
			#if MM_CINEMACHINE
			_virtualCamera.Follow = null;
			_virtualCamera.enabled = false;
			#endif
		}

		public virtual void OnMMEvent(MMCameraEvent cameraEvent)
		{
			#if MM_CINEMACHINE
			switch (cameraEvent.EventType)
			{
				case MMCameraEventTypes.SetTargetCharacter:
					SetTarget(cameraEvent.TargetCharacter);
					break;

				case MMCameraEventTypes.SetConfiner:                    
					if (_confiner != null && ListenToSetConfinerEvents)
					{
						_confiner.m_BoundingVolume = cameraEvent.Bounds;
					}
					break;

				case MMCameraEventTypes.StartFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StartFollowing();
					break;

				case MMCameraEventTypes.StopFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StopFollowing();
					break;

				case MMCameraEventTypes.RefreshPosition:
					StartCoroutine(RefreshPosition());
					break;

				case MMCameraEventTypes.ResetPriorities:
					_virtualCamera.Priority = 0;
					break;
			}
			#endif
		}

		protected virtual IEnumerator RefreshPosition()
		{
			#if MM_CINEMACHINE
			_virtualCamera.enabled = false;
			#endif
			yield return null;
			StartFollowing();
		}

		public virtual void OnMMEvent(TopDownEngineEvent topdownEngineEvent)
		{
			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwitch)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				StartFollowing();
			}

			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwap)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				MMCameraEvent.Trigger(MMCameraEventTypes.RefreshPosition);
			}
		}

		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMCameraEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMCameraEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}