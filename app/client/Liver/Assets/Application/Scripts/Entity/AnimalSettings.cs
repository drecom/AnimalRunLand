using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Liver.Entity
{
[CreateAssetMenu(menuName = "Configs/AnimalSettings")]
public class AnimalSettings : ScriptableObject
{
    public enum Jump
    {
        Short,      // 2m
        Standard,   // 4m
    }

    [Serializable]
    public class AnimalSetting
    {
        // 情報
        public string name;     // 名前
        public string desc;     // 紹介文
        public string strong;   // 得意
        public string weak;     // 苦手

        // パラメータ
        public float topSpeed;  // トップスピード
        public Jump jump;       // 跳躍力
        [Tooltip("推進力。大きな値ほど、すばやくトップスピードに戻ります")]
        public float force;     // 推進力
        [Tooltip("交差点などを曲がる時に道の向きに対してどれくらい角度差がついたら曲がるアニメーションを始めるかを指定(単位:度)")]
        public float turnAngle = 30; // この閾値超えると曲がるモーションになる
        [Tooltip("交差点を曲がった時にどれくらい速度が落ちるかを比率(0.0~1.0)で指定")]
        public float deceleration = 0.5f;  // 減速(%) : 曲がるモーションになるときの減速率

        [Tooltip("障害物に当たった時のペナルティ(秒)")]
        public float Penalty = 5;

        [Tooltip("解放コスト：必要スター数")]
        public int registCost = 0;

        [Tooltip("使用モデル")]
        public string model;

        [Tooltip("プレイヤーから見えるグリッド範囲(単位:グリッド数)")]
        public int POIGridDistance = 2;

        [Tooltip("スター倍化効果での、２倍スターが出現する率(残りが３倍になる)")]
        public float StarDoublerRate = 0.5f;

        [Tooltip("アイテム回収用の当たり判定の大きさ(立方体の幅、高さ、奥行きで指定(単位: m))")]
        public Vector3 ItemNormal = new Vector3(1.242663f, 1.424661f, 3.448621f);
        [Tooltip("アイテム使用時のアイテム回収用の当たり判定の大きさ(立方体の幅、高さ、奥行きで指定(単位: m))")]
        public Vector3 ItemBoost  = new Vector3(6, 8, 2.890463f);
    }

    [SerializeField] AnimalSetting Capybara;
    [SerializeField] AnimalSetting Panda;
    [SerializeField] AnimalSetting Giraffe;
    [SerializeField] AnimalSetting Elephant;
    [SerializeField] AnimalSetting Horse;
    [SerializeField] AnimalSetting Rabbit;
    [SerializeField] AnimalSetting Lion;
    [SerializeField] AnimalSetting Pig;

    static AnimalSettings _Instance;
    public static AnimalSettings Instance
    {
        get
        {
            if (_Instance == null) { _Instance = Resources.Load<AnimalSettings>("Configs/AnimalSettings"); }

            return _Instance;
        }
    }

    public AnimalSetting this[AnimalKind kind]
    {
        get
        {
            switch (kind)
            {
                case AnimalKind.Capybara:
                    return Capybara;

                case AnimalKind.Panda:
                    return Panda;

                case AnimalKind.Giraffe:
                    return Giraffe;

                case AnimalKind.Elephant:
                    return Elephant;

                case AnimalKind.Horse:
                    return Horse;

                case AnimalKind.Rabbit:
                    return Rabbit;

                case AnimalKind.Lion:
                    return Lion;

                case AnimalKind.Pig:
                    return Pig;

                default:
                    throw new Exception();
            }
        }
    }
}
}
