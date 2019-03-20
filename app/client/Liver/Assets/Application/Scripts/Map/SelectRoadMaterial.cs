using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SelectRoadMaterial : MonoBehaviour
{
    void Awake()
    {
        Liver.RaceWindow.RoadMaterial = 0;
        var dd = GetComponent<UnityEngine.UI.Dropdown>();
        dd.ClearOptions();
        var options = new List<string>();

        for (int i = 0; i < 5; ++i)
        {
            options.Add(i.ToString());
        }

        dd.AddOptions(options);
        dd.value = 0;
    }

    public void OnValueChanged(int result)
    {
        Liver.RaceWindow.RoadMaterial = result;
    }
}
