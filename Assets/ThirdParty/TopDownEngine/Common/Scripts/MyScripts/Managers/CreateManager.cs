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
        GameObject bfManager; //BattlefieldManager

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
            //level = GameObject.FindWithTag("Level");
            //bfManager = GameObject.FindWithTag("BattlefieldManager");

            //BarricadeRockCreation();

            if (GameManager.Instance.scenes == Define.Scenes.Battlefield)
            {
                level = GameObject.FindWithTag("Level");
                bfManager = GameObject.FindWithTag("BattlefieldManager");

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

            if(Input.GetKeyDown(KeyCode.H))
            {
                BarricadeRockCreation();
            }
        }

        //�ٸ�����Ʈ ������ ������ 0~4�� �������� ����
        void BarricadeRockCreation()
        {
            int randomNumber = Random.Range(0, 100);
            int randomNumber2 = Random.Range(0, 4);
            Debug.Log($"randomNumber - {randomNumber}, randomNumber2 - {randomNumber2}");

            if (randomNumber >= 0 && randomNumber <=  44) //45%
            {
                
                switch (randomNumber2)
                {
                    case 0:
                        BarricadeRock_1(); break;
                    case 1:
                        BarricadeRock_2(); break;
                    case 2:
                        BarricadeRock_3(); break;
                    case 3:
                        BarricadeRock_4(); break;
                }
            }
            else if (randomNumber >= 45 && randomNumber <= 74) //30%
            {
                switch (randomNumber2)
                {
                    case 0:
                        BarricadeRock_1(); BarricadeRock_3(); break;
                    case 1:
                        BarricadeRock_2(); BarricadeRock_4(); break;
                    case 2:
                        BarricadeRock_2(); BarricadeRock_3(); break;
                    case 3:
                        BarricadeRock_1(); BarricadeRock_4(); break;
                }
            }
            else if (randomNumber >= 75 && randomNumber <= 89) //15%
            {
                switch (randomNumber2)
                {
                    case 0:
                        BarricadeRock_1(); BarricadeRock_2(); BarricadeRock_3(); break;
                    case 1:
                        BarricadeRock_2(); BarricadeRock_3(); BarricadeRock_4(); break;
                    case 2:
                        BarricadeRock_3(); BarricadeRock_4(); BarricadeRock_1(); break;
                    case 3:
                        BarricadeRock_4(); BarricadeRock_1(); BarricadeRock_2(); break;
                }
            }
            else if (randomNumber >= 90 && randomNumber <= 99) //10%
            {
                BarricadeRock_1();
                BarricadeRock_2();
                BarricadeRock_3();
                BarricadeRock_4();
            }
            //�� ������ �׺�Ž� ����
            bfManager.GetComponent<BattlefieldManager>().NavMeshBake();
        }

        void BarricadeRock_1()
        {
            //1�� ������ ���� ����(x���� -7,-8, ... , -31, -32 ���� ���ϰ� �׿����� z�൵ �������ش�.)
            int posX = Random.Range(-7, -33);
            int posZ = Random.Range(posX, -posX);

            GameObject barricadeRock_1 = Instantiate("Battlefield/BF_BarricadeRock1", level.transform);
            barricadeRock_1.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_2()
        {
            //2�� ������ ���� ����(z���� 7,8, ... , 31, 32 ���� ���ϰ� �׿����� x�൵ �������ش�.)
            int posZ = Random.Range(7, 33);
            int posX = Random.Range((-posZ)+2, posZ-1);//2,4 �� �����̴� 1,3���� ������ �κ��� �������� �ʰ��Ѵ�.

            GameObject barricadeRock_2 = Instantiate("Battlefield/BF_BarricadeRock1", level.transform);
            barricadeRock_2.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_3()
        {
            //3�� ������ ���� ����(x���� 7,8, ... , 31, 32 ���� ���ϰ� �׿����� z�൵ �������ش�.)
            int posX = Random.Range(7, 33);
            int posZ = Random.Range(-posX, posX);

            GameObject barricadeRock_3 = Instantiate("Battlefield/BF_BarricadeRock1", level.transform);
            barricadeRock_3.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_4()
        {
            //4�� ������ ���� ����(z���� -7,-8, ... , -31, -32 ���� ���ϰ� �׿����� x�൵ �������ش�.)
            int posZ = Random.Range(-7, -33);
            int posX = Random.Range(posZ+3, (-posZ)-2);//2,4 �� �����̴� 1,3���� ������ �κ��� �������� �ʰ��Ѵ�.

            GameObject barricadeRock_4 = Instantiate("Battlefield/BF_BarricadeRock1", level.transform);
            barricadeRock_4.transform.position = new Vector3(posX, -1.2f, posZ);
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
