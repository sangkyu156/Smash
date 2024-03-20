using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간 경과에 따라 RectTransform의 크기 델타 속성(앵커 사이의 거리에 상대적인 이 RectTransform의 크기)을 제어할 수 있습니다. 
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간 경과에 따라 RectTransform의 크기 델타 속성(앵커 사이의 거리에 상대적인 이 RectTransform의 크기)을 제어할 수 있습니다.")]
	[FeedbackPath("UI/RectTransformSizeDelta")]
	public class MMF_RectTransformSizeDelta : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRectTransform == null); }
		public override string RequiredTargetText { get { return TargetRectTransform != null ? TargetRectTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRectTransform be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRectTransform = FindAutomatedTarget<RectTransform>();

		[MMFInspectorGroup("Target RectTransform", true, 37, true)]
		/// the rect transform we want to impact
		[Tooltip("the rect transform we want to impact")]
		public RectTransform TargetRectTransform;
        
		[MMFInspectorGroup("Size Delta", true, 38)] 
		/// the speed at which we should animate the size delta
		[Tooltip("크기 델타에 애니메이션을 적용해야 하는 속도")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType SpeedCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 RemapOne = Vector2.one;
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetRectTransform.gameObject;
			receiver.TargetComponent = TargetRectTransform;
			receiver.TargetPropertyName = "sizeDelta";
			receiver.RelativeValue = RelativeValues;
			receiver.Vector2RemapZero = RemapZero;
			receiver.Vector2RemapOne = RemapOne;
			target.Target = receiver;
			target.LevelCurve = SpeedCurve;
			target.RemapLevelZero = 0f;
			target.RemapLevelOne = 1f;
			target.InstantLevel = 1f;

			_targets.Add(target);
		}

	}
}