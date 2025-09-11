using UnityEngine;

public class DoubleJump : PlayerAbility
{
    private int jumpCount = 1;
    private int maxJumps = 2;
    private Rigidbody rb;

    public bool HasUsedAllJumps() => jumpCount >= maxJumps;

    private void OnEnable()
    {
        PlayerEvents.OnLanded += ResetJumps;
    }

    private void OnDisable()
    {
        PlayerEvents.OnLanded -= ResetJumps;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        abilityName = "Double Jump";
        if (isUnlocked) GetComponent<AbilityManager>().UnlockAbility(this);
    }

    public override void Activate()
    {
        Debug.Log("Jump again");
        if (jumpCount < maxJumps)
        {
            GetComponent<playerController>().ForceJump();
            jumpCount++;
        }
    }

    private void ResetJumps()
    {
        jumpCount = 1;
        Debug.Log("Reset jumps");
    }
}
