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
	public class MMF_MMSoundManagerTrackControl : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return Track.ToString() + " " + ControlMode.ToString();  } }
		#endif
        
		/// the possible modes you can use to interact with the track. Free will stop all sounds and return them to the pool
		public enum ControlModes { Mute, UnMute, SetVolume, Pause, Play, Stop, Free }
        
		[MMFInspectorGroup("MMSoundManager Track Control", true, 30)]
		/// the track to mute/unmute/pause/play/stop/free/etc
		[Tooltip("the track to mute/unmute/pause/play/stop/free/etc")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the selected control mode to interact with the track. Free will stop all sounds and return them to the pool
		[Tooltip("the selected control mode to interact with the track. Free will stop all sounds and return them to the pool")]
		public ControlModes ControlMode = ControlModes.Pause;
		/// if setting the volume, the volume to assign to the track 
		[Tooltip("if setting the volume, the volume to assign to the track")]
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