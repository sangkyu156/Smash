using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// PixelRogue 데모에서 인벤토리 및 지속성 데이터를 재설정하는 데 사용되는 매우 작은 클래스
    /// </summary>
    public class PixelRogueDemoResetAll : MonoBehaviour
	{
		const string _inventorySaveFolderName = "InventoryEngine"; 
		
		public virtual void ResetAll()
		{
			// we delete the save folder for inventories
			MMSaveLoadManager.DeleteSaveFolder (_inventorySaveFolderName);
			// we delete our persistence data
			MMPersistenceManager.Instance.ResetPersistence();
			// we reload the scene
			SceneManager.LoadScene("PixelRogueRoom1");
		}
	}	
}

