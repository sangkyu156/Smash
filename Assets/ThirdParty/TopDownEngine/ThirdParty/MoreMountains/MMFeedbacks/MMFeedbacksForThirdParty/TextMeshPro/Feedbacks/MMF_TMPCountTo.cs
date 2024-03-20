using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 곡선에서 A에서 B로 값이 바뀌는 TMP 텍스트 값을 업데이트할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 곡선에서 A에서 B로 값이 바뀌는 TMP 텍스트 값을 업데이트할 수 있습니다.")]
	#if MM_TEXTMESHPRO
	[FeedbackPath("TextMesh Pro/TMP Count To")]
	#endif
	public class MMF_TMPCountTo : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTMPText be set to be able to work properly. You can set one below."; } }
		#endif
		#if UNITY_EDITOR && MM_TEXTMESHPRO
		public override bool EvaluateRequiresSetup() { return (TargetTMPText == null); }
		public override string RequiredTargetText { get { return TargetTMPText != null ? TargetTMPText.name : "";  } }
		#endif

		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
        
		#if MM_TEXTMESHPRO
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTMPText = FindAutomatedTarget<TMP_Text>();

		[MMFInspectorGroup("TextMeshPro Target Text", true, 12, true)]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("the target TMP_Text component we want to change the text on")]
		public TMP_Text TargetTMPText;
		#endif
        
		[MMFInspectorGroup("Count Settings", true, 13)]
		/// the value from which to count from
		[Tooltip("계산할 값")]
		public float CountFrom = 0f;
		/// the value to count towards
		[Tooltip("계산할 값")]
		public float CountTo = 10f;
		/// the curve on which to animate the count
		[Tooltip("카운트에 애니메이션을 적용할 곡선")]
		public MMTweenType CountingCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the duration of the count, in seconds
		[Tooltip("카운트 기간(초)")]
		public float Duration = 5f;
		/// the format with which to display the count
		[Tooltip("개수를 표시하는 형식")]
		public string Format = "00.00";
		/// whether or not value should be floored
		[Tooltip("가치를 바닥에 두어야 하는지 여부")]
		public bool FloorValues = true;
		/// the minimum frequency (in seconds) at which to refresh the text field
		[Tooltip("텍스트 필드를 새로 고치는 최소 빈도(초)")]
		public float MinRefreshFrequency = 0f;

		protected string _newText;
		protected float _startTime;
		protected float _lastRefreshAt;
		protected string _initialText;
        
		/// <summary>
		/// On play we change the text of our target TMPText over time
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

			_initialText = TargetTMPText.text;
			#endif
			Owner.StartCoroutine(CountCo());
		}

		/// <summary>
		/// A coroutine used to animate the text
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator CountCo()
		{
			_lastRefreshAt = -float.MaxValue;
			float currentValue = CountFrom;
			_startTime = FeedbackTime;
	        
			while (FeedbackTime - _startTime <= Duration)
			{
				if (FeedbackTime - _lastRefreshAt >= MinRefreshFrequency)
				{
					currentValue = ProcessCount();
					UpdateText(currentValue);
					_lastRefreshAt = FeedbackTime;
				}
		        
				yield return null;
			}
			UpdateText(CountTo);
		}

		/// <summary>
		/// Updates the text of the target TMPText component with the updated value
		/// </summary>
		/// <param name="currentValue"></param>
		protected virtual void UpdateText(float currentValue)
		{
			if (FloorValues)
			{
				_newText = Mathf.Floor(currentValue).ToString(Format);
			}
			else
			{
				_newText = currentValue.ToString(Format);
			}
	        
			#if MM_TEXTMESHPRO
			TargetTMPText.text = _newText;
			#endif
		}

		/// <summary>
		/// Computes the new value of the count for the current time
		/// </summary>
		/// <param name="currentValue"></param>
		/// <returns></returns>
		protected virtual float ProcessCount()
		{
			float currentTime = FeedbackTime - _startTime;
			float currentValue = MMTween.Tween(currentTime, 0f, Duration, CountFrom, CountTo, CountingCurve);
			return currentValue;
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			#if MM_TEXTMESHPRO
			TargetTMPText.text = _initialText;
			#endif
		}
	}
}