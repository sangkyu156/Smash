using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
	[CustomEditor(typeof(MMAchievementList),true)]
    /// <summary>
    /// MMAchievementList 스크립트 가능 개체에 대한 사용자 정의 검사기입니다.
    /// </summary>
    public class MMAchievementListInspector : Editor 
	{
		/// <summary>
		/// When drawing the GUI, adds a "Reset Achievements" button, that does exactly what you think it does.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			MMAchievementList achievementList = (MMAchievementList)target;
			if(GUILayout.Button("Reset Achievements"))
			{
				achievementList.ResetAchievements();
			}	
			EditorUtility.SetDirty (achievementList);
		}
	}
}