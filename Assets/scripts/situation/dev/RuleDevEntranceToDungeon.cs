using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;
using nangka.situation;
using nangka.situation.dungeon;
using nangka.utility;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {

            public class RuleDevEntranceToDungeon : RuleBase, INpRule
            {
                public bool CheckRule()
                {
                    IEntityDevEntrance iDevEntrance = Utility.GetIEntityDevEntrance();
                    DEV_ITEM selected = iDevEntrance.GetSelected();
                    return (selected == DEV_ITEM.DUNGEON_TEST);
                }

                public void ReadyNextSituation()
                {
                    Debug.Log("RuleDevEntranceToDungeon.ReadyNextSituation()");

                    Utility.StartCoroutine(this.Ready());
                }

                public void CleanUpForce()
                {
                    Debug.Log("RuleDevEntranceToDungeon.CleanUpForce()");
                }

                private IEnumerator Ready()
                {
                    // 画面をフェードアウト
                    yield return Utility.FadeOut(1.0f);

                    // DevEntrance Entity を終了させる
                    // （終了すると自動的に登録解除される）
                    IEntityDevEntrance iDevEntrance = Utility.GetIEntityDevEntrance();
                    iDevEntrance.Terminate();

                    // DevEntrance に入るときに見えなくしていたダンジョン用カメラをもとにもどす
                    Global.Instance.cameraPlayer.enabled = true;

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

                    yield return Utility.RegistEntityPlayerData();
                    yield return Utility.RegistEntityMapData();
                    yield return Utility.RegistEntityRecreator();

                    IEntityRecreator iRecreator = Utility.GetIEntityRecreator();
                    iRecreator.Run();
                    if (iRecreator.IsFinished() == false) yield return null;
                    iRecreator.Terminate();

                    yield return Utility.RegistEntityFrame();
                    yield return Utility.RegistEntityMiniMap();
                    yield return Utility.RegistEntityStructure();
                    yield return Utility.RegistEntityPlayer();
                    yield return Utility.RegistEntityDungeon();
                }
            }

        } //namespace dev
    } //namespace situation
} //namespace nangka
