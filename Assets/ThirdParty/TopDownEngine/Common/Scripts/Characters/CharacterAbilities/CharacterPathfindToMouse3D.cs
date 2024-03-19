using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 플레이어 캐릭터에 사용되는 이 능력을 사용하면 땅을 클릭하고 캐릭터를 클릭 위치로 이동할 수 있습니다.
    /// LoftSuspendersMouseDriven 데모 캐릭터에서 이 능력의 데모를 찾을 수 있습니다. Loft3D 데모 장면의 LevelManager의 PlayerPrefabs 슬롯으로 드래그하여 시도해 볼 수 있습니다.
    /// AI의 경우 대신 MousePathfinderAI3D 스크립트와 MinimalPathfinding3D 데모 장면의 데모를 살펴보세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Pathfind To Mouse")]
	[RequireComponent(typeof(CharacterPathfinder3D))]
	public class CharacterPathfindToMouse3D : CharacterAbility
	{
		[Header("Mouse")]
		/// the index of the mouse button to read input on
		[Tooltip("입력을 읽을 마우스 버튼의 인덱스")]
		public int MouseButtonIndex = 1;
        
		[Header("OnClick")] 
		/// a feedback to play at the position of the click
		[Tooltip("클릭 위치에서 재생하기 위한 피드백")]
		public MMFeedbacks OnClickFeedbacks;

		/// if this is true, a click or tap on a UI element will block the click and won't cause the character to move
		[Tooltip("이것이 사실이라면 UI 요소를 클릭하거나 탭하면 클릭이 차단되고 캐릭터가 움직이지 않습니다.")]
		public bool UIShouldBlockInput = true;
        
		public GameObject Destination { get; set; }

		protected CharacterPathfinder3D _characterPathfinder3D;
		protected Plane _playerPlane;
		protected bool _destinationSet = false;
		protected Camera _mainCamera;

		/// <summary>
		/// On awake we create a plane to catch our ray
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_mainCamera = Camera.main;
			_characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
			_character.FindAbility<CharacterMovement>().ScriptDrivenInput = true;
            
			OnClickFeedbacks?.Initialization();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			if (Destination == null)
			{
				Destination = new GameObject();
				Destination.name = this.name + "PathfindToMouseDestination";
			}
		}
        
		/// <summary>
		/// Every frame we make sure we shouldn't be exiting our run state
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (!AbilityAuthorized)
			{
				return;
			}
			DetectMouse();
		}

		/// <summary>
		/// If the mouse is clicked, we cast a ray and if that ray hits the plane we make it the pathfinding target
		/// </summary>
		protected virtual void DetectMouse()
		{
			bool testUI = false;

			if (UIShouldBlockInput)
			{
				testUI = MMGUI.PointOrTouchBlockedByUI();
			}
            
			if (Input.GetMouseButtonDown(MouseButtonIndex) && !testUI)
			{
				Ray ray = _mainCamera.ScreenPointToRay(InputManager.Instance.MousePosition);
				Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
				float distance;
				if (_playerPlane.Raycast(ray, out distance))
				{
					Vector3 target = ray.GetPoint(distance);
					Destination.transform.position = target;
					_destinationSet = true;
					_characterPathfinder3D.SetNewDestination(Destination.transform);
					OnClickFeedbacks?.PlayFeedbacks(Destination.transform.position);
				}
			}
		}
	}
}