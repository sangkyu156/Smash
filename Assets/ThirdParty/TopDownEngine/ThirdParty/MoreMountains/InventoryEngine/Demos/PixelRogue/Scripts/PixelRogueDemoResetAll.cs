using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// PixelRogue ���𿡼� �κ��丮 �� ���Ӽ� �����͸� �缳���ϴ� �� ���Ǵ� �ſ� ���� Ŭ����
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

