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

            private CommonListDialog dialog;

            private EntityLoadMap.RESULT _result;
            public EntityLoadMap.RESULT GetResult() { return this._result; }


            //------------------------------------------------------------------
            // Entity ロジック処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityLoadMap.StartProc()");

                IEntityCommonListDialog iDialog = Utility.GetIEntityCommonListDialog();
                this.dialog = iDialog.Create(5, "Map List");

                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                this.dialog.SetParent(iMEConsole.GetRootCanvasTransform());
                this.dialog.SetEventCB(CommonListDialog.RESULT.OK, new CommonListDialog.EventKeyInfo(KeyCode.Return, this.DialogCB_OK));
                this.dialog.SetEventCB(CommonListDialog.RESULT.CANCEL, new CommonListDialog.EventKeyInfo(KeyCode.RightShift, this.DialogCB_Cancel));
                this.EnumerateMapFile(this.dialog);
                this.dialog.FlashItem();
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
                    IEntityCommonListDialog iDialog = Utility.GetIEntityCommonListDialog();
                    iDialog.Release(this.dialog);
                    this.dialog = null;
                }
            }

            //------------------------------------------------------------------
            // ファイル一覧
            //------------------------------------------------------------------

            private void EnumerateMapFile(CommonListDialog dlg)
            {
                try
                {
                    var files = Directory.GetFiles(Define.GetMapFilePath(), "*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string temp = file.TrimStart(Define.GetMapFilePath().ToCharArray());
                        dlg.AddItem(temp.Substring(1, temp.Length-1));
                    }
                }
                catch
                {
                }
            }

            //------------------------------------------------------------------
            // Dialog コールバック
            //------------------------------------------------------------------

            private void DialogCB_OK(int idx, string text)
            {
                if (text == null) return;
                Utility.StartCoroutine(this.Recreate(text));
            }

            private void DialogCB_Cancel(int idx, string text)
            {
                IEntityCommonListDialog iDialog = Utility.GetIEntityCommonListDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                this._result = EntityLoadMap.RESULT.CANCEL;
            }

            private IEnumerator Recreate(string mapFileName)
            {
                IEntityCommonListDialog iDialog = Utility.GetIEntityCommonListDialog();
                iDialog.Release(this.dialog);
                this.dialog = null;

                IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                iDungeon.Reset();

                yield return Utility.RegistEntityRecreator();
                IEntityRecreator iRecreator = Utility.GetIEntityRecreator();
                iRecreator.Run(EntityRecreator.MODE_PLAYER.EMPTY_MMOPEN, EntityRecreator.MODE_MAP.FILE, mapFileName);
                if (iRecreator.IsFinished() == false) yield return null;
                iRecreator.Terminate();

                iDungeon.Restart();

                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                iMEConsole.SetMapFileName(mapFileName);
                iMEConsole.SetMapTitle(iMapData.GetName());

                this._result = EntityLoadMap.RESULT.SUCCESS;
            }

        } //class EntityLoadMap

    } //namespace entity
} //namespace nangka