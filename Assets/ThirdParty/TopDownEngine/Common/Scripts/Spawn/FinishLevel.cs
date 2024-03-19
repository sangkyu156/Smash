using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 트리거에 추가하면 플레이어가 다음 레벨로 이동합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Finish Level")]
	public class FinishLevel : ButtonActivated
	{
		[Header("Finish Level")]
        /// 전환할 레벨의 정확한 이름
        [Tooltip("전환할 레벨의 정확한 이름")]
		public string LevelName;

        /// <summary>
        /// 버튼을 누르면 대화가 시작됩니다
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction ();
			GoToNextLevel();
		}

        /// <summary>
        /// 다음 레벨을 로드합니다.
        /// </summary>
        public virtual void GoToNextLevel()
		{
			if (LevelManager.HasInstance)
			{
				LevelManager.Instance.GotoLevel(LevelName);
			}
			else
			{
				MMSceneLoadingManager.LoadScene(LevelName);
			}
		}
	}
}