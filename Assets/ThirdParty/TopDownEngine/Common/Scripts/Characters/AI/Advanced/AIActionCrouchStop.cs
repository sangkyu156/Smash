using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 동작은 가능하다면 캐릭터가 웅크리는 것을 멈추도록 강제합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionCrouchStop")]
	//[RequireComponent(typeof(CharacterCrouch))]
	public class AIActionCrouchStop : AIAction
	{
		protected CharacterCrouch _characterCrouch;
		protected Character _character;

		/// <summary>
		/// Grabs dependencies
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterCrouch = _character?.FindAbility<CharacterCrouch>();
		}

		/// <summary>
		/// On PerformAction we stop crouching
		/// </summary>
		public override void PerformAction()
		{
			if ((_character == null) || (_characterCrouch == null))
			{
				return;
			}

			if ((_character.MovementState.CurrentState == CharacterStates.MovementStates.Crouching)
			    || (_character.MovementState.CurrentState == CharacterStates.MovementStates.Crawling))
			{
				_characterCrouch.StopForcedCrouch();
			}
		}
	}
}