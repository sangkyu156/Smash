using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 대상 텍스트의 색상을 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 텍스트의 색상을 제어할 수 있습니다.")]
	[FeedbackPath("UI/Text Color")]
	public class MMFeedbackTextColor : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return (ColorMode == ColorModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// the Text component to control
		[Tooltip("제어할 텍스트 구성요소")]
		public Text TargetText;

		[Header("Color")]
        /// 선택한 색상 모드 :
        /// None : 아무 일도 일어나지 않습니다.
        /// gradient : 왼쪽에서 오른쪽으로 해당 그래디언트의 시간 경과에 따른 색상을 평가합니다.
        /// interpolate : 현재 색상에서 대상 색상으로 이동합니다.
        [Tooltip("선택한 색상 모드 :" +
                 "None : 아무 일도 일어나지 않습니다.," +
                 "gradient : 왼쪽에서 오른쪽으로 해당 그래디언트의 시간 경과에 따른 색상을 평가합니다." +
                 "interpolate : 현재 색상에서 대상 색상으로 이동합니다.")]
		public ColorModes ColorMode = ColorModes.Interpolate;
		/// how long the color of the text should change over time
		[Tooltip("시간이 지남에 따라 텍스트 색상이 얼마나 오랫동안 변경되어야 하는지")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate, (int)ColorModes.Gradient)]
		public float Duration = 0.2f;
		/// the color to apply
		[Tooltip("적용할 색상")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Instant)]
		public Color InstantColor = Color.yellow;
		/// the gradient to use to animate the color over time
		[Tooltip("시간이 지남에 따라 색상에 애니메이션을 적용하는 데 사용할 그라데이션")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorGradient;
		/// the destination color when in interpolate mode
		[Tooltip("보간 모드에 있을 때의 대상 색상")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public Color DestinationColor = Color.yellow;
		/// the curve to use when interpolating towards the destination color
		[Tooltip("대상 색상을 향해 보간할 때 사용할 곡선")]
		[MMFEnumCondition("ColorMode", (int)ColorModes.Interpolate)]
		public AnimationCurve ColorCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;

		protected Color _initialColor;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial color
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			if (TargetText == null)
			{
				return;
			}

			_initialColor = TargetText.color;
		}

		/// <summary>
		/// On Play we change our text's color
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetText == null))
			{
				return;
			}

			switch (ColorMode)
			{
				case ColorModes.Instant:
					TargetText.color = InstantColor;
					break;
				case ColorModes.Gradient:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeColor());
					break;
				case ColorModes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeColor());
					break;
			}
		}

		/// <summary>
		/// Changes the color of the text over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeColor()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
            
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetColor(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetColor(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// Applies the color change
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetColor(float time)
		{
			if (ColorMode == ColorModes.Gradient)
			{
				TargetText.color = ColorGradient.Evaluate(time);
			}
			else if (ColorMode == ColorModes.Interpolate)
			{
				float factor = ColorCurve.Evaluate(time);
				TargetText.color = Color.LerpUnclamped(_initialColor, DestinationColor, factor);
			}
		}

		/// <summary>
		/// Stops the coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && (_coroutine != null))
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}
	}
}