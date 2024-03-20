using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMTransformRandomizer 클래스용 사용자 정의 편집기
    /// </summary>
    [CustomEditor(typeof(MMTransformRandomizer), true)]
	[CanEditMultipleObjects]
	public class MMTransformRandomizerEditor : Editor
	{
		/// <summary>
		/// On inspector we handle undo and display a test button
		/// </summary>
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Modified MMTransformRandomizer");
			DrawDefaultInspector();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);

			if (GUILayout.Button("Randomize"))
			{
				foreach (MMTransformRandomizer randomizer in targets)
				{
					randomizer.Randomize();
				}
			}
		}
	}
}