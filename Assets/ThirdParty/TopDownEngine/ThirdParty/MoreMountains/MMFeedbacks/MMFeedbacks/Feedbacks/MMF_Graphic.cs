using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 시간이 지남에 따라 대상 그래픽의 색상을 변경할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 시간이 지남에 따라 대상 그래픽의 색상을 변경할 수 있습니다.")]
	[FeedbackPath("UI/Graphic")]
	public class MMF_Graphic : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGraphic == null); }
		public override string RequiredTargetText { get { return TargetGraphic != null ? TargetGraphic.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetGraphic be set to be able to work properly. You can set one below."; } }
		#endif

		/// the duration of this feedback is the duration of the Graphic, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGraphic = FindAutomatedTarget<Graphic>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Graphic", true, 54, true)]
		/// the Graphic to affect when playing the feedback
		[Tooltip("피드백을 재생할 때 영향을 미치는 그래픽")]
		public Graphic TargetGraphic;
		/// whether the feedback should affect the Graphic instantly or over a period of time
		[Tooltip("피드백이 그래픽에 즉시 영향을 미칠지, 아니면 일정 기간에 걸쳐 영향을 미칠지 여부")]
		public Modes Mode = Modes.OverTime;
		/// how long the Graphic should change over time
		[Tooltip("시간이 지남에 따라 그래픽이 얼마나 오랫동안 변경되어야 하는지")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not that Graphic should be turned off on start
		[Tooltip("시작 시 그래픽을 꺼야 하는지 여부")]
		public bool StartsOff = false;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("이것이 사실이라면 이 피드백이 중지되면 대상이 비활성화됩니다.")] 
		public bool DisableOnStop = false;
        
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("이것이 사실이라면 피드백이 진행 중이더라도 해당 피드백을 호출하면 트리거됩니다. 거짓인 경우 현재 재생이 끝날 때까지 새로운 재생이 금지됩니다.")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the Graphic
		[Tooltip("그래픽 색상 수정 여부")]
		public bool ModifyColor = true;
		/// the colors to apply to the Graphic over time
		[Tooltip("시간이 지남에 따라 그래픽에 적용할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("인스턴트 모드에서 이동할 색상")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;

		protected Coroutine _coroutine;
		protected Color _initialColor;

		/// <summary>
		/// On init we turn the Graphic off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}
		}

		/// <summary>
		/// On Play we turn our Graphic on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialColor = TargetGraphic.color;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						TargetGraphic.color = InstantColor;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_coroutine = Owner.StartCoroutine(GraphicSequence());
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the Graphic
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator GraphicSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetGraphicValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetGraphicValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the Graphic on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetGraphicValues(float time)
		{
			if (ModifyColor)
			{
				TargetGraphic.color = ColorOverTime.Evaluate(time);
			}
		}

		/// <summary>
		/// Turns the Graphic off on stop
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
		}

		/// <summary>
		/// Turns the Graphic on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			TargetGraphic.gameObject.SetActive(status);
			TargetGraphic.enabled = status;
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
			TargetGraphic.color = _initialColor;
		}
	}
}