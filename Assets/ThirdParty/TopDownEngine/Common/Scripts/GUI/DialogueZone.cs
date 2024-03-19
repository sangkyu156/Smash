using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 대사 정보를 저장하는 데 사용되는 클래스
    /// </summary>
    [Serializable]
	public class DialogueElement
	{
		[Multiline]
		public string DialogueLine;
	}

    /// <summary>
    /// 이 클래스를 빈 구성 요소에 추가합니다. "is Trigger"로 설정된 Collider 또는 Collider2D가 필요합니다.
    /// 그런 다음 검사기를 통해 대화 영역을 사용자 정의할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Dialogue Zone")]
	public class DialogueZone : ButtonActivated
	{
		[Header("Dialogue Look")]

		/// the prefab to use to display the dialogue
		[Tooltip("대화를 표시하는 데 사용할 프리팹")]
		public DialogueBox DialogueBoxPrefab;
		/// the color of the text background.
		[Tooltip("텍스트 배경의 색상입니다.")]
		public Color TextBackgroundColor = Color.black;
		/// the color of the text
		[Tooltip("텍스트의 색상")]
		public Color TextColor = Color.white;
		/// the font that should be used to display the text
		[Tooltip("텍스트를 표시하는 데 사용해야 하는 글꼴")]
		public Font TextFont;
		/// the size of the font
		[Tooltip("글꼴의 크기")]
		public int TextSize = 40;
		/// the text alignment in the box used to display the text
		[Tooltip("텍스트를 표시하는 데 사용되는 상자의 텍스트 정렬")]
		public TextAnchor Alignment = TextAnchor.MiddleCenter;
        
		[Header("Dialogue Speed (in seconds)")]

        /// 인 및 아웃 페이드의 지속 시간
        [Tooltip("인 및 아웃 페이드의 지속 시간")]
		public float FadeDuration = 0.2f;
        /// 두 대화 사이의 시간
        [Tooltip("두 대화 사이의 시간")]
		public float TransitionTime = 0.2f;

		[Header("Dialogue Position")]

        /// 대화 상자가 나타나야 하는 상자 충돌체 상단으로부터의 거리
        [Tooltip("대화 상자가 나타나야 하는 상자 충돌체 상단으로부터의 거리")]
		public Vector3 Offset = Vector3.zero;
        /// 이것이 사실이라면 대화 상자는 영역의 위치를 ​​따릅니다.
        [Tooltip("이것이 사실이라면 대화 상자는 영역의 위치를 ​​따릅니다.")]
		public bool BoxesFollowZone = false;

		[Header("Player Movement")]

        /// true로 설정하면 대화가 진행되는 동안 캐릭터가 움직일 수 있습니다.
        [Tooltip("true로 설정하면 대화가 진행되는 동안 캐릭터가 움직일 수 있습니다.")]
		public bool CanMoveWhileTalking = true;

		[Header("Press button to go from one message to the next ?")]

        /// 이 영역이 버튼으로 처리되는지 여부
        [Tooltip("이 영역이 버튼으로 처리되는지 여부")]
		public bool ButtonHandled = true;
        /// 메시지의 지속 시간. 상자가 버튼으로 처리되지 않은 경우에만 고려됩니다.
        [Header("Only if the dialogue is not button handled :")]
		[Range(1, 100)]
		[Tooltip("메시지가 표시되어야 하는 기간(초)입니다. 상자가 버튼으로 처리되지 않은 경우에만 고려됩니다.")]
		public float MessageDuration = 3f;
        
		[Header("Activations")]
        /// 두 번 이상 활성화할 수 있으면 true
        [Tooltip("두 번 이상 활성화할 수 있으면 true")]
		public bool ActivableMoreThanOnce = true;
        /// 영역이 두 번 이상 활성화 가능한 경우 가동 시간 사이에 얼마나 오랫동안 비활성 상태로 유지되어야 합니까?
        [Range(1, 100)]
		[Tooltip("영역이 두 번 이상 활성화 가능한 경우 가동 시간 사이에 얼마나 오랫동안 비활성 상태로 유지되어야 합니까?")]
		public float InactiveTime = 2f;

        /// 대사
        [Tooltip("대사")]
		public DialogueElement[] Dialogue;

		/// private variables
		protected DialogueBox _dialogueBox;
		protected bool _activated = false;
		protected bool _playing = false;
		protected int _currentIndex;
		protected bool _activable = true;
		protected WaitForSeconds _transitionTimeWFS;
		protected WaitForSeconds _messageDurationWFS;
		protected WaitForSeconds _inactiveTimeWFS;

		/// <summary>
		/// Initializes the dialogue zone
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			_currentIndex = 0;
			_transitionTimeWFS = new WaitForSeconds(TransitionTime);
			_messageDurationWFS = new WaitForSeconds(MessageDuration);
			_inactiveTimeWFS = new WaitForSeconds(InactiveTime);
		}

		/// <summary>
		/// When the button is pressed we start the dialogue
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			if (_playing && !ButtonHandled)
			{
				return;
			}
			base.TriggerButtonAction();
			StartDialogue();
		}

		/// <summary>
		/// When triggered, either by button press or simply entering the zone, starts the dialogue
		/// </summary>
		public virtual void StartDialogue()
		{
			// if the dialogue zone has no box collider, we do nothing and exit
			if ((_collider == null) && (_collider2D == null))
			{
				return;
			}

			// if the zone has already been activated and can't be activated more than once.
			if (_activated && !ActivableMoreThanOnce)
			{
				return;
			}

			// if the zone is not activable, we do nothing and exit
			if (!_activable)
			{
				return;
			}

			// if the player can't move while talking, we notify the game manager
			if (!CanMoveWhileTalking)
			{
				LevelManager.Instance.FreezeCharacters();
				if (ShouldUpdateState && (_characterButtonActivation != null))
				{
					_characterButtonActivation.GetComponentInParent<Character>().MovementState.ChangeState(CharacterStates.MovementStates.Idle);
				}
			}

			// if it's not already playing, we'll initialize the dialogue box
			if (!_playing)
			{
				// we instantiate the dialogue box
				_dialogueBox = Instantiate(DialogueBoxPrefab);
				// we set its position
				if (_collider2D != null)
				{
					_dialogueBox.transform.position = _collider2D.bounds.center + Offset;
				}
				if (_collider != null)
				{
					_dialogueBox.transform.position = _collider.bounds.center + Offset;
				}
				// we set the color's and background's colors
				_dialogueBox.ChangeColor(TextBackgroundColor, TextColor);
				// if it's a button handled dialogue, we turn the A prompt on
				_dialogueBox.ButtonActive(ButtonHandled);

				// if font settings have been specified, we set them
				if (BoxesFollowZone)
				{
					_dialogueBox.transform.SetParent(this.gameObject.transform);
				}
				if (TextFont != null)
				{
					_dialogueBox.DialogueText.font = TextFont;
				}
				if (TextSize != 0)
				{
					_dialogueBox.DialogueText.fontSize = TextSize;
				}
				_dialogueBox.DialogueText.alignment = Alignment;

				// the dialogue is now playing
				_playing = true;
			}
			// we start the next dialogue
			StartCoroutine(PlayNextDialogue());
		}

		/// <summary>
		/// Turns collider on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void EnableCollider(bool status)
		{
			if (_collider2D != null)
			{
				_collider2D.enabled = status;
			}
			if (_collider != null)
			{
				_collider.enabled = status;
			}
		}

		/// <summary>
		/// Plays the next dialogue in the queue
		/// </summary>
		protected virtual IEnumerator PlayNextDialogue()
		{
			// we check that the dialogue box still exists
			if (_dialogueBox == null)
			{
				yield break;
			}
			// if this is not the first message
			if (_currentIndex != 0)
			{
				// we turn the message off
				_dialogueBox.FadeOut(FadeDuration);
				// we wait for the specified transition time before playing the next dialogue
				yield return _transitionTimeWFS;
			}
			// if we've reached the last dialogue line, we exit
			if (_currentIndex >= Dialogue.Length)
			{
				_currentIndex = 0;
				Destroy(_dialogueBox.gameObject);
				EnableCollider(false);
				// we set activated to true as the dialogue zone has now been turned on		
				_activated = true;
				// we let the player move again
				if (!CanMoveWhileTalking)
				{
					LevelManager.Instance.UnFreezeCharacters();
				}
				if ((_characterButtonActivation != null))
				{
					_characterButtonActivation.InButtonActivatedZone = false;
					_characterButtonActivation.ButtonActivatedZone = null;
				}
				// we turn the zone inactive for a while
				if (ActivableMoreThanOnce)
				{
					_activable = false;
					_playing = false;
					StartCoroutine(Reactivate());
				}
				else
				{
					gameObject.SetActive(false);
				}
				yield break;
			}

			// we check that the dialogue box still exists
			if (_dialogueBox.DialogueText != null)
			{
				// every dialogue box starts with it fading in
				_dialogueBox.FadeIn(FadeDuration);
				// then we set the box's text with the current dialogue
				_dialogueBox.DialogueText.text = Dialogue[_currentIndex].DialogueLine;
			}

			_currentIndex++;

			// if the zone is not button handled, we start a coroutine to autoplay the next dialogue
			if (!ButtonHandled)
			{
				StartCoroutine(AutoNextDialogue());
			}
		}

		/// <summary>
		/// Automatically goes to the next dialogue line
		/// </summary>
		/// <returns>The next dialogue.</returns>
		protected virtual IEnumerator AutoNextDialogue()
		{
			// we wait for the duration of the message
			yield return _messageDurationWFS;
			StartCoroutine(PlayNextDialogue());
		}

		/// <summary>
		/// Reactivate the dialogue zone
		/// </summary>
		protected virtual IEnumerator Reactivate()
		{
			yield return _inactiveTimeWFS;
			EnableCollider(true);
			_activable = true;
			_playing = false;
			_currentIndex = 0;
			_promptHiddenForever = false;

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
		}
	}
}