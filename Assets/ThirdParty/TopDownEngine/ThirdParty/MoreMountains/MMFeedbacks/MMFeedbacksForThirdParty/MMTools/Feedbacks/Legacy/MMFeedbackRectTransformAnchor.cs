using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 시간 경과에 따른 RectTransform의 최소 및 최대 앵커를 제어할 수 있습니다. 이는 왼쪽 하단과 오른쪽 상단 모서리가 고정되는 상위 RectTransform의 정규화된 위치입니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 시간 경과에 따른 RectTransform의 최소 및 최대 앵커를 제어할 수 있습니다. 이는 왼쪽 하단과 오른쪽 상단 모서리가 고정되는 상위 RectTransform의 정규화된 위치입니다.")]
	[FeedbackPath("UI/RectTransform Anchor")]
	public class MMFeedbackRectTransformAnchor : MMFeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// the target RectTransform to control
		[Tooltip("제어할 대상 RectTransform")]
		public RectTransform TargetRectTransform;

		[Header("Anchor Min")]
		/// whether or not to modify the min anchor
		[Tooltip("최소 앵커 수정 여부")]
		public bool ModifyAnchorMin = true;
		/// the curve to animate the min anchor on
		[Tooltip("the curve to animate the min anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType AnchorMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min anchor curve's 0 on
		[Tooltip("the value to remap the min anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMinRemapZero = Vector2.zero;
		/// the value to remap the min anchor curve's 1 on
		[Tooltip("the value to remap the min anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMinRemapOne = Vector2.one;
        
		[Header("Anchor Max")]
		/// whether or not to modify the max anchor
		[Tooltip("최대 앵커 수정 여부")]
		public bool ModifyAnchorMax = true;
		/// the curve to animate the max anchor on
		[Tooltip("the curve to animate the max anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType AnchorMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max anchor curve's 0 on
		[Tooltip("the value to remap the max anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMaxRemapZero = Vector2.zero;
		/// the value to remap the max anchor curve's 1 on
		[Tooltip("the value to remap the max anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMaxRemapOne = Vector2.one;
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMFeedbackBaseTarget targetMin = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiverMin = new MMPropertyReceiver();
			receiverMin.TargetObject = TargetRectTransform.gameObject;
			receiverMin.TargetComponent = TargetRectTransform;
			receiverMin.TargetPropertyName = "anchorMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = AnchorMinRemapZero;
			receiverMin.Vector2RemapOne = AnchorMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyAnchorMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = AnchorMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMFeedbackBaseTarget targetMax = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "anchorMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = AnchorMaxRemapZero;
			receiverMax.Vector2RemapOne = AnchorMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyAnchorMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = AnchorMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}

	}
}