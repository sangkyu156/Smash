using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// CharacterPathfinder3D 장착 캐릭터에 추가하는 클래스입니다.
    /// 화면의 아무 곳이나 클릭하면 새로운 목표가 결정되고 캐릭터는 그 길을 찾아갑니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/MouseDrivenPathfinderAI3D")]
	public class MouseDrivenPathfinderAI3D : TopDownMonoBehaviour 
	{
		[Header("Testing")]
		/// the camera we'll use to determine the destination from
		[Tooltip("목적지를 결정하는 데 사용할 카메라")]
		public Camera Cam;
		/// a gameobject used to show the destination
		[Tooltip("목적지를 표시하는 데 사용되는 게임오브젝트")]
		public GameObject Destination;

		protected CharacterPathfinder3D _characterPathfinder3D;
		protected Plane _playerPlane;
		protected bool _destinationSet = false;
		protected Camera _mainCamera;

		/// <summary>
		/// On awake we create a plane to catch our ray
		/// </summary>
		protected virtual void Awake()
		{
			_mainCamera = Camera.main;
			_characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
		}

		/// <summary>
		/// On Update we look for a mouse click
		/// </summary>
		protected virtual void Update()
		{
			DetectMouse();
		}

		/// <summary>
		/// If the mouse is clicked, we cast a ray and if that ray hits the plane we make it the pathfinding target
		/// </summary>
		protected virtual void DetectMouse()
		{
			if (Input.GetMouseButtonDown(0))
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
				}
			}
		}
	}
}