using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 무기를 제어하고, 무기를 다룰 캐릭터가 없어도 필요에 따라 무기를 시작하고 중지하는 데 사용할 수 있는 간단한 구성 요소입니다.
    /// KoalaHealth 데모 장면에서 실제로 작동하는 것을 볼 수 있습니다. 데모의 대포에 전력을 공급하고 있습니다.
    /// </summary>
    public class WeaponHandler : TopDownMonoBehaviour
    {
        [Header("Weapon")]
        /// the weapon you want this component to pilot
        [Tooltip("이 구성요소가 조종할 무기")]
        public Weapon TargetWeapon;

        [Header("Debug")] 
        [MMInspectorButton("StartShooting")]
        public bool StartShootingButton;
        [MMInspectorButton("StopShooting")]
        public bool StopShootingButton;

        /// <summary>
        /// Makes the associated weapon start shooting
        /// </summary>
        public virtual void StartShooting()
        {
            if (TargetWeapon == null)
            {
                return;
            }
            TargetWeapon.WeaponInputStart();
        }

        /// <summary>
        /// Makes the associated weapon stop shooting
        /// </summary>
        public virtual void StopShooting()
        {
            if (TargetWeapon == null)
            {
                return;
            }
            TargetWeapon.WeaponInputStop();
        }
    }
}

