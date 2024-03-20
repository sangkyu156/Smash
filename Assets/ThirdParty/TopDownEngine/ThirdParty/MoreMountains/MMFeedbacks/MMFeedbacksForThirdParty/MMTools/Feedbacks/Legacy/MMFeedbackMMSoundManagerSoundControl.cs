using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 처음에 재생한 사운드의 SoundID와 일치해야 하는 SoundID를 대상으로 하는 특정 사운드(들)을 제어할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Sound Control")]
	[FeedbackHelp("이 피드백을 사용하면 처음에 재생한 사운드의 SoundID와 일치해야 하는 SoundID를 대상으로 하는 특정 사운드(들)을 제어할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMFeedbackMMSoundManagerSoundControl : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		[Header("MMSoundManager Sound Control")]
		/// the action to trigger on the specified sound
		[Tooltip("지정된 사운드에서 트리거할 작업")]
		public MMSoundManagerSoundControlEventTypes ControlMode = MMSoundManagerSoundControlEventTypes.Pause;
		/// the ID of the sound, has to match the one you specified when playing it
		[Tooltip("사운드 ID는 재생 시 지정한 ID와 일치해야 합니다.")]
		public int SoundID = 0;

		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// On play, triggers an event meant to be caught by the MMSoundManager and acted upon
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMSoundManagerSoundControlEvent.Trigger(ControlMode, SoundID);
		}
	}
}