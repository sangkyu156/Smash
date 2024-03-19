using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 장면의 빈 개체에 추가하면 로그 및 디버그 그리기를 활성화 또는 비활성화하는 제어 지점 역할을 합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Utilities/MMDebugController")]
	public class MMDebugController : MonoBehaviour
	{
		/// whether or not debug logs (MMDebug.DebugLogTime, MMDebug.DebugOnScreen) should be displayed
		public bool DebugLogsEnabled = true;
		/// whether or not debug draws should be executed
		public bool DebugDrawEnabled = true;

		/// <summary>
		/// On Awake we turn our static debug checks on or off
		/// </summary>
		protected virtual void Awake()
		{
			MMDebug.SetDebugLogsEnabled(DebugLogsEnabled);
			MMDebug.SetDebugDrawEnabled(DebugDrawEnabled);
		}
	}
}