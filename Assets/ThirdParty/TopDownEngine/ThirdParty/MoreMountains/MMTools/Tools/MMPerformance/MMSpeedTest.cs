using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 속도 테스트와 관련된 데이터를 저장하는 구조체
    /// </summary>
    public struct MMSpeedTestItem
	{
		/// the name of the test, has to be unique
		public string TestID;
		/// a stopwatch to compute time
		public Stopwatch Timer;
		/// <summary>
		/// Creates a speed test with the specified ID and starts the timer
		/// </summary>
		/// <param name="testID"></param>
		public MMSpeedTestItem(string testID)
		{
			TestID = testID;
			Timer = Stopwatch.StartNew();
		}
	}

    /// <summary>
    /// 이 클래스를 사용하여 코드에서 성능 테스트를 실행합니다.
    /// StartTest 호출과 EndTest 호출 사이에 소요된 시간을 출력합니다.
    /// 두 호출 모두에 고유 ID를 사용해야 합니다.
    /// </summary>
    public static class MMSpeedTest 
	{
		private static readonly Dictionary<string, MMSpeedTestItem> _speedTests = new Dictionary<string, MMSpeedTestItem>();

		/// <summary>
		/// Starts a speed test of the specified ID
		/// </summary>
		/// <param name="testID"></param>
		public static void StartTest(string testID)
		{
			if (_speedTests.ContainsKey(testID))
			{
				_speedTests.Remove(testID);
			}

			MMSpeedTestItem item = new MMSpeedTestItem(testID);
			_speedTests.Add(testID, item);
		}

		/// <summary>
		/// Stops a speed test of the specified ID
		/// </summary>
		public static void EndTest(string testID)
		{
			if (!_speedTests.ContainsKey(testID))
			{
				return;
			}

			_speedTests[testID].Timer.Stop();
			float elapsedTime = _speedTests[testID].Timer.ElapsedMilliseconds / 1000f;
			_speedTests.Remove(testID);

			UnityEngine.Debug.Log("<color=red>MMSpeedTest</color> [Test "+testID+"] test duration : "+elapsedTime+"s");
		}        
	}
}