using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 정적 메서드에 추가할 속성은 MMDebugMenu의 명령줄을 통해 호출할 수 있습니다.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
	public class MMDebugLogCommandAttribute : System.Attribute { }
}