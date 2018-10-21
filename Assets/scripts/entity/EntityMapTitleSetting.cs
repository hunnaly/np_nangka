using System;
using System.IO;
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
        // IEntityMapTitleSetting
        //------------------------------------------------------------------
        public interface IEntityMapTitleSetting : IEntity
        {
            EntityMapTitleSetting.RESULT GetResult();

        } //interface IEntityMapTitleSetting


        //------------------------------------------------------------------
        // EntityMapTitleSetting
        //------------------------------------------------------------------
        public class EntityMapTitleSetting : NpEntity, IEntityMapTitleSetting
        {
            public enum RESULT
            {
                NONE,
                CANCEL,
                OK

            } //enum RESULT

            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private CommonInputDialog dialog;

            private RESULT _result;
            public RESULT GetResult() { return this._result; }


            //------------------------------------------------------------------
            // Entity ロジック処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMapTitleSetting.StartProc()");

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                IEntityCommonInputDialog iDialog = Utility.GetIEntityCommonInputDialog();
                this.dialog = iDialog.Create("Input Map Title", iMEConsole.GetMapTitle());

                this.dialog.SetParent(iMEConsole.GetRootCanvasTransform());
                this.dialog.SetEventCB(CommonInputDialog.RESULT.OK, new CommonInputDialog.EventKeyInfo(KeyCode.Return, this.DialogCB_OK));
                this.dialog.SetEventCB(CommonInputDialog.RESULT.CANCEL, new CommonInputDialog.EventKeyInfo(KeyCode.RightShift, this.DialogCB_Cancel));
                this.dialog.Show();

                this._result = RESULT.NONE;

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityMapTitleSetting.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                if (this.dialog != null)
                {
                    IEntityCommonInputDialog iDialog = Utility.GetIEntityCommonInputDialog();
                    iDialog.Release(this.dialog);
                    this.dialog = null;
                }
            }

            //------------------------------------------------------------------
            // Dialog コールバック
            //------------------------------------------------------------------

            private void DialogCB_OK(string text)
            {
                IEntityCommonInputDialog iDialog = Utility.GetIEntityCommonInputDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                iMEConsole.SetMapTitle(text);

                this._result = RESULT.OK;
            }

            private void DialogCB_Cancel(string text)
            {
                IEntityCommonInputDialog iDialog = Utility.GetIEntityCommonInputDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                this._result = RESULT.CANCEL;
            }

        } //class EntityMapTitleSetting

    } //namespace entity
} //namespace nangka