using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 방사형 이미지에 추가하면 채우기 양을 제어할 수 있습니다. 
	/// 이것은 레거시 클래스이며 대신 MMProgressBar를 사용하는 것이 좋습니다. 
	/// 동일한 기능을 제공합니다(FillMode로 FillAmount를 선택해야 합니다). 
	/// 지연된 막대, 이벤트, 범프 등과 같은 훨씬 더 많은 옵션!
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/GUI/MMRadialProgressBar")]
	public class MMRadialProgressBar : MonoBehaviour 
	{
		/// the start fill amount value 
		public float StartValue = 1f;
		/// the end goad fill amount value
		public float EndValue = 0f;
		/// the distance to the start or end value at which the class should start lerping
		public float Tolerance = 0.01f;
		/// optional - the ID of the player associated to this bar
		public string PlayerID;

		protected Image _radialImage;
		protected float _newPercent;

		/// <summary>
		/// On awake we grab our Image component
		/// </summary>
		protected virtual void Awake()
		{
			_radialImage = GetComponent<Image>();
		}

		/// <summary>
		/// Call this method to update the fill amount based on a currentValue between minValue and maxValue
		/// </summary>
		/// <param name="currentValue">Current value.</param>
		/// <param name="minValue">Minimum value.</param>
		/// <param name="maxValue">Max value.</param>
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue)
		{
			_newPercent = MMMaths.Remap(currentValue,minValue,maxValue,StartValue,EndValue);
			if (_radialImage == null) { return; }
			_radialImage.fillAmount = _newPercent;
			if (_radialImage.fillAmount > 1 - Tolerance)
			{
				_radialImage.fillAmount = 1;
			}
			if (_radialImage.fillAmount < Tolerance)
			{
				_radialImage.fillAmount = 0;
			}

		}
	}
}