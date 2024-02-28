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
            this.transform.position = GameManager.Instance.skillPostion.transform.position;

            // ���� ī�޶�κ��� ȭ�� �߽����� Ray�� ��� ���� ���� ���� ����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast�� �����Ͽ� �浹 ������ ������ ���� ����
            RaycastHit hit;

            // Raycast�� �����ϰ�, �浹�� �߻��ߴ��� ���θ� ��ȯ
            if (Physics.Raycast(ray, out hit, 300, 1 << LayerMask.NameToLayer("Ground")))
            {
                // ���� Ray�� � ��ü�� �浹�ߴٸ�, �� ������ ���
                transform.LookAt(hit.point);
                Vector3 currentRotation = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(-90, currentRotation.y, -90);
            }
            else
            {
                // ���� Ray�� �ƹ��͵� �浹���� �ʾҴٸ�, �ƹ� ���۵� �������� ����
            }
        }
    }
}