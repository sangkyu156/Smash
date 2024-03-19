using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 효과를 인스턴스화하고 선택 시 소리를 재생하는 항목 선택기
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/InventoryPickableItem")]
	public class InventoryPickableItem : ItemPicker 
	{
		/// The effect to instantiate when the coin is hit
		[Tooltip("코인이 맞았을 때 인스턴스화되는 효과")]
		public GameObject Effect;
		/// The sound effect to play when the object gets picked
		[Tooltip("개체를 선택할 때 재생되는 음향 효과")]
		public AudioClip PickSfx;

		protected override void PickSuccess()
		{
			base.PickSuccess ();
			Effects ();
		}

		/// <summary>
		/// Triggers the various pick effects
		/// </summary>
		protected virtual void Effects()
		{
			if (!Application.isPlaying)
			{
				return;
			}				
			else
			{
				if (PickSfx!=null) 
				{	
					MMSoundManagerSoundPlayEvent.Trigger(PickSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}

				if (Effect != null)
				{
					// adds an instance of the effect at the coin's position
					Instantiate(Effect, transform.position, transform.rotation);				
				}	
			}
		}
	}
}