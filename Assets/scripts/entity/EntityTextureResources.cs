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
        // IEntityTextureResources
        //------------------------------------------------------------------
        public interface IEntityTextureResources
        {
            bool IsInitialized();
            Texture Load(string path);
            void Unload(string path);
            void Terminate();

        } //interface IEntityTextureResources


        //------------------------------------------------------------------
        // EntityTextureResources
        //------------------------------------------------------------------
        public class EntityTextureResources : NpEntity, IEntityTextureResources
        {
            private bool bInitialized = false;
            public bool IsInitialized() { return this.bInitialized; }

            private Dictionary<string, Texture> cache = null;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityTextureResourceManager.StartProc()");
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
                return true;
            }

            protected override void CleanUp()
            {
                Debug.Log("EntityTextureResourceManager.CleanUp()");

                if (this.cache != null)
                {
                    List<string> keyList = new List<string>(this.cache.Keys);
                    foreach (string key in keyList)
                    {
                        this.UnloadAndRemove(key);
                    }
                    this.cache = null;
                }
            }


            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator Ready()
            {
                this.cache = new Dictionary<string, Texture>();
                this.bInitialized = true;
                yield return null;
            }


            //------------------------------------------------------------------
            // IEntityTextureResources
            //------------------------------------------------------------------

            public Texture Load(string path)
            {
                if (this.Exist(path)) return this.Get(path);

                Texture tex = Resources.Load<Texture>(path);
                if (tex != null) this.Add(path, tex);
                return tex;
            }

            public void Unload(string path)
            {
                if (!this.Exist(path)) return;
                this.UnloadAndRemove(path);
            }


            //------------------------------------------------------------------
            // 内部処理
            //------------------------------------------------------------------

            private void UnloadAndRemove(string path)
            {
                Texture tex = this.Get(path);
                if (tex != null)
                {
                    Resources.UnloadAsset(this.Get(path));
                }
                this.Remove(path);
            }

            private void Add(string key, Texture value)
            {
                this.cache.Add(key, value);
            }

            private void Remove(string path)
            {
                this.cache.Remove(path);
            }

            private Texture Get(string key)
            {
                return this.cache[key];
            }

            private bool Exist(string key)
            {
                return this.cache.ContainsKey(key);
            }
        }

    }
}
