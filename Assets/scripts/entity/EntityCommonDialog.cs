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
        // CommonDialog
        //------------------------------------------------------------------
        public class CommonDialog : EntityCommonDialog.ICommonDialogAccessor
        {
            private GameObject instance;
            void EntityCommonDialog.ICommonDialogAccessor.SetInstance(GameObject obj) { this.instance = obj; }
            GameObject EntityCommonDialog.ICommonDialogAccessor.GetInstance() { return this.instance; }

            public delegate void EventKeyProc();
            private Dictionary<KeyCode, CommonDialog.EventKeyProc> keyProcTable;

            private bool bActive;
            void EntityCommonDialog.ICommonDialogAccessor.SetActive(bool b) { this.SetActive(b); }


            void EntityCommonDialog.ICommonDialogAccessor.Clear()
            {
                this.instance = null;
                this.bActive = false;

                if (this.keyProcTable != null)
                {
                    this.keyProcTable.Clear();
                    this.keyProcTable = null;
                }
            }

            void EntityCommonDialog.ICommonDialogAccessor.EventProc()
            {
                if (this.instance == null) return;
                if (this.instance.activeInHierarchy == false) return;
                if (this.bActive == false) return;
                if (this.keyProcTable == null) return;

                foreach (KeyValuePair<KeyCode, CommonDialog.EventKeyProc> pair in this.keyProcTable)
                {
                    if (Input.GetKeyDown(pair.Key))
                    {
                        if (pair.Value == null) continue;
                        pair.Value();
                        break;
                    }
                }
            }


            //------------------------------------------------------------------
            // 外部提供メソッド
            //------------------------------------------------------------------

            public void SetParent(Transform parent, bool worldPositionStays = false)
            {
                if (this.instance == null) return;
                if (parent == null) return;
                this.instance.transform.SetParent(parent, worldPositionStays);
            }

            public void Show()
            {
                if (this.instance == null) return;
                this.instance.SetActive(true);
            }

            public void Hide()
            {
                if (this.instance == null) return;
                this.instance.SetActive(false);
            }

            public void SetText(string text)
            {
                if (this.instance == null) return;

                var component = this.instance.GetComponent<ObjectTable>();
                if (component == null) return;

                GameObject objText = component.objectTable[0];
                Text compText = objText.GetComponent<Text>();
                if (compText == null) return;

                compText.text = text;
            }

            public void SetActive(bool b)
            {
                this.bActive = b;
            }

            public void SetKeyCB(KeyCode keyCode, CommonDialog.EventKeyProc func)
            {
                if (this.instance == null) return;

                Dictionary<KeyCode, CommonDialog.EventKeyProc> table = this.GetKeyProcTable();
                if (table == null) return;

                table.Add(keyCode, func);
            }

            private Dictionary<KeyCode, CommonDialog.EventKeyProc> GetKeyProcTable()
            {
                if (this.keyProcTable == null)
                {
                    this.keyProcTable = new Dictionary<KeyCode, CommonDialog.EventKeyProc>();
                }
                return this.keyProcTable;
            }
        }

        //------------------------------------------------------------------
        // IEntityCommonDialog
        //------------------------------------------------------------------
        public interface IEntityCommonDialog : IEntity
        {
            CommonDialog Create();
            void Release(CommonDialog dlg);

        } //interface IEntityCommonDialog


        //------------------------------------------------------------------
        // EntityCommonDialog
        //------------------------------------------------------------------
        public class EntityCommonDialog : NpEntity, IEntityCommonDialog
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private GameObject prefabDialog;
            private List<CommonDialog> listTable;
            private List<CommonDialog> listAddTable;
            private List<CommonDialog> listReleaseTable;

            public interface ICommonDialogAccessor
            {
                void SetInstance(GameObject obj);
                GameObject GetInstance();
                void Clear();
                void EventProc();
                void SetActive(bool b);
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityCommonDialog.StartProc()");

                this.prefabDialog = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_COMMON_DIALOG);
                this.listTable = new List<CommonDialog>();
                this.listAddTable = new List<CommonDialog>();
                this.listReleaseTable = new List<CommonDialog>();

                this._bReadyLogic = true;
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this._bReadyLogic == false) return false;

                foreach (CommonDialog dlg in this.listAddTable)
                {
                    this.listTable.Add(dlg);
                }
                this.listAddTable.Clear();

                foreach(CommonDialog dlg in this.listReleaseTable)
                {
                    this.listTable.Remove(dlg);
                    this.ClearDialog(dlg);
                }
                this.listReleaseTable.Clear();

                foreach(CommonDialog dlg in this.listTable)
                {
                    this.UpdateDialog(dlg);
                }

                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityCommonDialog.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this.ClearTable(this.listTable);
                this.listTable = null;

                this.ClearTable(this.listAddTable);
                this.listAddTable = null;

                this.ClearTable(this.listReleaseTable);
                this.listReleaseTable = null;

                if (this.prefabDialog != null)
                {
                    Resources.UnloadAsset(this.prefabDialog);
                    this.prefabDialog = null;
                }
            }

            private void ClearTable(List<CommonDialog> list)
            {
                if (list == null) return;

                foreach (CommonDialog dlg in list)
                {
                    if (dlg == null) continue;
                    this.ClearDialog(dlg);
                }
                list.Clear();
            }

            private void ClearDialog(CommonDialog dlg)
            {
                ICommonDialogAccessor acc = (ICommonDialogAccessor)dlg;
                GameObject obj = acc.GetInstance();
                if (obj) Object.Destroy(obj);
                acc.Clear();
            }

            private void UpdateDialog(CommonDialog dlg)
            {
                ICommonDialogAccessor acc = (ICommonDialogAccessor)dlg;
                acc.EventProc();
            }

            //------------------------------------------------------------------
            // ダイアログ処理
            //------------------------------------------------------------------

            public CommonDialog Create()
            {
                if (this.IsReadyLogic() == false) return null;

                CommonDialog dlg = new CommonDialog();

                ICommonDialogAccessor acc = (ICommonDialogAccessor)dlg;
                GameObject obj = (GameObject)Object.Instantiate(this.prefabDialog);
                obj.SetActive(false);
                acc.SetInstance(obj);
                acc.SetActive(true);

                this.listAddTable.Add(dlg);

                return dlg;
            }

            public void Release(CommonDialog dlg)
            {
                if (this.IsReadyLogic() == false) return;

                CommonDialog temp = this.listAddTable.Find(x => x == dlg);
                if (temp == null) temp = this.listTable.Find(x => x == dlg);
                if (temp == null) return;

                this.listReleaseTable.Add(dlg);
            }


        } //class EntityCommonDialog

    } //namespace entity
} //namespace nangka
