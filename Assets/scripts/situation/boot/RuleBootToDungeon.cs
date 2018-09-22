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
                    // Frame 制御 Entity を登録
                    // 有効な状態になるまで待つ
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityFrame>();
                    IEntityFrame iEntityFrame = null;
                    while ((iEntityFrame = Utility.GetIEntityFrame()) == null) yield return null;
                    while (iEntityFrame.IsInitialized() == false) yield return null;

                    // TextureResources Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityTextureResources>();

                    // MapData Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityMapData>();

                    // PlayerData Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityPlayerData>();

                    // Player Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityPlayer>();

                    // Dungeon Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityDungeon>();

                    // 次の Situation を登録
                    this.nextSituation = NpSituation.Create<SituationDungeon>();                    

                    yield return null;
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
