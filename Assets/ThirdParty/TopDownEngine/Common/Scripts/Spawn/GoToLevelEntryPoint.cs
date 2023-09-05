using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
	/// 대상 레벨에 진입점을 지정하면서 한 레벨에서 다음 레벨로 이동하는 데 사용되는 클래스입니다.
	/// 진입점은 각 레벨의 LevelManager 구성 요소에 정의됩니다. 이는 단순히 목록의 변환일 뿐입니다.
	/// 목록의 인덱스는 진입점의 식별자입니다. 
    /// </summary>
        [AddComponentMenu("TopDown Engine/Spawn/GoToLevelEntryPoint")]
	public class GoToLevelEntryPoint : FinishLevel 
	{
		[Space(10)]
		[Header("Points of Entry")]

        /// 진입점을 사용할지 여부입니다. 그렇지 않으면 다음 레벨로 넘어가게 됩니다.
        [Tooltip("Whether or not to use entry points. If you don't, you'll simply move on to the next level")]
		public bool UseEntryPoints = false;
        /// 다음 레벨로 이동할 진입점 인덱스
        [Tooltip("The index of the point of entry to move to in the next level")]
		public int PointOfEntryIndex;
        /// 다음 레벨로 이동할 때 향하는 방향
        [Tooltip("The direction to face when moving to the next level")]
		public Character.FacingDirections FacingDirection;

        /// <summary>
        /// 다음 레벨을 로드하고 게임 관리자에 대상 진입점 인덱스를 저장합니다.
        /// </summary>
        public override void GoToNextLevel()
		{
			if (UseEntryPoints)
			{
				GameManager.Instance.StorePointsOfEntry(LevelName, PointOfEntryIndex, FacingDirection);
			}
			
			base.GoToNextLevel ();
		}
	}
}