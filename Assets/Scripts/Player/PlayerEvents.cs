using System;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public static event Action OnLanded;

    public static void Landed() => OnLanded?.Invoke();
}
