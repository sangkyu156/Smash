using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this component to a character and it'll be able to activate/desactivate the pause
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Pause")]
	public class CharacterPause : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Allows this character (and the player controlling it) to press the pause button to pause the game."; }
		
		[Header("Pause audio tracks")]
		/// whether or not to mute the sfx track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("whether or not to mute the sfx track when the game pauses, and to unmute it when it unpauses")]
		public bool MuteSfxTrackSounds = true;
		/// whether or not to mute the UI track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("whether or not to mute the UI track when the game pauses, and to unmute it when it unpauses")]
		public bool MuteUITrackSounds = false;
		/// whether or not to mute the music track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("whether or not to mute the music track when the game pauses, and to unmute it when it unpauses")]
		public bool MuteMusicTrackSounds = false;
		/// whether or not to mute the master track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("whether or not to mute the master track when the game pauses, and to unmute it when it unpauses")]
		public bool MuteMasterTrackSounds = false;

		[Header("Hooks")] 
		/// a UnityEvent that will trigger when the game pauses 
		[Tooltip("a UnityEvent that will trigger when the game pauses")]
		public UnityEvent OnPause;
		/// a UnityEvent that will trigger when the game unpauses
		[Tooltip("a UnityEvent that will trigger when the game unpauses")]
		public UnityEvent OnUnpause;


        /// <summary>
        /// 매 프레임마다 입력을 확인하여 게임을 일시 중지/일시 중지 해제해야 하는지 확인합니다.
        /// </summary>
        protected override void HandleInput()
		{
			if (_inputManager.PauseButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				TriggerPause();
			}
		}

        /// <summary>
        /// 일시정지 버튼을 누르면 일시정지 상태가 변경됩니다.
        /// </summary>
        protected virtual void TriggerPause()
		{
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Dead)
			{
				return;
			}
			if (!AbilityAuthorized)
			{
				return;
			}
			PlayAbilityStartFeedbacks();
			// GameManager 및 이를 수신할 수 있는 다른 클래스에 대해 Pause 이벤트를 트리거합니다.
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
		}

		/// <summary>
		/// Puts the character in the pause state
		/// </summary>
		public virtual void PauseCharacter()
		{
			if (!this.enabled)
			{
				return;
			}
			_condition.ChangeState(CharacterStates.CharacterConditions.Paused);
			
			OnPause?.Invoke();

			if (MuteSfxTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx); }
			if (MuteUITrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.UI); }
			if (MuteMusicTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Music); }
			if (MuteMasterTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Master); }
		}

		/// <summary>
		/// Restores the character to the state it was in before the pause.
		/// </summary>
		public virtual void UnPauseCharacter()
		{
			if (!this.enabled)
			{
				return;
			}
			_condition.RestorePreviousState();

			OnUnpause?.Invoke();

			if (MuteSfxTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx); }
			if (MuteUITrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.UI); }
			if (MuteMusicTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Music); }
			if (MuteMasterTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Master); }
		}
	}
}