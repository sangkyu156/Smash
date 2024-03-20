using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구조체를 사용하면 관찰 가능한 속성을 선언할 수 있습니다.
    /// 예를 들어 Character라는 클래스가 있고 속도를 다음과 같이 선언한다고 가정해 보겠습니다.
    ///
    /// public MMObservable<float> Speed;
    ///
    /// 그런 다음 다른 클래스에서 해당 속성(일반적으로 OnEnable)에 대한 OnValueChanged 이벤트에 등록할 수 있습니다.
    /// 
    /// protected virtual void OnEnable()
    /// {
    ///     _myCharacter.Speed.OnValueChanged += OnSpeedChange;
    /// }
    /// 
    /// 다음과 같이 구독을 취소하세요.
    /// 
    /// protected virtual void OnDisable()
    /// {
    ///     _myCharacter.Speed.OnValueChanged -= OnSpeedChange;
    /// }
    /// 
    /// 그런 다음 필요한 것은 해당 속도 변화를 처리하는 방법입니다.
    /// 
    /// protected virtual void OnSpeedChange()
    /// {
    ///     Debug.Log(_myCharacter.Speed.Value);
    /// }
    /// 
    /// 사용 방법에 대한 예를 보려면 MMObservableTest 데모 장면을 볼 수 있습니다.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct MMObservable<T>
	{
		public Action OnValueChanged;
		public Action<T> OnValueChangedTo;
		public Action<T,T> OnValueChangedFromTo;

		private T _value;
        
		public T Value
		{
			get { return _value;  }
			set
			{
				if (!EqualityComparer<T>.Default.Equals(value, _value))
				{
					var prev = _value;
					_value = value;
					OnValueChanged?.Invoke();
					OnValueChangedTo?.Invoke(_value);
					OnValueChangedFromTo?.Invoke(prev,_value);
				}
			}
		}
	}
}