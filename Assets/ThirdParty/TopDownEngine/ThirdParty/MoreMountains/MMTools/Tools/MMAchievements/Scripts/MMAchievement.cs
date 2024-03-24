using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 업적 시스템은 단순(무언가 수행 > 성취 달성) 및 진행 기반(X회 점프, X명의 적을 죽이는 등)의 2가지 유형의 업적를 지원합니다.
    /// </summary>
    public enum AchievementTypes { Simple, Progress }

	[Serializable]
	public class MMAchievement  
	{
		[Header("Identification")]
        ///이 업적의 고유 식별자
        public string AchievementID;
        /// 이 성취도 진행 기반인가요 아니면
        public AchievementTypes AchievementType;
        /// 이것이 사실이라면 업적이 목록에 표시되지 않습니다.
        public bool HiddenAchievement;
        /// 이것이 사실이라면 업적이 잠금 해제된 것입니다. 그렇지 않으면 여전히 손에 넣을 수 있는 상태입니다.
        public bool UnlockedStatus;

		[Header("Description")]
        /// 업적 이름/제목
        public string Title;
        /// 업적 설명
        public string Description;
        /// 이 성과를 잠금 해제하는 포인트의 양에 따라 얻을 수 있습니다.
        public int Points;

		[Header("Image and Sounds")]
        /// 이 업적이 잠겨 있는 동안 표시할 이미지
        public Sprite LockedImage;
        /// 업적이 잠금 해제될 때 표시되는 이미지
        public Sprite UnlockedImage;
        /// 업적이 잠금 해제될 때 재생할 소리
        public AudioClip UnlockedSound;

		[Header("Progress")]
        /// 이 업적을 달성하는 데 필요한 진행 상황입니다.
        public int ProgressTarget;
        /// 이 업적에 대한 현재 진행 상황
        public int ProgressCurrent;

		protected MMAchievementDisplayItem _achievementDisplayItem;

        /// <summary>
        /// 업적을 잠금 해제하고 현재 업적 저장을 요청하며 이 업적에 대해 MMAchievementUnlockedEvent를 트리거합니다.
        /// 이는 일반적으로 MMAchievementDisplay 클래스에 의해 포착됩니다.
        /// </summary>
        public virtual void UnlockAchievement()
		{
			// if the achievement has already been unlocked, we do nothing and exit
			if (UnlockedStatus)
			{
				return;
			}

			UnlockedStatus = true;

			MMGameEvent.Trigger("Save");
			MMAchievementUnlockedEvent.Trigger(this);
		}

        /// <summary>
        /// 업적을 잠급니다.
        /// </summary>
        public virtual void LockAchievement()
		{
			UnlockedStatus = false;
		}

        /// <summary>
        /// 현재 진행 상황에 지정된 값을 추가합니다.
        /// </summary>
        /// <param name="newProgress">New progress.</param>
        public virtual void AddProgress(int newProgress)
		{
			ProgressCurrent += newProgress;
			EvaluateProgress();
		}

        /// <summary>
        /// 매개변수에 전달된 값으로 진행률을 설정합니다.
        /// </summary>
        /// <param name="newProgress">New progress.</param>
        public virtual void SetProgress(int newProgress)
		{
			ProgressCurrent = newProgress;
			EvaluateProgress();
		}

        /// <summary>
        /// 업적의 현재 진행 상황을 평가하고 필요한 경우 잠금을 해제합니다.
        /// </summary>
        protected virtual void EvaluateProgress()
		{
			MMAchievementChangedEvent.Trigger(this);
			if (ProgressCurrent >= ProgressTarget)
			{
				ProgressCurrent = ProgressTarget;
				UnlockAchievement();
			}
		}

        /// <summary>
        /// 이 업적을 복사합니다(스크립팅 가능한 개체 목록에서 로드할 때 유용함).
        /// </summary>
        public virtual MMAchievement Copy()
		{
			MMAchievement clone = new MMAchievement ();
			// we use Json utility to store a copy of our achievement, not a reference
			clone = JsonUtility.FromJson<MMAchievement>(JsonUtility.ToJson(this));
			return clone;
		}
	}
}