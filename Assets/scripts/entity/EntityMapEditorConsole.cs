using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {
        public enum CONSOLE_MODE
        {
            INVALIDATE,
            HIDDEN,
            MAIN

        } //enum CONSOLE_MODE


        //------------------------------------------------------------------
        // IEntityMapEditorConsole
        //------------------------------------------------------------------
        public interface IEntityMapEditorConsole : IEntity
        {
            CONSOLE_MODE GetDetectedMode();
            void ChangeMode();

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
            private bool IsDetectedModeChange() { return (this._mode != this._tempMode); }

            public CONSOLE_MODE GetDetectedMode()
            {
                CONSOLE_MODE mode = CONSOLE_MODE.INVALIDATE;
                if (this.IsReadyLogic() && this.IsDetectedModeChange()) mode = this._tempMode;
                return mode;
            }

            public void ChangeMode()
            {
                if (this.IsReadyLogic() && this.IsDetectedModeChange())
                {
                    Debug.Log("ChangeMode "+this._mode+" to "+this._tempMode);
                    this.ChangeMode(this._mode, this._tempMode);
                    this._mode = this._tempMode;
                }
            }

            private delegate void EventProc();
            private EventProc funcEventProc;

            private delegate void ChangeModeProc();
            private ChangeModeProc funcChangeMode;

            private GameObject refObjNaviWindow;
            private GameObject refObjMainConsole;


            //------------------------------------------------------------------
            // Entity メイン処理
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
            // ロジック準備処理
            //------------------------------------------------------------------

            private IEnumerator ReadyLogic()
            {
                yield return SceneManager.LoadSceneAsync(Define.SCENE_NAME_MAPEDITOR_CONSOLE, LoadSceneMode.Additive);

                var scene = SceneManager.GetSceneByName(Define.SCENE_NAME_MAPEDITOR_CONSOLE);
                var canvas = scene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null).GetComponent<Canvas>();
                var component = canvas.GetComponent<ObjectTable>();

                this.refObjNaviWindow = component.objectTable[0];
                this.refObjMainConsole = component.objectTable[1];

                this._mode = CONSOLE_MODE.HIDDEN;
                this._tempMode = CONSOLE_MODE.HIDDEN;
                this.funcEventProc = this.EventProc_Hidden;

                this._bReadyLogic = true;
                yield return null;
            }

            private IEnumerator TerminateLogic()
            {
                yield return SceneManager.UnloadSceneAsync(Define.SCENE_NAME_MAPEDITOR_CONSOLE);

                this._bTerminating = false;
            }

            //------------------------------------------------------------------
            // ロジック更新処理
            //------------------------------------------------------------------

            private void UpdateLogic()
            {
                if (!this.IsReadyLogic()) return;

                if (this.funcEventProc != null) this.funcEventProc();
            }

            //------------------------------------------------------------------
            // イベント処理
            //------------------------------------------------------------------

            private void EventProc_Hidden()
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    this._tempMode = CONSOLE_MODE.MAIN;
                }
            }

            private void EventProc_Main()
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    this._tempMode = CONSOLE_MODE.HIDDEN;
                }
            }

            //------------------------------------------------------------------
            // モード変更処理
            //------------------------------------------------------------------

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
            }

            private void ChangeModeProc_MainToHidden()
            {
                this.refObjNaviWindow.SetActive(true);
                this.refObjMainConsole.SetActive(false);
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
                return func;
            }
            private ChangeModeProc GetChangeModeProc_ToMain(CONSOLE_MODE fromMode)
            {
                ChangeModeProc func = null;
                if (fromMode == CONSOLE_MODE.HIDDEN) func = this.ChangeModeProc_HiddenToMain;
                return func;
            }

        } //class EntityMapEditorConsole

    } //namespace entity
} //namespace nangka
