using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 플레이 시 (3D) 카메라의 줌을 변경할 수 있는 피드백
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("확대/축소 속성 정의: 특정 기간 동안 지정된 매개변수로 확대/축소를 설정합니다. " +
"세트는 그것들을 영원히 그대로 둘 것입니다. 확대/축소 속성에는 시야, 지속 시간이 포함됩니다." +
"줌 전환(초) 및 줌 지속 시간(카메라가 확대된 상태를 유지해야 하는 시간, 초). " +
"이 기능이 작동하려면 카메라에 MMCameraZoom 구성 요소를 추가해야 하며, 필요한 경우 MMCinemachineZoom을 추가해야 합니다." +
"가상 카메라를 사용합니다.")]
	[FeedbackPath("Camera/Camera Zoom")]
	public class MMFeedbackCameraZoom : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		[Header("Camera Zoom")]
		/// the channel to broadcast that zoom event on
		[Tooltip("the channel to broadcast that zoom event on")]
		public int Channel = 0;
		/// the zoom mode (for : forward for TransitionDuration, static for Duration, backwards for TransitionDuration)
		[Tooltip("확대/축소 모드(for: TransitionDuration의 경우 앞으로, Duration의 경우 정적, TransitionDuration의 경우 뒤로)")]
		public MMCameraZoomModes ZoomMode = MMCameraZoomModes.For;
		/// the target field of view
		[Tooltip("목표 시야")]
		public float ZoomFieldOfView = 30f;
		/// the zoom transition duration
		[Tooltip("확대/축소 전환 기간")]
		public float ZoomTransitionDuration = 0.05f;
		/// the duration for which the zoom is at max zoom
		[Tooltip("확대/축소가 최대 확대/축소 상태인 기간")]
		public float ZoomDuration = 0.1f;
		/// whether or not ZoomFieldOfView should add itself to the current camera's field of view value
		[Tooltip("ZoomFieldOfView가 현재 카메라의 시야 값에 추가되어야 하는지 여부")]
		public bool RelativeFieldOfView = false;

		/// the duration of this feedback is the duration of the zoom
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ZoomDuration); } set { ZoomDuration = value; } }

		/// <summary>
		/// On Play, triggers a zoom event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled, false, RelativeFieldOfView);
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
			MMCameraZoomEvent.Trigger(ZoomMode, ZoomFieldOfView, ZoomTransitionDuration, FeedbackDuration, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled, stop:true);
		}
	}
}