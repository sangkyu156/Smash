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
    public class DialogueElement2
    {
        [Multiline]
        public string DialogueLine;
    }

    /// <summary>
    /// 이 클래스를 빈 구성 요소에 추가합니다. "is Trigger"로 설정된 Collider 또는 Collider2D가 필요합니다.
    /// 그런 다음 검사기를 통해 대화 영역을 사용자 정의할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Dialogue Zone")]
    public class DialogueZone2 : ButtonActivated
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

        /// 개인 변수
        protected DialogueBox _dialogueBox;
        protected bool _activated = false;
        protected bool _playing = false;
        protected int _currentIndex;
        protected bool _activable = true;
        protected WaitForSeconds _transitionTimeWFS;
        protected WaitForSeconds _messageDurationWFS;
        protected WaitForSeconds _inactiveTimeWFS;

        private void Start()
        {
            Invoke("StartDialogue", 10f);
        }

        /// <summary>
        /// 대화 영역을 초기화합니다.
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
        /// 버튼을 누르면 대화가 시작됩니다
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
        /// 버튼을 누르거나 단순히 영역에 들어가면 트리거되면 대화가 시작됩니다.
        /// </summary>
        public virtual void StartDialogue()
        {
            // 대화 영역에 상자 충돌체가 없으면 아무것도 하지 않고 종료합니다.
            //if ((_collider == null) && (_collider2D == null))
            //{
            //    return;
            //}

            // 영역이 이미 활성화되어 두 번 이상 활성화할 수 없는 경우.
            //if (_activated && !ActivableMoreThanOnce)
            //{
            //    return;
            //}

            // 영역이 활성화되지 않으면 아무것도 하지 않고 종료됩니다.
            //if (!_activable)
            //{
            //    return;
            //}

            // 플레이어가 대화하는 동안 움직일 수 없으면 게임 관리자에게 알립니다.
            //if (!CanMoveWhileTalking)
            //{
            //    LevelManager.Instance.FreezeCharacters();
            //    if (ShouldUpdateState && (_characterButtonActivation != null))
            //    {
            //        _characterButtonActivation.GetComponentInParent<Character>().MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            //    }
            //}

            // 아직 재생 중이 아닌 경우 대화 상자를 초기화합니다.
            if (!_playing)
            {
                // 대화 상자를 인스턴스화합니다.
                _dialogueBox = Instantiate(DialogueBoxPrefab);
                // 우리는 그 위치를 설정
                if (_collider2D != null)
                {
                    _dialogueBox.transform.position = _collider2D.bounds.center + Offset;
                }
                if (_collider != null)
                {
                    _dialogueBox.transform.position = _collider.bounds.center + Offset;
                }
                // 색상과 배경색을 설정합니다.
                _dialogueBox.ChangeColor(TextBackgroundColor, TextColor);
                // 버튼 처리 대화상자인 경우 A 프롬프트를 켭니다.
                _dialogueBox.ButtonActive(ButtonHandled);

                // 글꼴 설정이 지정된 경우 이를 설정합니다.
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

                // 지금 대화가 재생되고 있어요
                _playing = true;
            }
            // 다음 대화를 시작하자
            StartCoroutine(PlayNextDialogue());
        }

        /// <summary>
        /// 충돌체를 켜거나 끕니다.
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
        /// 대기열의 다음 대화를 재생합니다.
        /// </summary>
        protected virtual IEnumerator PlayNextDialogue()
        {
            // 대화 상자가 여전히 존재하는지 확인합니다.
            if (_dialogueBox == null)
            {
                yield break;
            }
            // 이것이 첫 번째 메시지가 아닌 경우
            if (_currentIndex != 0)
            {
                // 우리는 메시지를 꺼
                _dialogueBox.FadeOut(FadeDuration);
                // 다음 대화를 재생하기 전에 지정된 전환 시간을 기다립니다.
                yield return _transitionTimeWFS;
            }
            // 마지막 대화 줄에 도달했다면 종료합니다.
            if (_currentIndex >= Dialogue.Length)
            {
                _currentIndex = 0;
                Destroy(_dialogueBox.gameObject);
                EnableCollider(false);
                // 이제 대화 영역이 켜져 있으므로 활성화를 true로 설정했습니다.
                _activated = true;
                // 플레이어를 다시 움직이게 합니다
                if (!CanMoveWhileTalking)
                {
                    LevelManager.Instance.UnFreezeCharacters();
                }
                if ((_characterButtonActivation != null))
                {
                    _characterButtonActivation.InButtonActivatedZone = false;
                    _characterButtonActivation.ButtonActivatedZone = null;
                }
                // 잠시 동안 영역을 비활성화합니다.
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

            // 대화 상자가 여전히 존재하는지 확인합니다.
            if (_dialogueBox.DialogueText != null)
            {
                // 모든 대화 상자는 페이드 인으로 시작됩니다.
                _dialogueBox.FadeIn(FadeDuration);
                // 그런 다음 현재 대화 상자의 텍스트를 설정합니다.
                _dialogueBox.DialogueText.text = Dialogue[_currentIndex].DialogueLine;
            }

            _currentIndex++;

            // 영역이 버튼으로 처리되지 않으면 코루틴을 시작하여 다음 대화를 자동 재생합니다.
            if (!ButtonHandled)
            {
                StartCoroutine(AutoNextDialogue());
            }
        }

        /// <summary>
        /// 자동으로 다음 대화 줄로 이동합니다.
        /// </summary>
        /// <returns>다음 대화.</returns>
        protected virtual IEnumerator AutoNextDialogue()
        {
            // 우리는 메시지가 지속되는 동안 기다립니다
            yield return _messageDurationWFS;
            StartCoroutine(PlayNextDialogue());
        }

        /// <summary>
        /// 대화 영역을 다시 활성화하세요
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