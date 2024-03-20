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
	[FeedbackPath("Animation/Animation Parameter")]
	public class MMF_Animation : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// the possible modes that pilot triggers        
		public enum TriggerModes { SetTrigger, ResetTrigger }
        
		/// the possible ways to set a value
		public enum ValueModes { None, Constant, Random, Incremental }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
		#endif
		
		/// the duration of this feedback is the declared duration 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("매개변수를 업데이트하려는 애니메이터")]
		public Animator BoundAnimator;
		/// the list of extra animators whose parameters you want to update
		[Tooltip("매개변수를 업데이트하려는 추가 애니메이터 목록")]
		public List<Animator> ExtraBoundAnimators;
		/// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("플레이어가 고려해야 할 기간. 이는 애니메이션에 영향을 미치지 않지만 이 피드백 기간을 MMF 플레이어와 통신하는 방법입니다. 일반적으로 실제 애니메이션과 일치하도록 설정하고 이 피드백이 일시 중지와 함께 작동하도록 설정하는 것이 유용할 수 있습니다.")]
		public float DeclaredDuration = 0f;
        
		[MMFInspectorGroup("Trigger", true, 16)]
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
        
		[MMFInspectorGroup("Random Trigger", true, 20)]
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
        
		[MMFInspectorGroup("Bool", true, 17)]
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
        
		[MMFInspectorGroup("Random Bool", true, 19)]
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
        
		[MMFInspectorGroup("Int", true, 24)]
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

		[MMFInspectorGroup("Float", true, 22)]
		/// the Float parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 Float 매개변수가 true로 전환됩니다.")]
		public ValueModes FloatValueMode = ValueModes.None;
		/// the float parameter to turn true when the feedback gets played
		[Tooltip("피드백이 재생되면 Float 매개변수가 true로 전환됩니다.")]
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
		protected override void CustomInitialization(MMF_Player owner)
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
				Debug.LogWarning("No animator was set for " + Owner.name);
				return;
			}

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			ApplyValue(BoundAnimator, intensityMultiplier);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				ApplyValue(animator, intensityMultiplier);
			}
		}

		/// <summary>
		/// Applies values on the target Animator
		/// </summary>
		/// <param name="targetAnimator"></param>
		/// <param name="intensityMultiplier"></param>
		protected virtual void ApplyValue(Animator targetAnimator, float intensityMultiplier)
		{
			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				targetAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				targetAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = targetAnimator.GetInteger(_intParameter) + IntIncrement;
					targetAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					targetAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = targetAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, randomValue);
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
			foreach (Animator animator in ExtraBoundAnimators)
			{
				animator.SetBool(_boolParameter, false);
			}
		}
	}
}