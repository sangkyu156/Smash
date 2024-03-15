using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffRandom : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            int ranMum = Random.Range(1, 6);

            switch (ranMum)
            {
                case 1: Stamina(other); break;
                case 2: Health(other); break;
                case 3: Heart(other); break;
                case 4: Invincibillty(other); break;
                case 5: Speedup(other); break;
            }
        }
    }

    //���׹̳� ȸ�� ����
    void Stamina(Collider other)
    {
        other.GetComponent<CharacterRun>().StaminaReset();
        other.GetComponent<PlayerEffectsController>().StaminaPlay();

        gameObject.SetActive(false);
    }

    //ü�� 30%ȸ�� ����
    void Health(Collider other)
    {
        //ĳ������ �ִ� ü�¿��� 30% ���ϱ�
        float healHealth = GetMaxHealthPercentage(other.GetComponent<Health>().MaximumHealth);

        other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);
        other.GetComponent<PlayerEffectsController>().Heal_1_Play();

        gameObject.SetActive(false);
    }

    //ĳ������ �ִ� ü�¿��� 30% ���ϱ�
    public float GetMaxHealthPercentage(float maxHealth)
    {
        return maxHealth * 0.3f;
    }

    //�ִ� ü�� +10
    void Heart(Collider other)
    {
        float plusHealth = 10;

        other.GetComponent<Health>().UpdateMaxHealth(plusHealth);
        other.GetComponent<PlayerEffectsController>().HeartPlay();

        gameObject.SetActive(false);
    }

    //5�ʵ��� ���� ����
    void Invincibillty(Collider other)
    {
        float invincibilityTime = 5;

        other.GetComponent<Health>().DamageDisabled(invincibilityTime);
        CreateManager.Instance.BuffInvincibilityEffectStart(invincibilityTime);
        other.GetComponent<PlayerEffectsController>().invincibilityPlay();

        gameObject.SetActive(false);
    }

    //5�ʵ��� �ӵ� 2�� ���� ����
    void Speedup(Collider other)
    {
        float speedIncreaseTime = 5;//������ �ӵ��� ��ŭ �����Ǿ���ϴ���
        float increaseSpeed = 2;//��ŭ �ӵ��� �����ؾ��ϴ���

        other.GetComponent<CharacterMovement>().ApplyMovementMultiplier(increaseSpeed, speedIncreaseTime);
        CreateManager.Instance.BuffSpeedupEffectStart(speedIncreaseTime);
        other.GetComponent<PlayerEffectsController>().SpeedupPlay();

        gameObject.SetActive(false);
    }
}
