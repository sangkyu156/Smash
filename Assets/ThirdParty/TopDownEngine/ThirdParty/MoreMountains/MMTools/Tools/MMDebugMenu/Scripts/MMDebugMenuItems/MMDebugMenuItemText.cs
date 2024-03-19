using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 텍스트 항목을 MMDebugMenu에 바인딩하는 데 사용되는 클래스
    /// </summary>
    public class MMDebugMenuItemText : MonoBehaviour
	{
		[Header("Bindings")]
		/// a text comp used to display the text
		[TextArea]
		public Text ContentText;
	}
}