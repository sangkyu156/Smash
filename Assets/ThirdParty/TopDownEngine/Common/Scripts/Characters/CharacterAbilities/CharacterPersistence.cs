using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 새 장면으로 전환할 때 정확한 현재 상태가 유지됩니다.
    /// 이 장면의 메인 캐릭터로 사용되도록 새 장면의 LevelManager에 자동으로 전달됩니다.
    /// 레벨을 완료하는 순간 모든 구성 요소의 정확한 상태를 유지합니다.
    /// 체력, 활성화된 능력, 구성 요소 값, 장착된 무기, 추가한 새 구성 요소 등은 모두 새 장면에 한 번만 유지됩니다.
    /// Animator parameters : None
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Persistence")]
	public class CharacterPersistence : CharacterAbility, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
	{
		public bool Initialized { get; set; }
        
		/// <summary>
		/// On Start(), we prevent our character from being destroyed if needed
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();

			if (AbilityAuthorized)
			{
				DontDestroyOnLoad(this.gameObject);
			}

			Initialized = true;
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			Initialized = false;
		}

		/// <summary>
		/// When we get a save request, we store our character in the game manager for future use
		/// </summary>
		/// <param name="gameEvent"></param>
		public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == "Save")
			{
				SaveCharacter();
			}
		}

		/// <summary>
		/// When we get a TopDown Engine event, we act on it
		/// </summary>
		/// <param name="gameEvent"></param>
		public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			if (!AbilityAuthorized)
			{
				return;
			}

			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LoadNextScene:
					this.gameObject.SetActive(false);
					break;
				case TopDownEngineEventTypes.SpawnCharacterStarts:
					this.transform.position = LevelManager.Instance.InitialSpawnPoint.transform.position;
					this.gameObject.SetActive(true);
					Character character = this.gameObject.GetComponentInParent<Character>(); 
					character.enabled = true;
					character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
					character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
					character.SetInputManager();
					break;
				case TopDownEngineEventTypes.LevelStart:
					if (_health != null)
					{
						_health.StoreInitialPosition();    
					}
					break;
				case TopDownEngineEventTypes.RespawnComplete:
					Initialized = true;
					break;
			}
		}

		/// <summary>
		/// Saves to the game manager a reference to our character
		/// </summary>
		protected virtual void SaveCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			GameManager.Instance.PersistentCharacter = _character;
		}

		/// <summary>
		/// Clears any saved character that may have been stored in the GameManager
		/// </summary>
		public virtual void ClearSavedCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			GameManager.Instance.PersistentCharacter = null;
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}