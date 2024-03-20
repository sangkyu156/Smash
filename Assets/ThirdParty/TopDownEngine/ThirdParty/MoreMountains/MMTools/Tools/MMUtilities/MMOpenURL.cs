using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 검사기에 지정된 URL을 여는 데 사용되는 클래스
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Utilities/MMOpenURL")]
	public class MMOpenURL : MonoBehaviour 
	{
		/// the URL to open when calling OpenURL()
		public string DestinationURL;

		/// <summary>
		/// Opens the URL specified in the DestinationURL field
		/// </summary>
		public virtual void OpenURL()
		{
			Application.OpenURL(DestinationURL);
		}		
	}
}