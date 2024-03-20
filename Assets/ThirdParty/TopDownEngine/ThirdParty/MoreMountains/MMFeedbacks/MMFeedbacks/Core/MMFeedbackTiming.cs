using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// the possible modes for the timescale
	public enum TimescaleModes { Scaled, Unscaled }

    /// <summary>
    /// 각 MMFeedback의 동작을 정의하는 데 사용되는 지연, 재사용 대기시간 및 반복 값을 수집하는 클래스입니다.
    /// </summary>
    [System.Serializable]
	public class MMFeedbackTiming
	{
		/// the possible ways this feedback can play based on the host MMFeedbacks' directions
		public enum MMFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards };
		/// the possible ways this feedback can play
		public enum PlayDirections { FollowMMFeedbacksDirection, OppositeMMFeedbacksDirection, AlwaysNormal, AlwaysRewind }

		[Header("Timescale")]
		/// whether we're working on scaled or unscaled time
		[Tooltip("우리가 확장된 시간을 사용하든, 확장되지 않은 시간을 사용하든 상관없습니다.")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
		[Header("Exceptions")]
		/// if this is true, holding pauses won't wait for this feedback to finish 
		[Tooltip("이것이 사실이라면 일시 정지를 유지해도 피드백이 완료될 때까지 기다리지 않습니다.")]
		public bool ExcludeFromHoldingPauses = false;
		/// whether to count this feedback in the parent MMFeedbacks(Player) total duration or not
		[Tooltip("이 피드백을 상위 MMFeedbacks(Player) 총 지속 시간에 계산할지 여부")]
		public bool ContributeToTotalDuration = true;

		[Header("Delays")]
		/// the initial delay to apply before playing the delay (in seconds)
		[Tooltip("지연을 재생하기 전에 적용할 초기 지연(초)")]
		public float InitialDelay = 0f;
		/// the cooldown duration mandatory between two plays
		[Tooltip("두 플레이 사이에 필수 쿨다운 시간")]
		public float CooldownDuration = 0f;

		[Header("Stop")]
		/// if this is true, this feedback will interrupt itself when Stop is called on its parent MMFeedbacks, otherwise it'll keep running
		[Tooltip("이것이 사실이라면 이 피드백은 상위 MMFeedbacks에서 Stop이 호출될 때 자체적으로 중단되고, 그렇지 않으면 계속 실행됩니다.")]
		public bool InterruptsOnStop = true;

		[Header("Repeat")]
		/// the repeat mode, whether the feedback should be played once, multiple times, or forever
		[Tooltip("피드백을 한 번, 여러 번 또는 영원히 재생해야 하는지 여부를 나타내는 반복 모드")]
		public int NumberOfRepeats = 0;
		/// if this is true, the feedback will be repeated forever
		[Tooltip("이것이 사실이라면 피드백은 영원히 반복될 것입니다.")]
		public bool RepeatForever = false;
		/// the delay (in seconds) between two firings of this feedback. This doesn't include the duration of the feedback. 
		[Tooltip("이 피드백이 두 번 실행되는 사이의 지연(초)입니다. 여기에는 피드백 기간이 포함되지 않습니다.")]
		public float DelayBetweenRepeats = 1f;

		[Header("Play Direction")]
		/// this defines how this feedback should play when the host MMFeedbacks is played :
		/// - always (default) : this feedback will always play
		/// - OnlyWhenForwards : this feedback will only play if the host MMFeedbacks is played in the top to bottom direction (forwards)
		/// - OnlyWhenBackwards : this feedback will only play if the host MMFeedbacks is played in the bottom to top direction (backwards)
		[Tooltip("이는 호스트 MMFeedbacks가 재생될 때 이 피드백이 재생되는 방식을 정의합니다. :" +
                 "- always (default) : 이 피드백은 항상 재생됩니다" +
                 "- OnlyWhenForwards : 이 피드백은 호스트 MMFeedbacks가 위에서 아래 방향(앞으로)으로 재생되는 경우에만 재생됩니다." +
                 "- OnlyWhenBackwards : 이 피드백은 호스트 MMFeedbacks가 아래쪽에서 위쪽 방향(뒤쪽)으로 재생되는 경우에만 재생됩니다.")]
		public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;
		/// this defines the way this feedback will play. It can play in its normal direction, or in rewind (a sound will play backwards, 
		/// an object normally scaling up will scale down, a curve will be evaluated from right to left, etc)
		/// - BasedOnMMFeedbacksDirection : will play normally when the host MMFeedbacks is played forwards, in rewind when it's played backwards
		/// - OppositeMMFeedbacksDirection : will play in rewind when the host MMFeedbacks is played forwards, and normally when played backwards
		/// - Always Normal : will always play normally, regardless of the direction of the host MMFeedbacks
		/// - Always Rewind : will always play in rewind, regardless of the direction of the host MMFeedbacks
		[Tooltip("이는 피드백이 재생되는 방식을 정의합니다. 일반 방향으로 재생하거나 되감기하여 재생할 수 있습니다(사운드는 뒤로 재생됩니다." +
" 일반적으로 확장되는 개체는 축소되고, 곡선은 오른쪽에서 왼쪽으로 평가됩니다. 등)" +
                 "- BasedOnMMFeedbacksDirection : 호스트 MMFeedbacks가 앞으로 재생되면 정상적으로 재생되고, 뒤로 재생되면 되감기됩니다." +
                 "- OppositeMMFeedbacksDirection : 호스트 MMFeedbacks가 앞으로 재생될 때 되감기로 재생되고, 일반적으로 뒤로 재생될 때 재생됩니다." +
                 "- Always Normal : 호스트 MMFeedbacks의 방향에 관계없이 항상 정상적으로 재생됩니다." +
                 "- Always Rewind : 호스트 MMFeedbacks의 방향에 관계없이 항상 되감기 재생됩니다.")]
		public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;

		[Header("Intensity")]
		/// if this is true, intensity will be constant, even if the parent MMFeedbacks is played at a lower intensity
		[Tooltip("이것이 사실이면 상위 MMFeedbacks가 더 낮은 강도로 재생되더라도 강도는 일정합니다.")]
		public bool ConstantIntensity = false;
		/// if this is true, this feedback will only play if its intensity is higher or equal to IntensityIntervalMin and lower than IntensityIntervalMax
		[Tooltip("이것이 사실이라면 이 피드백은 강도가 IntensityIntervalMin보다 높거나 같고 IntensityIntervalMax보다 낮은 경우에만 재생됩니다.")]
		public bool UseIntensityInterval = false;
		/// the minimum intensity required for this feedback to play
		[Tooltip("이 피드백이 재생되는 데 필요한 최소 강도")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMin = 0f;
		/// the maximum intensity required for this feedback to play
		[Tooltip("이 피드백이 재생되는 데 필요한 최대 강도")]
		[MMFCondition("UseIntensityInterval", true)]
		public float IntensityIntervalMax = 0f;

		[Header("Sequence")]
		/// A MMSequence to use to play these feedbacks on
		[Tooltip("이러한 피드백을 재생하는 데 사용할 MMSequence")]
		public MMSequence Sequence;
		/// The MMSequence's TrackID to consider
		[Tooltip("고려해야 할 MMSequence의 TrackID")]
		public int TrackID = 0;
		/// whether or not to use the quantized version of the target sequence
		[Tooltip("대상 시퀀스의 양자화된 버전을 사용할지 여부")]
		public bool Quantized = false;
		/// if using the quantized version of the target sequence, the BPM to apply to the sequence when playing it
		[Tooltip("대상 시퀀스의 양자화된 버전을 사용하는 경우 해당 시퀀스를 재생할 때 시퀀스에 적용할 BPM")]
		[MMFCondition("Quantized", true)]
		public int TargetBPM = 120;
		
		/// from any class, you can set UseScriptDrivenTimescale:true, from there, instead of looking at Time.time, Time.deltaTime (or their unscaled equivalents), this feedback will compute time based on the values you feed them via ScriptDrivenDeltaTime and ScriptDrivenTime
		public bool UseScriptDrivenTimescale { get; set; }
		/// the value this feedback should use for delta time
		public float ScriptDrivenDeltaTime { get; set; }
		/// the value this feedback should use for time
		public float ScriptDrivenTime { get; set; }
	}
}