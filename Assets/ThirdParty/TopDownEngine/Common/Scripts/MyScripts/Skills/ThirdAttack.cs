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

        //������Ʈ ȸ��
        public void OnTargetReached()
        {
            if (this.gameObject.activeSelf)
                CreateManager.Instance.ReturnPool(this);
        }

        public void OnCreatedInPool()
        {
            //ó������ �Ҷ��� �����
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

            //this.transform.rotation = GameManager.Instance.GetMouseRotation();//���콺 Ŀ�� ����
        }
    }
}