using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class EnemyBase : MonoBehaviour, IPoolObject
    {
        //ó������ �ѹ��� ȣ��� (��Ȱ��ȭ ������ ȣ��Ǵ� �Լ� OnGettingFromPool()�̰͵� ���� ȣ���)
        public virtual void OnCreatedInPool()
        {

        }

        //��Ȱ��ȭ (ó�� ���� ������ ȣ���)
        public virtual void OnGettingFromPool()
        {
            SetRandomPosition();
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public void SetRandomPosition()
        {
            Vector3 pos = Vector3.zero;

            switch (RandomValueBasedOnPlayerPosition())
            {
                case 1: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 2,   0, CreateManager.Instance.player.transform.position.z + 20); break;
                case 2: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 9,   0, CreateManager.Instance.player.transform.position.z + 18); break;
                case 3: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 15,  0, CreateManager.Instance.player.transform.position.z + 15); break;
                case 4: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 18,  0, CreateManager.Instance.player.transform.position.z + 9); break;
                case 5: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 20,  0, CreateManager.Instance.player.transform.position.z + 2); break;
                case 6: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 20,  0, CreateManager.Instance.player.transform.position.z - 2); break;
                case 7: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 18,  0, CreateManager.Instance.player.transform.position.z - 9); break;
                case 8: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 15,  0, CreateManager.Instance.player.transform.position.z - 15); break;
                case 9: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 9,   0, CreateManager.Instance.player.transform.position.z - 18); break;
                case 10: pos = new Vector3(CreateManager.Instance.player.transform.position.x + 2,  0, CreateManager.Instance.player.transform.position.z - 20); break;
                case 11: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 2,  0, CreateManager.Instance.player.transform.position.z - 20); break;
                case 12: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 9,  0, CreateManager.Instance.player.transform.position.z - 18); break;
                case 13: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 15, 0, CreateManager.Instance.player.transform.position.z - 15); break;
                case 14: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 18, 0, CreateManager.Instance.player.transform.position.z - 9); break;
                case 15: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 20, 0, CreateManager.Instance.player.transform.position.z - 2); break;
                case 16: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 20, 0, CreateManager.Instance.player.transform.position.z + 2); break;
                case 17: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 18, 0, CreateManager.Instance.player.transform.position.z + 9); break;
                case 18: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 15, 0, CreateManager.Instance.player.transform.position.z + 15); break;
                case 19: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 9,  0, CreateManager.Instance.player.transform.position.z + 18); break;
                case 20: pos = new Vector3(CreateManager.Instance.player.transform.position.x - 2,  0, CreateManager.Instance.player.transform.position.z + 20); break;
            }

            this.gameObject.transform.position = pos;
        }

        //�÷��̾� ��ġ�� ���� ������ ��ȯ �Լ�
        public int RandomValueBasedOnPlayerPosition()
        {
            //�÷��̾� x ���� -17���� �̰�, z���� -18�����̸� 1~5 �����ǿ��� ���;���.
            if (CreateManager.Instance.player.transform.position.x <= -17 && CreateManager.Instance.player.transform.position.z <= -18)
                return Random.Range(1, 6);
            //�÷��̾� x ���� -17���� �̰�, z���� 17�̻��̸� 6~10 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.x <= -17 && CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(6, 11);
            //�÷��̾� x ���� 18�̻� �̰�, z���� 17 �̻��̸� 11~15 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.x >= 18 && CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(11, 16);
            //�÷��̾� x ���� 18�̻� �̰�, z���� -17�����̸� 16~20 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.x >= 18 && CreateManager.Instance.player.transform.position.z <= -17)
                return Random.Range(16, 21);
            //�÷��̾� x ���� -17���� �̸� 1~10 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.x <= -17)
                return Random.Range(1, 11);
            //�÷��̾� z ���� 17�̻� �̸� 6~15 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(6, 16);
            //�÷��̾� x ���� 18�̻� �̸� 11~20 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.x >= 18)
                return Random.Range(11, 21);
            //�÷��̾� z ���� -18���� �̸� 1~5,16~20 �����ǿ��� ���;���.
            else if (CreateManager.Instance.player.transform.position.z <= -18)
            {
                int randomNumber;
                int range1to5 = Random.Range(1, 6);
                int range16to20 = Random.Range(16, 21);

                if (Random.Range(0, 2) == 0)
                    randomNumber = range1to5;
                else
                    randomNumber = range16to20;

                return randomNumber;
            }
            else
                return Random.Range(1, 21);
        }
    }
}