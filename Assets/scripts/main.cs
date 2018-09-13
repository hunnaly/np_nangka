using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;

namespace nangka {

    public class main : NpAppBase
    {
        protected override void OnStart()
        {
            // Global の初期化
            // Global 経由で簡単に EntityCtrl を利用できるようにする
            Global.Instance.Initialize(this);

            // Situation 機能を Entity として登録
            Global.Instance.EntityCtrl.CreateAndRegist<EntitySituation>();
        }

    } //class main

} //namespace nangka
