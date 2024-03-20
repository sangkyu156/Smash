using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소는 텍스트 구성 요소에 추가되면 레벨 이름을 표시합니다.
    /// </summary>
    public class MMSceneName : MonoBehaviour
	{
		protected Text _text;

		/// <summary>
		/// On Awake, stores the Text component
		/// </summary>
		protected virtual void Awake()
		{
			_text = this.gameObject.GetComponent<Text>();
		}

		/// <summary>
		/// On Start, sets the level name
		/// </summary>
		protected virtual void Start()
		{
			SetLevelNameText();
		}

		/// <summary>
		/// Assigns the level name to the Text
		/// </summary>
		public virtual void SetLevelNameText()
		{
			if (_text != null)
			{
				_text.text = SceneManager.GetActiveScene().name;
			}
		}
	}
}