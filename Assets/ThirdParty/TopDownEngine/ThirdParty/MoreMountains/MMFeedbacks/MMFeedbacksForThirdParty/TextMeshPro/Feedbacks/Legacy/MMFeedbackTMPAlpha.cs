﻿using MoreMountains.Tools;
using UnityEngine;
using System.Collections;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 시간 경과에 따른 대상 TMP의 알파를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 시간 경과에 따른 대상 TMP의 알파를 제어할 수 있습니다.")]
	[FeedbackPath("TextMesh Pro/TMP Alpha")]
	public class MMFeedbackTMPAlpha : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum AlphaModes { Instant, Interpolate, ToDestination }

		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return (AlphaMode == AlphaModes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

		#if MM_TEXTMESHPRO
		[Header("Target")]
		/// the TMP_Text component to control
		[Tooltip("제어할 TMP_Text 구성 요소")]
		public TMP_Text TargetTMPText;
		#endif

		[Header("Alpha")]
        /// 선택한 색상 모드 :
        /// None : 아무것도 안 일어날거야,
        /// gradient : 왼쪽에서 오른쪽으로 해당 그래디언트의 시간 경과에 따른 색상을 평가합니다.
        /// interpolate : 현재 색상에서 대상 색상으로 이동합니다.
        [Tooltip("선택한 색상 모드 :" +
                 "Instant : 알파가 즉시 목표 알파로 변경됩니다." +
                 "Curve : 알파가 곡선을 따라 보간됩니다" +
                 "interpolate : 현재 색상에서 대상 색상으로 이동합니다.")]
		public AlphaModes AlphaMode = AlphaModes.Interpolate;
		/// how long the color of the text should change over time
		[Tooltip("시간이 지남에 따라 텍스트 색상이 얼마나 오랫동안 변경되어야 하는지")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate, (int)AlphaModes.ToDestination)]
		public float Duration = 0.2f;
		/// the alpha to apply when in instant mode
		[Tooltip("인스턴트 모드에 있을 때 적용할 알파")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Instant)]
		public float InstantAlpha = 1f;

		/// the curve to use when interpolating towards the destination alpha
		[Tooltip("대상 알파를 향해 보간할 때 사용할 곡선")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate, (int)AlphaModes.ToDestination)]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("곡선의 0이 다시 매핑되어야 하는 값")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate)]
		public float CurveRemapZero = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("곡선의 1을 다시 매핑해야 하는 값")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.Interpolate)]
		public float CurveRemapOne = 1f;
		/// the alpha to aim towards when in ToDestination mode
		[Tooltip("ToDestination 모드에 있을 때 목표로 삼을 알파")]
		[MMFEnumCondition("AlphaMode", (int)AlphaModes.ToDestination)]
		public float DestinationAlpha = 1f;

		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;

		protected float _initialAlpha;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial alpha
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);

			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}
			_initialAlpha = TargetTMPText.alpha;
			#endif
		}

		/// <summary>
		/// On Play we change our text's alpha
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}

			switch (AlphaMode)
			{
				case AlphaModes.Instant:
					TargetTMPText.alpha = InstantAlpha;
					break;
				case AlphaModes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = StartCoroutine(ChangeAlpha());
					break;
				case AlphaModes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_initialAlpha = TargetTMPText.alpha;
					_coroutine = StartCoroutine(ChangeAlpha());
					break;
			}
			#endif
		}

		/// <summary>
		/// Changes the color of the text over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeAlpha()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetAlpha(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetAlpha(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield break;
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
			IsPlaying = false;
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

		/// <summary>
		/// Applies the alpha change
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetAlpha(float time)
		{
			#if MM_TEXTMESHPRO
			float newAlpha = 0f;
			if (AlphaMode == AlphaModes.Interpolate)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, CurveRemapZero, CurveRemapOne, Curve);    
			}
			else if (AlphaMode == AlphaModes.ToDestination)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, _initialAlpha, DestinationAlpha, Curve);
			}
			TargetTMPText.alpha = newAlpha;
			#endif
		}
	}
}