using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMSceneLoading 화면 내에서 텍스트를 업데이트하는 데 사용되는 매우 간단한 클래스입니다.
    /// 로딩 진행 상황에 따라
    /// </summary>
    public class MMSceneLoadingTextProgress : MonoBehaviour
	{
		/// the value to which the progress' zero value should be remapped to
		[Tooltip("진행률의 0 값을 다시 매핑해야 하는 값")]
		public float RemapMin = 0f;
		/// the value to which the progress' one value should be remapped to
		[Tooltip("진행률의 한 값을 다시 매핑해야 하는 값")]
		public float RemapMax = 100f;
		/// the amount of decimals to display
		[Tooltip("표시할 소수점 이하 자릿수")]
		public int NumberOfDecimals = 0;

		protected Text _text;

		/// <summary>
		/// On Awake we grab our Text and store it
		/// </summary>
		protected virtual void Awake()
		{
			_text = this.gameObject.GetComponent<Text>();
		}
        
		/// <summary>
		/// Updates the Text with the progress value
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetProgress(float newValue)
		{
			float remappedValue = MMMaths.Remap(newValue, 0f, 1f, RemapMin, RemapMax);
			float displayValue = MMMaths.RoundToDecimal(remappedValue, NumberOfDecimals);
			_text.text = displayValue.ToString(CultureInfo.InvariantCulture);
		}
	}
}