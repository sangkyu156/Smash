using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealRock : MonoBehaviour
{
    private float timer = 0f;
    private float interval = 1f; // ����

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                //ȸ���Ǵ� ü�·�
                float healHealth = 5;

                other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);
                other.GetComponent<PlayerEffectsController>().Heal_2_Play();

                timer = 0f; // Ÿ�̸� �ʱ�ȭ
            }
        }
    }
}
