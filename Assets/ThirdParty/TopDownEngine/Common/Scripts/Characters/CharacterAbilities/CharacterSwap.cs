using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 기능을 캐릭터에 추가하면 장면에서 교체할 캐릭터 풀의 일부가 됩니다.
    /// 이 작업을 수행하려면 장면에 CharacterSwapManager가 필요합니다.
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Swap")]
	public class CharacterSwap : CharacterAbility
	{
		[Header("Character Swap")]
		/// the order in which this character should be picked 
		[Tooltip("이 캐릭터를 선택해야 하는 순서")]
		public int Order = 0;
		/// the playerID to put back in the Character class once this character gets swapped
		[Tooltip("이 캐릭터가 교체되면 Character 클래스에 다시 넣을 플레이어 ID입니다.")]
		public string PlayerID = "Player1";

		[Header("AI")] 
		/// if this is true, the AI Brain (if there's one on this character) will reset on swap
		[Tooltip("이것이 사실이라면 AI 두뇌(이 캐릭터에 AI 두뇌가 있는 경우)는 교환 시 재설정됩니다.")]
		public bool ResetAIBrainOnSwap = true;

		protected string _savedPlayerID;
		protected Character.CharacterTypes _savedCharacterType;

		/// <summary>
		/// On init, we grab our character type and playerID and store them for later
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_savedCharacterType = _character.CharacterType;
			_savedPlayerID = _character.PlayerID;
		}

		/// <summary>
		/// Called by the CharacterSwapManager, changes this character's type and sets its input manager
		/// </summary>
		public virtual void SwapToThisCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			PlayAbilityStartFeedbacks();
			_character.PlayerID = PlayerID;
			_character.CharacterType = Character.CharacterTypes.Player;
			_character.SetInputManager();
			if (_character.CharacterBrain != null)
			{
				_character.CharacterBrain.BrainActive = false;
			}
		}

		/// <summary>
		/// Called when another character replaces this one as the active one, resets its type and player ID and kills its input
		/// </summary>
		public virtual void ResetCharacterSwap()
		{
			_character.CharacterType = Character.CharacterTypes.AI;
			_character.PlayerID = _savedPlayerID;
			_character.SetInputManager(null);
			_characterMovement.SetHorizontalMovement(0f);
			_characterMovement.SetVerticalMovement(0f);
			_character.ResetInput();
			if (_character.CharacterBrain != null)
			{
				_character.CharacterBrain.BrainActive = true;
				if (ResetAIBrainOnSwap)
				{
					_character.CharacterBrain.ResetBrain();    
				}
			}
		}

		/// <summary>
		/// Returns true if this character is the currently active swap character
		/// </summary>
		/// <returns></returns>
		public virtual bool Current()
		{
			return (_character.CharacterType == Character.CharacterTypes.Player);
		}
	}
}