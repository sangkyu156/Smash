using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 장면 보기에 이름을 그리는 Room용 사용자 정의 편집기
    /// </summary>
    [CanEditMultipleObjects]
	[CustomEditor(typeof(Room), true)]
	[InitializeOnLoad]
	public class RoomEditor : Editor
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawHandles(Room room, GizmoType gizmoType)
		{
			Room t = (room as Room);

			GUIStyle style = new GUIStyle();

			// draws the path item number
			style.normal.textColor = MMColors.Pink;
			Handles.Label(t.transform.position + (Vector3.up * 2f) + (Vector3.right * 2f), t.name, style);
		}
	}
}