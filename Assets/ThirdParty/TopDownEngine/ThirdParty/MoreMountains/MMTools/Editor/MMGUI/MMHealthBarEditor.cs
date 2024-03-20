using System;
using UnityEngine;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine.UI;

namespace MoreMountains.Tools
{	
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MMHealthBar),true)]
    /// <summary>
    /// 체력 막대용 사용자 정의 편집기(주로 조립식 기반/그려진 막대용 스위치)
    /// </summary>
    public class HealthBarEditor : Editor 
	{
		public MMHealthBar HealthBarTarget 
		{ 
			get 
			{ 
				return (MMHealthBar)target;
			}
		} 

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			switch (HealthBarTarget.HealthBarType)
			{
				case MMHealthBar.HealthBarTypes.Prefab:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"TargetProgressBar", "NestDrawnHealthBar", "Billboard", "FollowTargetMode", "Size","BackgroundPadding", "SortingLayerName", "InitialRotationAngles", "ForegroundColor", "DelayedColor", "BorderColor", "BackgroundColor", "Delay", "LerpFrontBar", "LerpFrontBarSpeed", "LerpDelayedBar", "LerpDelayedBarSpeed", "BumpScaleOnChange", "BumpDuration", "BumpAnimationCurve" });
					break;
				case MMHealthBar.HealthBarTypes.Drawn:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"TargetProgressBar", "HealthBarPrefab" });
					break;
				case MMHealthBar.HealthBarTypes.Existing:
					Editor.DrawPropertiesExcluding(serializedObject, new string[] {"HealthBarPrefab", "NestDrawnHealthBar", "Billboard", "FollowTargetMode", "Size","BackgroundPadding", "SortingLayerName", "InitialRotationAngles", "ForegroundColor", "DelayedColor", "BorderColor", "BackgroundColor", "Delay", "LerpFrontBar", "LerpFrontBarSpeed", "LerpDelayedBar", "LerpDelayedBarSpeed", "BumpScaleOnChange", "BumpDuration", "BumpAnimationCurve" });
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}

	}
}