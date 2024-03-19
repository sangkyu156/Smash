using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 사용하면 하나의 속성이 다른 속성의 가치를 매우 쉽게 유도하도록 할 수 있습니다.
    /// 그렇게 하려면 "읽고" 싶은 속성이 있는 객체를 이미터 속성 슬롯으로 드래그한 다음 속성이 있는 구성 요소를 선택하고 마지막으로 속성 자체를 선택합니다.
    /// 그런 다음 "쓰기"하려는 속성이 있는 개체를 ReceiverProperty 슬롯으로 드래그하고 이미터 값으로 구동하려는 속성을 선택합니다.
    /// </summary>
    public class MMEmmiterReceiver : MonoBehaviour
	{
		[MMInformation(
            "이 구성 요소를 사용하면 하나의 속성이 다른 속성의 가치를 매우 쉽게 유도하도록 할 수 있습니다. " +
"그렇게 하려면 '읽고 싶은' 속성이 있는 개체를 이미터 속성 슬롯으로 드래그한 다음 속성이 있는 구성 요소를 선택하고 마지막으로 속성 자체를 선택합니다." +
"그런 다음 '쓰기'하려는 속성이 있는 개체를 ReceiverProperty 슬롯으로 드래그하고 이미터 값으로 구동하려는 속성을 선택합니다.",
			MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		public bool Emitting = true;
		
		[Header("Emitter")]
		/// the property whose value you want to read and to have drive the ReceiverProperty's value
		[Tooltip("값을 읽고 ReceiverProperty의 값을 구동하려는 속성")]
		public MMPropertyEmitter EmitterProperty;
		
		[Header("Receiver")]
		/// the property whose value you want to be driven by the EmitterProperty's value
		[Tooltip("EmitterProperty의 값으로 구동하려는 값의 속성")]
		public MMPropertyReceiver ReceiverProperty;

		/// a delegate to handle value changes
		public delegate void OnValueChangeDelegate();
		/// what to do on value change
		public OnValueChangeDelegate OnValueChange;
		
		protected float _levelLastFrame;
		
		/// <summary>
		/// On Awake we initialize both properties
		/// </summary>
		protected virtual void Awake()
		{
			EmitterProperty.Initialization(EmitterProperty.TargetComponent.gameObject);
			ReceiverProperty.Initialization(ReceiverProperty.TargetComponent.gameObject);
		}
		
		/// <summary>
		/// On Update we emit our value to our receiver
		/// </summary>
		protected virtual void Update()
		{
			EmitValue();
		}

		/// <summary>
		/// If needed, reads the current level of the emitter and sets it to the receiver
		/// </summary>
		protected virtual void EmitValue()
		{
			if (!Emitting)
			{
				return;
			}
			
			float level = EmitterProperty.GetLevel();

			if (level != _levelLastFrame)
			{
				// we trigger a value change event
				OnValueChange?.Invoke();

				ReceiverProperty?.SetLevel(level);
			}           

			_levelLastFrame = level;
		}
	}	
}