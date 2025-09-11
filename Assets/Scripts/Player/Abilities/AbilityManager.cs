using UnityEngine;
using System.Collections.Generic;


public class AbilityManager : MonoBehaviour
{
    private List<PlayerAbility> abilities = new List<PlayerAbility>();

    public void UnlockAbility(PlayerAbility ability)
    {
        if (!abilities.Contains(ability))
            abilities.Add(ability);
    }

    public bool HasAbility<T>() where T : PlayerAbility
    {
        return abilities.Exists(a => a is T);
    }

    public void UseAbility<T>() where T : PlayerAbility
    {
        PlayerAbility ability = abilities.Find(a => a is T);
        ability?.Activate();
    }
}
