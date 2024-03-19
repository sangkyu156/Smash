using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스는 업적을 표시하는 데 사용됩니다. 아래 나열된 모든 필수 요소가 포함된 프리팹에 추가하세요.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Achievements/MMAchievementDisplayItem")]
	public class MMAchievementDisplayItem : MonoBehaviour 
	{		
		public Image BackgroundLocked;
		public Image BackgroundUnlocked;
		public Image Icon;
		public Text Title;
		public Text Description;
		public MMProgressBar ProgressBarDisplay;	
	}
}