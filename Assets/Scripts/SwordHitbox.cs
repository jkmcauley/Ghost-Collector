using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private PlayerCombat _combat;
    private bool _armed;
    private readonly HashSet<EnemyController> _alreadyHit = new();

    public void SetCombat(PlayerCombat combat)
    {
        _combat = combat;
    }

    public void Arm()
    {
        _alreadyHit.Clear();
        _armed = true;
    }

    public void Disarm()
    {
        _armed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_armed || _combat == null)
            return;

        if (!other.TryGetComponent(out EnemyController enemy))
            return;

        if (!_alreadyHit.Add(enemy))
            return;

        _combat.Hit(enemy);
    }
}
