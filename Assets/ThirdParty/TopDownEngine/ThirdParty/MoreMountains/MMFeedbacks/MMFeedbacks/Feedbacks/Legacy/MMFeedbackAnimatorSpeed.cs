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
	[FeedbackPath("GameObject/Animator Speed")]
	public class MMFeedbackAnimatorSpeed : MMFeedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		public enum Modes { Once, InstantThenReset, OverTime }
		public virtual float GetTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (Timing.TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		
		[Header("Animation")]
		/// the animator whose parameters you want to update
		[Tooltip("매개변수를 업데이트하려는 애니메이터")]
		public Animator BoundAnimator;

		[Header("Speed")] 
		/// whether to change the speed of the target animator once, instantly and reset it later, or have it change over time
		[Tooltip("대상 애니메이터의 속도를 한 번, 즉시 변경하고 나중에 재설정할지, 아니면 시간이 지남에 따라 변경하도록 할지 여부")]
		public Modes Mode = Modes.Once; 
		/// the new minimum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("애니메이터를 설정할 새로운 최소 속도 - 값은 최소와 최대 사이에서 무작위로 지정됩니다.")]
		public float NewSpeedMin = 0f; 
		/// the new maximum speed at which to set the animator - value will be randomized between min and max
		[Tooltip("애니메이터를 설정할 새로운 최대 속도 - 값은 최소와 최대 사이에서 무작위로 지정됩니다.")]
		public float NewSpeedMax = 0f;
		/// when in instant then reset or over time modes, the duration of the effect
		[Tooltip("순간 모드일 때, 재설정 또는 시간 초과 모드일 때, 효과 지속 시간")]
		[MMFEnumCondition("Mode", (int)Modes.InstantThenReset, (int)Modes.OverTime)]
		public float Duration = 1f;
		/// when in over time mode, the curve against which to evaluate the new speed
		[Tooltip("시간 초과 모드에서 새로운 속도를 평가할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
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
				Debug.LogWarning("No animator was set for " + this.name);
				return;
			}

			if (!IsPlaying)
			{
				_initialSpeed = BoundAnimator.speed;	
			}

			if (Mode == Modes.Once)
			{
				BoundAnimator.speed = DetermineNewSpeed();
			}
			else
			{
				_coroutine = StartCoroutine(ChangeSpeedCo());
			}
		}

		/// <summary>
		/// A coroutine used in ForDuration mode
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeSpeedCo()
		{
			if (Mode == Modes.InstantThenReset)
			{
				IsPlaying = true;
				BoundAnimator.speed = DetermineNewSpeed();
				yield return MMCoroutine.WaitFor(Duration);
				BoundAnimator.speed = _initialSpeed;	
				IsPlaying = false;
			}
			else if (Mode == Modes.OverTime)
			{
				IsPlaying = true;
				_startedAt = GetTime();
				float newTargetSpeed = DetermineNewSpeed();
				while (GetTime() - _startedAt < Duration)
				{
					float time = MMFeedbacksHelpers.Remap(GetTime() - _startedAt, 0f, Duration, 0f, 1f);
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
				StopCoroutine(_coroutine);	
			}

			BoundAnimator.speed = _initialSpeed;
		}
	}
}