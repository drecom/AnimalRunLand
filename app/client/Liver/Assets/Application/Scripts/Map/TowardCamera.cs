using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TowardCamera : MonoBehaviour
{
    public Camera raceCamera { set; private get; }


    void Update()
    {
        // カメラの方を向く処理
        Vector3 p = this.raceCamera.transform.position;
        p.y = transform.position.y;
        transform.LookAt(p);
    }
}
