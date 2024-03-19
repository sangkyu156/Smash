using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 SwitchCharacter 버튼을 누를 때 모델을 전환할 수 있습니다.
    /// 이는 프리팹이 아닌 모델만 변경한다는 점에 유의하세요. 능력이나 설정이 아닌 시각적 표현만 가능합니다.
    /// 대신 프리팹을 완전히 변경하려면 CharacterSwitchManager 클래스를 살펴보세요.
    /// 한 장면 내에서 여러 캐릭터 간에 캐릭터를 교환하려면 CharacterSwap 기능과 CharacterSwapManager를 살펴보세요.
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Switch Model")] 
	public class CharacterSwitchModel : CharacterAbility
	{
		/// the possible orders the next character can be selected from
		public enum NextModelChoices { Sequential, Random }

		[Header("Models")]
		[MMInformation("이 구성 요소를 캐릭터에 추가하면 SwitchCharacter 버튼(기본적으로 P)을 누를 때 모델을 전환할 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]

		/// the list of possible characters models to switch to
		[Tooltip("전환할 수 있는 캐릭터 모델 목록")]
		public GameObject[] CharacterModels;
		/// the order in which to pick the next character
		[Tooltip("다음 문자를 선택하는 순서")]
		public NextModelChoices NextCharacterChoice = NextModelChoices.Sequential;
		/// the initial (and at runtime, current) index of the character prefab
		[Tooltip("캐릭터 프리팹의 초기(그리고 런타임 시 현재) 인덱스")]
		public int CurrentIndex = 0;
		/// if you set this to true, when switching model, the Character's animator will also be bound. This requires your model's animator is at the top level of the model in the hierarchy.
		/// you can look at the MinimalModelSwitch scene for examples of that
		[Tooltip("이를 true로 설정하면 모델을 전환할 때 캐릭터의 애니메이터도 바인딩됩니다. 이를 위해서는 모델의 애니메이터가 계층 구조에서 모델의 최상위 수준에 있어야 합니다. 그 예를 보려면 MinimalModelSwitch 장면을 볼 수 있습니다.")]
		public bool AutoBindAnimator = true;

		[Header("Visual Effects")]
		/// a particle system to play when a character gets changed
		[Tooltip("캐릭터가 변경될 때 재생할 파티클 시스템")]
		public ParticleSystem CharacterSwitchVFX;

		protected ParticleSystem _instantiatedVFX;
		protected string _bindAnimatorMessage = "BindAnimator";
		protected bool[] _characterModelsFlipped;

		/// <summary>
		/// On init we disable our models and activate the current one
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (CharacterModels.Length == 0)
			{
				return;
			}

			foreach (GameObject model in CharacterModels)
			{
				model.SetActive(false);
			}

			CharacterModels[CurrentIndex].SetActive(true);
			_characterModelsFlipped = new bool[CharacterModels.Length];
			InstantiateVFX();
		}

		/// <summary>
		/// Instantiates and disables the particle system if needed
		/// </summary>
		protected virtual void InstantiateVFX()
		{
			if (CharacterSwitchVFX != null)
			{
				_instantiatedVFX = Instantiate(CharacterSwitchVFX);
				_instantiatedVFX.Stop();
				_instantiatedVFX.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// At the beginning of each cycle, we check if we've pressed or released the switch button
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchModel();
			}	
		}

		/// <summary>
		/// On flip we store our state for our current model
		/// </summary>
		public override void Flip()
		{
			if (_characterModelsFlipped == null)
			{
				_characterModelsFlipped = new bool[CharacterModels.Length];
			}
			if (_characterModelsFlipped.Length == 0)
			{
				_characterModelsFlipped = new bool[CharacterModels.Length];
			}
			if (_character == null)
			{
				_character = this.gameObject.GetComponentInParent<Character>();
			}
		}

		/// <summary>
		/// Switches to the next model in line
		/// </summary>
		protected virtual void SwitchModel()
		{
			if (CharacterModels.Length <= 1)
			{
				return;
			}
            
			CharacterModels[CurrentIndex].gameObject.SetActive(false);

			// we determine the next index
			if (NextCharacterChoice == NextModelChoices.Random)
			{
				CurrentIndex = Random.Range(0, CharacterModels.Length);
			}
			else
			{
				CurrentIndex = CurrentIndex + 1;
				if (CurrentIndex >= CharacterModels.Length)
				{
					CurrentIndex = 0;
				}
			}

			// we activate the new current model
			CharacterModels[CurrentIndex].gameObject.SetActive(true);
			_character.CharacterModel = CharacterModels[CurrentIndex];

			// we bind our animator
			if (AutoBindAnimator)
			{
				_character.CharacterAnimator = CharacterModels[CurrentIndex].gameObject.MMGetComponentNoAlloc<Animator>();
				_character.AssignAnimator(true);
				SendMessage(_bindAnimatorMessage, SendMessageOptions.DontRequireReceiver);                
			} 

			// we play our vfx
			if (_instantiatedVFX != null)
			{
				_instantiatedVFX.gameObject.SetActive(true);
				_instantiatedVFX.transform.position = this.transform.position;
				_instantiatedVFX.Play();
			}
			// we play our feedback
			PlayAbilityStartFeedbacks();
		}
	}
}