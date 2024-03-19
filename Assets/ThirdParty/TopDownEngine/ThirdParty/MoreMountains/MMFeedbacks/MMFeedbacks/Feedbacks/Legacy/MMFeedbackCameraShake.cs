using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 흔들림 이벤트를 전송합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("카메라 흔들림 속성(초 단위의 지속 시간, 진폭 및 빈도)을 정의하면 동일한 설정으로 MMCameraShakeEvent가 브로드캐스팅됩니다. " +
"이 기능이 작동하려면 카메라에 MMCameraShaker를 추가해야 합니다(또는 Cinemachine을 사용하는 경우 가상 카메라에 MMCinemachineCameraShaker 구성 요소)." +
"이 이벤트와 시스템은 카메라용으로 제작되었지만 기술적으로는 다른 물체를 흔들 때도 사용할 수 있습니다.")]
	[FeedbackPath("Camera/Camera Shake")]
	public class MMFeedbackCameraShake : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif

		[Header("Camera Shake")]
		/// whether or not this shake should repeat forever, until stopped
		[Tooltip("이 흔들림이 멈출 때까지 영원히 반복되어야 하는지 여부")]
		public bool RepeatUntilStopped = false;
		/// the channel to broadcast this shake on
		[Tooltip("이 셰이크를 방송할 채널")]
		public int Channel = 0;
		/// the properties of the shake (duration, intensity, frequenc)
		[Tooltip("흔들림의 특성(지속 시간, 강도, 빈도)")]
		public MMCameraShakeProperties CameraShakeProperties = new MMCameraShakeProperties(0.1f, 0.2f, 40f);
        
		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(CameraShakeProperties.Duration); } set { CameraShakeProperties.Duration = value; } }

		/// <summary>
		/// On Play, sends a shake camera event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMCameraShakeEvent.Trigger(FeedbackDuration, CameraShakeProperties.Amplitude * intensityMultiplier, CameraShakeProperties.Frequency, 
				CameraShakeProperties.AmplitudeX * intensityMultiplier, CameraShakeProperties.AmplitudeY * intensityMultiplier, CameraShakeProperties.AmplitudeZ * intensityMultiplier,
				RepeatUntilStopped, ChannelData(Channel), Timing.TimescaleMode == TimescaleModes.Unscaled);
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMCameraShakeStopEvent.Trigger(ChannelData(Channel));
		}
	}
}