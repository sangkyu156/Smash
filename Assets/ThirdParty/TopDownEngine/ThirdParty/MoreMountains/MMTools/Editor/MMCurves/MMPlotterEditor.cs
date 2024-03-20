using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 폴더형 MMFeedback 목록, 더 추가할 수 있는 드롭다운, 런타임 시 피드백을 테스트할 수 있는 테스트 버튼을 표시하는 사용자 정의 편집기
    /// </summary>
    [CanEditMultipleObjects]
	[CustomEditor(typeof(MMPlotter), true)]
	public class MMPlotterEditor : Editor
	{
		protected string[] _typeDisplays;
		protected string[] _excludedProperties = new string[] { "TweenMethod", "m_Script" }; 

		protected MMPlotter _mmPlotter;

		protected virtual void OnEnable()
		{
			_mmPlotter = target as MMPlotter;
			_typeDisplays = _mmPlotter.GetMethodsList();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Modified Plotter");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Tween Method", EditorStyles.boldLabel);

			_mmPlotter.TweenMethodIndex = EditorGUILayout.Popup("Tween Method", _mmPlotter.TweenMethodIndex, _typeDisplays, EditorStyles.popup);

			//int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;
			//DrawDefaultInspector();
			DrawPropertiesExcluding(serializedObject, _excludedProperties);

			if (GUILayout.Button("Draw Graph"))
			{
				_mmPlotter.DrawGraph();
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}