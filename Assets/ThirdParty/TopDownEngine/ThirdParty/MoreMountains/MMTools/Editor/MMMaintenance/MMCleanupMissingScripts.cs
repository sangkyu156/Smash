using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 사용하면 선택한 게임 개체에서 누락된 모든 스크립트를 정리할 수 있습니다.
    /// </summary>
    public class MMCleanupMissingScripts : MonoBehaviour
	{
        /// <summary>
        /// 누락된 모든 스크립트에 대해 게임 개체 정리를 처리합니다.
        /// </summary>
        [MenuItem("Tools/More Mountains/Cleanup missing scripts on selected GameObjects", false, 504)]
		protected static void CleanupMissingScripts()
		{
			Object[] collectedDeepHierarchy = EditorUtility.CollectDeepHierarchy(Selection.gameObjects);
			int removedComponentsCounter = 0;
			int gameobjectsAffectedCounter = 0;
			foreach (Object targetObject in collectedDeepHierarchy)
			{
				if (targetObject is GameObject gameObject)
				{
					int amountOfMissingScripts = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
					if (amountOfMissingScripts > 0)
					{
						Undo.RegisterCompleteObjectUndo(gameObject, "Removing missing scripts");
						GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
						removedComponentsCounter += amountOfMissingScripts;
						gameobjectsAffectedCounter++;
					}
				}
			}
			Debug.Log("[MMCleanupMissingScripts] Removed " + removedComponentsCounter + " missing scripts from " + gameobjectsAffectedCounter + " GameObjects");
		}
	}
}