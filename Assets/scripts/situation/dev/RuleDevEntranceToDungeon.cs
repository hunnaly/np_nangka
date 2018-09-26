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
                    yield return Utility.FadeOut(1.0f);

                    // DevEntrance Entity を終了させる
                    // （終了すると自動的に登録解除される）
                    IEntityDevEntrance iDevEntrance = Utility.GetIEntityDevEntrance();
                    iDevEntrance.Terminate();

                    // DevEntrance に入るときに見えなくしていたダンジョン用カメラをもとにもどす
                    Global.Instance.cameraPlayer.enabled = true;

                    // TextureResources Entity の準備
                    yield return ReadyEntityTextureResources();

                    // ダンジョンの準備
                    yield return ReadyDungeon();
                }

                private IEnumerator ReadyEntityTextureResources()
                {
                    // TextureResources Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityTextureResources>();

                    IEntityTextureResources iTexRes = null;
                    while ((iTexRes = Utility.GetIEntityTextureResources()) == null) yield return null;
                    while (iTexRes.IsReadyLogic() == false) yield return null;
                }

                private IEnumerator ReadyDungeon()
                {
                    // Frame 制御 Entity を登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityFrame>();
                    IEntityFrame iEntityFrame = null;
                    while ((iEntityFrame = Utility.GetIEntityFrame()) == null) yield return null;
                    while (iEntityFrame.IsReadyLogic() == false) yield return null;

                    // MapData Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityMapData>();
                    IEntityMapData iMapData = null;
                    while ((iMapData = Utility.GetIEntityMapData()) == null) yield return null;
                    while (iMapData.IsReadyLogic() == false) yield return null;
                    iMapData.Load(Utility.GetIEntityTextureResources());

                    // Structure Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityStructure>();
                    IEntityStructure iStructure = null;
                    while ((iStructure = Utility.GetIEntityStructure()) == null) yield return null;
                    while (iStructure.IsReadyLogic() == false) yield return null;

                    // PlayerData Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityPlayerData>();
                    IEntityPlayerData iPlayerData = null;
                    while ((iPlayerData = Utility.GetIEntityPlayerData()) == null) yield return null;
                    while (iPlayerData.IsReadyLogic() == false) yield return null;

                    // Player Entity の登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityPlayer>();
                    IEntityPlayer iPlayer = null;
                    while ((iPlayer = Utility.GetIEntityPlayer()) == null) yield return null;
                    while (iPlayer.IsReadyLogic() == false) yield return null;


                    // Dungeon Entity の登録
                    // 準備完了すると状態を維持。明示的な起動が必要。
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityDungeon>();
                    IEntityDungeon iDungeon = null;
                    while ((iDungeon = Utility.GetIEntityDungeon()) == null) yield return null;
                    while (iDungeon.IsReadyLogic() == false) yield return null;

                    // フェードイン
                    yield return Utility.FadeIn();

                    // Dungeon Entity を開始
                    iDungeon.Run();

                    // 次の Situation を登録
                    this.nextSituation = NpSituation.Create<SituationDungeon>();

                    yield return null;
                }
            }

        } //namespace dev
    } //namespace situation
} //namespace nangka
