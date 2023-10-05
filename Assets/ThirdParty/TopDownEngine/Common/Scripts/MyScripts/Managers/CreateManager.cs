using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

namespace MoreMountains.TopDownEngine
{
    public class CreateManager : MonoBehaviour
    {
        PoolManager poolManager; //오브젝트 풀링 매니져
        GameObject level; //Battlefield 씬 맵

        static CreateManager c_instance; // 유일성이 보장된다       
        static public CreateManager Instance { get { /*Init();*/ return c_instance; } } // 유일한 매니저를 갖고온다

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

        //바리게이트 돌멩이 프리펩 0~4개 랜덤으로 생성
        void BarricadeRockCreation()
        {
            BarricadeRock_1();

            //돌 생성후 네비매쉬 굽기
        }

        void BarricadeRock_1()
        {
            //1번 돌맹이 생성 범위(x축을 -7,-8, ... , -31, -32 으로 정하고 그에따라 z축도 결정해준다.)
            //1.x축이 - 32일때 z축은 -32~32 까지
            //2.x축이 - 31일떄 z축은 -31~31 까지
            //...
            //마지막.축이 - 7일때 z축은 -7~7 까지
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

        //오브젝트 생성
        public void ThirdAttackSpawn()
        {
            ThirdAttack thirdAttack = poolManager.GetFromPool<ThirdAttack>();
        }
        public void SlimeSpawn()
        {
            Slime slime = poolManager.GetFromPool<Slime>();
        }

        //오브젝트 회수
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
