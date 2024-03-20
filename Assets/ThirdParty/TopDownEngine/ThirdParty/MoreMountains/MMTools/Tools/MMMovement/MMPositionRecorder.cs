using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 Transform에 추가하면 주기적으로 위치가 기록됩니다.
    /// 그런 다음 Positions 배열을 어디에서나 읽어 해당 객체가 과거에 어디에 있었는지 알 수 있습니다.
    /// </summary>
    public class MMPositionRecorder : MonoBehaviour
	{
		/// the possible modes to run this recorder on 
		public enum Modes { Framecount, Time }

		[Header("Recording Settings")] 
		/// the amount of positions to record
		public int NumberOfPositionsToRecord = 100;
		/// whether to record every X frames, or every X seconds
		public Modes Mode = Modes.Framecount;
		/// the amount of frames to wait for between two recordings
		[MMEnumCondition("Mode", (int)Modes.Framecount)]
		public int FrameInterval = 0;
		/// the duration (in seconds) between two recordings
		[MMEnumCondition("Mode", (int) Modes.Time)]
		public float TimeInterval = 0.02f;
		/// whether or not to record if the timescale is 0
		public bool RecordOnTimescaleZero = false;
        
		[Header("Debug")]
		/// the array of positions (0 most recent, higher less recent)
		public Vector3[] Positions;
		/// the current frame counter
		[MMReadOnly]
		public int FrameCounter;

		protected int _frameCountLastRecord = 0;
		protected float _timeLastRecord = 0f;
        
		/// <summary>
		/// On Awake, we initialize our array of positions
		/// </summary>
		protected virtual void Awake()
		{
			Positions = new Vector3[NumberOfPositionsToRecord];
			for (int i = 0; i < Positions.Length; i++)
			{
				Positions[i] = this.transform.position;    
			}
		}

		/// <summary>
		/// On Update we store our positions
		/// </summary>
		protected virtual void Update()
		{
			if (!RecordOnTimescaleZero && Time.timeScale == 0f)
			{
				return;
			}
			StorePositions();
		}
        
		/// <summary>
		/// Stores the position in the array and offsets it
		/// </summary>
		protected virtual void StorePositions()
		{
			FrameCounter = Time.frameCount;

			if (Mode == Modes.Framecount)
			{
				if (FrameCounter - _frameCountLastRecord < FrameInterval)
				{
					return;
				}

				_frameCountLastRecord = FrameCounter;
			}
			else
			{
				if (Time.time - _timeLastRecord < TimeInterval)
				{
					return;
				}

				_timeLastRecord = Time.time;
			}
            
			// we put our current position in the array at index 0
			Positions[0] = this.transform.position;
            
			// we offset the array by 1 (index 0 moves to 1, etc)
			Array.Copy(Positions, 0, Positions, 1, Positions.Length - 1);
		}
	}
}