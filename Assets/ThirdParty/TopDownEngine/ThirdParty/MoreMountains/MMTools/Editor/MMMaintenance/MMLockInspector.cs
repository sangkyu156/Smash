using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// ctrl(또는 cmd) + L을 눌러 현재 검사기를 잠글 수 있는 간단한 클래스
    /// 동일한 단축키를 다시 누르면 잠금이 해제됩니다.
    /// </summary>
    public class MMLockInspector : MonoBehaviour
	{
		[MenuItem("Tools/More Mountains/Lock Inspector %l")]
		static public void LockInspector()
		{
			Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			EditorWindow inspectorWindow = EditorWindow.GetWindow(inspectorType);

			PropertyInfo isLockedPropertyInfo = inspectorType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
			bool state = (bool)isLockedPropertyInfo.GetGetMethod().Invoke(inspectorWindow, new object[] { });

			isLockedPropertyInfo.GetSetMethod().Invoke(inspectorWindow, new object[] { !state });
		}
	}
}