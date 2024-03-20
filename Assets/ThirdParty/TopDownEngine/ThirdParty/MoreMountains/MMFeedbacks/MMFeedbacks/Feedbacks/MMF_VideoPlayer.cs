using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 모든 종류의 방법(재생, 일시 중지, 전환, 중지, 준비, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop)으로 비디오 플레이어를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 모든 종류의 방법(재생, 일시 중지, 전환, 중지, 준비, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop)으로 비디오 플레이어를 제어할 수 있습니다.")]
	[FeedbackPath("UI/Video Player")]
	public class MMF_VideoPlayer : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum VideoActions { Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop  }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetVideoPlayer == null); }
		public override string RequiredTargetText { get { return TargetVideoPlayer != null ? TargetVideoPlayer.name + " " + VideoAction.ToString() : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetVideoPlayer be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetVideoPlayer = FindAutomatedTarget<VideoPlayer>();

		[MMFInspectorGroup("Video Player", true, 58, true)]
		/// the Video Player to control with this feedback
		[Tooltip("이 피드백으로 제어할 비디오 플레이어")]
		public VideoPlayer TargetVideoPlayer;
		/// the Video Player to control with this feedback
		[Tooltip("이 피드백으로 제어할 비디오 플레이어")]
		public VideoActions VideoAction = VideoActions.Pause;
		/// the frame at which to jump when in GoToFrame mode
		[Tooltip("GoToFrame 모드에 있을 때 점프할 프레임")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.GoToFrame)]
		public long TargetFrame = 10;
		/// the new playback speed (between 0 and 10)
		[Tooltip("새로운 재생 속도(0에서 10 사이)")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetPlaybackSpeed)]
		public float PlaybackSpeed = 2f;
		/// the track index on which to control volume
		[Tooltip("볼륨을 제어할 트랙 인덱스")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute, (int)VideoActions.SetDirectAudioVolume)]
		public int TrackIndex = 0;
		/// the new volume for the specified track, between 0 and 1
		[Tooltip("지정된 트랙의 새 볼륨(0과 1 사이)")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioVolume)]
		public float Volume = 1f;
		/// whether to mute the track or not when that feedback plays
		[Tooltip("해당 피드백이 재생될 때 트랙을 음소거할지 여부")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute)]
		public bool Mute = true;

		/// <summary>
		/// On play we apply the selected command to our target video player
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetVideoPlayer == null)
			{
				return;
			}

			switch (VideoAction)
			{
				case VideoActions.Play:
					TargetVideoPlayer.Play();
					break;
				case VideoActions.Pause:
					TargetVideoPlayer.Pause();
					break;
				case VideoActions.Toggle:
					if (TargetVideoPlayer.isPlaying)
					{
						TargetVideoPlayer.Pause();
					}
					else
					{
						TargetVideoPlayer.Play();
					}
					break;
				case VideoActions.Stop:
					TargetVideoPlayer.Stop();
					break;
				case VideoActions.Prepare:
					TargetVideoPlayer.Prepare();
					break;
				case VideoActions.StepForward:
					TargetVideoPlayer.StepForward();
					break;
				case VideoActions.StepBackward:
					TargetVideoPlayer.Pause();
					TargetVideoPlayer.frame = TargetVideoPlayer.frame - 1;
					break;
				case VideoActions.SetPlaybackSpeed:
					TargetVideoPlayer.playbackSpeed = PlaybackSpeed;
					break;
				case VideoActions.SetDirectAudioVolume:
					TargetVideoPlayer.SetDirectAudioVolume((ushort)TrackIndex, Volume);
					break;
				case VideoActions.SetDirectAudioMute:
					TargetVideoPlayer.SetDirectAudioMute((ushort)TrackIndex, Mute);
					break;
				case VideoActions.GoToFrame:
					TargetVideoPlayer.frame = TargetFrame;
					break;
				case VideoActions.ToggleLoop:
					TargetVideoPlayer.isLooping = !TargetVideoPlayer.isLooping;
					break;
			}

		}
	}
}