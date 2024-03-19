using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;
using UnityEngine.UI;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/GUI/ButtonPrompt")]
	public class ButtonPrompt : TopDownMonoBehaviour
	{
		[Header("Bindings")]
        /// 프롬프트의 테두리로 사용할 이미지
        [Tooltip("프롬프트의 테두리로 사용할 이미지")]
		public Image Border;
        /// 배경으로 사용할 이미지
        [Tooltip("배경으로 사용할 이미지")]
		public Image Background;
        /// 프롬프트 컨테이너의 캔버스 그룹
        [Tooltip("프롬프트 컨테이너의 캔버스 그룹")]
		public CanvasGroup ContainerCanvasGroup;
        /// 프롬프트의 텍스트 구성 요소
        [Tooltip("프롬프트의 텍스트 구성 요소")]
		public Text PromptText;

		[Header("Durations")]
        /// 페이드 인 지속 시간(초)
        [Tooltip("페이드 인 지속 시간(초)")]
		public float FadeInDuration = 0.2f;
        /// 페이드 아웃 기간(초)
        [Tooltip("페이드 아웃 기간(초)")]
		public float FadeOutDuration = 0.2f;
        
		protected Color _alphaZero = new Color(1f, 1f, 1f, 0f);
		protected Color _alphaOne = new Color(1f, 1f, 1f, 1f);
		protected Coroutine _hideCoroutine;

		protected Color _tempColor;

        //내가만든 변수
        protected GameObject _NPCInventory;

        public virtual void Initialization()
		{
			ContainerCanvasGroup.alpha = 0f;
		}

		public virtual void SetText(string newText)
		{
			PromptText.text = newText;
		}

		public virtual void SetBackgroundColor(Color newColor)
		{
			Background.color = newColor;
		}

		public virtual void SetTextColor(Color newColor)
		{
			PromptText.color = newColor;
		}

		public virtual void Show()
		{
			this.gameObject.SetActive(true);
			if (_hideCoroutine != null)
			{
				StopCoroutine(_hideCoroutine);
			}
			ContainerCanvasGroup.alpha = 0f;
			StartCoroutine(MMFade.FadeCanvasGroup(ContainerCanvasGroup, FadeInDuration, 1f, true));
            if(_NPCInventory == null)
			{
				_NPCInventory = GameObject.FindWithTag("InventoryCanvas");
            }
            _NPCInventory.GetComponent<InventoryInputManager>().ButtonPromptIsOpen = true;
		}

		public virtual void Hide()
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}
			_hideCoroutine = StartCoroutine(HideCo());
            _NPCInventory.GetComponent<InventoryInputManager>().ButtonPromptIsOpen = false;
        }

		protected virtual IEnumerator HideCo()
		{
			ContainerCanvasGroup.alpha = 1f;
			StartCoroutine(MMFade.FadeCanvasGroup(ContainerCanvasGroup, FadeOutDuration, 0f, true));
			yield return new WaitForSeconds(FadeOutDuration);
			this.gameObject.SetActive(false);
		}
	}
}