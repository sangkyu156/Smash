using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 왼쪽 하단 앵커를 기준으로 직사각형의 왼쪽 하단 모서리 오프셋과 오른쪽 상단 앵커를 기준으로 직사각형 오른쪽 상단 모서리의 오프셋을 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 왼쪽 하단 앵커를 기준으로 직사각형의 왼쪽 하단 모서리 오프셋과 오른쪽 상단 앵커를 기준으로 직사각형 오른쪽 상단 모서리의 오프셋을 제어할 수 있습니다.")]
	[FeedbackPath("UI/RectTransform Offset")]
	public class MMFeedbackRectTransformOffset : MMFeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// The RectTransform we want to modify
		public RectTransform TargetRectTransform;

		[Header("Offset Min")]
		/// whether we should modify the offset min or not
		[Tooltip("오프셋 최소값을 수정해야 하는지 여부")]
		public bool ModifyOffsetMin = true;
		/// the curve to animate the min offset on
		[Tooltip("the curve to animate the min offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType OffsetMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min curve's 0 on
		[Tooltip("the value to remap the min curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMinRemapZero = Vector2.zero;
		/// the value to remap the min curve's 1 on
		[Tooltip("the value to remap the min curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMinRemapOne = Vector2.one;
        
		[Header("Offset Max")]
		/// whether we should modify the offset max or not
		[Tooltip("오프셋 최대값을 수정해야 하는지 여부")]
		public bool ModifyOffsetMax = true;
		/// the curve to animate the max offset on
		[Tooltip("the curve to animate the max offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType OffsetMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max curve's 0 on
		[Tooltip("the value to remap the max curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMaxRemapZero = Vector2.zero;
		/// the value to remap the max curve's 1 on
		[Tooltip("the value to remap the max curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMaxRemapOne = Vector2.one;
        
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
			receiverMin.TargetPropertyName = "offsetMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = OffsetMinRemapZero;
			receiverMin.Vector2RemapOne = OffsetMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyOffsetMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = OffsetMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMFeedbackBaseTarget targetMax = new MMFeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "offsetMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = OffsetMaxRemapZero;
			receiverMax.Vector2RemapOne = OffsetMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyOffsetMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = OffsetMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}

	}
}