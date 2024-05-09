using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAround : MonoBehaviour
{
    public GameObject objectA; // A 오브젝트

    public float speed = 0.05f; // 이동 속도
    public float radius = 33f; // 원의 반지름

    private void Start()
    {
        speed = 0.03f;

        MoveAroundObjectA();
    }

    void Update()
    {
        if (objectA != null)
        {
            MoveAroundObjectA();
        }
    }

    //내가 원하는 시작위치 = Vector3(122.6f, 9.1f, 88.24f)

    void MoveAroundObjectA()
    {
        // A 오브젝트의 현재 위치
        Vector3 center = objectA.transform.position;
        Vector3 newPosition;

        // 원운동을 위한 새로운 위치 계산  new Vector3(42f, 0f, 31.8f)
        float angle = Time.time * speed;

        newPosition = new Vector3(Mathf.Sin(angle), 0, -Mathf.Cos(angle)) * radius + center;
        newPosition.y = 9.1f;

        // 새로운 위치로 이동
        transform.position = newPosition;
        transform.LookAt(objectA.transform);
    }
}
