using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 일반적으로 발자국 입자 및/또는 소리를 트리거하는 데 사용되는 애니메이터의 피드백을 트리거하는 데 사용할 수 있습니다.
    /// </summary>
    public class CharacterAnimationFeedbacks : TopDownMonoBehaviour
	{
        /// 걷는 동안 발이 땅에 닿을 때마다 재생되는 피드백
        [Tooltip("걷는 동안 발이 땅에 닿을 때마다 재생되는 피드백")]
		public MMFeedbacks WalkFeedbacks;

        /// 달리는 동안 발이 땅에 닿을 때마다 재생되는 피드백
        [Tooltip("달리는 동안 발이 땅에 닿을 때마다 재생되는 피드백")]
		public MMFeedbacks RunFeedbacks;

		/// <summary>
		/// Plays the walk feedback if there's one, when a foot touches the ground (triggered via animation events)
		/// </summary>
		public virtual void WalkStep()
		{
			WalkFeedbacks?.PlayFeedbacks();
		}

		/// <summary>
		/// Plays the run feedback if there's one, when a foot touches the ground (triggered via animation events)
		/// </summary>
		public virtual void RunStep()
		{
			RunFeedbacks?.PlayFeedbacks();
		}
	}
}