using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이것을 캐릭터에 추가하면 사망 시 MMRagdoller가 래그돌로 작동됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Ragdoll on Death")]
	public class CharacterRagdollOnDeath : TopDownMonoBehaviour
	{
		[Header("Binding")]
		/// The MMRagdoller for this character
		[Tooltip("이 캐릭터의 MMRagdoller")]
		public MMRagdoller Ragdoller;
		/// A list of optional objects to disable on death
		[Tooltip("사망 시 비활성화할 선택적 개체 목록")]
		public List<GameObject> ObjectsToDisableOnDeath;
		/// A list of optional monos to disable on death
		[Tooltip("사망 시 비활성화할 선택적 모노 목록")]
		public List<MonoBehaviour> MonosToDisableOnDeath;

		[Header("Force")]
		/// the force by which the impact will be multiplied
		[Tooltip("충격이 배가되는 힘")]
		public float ForceMultiplier = 10000f;

		[Header("Test")]
		/// A test button to trigger the ragdoll from the inspector
		[MMInspectorButton("Ragdoll")]
		[Tooltip("검사기에서 봉제 인형을 실행하는 테스트 버튼")]
		public bool RagdollButton;
		/// A test button to reset the ragdoll from the inspector
		[MMInspectorButton("ResetRagdoll")]
		[Tooltip("검사기에서 봉제 인형을 재설정하는 테스트 버튼")]
		public bool ResetRagdollButton;
        
		protected TopDownController _controller;
		protected Health _health;
		protected Transform _initialParent;
		protected Vector3 _initialPosition;
		protected Quaternion _initialRotation;
        
		/// <summary>
		/// On Awake we initialize our component
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs our health and controller
		/// </summary>
		protected virtual void Initialization()
		{
			_health = this.gameObject.GetComponent<Health>();
			_controller = this.gameObject.GetComponent<TopDownController>();
			_initialParent = Ragdoller.transform.parent;
			_initialPosition = Ragdoller.transform.localPosition;
			_initialRotation = Ragdoller.transform.localRotation;
		}

		/// <summary>
		/// When we get a OnDeath event, we ragdoll
		/// </summary>
		protected virtual void OnDeath()
		{
			Ragdoll();
		}

		protected virtual void OnRevive()
		{
			this.transform.position = Ragdoller.GetPosition();
			ResetRagdoll();
		}

		/// <summary>
		/// Disables the specified objects and monos and triggers the ragdoll
		/// </summary>
		protected virtual void Ragdoll()
		{
			foreach (GameObject go in ObjectsToDisableOnDeath)
			{
				go.SetActive(false);
			}
			foreach (MonoBehaviour mono in MonosToDisableOnDeath)
			{
				mono.enabled = false;
			}
			Ragdoller.Ragdolling = true;
			Ragdoller.transform.SetParent(null);
			Ragdoller.MainRigidbody.AddForce(_controller.AppliedImpact.normalized * ForceMultiplier, ForceMode.Acceleration);
		}

		public virtual void ResetRagdoll()
		{
			Ragdoller.AllowBlending = false;
			
			foreach (GameObject go in ObjectsToDisableOnDeath)
			{
				go.SetActive(true);
			}
			foreach (MonoBehaviour mono in MonosToDisableOnDeath)
			{
				mono.enabled = true;
			}
			
			Ragdoller.transform.SetParent(_initialParent);
			Ragdoller.Ragdolling = false;
			Ragdoller.transform.localPosition = _initialPosition;
			Ragdoller.transform.localRotation = _initialRotation;
		}

		/// <summary>
		/// On enable we start listening to OnDeath events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnRevive += OnRevive;
			}
		}
        
		/// <summary>
		/// OnDisable we stop listening to OnDeath events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnRevive -= OnRevive;
			}
		}
	}
}