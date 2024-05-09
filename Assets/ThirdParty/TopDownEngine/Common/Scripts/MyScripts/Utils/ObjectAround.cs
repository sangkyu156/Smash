using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAround : MonoBehaviour
{
    public GameObject objectA; // A ������Ʈ

    public float speed = 0.05f; // �̵� �ӵ�
    public float radius = 33f; // ���� ������

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

    //���� ���ϴ� ������ġ = Vector3(122.6f, 9.1f, 88.24f)

    void MoveAroundObjectA()
    {
        // A ������Ʈ�� ���� ��ġ
        Vector3 center = objectA.transform.position;
        Vector3 newPosition;

        // ����� ���� ���ο� ��ġ ���  new Vector3(42f, 0f, 31.8f)
        float angle = Time.time * speed;

        newPosition = new Vector3(Mathf.Sin(angle), 0, -Mathf.Cos(angle)) * radius + center;
        newPosition.y = 9.1f;

        // ���ο� ��ġ�� �̵�
        transform.position = newPosition;
        transform.LookAt(objectA.transform);
    }
}
