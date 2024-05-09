using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class Slime : EnemyBase
    {
        public string idName = "Slime";
        CharacterController controller;
        TopDownController3D topDown;
        AIBrain brain;
        Character character;
        CharacterHandleWeapon characterHandleWeapon;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            topDown = GetComponent<TopDownController3D>();
            brain = GetComponent<AIBrain>();
            character = GetComponent<Character>();
            characterHandleWeapon = GetComponent<CharacterHandleWeapon>();
        }

        //처음 생성될때 호출 되는 함수
        public override void OnCreatedInPool()
        {
            base.OnCreatedInPool();
        }

        //재활용 될때마다 호출 되는 함수
        public override void OnGettingFromPool()
        {
            base.OnGettingFromPool();
            EnemyReset();
        }

        private void EnemyReset()
        {
            controller.enabled = true;
            topDown.enabled = true;
            brain.enabled = true;
            character.ConditionState.CurrentState = CharacterStates.CharacterConditions.Normal;
            characterHandleWeapon.Setup();
        }
    }
}