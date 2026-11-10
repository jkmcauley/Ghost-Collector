using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private EnemyAI _ai;
    private bool _armed;

    public void SetOwner(EnemyAI ai)
    {
        _ai = ai;
    }

    public void Arm()
    {
        _armed = true;
    }

    public void Disarm()
    {
        _armed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_armed || _ai == null)
            return;

        if (!other.TryGetComponent(out PlayerHealth player))
            return;

        _ai.Hit(player);
    }
}
