using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMAudioBroadcaster에 의해 방송될 대상 MMAudioAnalyzer의 비트 레벨을 노출하는 데 사용되는 클래스
    /// </summary>
    public class MMRadioSignalAudioAnalyzer : MMRadioSignal
	{
		[Header("Audio Analyzer")]
		/// the MMAudioAnalyzer to read the value on
		public MMAudioAnalyzer TargetAnalyzer;
		/// the ID of the beat to listen to
		public int BeatID;

		/// <summary>
		/// On Shake, we output our beat value
		/// </summary>
		protected override void Shake()
		{
			base.Shake();
			CurrentLevel = TargetAnalyzer.Beats[BeatID].CurrentValue * GlobalMultiplier;
		}
	}
}