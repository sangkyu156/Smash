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
    /// 이 피드백을 사용하면 특정 트랙(마스터, UI, 음악, sfx)에서 재생되는 모든 사운드를 제어하고 재생, 일시 중지, 음소거, 음소거 해제, 재개, 중지, 해제를 한 번에 수행할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Track Control")]
	[FeedbackHelp("이 피드백을 사용하면 특정 트랙(마스터, UI, 음악, sfx)에서 재생되는 모든 사운드를 제어하고 재생, 일시 중지, 음소거, 음소거 해제, 재개, 중지, 해제를 한 번에 수행할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMFeedbackMMSoundManagerTrackControl : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif
        
		/// the possible modes you can use to interact with the track. Free will stop all sounds and return them to the pool
		public enum ControlModes { Mute, UnMute, SetVolume, Pause, Play, Stop, Free }
        
		[Header("MMSoundManager Track Control")]
		/// the track to mute/unmute/pause/play/stop/free/etc
		[Tooltip("음소거/음소거 해제/일시 중지/재생/중지/무료/등을 위한 트랙")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the selected control mode to interact with the track. Free will stop all sounds and return them to the pool
		[Tooltip("트랙과 상호 작용하기 위해 선택한 제어 모드. Free는 모든 소리를 중지하고 풀로 반환합니다.")]
		public ControlModes ControlMode = ControlModes.Pause;
		/// if setting the volume, the volume to assign to the track 
		[Tooltip("볼륨을 설정하는 경우 트랙에 할당할 볼륨")]
		[MMEnumCondition("ControlMode", (int) ControlModes.SetVolume)]
		public float Volume = 0.5f;

		/// <summary>
		/// On play, orders the entire track to follow the specific command, via a MMSoundManager event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (ControlMode)
			{
				case ControlModes.Mute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, Track);
					break;
				case ControlModes.UnMute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, Track);
					break;
				case ControlModes.SetVolume:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, Track, Volume);
					break;
				case ControlModes.Pause:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PauseTrack, Track);
					break;
				case ControlModes.Play:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PlayTrack, Track);
					break;
				case ControlModes.Stop:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.StopTrack, Track);
					break;
				case ControlModes.Free:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.FreeTrack, Track);
					break;
			}
		}
	}
}