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
        public enum DEV_ITEM
        {
            NONE,
            MAP_EDITOR,
            DUNGEON_TEST
        }

        //------------------------------------------------------------------
        // IEntityDevEntrance
        //------------------------------------------------------------------
        public interface IEntityDevEntrance : IEntity
        {
            DEV_ITEM GetSelected();

        } //interface IEntityDevEntrance


        //------------------------------------------------------------------
        // EntityDevEntrance
        //------------------------------------------------------------------
        public class EntityDevEntrance : NpEntity, IEntityDevEntrance
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private bool _bTerminating;

            private DEV_ITEM _selected;
            public DEV_ITEM GetSelected() { return this._selected; }
            private bool IsSelected() { return (this._selected != DEV_ITEM.NONE); }

            private GameObject _refButtonMapEditor;
            private GameObject _refButtonDungeonTest;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                this._selected = DEV_ITEM.NONE;

                Utility.StartCoroutine(this.ReadyLogic());
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this._bReadyLogic == false) return false;
                return false;
            }

            protected override bool TerminateProc()
            {
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
                Debug.Log("EntityDevEntrance.CleanUp()");
            }

            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator ReadyLogic()
            {
                yield return SceneManager.LoadSceneAsync(Define.SCENE_NAME_DEV_ENTRANCE, LoadSceneMode.Additive);

                var scene = SceneManager.GetSceneByName(Define.SCENE_NAME_DEV_ENTRANCE);
                var canvas = scene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null).GetComponent<Canvas>();
                var component = canvas.GetComponent<ObjectTable>();

                // MapEditor
                this._refButtonMapEditor = component.objectTable[0];
                Button compBtn = this._refButtonMapEditor.GetComponent<Button>();
                if (compBtn != null) { compBtn.onClick.AddListener(this.OnClickButtonMapEditor); }

                // DungeonTest
                this._refButtonDungeonTest = component.objectTable[1];
                compBtn = this._refButtonDungeonTest.GetComponent<Button>();
                if (compBtn != null) { compBtn.onClick.AddListener(this.OnClickButtonDungeonTest); }

                this._bReadyLogic = true;
                yield return null;
            }

            private IEnumerator TerminateLogic()
            {
                Button compBtn = this._refButtonMapEditor.GetComponent<Button>();
                if (compBtn != null) { compBtn.onClick.RemoveAllListeners(); }

                compBtn = this._refButtonDungeonTest.GetComponent<Button>();
                if (compBtn != null) { compBtn.onClick.RemoveAllListeners(); }

                yield return SceneManager.UnloadSceneAsync(Define.SCENE_NAME_DEV_ENTRANCE);

                this._bTerminating = false;
            }

            public void OnClickButtonMapEditor()
            {
                // 二重選択禁止チェック
                if (this.IsSelected()) return;

                this._selected = DEV_ITEM.MAP_EDITOR;
            }

            public void OnClickButtonDungeonTest()
            {
                // 二重選択禁止チェック
                if (this.IsSelected()) return;

                this._selected = DEV_ITEM.DUNGEON_TEST;
            }

        } //class EntityDevEntrance

    } //namespace entity
} //namespace nangka
