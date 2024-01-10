using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public int CurPlayerGold {  get; private set; }

    private void Start()
    {
        //파일 있는지 체크하고(해당 컴퓨터에서 처음 실행하는지 확인하고) 파일이 있으면 로드하고 없으면 초기값 넣어서 저장하고 불러온다
        if (ES3.FileExists())
        {
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }
        else
        {
            ES3.Save("PlayerGold", CurPlayerGold = 777);
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }
    }
}
