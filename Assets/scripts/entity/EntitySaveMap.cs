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
        // IEntitySaveMap
        //------------------------------------------------------------------
        public interface IEntitySaveMap : IEntity
        {
            EntitySaveMap.RESULT GetResult();

        } //interface IEntitySaveMap


        //------------------------------------------------------------------
        // EntitySaveMap
        //------------------------------------------------------------------
        public class EntitySaveMap : NpEntity, IEntitySaveMap
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

            private EntitySaveMap.RESULT _result;
            public EntitySaveMap.RESULT GetResult() { return this._result; }


            public interface IMapDataAccessor
            {
                void Save(string fileName);

            } //interface IMapDataAccessor


            //------------------------------------------------------------------
            // Entity ロジック処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntitySaveMap.StartProc()");

                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                this.dialog = iDialog.Create();

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                this.dialog.SetParent(iMEConsole.GetRootCanvasTransform());
                this.dialog.SetKeyCB(KeyCode.Return, this.DialogCB_OK);
                this.dialog.SetKeyCB(KeyCode.RightShift, this.DialogCB_Cancel);
                this.dialog.SetText("マップをセーブします。よろしいですか？\n\n  [OK(Return)]   [Cancel(Right-Shift)]");
                this.dialog.Show();

                this._result = EntitySaveMap.RESULT.NONE;

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntitySaveMap.TerminateProc()");

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

                bool bSuccess = this.SaveMapData();
                this.CreateResultDialog(bSuccess);
            }

            private void DialogCB_Cancel()
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                this._result = EntitySaveMap.RESULT.CANCEL;
            }

            private bool SaveMapData()
            {
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IMapDataAccessor acc = (IMapDataAccessor)(iMapData.GetOwnEntity());
                acc.Save("map_test.dat");
                return true;
            }

            private void CreateResultDialog(bool bSuccess)
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                this.dialog = iDialog.Create();

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                this.dialog.SetParent(iMEConsole.GetRootCanvasTransform());
                this.dialog.SetKeyCB(KeyCode.Return, this.ResultDialog_OK);
                this.dialog.SetText(bSuccess ? "セーブしました。" : "セーブに失敗しました。");
                this.dialog.Show();
            }

            private void ResultDialog_OK()
            {
                IEntityCommonDialog iDialog = Utility.GetIEntityCommonDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                this._result = EntitySaveMap.RESULT.SUCCESS;
            }

        } //class EntitySaveMap

    } //namespace entity
} //namespace nangka