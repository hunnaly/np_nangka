using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using np;
using nangka.entity;
using nangka.situation.dev;
using nangka.utility;

namespace nangka
{
    namespace situation
    {
        namespace boot
        {

            // チェック用に用意したルール
            // 通常は利用しないもの
            public class RuleBootToDevEntrance : RuleBootBase, INpRule
            {
                protected override IEnumerator Ready()
                {
                    // ダンジョン用カメラを非アクティブ化
                    Global.Instance.cameraPlayer.enabled = false;

                    // Fade 制御 Entity の準備
                    yield return ReadyEntityFade();

                    // DevEntrance Entity の登録
                    yield return Utility.RegistEntityDevEntrance();

                    // フェードイン
                    yield return Utility.FadeIn();

                    // 次の Situation を登録
                    this.nextSituation = NpSituation.Create<SituationDevEntrance>();
                    yield return null;
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
