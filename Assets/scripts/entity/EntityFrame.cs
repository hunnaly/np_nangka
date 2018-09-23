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
        // IEntityFrame
        //------------------------------------------------------------------
        public interface IEntityFrame
        {
            bool IsReadyLogic();
            void Terminate();

        } //interface EntityFrame


        //------------------------------------------------------------------
        // EntityFrame
        //------------------------------------------------------------------
        public class EntityFrame : NpEntity, IEntityFrame
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private bool _bTerminating;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
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
                Debug.Log("EntityFrame.CleanUp()");
            }

            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator ReadyLogic()
            {
                yield return SceneManager.LoadSceneAsync(Define.SCENE_NAME_FRAME, LoadSceneMode.Additive);

                this._bReadyLogic = true;
                yield return null;
            }

            private IEnumerator TerminateLogic()
            {
                yield return SceneManager.UnloadSceneAsync(Define.SCENE_NAME_FRAME);

                this._bTerminating = false;
            }

        }

    }
}
