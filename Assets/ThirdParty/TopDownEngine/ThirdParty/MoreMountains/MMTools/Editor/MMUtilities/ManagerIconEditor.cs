using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스는 더 쉬운 설정을 위해 장면 뷰에서 옆에 있는 각 LevelMapPathElement의 이름을 추가합니다.
    /// </summary>
    [CustomEditor(typeof(MMSceneViewIcon))]
	[InitializeOnLoad]
	public class SceneViewIconEditor : Editor 
	{		
		//protected SceneViewIcon _sceneViewIcon;

		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawGameObjectName(MMSceneViewIcon sceneViewIcon, GizmoType gizmoType)
		{   
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.blue;	 
			Handles.Label(sceneViewIcon.transform.position, sceneViewIcon.gameObject.name,style);
		}


	}
}