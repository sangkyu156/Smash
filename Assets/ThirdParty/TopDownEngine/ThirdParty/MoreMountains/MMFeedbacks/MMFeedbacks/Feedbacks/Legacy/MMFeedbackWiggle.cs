using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 재생되면 이 피드백은 선택한 설정에 따라 MMWiggle 개체의 Wiggle 메서드를 활성화하여 해당 위치, 회전, 배율 또는 이들 모두를 흔들게 됩니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 지정된 기간 동안 MMWiggle 구성 요소가 장착된 개체의 위치, 회전 및/또는 크기 조정 흔들기를 트리거할 수 있습니다.")]
	[FeedbackPath("Transform/Wiggle")]
	public class MMFeedbackWiggle : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		#endif

		[Header("Target")]
		/// the Wiggle component to target
		[Tooltip("대상으로 삼을 Wiggle 구성요소")]
		public MMWiggle TargetWiggle;
        
		[Header("Position")]
		/// whether or not to wiggle position
		[Tooltip("위치를 흔들거나 말거나")]
		public bool WigglePosition = true;
		/// the duration (in seconds) of the position wiggle
		[Tooltip("위치 흔들기의 지속 시간(초)")]
		public float WigglePositionDuration;

		[Header("Rotation")]
		/// whether or not to wiggle rotation
		[Tooltip("회전을 흔들지 여부")]
		public bool WiggleRotation;
		/// the duration (in seconds) of the rotation wiggle
		[Tooltip("회전 흔들기의 지속 시간(초)")]
		public float WiggleRotationDuration;

		[Header("Scale")]
		/// whether or not to wiggle scale
		[Tooltip("스케일을 흔들지 말지")]
		public bool WiggleScale;
		/// the duration (in seconds) of the scale wiggle
		[Tooltip("눈금 흔들기의 지속 시간(초)")]
		public float WiggleScaleDuration;


		/// the duration of this feedback is the duration of the clip being played
		public override float FeedbackDuration
		{
			get { return Mathf.Max(ApplyTimeMultiplier(WigglePositionDuration), ApplyTimeMultiplier(WiggleRotationDuration), ApplyTimeMultiplier(WiggleScaleDuration)); }
			set { WigglePositionDuration = value;
				WiggleRotationDuration = value;
				WiggleScaleDuration = value;
			} 
		}

		/// <summary>
		/// On Play we trigger the desired wiggles
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetWiggle == null))
			{
				return;
			}
            
			TargetWiggle.enabled = true;
			if (WigglePosition)
			{
				TargetWiggle.PositionWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
				TargetWiggle.WigglePosition(ApplyTimeMultiplier(WigglePositionDuration));
			}
			if (WiggleRotation)
			{
				TargetWiggle.RotationWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
				TargetWiggle.WiggleRotation(ApplyTimeMultiplier(WiggleRotationDuration));
			}
			if (WiggleScale)
			{
				TargetWiggle.ScaleWiggleProperties.UseUnscaledTime = Timing.TimescaleMode == TimescaleModes.Unscaled;
				TargetWiggle.WiggleScale(ApplyTimeMultiplier(WiggleScaleDuration));
			}
		}

		/// <summary>
		/// On Stop we change the state of our object if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetWiggle == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);

			TargetWiggle.enabled = false;
		}
	}
}