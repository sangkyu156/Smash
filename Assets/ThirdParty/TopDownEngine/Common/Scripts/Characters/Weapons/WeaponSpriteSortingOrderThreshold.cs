using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 무기의 현재 회전을 기반으로 정렬 순서와 관련된 스프라이트를 수정합니다.
    /// 2D 무기의 이 각도에 따라 무기가 캐릭터 앞이나 뒤에 있게 하는 데 유용합니다.
    /// </summary>
    [RequireComponent(typeof(WeaponAim2D))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Sprite Sorting Order Threshold")]
	public class WeaponSpriteSortingOrderThreshold : TopDownMonoBehaviour
	{
		/// the angle threshold at which to switch the sorting order
		[Tooltip("정렬 순서를 전환할 각도 임계값")]
		public float Threshold = 0f;
		/// the sorting order to apply when the weapon's rotation is below threshold
		[Tooltip("무기 회전이 임계값 미만일 때 적용할 정렬 순서")]
		public int BelowThresholdSortingOrder = 1;
		/// the sorting order to apply when the weapon's rotation is above threshold
		[Tooltip("무기 회전이 임계값을 초과할 때 적용할 정렬 순서")]
		public int AboveThresholdSortingOrder = -1;
		/// the sprite whose sorting order we want to modify
		[Tooltip("정렬 순서를 수정하려는 스프라이트")]
		public SpriteRenderer Sprite;

		protected WeaponAim2D _weaponAim2D;
        
		/// <summary>
		/// On Awake we grab our weapon aim component
		/// </summary>
		protected virtual void Awake()
		{
			_weaponAim2D = this.gameObject.GetComponent<WeaponAim2D>();
		}

		/// <summary>
		/// On update we change our sorting order based on current weapon angle
		/// </summary>
		protected virtual void Update()
		{
			if ((_weaponAim2D == null) || (Sprite == null)) 
			{
				return;
			}

			Sprite.sortingOrder = (_weaponAim2D.CurrentAngleRelative > Threshold) ? AboveThresholdSortingOrder : BelowThresholdSortingOrder;
		}
	}
}