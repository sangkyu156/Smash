using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 애니메이터의 속도를 한 번 또는 즉시 변경한 다음 재설정하거나 시간이 지남에 따라 보간할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 애니메이터의 속도를 한 번 또는 즉시 변경한 다음 재설정하거나 시간이 지남에 따라 보간할 수 있습니다.")]
	[FeedbackPath("Animation/Animator Speed")]
	public class MMF_AnimatorSpeed : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "이 피드백을 위해서는 BoundAnimator가 제대로 작동할 수 있도록 설정되어야 합니다. 아래에서 하나를 설정할 수 있습니다."; } }
		#endif
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		public enum SpeedModes { Once, InstantThenReset, OverTime }
		
		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("매개변수를 업데이트하려는 애니메이터")]
		public Animator BoundAnimator;

		[MMFInspectorGroup("Speed", true, 14, true)]
		/// whether to change the speed of the target animator once, instantly and reset it later, or have it change over time
		[Tooltip("대상 애니메이터의 속도를 한 번, 즉시 변경하고 나중에 재설정할지, 아니면 시간이 지남에 따라 변경하도록 할지 여부")]
		public SpeedModes Mode = SpeedModes.Once; 
		/// the new minimum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("애니메이터를 설정할 새로운 최소 속도 - 값은 최소와 최대 사이에서 무작위로 지정됩니다.")]
		public float NewSpeedMin = 0f; 
		/// the new maximum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("애니메이터를 설정할 새로운 최대 속도 - 값은 최소와 최대 사이에서 무작위로 지정됩니다.")]
		public float NewSpeedMax = 0f;
		/// when in instant then reset or over time modes, the duration of the effect
		[Tooltip("순간 모드일 때, 재설정 또는 시간 초과 모드일 때, 효과 지속 시간")]
		[MMFEnumCondition("Mode", (int)SpeedModes.InstantThenReset, (int)SpeedModes.OverTime)]
		public float Duration = 1f;
		/// when in over time mode, the curve against which to evaluate the new speed
		[Tooltip("시간 초과 모드에서 새로운 속도를 평가할 곡선")]
		[MMFEnumCondition("Mode", (int)SpeedModes.OverTime)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));

		protected Coroutine _coroutine;
		protected float _initialSpeed;
		protected float _startedAt;
        
		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("No animator was set for " + Owner.name);
				return;
			}

			if (!IsPlaying)
			{
				_initialSpeed = BoundAnimator.speed;	
			}

			if (Mode == SpeedModes.Once)
			{
				BoundAnimator.speed = ComputeIntensity(DetermineNewSpeed(), position);
			}
			else
			{
				_coroutine = Owner.StartCoroutine(ChangeSpeedCo());
			}
		}

		/// <summary>
		/// A coroutine used in ForDuration mode
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeSpeedCo()
		{
			if (Mode == SpeedModes.InstantThenReset)
			{
				IsPlaying = true;
				BoundAnimator.speed = DetermineNewSpeed();
				yield return MMCoroutine.WaitFor(Duration);
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
			else if (Mode == SpeedModes.OverTime)
			{
				IsPlaying = true;
				_startedAt = FeedbackTime;
				float newTargetSpeed = DetermineNewSpeed();
				while (FeedbackTime - _startedAt < Duration)
				{
					float time = MMFeedbacksHelpers.Remap(FeedbackTime - _startedAt, 0f, Duration, 0f, 1f);
					float t = Curve.Evaluate(time);
					BoundAnimator.speed = Mathf.Max(0f, MMFeedbacksHelpers.Remap(t, 0f, 1f, _initialSpeed, newTargetSpeed));
					yield return null;
				}
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
		}

		/// <summary>
		/// Determines the new speed for the target animator
		/// </summary>
		/// <returns></returns>
		protected virtual float DetermineNewSpeed()
		{
			return Mathf.Abs(Random.Range(NewSpeedMin, NewSpeedMax));
		}
        
		/// <summary>
		/// On stop, turns the bool parameter to false
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);	
			}

			BoundAnimator.speed = _initialSpeed;
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

			BoundAnimator.speed = _initialSpeed;
		}
	}
}