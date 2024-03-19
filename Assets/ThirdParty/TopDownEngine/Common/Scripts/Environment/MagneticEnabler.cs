using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 개체는 관련 collider2D에 들어갈 때 장면에서 자기 개체를 활성화합니다(추가해야 함).
    /// 자성 물체는 스스로 작동하고 자체 범위 감지를 처리할 수 있지만, 인에이블러가 물체를 움직이게 하는 다른 아키텍처를 사용할 수도 있습니다.
    /// 일반적인 사용 사례는 최상위 수준 아래에 중첩된 캐릭터에 추가하는 것입니다.
    /// 
    /// MyCharacter(최상위, 캐릭터, 컨트롤러, 능력 등 포함)
    /// - MyMagneticEnabler(예를 들어 이 클래스와 CircleCollider2D 사용)
    ///
    /// 그러면 장면에 StartMagnetOnEnter가 비활성화된 Magnetic 개체가 있게 됩니다.
    /// 자기 활성화 장치는 입력 시 이 특정 대상을 따르도록 만듭니다.
    /// 인에이블러에서 추종 속도와 가속도를 재정의하도록 할 수도 있습니다.
    /// </summary>
    public class MagneticEnabler : TopDownMonoBehaviour
	{
		[Header("Detection")]
		/// the layermask this magnetic enabler looks at to enable magnetic elements
		[Tooltip("이 자기 활성화 장치가 자기 요소를 활성화하기 위해 살펴보는 레이어 마스크")]
		public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;
		/// a list of the magnetic type ID this enabler targets
		[Tooltip("이 Enabler가 대상으로 하는 자기 유형 ID 목록")]
		public List<int> MagneticTypeIDs;

		[Header("Overrides")]
		/// if this is true, the follow position speed will be overridden with the one specified here
		[Tooltip("이것이 사실이라면 추종 위치 속도는 여기에 지정된 속도로 재정의됩니다.")]
		public bool OverrideFollowPositionSpeed = false;
		/// the value with which to override the speed
		[Tooltip("속도를 무시할 속도")]
		[MMCondition("OverrideFollowPositionSpeed", true)]
		public float FollowPositionSpeed = 5f;
		/// if this is true, the acceleration will be overridden with the one specified here
		[Tooltip("이것이 사실이라면 가속도는 여기에 지정된 것으로 재정의됩니다.")]
		public bool OverrideFollowAcceleration = false;
		/// the value with which to override the acceleration
		[Tooltip("가속도를 무시하는 속도")]
		[MMCondition("OverrideFollowAcceleration", true)]
		public float FollowAcceleration = 0.75f;

		protected Collider2D _collider2D;
		protected Magnetic _magnetic;

		/// <summary>
		/// On Awake we initialize our magnetic enabler
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs the collider2D and ensures it's set as trigger
		/// </summary>
		protected virtual void Initialization()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			if (_collider2D != null)
			{
				_collider2D.isTrigger = true;
			}
		}

		/// <summary>
		/// When something enters our trigger 2D, if it's a proper target, we start following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter2D(Collider2D colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

		/// <summary>
		/// When something enters our trigger, if it's a proper target, we start following it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider colliding)
		{
			OnTriggerEnterInternal(colliding.gameObject);
		}

		/// <summary>
		/// If we trigger with a Magnetic, and the ID matches, we enable it 
		/// </summary>
		/// <param name="colliding"></param>
		protected virtual void OnTriggerEnterInternal(GameObject colliding)
		{
			if (!TargetLayerMask.MMContains(colliding.layer))
			{
				return;
			}

			_magnetic = colliding.MMGetComponentNoAlloc<Magnetic>();
			if (_magnetic == null)
			{
				return;
			}

			bool idFound = false;
			if (_magnetic.MagneticTypeID == 0)
			{
				idFound = true;
			}
			else
			{
				foreach (int id in MagneticTypeIDs)
				{
					if (id == _magnetic.MagneticTypeID)
					{
						idFound = true;
					}
				}
			}            

			if (!idFound)
			{
				return;
			}

			if (OverrideFollowAcceleration)
			{
				_magnetic.FollowAcceleration = FollowAcceleration;
			}

			if (OverrideFollowPositionSpeed)
			{
				_magnetic.FollowPositionSpeed = FollowPositionSpeed;
			}

			_magnetic.SetTarget(this.transform);
			_magnetic.StartFollowing();
		}
	}
}