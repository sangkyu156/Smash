using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 단계의 지속 시간으로 정의된 깜박임 단계와 비활성 및 활성 상태를 순차적으로 유지해야 하는 시간을 설명합니다.
    /// 단계 기간 동안 개체는 OffDuration 동안 꺼진 다음 OnDuration 동안 켜진 다음 OffDuration 동안 다시 꺼지는 식입니다.
    /// 수류탄이 0.2초마다 1초 동안 짧게 깜박이도록 하려면 다음 매개변수를 사용하세요.
    /// PhaseDuration = 1f;
    /// OffDuration = 0.2f;
    /// OnDuration = 0.1f;
    /// </summary>
    [Serializable]
	public class BlinkPhase
	{
		/// the duration of that specific phase, in seconds
		public float PhaseDuration = 1f;
		/// the time the object should remain off
		public float OffDuration = 0.2f;
		/// the time the object should then remain on
		public float OnDuration = 0.1f;
		/// the speed at which to lerp to off state
		public float OffLerpDuration = 0.05f;
		/// the speed at which to lerp to on state
		public float OnLerpDuration = 0.05f;
	}

    /// <summary>
    /// 게임 개체를 활성화/비활성화하거나 해당 알파, 방출 강도 또는 셰이더 값을 변경하여 깜박이게 하려면 이 클래스를 게임 개체에 추가하세요.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MMBlink")]
	public class MMBlink : MonoBehaviour
	{
		/// the possible states of the blinking object
		public enum States { On, Off }
		/// the possible methods to blink an object
		public enum Methods { SetGameObjectActive, MaterialAlpha, MaterialEmissionIntensity, ShaderFloatValue }
        
		[Header("Blink Method")]
		/// the selected method to blink the target object
		[Tooltip("대상 객체를 깜박이는 선택된 방법")]
		public Methods Method = Methods.SetGameObjectActive;
		/// the object to set active/inactive if that method was chosen
		[Tooltip("해당 메소드가 선택된 경우 활성/비활성으로 설정할 객체")]
		[MMFEnumCondition("Method", (int)Methods.SetGameObjectActive)]
		public GameObject TargetGameObject;
		/// the target renderer to work with
		[Tooltip("작업할 대상 렌더러")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public Renderer TargetRenderer;
		/// the shader property to alter a float on
		[Tooltip("플로트를 변경하는 셰이더 속성")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public string ShaderPropertyName = "_Color";
		/// the value to apply when blinking is off
		[Tooltip("깜박임이 꺼졌을 때 적용할 값")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public float OffValue = 0f;
		/// the value to apply when blinking is on
		[Tooltip("깜박임이 켜져 있을 때 적용할 값")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public float OnValue = 1f;
		/// whether to lerp these values or not
		[Tooltip("이 값을 lerp할지 여부")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public bool LerpValue = true;
		/// the curve to apply to the lerping
		[Tooltip("러핑에 적용할 곡선")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.05f), new Keyframe(1, 0));
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.")] 
		public bool UseMaterialPropertyBlocks = false;

		[Header("State")]
		/// whether the object should blink or not
		[Tooltip("객체가 깜박여야 하는지 여부")]
		public bool Blinking = true;
		/// whether or not to force a certain state on exit
		[Tooltip("종료 시 특정 상태를 강제로 적용할지 여부")]
		public bool ForceStateOnExit = false;
		/// the state to apply on exit
		[Tooltip("종료 시 적용할 상태")]
		[MMFCondition("ForceStateOnExit", true)]
		public States StateOnExit = States.On;

		[Header("Timescale")] 
		/// whether or not this MMBlink should operate on unscaled time 
		[Tooltip("이 MMBlink가 확장되지 않은 시간에 작동해야 하는지 여부")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
		[Header("Sequence")]
		/// how many times the sequence should repeat (-1 : infinite)
		[Tooltip("시퀀스가 몇 번 반복되어야 하는지(-1 : 무한)")]
		public int RepeatCount = 0;
		/// The list of phases to apply blinking with
		[Tooltip("적용할 단계 목록이 깜박입니다.")]
		public List<BlinkPhase> Phases;
        
		[Header("Debug")]
		/// Test button
		[Tooltip("Test button")]
		[MMFInspectorButton("ToggleBlinking")]
		public bool ToggleBlinkingButton;
		/// Test button
		[Tooltip("Test button")]
		[MMFInspectorButton("StartBlinking")]
		public bool StartBlinkingButton;
		/// Test button
		[Tooltip("Test button")]
		[MMFInspectorButton("StopBlinking")]
		public bool StopBlinkingButton;
		/// is the blinking object in an active state right now?
		[Tooltip("깜박이는 물체가 지금 활성 상태인가요?")]
		[MMFReadOnly]
		public bool Active = false;
		/// the index of the phase we're currently in
		[Tooltip("우리가 현재 있는 단계의 인덱스")]
		[MMFReadOnly]
		public int CurrentPhaseIndex = 0;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected float _lastBlinkAt = 0f;
		protected float _currentPhaseStartedAt = 0f;
		protected float _currentBlinkDuration;
		protected float _currentLerpDuration;
		protected int _propertyID;
		protected float _initialShaderFloatValue;
		protected Color _initialColor;
		protected Color _currentColor;
		protected int _repeatCount;
		protected MaterialPropertyBlock _propertyBlock;

		/// <summary>
		/// Makes the object blink if it wasn't already blinking, stops it otherwise
		/// </summary>
		public virtual void ToggleBlinking()
		{
			Blinking = !Blinking;
			ResetBlinkProperties();
		}

		/// <summary>
		/// Makes the object start blinking
		/// </summary>
		public virtual void StartBlinking()
		{
			this.enabled = true;
			Blinking = true;
			ResetBlinkProperties();
		}

		/// <summary>
		/// Makes the object stop blinking
		/// </summary>
		public virtual void StopBlinking()
		{
			Blinking = false;
			ResetBlinkProperties();
		}
                
		/// <summary>
		/// On Update, we blink if we are supposed to
		/// </summary>
		protected virtual void Update()
		{
			DetermineState();

			if (!Blinking)
			{
				return;
			}

			Blink();
		}

		/// <summary>
		/// Determines the current phase and determines whether the object should be active or inactive
		/// </summary>
		protected virtual void DetermineState()
		{
			DetermineCurrentPhase();
            
			if (!Blinking)
			{
				return;
			}

			if (Active)
			{
				if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OnDuration)
				{
					Active = false;
					_lastBlinkAt = GetTime();
				}
			}
			else
			{
				if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OffDuration)
				{
					Active = true;
					_lastBlinkAt = GetTime();
				}
			}
			_currentBlinkDuration = Active ? Phases[CurrentPhaseIndex].OnDuration : Phases[CurrentPhaseIndex].OffDuration;
			_currentLerpDuration = Active ? Phases[CurrentPhaseIndex].OnLerpDuration : Phases[CurrentPhaseIndex].OffLerpDuration;
		}

		/// <summary>
		/// Blinks the object based on its computed state
		/// </summary>
		protected virtual void Blink()
		{
			float currentValue = _currentColor.a;
			float initialValue = Active ? OffValue : OnValue;
			float targetValue = Active ? OnValue : OffValue;
			float newValue = targetValue;

			if (LerpValue && (GetTime() - _lastBlinkAt < _currentLerpDuration))
			{
				float t = MMFeedbacksHelpers.Remap(GetTime() - _lastBlinkAt, 0f, _currentLerpDuration, 0f, 1f);
				newValue = Curve.Evaluate(t);
				newValue = MMFeedbacksHelpers.Remap(newValue, 0f, 1f, initialValue, targetValue);
			}
			else
			{
				newValue = targetValue;
			}
            
			ApplyBlink(Active, newValue);
		}

		/// <summary>
		/// The duration of the blink is the sum of its phases' durations, plus the time it takes to repeat them all
		/// </summary>
		public virtual float Duration
		{
			get
			{
				if ((RepeatCount < 0)
				    || (Phases.Count == 0))
				{
					return 0f;
				}

				float totalDuration = 0f;
				foreach (BlinkPhase phase in Phases)
				{
					totalDuration += phase.PhaseDuration;
				}
				return totalDuration + totalDuration * RepeatCount;
			}
		}

		/// <summary>
		/// Applies the blink to the object based on its type
		/// </summary>
		/// <param name="active"></param>
		/// <param name="value"></param>
		protected virtual void ApplyBlink(bool active, float value)
		{
			switch (Method)
			{
				case Methods.SetGameObjectActive:
					TargetGameObject.SetActive(active);
					break;
				case Methods.MaterialAlpha:
					_currentColor.a = value;
					if (UseMaterialPropertyBlocks)
					{
						TargetRenderer.GetPropertyBlock(_propertyBlock);
						_propertyBlock.SetColor(_propertyID, _currentColor);
						TargetRenderer.SetPropertyBlock(_propertyBlock);
					}
					else
					{
						TargetRenderer.material.SetColor(_propertyID, _currentColor);    
					}
					break;
				case Methods.MaterialEmissionIntensity:
					_currentColor = _initialColor * value;
					if (UseMaterialPropertyBlocks)
					{
						TargetRenderer.GetPropertyBlock(_propertyBlock);
						_propertyBlock.SetColor(_propertyID, _currentColor);
						TargetRenderer.SetPropertyBlock(_propertyBlock);
					}
					else
					{
						TargetRenderer.material.SetColor(_propertyID, _currentColor);    
					}
					break;
				case Methods.ShaderFloatValue:
					if (UseMaterialPropertyBlocks)
					{
						TargetRenderer.GetPropertyBlock(_propertyBlock);
						_propertyBlock.SetFloat(_propertyID, value);
						TargetRenderer.SetPropertyBlock(_propertyBlock);
					}
					else
					{
						TargetRenderer.material.SetFloat(_propertyID, value); 
					}
					break;
			}
		}

		/// <summary>
		/// Determines the current phase index based on phase durations
		/// </summary>
		protected virtual void DetermineCurrentPhase()
		{
			// if the phase duration is null or less, we'll be in that phase forever, and return
			if (Phases[CurrentPhaseIndex].PhaseDuration <= 0)
			{
				return;
			}
			// if the phase's duration is elapsed, we move to the next phase
			if (GetTime() - _currentPhaseStartedAt > Phases[CurrentPhaseIndex].PhaseDuration)
			{
				CurrentPhaseIndex++;
				_currentPhaseStartedAt = GetTime();
			}
			if (CurrentPhaseIndex > Phases.Count -1)
			{
				CurrentPhaseIndex = 0;
				if (RepeatCount != -1)
				{
					_repeatCount--;
					if (_repeatCount < 0)
					{
						ResetBlinkProperties();

						if (ForceStateOnExit)
						{
							if (StateOnExit == States.Off)
							{
								ApplyBlink(false, 0f);
							}
							else
							{
								ApplyBlink(true, 1f);
							}
						}

						Blinking = false;
					}
				}                
			}
		}

        /// <summary>
        /// 활성화 시 깜박임 속성을 초기화합니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			InitializeBlinkProperties();            
		}

        /// <summary>
        /// 카운터를 재설정하고 속성과 초기 색상을 가져옵니다.
        /// </summary>
        protected virtual void InitializeBlinkProperties()
		{
			if (Phases.Count == 0)
			{
				Debug.LogError("MMBlink : You need to define at least one phase for this component to work.");
				this.enabled = false;
				return;
			}
            
			_currentPhaseStartedAt = GetTime();
			CurrentPhaseIndex = 0;
			_repeatCount = RepeatCount;
			_propertyBlock = new MaterialPropertyBlock();
            
			switch (Method)
			{
				case Methods.MaterialAlpha:
					TargetRenderer.GetPropertyBlock(_propertyBlock);
					_propertyID = Shader.PropertyToID(ShaderPropertyName);
					_initialColor = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetColor(_propertyID) : TargetRenderer.material.GetColor(_propertyID);
					_currentColor = _initialColor;
					break;
				case Methods.MaterialEmissionIntensity:
					TargetRenderer.GetPropertyBlock(_propertyBlock);
					_propertyID = Shader.PropertyToID(ShaderPropertyName);
					_initialColor = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetColor(_propertyID) : TargetRenderer.material.GetColor(_propertyID);
					_currentColor = _initialColor;
					break;
				case Methods.ShaderFloatValue:
					TargetRenderer.GetPropertyBlock(_propertyBlock);
					_propertyID = Shader.PropertyToID(ShaderPropertyName);
					_initialShaderFloatValue = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterial.GetFloat(_propertyID) : TargetRenderer.material.GetFloat(_propertyID);
					break;
			}
		}

		/// <summary>
		/// Resets blinking properties to original values
		/// </summary>
		protected virtual void ResetBlinkProperties()
		{
			_currentPhaseStartedAt = GetTime();
			CurrentPhaseIndex = 0;
			_repeatCount = RepeatCount;

			float value = 1f;
			if (Method == Methods.ShaderFloatValue)
			{
				value = _initialShaderFloatValue;
			}
            
			ApplyBlink(false, value);
		}

		protected void OnDisable()
		{
			if (ForceStateOnExit)
			{
				if (StateOnExit == States.Off)
				{
					ApplyBlink(false, 0f);
				}
				else
				{
					ApplyBlink(true, 1f);
				}
			}
		}
	}
}