using System;
using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    public static ScorePopup Instance { get; private set; }
    [Serializable]
    public class Style
    {
        [SerializeField] private int _points;
        [SerializeField] private DamageNumber _prefab;

        public int Points => _points;
        public DamageNumber Prefab => _prefab;
    }

    [SerializeField] private DamageNumber _numberPrefab;
    [SerializeField] private Style[] _styles;
    [SerializeField] private string _prefix = "+";
    [SerializeField, Min(0.05f)] private float _scale = 0.4f;
    [SerializeField, Min(0f)] private float _repeatDelay = 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Show(int score)
    {
        Show(score, transform.position);
    }

    public void Show(int score, Vector3 worldPosition)
    {
        if (score == 0)
            return;

        DamageNumber prefab = PrefabFor(score);
        if (prefab == null)
            return;

        Vector3 position = worldPosition;
        position.x += UnityEngine.Random.Range(-0.15f, 0.15f);
        position.y += UnityEngine.Random.Range(0f, 0.15f);

        DamageNumber popup = prefab.Spawn(position, score);
        popup.enableLeftText = _prefix.Length > 0;
        popup.leftText = _prefix;
        popup.SetScale(_scale);
    }

    public void ShowEach(IReadOnlyList<int> scores, Vector3 worldPosition)
    {
        if (scores == null || scores.Count == 0)
            return;

        StartCoroutine(ShowEachRoutine(new List<int>(scores), worldPosition));
    }

    private IEnumerator ShowEachRoutine(List<int> scores, Vector3 worldPosition)
    {
        for (int i = 0; i < scores.Count; i++)
        {
            Show(scores[i], worldPosition);

            if (i < scores.Count - 1 && _repeatDelay > 0f)
                yield return new WaitForSeconds(_repeatDelay);
        }
    }

    private DamageNumber PrefabFor(int score)
    {
        if (_styles != null)
        {
            for (int i = 0; i < _styles.Length; i++)
            {
                Style style = _styles[i];
                if (style != null && style.Points == score && style.Prefab != null)
                    return style.Prefab;
            }
        }

        return _numberPrefab;
    }
}
