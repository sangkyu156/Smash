using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 Health 구성 요소에 가해진 피해를 반영하는 부동 텍스트 모양을 트리거할 수 있습니다.
    /// 이를 위해서는 장면에서 MMFloatingTextSpawner가 올바르게 설정되어야 합니다. 그렇지 않으면 아무 일도 일어나지 않습니다.
    /// 이렇게 하려면 새 빈 개체를 만들고 여기에 MMFloatingTextSpawner를 추가합니다. (적어도) 하나의 MMFloatingText 프리팹을 PooledSimpleMMFloatingText 슬롯으로 드래그하세요.
    /// MMTools/Tools/MMFloatingText/Prefabs 폴더에 이미 만들어진 이러한 프리팹이 있지만 자유롭게 직접 만들어도 됩니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("UI/TopDown Engine Floating Text")]
	[FeedbackHelp("이 피드백을 사용하면 대상 건강 구성 요소에 가해진 피해를 반영하는 부동 텍스트 모양을 트리거할 수 있습니다." +
"이를 위해서는 MMFloatingTextSpawner가 장면에 올바르게 설정되어야 합니다. 그렇지 않으면 아무 일도 일어나지 않습니다." +
"그렇게 하려면 새 빈 개체를 만들고 여기에 MMFloatingTextSpawner를 추가하세요. (최소한) 하나의 MMFloatingText 프리팹을 PooledSimpleMMFloatingText 슬롯으로 드래그하세요." +
"MMTools/Tools/MMFloatingText/Prefabs 폴더에서 이미 만들어진 이러한 프리팹을 찾을 수 있지만 자유롭게 직접 만들어도 됩니다.")]
	public class MMF_TopDownEngineFloatingText : MMF_FloatingText
	{
		[MMFInspectorGroup("TopDown Engine Settings", true, 17)]

		/// the Health component where damage data should be read
		[Tooltip("손상 데이터를 읽어야 하는 상태 구성 요소")]
		public Health TargetHealth;
		/// the number formatting of your choice, 
		/// check https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings?redirectedfrom=MSDN#NFormatString for examples
		[Tooltip("원하는 숫자 형식을 입력하세요. 자유롭게 비워두세요.")]
		public string Formatting = "";

		[MMFInspectorGroup("Direction", true, 18)]
		/// whether or not the direction of the damage should impact the direction of the floating text 
		[Tooltip("손상 방향이 플로팅 텍스트 방향에 영향을 미치는지 여부")]
		public bool DamageDirectionImpactsTextDirection = true;
		/// the multiplier to apply to the damage direction. Usually you'll want it to be less than 1. With a value of 0.5, a character being hit from the left will spawn a floating text at a 45° up/right angle
		[Tooltip("피해 방향에 적용할 승수입니다. 일반적으로 1보다 작은 값을 원할 것입니다. 값이 0.5인 경우 왼쪽에서 캐릭터를 때리면 45° 위쪽/직각으로 떠 있는 텍스트가 생성됩니다.")]
		public float DamageDirectionMultiplier = 0.5f;

		/// <summary>
		/// On play, we ask for a floating text to be spawned
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (Active)
			{
				if (TargetHealth == null)
				{
					return;
				}

				if (DamageDirectionImpactsTextDirection)
				{
					Direction = TargetHealth.LastDamageDirection.normalized * DamageDirectionMultiplier;
				}

				Value = ApplyRounding(TargetHealth.LastDamage).ToString(Formatting);
				
				_playPosition = (PositionMode == PositionModes.FeedbackPosition) ? Owner.transform.position : TargetTransform.position;
				MMFloatingTextSpawnEvent.Trigger(ChannelData, _playPosition, Value, Direction, Intensity, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient);
			}
		}
	}
}