using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 객체(보통 스프라이트)에 추가하면 항상 카메라를 향하게 됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Camera/MMBillboard")]
	public class MMBillboard : MonoBehaviour
	{
        /// 우리가 마주하고 있는 카메라
        public Camera MainCamera { get; set; }
        /// 이 객체가 시작 시 자동으로 카메라를 잡아야 하는지 여부
        [Tooltip("이 객체가 시작 시 자동으로 카메라를 잡아야 하는지 여부")]
		public bool GrabMainCameraOnStart = true;
        /// 이 객체를 상위 컨테이너 아래에 중첩할지 여부
        [Tooltip("이 객체를 상위 컨테이너 아래에 중첩할지 여부")]
		public bool NestObject = true;
        /// Vector3을 사용하여 방향을 바라보는 방향을 오프셋합니다.
        [Tooltip("Vector3을 사용하여 방향을 바라보는 방향을 오프셋합니다.")]
		public Vector3 OffsetDirection = Vector3.back;
        /// "월드 업"으로 간주할 Vector3
        [Tooltip("\"월드 업\"으로 간주할 Vector3")] 
		public Vector3 Up = Vector3.up;

		protected GameObject _parentContainer;
		private Transform _transform;

        /// <summary>
        /// 깨어 있을 때 필요한 경우 카메라를 잡고 개체를 중첩합니다.
        /// </summary>
        protected virtual void Awake()
		{
			_transform = transform;

			if (GrabMainCameraOnStart == true)
			{
				GrabMainCamera ();
			}
		}

		private void Start()
		{
			if (NestObject)
			{
				NestThisObject();
			}                
		}

        /// <summary>
        /// 이 개체를 상위 컨테이너 아래에 중첩합니다.
        /// </summary>
        protected virtual void NestThisObject()
		{
			_parentContainer = new GameObject();
			SceneManager.MoveGameObjectToScene(_parentContainer, this.gameObject.scene);
			_parentContainer.name = "Parent"+transform.gameObject.name;
			_parentContainer.transform.position = transform.position;
			transform.SetParent(_parentContainer.transform);
		}

        /// <summary>
        /// 메인 카메라를 잡습니다.
        /// </summary>
        protected virtual void GrabMainCamera()
		{
			MainCamera = Camera.main;
		}

        /// <summary>
        /// 업데이트 시 상위 컨테이너의 회전이 카메라를 향하도록 변경합니다.
        /// </summary>
        protected virtual void Update()
		{
			if (NestObject)
			{
				_parentContainer.transform.LookAt(_parentContainer.transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
			}                
			else
			{
				_transform.LookAt(_transform.position + MainCamera.transform.rotation * OffsetDirection, MainCamera.transform.rotation * Up);
			}
		}
	}
}