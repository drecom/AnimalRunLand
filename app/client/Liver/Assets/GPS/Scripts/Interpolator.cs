using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GPS
{

public class Interpolator : MonoBehaviour
{
    LocationManager locationManager;

    double lastTimeStamp = 0;
    Vector2d lastLocation = new Vector2d();

    // 補間データ
    class Body
    {
        public Vector2d begin;
        public Vector2d end;
        public double duration;
    }
    List<Body> bodies = new List<Body>();

    double currentTime = 0;


    // longitude 経度(東西)
    // latitude  緯度(南北)
    public double Longitude { get; private set; }
    public double Latitude  { get; private set; }


    public bool Started { get; private set; }


    void Start()
    {
        this.locationManager = GetComponent<LocationManager>();
    }


    public void OnEnable()
    {
        startPlayback();
    }


    void Update()
    {
        if (!this.locationManager.Started) { return; }

        if (this.locationManager.CurrentTimeStamp > this.lastTimeStamp)
        {
            // 最後尾に追加
            var d   = this.locationManager.CurrentTimeStamp - this.lastTimeStamp;
            var end = new Vector2d(this.locationManager.Longitude, this.locationManager.Latitude);

            if (!this.Started)
            {
                // 最初のデータ
                this.currentTime  = 0;
                this.lastLocation = end;
                d = 0;
                this.Started = true;
            }

            Body body = new Body { begin = this.lastLocation, end = end, duration = d };
            this.bodies.Add(body);
            this.lastTimeStamp = this.locationManager.CurrentTimeStamp;
            this.lastLocation = end;
        }

        if (this.bodies.Count > 0)
        {
            this.currentTime += Time.deltaTime;

            if (this.currentTime >= this.bodies[0].duration)
            {
                this.currentTime -= this.bodies[0].duration;
                this.Longitude = this.bodies[0].end.x;
                this.Latitude  = this.bodies[0].end.y;
                // 次のデータへ
                this.bodies.RemoveAt(0);
            }
            else
            {
                // 現在値を線形補間
                // FIXME プラスとマイナスの値を跨げない
                var t = this.currentTime / this.bodies[0].duration;
                var v = Vector2d.Lerp(this.bodies[0].begin, this.bodies[0].end, (float)t);
                this.Longitude = v.x;
                this.Latitude  = v.y;
            }
        }
    }


    // 動作検証用
    public void startPlayback()
    {
        this.Started = false;
        this.bodies.Clear();
        this.lastTimeStamp = 0;
    }
}
}
