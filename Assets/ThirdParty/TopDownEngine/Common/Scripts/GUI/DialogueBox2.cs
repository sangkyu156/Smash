using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using TMPro;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 대화 상자 클래스. 이것을 게임에 직접 추가하지 말고 대신 DialogueZone을 살펴보세요.
    /// </summary>
    public class DialogueBox2 : TopDownMonoBehaviour
    {
        [Header("Dialogue Box")]
        /// the text panel background
        [Tooltip("텍스트 패널 배경")]
        public CanvasGroup TextPanelCanvasGroup;
        /// the text to display
        [Tooltip("표시할 텍스트")]
        public TextMeshProUGUI DialogueText;
        /// the Button A prompt
        [Tooltip("버튼 A 프롬프트")]
        public CanvasGroup Prompt;
        /// the list of images to colorize
        [Tooltip("색상화할 이미지 목록")]
        public List<Image> ColorImages;

        protected Color _backgroundColor;
        protected Color _textColor;

        public int textIndext = 0;
        public string[] DialogueTexts = new string[11];
        public float typingSpeed = 0.1f;

        private void Start()
        {
            if (TextUtil.languageNumber == 2)//영어라면
                typingSpeed = 0.02f;
            else
                typingSpeed = 0.08f;

            DialogueTexts[0] = TextUtil.GetText("game:chief:dialogue1");
            DialogueTexts[1] = TextUtil.GetText("game:chief:dialogue2");
            DialogueTexts[2] = TextUtil.GetText("game:chief:dialogue3");
            DialogueTexts[3] = TextUtil.GetText("game:chief:dialogue4");
            DialogueTexts[4] = TextUtil.GetText("game:chief:dialogue5");
            DialogueTexts[5] = TextUtil.GetText("game:chief:dialogue6");
            DialogueTexts[6] = TextUtil.GetText("game:chief:dialogue7");
            DialogueTexts[7] = TextUtil.GetText("game:chief:dialogue8");
            DialogueTexts[8] = TextUtil.GetText("game:player:dialogue1");
            DialogueTexts[9] = TextUtil.GetText("game:player:dialogue2");
            DialogueTexts[10] = TextUtil.GetText("game:player:dialogue3");

            StartCoroutine(TypewriterEffect());
        }


        //타이핑 함수
        public IEnumerator TypewriterEffect()
        {
            for (int i = 0; i <= DialogueTexts[textIndext].Length; i++)
            {
                string currentText = DialogueTexts[textIndext].Substring(0, i);
                DialogueText.text = currentText;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        /// <summary>
        /// Changes the text.
        /// </summary>
        /// <param name="newText">New text.</param>
        public virtual void ChangeText(string newText)
        {
            DialogueText.text = newText;
        }

        /// <summary>
        /// Activates the ButtonA prompt
        /// </summary>
        /// <param name="state">If set to <c>true</c> state.</param>
        public virtual void ButtonActive(bool state)
        {
            Prompt.gameObject.SetActive(state);
        }

        /// <summary>
        /// Changes the color of the dialogue box to the ones in parameters
        /// </summary>
        /// <param name="backgroundColor">Background color.</param>
        /// <param name="textColor">Text color.</param>
        public virtual void ChangeColor(Color backgroundColor, Color textColor)
        {
            _backgroundColor = backgroundColor;
            _textColor = textColor;

            foreach (Image image in ColorImages)
            {
                image.color = _backgroundColor;
            }
            DialogueText.color = _textColor;
        }

        /// <summary>
        /// Fades the dialogue box in.
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void FadeIn(float duration)
        {
            if (TextPanelCanvasGroup != null)
            {
                StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 1f));
            }
            if (DialogueText != null)
            {
                StartCoroutine(MMFade.FadeText(DialogueText, duration, _textColor));
            }
            if (Prompt != null)
            {
                StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 1f));
            }
        }

        /// <summary>
        /// Fades the dialogue box out.
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void FadeOut(float duration)
        {
            Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
            Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

            StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 0f));
            StartCoroutine(MMFade.FadeText(DialogueText, duration, newTextColor));
            StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 0f));
        }
    }
}