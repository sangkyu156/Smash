using UnityEngine;
using System.Collections;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 레벨의 경계를 나타내기 위해 이 클래스를 boxcollider에 추가하세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Camera/LevelLimits")]
	public class LevelLimits : TopDownMonoBehaviour
	{
		/// left x coordinate
		[Tooltip("왼쪽 x 좌표")]
		public float LeftLimit;
		/// right x coordinate
		[Tooltip("오른쪽 x 좌표")]
		public float RightLimit;
		/// bottom y coordinate 
		[Tooltip("하단 y 좌표 ")]
		public float BottomLimit;
		/// top y coordinate
		[Tooltip("상단 Y 좌표")]
		public float TopLimit;

		protected BoxCollider2D _collider;

		/// <summary>
		/// On awake, fills the public variables with the level's limits
		/// </summary>
		protected virtual void Awake()
		{
			_collider = GetComponent<BoxCollider2D>();

			LeftLimit = _collider.bounds.min.x;
			RightLimit = _collider.bounds.max.x;
			BottomLimit = _collider.bounds.min.y;
			TopLimit = _collider.bounds.max.y;
		}
	}
}