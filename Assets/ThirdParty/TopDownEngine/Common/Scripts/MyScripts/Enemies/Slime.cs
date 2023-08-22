using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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
        Vector3 ranPostion = Vector3.zero;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            topDown = GetComponent<TopDownController3D>();
            brain = GetComponent<AIBrain>();
            character = GetComponent<Character>();
        }

        void Start()
        {

        }

        void Update()
        {

        }

        //오브젝트 비활성화
        public void OnTargetReached()
        {
            if (gameObject.activeSelf)
            {
                CreateManager.Instance.ReturnPool(this);
            }
        }

        public override void OnCreatedInPool()
        {
            base.OnCreatedInPool();

        }

        public override void OnGettingFromPool()
        {
            base.OnGettingFromPool();
            EnemyReset();
        }

        private void EnemyReset()
        {
            //램덤으로 위치 지정
            transform.position = RandomPostion();

            controller.enabled = true;
            topDown.enabled = true;
            brain.enabled = true;
            character.ConditionState.CurrentState = CharacterStates.CharacterConditions.Normal;
        }

        Vector3 RandomPostion()
        {
            int num = Random.Range(1, 9);

            switch (num)
            {
                case 1:
                    ranPostion = new Vector3(-8, 0, 8); break;
                case 2:
                    ranPostion = new Vector3(0, 0, 8); break;
                case 3:
                    ranPostion = new Vector3(8, 0, 8); break;
                case 4:
                    ranPostion = new Vector3(-8, 0, 0); break;
                case 5:
                    ranPostion = new Vector3(8, 0, 0); break;
                case 6:
                    ranPostion = new Vector3(-8, 0, -8); break;
                case 7:
                    ranPostion = new Vector3(0, 0, -8); break;
                case 8:
                    ranPostion = new Vector3(8, 0, -8); break;
            }

            return ranPostion;
        }
    }
}