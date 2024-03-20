using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 빈 개체에 추가하고 몇 개의 프리팹을 RandomPool 슬롯에 바인딩하면 Start 또는 Awake의 위치/회전에서 그 중 하나를 인스턴스화합니다.
    /// 언제든지 InstantiateRandomObject 메서드를 호출할 수 있으며 요청 시 다른 임의 개체를 인스턴스화합니다.
    /// 결정하면 이전 항목을 잠재적으로 파괴할 수 있습니다.
    /// </summary>
    public class MMRandomInstantiator : MonoBehaviour
	{
		/// the possible start modes
		public enum StartModes { Awake, Start, None }

		[Header("Random instantiation")]
		/// whether this instantiator should auto trigger on Awake, Start, or never
		public StartModes StartMode = StartModes.Awake;
		/// the name to give to the instantiated object
		public string InstantiatedObjectName = "RandomInstantiated";
		/// if this is true, the instantiated object will be parented to the spawner
		public bool ParentInstantiatedToThisObject = true;
		/// if this is true, every time InstantiateRandomObject is called, any previously instantiated object will be destroyed
		public bool DestroyPreviouslyInstantiatedObject = true;
		/// the list containing all the objects that can potentially be instantiated
		public List<GameObject> RandomPool;

		[Header("Test")]
		/// a test button for your inspector
		[MMInspectorButton("InstantiateRandomObject")]
		public bool InstantiateButton;

		protected GameObject _instantiatedGameObject;

		/// <summary>
		/// On awake we instantiate if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (StartMode == StartModes.Awake)
			{
				InstantiateRandomObject();
			}
		}

		/// <summary>
		/// On Start we instantiate if needed
		/// </summary>
		protected virtual void Start()
		{
			if (StartMode == StartModes.Start)
			{
				InstantiateRandomObject();
			}
		}

		/// <summary>
		/// Spawns a random object from the pool of choices
		/// </summary>
		public virtual void InstantiateRandomObject()
		{
			// if the pool is empty we do nothing and exit
			if (RandomPool.Count == 0)
			{
				return;
			}

			// we destroy our previous object if needed
			if (DestroyPreviouslyInstantiatedObject)
			{
				if (_instantiatedGameObject != null)
				{
					DestroyImmediate(_instantiatedGameObject);
				}
			}

			// pick a random object and instantiates it
			int randomIndex = Random.Range(0, RandomPool.Count);
			_instantiatedGameObject = Instantiate(RandomPool[randomIndex], this.transform.position, this.transform.rotation);
			SceneManager.MoveGameObjectToScene(_instantiatedGameObject, this.gameObject.scene);
			_instantiatedGameObject.name = InstantiatedObjectName;
			if (ParentInstantiatedToThisObject)
			{
				_instantiatedGameObject.transform.SetParent(this.transform);
			}
		}
	}
}