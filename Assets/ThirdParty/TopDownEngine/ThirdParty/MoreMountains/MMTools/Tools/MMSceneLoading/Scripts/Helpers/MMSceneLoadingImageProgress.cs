using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이미지의 채우기 양을 업데이트하기 위해 MMSceneLoading 화면 내에서 사용되는 매우 간단한 클래스입니다.
    /// 로딩 진행 상황에 따라
    /// </summary>
    public class MMSceneLoadingImageProgress : MonoBehaviour
	{
		protected Image _image;

		/// <summary>
		/// On Awake we store our Image
		/// </summary>
		protected virtual void Awake()
		{
			_image = this.gameObject.GetComponent<Image>();
		}
        
		/// <summary>
		/// Meant to be called by the MMSceneLoadingManager, turns the progress of a load into fill amount
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetProgress(float newValue)
		{
			_image.fillAmount = newValue;
		}
	}
}