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
        // CommonListDialog
        //------------------------------------------------------------------
        public class CommonListDialog : EntityCommonListDialog.ICommonListDialogAccessor
        {
            private GameObject _instance;
            public GameObject gameObject { get { return this._instance; } }

            private Dictionary<int, GameObject> _objChildTable;

            private int _itemCountPerPage;
            public int itemCountPerPage { get { return this._itemCountPerPage; } }

            private int _itemCount;
            public int itemCount { get { return this._itemCount; } }

            private int _selectItem;
            public int selectItem { get { return this._selectItem; } }

            private Dictionary<int, string> _itemNameTable;


            public enum RESULT : int
            {
                OK,
                CANCEL,
                MAX
            }
            public delegate void EventKeyProc(int idx, string text);
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
            void EntityCommonListDialog.ICommonListDialogAccessor.SetActive(bool b) { this.SetActive(b); }


            //------------------------------------------------------------------
            // EntityCommonListDialog 用
            //------------------------------------------------------------------

            void EntityCommonListDialog.ICommonListDialogAccessor.Start(
                GameObject prefabDialog, GameObject prefabItem, int itemCountPerPage, string title="")
            {
                this.NewItemNameTable();
                this.NewChildObjectTable();
                this.NewEventProcTable();

                GameObject obj = this.CreateDialog(prefabDialog, prefabItem, itemCountPerPage);
                obj.SetActive(false);

                this.SetTitle(obj, title);

                this._instance = obj;
            }

            void EntityCommonListDialog.ICommonListDialogAccessor.Release()
            {
                this.SetActive(false);
                this.ReleaseEventProcTable();
                this.ReleaseChildObjectTable();
                this.ReleaseItemNameTable();
                this.ReleaseDialog();
            }

            void EntityCommonListDialog.ICommonListDialogAccessor.EventProc()
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
                int next = this.CursorProc();
                bool bProc = (next != this._selectItem);

                this.ChangeSelect(next);

                return bProc;
            }

            private int CursorProc()
            {
                int next = this._selectItem;

                if (this._itemCount > 0)
                {
                    int top = this.GetCurTop();
                    int count = this.GetCurPageItemCount();
                    int maxPage = this.GetMaxPage();

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        int temp = this._selectItem - 1;
                        if (temp < top) temp = top + count - 1;
                        next = temp;
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        int temp = this._selectItem + 1;
                        if (temp > top + count - 1) temp = top;
                        next = temp;
                    }
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        int tempPage = this.GetCurPage() - 1;
                        if (tempPage <= 0) tempPage = maxPage;
                        if (tempPage != this.GetCurPage())
                        {
                            int delta = this._selectItem - top;
                            int temp = this.GetTop(tempPage) + delta;
                            if (temp > (this._itemCount - 1)) temp = this._itemCount - 1;
                            next = temp;
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        int tempPage = this.GetCurPage() + 1;
                        if (tempPage > maxPage) tempPage = 1;
                        if (tempPage != this.GetCurPage())
                        {
                            int delta = this._selectItem - top;
                            int temp = this.GetTop(tempPage) + delta;
                            if (temp > (this._itemCount - 1)) temp = this._itemCount - 1;
                            next = temp;
                        }
                    }
                }
                return next;
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
                        string text = (this._itemNameTable.ContainsKey(this._selectItem))
                            ? this._itemNameTable[this._selectItem]
                            : null;
                        info.func(this._selectItem, text);
                        break;
                    }
                }
            }

            private void ChangeSelect(int next, bool bForceChange = false)
            {
                if ((bForceChange == false) && (next == this._selectItem)) return;

                int temp = this._selectItem;
                this._selectItem = next;

                if (this.GetPage(next) != this.GetPage(temp))
                {
                    this.FlashItem();
                }
                else
                {
                    int idxObj = temp - this.GetTop(this.GetPage(temp));
                    this.SetCursor(this._objChildTable[idxObj], false);
                }

                int idxObjNext = next - this.GetTop(this.GetPage(next));
                this.SetCursor(this._objChildTable[idxObjNext], true);
            }

            private void SetCursor(GameObject obj, bool bShow)
            {
                Image image = obj.GetComponent<Image>();
                image.color = new Color(255.0f, 255.0f, (bShow ? 0.0f : 255.0f), 255.0f);
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

            public void AddItem(string text, bool bFlash = true)
            {
                if (this._instance == null) return;
                this.RegistItem(text);

                if (bFlash == false) return;
                int maxPage = this.GetMaxPage();
                int curPage = this.GetCurPage();
                if (curPage == maxPage) this.FlashItem();
            }

            public void FlashItem()
            {
                if (this._instance == null) return;

                int idxStart = this.GetCurTop();

                for (int i=0; i<this._itemCountPerPage; i++)
                {
                    int idx = idxStart + i;
                    if (this._itemNameTable.ContainsKey(idx)) this.ShowChild(i, this._itemNameTable[idx]);
                    else this.HideChild(i);

                    this.SetCursor(this._objChildTable[i], (idx == this._selectItem));
                }

                var component = this._instance.GetComponent<ObjectTable>();
                Text compText = component.objectTable[2].GetComponent<Text>();
                compText.text = "<" + this.GetCurPage() + "/" + this.GetMaxPage() + ">";
            }

            public void SetActive(bool b)
            {
                this.bActiveEvent = b;
            }

            public void SetEventCB(RESULT result, EventKeyInfo info)
            {
                if (this._instance == null) return;

                Dictionary<RESULT, EventKeyInfo> table = this.GetEventProcTable();
                if (table == null) return;

                table.Add(result, info);
            }

            //------------------------------------------------------------------
            // ページ関連
            //------------------------------------------------------------------

            private int GetMaxPage()
            {
                return this.GetPage(this._itemCount - 1);
            }
            private int GetCurPage()
            {
                return this.GetPage(this._selectItem);
            }
            private int GetPage(int itemIdx)
            {
                return (itemIdx / this._itemCountPerPage) + 1;
            }

            private int GetCurTop()
            {
                return this.GetTop(this.GetCurPage());
            }
            private int GetTop(int page)
            {
                return (page - 1) * this._itemCountPerPage;
            }

            private int GetCurPageItemCount()
            {
                return this.GetItemCount(this.GetCurPage());
            }
            private int GetItemCount(int page)
            {
                int idxMaxPerPage = page * this._itemCountPerPage - 1;
                int delta = idxMaxPerPage - (this._itemCount - 1);
                return this._itemCountPerPage - ((delta < 0) ? 0 : (delta % this._itemCountPerPage));
            }

            private void ShowChild(int idx, string text)
            {
                this._objChildTable[idx].SetActive(true);
                this.SetChildText(this._objChildTable[idx], text);
            }

            private void HideChild(int idx)
            {
                this._objChildTable[idx].SetActive(false);
            }

            private void SetChildText(GameObject obj, string text)
            {
                var component = obj.GetComponent<ObjectTable>();
                if (component == null) return;

                GameObject objText = component.objectTable[0];
                Text compText = objText.GetComponent<Text>();
                if (compText == null) return;

                compText.text = text;
            }

            //------------------------------------------------------------------
            // ダイアログ生成処理
            //------------------------------------------------------------------

            private GameObject CreateDialog(
                GameObject prefabDialog, GameObject prefabItem, int itemCountPerPage)
            {
                GameObject objDialog = (GameObject)Object.Instantiate(prefabDialog);
                objDialog.SetActive(false);

                var component = objDialog.GetComponent<ObjectTable>();
                GameObject rootItem = component.objectTable[1];

                for (int i = 0; i < itemCountPerPage; i++)
                {
                    GameObject objItem = (GameObject)Object.Instantiate(prefabItem);
                    objItem.transform.SetParent(rootItem.transform, false);
                    objItem.SetActive(false);

                    this._objChildTable.Add(i, objItem);
                }

                this._itemCountPerPage = itemCountPerPage;

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
            // ItemNameTable 関連
            //------------------------------------------------------------------

            private void NewItemNameTable()
            {
                this._itemNameTable = new Dictionary<int, string>();
                this._itemCount = 0;
            }
            private void CleanItemNameTable()
            {
                if (this._itemNameTable != null) this._itemNameTable.Clear();
                this._itemCount = 0;
            }
            private void ReleaseItemNameTable()
            {
                this.CleanItemNameTable();
                this._itemNameTable = null;
            }

            private int RegistItem(string text)
            {
                int idx = this._itemCount;
                if (this._itemNameTable != null) this._itemNameTable.Add(this._itemCount++, text);
                return idx;
            }

            //------------------------------------------------------------------
            // ChildObjectTable 関連
            //------------------------------------------------------------------

            private void NewChildObjectTable()
            {
                this._objChildTable = new Dictionary<int, GameObject>();
            }
            private void CleanChildObjectTable()
            {
                if (this._objChildTable == null) return;
                this._objChildTable.Clear();
            }
            private void ReleaseChildObjectTable()
            {
                this.CleanChildObjectTable();
                this._objChildTable = null;
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
        // IEntityCommonList
        //------------------------------------------------------------------
        public interface IEntityCommonListDialog : IEntity
        {
            CommonListDialog Create(int itemCountPerPage, string title = "");
            void Release(CommonListDialog dlg);

        } //interface IEntityCommonListDialog


        //------------------------------------------------------------------
        // EntityCommonListDialog
        //------------------------------------------------------------------
        public class EntityCommonListDialog : NpEntity, IEntityCommonListDialog
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private GameObject prefabDialog;
            private GameObject prefabItem;
            private List<CommonListDialog> listTable;
            private List<CommonListDialog> listAddTable;
            private List<CommonListDialog> listReleaseTable;

            public interface ICommonListDialogAccessor
            {
                void Start(GameObject prefabDialog, GameObject prefabItem, int itemCountPerPage, string title = "");
                void Release();
                void EventProc();
                void SetActive(bool b);
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityCommonListDialog.StartProc()");

                this.prefabDialog = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_COMMON_LIST_DIALOG);
                this.prefabItem = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_COMMON_ITEM);
                this.listTable = new List<CommonListDialog>();
                this.listAddTable = new List<CommonListDialog>();
                this.listReleaseTable = new List<CommonListDialog>();

                this._bReadyLogic = true;
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this._bReadyLogic == false) return false;

                foreach (CommonListDialog dlg in this.listAddTable)
                {
                    this.listTable.Add(dlg);
                    dlg.SetActive(true);
                }
                this.listAddTable.Clear();

                foreach (CommonListDialog dlg in this.listReleaseTable)
                {
                    dlg.SetActive(false);
                    this.listTable.Remove(dlg);

                    this.ReleaseDialog(dlg);
                }
                this.listReleaseTable.Clear();

                foreach (CommonListDialog dlg in this.listTable)
                {
                    this.UpdateDialog(dlg);
                }

                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityCommonListDialog.TerminateProc()");

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

                if (this.prefabItem != null)
                {
                    //Resources.UnloadAsset(this.prefabItem);
                    this.prefabItem = null;
                }
                if (this.prefabDialog != null)
                {
                    //Resources.UnloadAsset(this.prefabDialog);
                    this.prefabDialog = null;
                }

                Resources.UnloadUnusedAssets();
            }

            private void ClearTable(List<CommonListDialog> list)
            {
                if (list == null) return;

                foreach (CommonListDialog dlg in list)
                {
                    if (dlg == null) continue;
                    this.ReleaseDialog(dlg);
                }
                list.Clear();
            }

            private void ReleaseDialog(CommonListDialog dlg)
            {
                ICommonListDialogAccessor acc = (ICommonListDialogAccessor)dlg;
                acc.Release();
            }

            private void UpdateDialog(CommonListDialog dlg)
            {
                ICommonListDialogAccessor acc = (ICommonListDialogAccessor)dlg;
                acc.EventProc();
            }

            //------------------------------------------------------------------
            // ダイアログ処理
            //------------------------------------------------------------------

            public CommonListDialog Create(int itemCountPerPage, string title = "")
            {
                if (this.IsReadyLogic() == false) return null;

                CommonListDialog dlg = new CommonListDialog();
                ICommonListDialogAccessor acc = (ICommonListDialogAccessor)dlg;
                acc.Start(this.prefabDialog, this.prefabItem, itemCountPerPage, title);

                this.listAddTable.Add(dlg);

                return dlg;
            }

            public void Release(CommonListDialog dlg)
            {
                if (this.IsReadyLogic() == false) return;

                CommonListDialog temp = this.listAddTable.Find(x => x == dlg);
                if (temp == null) temp = this.listTable.Find(x => x == dlg);
                if (temp == null) return;

                this.listReleaseTable.Add(dlg);
            }


        } //class EntityCommonListDialog

    } //namespace entity
} //namespace nangka
