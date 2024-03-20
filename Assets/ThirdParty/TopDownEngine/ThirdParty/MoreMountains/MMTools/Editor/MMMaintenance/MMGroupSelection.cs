using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 메뉴 항목을 추가하는 데 사용되는 클래스와 상위 게임 개체 아래에 개체를 그룹화하는 바로가기
    /// </summary>
    public class MMGroupSelection 
	{
		/// <summary>
		/// Creates a parent object and puts all selected transforms under it
		/// </summary>
		[MenuItem("Tools/More Mountains/Group Selection %g")]
		public static void GroupSelection()
		{
			if (!Selection.activeTransform)
			{
				return;
			}

			GameObject groupObject = new GameObject();
			groupObject.name = "Group";

			Undo.RegisterCreatedObjectUndo(groupObject, "Group Selection");

			groupObject.transform.SetParent(Selection.activeTransform.parent, false);

			foreach (Transform selectedTransform in Selection.transforms)
			{
				Undo.SetTransformParent(selectedTransform, groupObject.transform, "Group Selection");
			}
			Selection.activeGameObject = groupObject;
		}
	}
}