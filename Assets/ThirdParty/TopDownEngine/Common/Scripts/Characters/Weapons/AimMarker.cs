using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 조준 마커(보통 원형) 시각적 요소를 처리하는 데 사용되는 클래스
    /// </summary>
    public class AimMarker : TopDownMonoBehaviour
	{
		/// the possible movement modes for aim markers
		public enum MovementModes { Instant, Interpolate }

		[Header("Movement")]
        /// 이 조준 마커에 대해 선택된 이동 모드입니다. Instant는 마커를 대상으로 즉시 이동하고 Interpolate는 시간이 지남에 따라 위치를 애니메이션으로 표시합니다.
        [Tooltip("이 조준 마커에 대해 선택된 이동 모드입니다. Instant는 마커를 대상으로 즉시 이동하고 Interpolate는 시간이 지남에 따라 위치를 애니메이션으로 표시합니다.")]
		public MovementModes MovementMode;
        /// 대상 위치에 적용할 오프셋(예를 들어 대상 위에 마커를 표시하려는 경우 유용함)
        [Tooltip("대상 위치에 적용할 오프셋(예를 들어 대상 위에 마커를 표시하려는 경우 유용함)")]
		public Vector3 Offset;
        /// 보간 모드에 있을 때 이동 애니메이션의 지속 시간
        [Tooltip("보간 모드에 있을 때 이동 애니메이션의 지속 시간")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public float MovementDuration = 0.2f;
        /// 보간 모드에 있을 때 움직임을 애니메이션하는 곡선
        [Tooltip("보간 모드에 있을 때 움직임을 애니메이션하는 곡선")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public MMTween.MMTweenCurve MovementCurve = MMTween.MMTweenCurve.EaseInCubic;
        /// 보간 모드에서 대상을 변경할 때 마커가 이동하기 전 지연 시간
        [Tooltip("보간 모드에서 대상을 변경할 때 마커가 이동하기 전 지연 시간")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public float MovementDelay = 0f;

		[Header("Feedbacks")]
        /// 목표를 찾았지만 아직 목표가 없었을 때 플레이할 피드백
        [Tooltip("목표를 찾았지만 아직 목표가 없었을 때 플레이할 피드백")]
		public MMFeedbacks FirstTargetFeedback;
        /// 이미 목표가 있고 방금 새 목표를 찾았을 때 플레이할 수 있는 피드백
        [Tooltip("이미 목표가 있고 방금 새 목표를 찾았을 때 플레이할 수 있는 피드백")]
		public MMFeedbacks NewTargetAssignedFeedback;
        /// 더 이상 목표를 찾을 수 없고 마지막 목표를 잃었을 때 플레이하기 위한 피드백
        [Tooltip("더 이상 목표를 찾을 수 없고 마지막 목표를 잃었을 때 플레이하기 위한 피드백")]
		public MMFeedbacks NoMoreTargetFeedback;

		protected Transform _target;
		protected Transform _targetLastFrame = null;
		protected WaitForSeconds _movementDelayWFS;
		protected float _lastTargetChangeAt = 0f;

		/// <summary>
		/// On Awake we initialize our feedbacks and delay
		/// </summary>
		protected virtual void Awake()
		{
			FirstTargetFeedback?.Initialization(this.gameObject);
			NewTargetAssignedFeedback?.Initialization(this.gameObject);
			NoMoreTargetFeedback?.Initialization(this.gameObject);
			if (MovementDelay > 0f)
			{
				_movementDelayWFS = new WaitForSeconds(MovementDelay);
			}
		}

		/// <summary>
		/// On Update we check if we've changed target, and follow it if needed
		/// </summary>
		protected virtual void Update()
		{
			HandleTargetChange();
			FollowTarget();
			_targetLastFrame = _target;
		}

		/// <summary>
		/// Makes this object follow the target's position
		/// </summary>
		protected virtual void FollowTarget()
		{
			if (MovementMode == MovementModes.Instant)
			{
				this.transform.position = _target.transform.position + Offset;
			}
			else
			{
				if ((_target != null) && (Time.time - _lastTargetChangeAt > MovementDuration))
				{
					this.transform.position = _target.transform.position + Offset;
				}
			}
		}

		/// <summary>
		/// Sets a new target for this aim marker
		/// </summary>
		/// <param name="newTarget"></param>
		public virtual void SetTarget(Transform newTarget)
		{
			_target = newTarget;

			if (newTarget == null)
			{
				return;
			}

			this.gameObject.SetActive(true);

			if (_targetLastFrame == null)
			{
				this.transform.position = _target.transform.position + Offset;
			}

			if (MovementMode == MovementModes.Instant)
			{
				this.transform.position = _target.transform.position + Offset;
			}
			else
			{
				MMTween.MoveTransform(this, this.transform, this.transform.position, _target.transform.position + Offset, _movementDelayWFS, MovementDelay, MovementDuration, MovementCurve);
			}
		}

		/// <summary>
		/// Checks for target changes and triggers the appropriate methods if needed
		/// </summary>
		protected virtual void HandleTargetChange()
		{
			if (_target == _targetLastFrame)
			{
				return;
			}

			_lastTargetChangeAt = Time.time;

			if (_target == null)
			{
				NoMoreTargets();
				return;
			}

			if (_targetLastFrame == null)
			{
				FirstTargetFound();
				return;
			}

			if ((_targetLastFrame != null) && (_target != null))
			{
				NewTargetFound();
			}
		}

		/// <summary>
		/// When no more targets are found, and we just lost one, we play a dedicated feedback
		/// </summary>
		protected virtual void NoMoreTargets()
		{
			NoMoreTargetFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// When a new target is found and we didn't have one already, we play a dedicated feedback
		/// </summary>
		protected virtual void FirstTargetFound()
		{
			FirstTargetFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// When a new target is found, and we previously had another, we play a dedicated feedback
		/// </summary>
		protected virtual void NewTargetFound()
		{
			NewTargetAssignedFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// Hides this object
		/// </summary>
		public virtual void Disable()
		{
			this.gameObject.SetActive(false);
		}
	}
}