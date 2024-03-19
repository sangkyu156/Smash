using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 간단한 작업을 통해 런타임 시 AI의 두뇌를 검사기에 지정된 새 두뇌로 교체할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Swap Brain")]
	public class AIActionSwapBrain : AIAction
	{
        /// 캐릭터의 두뇌를 대체할 두뇌
        [Tooltip("캐릭터의 두뇌를 대체할 두뇌")]
		public AIBrain NewAIBrain;

		protected Character _character;

		/// <summary>
		/// On init, we grab and store our Character
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
		}

		/// <summary>
		/// On PerformAction we swap our brain
		/// </summary>
		public override void PerformAction()
		{
			SwapBrain();
		}

		/// <summary>
		/// Disables the old brain, swaps it with a new one and enables it
		/// </summary>
		protected virtual void SwapBrain()
		{
			if (NewAIBrain == null) return;

			// we disable the "old" brain
			_character.CharacterBrain.gameObject.SetActive(false);
			_character.CharacterBrain.enabled = false;            
			// we swap it with the new one
			_character.CharacterBrain = NewAIBrain;
			// we enable the new one and reset it
			NewAIBrain.gameObject.SetActive(true);
			NewAIBrain.enabled = true;
			NewAIBrain.Owner = _character.gameObject;
			NewAIBrain.ResetBrain();
		}
	}
}