using System.Collections;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 박스 콜라이더에 추가하면 입력 시 가상 카메라를 활성화하는 영역을 정의하여 레벨 내부 섹션을 쉽게 정의할 수 있습니다.
    /// </summary>
    public class TopDownCinemachineZone3D : MMCinemachineZone3D
	{
		[Header("Top Down Engine")]
		/// if this is true, the zone will require colliders that want to trigger it to have a Character components of type Player
		[Tooltip("이것이 사실이라면 영역에는 플레이어 유형의 캐릭터 구성 요소를 갖도록 트리거하려는 충돌체가 필요합니다.")]
		public bool RequiresPlayerCharacter = true;
		protected CinemachineCameraController _cinemachineCameraController;
		protected Character _character;
        
		/// <summary>
		/// On Awake, adds a camera controller if needed
		/// </summary>
		#if MM_CINEMACHINE
		protected override void Awake()
		{
			base.Awake();
			if (Application.isPlaying)
			{
				_cinemachineCameraController = VirtualCamera.gameObject.MMGetComponentAroundOrAdd<CinemachineCameraController>();
				_cinemachineCameraController.ConfineCameraToLevelBounds = false;    
			}
		}

		/// <summary>
		/// Enables/Disables the camera
		/// </summary>
		/// <param name="state"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		protected override IEnumerator EnableCamera(bool state, int frames)
		{
			yield return base.EnableCamera(state, frames);
			if (state)
			{
				_cinemachineCameraController.FollowsAPlayer = true;
				_cinemachineCameraController.StartFollowing();
			}
			else
			{
				_cinemachineCameraController.StopFollowing();
				_cinemachineCameraController.FollowsAPlayer = false;
			}
		}
		
		/// <summary>
		/// An extra test you can override to add extra collider conditions
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected override bool TestCollidingGameObject(GameObject collider)
		{
			if (RequiresPlayerCharacter)
			{
				_character = collider.MMGetComponentNoAlloc<Character>();
				if (_character == null)
				{
					return false;
				}

				if (_character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}
			
			return true;
		}
		#endif
	}    
}