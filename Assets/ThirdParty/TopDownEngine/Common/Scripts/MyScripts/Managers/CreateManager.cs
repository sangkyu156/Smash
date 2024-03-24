using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    public class CreateManager : MonoBehaviour
    {
        PoolManager poolManager; //오브젝트 풀링 매니져
        public GameObject level; //Battlefield 씬 맵(입장시 돌맹이 만다는데 필요)
        public GameObject bfManager; //BattlefieldManager
        public GameObject player;
        public GameObject clearSplash;

        static CreateManager c_instance; // 유일성이 보장된다       
        static public CreateManager Instance { get { return c_instance; } } // 유일한 매니저를 갖고온다

        float createTime = 0;
        float clearTime = 0;
        int curEnemyCount = 0;//현재 필드에 나와있는 에너미 숫자
        int enemyKillCount = 0;//에너미 죽일때 올라가는 숫자 (5마리 죽이면 다시 0으로됨)
        GameObject buff;

        private void Awake()
        {
            c_instance = this.GetComponent<CreateManager>();
            poolManager = GetComponent<PoolManager>();
        }

        void Start()
        {
            clearTime = 0;
            curEnemyCount = 0;
            enemyKillCount = 0;
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Battlefield01" || sceneName == "Battlefield02" || sceneName == "Battlefield03" || sceneName == "Battlefield04" || sceneName == "Battlefield05")
            {
                BarricadeRockCreation();
            }
        }

        private void Update()
        {
            createTime += Time.deltaTime;
            clearTime += Time.deltaTime;

            if (createTime > 0.1f && curEnemyCount <= 35)
            {
                createTime = 0;
                if (GameManager.Instance.stage == Define.Stage.Stage01)
                {
                    //SlimeSpawn();
                }
            }

            //에너미 5마리 잡을때마다 20%확률로 랜덤하게 버프 생성
            if (enemyKillCount % 5 == 0 && enemyKillCount != 0)
            {
                enemyKillCount = 0;
                RandomBuffCreation();
            }
        }

        //바리게이트 돌멩이 프리펩 0~4개 랜덤으로 생성
        void BarricadeRockCreation()
        {
            int randomNumber = Random.Range(0, 100);
            int randomNumber2 = Random.Range(0, 4);

            if (randomNumber >= 0 && randomNumber <= 44) //45%
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
            //돌 생성후 네비매쉬 굽기
            bfManager.GetComponent<BattlefieldManager>().NavMeshBake();
        }

        void BarricadeRock_1()
        {
            //1번 돌맹이 생성 범위(x축을 -7,-8, ... , -31, -32 으로 정하고 그에따라 z축도 결정해준다.)
            int posX = Random.Range(-7, -33);
            int posZ = Random.Range(posX, -posX);

            GameObject barricadeRock_1 = Instantiate("Battlefield/Obstacles/Obstacles_Rock1", level.transform);
            barricadeRock_1.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_2()
        {
            //2번 돌맹이 생성 범위(z축을 7,8, ... , 31, 32 으로 정하고 그에따라 x축도 결정해준다.)
            int posZ = Random.Range(7, 33);
            int posX = Random.Range((-posZ) + 2, posZ - 1);//2,4 번 돌맹이는 1,3번의 교집합 부분을 생성하지 않게한다.

            GameObject barricadeRock_2 = Instantiate("Battlefield/Obstacles/Obstacles_Rock1", level.transform);
            barricadeRock_2.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_3()
        {
            //3번 돌맹이 생성 범위(x축을 7,8, ... , 31, 32 으로 정하고 그에따라 z축도 결정해준다.)
            int posX = Random.Range(7, 33);
            int posZ = Random.Range(-posX, posX);

            GameObject barricadeRock_3 = Instantiate("Battlefield/Obstacles/Obstacles_Rock1", level.transform);
            barricadeRock_3.transform.position = new Vector3(posX, -1.2f, posZ);
        }

        void BarricadeRock_4()
        {
            //4번 돌맹이 생성 범위(z축을 -7,-8, ... , -31, -32 으로 정하고 그에따라 x축도 결정해준다.)
            int posZ = Random.Range(-7, -33);
            int posX = Random.Range(posZ + 3, (-posZ) - 2);//2,4 번 돌맹이는 1,3번의 교집합 부분을 생성하지 않게한다.

            GameObject barricadeRock_4 = Instantiate("Battlefield/Obstacles/Obstacles_Rock1", level.transform);
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

        public void SetRandomPosition(GameObject object_)
        {
            Vector3 pos = Vector3.zero;

            switch (RandomValueBasedOnPlayerPosition())
            {
                case 1: pos = new Vector3(player.transform.position.x + 2, 0, player.transform.position.z + 20); break;
                case 2: pos = new Vector3(player.transform.position.x + 9, 0, player.transform.position.z + 18); break;
                case 3: pos = new Vector3(player.transform.position.x + 15, 0, player.transform.position.z + 15); break;
                case 4: pos = new Vector3(player.transform.position.x + 18, 0, player.transform.position.z + 9); break;
                case 5: pos = new Vector3(player.transform.position.x + 20, 0, player.transform.position.z + 2); break;
                case 6: pos = new Vector3(player.transform.position.x + 20, 0, player.transform.position.z - 2); break;
                case 7: pos = new Vector3(player.transform.position.x + 18, 0, player.transform.position.z - 9); break;
                case 8: pos = new Vector3(player.transform.position.x + 15, 0, player.transform.position.z - 15); break;
                case 9: pos = new Vector3(player.transform.position.x + 9, 0, player.transform.position.z - 18); break;
                case 10: pos = new Vector3(player.transform.position.x + 2, 0, player.transform.position.z - 20); break;
                case 11: pos = new Vector3(player.transform.position.x - 2, 0, player.transform.position.z - 20); break;
                case 12: pos = new Vector3(player.transform.position.x - 9, 0, player.transform.position.z - 18); break;
                case 13: pos = new Vector3(player.transform.position.x - 15, 0, player.transform.position.z - 15); break;
                case 14: pos = new Vector3(player.transform.position.x - 18, 0, player.transform.position.z - 9); break;
                case 15: pos = new Vector3(player.transform.position.x - 20, 0, player.transform.position.z - 2); break;
                case 16: pos = new Vector3(player.transform.position.x - 20, 0, player.transform.position.z + 2); break;
                case 17: pos = new Vector3(player.transform.position.x - 18, 0, player.transform.position.z + 9); break;
                case 18: pos = new Vector3(player.transform.position.x - 15, 0, player.transform.position.z + 15); break;
                case 19: pos = new Vector3(player.transform.position.x - 9, 0, player.transform.position.z + 18); break;
                case 20: pos = new Vector3(player.transform.position.x - 2, 0, player.transform.position.z + 20); break;
            }

            object_.transform.position = pos;
        }

        //플레이어 위치에 따른 랜덤값 반환 함수
        public int RandomValueBasedOnPlayerPosition()
        {
            //플레이어 x 값이 -17이하 이고, z값이 -18이하이면 1~5 포지션에서 나와야함.
            if (player.transform.position.x <= -17 && player.transform.position.z <= -18)
                return Random.Range(1, 6);
            //플레이어 x 값이 -17이하 이고, z값이 17이상이면 6~10 포지션에서 나와야함.
            else if (player.transform.position.x <= -17 && player.transform.position.z >= 17)
                return Random.Range(6, 11);
            //플레이어 x 값이 18이상 이고, z값이 17 이상이면 11~15 포지션에서 나와야함.
            else if (player.transform.position.x >= 18 && player.transform.position.z >= 17)
                return Random.Range(11, 16);
            //플레이어 x 값이 18이상 이고, z값이 -17이하이면 16~20 포지션에서 나와야함.
            else if (player.transform.position.x >= 18 && player.transform.position.z <= -17)
                return Random.Range(16, 21);
            //플레이어 x 값이 -17이하 이면 1~10 포지션에서 나와야함.
            else if (player.transform.position.x <= -17)
                return Random.Range(1, 11);
            //플레이어 z 값이 17이상 이면 6~15 포지션에서 나와야함.
            else if (player.transform.position.z >= 17)
                return Random.Range(6, 16);
            //플레이어 x 값이 18이상 이면 11~20 포지션에서 나와야함.
            else if (player.transform.position.x >= 18)
                return Random.Range(11, 21);
            //플레이어 z 값이 -18이하 이면 1~5,16~20 포지션에서 나와야함.
            else if (player.transform.position.z <= -18)
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

        //클리어씬에 표시해야할 정보
        public void SetClearData()
        {
            clearSplash.GetComponent<ClearSplash>().ClearTimePrint(clearTime);
            clearSplash.GetComponent<ClearSplash>().RewardsPrint();
        }

        void RandomBuffCreation()
        {
            int randomValue = Random.Range(1, 11);

            if (randomValue <= 2)
            {
                int randomValue2 = Random.Range(1, 7);
                switch (randomValue2)
                {
                    case 1: buff = Instantiate("Battlefield/Buff/BuffInvincibility"); break;
                    case 2: buff = Instantiate("Battlefield/Buff/BuffInvincibility"); break;
                    case 3: buff = Instantiate("Battlefield/Buff/BuffInvincibility"); break;
                    case 4: buff = Instantiate("Battlefield/Buff/BuffInvincibility"); break;
                    case 5: buff = Instantiate("Battlefield/Buff/BuffSpeedup"); break;
                    case 6: buff = Instantiate("Battlefield/Buff/BuffInvincibility"); break;
                }

                SetRandomPosition(buff);
            }
        }

        /// <summary>
        /// 무적 버프먹으면 BuffInvincibility 스크립트에서 호출하는 함수
        /// </summary>
        /// <param name="time"></param>
        public void BuffInvincibilityEffectStart(float time)
        {
            GameObject effect = Instantiate("Battlefield/Buff/BuffInvincibilityPlayer",player.transform);

            StartCoroutine(ObjectDestructionOverTime(effect, time));
        }

        /// <summary>
        /// 이동속도 증가 버프를 먹으면 BuffSpeedup 스크립트에서 호출하는 함수
        /// </summary>
        /// <param name="time"></param>
        public void BuffSpeedupEffectStart(float time)
        {
            GameObject effect = Instantiate("Battlefield/Buff/PlayerMoveTrail", player.transform);

            StartCoroutine(ObjectDestructionOverTime(effect, time));
        }

        /// <summary>
        /// 매개변수로 받은 오브젝트, 시간에 따라 시간이 지나면 오브젝트 파괴하는 함수
        /// </summary>
        public IEnumerator ObjectDestructionOverTime(GameObject oj,float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(oj);
        }

        //↓오브젝트 생성 함수들

        //3번째 공격
        public void ThirdAttack_IceSpawn()
        {
            ThirdAttack_Ice thirdAttack = poolManager.GetFromPool<ThirdAttack_Ice>();
        }

        public void ThirdAttack_HolySpawn()
        {
            ThirdAttack_Holy thirdAttack = poolManager.GetFromPool<ThirdAttack_Holy>();
        }

        public void SlimeSpawn()
        {
            Slime slime = poolManager.GetFromPool<Slime>();
            curEnemyCount++;
        }

        //↓오브젝트 회수 함수들
        public void ReturnPool(ThirdAttack_Ice clone)
        {
            poolManager.TakeToPool<ThirdAttack_Ice>(clone.idName, clone);
        }

        public void ReturnPool(ThirdAttack_Holy clone)
        {
            poolManager.TakeToPool<ThirdAttack_Holy>(clone.idName, clone);
        }

        public void ReturnPool(Slime clone)
        {
            poolManager.TakeToPool<Slime>(clone.idName, clone);
            curEnemyCount--;
            enemyKillCount++;
        }
    }
}
