using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownListSort : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(RaiseList());
    }

    IEnumerator RaiseList()
    {
        yield return null;

        Transform list = transform.Find("Dropdown List");
        if (list == null)
            yield break;

        Canvas listCanvas = list.GetComponent<Canvas>();
        Canvas hud = GetComponentInParent<Canvas>();
        if (listCanvas == null || hud == null)
            yield break;

        listCanvas.overrideSorting = true;
        listCanvas.sortingOrder = hud.sortingOrder + 1;
    }
}
