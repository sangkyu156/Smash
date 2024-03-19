using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMDebugMenu 데모 장면에서 몇 가지 값을 흔들어 화면 콘솔의 디버그에 출력하는 데 사용되는 간단한 테스트 클래스입니다.
    /// </summary>
    public class MMDebugMenuTestClass : MonoBehaviour
	{
		/// a label to display
		public string Label;

		private float multiplier;

		/// <summary>
		/// On starts, randomizes a multiplier
		/// </summary>
		private void Start()
		{
			multiplier = Random.Range(0f, 50000f);
		}
		/// <summary>
		/// On update, outputs a text on screen
		/// </summary>
		void Update()
		{
			float test = (Mathf.Sin(Time.time) + 2) * multiplier;
			MMDebug.DebugOnScreen(Label, test);
		}
	}
}