using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;
using nangka.situation;
using nangka.situation.dungeon;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {

            public class RuleDevEntranceToMapEditor : RuleBase, INpRule
            {
                public bool CheckRule()
                {
                    //Debug.Log("RuleDevEntranceToMapEditor.CheckRule()");

                    return false;
                    // Boot Entity インタフェースの取得
                    //NpEntity entity = Global.Instance.EntityCtrl.Exist("np.entity.EntityBoot");
                    //IEntityBoot iEntityBoot = entity.GetInterface<EntityBoot, IEntityBoot>();

                    // Boot 処理が完了しているかチェック
                    //return iEntityBoot.IsCompleted();
                }

                public void ReadyNextSituation()
                {
                    Debug.Log("RuleDevEntranceToMapEditor.ReadyNextSituation()");

                    // Boot Entity を終了させる
                    // （終了すると自動的に登録解除される）
                    //NpEntity entity = Global.Instance.EntityCtrl.Exist("np.entity.EntityBoot");
                    //IEntityBoot iEntityBoot = entity.GetInterface<EntityBoot, IEntityBoot>();
                    //iEntityBoot.Terminate();

                    // Battle Entity の登録
                    //Global.Instance.EntityCtrl.CreateAndRegist<EntityBattle>();

                    //this.nextSituation = NpSituation.Create<SituationBattle>();
                }

                public void CleanUpForce()
                {
                    Debug.Log("RuleDevEntranceToMapEditor.CleanUpForce()");
                }
            }

        } //namespace dev
    } //namespace situation
} //namespace nangka
