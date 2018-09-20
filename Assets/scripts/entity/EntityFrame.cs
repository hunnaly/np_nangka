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
            bool IsInitialized();
            void Terminate();

        } //interface EntityFrame


        //------------------------------------------------------------------
        // EntityFrame
        //------------------------------------------------------------------
        public class EntityFrame : NpEntity, IEntityFrame
        {
            private bool bInitialized = false;
            public bool IsInitialized() { return this.bInitialized; }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityFrame.StartProc()");
                this.bInitialized = false;
                Utility.StartCoroutine(this.Ready());
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this.bInitialized == false) return false;
                return false;
            }

            protected override bool TerminateProc()
            {
                this.bInitialized = false;
                return true;
            }

            protected override void CleanUp()
            {
                Debug.Log("EntityFrame.CleanUp()");
            }


            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator Ready()
            {
                yield return SceneManager.LoadSceneAsync(Define.SCENE_NAME_FRAME, LoadSceneMode.Additive);

                this.bInitialized = true;
                yield return null;
            }

        }

    }
}
