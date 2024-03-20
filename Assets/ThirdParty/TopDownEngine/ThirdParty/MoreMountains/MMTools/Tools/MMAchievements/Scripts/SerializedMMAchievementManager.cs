using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	[Serializable]
    /// <summary>
    /// 업적을 저장 파일에 저장하는 데 사용되는 직렬화 가능 클래스
    /// </summary>
    public class SerializedMMAchievement
	{
		public string AchievementID;
		public bool UnlockedStatus;
		public int ProgressCurrent;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.Tools.SerializedMMAchievement"/> class.
		/// </summary>
		/// <param name="achievementID">Achievement I.</param>
		/// <param name="unlockedStatus">If set to <c>true</c> unlocked status.</param>
		/// <param name="progressCurrent">Progress current.</param>
		public SerializedMMAchievement(string achievementID, bool unlockedStatus, int progressCurrent)
		{
			AchievementID = achievementID;
			UnlockedStatus = unlockedStatus;
			ProgressCurrent = progressCurrent;
		}
	}

	[Serializable]
	/// <summary>
	/// Serializable MM achievement manager.
	/// </summary>
	public class SerializedMMAchievementManager 
	{
		public SerializedMMAchievement[] Achievements;
	}
}