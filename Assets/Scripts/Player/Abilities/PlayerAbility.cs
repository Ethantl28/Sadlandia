using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour
{
    public string abilityName;
    public bool isUnlocked = false;
    public float cooldown = 0.0f;
    protected float cooldownTimer = 0.0f;

    public virtual void Activate() { }
    public virtual void UpdateAbility()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    protected bool IsOnCooldown() => cooldownTimer > 0;
}
