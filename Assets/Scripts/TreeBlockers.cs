using System.Collections.Generic;
using UnityEngine;

public class TreeBlockers : MonoBehaviour
{
    const float MinRadius = 0.11f;
    const float MaxRadius = 0.18f;

    static readonly List<TreeBlockers> Blockers = new();
    readonly List<Collider2D> _passable = new();

    public static void IgnoreFor(Collider2D player, bool ignore)
    {
        if (player == null)
            return;

        for (int i = 0; i < Blockers.Count; i++)
            Blockers[i].Apply(player, ignore);
    }

    private void Awake()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            string name = renderer.gameObject.name;
            if (renderer.sprite == null || renderer.GetComponent<Collider2D>() != null)
                continue;

            if (IsTree(name))
                AddTrunk(renderer);
            else if (name.StartsWith("lamp post"))
                AddPole(renderer);
        }
    }

    private void OnEnable()
    {
        if (!Blockers.Contains(this))
            Blockers.Add(this);
    }

    private void OnDisable()
    {
        Blockers.Remove(this);
    }

    private void Apply(Collider2D player, bool ignore)
    {
        for (int i = 0; i < _passable.Count; i++)
        {
            Collider2D obstacle = _passable[i];
            if (obstacle != null)
                Physics2D.IgnoreCollision(player, obstacle, ignore);
        }
    }

    void AddTrunk(SpriteRenderer renderer)
    {
        Bounds bounds = renderer.sprite.bounds;
        float radius = Mathf.Clamp(bounds.size.x * 0.08f, MinRadius, MaxRadius);
        CircleCollider2D trunk = renderer.gameObject.AddComponent<CircleCollider2D>();
        trunk.isTrigger = false;
        trunk.radius = radius;
        trunk.offset = new Vector2(bounds.center.x, bounds.min.y + radius);
        _passable.Add(trunk);
    }

    void AddPole(SpriteRenderer renderer)
    {
        Bounds bounds = renderer.sprite.bounds;
        float width = 0.22f;
        float height = bounds.size.y * 0.9f;
        BoxCollider2D pole = renderer.gameObject.AddComponent<BoxCollider2D>();
        pole.isTrigger = false;
        pole.size = new Vector2(width, height);
        pole.offset = new Vector2(bounds.center.x, bounds.min.y + height * 0.5f);
        _passable.Add(pole);
    }

    static bool IsTree(string name)
    {
        return name.StartsWith("tree") && !name.Contains("root");
    }
}
