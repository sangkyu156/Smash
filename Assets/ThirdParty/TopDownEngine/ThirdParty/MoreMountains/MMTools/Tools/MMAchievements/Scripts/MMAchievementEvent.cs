using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 업적이 잠금 해제되었다는 사실을 알리는 데 사용되는 이벤트 유형
    /// </summary>
    public struct MMAchievementUnlockedEvent
	{
		/// the achievement that has been unlocked
		public MMAchievement Achievement;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="newAchievement">New achievement.</param>
		public MMAchievementUnlockedEvent(MMAchievement newAchievement)
		{
			Achievement = newAchievement;
		}

		static MMAchievementUnlockedEvent e;
		public static void Trigger(MMAchievement newAchievement)
		{
			e.Achievement = newAchievement;
			MMEventManager.TriggerEvent(e);
		}
	}
	
	public struct MMAchievementChangedEvent
	{
		/// the achievement that has been unlocked
		public MMAchievement Achievement;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="newAchievement">New achievement.</param>
		public MMAchievementChangedEvent(MMAchievement newAchievement)
		{
			Achievement = newAchievement;
		}

		static MMAchievementChangedEvent e;
		public static void Trigger(MMAchievement newAchievement)
		{
			e.Achievement = newAchievement;
			MMEventManager.TriggerEvent(e);
		}
	}
}