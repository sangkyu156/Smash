using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{	
	public static class MMAchievementMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all achievements", false,21)]
        /// <summary>
        /// 도움말을 활성화하는 메뉴 항목을 추가합니다.
        /// </summary>
        private static void EnableHelpInInspectors()
		{
			MMAchievementManager.ResetAllAchievements ();
		}
	}
}