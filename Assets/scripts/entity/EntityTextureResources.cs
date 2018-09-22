﻿using System.Collections;
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
            void InitLogic();
            void ReadyLogic();
            void Reset();
            void Clear();

            bool IsReadyLogic();

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
            // 初期化関連変数
            //------------------------------------------------------------------

            private bool _bInitializedLogic;
            private bool IsInitializedLogic() { return this._bInitializedLogic; }

            private Dictionary<string, Texture> cache = null;

            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool UpdateProc()
            {
                return false;
            }

            protected override void CleanUp()
            {
                Debug.Log("EntityTextureResourceManager.CleanUp()");

                this.ClearCache();
                this.cache = null;
            }


            //------------------------------------------------------------------
            // ロジック初期化処理／ロジック終了処理
            //------------------------------------------------------------------

            public void InitLogic()
            {
                if (this.IsInitializedLogic()) return;

                bool b = false;
                do {
                    this.cache = new Dictionary<string, Texture>();
                    if (this.cache == null) break;

                    b = true;
                }
                while (false);

                this._bInitializedLogic = b;
            }

            public void Clear()
            {
                if (!this.IsInitializedLogic()) return;

                this.Reset();

                this.cache = null;

                this._bInitializedLogic = false;
            }

            //------------------------------------------------------------------
            // ロジック準備処理／ロジックリセット処理
            //------------------------------------------------------------------

            public void ReadyLogic()
            {
                if (!this.IsInitializedLogic() || this.IsReadyLogic()) return;

                this._bReadyLogic = true;
            }

            public void Reset()
            {
                if (!this.IsInitializedLogic() || !this.IsReadyLogic()) return;

                this.ClearCache();

                this._bReadyLogic = false;
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
