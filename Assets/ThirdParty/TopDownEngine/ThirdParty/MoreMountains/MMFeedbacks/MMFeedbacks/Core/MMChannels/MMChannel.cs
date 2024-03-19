using System;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// int 또는 MMChannel 스크립트 가능 객체를 통해 채널을 식별하는 데 사용되는 가능한 모드
    /// </summary>
    public enum MMChannelModes
	{
		Int,
		MMChannel
	}

    /// <summary>
    /// 채널 정보를 전달하는 데 사용되는 데이터 구조
    /// </summary>
    [Serializable]
	public class MMChannelData
	{
		public MMChannelModes MMChannelMode;
		public int Channel;
		public MMChannel MMChannelDefinition;

		public MMChannelData(MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			MMChannelMode = mode;
			Channel = channel;
			MMChannelDefinition = channelDefinition;
		}
	}

    /// <summary>
    /// MMChannelData의 확장 클래스
    /// </summary>
    public static class MMChannelDataExtensions
	{
		public static MMChannelData Set(this MMChannelData data, MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			data.MMChannelMode = mode;
			data.Channel = channel;
			data.MMChannelDefinition = channelDefinition;
			return data;
		}
	}

    /// <summary>
    /// 피드백 및 셰이커에서 주로(그뿐만 아니라) 사용되는 채널을 식별하기 위해 자산을 생성할 수 있는 스크립트 가능한 개체입니다.
    /// 일반적으로 송신기와 수신기 간의 통신 채널을 결정합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/MMChannel", fileName = "MMChannel")]
	public class MMChannel : ScriptableObject
	{
		public static bool Match(MMChannelData dataA, MMChannelData dataB)
		{
			if (dataA.MMChannelMode != dataB.MMChannelMode)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == dataB.Channel;
			}
			else
			{
				return dataA.MMChannelDefinition == dataB.MMChannelDefinition;
			}
		}
		public static bool Match(MMChannelData dataA, MMChannelModes modeB, int channelB, MMChannel channelDefinitionB)
		{
			if (dataA == null)
			{
				return true;
			}
			
			if (dataA.MMChannelMode != modeB)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == channelB;
			}
			else
			{
				return dataA.MMChannelDefinition == channelDefinitionB;
			}
		}
	}
}