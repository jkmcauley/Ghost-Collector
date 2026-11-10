using System.Collections.Generic;

using UnityEngine;

public class GhostCollector : MonoBehaviour

{

    [Header("Line")]

    [SerializeField, Min(0.1f)]

    private float _ghostSpacing = 0.8f;

    [SerializeField, Min(0.01f)]

    private float _sampleDistance = 0.1f;

    [SerializeField, Min(1)]

    private int _maxGhosts = 30;

    private readonly List<GhostFollower> _ghosts = new();

    private readonly List<Vector3> _path = new();

    public int Count => _ghosts.Count;

    private void Awake()

    {

        _path.Add(transform.position);

    }

    private void LateUpdate()

    {

        RecordPath();

        MoveGhosts();

    }

    private void RecordPath()

    {

        float distance = Vector3.Distance(

            transform.position,

            _path[0]

        );

        // Add intermediate samples, even when the guard moves fast.

        while (distance >= _sampleDistance)

        {

            Vector3 sample = Vector3.MoveTowards(

                _path[0],

                transform.position,

                _sampleDistance

            );

            _path.Insert(0, sample);

            distance = Vector3.Distance(

                transform.position,

                _path[0]

            );

        }

        int maxSamples = Mathf.CeilToInt(

            (_maxGhosts * _ghostSpacing) / _sampleDistance

        ) + 2;

        if (_path.Count > maxSamples)

        {

            _path.RemoveRange(

                maxSamples,

                _path.Count - maxSamples

            );

        }

    }

    private void MoveGhosts()

    {

        for (int i = 0; i < _ghosts.Count; i++)

        {

            float distanceBehind = (i + 1) * _ghostSpacing;

            Vector3 destination = GetPathPosition(distanceBehind);

            _ghosts[i].Follow(destination);

        }

    }

    private Vector3 GetPathPosition(float distanceBehind)

    {

        Vector3 previous = transform.position;

        foreach (Vector3 point in _path)

        {

            float segmentLength = Vector3.Distance(previous, point);

            if (segmentLength > 0f &&

                distanceBehind <= segmentLength)

            {

                return Vector3.Lerp(

                    previous,

                    point,

                    distanceBehind / segmentLength

                );

            }

            distanceBehind -= segmentLength;

            previous = point;

        }

        // Until enough history exists, use the oldest position.

        return _path[_path.Count - 1];

    }

    public void Collect(GhostFollower ghost)

    {

        if (ghost.IsCollected || Count >= _maxGhosts)

            return;

        ghost.JoinLine();

        _ghosts.Add(ghost);

        if (SoundManager.Instance != null)

            SoundManager.Instance.PlayGhostCollect();

    }

    public int Deposit(int amount, List<int> points)

    {

        int deposited = Mathf.Clamp(amount, 0, Count);

        for (int i = 0; i < deposited; i++)

        {

            int lastIndex = _ghosts.Count - 1;

            GhostFollower follower = _ghosts[lastIndex];

            _ghosts.RemoveAt(lastIndex);

            int value = 0;

            if (follower.TryGetComponent(out Ghost ghost))

                value = ghost.Points;

            points?.Add(value);

            follower.Remove();

        }

        return deposited;

    }

    public void LoseGhost()
    {
        if (PlayerPower.IsActive)
            return;

        LoseGhosts(1);
    }

    public void LoseGhosts(int count)
    {
        if (PlayerPower.IsActive)
            return;

        int lost = Mathf.Min(count, Count);
        for (int i = 0; i < lost; i++)
            RemoveLastGhost();
    }

    public GhostFollower GhostFromTail(int stepsFromTail)
    {
        int index = _ghosts.Count - 1 - stepsFromTail;
        if (index < 0 || index >= _ghosts.Count)
            return null;

        return _ghosts[index];
    }

    public bool Steal(GhostFollower ghost)
    {
        if (PlayerPower.IsActive || ghost == null)
            return false;

        int index = _ghosts.IndexOf(ghost);
        if (index < 0)
            return false;

        _ghosts.RemoveAt(index);
        ghost.Explode();
        return true;
    }

    private void RemoveLastGhost()

    {

        int lastIndex = _ghosts.Count - 1;

        GhostFollower ghost = _ghosts[lastIndex];

        _ghosts.RemoveAt(lastIndex);

        ghost.Explode();

    }

}
