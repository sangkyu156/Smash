using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_CINEMACHINE
using Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 통해 카메라의 우선순위를 변경할 수 있습니다.
    /// 약간의 설정이 필요합니다. MMCinemachinePriorityListener를 고유한 채널 값과 함께 다양한 카메라에 추가합니다.
    /// 선택적으로 Cinemachine Brain에 MMCinemachinePriorityBrainListener를 추가하여 다양한 전환 유형과 기간을 처리할 수 있습니다.
    /// 그런 다음 피드백에 대한 채널과 새로운 우선순위를 선택하고 재생하기만 하면 됩니다. 마법의 전환!
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Camera/Cinemachine Transition")]
	[FeedbackHelp("이 피드백을 통해 카메라의 우선순위를 변경할 수 있습니다. 약간의 설정이 필요합니다: " +
"고유한 채널 값을 사용하여 다양한 카메라에 MMCinemachinePriorityListener를 추가합니다." +
"선택적으로 Cinemachine Brain에 MMCinemachinePriorityBrainListener를 추가하여 다양한 전환 유형과 기간을 처리할 수 있습니다." +
"그렇다면 당신이 해야 할 일은 피드백에 대한 채널과 새로운 우선순위를 선택하고 재생하는 것뿐입니다. 마법의 전환!")]
	public class MMFeedbackCinemachineTransition : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum Modes { Event, Binding }
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		#endif
		/// the duration of this feedback is the duration of the shake
		#if MM_CINEMACHINE
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(BlendDefintion.m_Time); } set { BlendDefintion.m_Time = value; } }
		#endif

		[Header("Cinemachine Transition")] 
		/// the selected mode (either via event, or via direct binding of a specific camera)
		[Tooltip("선택한 모드(이벤트를 통해 또는 특정 카메라의 직접 바인딩을 통해)")]
		public Modes Mode = Modes.Event;
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		#if MM_CINEMACHINE
		/// the virtual camera to target
		[Tooltip("대상으로 삼을 가상 카메라")]
		[MMFEnumCondition("Mode", (int)Modes.Binding)]
		public CinemachineVirtualCamera TargetVirtualCamera;
		#endif
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetValuesAfterTransition = true;

		[Header("Priority")]
		/// the new priority to apply to all virtual cameras on the specified channel
		[Tooltip("지정된 채널의 모든 가상 카메라에 적용할 새로운 우선순위")]
		public int NewPriority = 10;
		/// whether or not to force all virtual cameras on other channels to reset their priority to zero
		[Tooltip("다른 채널의 모든 가상 카메라를 강제로 우선순위를 0으로 재설정할지 여부")]
		public bool ForceMaxPriority = true;
		/// whether or not to apply a new blend
		[Tooltip("새로운 블렌드를 적용할지 여부")]
		public bool ForceTransition = false;
        
		#if MM_CINEMACHINE
		/// the new blend definition to apply
		[Tooltip("적용할 새 블렌드 정의")]
		[MMFCondition("ForceTransition", true)]
		public CinemachineBlendDefinition BlendDefintion;

		protected CinemachineBlendDefinition _tempBlend;
		#endif

		/// <summary>
		/// Triggers a priority change on listening virtual cameras
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			#if MM_CINEMACHINE
			_tempBlend = BlendDefintion;
			_tempBlend.m_Time = FeedbackDuration;
			if (Mode == Modes.Event)
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData(Channel), ForceMaxPriority, NewPriority, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode);    
			}
			else
			{
				MMCinemachinePriorityEvent.Trigger(ChannelData(Channel), ForceMaxPriority, 0, ForceTransition, _tempBlend, ResetValuesAfterTransition, Timing.TimescaleMode); 
				TargetVirtualCamera.Priority = NewPriority;
			}
			#endif
		}
	}
}