using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class EnemyBase : MonoBehaviour, IPoolObject
    {
        //처음시작 한번만 호출됨 (재활성화 됐을떄 호출되는 함수 OnGettingFromPool()이것도 같이 호출됨)
        public virtual void OnCreatedInPool()
        {

        }

        //재활성화 (처음 생성 됐을도 호출됨)
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

        //플레이어 위치에 따른 랜덤값 반환 함수
        public int RandomValueBasedOnPlayerPosition()
        {
            //플레이어 x 값이 -17이하 이고, z값이 -18이하이면 1~5 포지션에서 나와야함.
            if (CreateManager.Instance.player.transform.position.x <= -17 && CreateManager.Instance.player.transform.position.z <= -18)
                return Random.Range(1, 6);
            //플레이어 x 값이 -17이하 이고, z값이 17이상이면 6~10 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.x <= -17 && CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(6, 11);
            //플레이어 x 값이 18이상 이고, z값이 17 이상이면 11~15 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.x >= 18 && CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(11, 16);
            //플레이어 x 값이 18이상 이고, z값이 -17이하이면 16~20 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.x >= 18 && CreateManager.Instance.player.transform.position.z <= -17)
                return Random.Range(16, 21);
            //플레이어 x 값이 -17이하 이면 1~10 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.x <= -17)
                return Random.Range(1, 11);
            //플레이어 z 값이 17이상 이면 6~15 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.z >= 17)
                return Random.Range(6, 16);
            //플레이어 x 값이 18이상 이면 11~20 포지션에서 나와야함.
            else if (CreateManager.Instance.player.transform.position.x >= 18)
                return Random.Range(11, 21);
            //플레이어 z 값이 -18이하 이면 1~5,16~20 포지션에서 나와야함.
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