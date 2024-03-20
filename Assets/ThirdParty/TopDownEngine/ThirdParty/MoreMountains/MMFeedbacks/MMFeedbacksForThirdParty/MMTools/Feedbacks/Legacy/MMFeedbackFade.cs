using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 대상 FloatController에서 일회성 재생을 트리거합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 페이드 이벤트를 트리거할 수 있습니다.")]
	[FeedbackPath("Camera/Fade")]
	public class MMFeedbackFade : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif
		/// the different possible types of fades
		public enum FadeTypes { FadeIn, FadeOut, Custom }
        /// 위치를 페이더로 보내는 다양한 방법:
        /// - FeedbackPosition : 피드백 위치에서 페이드 및 선택적 오프셋
        /// - Transform : 지정된 변환 위치와 선택적 오프셋에서 페이드
        /// - WorldPosition : 지정된 월드 포지션 벡터와 선택적 오프셋에서 페이드
        /// - Script : 피드백을 호출할 때 매개변수에 전달된 위치
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

		[Header("Fade")]
		/// the type of fade we want to use when this feedback gets played
		[Tooltip("이 피드백이 재생될 때 사용하려는 페이드 유형")]
		public FadeTypes FadeType;
		/// the ID of the fader(s) to pilot
		[Tooltip("파일럿할 페이더의 ID")]
		public int ID = 0;
		/// the duration (in seconds) of the fade
		[Tooltip("페이드 지속 시간(초)")]
		public float Duration = 1f;
		/// the curve to use for this fade
		[Tooltip("이 페이드에 사용할 곡선")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// whether or not this fade should ignore timescale
		[Tooltip("이 페이드가 시간 척도를 무시해야 하는지 여부")]
		public bool IgnoreTimeScale = true;

		[Header("Custom")]
		/// the target alpha we're aiming for with this fade
		[Tooltip("이 페이드로 우리가 목표로 하는 목표 알파")]
		public float TargetAlpha;

		[Header("Position")]
		/// the chosen way to position the fade 
		[Tooltip("페이드 위치를 정하기 위해 선택한 방법")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the transform on which to center the fade
		[Tooltip("페이드의 중심이 되는 변환")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the coordinates on which to center the fadet
		[Tooltip("페이드의 중심이 되는 좌표")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset to apply when centering the fade
		[Tooltip("페이드를 중앙에 배치할 때 적용할 위치 오프셋")]
		public Vector3 PositionOffset;

		/// the duration of this feedback is the duration of the fade
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }

		protected Vector3 _position;
		protected FadeTypes _fadeType;

		/// <summary>
		/// On play we trigger the selected fade event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_position = GetPosition(position);
			_fadeType = FadeType;
			if (!NormalPlayDirection)
			{
				if (FadeType == FadeTypes.FadeIn)
				{
					_fadeType = FadeTypes.FadeOut;
				}
				else if (FadeType == FadeTypes.FadeOut)
				{
					_fadeType = FadeTypes.FadeIn;
				}
			}
			switch (_fadeType)
			{
				case FadeTypes.Custom:
					MMFadeEvent.Trigger(FeedbackDuration, TargetAlpha, Curve, ID, IgnoreTimeScale, _position);
					break;
				case FadeTypes.FadeIn:
					MMFadeInEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
					break;
				case FadeTypes.FadeOut:
					MMFadeOutEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
					break;
			}
		}

		/// <summary>
		/// Stops the animation if needed
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
			MMFadeStopEvent.Trigger(ID);
		}

		/// <summary>
		/// Computes the proper position for this fade
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return this.transform.position + PositionOffset;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset;
				case PositionModes.Script:
					return position + PositionOffset;
				default:
					return position + PositionOffset;
			}
		}
	}
}