using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	[CreateAssetMenu(fileName="AchievementList",menuName="MoreMountains/Achievement List")]
    /// <summary>
    /// 업적 목록이 포함된 스크립트 가능한 개체입니다. 이 작업을 수행하려면 하나를 만들고 리소스 폴더에 저장해야 합니다.
    /// </summary>
    public class MMAchievementList : ScriptableObject 
	{
		/// the unique ID of this achievement list. This is used to save/load data.
		public string AchievementsListID = "AchievementsList";

		/// the list of achievements 
		public List<MMAchievement> Achievements;

		/// <summary>
		/// Asks for a reset of all the achievements in this list (they'll all be locked again, their progress lost).
		/// </summary>
		public virtual void ResetAchievements()
		{
			Debug.LogFormat ("Reset Achievements");
			MMAchievementManager.ResetAchievements (AchievementsListID);
		}

		private MMReferenceHolder<MMAchievementList> _instances;
		protected virtual void OnEnable() { _instances.Reference(this); }
		protected virtual void OnDisable() { _instances.Dispose(); }
		public static MMAchievementList Any => MMReferenceHolder<MMAchievementList>.Any;
	}
}