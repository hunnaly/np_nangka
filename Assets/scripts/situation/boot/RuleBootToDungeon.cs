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

                    // 各種 Entity の登録および利用準備待ち
                    yield return ReadyEntities();

                    // フェードイン
                    yield return Utility.FadeIn();

                    // ダンジョン処理開始
                    IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                    iDungeon.Run();

                    // 次の Situation を登録
                    this.nextSituation = NpSituation.Create<SituationDungeon>();
                    yield return null;
                }

                private IEnumerator ReadyEntities()
                {
                    yield return Utility.RegistEntityTextureResources();
                    yield return Utility.RegistEntityFrame();

                    yield return Utility.RegistEntityMapData();
                    IEntityMapData iMapData = Utility.GetIEntityMapData();
                    iMapData.Load(Utility.GetIEntityTextureResources());

                    yield return Utility.RegistEntityStructure();
                    yield return Utility.RegistEntityPlayerData();
                    yield return Utility.RegistEntityPlayer();
                    yield return Utility.RegistEntityDungeon();
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
