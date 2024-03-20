using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Unity는 여전히 기본적으로 사전을 직렬화할 수 없기 때문에 직렬화 가능한 사전 구현입니다.
    ///
    /// 사용하는 방법 :
    ///
    /// 직렬화하려는 각 사전 유형에 대해 MMSerializedDictionary에서 상속되는 직렬화 가능 클래스를 생성합니다.
    /// 다음과 같이 생성자와 SerializationInfo 생성자를 재정의합니다(여기서는 string/int 사전 사용).
    ///
    /// [Serializable]
    /// public class DictionaryStringInt : MMSerializableDictionary<string, int>
    /// {
    ///   public DictionaryStringInt() : base() { }
    ///   protected DictionaryStringInt(SerializationInfo info, StreamingContext context) : base(info, context) { }
    /// }
    ///  
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
	public class MMSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField] 
		protected List<TKey> _keys = new List<TKey>();
		[SerializeField] 
		protected List<TValue> _values = new List<TValue>();
		
		public MMSerializableDictionary() : base() { }
		public MMSerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
		
		/// <summary>
		/// We save the dictionary to our two lists
		/// </summary>
		public void OnBeforeSerialize()
		{
			_keys.Clear();
			_values.Clear();
			
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				_keys.Add(pair.Key);
				_values.Add(pair.Value);
			}
		}

		/// <summary>
		/// Loads our two lists to our dictionary
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void OnAfterDeserialize()
		{
			this.Clear();

			if (_keys.Count != _values.Count)
			{
				Debug.LogError("MMSerializableDictionary : there are " + _keys.Count + " keys and " + _values.Count + " values after deserialization. Counts need to match, make sure both key and value types are serializable.");
			}

			for (int i = 0; i < _keys.Count; i++)
			{
				this.Add(_keys[i], _values[i]);
			}
		}
	}
}



