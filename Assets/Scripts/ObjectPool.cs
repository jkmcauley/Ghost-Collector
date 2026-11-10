using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Ghost Prefabs")]
    [Tooltip("Spawn table. Repeat a prefab to make it more common.")]
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField, Min(1)] private int _startingAmount = 20;

    private readonly List<GameObject> _objects = new();

    private void Awake()
    {
        if (_prefabs == null || _prefabs.Length == 0)
            return;

        for (int i = 0; i < _startingAmount; i++)
        {
            GameObject prefab = _prefabs[i % _prefabs.Length];
            if (prefab != null)
                CreateObject(prefab);
        }
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _objects.Add(obj);
        return obj;
    }

    public GameObject Spawn(Vector3 position)
    {
        GameObject prefab = PickPrefab();
        if (prefab == null)
            return null;

        GameObject inactive = FindInactive(prefab);
        if (inactive != null)
            return ActivateObject(inactive, position);

        return ActivateObject(CreateObject(prefab), position);
    }

    public GameObject TrySpawn(Vector3 position)
    {
        GameObject prefab = PickPrefab();
        if (prefab == null)
            return null;

        GameObject inactive = FindInactive(prefab);
        if (inactive == null)
            return null;

        return ActivateObject(inactive, position);
    }

    private GameObject PickPrefab()
    {
        if (_prefabs == null || _prefabs.Length == 0)
            return null;

        return _prefabs[Random.Range(0, _prefabs.Length)];
    }

    private GameObject FindInactive(GameObject prefab)
    {
        string cloneName = prefab.name + "(Clone)";

        for (int i = _objects.Count - 1; i >= 0; i--)
        {
            GameObject obj = _objects[i];
            if (obj == null)
            {
                _objects.RemoveAt(i);
                continue;
            }

            if (!obj.activeSelf && (obj.name == prefab.name || obj.name == cloneName))
                return obj;
        }

        return null;
    }

    private GameObject ActivateObject(GameObject obj, Vector3 position)
    {
        obj.transform.SetPositionAndRotation(position, Quaternion.identity);
        obj.SetActive(true);
        return obj;
    }
}
