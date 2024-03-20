using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 물리 기반 발사체(ProjectileWeapon에 의해 투척됨)에 이 클래스를 사용합니다.
    /// </summary>
    public class PhysicsProjectile : Projectile
	{
		[Header("Physics")] 
		public float InitialForce = 10f;
		public Vector3 InitialRotation = Vector3.zero;

		public ForceMode InitialForceMode = ForceMode.Impulse;
		public ForceMode2D InitialForceMode2D = ForceMode2D.Impulse;

		public override void Movement()
		{
			//do nothing
		}
		
		public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			base.SetDirection(newDirection, newRotation, spawnerIsFacingRight);
			
			this.transform.Rotate(InitialRotation, Space.Self);

			newDirection = this.transform.forward;
			
			if (_rigidBody != null)
			{
				_rigidBody.AddForce(newDirection * InitialForce, InitialForceMode);	
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.AddForce(newDirection * InitialForce, InitialForceMode2D);
			}
		}
		
		/// <summary>
		/// Sets the associated rb or rb2D to kinematic or not depending on the state
		/// </summary>
		/// <param name="state"></param>
		protected virtual void SetRigidbody(bool state)
		{
			if (_rigidBody != null)
			{
				_rigidBody.isKinematic = state;
			}

			if (_rigidBody2D != null)
			{
				_rigidBody2D.isKinematic = state;
			}
		}
		
		/// <summary>
		/// On enable, we force our rb to not be kinematic
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			SetRigidbody(false);
		}

		/// <summary>
		/// On disable, we force our rb to be kinematic to kill any remaining velocity
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			SetRigidbody(true);
		}
	}	
}