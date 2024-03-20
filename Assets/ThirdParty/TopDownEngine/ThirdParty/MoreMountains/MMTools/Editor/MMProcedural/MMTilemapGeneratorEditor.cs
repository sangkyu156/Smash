using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMTilemapGenerator용 사용자 정의 편집기는 생성 버튼 및 재정렬 가능한 레이어를 처리합니다.
    /// </summary>
    [CustomEditor(typeof(MMTilemapGenerator), true)]
	[CanEditMultipleObjects]
	public class MMTilemapGeneratorEditor : Editor
	{
    
		protected MMReorderableList _list;

		protected virtual void OnEnable()
		{
			_list = new MMReorderableList(serializedObject.FindProperty("Layers"));
			_list.elementNameProperty = "Layer";
			_list.elementDisplayType = MMReorderableList.ElementDisplayType.Expandable;
		}
        
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
            
			DrawPropertiesExcluding(serializedObject,  "Layers");
			EditorGUILayout.Space(10);
			_list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
            
			if (GUILayout.Button("Generate"))
			{
				(target as MMTilemapGenerator).Generate();
			}
		}
	}
}