using Codice.CM.Common;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceRock : MonoBehaviour
{
    //private float timer = 0f;
    //float originalSpeed;
    //float changedSpeed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            other.gameObject.GetComponent<CharacterMovement>().MovementSpeed = other.gameObject.GetComponent<CharacterMovement>().WalkSpeed * 0.5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            other.gameObject.GetComponent<CharacterMovement>().MovementSpeed = other.gameObject.GetComponent<CharacterMovement>().WalkSpeed;
        }
    }
}
