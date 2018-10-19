using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityLoadMap
        //------------------------------------------------------------------
        public interface IEntityLoadMap : IEntity
        {
            EntityLoadMap.RESULT GetResult();

        } //interface IEntityLoadMap


        //------------------------------------------------------------------
        // EntityLoadMap
        //------------------------------------------------------------------
        public class EntityLoadMap : NpEntity, IEntityLoadMap
        {
            public enum RESULT
            {
                NONE,
                CANCEL,
                SUCCESS

            } //enum RESULT

            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private CommonDialog dialog;

            private EntityLoadMap.RESULT _result;
            public EntityLoadMap.RESULT GetResult() { return this._result; }


            //------------------------------------------------------------------
            // Entity ロジック処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityLoadMap.StartProc()");

                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                this.dialog = iDialog.Create();

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                this.dialog.SetParent(iMEConsole.GetRootCanvasTransform());
                this.dialog.SetKeyCB(KeyCode.Return, this.DialogCB_OK);
                this.dialog.SetKeyCB(KeyCode.RightShift, this.DialogCB_Cancel);
                this.dialog.SetText("マップをロードします。よろしいですか？\n\n  [OK(Return)]   [Cancel(Right-Shift)]");
                this.dialog.Show();

                this._result = EntityLoadMap.RESULT.NONE;

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityLoadMap.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                if (this.dialog != null)
                {
                    IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                    iDialog.Release(this.dialog);
                    this.dialog = null;
                }
            }


            //------------------------------------------------------------------
            // Dialog コールバック
            //------------------------------------------------------------------

            private void DialogCB_OK()
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                Utility.StartCoroutine(this.Recreate());
            }

            private void DialogCB_Cancel()
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                this._result = EntityLoadMap.RESULT.CANCEL;
            }

            private IEnumerator Recreate()
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                iDungeon.Reset();

                yield return Utility.RegistEntityRecreator();
                IEntityRecreator iRecreator = Utility.GetIEntityRecreator();
                iRecreator.Run(EntityRecreator.MODE_PLAYER.EMPTY_MMOPEN, EntityRecreator.MODE_MAP.FILE, "map_test.dat");
                if (iRecreator.IsFinished() == false) yield return null;
                iRecreator.Terminate();

                iDungeon.Restart();

                this._result = EntityLoadMap.RESULT.SUCCESS;
            }

        } //class EntityLoadMap

    } //namespace entity
} //namespace nangka