using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 대상 스프라이트 렌더러의 색상을 변경하고 X 또는 Y로 뒤집을 수 있습니다. 또한 이를 사용하여 하나 이상의 MMSpriteRendererShaker를 명령할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 이미지의 색상을 변경할 수 있습니다. 또한 이를 사용하여 하나 이상의 MMImageShaker에 명령을 내릴 수도 있습니다.")]
	[FeedbackPath("UI/Image")]
	public class MMF_Image : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundImage == null); }
		public override string RequiredTargetText { get { return BoundImage != null ? BoundImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundImage be set to be able to work properly. You can set one below."; } }
		#endif

		/// the duration of this feedback is the duration of the Image, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundImage = FindAutomatedTarget<Image>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Image", true, 54, true)]
		/// the Image to affect when playing the feedback
		[Tooltip("피드백을 재생할 때 영향을 미칠 이미지")]
		public Image BoundImage;
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("피드백이 이미지에 즉시 영향을 미칠지, 아니면 일정 기간에 걸쳐 영향을 미칠지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the Image should change over time
		[Tooltip("시간이 지남에 따라 이미지가 얼마나 오랫동안 변경되어야 하는지")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the image
		[Tooltip("이미지 색상 수정 여부")]
		public bool ModifyColor = true;
		/// the colors to apply to the Image over time
		[Tooltip("시간이 지남에 따라 이미지에 적용할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;
		/// whether or not that Image should be turned off on start
		[Tooltip("시작 시 해당 이미지를 꺼야 하는지 여부")]
		[FormerlySerializedAs("StartsOff")]
		public bool DisableOnInit = false;
		/// if this is true, the target will be enabled when this feedback gets played
		[Tooltip("이것이 사실이라면 이 피드백이 재생될 때 타겟이 활성화됩니다.")] 
		public bool EnableOnPlay = true;
		/// if this is true, the target disabled after the color over time change ends
		[Tooltip("이것이 사실이라면 시간 경과에 따른 색상 변경이 끝난 후 대상이 비활성화됩니다.")]
		public bool DisableOnSequenceEnd = false;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("이것이 사실이라면 이 피드백이 중지되면 대상이 비활성화됩니다.")] 
		public bool DisableOnStop = false;

		protected Coroutine _coroutine;
		protected Color _initialColor;

		/// <summary>
		/// On init we turn the Image off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (DisableOnInit)
				{
					Turn(false);
				}
			}
		}

		/// <summary>
		/// On Play we turn our Image on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			_initialColor = BoundImage.color;
			if (EnableOnPlay)
			{
				Turn(true);	
			}
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						BoundImage.color = InstantColor;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the Image
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ImageSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetImageValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetImageValues(FinalNormalizedTime);
			if (DisableOnSequenceEnd)
			{
				Turn(false);
			}
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetImageValues(float time)
		{
			if (ModifyColor)
			{
				BoundImage.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Turns the sprite renderer off on stop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && DisableOnStop)
			{
				Turn(false);    
			}
			_coroutine = null;
		}

		/// <summary>
		/// Turns the sprite renderer on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundImage.gameObject.SetActive(status);
			BoundImage.enabled = status;
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
			BoundImage.color = _initialColor;
		}
	}
}