using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 플래시 이벤트(MMFlash에 의해 포착됨)를 트리거합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("플레이 시 이 피드백은 MMFlashEvent를 방송합니다. MMFlash 구성 요소가 포함된 UI 이미지를 생성하면(데모 장면의 예 참조) 해당 이벤트를 가로채서 플래시합니다(일반적으로 화면의 전체 크기를 차지하기를 원하지만 필수는 아닙니다). . 피드백 검사기에서 플래시 색상, 지속 시간, 알파 및 FlashID를 정의할 수 있습니다. 피드백과 MMFlash가 함께 작동하려면 해당 FlashID가 동일해야 합니다. 이를 통해 장면에 여러 개의 MMFlash를 두고 별도로 플래시할 수 있습니다.")]
	[FeedbackPath("Camera/Flash")]
	public class MMF_Flash : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// the duration of this feedback is the duration of the flash
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlashDuration); } set { FlashDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Flash", true, 37)]
		/// the color of the flash
		[Tooltip("플래시의 색상")]
		public Color FlashColor = Color.white;
		/// the flash duration (in seconds)
		[Tooltip("플래시 지속 시간(초)")]
		public float FlashDuration = 0.2f;
		/// the alpha of the flash
		[Tooltip("플래시의 알파")]
		public float FlashAlpha = 1f;
		/// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
		[Tooltip("플래시의 ID(보통 0)입니다. 각 MMFlash 개체에 ID를 지정하여 한 장면에서 서로 다른 플래시 이미지를 갖고 별도로 호출할 수 있습니다(손상용, 체력 회복용 등).")]
		public int FlashID = 0;

		/// <summary>
		/// On Play we trigger a flash event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration * intensityMultiplier, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode);
		}

		/// <summary>
		/// On stop we stop our transition
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
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
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
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
		}
	}
}