using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemCollider : MonoBehaviour
{
    public event Action<GameObject, bool> OnCollision = (o, flag) => { };
    public event Action<GameObject> OnPoi = (o) => { };
    public bool poiEnable = true;
    bool flag;

    private void Awake()
    {
        flag = name == "ItemBoost";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ItemGroup")
        {
            SetItemGroupCollider(other, true);
        }
        else if (other.tag == "Item")
        {
            OnCollision(other.gameObject, flag);
        }
        else if (other.tag == "Poi")
        {
            OnPoi(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "ItemGroup")
        {
            SetItemGroupCollider(other, false);
        }
    }

    void SetItemGroupCollider(Collider other, bool enable)
    {
        foreach (Transform t in other.transform)
        {
            var c = t.GetComponent<Collider>();

            if (c != null) { c.enabled = enable; }
        }
    }
}
