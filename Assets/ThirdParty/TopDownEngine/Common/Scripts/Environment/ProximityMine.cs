using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 충돌기(2D 또는 3D)에 추가하면 광산처럼 일정 기간 후에 작업을 트리거할 수 있습니다.
    /// 종료 시 타이머를 중단하거나 재설정하는 옵션도 제공됩니다.
    /// </summary>
    public class ProximityMine : TopDownMonoBehaviour
	{
		[Header("Proximity Mine")]
		/// the layers that will trigger this mine
		[Tooltip("이 광산을 작동시킬 레이어")]
		public LayerMask TargetLayerMask;
		/// whether or not to disable the mine when it triggers/explodes
		[Tooltip("지뢰가 작동/폭발할 때 지뢰를 비활성화할지 여부")]
		public bool DisableMineOnTrigger = true;

		[Header("WarningDuration")] 
		/// the duration of the warning phase, in seconds, betfore the mine triggers
		[Tooltip("지뢰가 작동하기 전의 경고 단계 기간(초)")]
		public float WarningDuration = 2f;
		/// whether or not the warning should stop when exiting the zone
		[Tooltip("영역을 나갈 때 경고가 중지되어야 하는지 여부")]
		public bool WarningStopsOnExit = false;
		/// whether or not the warning duration should reset when exiting the zone
		[Tooltip("구역을 나갈 때 경고 지속 시간을 재설정해야 하는지 여부")]
		public bool WarningDurationResetsOnExit = false;

		/// a read only display of the current duration before explosion
		[Tooltip("폭발 전 현재 기간의 읽기 전용 표시")]
		[MMReadOnly] 
		public float TimeLeftBeforeTrigger;
        
		[Header("Feedbacks")]
		/// the feedback to play when the warning phase starts
		[Tooltip("경고 단계가 시작될 때 재생할 피드백")]
		public MMFeedbacks OnWarningStartsFeedbacks;
		/// a feedback to play when the warning phase stops
		[Tooltip("경고 단계가 중지될 때 재생할 피드백")] 
		public MMFeedbacks OnWarningStopsFeedbacks;
		/// a feedback to play when the warning phase is reset
		[Tooltip("경고 단계가 재설정될 때 재생할 피드백")] 
		public MMFeedbacks OnWarningResetFeedbacks;
		/// a feedback to play when the mine triggers
		[Tooltip("광산이 작동할 때 재생할 피드백")]
		public MMFeedbacks OnMineTriggerFeedbacks;
        
		protected bool _inside = false;
        
		/// <summary>
		/// On Start we initialize our mine
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we initialize our feedbacks and duration
		/// </summary>
		public virtual void Initialization()
		{
			OnWarningStartsFeedbacks?.Initialization();
			OnWarningStopsFeedbacks?.Initialization();
			OnWarningResetFeedbacks?.Initialization();
			OnMineTriggerFeedbacks?.Initialization();
			TimeLeftBeforeTrigger = WarningDuration;
		}
        
		/// <summary>
		/// When colliding, we start our timer if needed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			_inside = true;
            
			OnWarningStartsFeedbacks?.PlayFeedbacks();
		}

		/// <summary>
		/// When exiting, we stop our timer if needed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Exiting(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			if (!WarningStopsOnExit)
			{
				return;
			}
            
			OnWarningStopsFeedbacks?.PlayFeedbacks();

			if (WarningDurationResetsOnExit)
			{
				OnWarningResetFeedbacks?.PlayFeedbacks();
				TimeLeftBeforeTrigger = WarningDuration;
			}
            
			_inside = false;
		}

		/// <summary>
		/// Describes what happens when the mine explodes
		/// </summary>
		public virtual void TriggerMine()
		{
			OnMineTriggerFeedbacks?.PlayFeedbacks();
            
			if (DisableMineOnTrigger)
			{
				this.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// On Update if a target is inside the zone, we update our timer
		/// </summary>
		protected virtual void Update()
		{
			if (_inside)
			{
				TimeLeftBeforeTrigger -= Time.deltaTime;
			}

			if (TimeLeftBeforeTrigger <= 0)
			{
				TriggerMine();
			}
		}
        
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger stay, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerStay(Collider collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerExit(Collider collider)
		{
			Exiting(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerExit2D(Collider2D collider)
		{
			Exiting(collider.gameObject);
		}
	}    
}