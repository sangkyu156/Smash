using MoreMountains.Tools;
using PolygonArsenal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// WeaponAutoAim의 3D 버전으로, WeaponAim3D가 장착된 개체에 사용됩니다.
    /// 정의된 반경 내의 대상을 감지하고, 가장 가까운 대상을 선택하고, 대상이 발견되면 WeaponAim 구성 요소가 해당 대상을 조준하도록 강제합니다.
    /// </summary>
    [RequireComponent(typeof(WeaponAim3D))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim 3D")]
	public class WeaponAutoAim3D : WeaponAutoAim
	{
		[Header("Overlap Detection")]
		/// the maximum amount of targets the overlap detection can acquire
		[Tooltip("중첩 감지가 획득할 수 있는 최대 대상 수")]
		public int OverlapMaximum = 10;

		//플레이어 정보를 넘겨줄 스크립트
		public PolygonBeamStatic beamscript;

        protected Vector3 _aimDirection;
		protected Collider[] _hits;
		protected Vector3 _raycastDirection;
		protected Collider _potentialHit;
		protected TopDownController3D _topDownController3D;
		protected Vector3 _origin;
		protected List<Transform> _potentialTargets;
        
		public Vector3 Origin
		{
			get
			{
				_origin = this.transform.position;
				if (_topDownController3D != null)
				{
					_origin += Quaternion.FromToRotation(Vector3.forward, _topDownController3D.CurrentDirection.normalized) * DetectionOriginOffset;
				}
				return _origin;
			}
		}

        /// <summary>
        /// On init we grab our orientation to be able to detect facing direction
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_potentialTargets = new List<Transform>();
			_hits = new Collider[10];
			if (_weapon.Owner != null)
			{
				_topDownController3D = _weapon.Owner.GetComponent<TopDownController3D>();
			}
		}

        /// <summary>
        /// 중첩 감지를 수행한 후 박스캐스트로 사선을 확인하여 표적을 스캔합니다.
        /// </summary>
        /// <returns></returns>
        protected override bool ScanForTargets()
		{
			Target = null;
            
			int numberOfHits = Physics.OverlapSphereNonAlloc(Origin, ScanRadius, _hits, TargetsMask);
            
			if (numberOfHits == 0)
			{
				return false;
			}
            
			_potentialTargets.Clear();

            // 우리는 발견된 각 collider를 살펴봅니다.
            int min = Mathf.Min(OverlapMaximum, numberOfHits);
			for (int i = 0; i < min; i++)
			{
				if (_hits[i] == null)
				{
					continue;
				}
				if ((_hits[i].gameObject == this.gameObject) || (_hits[i].transform.IsChildOf(this.transform)))
				{
					continue;
				}  
                
				_potentialTargets.Add(_hits[i].gameObject.transform);
			}

            // 우리는 거리에 따라 목표를 정렬합니다
            _potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector3.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
			});

            // 우리는 가려지지 않은 첫 번째 타겟을 반환합니다.
            foreach (Transform t in _potentialTargets)
			{
				_raycastDirection = t.position - _raycastOrigin;
				RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, _raycastDirection.magnitude, ObstacleMask.value, Color.yellow, true);
				if ((hit.collider == null) && CanAcquireNewTargets())
				{
					Target = t;
					PassingPlayerInformation();//레이저 쏘려고 별짓다함 ㅋㅋ(플레이어 위치 넘김)
                    return true;
				}
			}

			//_weapon.CharacterHandleWeapon.ForceStop();//타겟이 없으면 강제로 무기 정지
            return false;
		}

		public void PassingPlayerInformation()
		{
			if (beamscript != null)
				beamscript.target = Target;
        }

        /// <summary>
        /// Sets the aim to the relative direction of the target
        /// </summary>
        protected override void SetAim()
		{
			_aimDirection = (Target.transform.position - _raycastOrigin).normalized;
			_weaponAim.SetCurrentAim(_aimDirection, ApplyAutoAimAsLastDirection);
		}

		/// <summary>
		/// Determines the raycast origin
		/// </summary>
		protected override void DetermineRaycastOrigin()
		{
			_raycastOrigin = Origin;
		}
        
		protected override void OnDrawGizmos()
		{
			if (DrawDebugRadius)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(Origin, ScanRadius);
			}
		}
	}
}