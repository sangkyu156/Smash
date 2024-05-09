using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCutScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MasterAudio.StartPlaylist("Cut");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
