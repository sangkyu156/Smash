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

    //스테미나 회복 버프
    void Stamina(Collider other)
    {
        other.GetComponent<CharacterRun>().StaminaReset();

        GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffStaminaEffect");
        effect.transform.position = other.transform.position;

        Destroy(this.gameObject);
    }

    //체력 30%회복 버프
    void Health(Collider other)
    {
        //캐릭터의 최대 체력에서 30% 구하기
        float healHealth = GetMaxHealthPercentage(other.GetComponent<Health>().MaximumHealth);

        other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);

        GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffHealthEffect");
        effect.transform.position = other.transform.position;

        Destroy(this.gameObject);
    }

    //캐릭터의 최대 체력에서 30% 구하기
    public float GetMaxHealthPercentage(float maxHealth)
    {
        return maxHealth * 0.3f;
    }

    //최대 체력 +10
    void Heart(Collider other)
    {
        float plusHealth = 10;

        other.GetComponent<Health>().UpdateMaxHealth(plusHealth);

        GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffHeartEffect");
        effect.transform.position = other.transform.position;

        Destroy(this.gameObject);
    }

    //5초동안 무적 버프
    void Invincibillty(Collider other)
    {
        float invincibilityTime = 5;

        other.GetComponent<Health>().DamageDisabled(invincibilityTime);
        CreateManager.Instance.BuffInvincibilityEffectStart(invincibilityTime);

        GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffInvincibilityEffect");
        effect.transform.position = other.transform.position;

        Destroy(this.gameObject);
    }

    //5초동안 속도 2배 증가 버프
    void Speedup(Collider other)
    {
        float speedIncreaseTime = 5;//증가한 속도가 얼만큼 유지되어야하는지
        float increaseSpeed = 2;//얼만큼 속도가 증가해야하는지

        other.GetComponent<CharacterMovement>().ApplyMovementMultiplier(increaseSpeed, speedIncreaseTime);
        CreateManager.Instance.BuffSpeedupEffectStart(speedIncreaseTime);

        GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffSpeedupEffect");
        effect.transform.position = other.transform.position;

        Destroy(this.gameObject);
    }
}
