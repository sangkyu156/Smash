using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 저장된 모든 데이터를 삭제하기 위해 상단 표시줄 추가 산 항목에 전용 도구 메뉴를 추가합니다.
    /// </summary>
    public static class MMSaveLoadMenu 
	{
		[MenuItem("Tools/More Mountains/Delete all saved data",false,31)]
		/// <summary>
		/// Adds a menu item to reset all data saved by the MMSaveLoadManager. No turning back.
		/// </summary>
		private static void ResetAllSavedInventories()
		{
			MMSaveLoadManager.DeleteAllSaveFiles();
			Debug.LogFormat ("All Save Files Deleted");
		}
	}
}