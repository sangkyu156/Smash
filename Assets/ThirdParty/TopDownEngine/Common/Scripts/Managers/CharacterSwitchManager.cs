using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 장면의 빈 개체에 추가하고 SwitchCharacter 버튼(기본적으로 P는 Unity의 InputManager 설정에서 변경)을 누르면 주인공이 이 항목에 설정된 목록의 프리팹 중 하나로 대체됩니다. 요소. 순서(순차적 또는 무작위)를 결정하고 원하는 만큼 가질 수 있습니다.
    /// 이는 시각적 요소뿐만 아니라 전체 프리팹을 변경한다는 점에 유의하세요.
    /// 시각적 변화가 발생한 직후라면 CharacterSwitchModel 기능을 살펴보세요.
    /// 한 장면 내에서 여러 캐릭터 간에 캐릭터를 교환하려면 CharacterSwap 기능과 CharacterSwapManager를 살펴보세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/CharacterSwitchManager")]
	public class CharacterSwitchManager : TopDownMonoBehaviour
	{
		/// the possible orders the next character can be selected from
		public enum NextCharacterChoices { Sequential, Random }

		[Header("Character Switch")]
		[MMInformation("이 구성 요소를 장면의 빈 개체에 추가하고 SwitchCharacter 버튼(기본적으로 P는 Unity의 InputManager 설정에서 변경)을 누르면 주인공이 이 항목에 설정된 목록의 프리팹 중 하나로 대체됩니다. 요소. 순서(순차적 또는 무작위)를 결정하고 원하는 만큼 가질 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]

		/// the list of possible characters prefabs to switch to
		[Tooltip("전환할 수 있는 캐릭터 프리팹 목록")]
		public Character[] CharacterPrefabs;
		/// the order in which to pick the next character
		[Tooltip("다음 문자를 선택하는 순서")]
		public NextCharacterChoices NextCharacterChoice = NextCharacterChoices.Sequential;
		/// the initial (and at runtime, current) index of the character prefab
		[Tooltip("캐릭터 프리팹의 초기(그리고 런타임 시 현재) 인덱스")]
		public int CurrentIndex = 0;
		/// if this is true, current health value will be passed from character to character
		[Tooltip("이것이 사실이라면 현재 체력 값이 캐릭터에서 캐릭터로 전달됩니다.")]
		public bool CommonHealth;

		[Header("Visual Effects")]
		/// a particle system to play when a character gets changed
		[Tooltip("캐릭터가 변경될 때 재생할 파티클 시스템")]
		public ParticleSystem CharacterSwitchVFX;

		protected Character[] _instantiatedCharacters;
		protected ParticleSystem _instantiatedVFX;
		protected InputManager _inputManager;
		protected TopDownEngineEvent _switchEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwitch, null);

		/// <summary>
		/// On Awake we grab our input manager and instantiate our characters and VFX
		/// </summary>
		protected virtual void Start()
		{
			_inputManager = FindObjectOfType(typeof(InputManager)) as InputManager;
			InstantiateCharacters();
			InstantiateVFX();
		}

		/// <summary>
		/// Instantiates and disables all characters in our list
		/// </summary>
		protected virtual void InstantiateCharacters()
		{
			_instantiatedCharacters = new Character[CharacterPrefabs.Length];

			for (int i = 0; i < CharacterPrefabs.Length; i++)
			{
				Character newCharacter = Instantiate(CharacterPrefabs[i]);
				newCharacter.name = "CharacterSwitch_" + i;
				newCharacter.gameObject.SetActive(false);
				_instantiatedCharacters[i] = newCharacter;
			}
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
		/// On Update we watch for our input
		/// </summary>
		protected virtual void Update()
		{
			if (_inputManager == null)
			{
				return;
			}

			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchCharacter();
			}
		}

		/// <summary>
		/// Switches to the next character in the list
		/// </summary>
		protected virtual void SwitchCharacter()
		{
			if (_instantiatedCharacters.Length <= 1)
			{
				return;
			}

			// we determine the next index
			if (NextCharacterChoice == NextCharacterChoices.Random)
			{
				CurrentIndex = Random.Range(0, _instantiatedCharacters.Length);
			}
			else
			{
				CurrentIndex = CurrentIndex + 1;
				if (CurrentIndex >= _instantiatedCharacters.Length)
				{
					CurrentIndex = 0;
				}
			}

			// we disable the old main character, and enable the new one
			LevelManager.Instance.Players[0].gameObject.SetActive(false);
			_instantiatedCharacters[CurrentIndex].gameObject.SetActive(true);

			// we move the new one at the old one's position
			_instantiatedCharacters[CurrentIndex].transform.position = LevelManager.Instance.Players[0].transform.position;
			_instantiatedCharacters[CurrentIndex].transform.rotation = LevelManager.Instance.Players[0].transform.rotation;

			// we keep the health if needed
			if (CommonHealth)
			{
				_instantiatedCharacters[CurrentIndex].gameObject.MMGetComponentNoAlloc<Health>().SetHealth(LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth);
			}

			// we put it in the same state the old one was in
			_instantiatedCharacters[CurrentIndex].MovementState.ChangeState(LevelManager.Instance.Players[0].MovementState.CurrentState);
			_instantiatedCharacters[CurrentIndex].ConditionState.ChangeState(LevelManager.Instance.Players[0].ConditionState.CurrentState);

			// we make it the current character
			LevelManager.Instance.Players[0] = _instantiatedCharacters[CurrentIndex];

			// we play our vfx
			if (_instantiatedVFX != null)
			{
				_instantiatedVFX.gameObject.SetActive(true);
				_instantiatedVFX.transform.position = _instantiatedCharacters[CurrentIndex].transform.position;
				_instantiatedVFX.Play();
			}

			// we trigger a switch event (for the camera to know, mostly)
			MMEventManager.TriggerEvent(_switchEvent);
			MMCameraEvent.Trigger(MMCameraEventTypes.RefreshAutoFocus, LevelManager.Instance.Players[0], null);
		}
	}
}