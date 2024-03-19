using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 캐릭터 주위에 시야의 원뿔을 캐스팅하는 능력입니다.
    /// </summary>
    [RequireComponent(typeof(MMConeOfVision))]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Cone of Vision")]
	public class CharacterConeOfVision : TopDownMonoBehaviour
	{
		protected MMConeOfVision _coneOfVision;
		protected CharacterOrientation3D _characterOrientation;

		/// <summary>
		/// On awake, we grab our components
		/// </summary>
		protected virtual void Awake()
		{
			_characterOrientation = this.gameObject.GetComponentInParent<CharacterOrientation3D>();
			_coneOfVision = this.gameObject.GetComponent<MMConeOfVision>();
		}

		/// <summary>
		/// On update, we update our cone of vision
		/// </summary>
		protected virtual void Update()
		{
			UpdateDirection();   
		}

		/// <summary>
		/// Sends the character orientation's angle to the cone of vision
		/// </summary>
		protected virtual void UpdateDirection()
		{
			if (_characterOrientation == null)
			{
				_coneOfVision.SetDirectionAndAngles(this.transform.forward, this.transform.eulerAngles);              
			}
			else
			{
				_coneOfVision.SetDirectionAndAngles(_characterOrientation.ModelDirection, _characterOrientation.ModelAngles);              
			}
		}
	}
}