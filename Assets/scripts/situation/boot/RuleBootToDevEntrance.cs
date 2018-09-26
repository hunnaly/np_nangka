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
                    Global.Instance.cameraPlayer.enabled = false;

                    // Fade 制御 Entity の準備
                    yield return ReadyEntityFade();

                    // 開発画面の準備
                    yield return ReadyDevEntrance();
                }

                private IEnumerator ReadyDevEntrance()
                {
                    // DevEntrance Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityDevEntrance>();
                    IEntityDevEntrance iDevEntrance = null;
                    while ((iDevEntrance = Utility.GetIEntityDevEntrance()) == null) yield return null;
                    while (iDevEntrance.IsReadyLogic() == false) yield return null;

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
