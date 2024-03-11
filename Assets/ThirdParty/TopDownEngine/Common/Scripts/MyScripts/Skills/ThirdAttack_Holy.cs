using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ThirdAttack_Holy : MonoBehaviour, IPoolObject
    {
        public string idName = "ThirdAttack_Holy";
        BoxCollider damageCollider;

        private void Awake()
        {
            damageCollider = GetComponent<BoxCollider>();
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
            Invoke("damageColliderOff", 0.4f);
            Invoke("OnTargetReached", 1f);
        }

        public void damageColliderOff()
        {
            damageCollider.enabled = false;
        }

        public void SetAbility()
        {
            damageCollider.enabled = true;

            this.transform.position = GameManager.Instance.skillPostion.transform.position;

            // 메인 카메라로부터 화면 중심으로 Ray를 쏘기 위해 사용될 변수 선언
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast를 수행하여 충돌 정보를 저장할 변수 선언
            RaycastHit hit;

            // Raycast를 수행하고, 충돌이 발생했는지 여부를 반환
            if (Physics.Raycast(ray, out hit, 300, 1 << LayerMask.NameToLayer("Ground")))
            {
                // 만약 Ray가 어떤 물체와 충돌했다면, 그 정보를 출력
                transform.LookAt(hit.point);
                Vector3 currentRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(-90, currentRotation.y, -90);
            }
            else
            {
                // 만약 Ray가 아무것도 충돌하지 않았다면, 아무 동작도 수행하지 않음
            }
        }
    }
}