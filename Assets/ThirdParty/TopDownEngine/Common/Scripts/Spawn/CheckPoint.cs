using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// üũ����Ʈ�� �������� �� Ʈ���ŵǴ� �̺�Ʈ
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
    /// üũ����Ʈ Ŭ����. �÷��̾ ������ �� �������� �ٽ� �����˴ϴ�.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Checkpoint")]
	public class CheckPoint : TopDownMonoBehaviour 
	{
		[Header("Spawn")]
		[MMInformation("�� ��ũ��Ʈ�� (������ ��� �ִ�) GameObject�� �߰��ϸ� ������ üũ����Ʈ ��Ͽ� �߰��Ǿ� �ű⿡�� �ٽ� ������ �� �ֽ��ϴ�. LevelManager�� �������� ���ε��ϸ� ���� ���� �� ĳ���Ͱ� �����Ǵ� ��ġ�� �˴ϴ�. �׸��� ���⿡�� ĳ���Ͱ� ���� �Ǵ� �������� ���� �����Ǿ�� �ϴ��� ������ �� �ֽ��ϴ�.", MMInformationAttribute.InformationType.Info,false)]
		/// the facing direction the character should face when spawning from this checkpoint
		[Tooltip("�� üũ����Ʈ���� ������ �� ĳ���Ͱ� ���ؾ� �ϴ� �����Դϴ�.")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.East ;
		/// whether or not this checkpoint should override any order and assign itself on entry
		[Tooltip("�� üũ����Ʈ�� ��� ����� �����ϰ� ���� �� ������ �Ҵ��ؾ� �ϴ��� ����")]
		public bool ForceAssignation = false;
		/// the order of the checkpoint
		[Tooltip("üũ����Ʈ�� ����")]
		public int CheckPointOrder;
        
		protected List<Respawnable> _listeners;

        /// <summary>
        /// ������ ����� �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}

        /// <summary>
        /// üũ����Ʈ���� �÷��̾ �����մϴ�.	
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