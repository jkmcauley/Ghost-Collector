using System.Collections.Generic;
using UnityEngine;

public class GhostVortex : MonoBehaviour
{
    [Tooltip("0 deposits the entire line.")]
    [SerializeField, Min(0)]
    private int _depositLimit = 0;

    [SerializeField]
    private ScorePopup _scorePopup;

    private readonly List<int> _depositedPoints = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<GhostCollector>(out var collector))
            return;

        int amount = _depositLimit == 0
            ? collector.Count
            : _depositLimit;

        _depositedPoints.Clear();
        int deposited = collector.Deposit(amount, _depositedPoints);
        if (deposited == 0)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayWhoosh();

        int points = 0;
        for (int i = 0; i < _depositedPoints.Count; i++)
            points += _depositedPoints[i];

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(points);
            GameManager.Instance.AddReturnedGhosts(deposited);
        }

        if (UIManager.Instance != null && GameManager.Instance != null)
            UIManager.Instance.UpdateScore(GameManager.Instance.Score);

        if (_scorePopup != null)
            _scorePopup.ShowEach(_depositedPoints, transform.position);
    }
}
