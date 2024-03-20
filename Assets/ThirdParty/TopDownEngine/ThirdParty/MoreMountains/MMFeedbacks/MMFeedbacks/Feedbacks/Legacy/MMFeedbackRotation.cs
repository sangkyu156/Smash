using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 지정된 개체의 회전에 애니메이션을 적용합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 지정된 기간(초) 동안 지정된 3개의 애니메이션 곡선(축당 하나)에서 대상의 회전을 애니메이션화합니다.")]
	[FeedbackPath("Transform/Rotation")]
	public class MMFeedbackRotation : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible modes for this feedback (Absolute : always follow the curve from start to finish, Additive : add to the values found when this feedback gets played)
		public enum Modes { Absolute, Additive, ToDestination }
		/// the timescale modes this feedback can operate on
		public enum TimeScales { Scaled, Unscaled }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif

		[Header("Rotation Target")]
		/// the object whose rotation you want to animate
		[Tooltip("회전을 애니메이션화하려는 객체")]
		public Transform AnimateRotationTarget;

		[Header("Animation")]
		/// whether this feedback should animate in absolute values or additive
		[Tooltip("이 피드백이 절대값으로 애니메이션되어야 하는지 아니면 추가로 애니메이션되어야 하는지 여부")]
		public Modes Mode = Modes.Absolute;
		/// whether this feedback should play in scaled or unscaled time
		[Tooltip("이 피드백이 확장된 시간에 재생되어야 하는지 또는 확장되지 않은 시간에 재생되어야 하는지 여부")]
		public TimeScales TimeScale = TimeScales.Scaled;
		/// whether this feedback should play on local or world rotation
		[Tooltip("이 피드백이 로컬 또는 월드 회전에서 재생되어야 하는지 여부")]
		public Space RotationSpace = Space.World;
		/// the duration of the transition
		[Tooltip("전환 기간")]
		public float AnimateRotationDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("the value to remap the curve's 0 value to")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("the value to remap the curve's 1 value to")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveOne = 360f;
		/// if this is true, should animate the X rotation
		[Tooltip("이것이 사실이라면 X 회전에 애니메이션을 적용해야 합니다")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateX = true;
		/// how the x part of the rotation should animate over time, in degrees
		[Tooltip("how the x part of the rotation should animate over time, in degrees")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public AnimationCurve AnimateRotationX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate the X rotation
		[Tooltip("if this is true, should animate the X rotation")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateY = true;
		/// how the y part of the rotation should animate over time, in degrees
		[Tooltip("how the y part of the rotation should animate over time, in degrees")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public AnimationCurve AnimateRotationY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate the X rotation
		[Tooltip("if this is true, should animate the X rotation")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateZ = true;
		/// how the z part of the rotation should animate over time, in degrees
		[Tooltip("회전의 z 부분이 시간에 따라 어떻게 애니메이션화되어야 하는지(도 단위)")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public AnimationCurve AnimateRotationZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination rotations will be recomputed on every play
		[Tooltip("이것이 사실이라면 매 플레이마다 초기 및 대상 회전이 다시 계산됩니다.")]
		public bool DetermineRotationOnPlay = false;
        
		[Header("To Destination")]
		/// the space in which the ToDestination mode should operate 
		[Tooltip("ToDestination 모드가 동작해야 하는 공간")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Space ToDestinationSpace = Space.World;
		/// the angles to match when in ToDestination mode
		[Tooltip("ToDestination 모드에 있을 때 일치시킬 각도")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector3 DestinationAngles = new Vector3(0f, 180f, 0f);
		/// the animation curve to use when animating to destination (individual x,y,z curves above won't be used)
		[Tooltip("대상으로 애니메이션을 적용할 때 사용할 애니메이션 곡선(위의 개별 x, y, z 곡선은 사용되지 않음)")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
        
		/// the duration of this feedback is the duration of the rotation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }

		protected Quaternion _initialRotation;
		protected Vector3 _initialToDestinationAngles;
		protected Quaternion _destinationRotation;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial rotation
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateRotationTarget != null))
			{
				GetInitialRotation();
			}
		}

		/// <summary>
		/// Stores initial rotation for future use
		/// </summary>
		protected virtual void GetInitialRotation()
		{
			_initialRotation = (RotationSpace == Space.World) ? AnimateRotationTarget.rotation : AnimateRotationTarget.localRotation;
			_initialToDestinationAngles = _initialRotation.eulerAngles;
		}

		/// <summary>
		/// On play, we trigger our rotation animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateRotationTarget == null))
			{
				return;
			}

			if (DetermineRotationOnPlay && NormalPlayDirection)
			{
				GetInitialRotation();
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			if (isActiveAndEnabled || _hostMMFeedbacks.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationX, AnimateRotationY, AnimateRotationZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				else if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(RotateToDestination());
				}
			}
		}

		/// <summary>
		/// A coroutine used to rotate the target to its destination rotation
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator RotateToDestination()
		{
			if (AnimateRotationTarget == null)
			{
				yield break;
			}

			if ((AnimateRotationX == null) || (AnimateRotationY == null) || (AnimateRotationZ == null))
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			Vector3 destinationAngles = NormalPlayDirection ? DestinationAngles : _initialToDestinationAngles;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialRotation = AnimateRotationTarget.transform.rotation;
			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
            
			_destinationRotation = AnimateRotationTarget.transform.rotation;
			AnimateRotationTarget.transform.rotation = _initialRotation;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);
				percent = ToDestinationCurve.Evaluate(percent);

				Quaternion newRotation = Quaternion.LerpUnclamped(_initialRotation, _destinationRotation, percent);
				AnimateRotationTarget.transform.rotation = newRotation;

				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
				yield return null;
			}

			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

		/// <summary>
		/// A coroutine used to compute the rotation over time
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="vector"></param>
		/// <param name="duration"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		protected virtual IEnumerator AnimateRotation(Transform targetTransform,
			Vector3 vector,
			float duration,
			AnimationCurve curveX,
			AnimationCurve curveY,
			AnimationCurve curveZ,
			float remapZero,
			float remapOne)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if ((curveX == null) || (curveY == null) || (curveZ == null))
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : duration;

			if (Mode == Modes.Additive)
			{
				_initialRotation = (RotationSpace == Space.World) ? targetTransform.rotation : targetTransform.localRotation;
			}

			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
                
				ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, percent);

				if (TimeScale == TimeScales.Scaled)
				{
					journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				}
				else
				{
					journey += NormalPlayDirection ? Time.unscaledDeltaTime : -Time.unscaledDeltaTime;
				}
				yield return null;
			}

			ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
            
			yield break;
		}

		/// <summary>
		/// Computes and applies the rotation to the object
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="multiplier"></param>
		/// <param name="curveX"></param>
		/// <param name="curveY"></param>
		/// <param name="curveZ"></param>
		/// <param name="percent"></param> 
		protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float percent)
		{
			if (RotationSpace == Space.World)
			{
				targetTransform.transform.rotation = _initialRotation;    
			}
			else
			{
				targetTransform.transform.localRotation = _initialRotation;
			}

			if (AnimateX)
			{
				float x = curveX.Evaluate(percent);
				x = MMFeedbacksHelpers.Remap(x, 0f, 1f, remapZero, remapOne);
				targetTransform.Rotate(Vector3.right, x, RotationSpace);
			}
			if (AnimateY)
			{
				float y = curveY.Evaluate(percent);
				y = MMFeedbacksHelpers.Remap(y, 0f, 1f, remapZero, remapOne);
				targetTransform.Rotate(Vector3.up, y, RotationSpace);
			}
			if (AnimateZ)
			{
				float z = curveZ.Evaluate(percent);
				z = MMFeedbacksHelpers.Remap(z, 0f, 1f, remapZero, remapOne);
				targetTransform.Rotate(Vector3.forward, z, RotationSpace);
			}
		}
        
		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
				IsPlaying = false;
			}
		}

		/// <summary>
		/// On disable we reset our coroutine
		/// </summary>
		protected virtual void OnDisable()
		{
			_coroutine = null;
		}
	}
}