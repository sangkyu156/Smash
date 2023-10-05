using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

namespace MoreMountains.TopDownEngine
{
    public class CreateManager : MonoBehaviour
    {
        PoolManager poolManager; //������Ʈ Ǯ�� �Ŵ���
        GameObject level; //Battlefield �� ��

        static CreateManager c_instance; // ���ϼ��� ����ȴ�       
        static public CreateManager Instance { get { /*Init();*/ return c_instance; } } // ������ �Ŵ����� ����´�

        float createTime = 0;

        private void Awake()
        {
            c_instance = this.GetComponent<CreateManager>();
            poolManager = GetComponent<PoolManager>();
        }

        void Start()
        {
            level = GameObject.FindWithTag("Level");

            for (int i = 0;i < 2000;i++)
            {
                BarricadeRock_1();
            }

            if (GameManager.Instance.scenes == Define.Scenes.Battlefield)
            {
                level = GameObject.FindWithTag("Level");

                BarricadeRockCreation();

                
            }
        }

        private void Update()
        {
            createTime += Time.deltaTime;

            if (createTime > 1.5f)
            {
                createTime = 0;
                if (GameManager.Instance.stage == Define.Stage.Stage01)
                    SlimeSpawn();
            }
        }

        //�ٸ�����Ʈ ������ ������ 0~4�� �������� ����
        void BarricadeRockCreation()
        {
            BarricadeRock_1();

            //�� ������ �׺�Ž� ����
        }

        void BarricadeRock_1()
        {
            //1�� ������ ���� ����(x���� -7,-8, ... , -31, -32 ���� ���ϰ� �׿����� z�൵ �������ش�.)
            //1.x���� - 32�϶� z���� -32~32 ����
            //2.x���� - 31�ϋ� z���� -31~31 ����
            //...
            //������.���� - 7�϶� z���� -7~7 ����
            int posX = Random.Range(-7, -33);
            int posZ = Random.Range(posX, -posX);

            GameObject barricadeRock_1 = Instantiate("Battlefield/BF_BarricadeRock1", level.transform);
            barricadeRock_1.transform.position = new Vector3(posX, 0, posZ);
        }

        static public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        static public GameObject Instantiate(string path, Transform parent = null)
        {
            GameObject original = Load<GameObject>($"Prefabs/{path}");
            if (original == null)
            {
                Debug.Log($"Failed to load prefab : {path}");
                return null;
            }

            GameObject go = Object.Instantiate(original, parent);
            go.name = original.name;
            return go;
        }

        //������Ʈ ����
        public void ThirdAttackSpawn()
        {
            ThirdAttack thirdAttack = poolManager.GetFromPool<ThirdAttack>();
        }
        public void SlimeSpawn()
        {
            Slime slime = poolManager.GetFromPool<Slime>();
        }

        //������Ʈ ȸ��
        public void ReturnPool(ThirdAttack clone)
        {
            poolManager.TakeToPool<ThirdAttack>(clone.idName, clone);
        }
        public void ReturnPool(Slime clone)
        {
            poolManager.TakeToPool<Slime>(clone.idName, clone);
        }
    }
}
