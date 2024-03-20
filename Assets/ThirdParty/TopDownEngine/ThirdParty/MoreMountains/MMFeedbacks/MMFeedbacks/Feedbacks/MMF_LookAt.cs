using System;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 대상을 볼 수 있도록 변환 회전에 애니메이션을 적용할 수 있습니다.
    /// 또한 이를 사용하여 올바른 채널의 MMLookAtShaker가 듣고 조치를 취할 수 있는 MMLookAtShake 이벤트를 브로드캐스트할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상을 볼 수 있도록 변환 회전에 애니메이션을 적용할 수 있습니다. 또한 이를 사용하여 올바른 채널의 MMLookAtShaker가 듣고 조치를 취할 수 있는 MMLookAtShake 이벤트를 브로드캐스트할 수도 있습니다.")]
	[FeedbackPath("Transform/LookAt")]
	public class MMF_LookAt : MMF_Feedback
	{
        /// 이 유형의 모든 피드백을 한 번에 비활성화하는 데 사용되는 정적 부울입니다.
        public static bool FeedbackTypeAuthorized = true;
        /// 이 피드백에 대한 검사기 색상을 설정합니다.
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup()
		{
			if (Mode == Modes.Direct)
			{
				return TransformToRotate == null;
			}
			else
			{
				return false;
			}
		}
		public override string RequiredTargetText
		{
			get
			{
				if ((Mode == Modes.Direct) && (TransformToRotate != null))
				{
					return TransformToRotate.name;
				}
				else
				{
					return "";
				}
			}
		}
		public override string RequiresSetupText { get { return "In Direct mode, this feedback requires that a DirectTargetTransform be set to be able to work properly. You can set one below."; } } 
		#endif

		/// the duration of this feedback is the duration of the movement, in seconds
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } } 
		public override bool HasChannel => true; 
		public override bool HasRange => true;  

		/// the possible modes for this feedback, either directly targeting a transform, or broadcasting an event
		public enum Modes { Direct, Event } 
		/// whether to look at a specific transform, at a position in the world, or at a direction vector
		public enum LookAtTargetModes { Transform, TargetWorldPosition, Direction }
		/// the vector to consider as "up" when looking at a direction
		public enum UpwardVectors { Forward, Up, Right }
		
		[MMFInspectorGroup("Look at settings", true, 37, true)]
		/// the duration of this feedback, in seconds
		[Tooltip("이 피드백의 지속 시간(초)")]
		public float Duration = 1f;
		/// the curve over which to animate the look at transition
		[Tooltip("전환 시 모습에 애니메이션을 적용할 곡선")]
		public MMTweenType LookAtTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// whether or not to lock rotation on the x axis
		[Tooltip("x축 회전을 잠글지 여부")]
		public bool LockXAxis = false;
		/// whether or not to lock rotation on the y axis
		[Tooltip("y축 회전을 잠글지 여부")]
		public bool LockYAxis = false;
		/// whether or not to lock rotation on the z axis
		[Tooltip("z축 회전을 잠글지 여부")]
		public bool LockZAxis = false;

		[MMFInspectorGroup("What we want to rotate", true, 37, true)]
		/// whether to make a certain transform look at a target, or to broadcast an event
		[Tooltip("특정 변환이 대상을 보도록 할지 아니면 이벤트를 브로드캐스트할지 여부")]
		public Modes Mode = Modes.Direct;
		/// in Direct mode, the transform to rotate to have it look at our target
		[Tooltip("Direct 모드에서는 대상을 보기 위해 회전하는 변환입니다.")]
		[MMFEnumCondition("Mode", (int)Modes.Direct)]
		public Transform TransformToRotate;
		/// the vector representing the up direction on the object we want to rotate and look at our target
		[Tooltip("회전하고 대상을 바라보려는 객체의 위쪽 방향을 나타내는 벡터")]
		[MMFEnumCondition("Mode", (int)Modes.Direct)]
		public UpwardVectors UpwardVector = UpwardVectors.Up;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.Event)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.Event)]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("What we want to look at", true, 37, true)]
		/// the different target modes : either a specific transform to look at, the coordinates of a world position, or a direction vector
		[Tooltip("다양한 대상 모드: 살펴볼 특정 변환, 세계 위치의 좌표 또는 방향 벡터")]
		public LookAtTargetModes LookAtTargetMode = LookAtTargetModes.Transform;
        /// 우리가 보고 싶은 변환
        [Tooltip("우리가 보고 싶은 변환")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.Transform)]
		public Transform LookAtTarget;
		/// the coordinates of a point the world that we want to look at
		[Tooltip("우리가 보고 싶은 세계의 한 지점의 좌표")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.TargetWorldPosition)]
		public Vector3 LookAtTargetWorldPosition = Vector3.forward;
		/// a direction (from our rotating object) that we want to look at
		[Tooltip("우리가 보고 싶은 방향(회전하는 물체로부터)")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.Direction)]
		public Vector3 LookAtDirection = Vector3.forward;
		
		protected Coroutine _coroutine;
		protected Quaternion _initialDirectTargetTransformRotation;
		protected Quaternion _newRotation;
		protected Vector3 _lookAtPosition;
		protected Vector3 _upwards;
		protected Vector3 _direction;
		protected Quaternion _initialRotation;

		/// <summary>
		/// On init we initialize our upwards vector
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			
			switch (UpwardVector)
			{
				case UpwardVectors.Forward:
					_upwards = Vector3.forward;
					break;
				case UpwardVectors.Up:
					_upwards = Vector3.up;
					break;
				case UpwardVectors.Right:
					_upwards = Vector3.right;
					break;
			}
		}

		/// <summary>
		/// On Play we start looking at our target
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (Active || Owner.AutoPlayOnEnable)
			{
				InitiateLookAt(position);
			}
		}

		/// <summary>
		/// Depending on our selected mode, initiates the lookat by starting a coroutine or broadcasting an event
		/// </summary>
		/// <param name="position"></param>
		protected virtual void InitiateLookAt(Vector3 position)
		{
			_initialRotation = TransformToRotate.transform.rotation;
			
			switch (Mode)
			{
				case Modes.Direct:
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(AnimateLookAt());
					break;
				case Modes.Event:
					MMLookAtShaker.MMLookAtShakeEvent.Trigger(Duration, LockXAxis, LockYAxis, LockZAxis, UpwardVector,
						LookAtTargetMode, LookAtTarget, LookAtTargetWorldPosition, LookAtDirection, null,
						LookAtTween,
						UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
						1f, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection,
						ComputedTimescaleMode);
					break;
			}
		}

		/// <summary>
		/// Animates look at direction over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AnimateLookAt()
		{
			if (TransformToRotate != null)
			{
				_initialDirectTargetTransformRotation = TransformToRotate.transform.rotation;
			}

			float duration = FeedbackDuration;
			float journey = NormalPlayDirection ? 0f : duration;

			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
				percent = LookAtTween.Evaluate(percent);
				ApplyRotation(percent);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyRotation(LookAtTween.Evaluate(1f));
			_coroutine = null;
			IsPlaying = false;
		}

		/// <summary>
		/// Applies rotation at the specified time along the journey
		/// </summary>
		/// <param name="percent"></param>
		protected virtual void ApplyRotation(float percent)
		{
			switch (LookAtTargetMode)
			{
				case LookAtTargetModes.Transform:
					_lookAtPosition = LookAtTarget.position;
					break;
				case LookAtTargetModes.TargetWorldPosition:
					_lookAtPosition = LookAtTargetWorldPosition;
					break;
				case LookAtTargetModes.Direction:
					_lookAtPosition = TransformToRotate.position + LookAtDirection;
					break;
			}
			
			if (LockXAxis) { _lookAtPosition.x = TransformToRotate.position.x; }
			if (LockYAxis) { _lookAtPosition.y = TransformToRotate.position.y; }
			if (LockZAxis) { _lookAtPosition.z = TransformToRotate.position.z; }
	            
			_direction = _lookAtPosition - TransformToRotate.position;
			_newRotation = Quaternion.LookRotation(_direction, _upwards);
			
			TransformToRotate.transform.rotation = Quaternion.SlerpUnclamped(_initialDirectTargetTransformRotation, _newRotation, percent);
		}

		/// <summary>
		/// On Stop we stop our movement if we had one going (only in Direct mode)
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			ClearCoroutine();
		}

		/// <summary>
		/// Clears the current coroutine
		/// </summary>
		protected virtual void ClearCoroutine()
		{
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TransformToRotate.transform.rotation = _initialRotation;
		}
	}
}