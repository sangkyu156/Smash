using UnityEngine;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 지속적 겸손 싱글톤, 기본적으로 클래식 싱글톤이지만 깨어 있을 때 발견되는 동일한 유형의 다른 이전 구성 요소를 모두 파괴합니다.
    /// </summary>
    public class MMPersistentHumbleSingleton<T> : MonoBehaviour	where T : Component
	{
		/// whether or not this singleton already has an instance 
		public static bool HasInstance => _instance != null;
		public static T Current => _instance;
		
		protected static T _instance;
		
		/// the timestamp at which this singleton got initialized
		[MMReadOnly]
		public float InitializationTime;

		/// <summary>
		/// Singleton design pattern
		/// </summary>
		/// <value>The instance.</value>
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T> ();
					if (_instance == null)
					{
						GameObject obj = new GameObject ();
						obj.hideFlags = HideFlags.HideAndDontSave;
						obj.name = typeof(T).Name + "_AutoCreated";
						_instance = obj.AddComponent<T> ();
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
		/// </summary>
		protected virtual void Awake ()
		{
			InitializeSingleton();			
		}

		/// <summary>
		/// Initializes the singleton.
		/// </summary>
		protected virtual void InitializeSingleton()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			
			InitializationTime = Time.time;

			DontDestroyOnLoad (this.gameObject);
			// we check for existing objects of the same type
			T[] check = FindObjectsOfType<T>();
			foreach (T searched in check)
			{
				if (searched!=this)
				{
					// if we find another object of the same type (not this), and if it's older than our current object, we destroy it.
					if (searched.GetComponent<MMPersistentHumbleSingleton<T>>().InitializationTime < InitializationTime)
					{
						Destroy (searched.gameObject);
					}
				}
			}

			if (_instance == null)
			{
				_instance = this as T;
			}
		}
	}
}