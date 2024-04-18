using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutControler : MonoBehaviour
{
    public static CutControler Instance;

    public TimelineAsset[] ta;
    PlayableDirector pd;

    private void Awake()
    {
        Instance = this;
        pd = GetComponent<PlayableDirector>();
    }
    void Start()
    {
        pd.Play(ta[0]);
        //여기에 배경음 추가해야함
    }
}
