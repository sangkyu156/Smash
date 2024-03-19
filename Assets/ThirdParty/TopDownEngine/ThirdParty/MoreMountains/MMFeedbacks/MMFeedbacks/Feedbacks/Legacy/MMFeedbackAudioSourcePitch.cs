﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 AudioSource의 피치를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/AudioSource Pitch")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 AudioSource의 피치를 제어할 수 있습니다.")]
	public class MMFeedbackAudioSourcePitch : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		[Header("Pitch Feedback")]
		/// the channel to emit on
		[Tooltip("방출할 채널")]
		public int Channel = 0;
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("Pitch")]
		/// whether or not to add to the initial value
		[Tooltip("초기값에 추가할지 여부")]
		public bool RelativePitch = false;
		/// the curve used to animate the intensity value on
		[Tooltip("강도 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve PitchTween = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
		/// the value to remap the curve's 0 to
		[Range(-3f, 3f)]
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		public float RemapPitchZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(-3f, 3f)]
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		public float RemapPitchOne = 1f;

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMAudioSourcePitchShakeEvent.Trigger(PitchTween, FeedbackDuration, RemapPitchZero, RemapPitchOne, RelativePitch,
				intensityMultiplier, ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMAudioSourcePitchShakeEvent.Trigger(PitchTween, FeedbackDuration, RemapPitchZero, RemapPitchOne, RelativePitch, stop:true);
		}
	}
}