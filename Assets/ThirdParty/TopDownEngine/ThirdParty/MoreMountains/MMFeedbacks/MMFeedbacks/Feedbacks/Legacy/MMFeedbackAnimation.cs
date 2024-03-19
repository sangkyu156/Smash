using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 무작위 여부에 관계없이 관련 애니메이터에서 애니메이션(bool, int, float 또는 트리거)을 트리거하는 데 사용되는 피드백입니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 bool, int, float 또는 트리거 매개변수를 애니메이터(검사기에 바인딩된)에 보낼 수 있어 무작위 여부에 관계없이 애니메이션을 트리거할 수 있습니다.")]
	[FeedbackPath("GameObject/Animation")]
	public class MMFeedbackAnimation : MMFeedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// the possible modes that pilot triggers        
		public enum TriggerModes { SetTrigger, ResetTrigger }
        
		/// the possible ways to set a value
		public enum ValueModes { None, Constant, Random, Incremental }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("Animation")]
		/// the animator whose parameters you want to update
		[Tooltip("매개변수를 업데이트하려는 애니메이터")]
		public Animator BoundAnimator;
        
		[Header("Trigger")]
		/// if this is true, will update the specified trigger parameter
		[Tooltip("이것이 true이면 지정된 트리거 매개변수가 업데이트됩니다.")]
		public bool UpdateTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("이 트리거와 상호 작용하기 위해 선택한 모드")]
		[MMFCondition("UpdateTrigger", true)]
		public TriggerModes TriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameter to, well, trigger when the feedback is played
		[Tooltip("피드백이 재생될 때 트리거되는 트리거 애니메이터 매개변수")]
		[MMFCondition("UpdateTrigger", true)]
		public string TriggerParameterName;
        
		[Header("Random Trigger")]
		/// if this is true, will update a random trigger parameter, picked from the list below
		[Tooltip("이것이 사실이라면 아래 목록에서 선택한 임의의 트리거 매개변수를 업데이트합니다.")]
		public bool UpdateRandomTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("이 트리거와 상호 작용하기 위해 선택한 모드")]
		[MMFCondition("UpdateRandomTrigger", true)]
		public TriggerModes RandomTriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameters to trigger at random when the feedback is played
		[Tooltip("피드백이 재생될 때 무작위로 트리거할 트리거 애니메이터 매개변수")]
		public List<string> RandomTriggerParameterNames;
        
		[Header("Bool")]
		/// if this is true, will update the specified bool parameter
		[Tooltip("이것이 true이면 지정된 bool 매개변수를 업데이트합니다.")]
		public bool UpdateBool = false;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생될 때 true로 바뀌는 bool 매개변수")]
		[MMFCondition("UpdateBool", true)]
		public string BoolParameterName;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("bool 모드에서 bool 매개변수를 true 또는 false로 설정할지 여부")]
		[MMFCondition("UpdateBool", true)]
		public bool BoolParameterValue = true;
        
		[Header("Random Bool")]
		/// if this is true, will update a random bool parameter picked from the list below
		[Tooltip("이것이 사실이라면 아래 목록에서 선택된 임의의 bool 매개변수를 업데이트합니다.")]
		public bool UpdateRandomBool = false;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("bool 모드에서 bool 매개변수를 true 또는 false로 설정할지 여부")]
		[MMFCondition("UpdateRandomBool", true)]
		public bool RandomBoolParameterValue = true;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생될 때 true로 바뀌는 bool 매개변수")]
		public List<string> RandomBoolParameterNames;
        
		[Header("Int")]
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 true로 바뀌는 int 매개변수")]
		public ValueModes IntValueMode = ValueModes.None;
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 true로 바뀌는 int 매개변수")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string IntParameterName;
		/// the value to set to that int parameter
		[Tooltip("해당 int 매개변수에 설정할 값")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant)]
		public int IntValue;
		/// the min value (inclusive) to set at random to that int parameter
		[Tooltip("해당 int 매개변수에 무작위로 설정할 최소값(포함)")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMin;
		/// the max value (exclusive) to set at random to that int parameter
		[Tooltip("해당 int 매개변수에 무작위로 설정할 최대값(제외)")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMax = 5;
		/// the value to increment that int parameter by
		[Tooltip("해당 int 매개변수를 증가시킬 값")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Incremental)]
		public int IntIncrement = 1;

		[Header("Float")]
		/// the Float parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 true로 바뀌는 float 매개변수")]
		public ValueModes FloatValueMode = ValueModes.None;
		/// the float parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 true로 바뀌는 float 매개변수")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string FloatParameterName;
		/// the value to set to that float parameter
		[Tooltip("해당 float 매개변수에 설정할 값")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant)]
		public float FloatValue;
		/// the min value (inclusive) to set at random to that float parameter
		[Tooltip("해당 float 매개변수에 무작위로 설정할 최소값(포함)")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMin;
		/// the max value (exclusive) to set at random to that float parameter
		[Tooltip("해당 float 매개변수에 무작위로 설정할 최대값(제외)")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMax = 5;
		/// the value to increment that float parameter by
		[Tooltip("해당 float 매개변수를 증가시킬 값")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Incremental)]
		public float FloatIncrement = 1;

		protected int _triggerParameter;
		protected int _boolParameter;
		protected int _intParameter;
		protected int _floatParameter;
		protected List<int> _randomTriggerParameters;
		protected List<int> _randomBoolParameters;

		/// <summary>
		/// Custom Init
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			_triggerParameter = Animator.StringToHash(TriggerParameterName);
			_boolParameter = Animator.StringToHash(BoolParameterName);
			_intParameter = Animator.StringToHash(IntParameterName);
			_floatParameter = Animator.StringToHash(FloatParameterName);

			_randomTriggerParameters = new List<int>();
			foreach (string name in RandomTriggerParameterNames)
			{
				_randomTriggerParameters.Add(Animator.StringToHash(name));
			}

			_randomBoolParameters = new List<int>();
			foreach (string name in RandomBoolParameterNames)
			{
				_randomBoolParameters.Add(Animator.StringToHash(name));
			}
		}

		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("No animator was set for " + this.name);
				return;
			}

			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;

			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					BoundAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					BoundAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					BoundAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					BoundAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				BoundAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				BoundAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					BoundAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = BoundAnimator.GetInteger(_intParameter) + IntIncrement;
					BoundAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					BoundAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					BoundAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = BoundAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					BoundAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					BoundAnimator.SetFloat(_floatParameter, randomValue);
					break;
			}
            
		}
        
		/// <summary>
		/// On stop, turns the bool parameter to false
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !UpdateBool || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			BoundAnimator.SetBool(_boolParameter, false);
		}
	}
}