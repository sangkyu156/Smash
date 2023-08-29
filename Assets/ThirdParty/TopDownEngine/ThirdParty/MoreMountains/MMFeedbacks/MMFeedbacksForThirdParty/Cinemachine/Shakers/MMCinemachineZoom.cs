using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using DG.Tweening;
using System;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 클래스를 사용하면 다른 클래스에서 MMCameraZoomEvents를 전송하여 시네마머신 카메라에서 확대/축소를 트리거할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachineZoom")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
	#endif
	public class MMCinemachineZoom : MonoBehaviour
	{
		[Header("Channel")]
		[MMFInspectorGroup("Shaker Settings", true, 3)]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;

		[Header("Transition Speed")]
		/// the animation curve to apply to the zoom transition
		[Tooltip("the animation curve to apply to the zoom transition")]
		public MMTweenType ZoomTween = new MMTweenType( new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)));

		[Header("Test Zoom")]
		/// the mode to apply the zoom in when using the test button in the inspector
		[Tooltip("the mode to apply the zoom in when using the test button in the inspector")]
		public MMCameraZoomModes TestMode;
		/// the target field of view to apply the zoom in when using the test button in the inspector
		[Tooltip("the target field of view to apply the zoom in when using the test button in the inspector")]
		public float TestFieldOfView = 30f;
		/// the transition duration to apply the zoom in when using the test button in the inspector
		[Tooltip("the transition duration to apply the zoom in when using the test button in the inspector")]
		public float TestTransitionDuration = 0.1f;
		/// the duration to apply the zoom in when using the test button in the inspector
		[Tooltip("the duration to apply the zoom in when using the test button in the inspector")]
		public float TestDuration = 0.05f;

		[MMFInspectorButton("TestZoom")]
        /// 재생 모드에서 확대/축소를 테스트하는 검사기 버튼
        public bool TestZoomButton;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		public TimescaleModes TimescaleMode { get; set; }
        
		#if MM_CINEMACHINE
		protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
		protected float _initialFieldOfView;
		protected MMCameraZoomModes _mode;
		protected bool _zooming = false;
		protected float _startFieldOfView;
		protected float _transitionDuration;
		protected float _duration;
		protected float _targetFieldOfView;
		protected float _elapsedTime = 0f;
		protected int _direction = 1;
		protected float _reachedDestinationTimestamp;
		protected bool _destinationReached = false;
		protected float _zoomStartedAt = 0f;
		//내가만든 변수
		public float rotateSpeed = 15f;
        Vector2 clickPoint;


        /// <summary>
        /// On Awake we grab our virtual camera
        /// </summary>
        protected virtual void Awake()
		{
			_virtualCamera = this.gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
			_initialFieldOfView = _virtualCamera.m_Lens.FieldOfView;
		}

        /// <summary>
        /// 업데이트 시 확대/축소하는 경우 이에 따라 시야를 수정합니다.
        /// </summary>
        protected virtual void Update()
		{
			MyZoom();
			MyRotate();

            if (!_zooming)
			{
				return;
			}

            _elapsedTime = GetTime() - _zoomStartedAt;
			if (_elapsedTime <= _transitionDuration)
			{
				float t = MMMaths.Remap(_elapsedTime, 0f, _transitionDuration, 0f, 1f);
				_virtualCamera.m_Lens.FieldOfView = Mathf.LerpUnclamped(_startFieldOfView, _targetFieldOfView, ZoomTween.Evaluate(t));
			}
			else
			{
				if (!_destinationReached)
				{
					_reachedDestinationTimestamp = GetTime();
					_destinationReached = true;
				}
				if ((_mode == MMCameraZoomModes.For) && (_direction == 1))
				{
					if (GetTime() - _reachedDestinationTimestamp > _duration)
					{
						_direction = -1;
						_zoomStartedAt = GetTime();
						_startFieldOfView = _targetFieldOfView;
						_targetFieldOfView = _initialFieldOfView;
					}                    
				}
				else
				{
					_zooming = false;
				}   
			}
		}

		public void MyZoom()
		{
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (_virtualCamera.m_Lens.FieldOfView <= 20f && scroll > 0)
            {
                _virtualCamera.m_Lens.FieldOfView = 20f;
            }
            else if (_virtualCamera.m_Lens.FieldOfView >= 40f && scroll < 0)
            {
                _virtualCamera.m_Lens.FieldOfView = 40f;
            }
            else
            {
                _virtualCamera.m_Lens.FieldOfView -= (scroll * 10);
            }
        }

		public void MyRotate()
		{
            if (Input.GetMouseButtonDown(1))
                clickPoint = Input.mousePosition;

			if(Input.GetMouseButton(1))
			{
                Vector3 position = Camera.main.ScreenToViewportPoint((Vector2)Input.mousePosition - clickPoint);

                float moveX = position.x * rotateSpeed;

                if (moveX > 5f)
                    moveX = 5f;
                else if (moveX < -5f)
                    moveX = -5f;

                _virtualCamera.transform.RotateAround(_virtualCamera.Follow.transform.position, Vector3.up, moveX);
            }
        }

        /// <summary>
        /// 이상적으로는 이벤트를 통해서만 호출되지만 편의상 공개되는 확대/축소를 트리거하는 메서드입니다.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="newFieldOfView"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="duration"></param>
        public virtual void Zoom(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, bool useUnscaledTime, bool relative = false, MMTweenType tweenType = null)
		{
			if (_zooming)
			{
				return;
			}

			_zooming = true;
			_elapsedTime = 0f;
			_mode = mode;

			TimescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			_startFieldOfView = _virtualCamera.m_Lens.FieldOfView;
			_transitionDuration = transitionDuration;
			_duration = duration;
			_transitionDuration = transitionDuration;
			_direction = 1;
			_destinationReached = false;
			_zoomStartedAt = GetTime();
			
			if (tweenType != null)
			{
				ZoomTween = tweenType;
			}

			switch (mode)
			{
				case MMCameraZoomModes.For:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Set:
					_targetFieldOfView = newFieldOfView;
					break;

				case MMCameraZoomModes.Reset:
					_targetFieldOfView = _initialFieldOfView;
					break;
			}

			if (relative)
			{
				_targetFieldOfView += _initialFieldOfView;
			}
		}

		/// <summary>
		/// The method used by the test button to trigger a test zoom
		/// </summary>
		protected virtual void TestZoom()
		{
			Zoom(TestMode, TestFieldOfView, TestTransitionDuration, TestDuration, false);
		}

        /// <summary>
        /// MMCameraZoomEvent를 받으면 확대/축소 메서드를 호출합니다.
        /// </summary>
        /// <param name="zoomEvent"></param>
        public virtual void OnCameraZoomEvent(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, 
			bool useUnscaledTime, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			if (stop)
			{
				_zooming = false;
				return;
			}
			if (restore)
			{
				_virtualCamera.m_Lens.FieldOfView = _initialFieldOfView;
				return;
			}
			this.Zoom(mode, newFieldOfView, transitionDuration, duration, useUnscaledTime, relative, tweenType);
		}

		/// <summary>
		/// Starts listening for MMCameraZoomEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			MMCameraZoomEvent.Register(OnCameraZoomEvent);
		}

		/// <summary>
		/// Stops listening for MMCameraZoomEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			MMCameraZoomEvent.Unregister(OnCameraZoomEvent);
		}
		#endif
	}
}