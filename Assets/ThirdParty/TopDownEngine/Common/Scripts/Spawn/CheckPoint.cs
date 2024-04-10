using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 체크포인트에 도달했을 때 트리거되는 이벤트
    /// </summary>
    public struct CheckPointEvent
	{
		public int Order;
		public CheckPointEvent(int order)
		{
			Order = order;
		}

		static CheckPointEvent e;
		public static void Trigger(int order)
		{
			e.Order = order;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 체크포인트 클래스. 플레이어가 죽으면 이 시점에서 다시 생성됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Checkpoint")]
	public class CheckPoint : TopDownMonoBehaviour 
	{
		[Header("Spawn")]
		[MMInformation("이 스크립트를 (가급적 비어 있는) GameObject에 추가하면 레벨의 체크포인트 목록에 추가되어 거기에서 다시 생성할 수 있습니다. LevelManager의 시작점에 바인딩하면 레벨 시작 시 캐릭터가 생성되는 위치가 됩니다. 그리고 여기에서 캐릭터가 왼쪽 또는 오른쪽을 향해 생성되어야 하는지 결정할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the facing direction the character should face when spawning from this checkpoint
		[Tooltip("이 체크포인트에서 생성될 때 캐릭터가 향해야 하는 방향입니다.")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.East ;
		/// whether or not this checkpoint should override any order and assign itself on entry
		[Tooltip("이 체크포인트가 모든 명령을 무시하고 진입 시 스스로 할당해야 하는지 여부")]
		public bool ForceAssignation = false;
		/// the order of the checkpoint
		[Tooltip("체크포인트의 순서")]
		public int CheckPointOrder;
        
		protected List<Respawnable> _listeners;

        /// <summary>
        /// 리스너 목록을 초기화합니다.
        /// </summary>
        protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}

        /// <summary>
        /// 체크포인트에서 플레이어를 생성합니다.	
        /// </summary>
        /// <param name="player">Player.</param>
        public virtual void SpawnPlayer(Character player)
		{
			player.RespawnAt(transform, FacingDirection);
			
			foreach(Respawnable listener in _listeners)
			{
				listener.OnPlayerRespawn(this,player);
			}
		}
		
		/// <summary>
		/// Assigns the Respawnable to this checkpoint
		/// </summary>
		/// <param name="listener"></param>
		public virtual void AssignObjectToCheckPoint (Respawnable listener) 
		{
			_listeners.Add(listener);
		}

		/// <summary>
		/// Describes what happens when something enters the checkpoint
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			TriggerEnter(collider.gameObject);            
		}

		protected virtual void OnTriggerEnter(Collider collider)
		{
			TriggerEnter(collider.gameObject);
		}

		protected virtual void TriggerEnter(GameObject collider)
		{
			Character character = collider.GetComponent<Character>();

			if (character == null) { return; }
			if (character.CharacterType != Character.CharacterTypes.Player) { return; }
			if (!LevelManager.HasInstance) { return; }
			LevelManager.Instance.SetCurrentCheckpoint(this);
			CheckPointEvent.Trigger(CheckPointOrder);
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR

			if (!LevelManager.HasInstance)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints.Count == 0)
			{
				return;
			}

			for (int i=0; i < LevelManager.Instance.Checkpoints.Count; i++)
			{
				// we draw a line towards the next point in the path
				if ((i+1) < LevelManager.Instance.Checkpoints.Count)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(LevelManager.Instance.Checkpoints[i].transform.position,LevelManager.Instance.Checkpoints[i+1].transform.position);
				}
			}
			#endif
		}
	}
}