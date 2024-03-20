using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 일반적으로 손상을 나타내기 위해 부동 텍스트의 생성을 요청하지만 반드시 그런 것은 아닙니다.
    /// 이를 위해서는 장면에서 MMFloatingTextSpawner가 올바르게 설정되어야 합니다. 그렇지 않으면 아무 일도 일어나지 않습니다.
    /// 이렇게 하려면 새 빈 개체를 만들고 여기에 MMFloatingTextSpawner를 추가합니다. (적어도) 하나의 MMFloatingText 프리팹을 PooledSimpleMMFloatingText 슬롯으로 드래그하세요.
    /// MMTools/Tools/MMFloatingText/Prefabs 폴더에 이미 만들어진 이러한 프리팹이 있지만 자유롭게 직접 만들어도 됩니다.
    /// 해당 피드백을 사용하면 항상 동일한 텍스트가 생성됩니다. 이것이 원하는 것일 수도 있지만 Corgi 엔진 또는 TopDown 엔진을 사용하는 경우 전용 버전을 찾을 수 있습니다.
    /// Health 구성 요소에 직접 연결되어 받은 피해를 표시할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 일반적으로 손상을 나타내기 위해 부동 텍스트의 생성을 요청하지만 반드시 그런 것은 아닙니다. " +
"이를 위해서는 MMFloatingTextSpawner가 장면에서 올바르게 설정되어야 합니다. 그렇지 않으면 아무 일도 일어나지 않습니다." +
"그렇게 하려면 새 빈 개체를 만들고 여기에 MMFloatingTextSpawner를 추가하세요. (적어도) 하나의 MMFloatingText 프리팹을 PooledSimpleMMFloatingText 슬롯으로 드래그하세요." +
"MMTools/Tools/MMFloatingText/Prefabs 폴더에서 이미 만들어진 이러한 프리팹을 찾을 수 있지만 자유롭게 직접 만들어도 됩니다." +
"해당 피드백을 사용하면 항상 동일한 텍스트가 생성됩니다. 이것이 원하는 것일 수도 있지만 Corgi 엔진 또는 TopDown 엔진을 사용하는 경우 전용 버전을 찾을 수 있습니다. " +
"Health 구성 요소에 직접 연결되어 받은 피해를 표시할 수 있습니다.")]
	[FeedbackPath("UI/Floating Text")]
	public class MMFeedbackFloatingText : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		/// the possible places where the floating text should spawn at
		public enum PositionModes { TargetTransform, FeedbackPosition, PlayPosition }

		[Header("Floating Text")]
		/// the channel to require a floating text spawn on. This has to match the Channel value in the Pooler settings of your chosen MMFloatingTextSpawner
		[Tooltip("플로팅 텍스트 생성이 필요한 채널입니다. 이는 선택한 MMFloatingTextSpawner의 일반 설정에 있는 채널 값과 일치해야 합니다.")]
		public int Channel = 0;
		/// the Intensity to spawn this text with, will act as a lifetime/movement/scale multiplier based on the spawner's settings
		[Tooltip("이 텍스트를 생성하는 강도는 생성기의 설정에 따라 수명/이동/배율 승수 역할을 합니다.")]
		public float Intensity = 1f;
		/// the value to display when spawning this text
		[Tooltip("이 텍스트를 생성할 때 표시할 값")]
		public string Value = "100";
		/// if this is true, the intensity passed to this feedback will be the value displayed
		[Tooltip("이것이 사실이라면 이 피드백에 전달된 강도가 표시되는 값이 됩니다.")]
		public bool UseIntensityAsValue = false;

		[Header("Color")]

		/// whether or not to force a color on the new text, if not, the default colors of the spawner will be used
		[Tooltip("새 텍스트에 색상을 강제할지 여부. 그렇지 않은 경우 생성기의 기본 색상이 사용됩니다.")]
		public bool ForceColor = false;
		/// the gradient to apply over the lifetime of the text
		[Tooltip("텍스트의 수명 동안 적용할 그라데이션")]
		[GradientUsage(true)]
		public Gradient AnimateColorGradient = new Gradient();

		[Header("Lifetime")]

		/// whether or not to force a lifetime on the new text, if not, the default colors of the spawner will be used
		[Tooltip("새 텍스트에 수명을 강제할지 여부, 그렇지 않은 경우 생성기의 기본 색상이 사용됩니다.")]
		public bool ForceLifetime = false;
		/// the forced lifetime for the spawned text
		[Tooltip("생성된 텍스트의 강제 수명")]
		[MMFCondition("ForceLifetime", true)]
		public float Lifetime = 0.5f;

		[Header("Position")]

		/// where to spawn the new text (at the position of the feedback, or on a specified Transform)
		[Tooltip("새 텍스트를 생성할 위치(피드백 위치 또는 지정된 변환에서)")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// in transform mode, the Transform on which to spawn the new floating text
		[Tooltip("변환 모드에서 새 부동 텍스트를 생성할 변환입니다.")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.TargetTransform)]
		public Transform TargetTransform;
		/// the direction to apply to the new floating text (leave it to 0 to let the Spawner decide based on its settings)
		[Tooltip("새 부동 텍스트에 적용할 방향(Spawner가 해당 설정에 따라 결정하도록 하려면 0으로 유지)")]
		public Vector3 Direction = Vector3.zero;

		protected Vector3 _playPosition;
		protected string _value;

		/// the duration of this feedback is a fixed value or the lifetime
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Lifetime); } set { Lifetime = value; } }

		/// <summary>
		/// On play we ask the spawner on the specified channel to spawn a new floating text
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					_playPosition = this.transform.position;
					break;
				case PositionModes.PlayPosition:
					_playPosition = position;
					break;
				case PositionModes.TargetTransform:
					_playPosition = TargetTransform.position;
					break;
			}
			_value = UseIntensityAsValue ? feedbacksIntensity.ToString() : Value;
			MMFloatingTextSpawnEvent.Trigger(ChannelData(Channel), _playPosition, _value, Direction, Intensity * intensityMultiplier, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient, Timing.TimescaleMode == TimescaleModes.Unscaled);
            
		}
	}
}