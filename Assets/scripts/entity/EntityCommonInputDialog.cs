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
        // CommonInput
        //------------------------------------------------------------------
        public class CommonInputDialog : EntityCommonInputDialog.ICommonInputDialogAccessor
        {
            private GameObject _instance;
            public GameObject gameObject { get { return this._instance; } }

            public enum RESULT : int
            {
                OK,
                CANCEL,
                MAX
            }
            public delegate void EventKeyProc(string text);
            public class EventKeyInfo
            {
                private KeyCode _keyCode;
                public KeyCode keyCode { get { return this._keyCode; } }

                private EventKeyProc _func;
                public EventKeyProc func { get { return this._func; } }

                public EventKeyInfo(KeyCode keyCode, EventKeyProc func)
                {
                    this._keyCode = keyCode;
                    this._func = func;
                }
            }
            private Dictionary<RESULT, EventKeyInfo> _eventProcTable;

            private bool bActiveEvent;
            void EntityCommonInputDialog.ICommonInputDialogAccessor.SetActive(bool b) { this.SetActive(b); }


            //------------------------------------------------------------------
            // EntityCommonInputDialog 用
            //------------------------------------------------------------------

            void EntityCommonInputDialog.ICommonInputDialogAccessor.Start(
                GameObject prefabDialog, string title, string textInit)
            {
                this.NewEventProcTable();

                GameObject obj = this.CreateDialog(prefabDialog);
                obj.SetActive(false);

                this.SetTitle(obj, title);
                this.SetInputFieldText(obj, textInit);

                this._instance = obj;
            }

            void EntityCommonInputDialog.ICommonInputDialogAccessor.Release()
            {
                this.SetActive(false);
                this.ReleaseEventProcTable();
                this.ReleaseDialog();
            }

            void EntityCommonInputDialog.ICommonInputDialogAccessor.EventProc()
            {
                if (this._instance == null) return;
                if (this._instance.activeInHierarchy == false) return;
                if (this.bActiveEvent == false) return;

                if (this.DefaultEventProc() == false)
                {
                    this.UserEventProc();
                }
            }

            private bool DefaultEventProc()
            {
                return false;
            }

            private void UserEventProc()
            {
                if (this._eventProcTable == null) return;

                foreach (KeyValuePair<RESULT, EventKeyInfo> pair in this._eventProcTable)
                {
                    EventKeyInfo info = pair.Value;

                    if (Input.GetKeyDown(info.keyCode))
                    {
                        if (info == null) continue;

                        var component = this._instance.GetComponent<ObjectTable>();
                        InputField inputField = component.objectTable[1].GetComponent<InputField>();
                        info.func(inputField.text);
                        break;
                    }
                }
            }

            private void SetInputFieldText(GameObject obj, string text)
            {
                var component = obj.GetComponent<ObjectTable>();
                InputField inputField = component.objectTable[1].GetComponent<InputField>();
                inputField.text = text;
            }

            //------------------------------------------------------------------
            // 外部提供メソッド
            //------------------------------------------------------------------

            public void SetTitle(string title)
            {
                if (this._instance == null) return;
                this.SetTitle(this._instance, title);
            }
            private void SetTitle(GameObject obj, string title)
            {
                var component = obj.GetComponent<ObjectTable>();
                Text compText = component.objectTable[0].GetComponent<Text>();
                compText.text = title;
            }

            public void SetParent(Transform parent, bool worldPositionStays = false)
            {
                if (this._instance == null) return;
                if (parent == null) return;
                this._instance.transform.SetParent(parent, worldPositionStays);
            }

            public void Show()
            {
                if (this._instance == null) return;
                this._instance.SetActive(true);
            }

            public void Hide()
            {
                if (this._instance == null) return;
                this._instance.SetActive(false);
            }

            public void SetActive(bool b)
            {
                this.bActiveEvent = b;

                if (this._instance == null) return;

                var component = this._instance.GetComponent<ObjectTable>();
                InputField inputField = component.objectTable[1].GetComponent<InputField>();

                if (b) inputField.ActivateInputField();
                else inputField.DeactivateInputField();
            }

            public void SetEventCB(RESULT result, EventKeyInfo info)
            {
                if (this._instance == null) return;

                Dictionary<RESULT, EventKeyInfo> table = this.GetEventProcTable();
                if (table == null) return;

                table.Add(result, info);
            }

            //------------------------------------------------------------------
            // ダイアログ生成処理
            //------------------------------------------------------------------

            private GameObject CreateDialog(GameObject prefabDialog)
            {
                GameObject objDialog = (GameObject)Object.Instantiate(prefabDialog);
                objDialog.SetActive(false);

                return objDialog;
            }

            private void ReleaseDialog()
            {
                if (this._instance)
                {
                    Object.Destroy(this._instance);
                }
                this._instance = null;
            }

            //------------------------------------------------------------------
            // EventProcTable 関連
            //------------------------------------------------------------------

            private void NewEventProcTable()
            {
                this._eventProcTable = new Dictionary<RESULT, EventKeyInfo>();
            }
            private void CleanEventProcTable()
            {
                if (this._eventProcTable == null) return;
                this._eventProcTable.Clear();
            }
            private void ReleaseEventProcTable()
            {
                this.CleanEventProcTable();
                this._eventProcTable = null;
            }

            private Dictionary<RESULT, EventKeyInfo> GetEventProcTable()
            {
                if (this._eventProcTable == null)
                {
                    this.NewEventProcTable();
                }
                return this._eventProcTable;
            }
        }

        //------------------------------------------------------------------
        // IEntityCommonInputDialog
        //------------------------------------------------------------------
        public interface IEntityCommonInputDialog : IEntity
        {
            CommonInputDialog Create(string title = "", string textInit = null);
            void Release(CommonInputDialog dlg);

        } //interface IEntityCommonInputDialog


        //------------------------------------------------------------------
        // EntityCommonInputDialog
        //------------------------------------------------------------------
        public class EntityCommonInputDialog : NpEntity, IEntityCommonInputDialog
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private GameObject prefabDialog;
            private List<CommonInputDialog> listTable;
            private List<CommonInputDialog> listAddTable;
            private List<CommonInputDialog> listReleaseTable;

            public interface ICommonInputDialogAccessor
            {
                void Start(GameObject prefabDialog, string title, string textInit);
                void Release();
                void EventProc();
                void SetActive(bool b);
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityCommonInputDialog.StartProc()");

                this.prefabDialog = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_COMMON_INPUT_DIALOG);
                this.listTable = new List<CommonInputDialog>();
                this.listAddTable = new List<CommonInputDialog>();
                this.listReleaseTable = new List<CommonInputDialog>();

                this._bReadyLogic = true;
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this._bReadyLogic == false) return false;

                foreach (CommonInputDialog dlg in this.listAddTable)
                {
                    this.listTable.Add(dlg);
                    dlg.SetActive(true);
                }
                this.listAddTable.Clear();

                foreach (CommonInputDialog dlg in this.listReleaseTable)
                {
                    dlg.SetActive(false);
                    this.listTable.Remove(dlg);

                    this.ReleaseDialog(dlg);
                }
                this.listReleaseTable.Clear();

                foreach (CommonInputDialog dlg in this.listTable)
                {
                    this.UpdateDialog(dlg);
                }

                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityCommonInputDialog.TerminateProc()");

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

            private void ClearTable(List<CommonInputDialog> list)
            {
                if (list == null) return;

                foreach (CommonInputDialog dlg in list)
                {
                    if (dlg == null) continue;
                    this.ReleaseDialog(dlg);
                }
                list.Clear();
            }

            private void ReleaseDialog(CommonInputDialog dlg)
            {
                ICommonInputDialogAccessor acc = (ICommonInputDialogAccessor)dlg;
                acc.Release();
            }

            private void UpdateDialog(CommonInputDialog dlg)
            {
                ICommonInputDialogAccessor acc = (ICommonInputDialogAccessor)dlg;
                acc.EventProc();
            }

            //------------------------------------------------------------------
            // ダイアログ処理
            //------------------------------------------------------------------

            public CommonInputDialog Create(string title = "", string textInit = null)
            {
                if (this.IsReadyLogic() == false) return null;

                CommonInputDialog dlg = new CommonInputDialog();
                ICommonInputDialogAccessor acc = (ICommonInputDialogAccessor)dlg;
                acc.Start(this.prefabDialog, title, textInit);

                this.listAddTable.Add(dlg);

                return dlg;
            }

            public void Release(CommonInputDialog dlg)
            {
                if (this.IsReadyLogic() == false) return;

                CommonInputDialog temp = this.listAddTable.Find(x => x == dlg);
                if (temp == null) temp = this.listTable.Find(x => x == dlg);
                if (temp == null) return;

                this.listReleaseTable.Add(dlg);
            }


        } //class EntityCommonInputDialog

    } //namespace entity
} //namespace nangka
