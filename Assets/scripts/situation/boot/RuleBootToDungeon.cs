using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using np;
using nangka.entity;
using nangka.situation.dungeon;
using nangka.utility;

namespace nangka {
    namespace situation {
        namespace boot
        {

            // チェック用に用意したルール
            // 通常は利用しないもの
            public class RuleBootToDungeon : RuleBootBase, INpRule
            {
                protected override IEnumerator Ready()
                {
                    // Fade 制御 Entity の準備
                    yield return ReadyEntityFade();

                    // ダンジョンの準備
                    yield return ReadyDungeon();
                }

                private IEnumerator ReadyDungeon()
                {
                    yield return null;

                    // Dungeon Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityDungeon>();

                    // 次の Situation を登録
                    this.nextSituation = NpSituation.Create<SituationDungeon>();
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
