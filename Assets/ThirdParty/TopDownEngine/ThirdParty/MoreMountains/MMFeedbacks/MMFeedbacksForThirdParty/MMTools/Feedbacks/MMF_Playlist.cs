using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 MMPlaylist를 시험해 볼 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 MMPlaylist를 시험해 볼 수 있습니다.")]
	[FeedbackPath("Audio/MMPlaylist")]
	public class MMF_Playlist : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get => Mode.ToString(); }
		public override bool HasChannel => true;
		#endif
		
		public enum Modes { Play, PlayNext, PlayPrevious, Stop, Pause, PlaySongAt }
 
		[MMFInspectorGroup("MMPlaylist", true, 13)]
		/// the action to call on the playlist
		[Tooltip("재생목록에서 호출할 작업")]
		public Modes Mode = Modes.PlayNext;
		/// the index of the song to play
		[Tooltip("재생할 노래의 인덱스")]
		[MMEnumCondition("Mode", (int)Modes.PlaySongAt)]
		public int SongIndex = 0;
        
		protected Coroutine _coroutine;

		/// <summary>
		/// On Play we change the values of our fog
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.Play:
					MMPlaylistPlayEvent.Trigger(Channel);
					break;
				case Modes.PlayNext:
					MMPlaylistPlayNextEvent.Trigger(Channel);
					break;
				case Modes.PlayPrevious:
					MMPlaylistPlayPreviousEvent.Trigger(Channel);
					break;
				case Modes.Stop:
					MMPlaylistStopEvent.Trigger(Channel);
					break;
				case Modes.Pause:
					MMPlaylistPauseEvent.Trigger(Channel);
					break;
				case Modes.PlaySongAt:
					MMPlaylistPlayIndexEvent.Trigger(Channel, SongIndex);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
            
		}
	}
}