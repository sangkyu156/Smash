using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEditor;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 상단 표시줄 추가 산 항목에 전용 InventoryEngine 메뉴를 추가합니다.
    /// </summary>
    public static class InventoryEngineMenu 
	{
		const string _saveFolderName = "InventoryEngine"; 

		[MenuItem("Tools/More Mountains/Reset all saved inventories",false,31)]
        /// <summary>
        /// Unity에서 직접 저장된 모든 인벤토리를 재설정할 수 있는 메뉴 항목을 추가합니다.
        /// 이렇게 하면 전체 MMData/InventoryEngine 폴더가 제거되므로 주의해서 사용하세요.
        /// </summary>
        private static void ResetAllSavedInventories()
		{
			MMSaveLoadManager.DeleteSaveFolder (_saveFolderName);
			Debug.LogFormat ("Inventories Save Files Reset");
		}
	}
}