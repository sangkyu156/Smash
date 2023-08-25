using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ThirdAttack : MonoBehaviour, IPoolObject
    {
        public string idName = "ThirdAttack";

        private void Awake()
        {
            SetAbility();
        }

        void Start()
        {

        }

        private void Update()
        {

        }

        //오브제트 회수
        public void OnTargetReached()
        {
            if (this.gameObject.activeSelf)
                CreateManager.Instance.ReturnPool(this);
        }

        public void OnCreatedInPool()
        {
            //처음시작 할때만 실행됨
        }

        public void OnGettingFromPool()
        {
            SetAbility();
            Invoke("OnTargetReached", 1f);
        }

        public void SetAbility()
        {
            GameManager.Instance.SetSkillPostion();
            this.transform.position = GameManager.Instance.skillPostion.transform.position;

            //this.transform.rotation = GameManager.Instance.GetMouseRotation();//마우스 커서 방향
        }
    }
}