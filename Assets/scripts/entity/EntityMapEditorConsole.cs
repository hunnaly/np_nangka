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
        // CONSOLE_MODE
        //------------------------------------------------------------------
        public enum CONSOLE_MODE
        {
            INVALIDATE, // 無効なモード（モード変更検出処理において未変更を表す値）

            HIDDEN,     // コンソールが隠れている状態（初期状態）
            MAIN,       // メインコンソールが表示され操作可能な状態
            DECIDED     // メインコンソール上でアイテムを決定した状態

        } //enum CONSOLE_MODE

        //------------------------------------------------------------------
        // MAIN_CONSOLE_ITEM
        //------------------------------------------------------------------
        public enum MAIN_CONSOLE_ITEM : int
        {
            MAP_NEW,                // 新規マップ作成
            MAP_LOAD,               // 既存のマップをロード
            MAP_SAVE,               // 編集中のマップをセーブ
            NAVI_SWITCH,            // ナビゲーションウィンドウの表示切替
            WALL_THROUGH_SWITCH,    // 状態ウィンドウの表示切替
            BOTH_SIDE_SWITCH,       // 壁の生成／破棄を両面で行うかどうかの切替
            RETURN_TO_DEV,          // 開発エントランスへ戻る

            MAX,
            NONE,                   // 未決定時
        } //enum MAIN_CONSOLE_ITEM


        //------------------------------------------------------------------
        // IEntityMapEditorConsole
        //------------------------------------------------------------------
        public interface IEntityMapEditorConsole : IEntity
        {
            CONSOLE_MODE GetDetectedMode();
            MAIN_CONSOLE_ITEM GetDecidedItem();
            void Cancel(bool bHide);
            void ChangeMode();
            Transform GetRootCanvasTransform();
            void FlashWallState();

        } //interface IEntityMapEditorConsole


        //------------------------------------------------------------------
        // EntityMapEditorConsole
        //------------------------------------------------------------------
        public class EntityMapEditorConsole : NpEntity, IEntityMapEditorConsole
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private bool _bTerminating;

            private CONSOLE_MODE _mode;
            private CONSOLE_MODE _tempMode;

            private delegate void EventProc();
            private EventProc funcEventProc;

            private delegate void ChangeModeProc();
            private ChangeModeProc funcChangeMode;

            private GameObject refObjNaviWindow;
            private GameObject refObjMainConsole;
            private GameObject refObjStateWindow;
            private GameObject refObjWallStateWindow;

            private MAIN_CONSOLE_ITEM _selectItem;
            private MAIN_CONSOLE_ITEM _decidedItem;
            public MAIN_CONSOLE_ITEM GetDecidedItem() { return this._decidedItem; }

            private GameObject _rootCanvas;
            public Transform GetRootCanvasTransform() { return this._rootCanvas.transform; }

            private bool _bShowNavi;
            private bool _bThroughWall;
            private bool _bBothSideChangeWall;


            public interface IMapDataAccessor
            {
                void ThroughWall(bool bThrough);
                bool ChangeWall(int x, int y, Direction dir, bool bBothSide);
                EntityMapData.WALL_STATE GetWallState(int x, int y, Direction dir);
            }

            public interface IDungeonAccessor
            {
                void SetOnMoveProc(EntityDungeon.OnMoveProc func);
                void ResetOnMoveProc();
            }


            //------------------------------------------------------------------
            // Entity ロジック処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMapEditorConsole.StartProc()");

                Utility.StartCoroutine(this.ReadyLogic());
                return true;
            }

            protected override bool UpdateProc()
            {
                this.UpdateLogic();
                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityMapEditorConsole.TerminateProc()");

                if (this._bReadyLogic)
                {
                    this._bReadyLogic = false;
                    this._bTerminating = true;
                    Utility.StartCoroutine(this.TerminateLogic());
                }
                return (this._bTerminating == false);
            }

            protected override void CleanUp()
            {
            }

            //------------------------------------------------------------------
            // ロジック内部処理
            //------------------------------------------------------------------

            private IEnumerator ReadyLogic()
            {
                yield return SceneManager.LoadSceneAsync(Define.SCENE_NAME_MAPEDITOR_CONSOLE, LoadSceneMode.Additive);

                var scene = SceneManager.GetSceneByName(Define.SCENE_NAME_MAPEDITOR_CONSOLE);
                this._rootCanvas = scene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null);
                var canvas = this._rootCanvas.GetComponent<Canvas>();
                var component = canvas.GetComponent<ObjectTable>();

                this.refObjNaviWindow = component.objectTable[0];
                this.refObjMainConsole = component.objectTable[1];
                this.refObjStateWindow = component.objectTable[2];
                this.refObjWallStateWindow = component.objectTable[3];

                this._mode = CONSOLE_MODE.HIDDEN;
                this._tempMode = CONSOLE_MODE.HIDDEN;
                this.funcEventProc = this.EventProc_Hidden;

                this.Select(MAIN_CONSOLE_ITEM.MAP_NEW);
                this._decidedItem = MAIN_CONSOLE_ITEM.NONE;

                this._bShowNavi = true;
                this.FlashNavi();

                this._bThroughWall = false;
                this.FlashWallThrough();

                this._bBothSideChangeWall = true;
                this.FlashBothSideChangeWall();

                IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                IDungeonAccessor acc = (IDungeonAccessor)(iDungeon.GetOwnEntity());
                acc.SetOnMoveProc(this.OnMoveProc);

                this._bReadyLogic = true;
                yield return null;
            }

            private IEnumerator TerminateLogic()
            {
                yield return SceneManager.UnloadSceneAsync(Define.SCENE_NAME_MAPEDITOR_CONSOLE);

                this._bTerminating = false;
            }

            private void UpdateLogic()
            {
                if (!this.IsReadyLogic()) return;

                if (this.funcEventProc != null) this.funcEventProc();
            }

            //------------------------------------------------------------------
            // 各モードごとのイベント処理
            //------------------------------------------------------------------

            private void EventProc_Hidden()
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    this._tempMode = CONSOLE_MODE.MAIN;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    this.ChangeWall();
                    this.FlashWallState();
                }
            }

            private void EventProc_Main()
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    this._tempMode = CONSOLE_MODE.HIDDEN;
                }
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    this.SelectUp();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    this.SelectDown();
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    this._decidedItem = this._selectItem;
                    this._tempMode = CONSOLE_MODE.DECIDED;
                }

                this.SwitchProc();
            }

            private void SwitchProc()
            {
                if (this.GetDetectedMode() != CONSOLE_MODE.DECIDED) return;
                switch (this.GetDecidedItem())
                {
                    case MAIN_CONSOLE_ITEM.NAVI_SWITCH: this.SwitchNavi(); break;
                    case MAIN_CONSOLE_ITEM.WALL_THROUGH_SWITCH: this.SwitchWallThrough(); break;
                    case MAIN_CONSOLE_ITEM.BOTH_SIDE_SWITCH: this.SwitchBothSideChangeWall(); break;
                    default: break;
                }
            }

            private void SwitchNavi()
            {
                this._bShowNavi ^= true;
                this.FlashNavi();
                this.Cancel(false);
            }
            private void FlashNavi()
            {
                var component = this.refObjStateWindow.GetComponent<ObjectTable>();
                component = component.objectTable[0].GetComponent<ObjectTable>();
                Text compText = component.objectTable[0].GetComponent<Text>();
                if (compText != null)
                {
                    compText.text = "Navigation " + (this._bShowNavi ? "ON" : "OFF");
                }
            }

            private void SwitchWallThrough()
            {
                this._bThroughWall ^= true;
                this.FlashWallThrough();
                this.Cancel(false);
            }
            private void FlashWallThrough()
            {
                var component = this.refObjStateWindow.GetComponent<ObjectTable>();
                component = component.objectTable[1].GetComponent<ObjectTable>();
                Text compText = component.objectTable[0].GetComponent<Text>();
                if (compText != null)
                {
                    compText.text = "Through-Wall " + (this._bThroughWall ? "ON" : "OFF");
                }

                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IMapDataAccessor acc = (IMapDataAccessor)(iMapData.GetOwnEntity());
                acc.ThroughWall(this._bThroughWall);
            }

            private void SwitchBothSideChangeWall()
            {
                this._bBothSideChangeWall ^= true;
                this.FlashBothSideChangeWall();
                this.Cancel(false);
            }
            private void FlashBothSideChangeWall()
            {
                var component = this.refObjStateWindow.GetComponent<ObjectTable>();
                component = component.objectTable[2].GetComponent<ObjectTable>();
                Text compText = component.objectTable[0].GetComponent<Text>();
                if (compText != null)
                {
                    compText.text = "BothSide-ChangeWall " + (this._bBothSideChangeWall ? "ON" : "OFF");
                }
            }

            //------------------------------------------------------------------
            // 壁操作処理
            //------------------------------------------------------------------

            private void ChangeWall()
            {
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IMapDataAccessor acc = (IMapDataAccessor)(iMapData.GetOwnEntity());

                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();
                bool bChanged = acc.ChangeWall(iPlayerData.GetX(), iPlayerData.GetY(), iPlayerData.GetDir(), this._bBothSideChangeWall);

                if (bChanged)
                {
                    this.FlashBlock(0, 0, iPlayerData.GetDir());
                    if (this._bBothSideChangeWall) this.FlashAdvanceBlock();
                }
            }

            public void FlashWallState()
            {
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IMapDataAccessor acc = (IMapDataAccessor)(iMapData.GetOwnEntity());

                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();
                EntityMapData.WALL_STATE state = acc.GetWallState(iPlayerData.GetX(), iPlayerData.GetY(), iPlayerData.GetDir());

                this.SetWallState(state);
            }

            private void SetWallState(EntityMapData.WALL_STATE state)
            {
                bool bWall = false;
                bool bCollision = false;
                switch (state)
                {
                    case EntityMapData.WALL_STATE.WALL: bWall = true; bCollision = true; break;
                    case EntityMapData.WALL_STATE.WALL_WITHOUT_COLLISION: bWall = true; break;
                    case EntityMapData.WALL_STATE.NO_WALL_WITH_COLLISION: bCollision = true; break;
                    default: break;
                }

                var component = this.refObjWallStateWindow.GetComponent<ObjectTable>();
                var tempComp = component.objectTable[0].GetComponent<ObjectTable>();
                Text textWall = tempComp.objectTable[0].GetComponent<Text>();

                tempComp = component.objectTable[1].GetComponent<ObjectTable>();
                Text textCollision = tempComp.objectTable[0].GetComponent<Text>();

                textWall.text = "Wall " + (bWall ? "ON" : "OFF");
                textCollision.text = "Collision " + (bCollision ? "ON" : "OFF");
            }

            private void FlashAdvanceBlock()
            {
                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();
                Direction dir = iPlayerData.GetDir();

                int dx = 0;
                int dy = 0;
                switch (dir)
                {
                    case Direction.NORTH: --dy; break;
                    case Direction.SOUTH: ++dy; break;
                    case Direction.EAST: ++dx; break;
                    case Direction.WEST: --dx; break;
                    default: break;
                }

                this.FlashBlock(dx, dy, Utility.GetOppositeDirection(dir));
            }

            private void FlashBlock(int dx, int dy, Direction dir)
            {
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();

                // Structure の更新
                IEntityStructure iStructure = Utility.GetIEntityStructure();
                Texture tex = iMapData.GetTexture(iPlayerData.GetX() + dx, iPlayerData.GetY() + dy, dir);
                iStructure.ChangeWall(dx, dy, dir, tex);

                // MiniMap の更新
                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Flash(iMapData, iPlayerData.GetX() + dx, iPlayerData.GetY() + dy, true);
            }

            private void OnMoveProc()
            {
                this.FlashWallState();
            }

            //------------------------------------------------------------------
            // メインコンソール操作処理
            //------------------------------------------------------------------

            private void Select(MAIN_CONSOLE_ITEM item)
            {
                this.SwitchCursor(this._selectItem, false);
                this.SwitchCursor(item, true);
                this._selectItem = item;
            }

            private void SelectUp()
            {
                int temp = (int)this._selectItem - 1;
                if (temp < 0) temp = (int)MAIN_CONSOLE_ITEM.MAX - 1;
                MAIN_CONSOLE_ITEM item = (MAIN_CONSOLE_ITEM)(temp % (int)MAIN_CONSOLE_ITEM.MAX);
                this.Select(item);
            }

            private void SelectDown()
            {
                MAIN_CONSOLE_ITEM item = (MAIN_CONSOLE_ITEM)(((int)this._selectItem + 1) % (int)MAIN_CONSOLE_ITEM.MAX);
                this.Select(item);
            }

            private void SwitchCursor(MAIN_CONSOLE_ITEM item, bool bOn)
            {
                var component = this.refObjMainConsole.GetComponent<ObjectTable>();
                Image image = component.objectTable[(int)item].GetComponent<Image>();
                image.color = new Color(255.0f, 255.0f, (bOn ? 0.0f : 255.0f), 255.0f);
            }

            //------------------------------------------------------------------
            // モード変更関連処理
            //------------------------------------------------------------------

            private bool IsDetectedModeChange() { return (this._mode != this._tempMode); }

            public CONSOLE_MODE GetDetectedMode()
            {
                CONSOLE_MODE mode = CONSOLE_MODE.INVALIDATE;
                if (this.IsReadyLogic() && this.IsDetectedModeChange()) mode = this._tempMode;
                return mode;
            }

            public void Cancel(bool bHide)
            {
                this._tempMode = bHide ? CONSOLE_MODE.HIDDEN : CONSOLE_MODE.MAIN;
                this.ChangeMode();
            }

            public void ChangeMode()
            {
                if (this.IsReadyLogic() && this.IsDetectedModeChange())
                {
                    Debug.Log("ChangeMode " + this._mode + " to " + this._tempMode);
                    this.ChangeMode(this._mode, this._tempMode);
                    this._mode = this._tempMode;
                }
            }

            private void ChangeMode(CONSOLE_MODE fromMode, CONSOLE_MODE toMode)
            {
                this.funcEventProc = GetEventProc(toMode);

                ChangeModeProc func = this.GetChangeModeProc(fromMode, toMode);
                if (func != null) func();
            }

            private void ChangeModeProc_HiddenToMain()
            {
                this.refObjNaviWindow.SetActive(false);
                this.refObjMainConsole.SetActive(true);
                this.refObjWallStateWindow.SetActive(false);
            }

            private void ChangeModeProc_MainToHidden()
            {
                this.refObjNaviWindow.SetActive(this._bShowNavi);
                this.refObjMainConsole.SetActive(false);

                this.refObjWallStateWindow.SetActive(true);
                this.FlashWallState();
            }

            private void ChangeModeProc_DecidedToMain()
            {
                this._decidedItem = MAIN_CONSOLE_ITEM.NONE;
            }
            private void ChangeModeProc_DecidedToHidden()
            {
                if ((this._decidedItem == MAIN_CONSOLE_ITEM.MAP_NEW)
                    || (this._decidedItem == MAIN_CONSOLE_ITEM.MAP_LOAD))
                {
                    this.FlashWallThrough();
                }

                this.ChangeModeProc_DecidedToMain();
                this.ChangeModeProc_MainToHidden();
            }

            //------------------------------------------------------------------
            // 各種デリゲート取得
            //------------------------------------------------------------------

            private EventProc GetEventProc(CONSOLE_MODE mode)
            {
                EventProc func = null;
                switch (mode)
                {
                    case CONSOLE_MODE.HIDDEN: func = this.EventProc_Hidden; break;
                    case CONSOLE_MODE.MAIN: func = this.EventProc_Main; break;
                    default: break;
                }
                return func;
            }

            private ChangeModeProc GetChangeModeProc(CONSOLE_MODE fromMode, CONSOLE_MODE toMode)
            {
                ChangeModeProc func = null;
                switch (toMode)
                {
                    case CONSOLE_MODE.HIDDEN: func = this.GetChangeModeProc_ToHidden(fromMode); break;
                    case CONSOLE_MODE.MAIN: func = this.GetChangeModeProc_ToMain(fromMode); break;
                    default: break;
                }
                return func;
            }
            private ChangeModeProc GetChangeModeProc_ToHidden(CONSOLE_MODE fromMode)
            {
                ChangeModeProc func = null;
                if (fromMode == CONSOLE_MODE.MAIN) func = this.ChangeModeProc_MainToHidden;
                else if (fromMode == CONSOLE_MODE.DECIDED) func = this.ChangeModeProc_DecidedToHidden;
                return func;
            }
            private ChangeModeProc GetChangeModeProc_ToMain(CONSOLE_MODE fromMode)
            {
                ChangeModeProc func = null;
                if (fromMode == CONSOLE_MODE.HIDDEN) func = this.ChangeModeProc_HiddenToMain;
                else if (fromMode == CONSOLE_MODE.DECIDED) func = this.ChangeModeProc_DecidedToMain;
                return func;
            }

        } //class EntityMapEditorConsole

    } //namespace entity
} //namespace nangka
