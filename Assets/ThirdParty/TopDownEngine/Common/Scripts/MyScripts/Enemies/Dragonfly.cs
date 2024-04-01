using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragonfly : MonoBehaviour
{
    MMF_Player feedbackPlayer;
    CharacterController characterController;

    private void Awake()
    {
        feedbackPlayer = GetComponent<MMF_Player>();
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        feedbackPlayer.PlayFeedbacks();
        characterController.enabled = true;
    }
}
