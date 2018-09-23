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
            bool IsReadyLogic();
            void Reset();

            Texture Load(string path);
            void Unload(string path);

            void Terminate();

        } //interface IEntityTextureResources


        //------------------------------------------------------------------
        // EntityTextureResources
        //------------------------------------------------------------------
        public class EntityTextureResources : NpEntity, IEntityTextureResources
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private Dictionary<string, Texture> cache = null;

            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityTextureResourceManager.StartProc()");

                // TODO: 例外エラー対応を行う必要がある
                this.cache = new Dictionary<string, Texture>();
                this._bReadyLogic = true;

                return true;
            }

            protected override bool UpdateProc()
            {
                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityTextureResourceManager.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                Debug.Log("EntityTextureResourceManager.CleanUp()");

                this.ClearCache();
                this.cache = null;
            }


            //------------------------------------------------------------------
            // ロジック準備処理／ロジックリセット処理
            //------------------------------------------------------------------

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                this.ClearCache();
            }

            private void ClearCache()
            {
                if (this.cache != null)
                {
                    List<string> keyList = new List<string>(this.cache.Keys);
                    foreach (string key in keyList)
                    {
                        this.UnloadAndRemove(key);
                    }
                    this.cache.Clear();
                }
            }


            //------------------------------------------------------------------
            // ロード処理 / アンロード処理
            //------------------------------------------------------------------

            public Texture Load(string path)
            {
                if (!this.IsReadyLogic()) return null;

                if (this.Exist(path)) return this.Get(path);

                Texture tex = Resources.Load<Texture>(path);
                if (tex != null) this.Add(path, tex);
                return tex;
            }

            public void Unload(string path)
            {
                if (!this.IsReadyLogic()) return;

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
