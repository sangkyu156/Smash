using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 제목 항목을 MMDebugMenu에 바인딩하는 데 사용되는 클래스
    /// </summary>
    public class MMDebugMenuItemTitle : MonoBehaviour
	{
		[Header("Bindings")]
		/// the text comp used to display the title
		public Text TitleText;
		/// a line below the title
		public Image TitleLine;
	}
}