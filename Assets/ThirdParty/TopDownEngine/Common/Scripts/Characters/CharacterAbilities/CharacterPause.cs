using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 일시 정지를 활성화/비활성화할 수 있습니다.
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Pause")]
	public class CharacterPause : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Allows this character (and the player controlling it) to press the pause button to pause the game."; }
		
		[Header("Pause audio tracks")]
		/// whether or not to mute the sfx track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("게임이 일시 정지될 때 sfx 트랙을 음소거할지, 일시 정지가 해제되면 음소거를 해제할지 여부")]
		public bool MuteSfxTrackSounds = true;
		/// whether or not to mute the UI track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("게임이 일시 중지될 때 UI 트랙을 음소거할지, 일시 중지가 해제될 때 음소거를 해제할지 여부")]
		public bool MuteUITrackSounds = false;
		/// whether or not to mute the music track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("게임이 일시 중지될 때 음악 트랙을 음소거할지 여부와 일시 중지가 해제될 때 음소거를 해제할지 여부")]
		public bool MuteMusicTrackSounds = false;
		/// whether or not to mute the master track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("게임이 일시 중지될 때 마스터 트랙을 음소거할지, 일시 중지가 해제될 때 음소거를 해제할지 여부")]
		public bool MuteMasterTrackSounds = false;

		[Header("Hooks")] 
		/// a UnityEvent that will trigger when the game pauses 
		[Tooltip("게임이 일시 중지될 때 트리거되는 UnityEvent")]
		public UnityEvent OnPause;
		/// a UnityEvent that will trigger when the game unpauses
		[Tooltip("게임이 일시정지 해제될 때 트리거되는 UnityEvent")]
		public UnityEvent OnUnpause;


        //'esc' 처음에 누르고 설치 취소하면 정상인데 설치 취소부터 하면 퍼즈 걸리는 현상 때문에 시작하지마자 퍼즈걸었다 품
        protected override void Start()
        {
            base.Start();
			TriggerPause();
            TriggerPause();
        }

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