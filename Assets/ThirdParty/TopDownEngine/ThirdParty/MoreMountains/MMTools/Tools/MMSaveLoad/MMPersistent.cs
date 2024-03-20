using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 객체의 필수 부분을 저장할 수 있는 영속 클래스:
    /// 변환 데이터(위치, 회전, 크기 조정) 및 활성 상태
    /// 이는 MMPersistantBase를 상속하고 IMMPercious 인터페이스를 구현합니다.
    /// 인터페이스의 OnSave 및 OnLoad 메서드를 구현하는 방법에 대한 좋은 예입니다.
    /// </summary>
    public class MMPersistent : MMPersistentBase
	{
		
		[Header("Properties")]
		/// whether or not to save this object's position
		[Tooltip("이 객체의 위치를 ​​저장할지 여부")]
		public bool SavePosition = true;
		/// whether or not to save this object's rotation
		[Tooltip("이 객체의 회전을 저장할지 여부")]
		public bool SaveLocalRotation = true;
		/// whether or not to save this object's scale
		[Tooltip("이 개체의 크기를 저장할지 여부")]
		public bool SaveLocalScale = true;
		/// whether or not to save this object's active state
		[Tooltip("이 개체의 활성 상태를 저장할지 여부")]
		public bool SaveActiveState = true;
		
		/// <summary>
		/// A struct used to store and serialize the data we want to save
		/// </summary>
		[Serializable]
		public struct Data 
		{
			public Vector3 Position;
			public Quaternion LocalRotation;
			public Vector3 LocalScale;
			public bool ActiveState;
		}

		/// <summary>
		/// On Save, we turn the object's transform data and active state to a Json string and return it to the MMPersistencyManager
		/// </summary>
		/// <returns></returns>
		public override string OnSave()
		{
			return JsonUtility.ToJson(new Data { Position = this.transform.position, 
													LocalRotation = this.transform.localRotation, 
													LocalScale = this.transform.localScale, 
													ActiveState = this.gameObject.activeSelf });
		}

		/// <summary>
		/// On load, we read the saved json data and apply it to our object's properties
		/// </summary>
		/// <param name="data"></param>
		public override void OnLoad(string data)
		{
			if (SavePosition)
			{
				this.transform.position = JsonUtility.FromJson<Data>(data).Position;
			}

			if (SaveLocalRotation)
			{
				this.transform.localRotation = JsonUtility.FromJson<Data>(data).LocalRotation;	
			}

			if (SaveLocalScale)
			{
				this.transform.localScale = JsonUtility.FromJson<Data>(data).LocalScale;	
			}

			if (SaveActiveState)
			{
				this.gameObject.SetActive(JsonUtility.FromJson<Data>(data).ActiveState);	
			}
		}
	}	
}

