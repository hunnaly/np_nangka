using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityMapData
        //------------------------------------------------------------------
        public interface IEntityMapData : IEntity
        {
            void Reset();

            bool IsLoaded();
            string GetName();
            int GetWidth();
            int GetHeight();

            Texture[] GetBlockTexture(int x, int y);
            Texture GetTexture(int x, int y, Direction dir);
            bool IsMovable(int x, int y, Direction dir);
            bool IsOutOfRange(int x, int y);

            void Through(int x, int y, bool bThrough = true);
            bool IsThorough(int x, int y);

            EntityMapData GetOwnEntity();

        } //interface IEntityMapData


        //------------------------------------------------------------------
        // EntityMapData
        //------------------------------------------------------------------
        public class EntityMapData : NpEntity, IEntityMapData,
            EntityRecreator.IMapDataRecreator
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }


            private bool _bRecreating;
            
            private bool _bLoaded;
            public bool IsLoaded() { return this._bLoaded; }
            
            private string _name;
            public string GetName() { return this._name; }

            private int _width;
            public int GetWidth() { return this._width; }

            private int _height;
            public int GetHeight() { return this._height; }

            private Dictionary<byte, string> tableTexturePath;
            private Dictionary<byte, Texture> tableTexture; // 0 => null をダミー値としてもたせる
            private BlockData[] tableData;
            private bool[] tableThrough;

            public EntityMapData GetOwnEntity() { return this; }


            //------------------------------------------------------------------
            // データ設定メソッド（EntityNewCreator用）
            //------------------------------------------------------------------

            void EntityRecreator.IMapDataRecreator.Begin(string name, int width, int height)
            {
                if (this._bRecreating) return;

                this.Reset();

                this._bRecreating = true;
                this._bLoaded = false;

                this._name = name;
                this._width = width;
                this._height = height;

                int num = this._width * this._height;
                this.tableData = new BlockData[num];
                this.tableThrough = new bool[num];
            }

            void EntityRecreator.IMapDataRecreator.AddTexture(byte id, string path)
            {
                if (this._bRecreating == false) return;
                if (this.tableTexturePath == null) return;
                if (id == 0) return;

                this.tableTexturePath.Add(id, path);
            }

            void EntityRecreator.IMapDataRecreator.SetBlock(int idx, BlockData data)
            {
                if (this._bRecreating == false) return;
                if (this.tableData == null) return;
                if (idx < 0 || idx >= this._width * this._height) return;

                this.tableData[idx] = data;
            }

            void EntityRecreator.IMapDataRecreator.End()
            {
                if (this._bRecreating == false) return;

                if (this.tableTexturePath != null && this.tableTexture != null)
                {
                    IEntityTextureResources iTexRes = Utility.GetIEntityTextureResources();
                    foreach (KeyValuePair<byte, string> pair in this.tableTexturePath)
                    {
                        Texture tex = iTexRes.Load(pair.Value);
                        this.tableTexture.Add(pair.Key, tex);
                    }
                }

                this._bLoaded = true;
                this._bRecreating = false;
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMapData.StartProc()");

                // TODO: 例外エラー対応が必要
                this.tableTexturePath = new Dictionary<byte, string>();
                this.tableTexture = new Dictionary<byte, Texture>();
                this.tableTexture.Add(0, null);

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityMapData.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this.ClearThroughTable();
                this.tableThrough = null;

                this.ClearBlockTable();
                this.tableData = null;

                this.ClearTextureTable();
                this.tableTexturePath = null;
                this.tableTexture = null;
            }

            //------------------------------------------------------------------
            // ロジック準備処理／ロジックリセット処理
            //------------------------------------------------------------------

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                // 踏破情報
                this.ClearThroughTable();
                this.tableThrough = null;

                // コリジョン情報とデザイン情報
                this.ClearBlockTable();
                this.tableData = null;

                // テクスチャ情報
                this.ClearTextureTable();

                // 基本情報
                this._bRecreating = false;
                this._bLoaded = false;
                this._name = "";
                this._width = 0;
                this._height = 0;
            }

            private void ClearThroughTable()
            {
                if (this.tableThrough == null) return;
                for (int i = 0; i < this.tableThrough.Length; i++) { this.tableThrough[i] = false; }
            }

            private void ClearBlockTable()
            {
                if (this.tableData == null) return;
                for (int i = 0; i < this.tableData.Length; i++) { this.tableData[i].idTip = null; }
            }

            private void ClearTextureTable()
            {
                if (this.tableTexturePath != null)
                {
                    foreach (KeyValuePair<byte, string> pair in this.tableTexturePath)
                    {
                        Utility.GetIEntityTextureResources().Unload(pair.Value);
                    }
                    this.tableTexturePath.Clear();
                }

                if (this.tableTexture != null)
                {
                    this.tableTexture.Clear();
                    this.tableTexture.Add(0, null);
                }
            }


            //------------------------------------------------------------------
            // 処理
            //------------------------------------------------------------------

            public Texture[] GetBlockTexture(int x, int y)
            {
                if (this.IsOutOfRange(x, y)) return null;

                Texture[] tableTex = new Texture[(int)Direction.SOLID_MAX];
                for (int i = 0; i < (int)Direction.SOLID_MAX; i++)
                {
                    tableTex[i] = this.GetTextureReal(x, y, (Direction)i);
                }
                return tableTex;
            }

            public Texture GetTexture(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return null;
                return this.GetTextureReal(x, y, dir);
            }
            private Texture GetTextureReal(int x, int y, Direction dir)
            {
                int idx = y * this._width + x;
                return this.tableTexture[this.tableData[idx].idTip[(int)dir]];
            }

            public bool IsMovable(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return false;

                int idx = y * this._width + x;
                return ((this.tableData[idx].collision & (1 << (int)dir)) == 0);
            }

            public bool IsOutOfRange(int x, int y)
            {
                return (x < 0 || y < 0 || x >= this._width || y >= this._height) ? true : false;
            }

            public void Through(int x, int y, bool bThrough = true)
            {
                if (this.IsOutOfRange(x, y)) return;

                int idx = y * this._width + x;
                this.tableThrough[idx] = bThrough;
            }

            public bool IsThorough(int x, int y)
            {
                if (this.IsOutOfRange(x, y)) return false;

                int idx = y * this._width + x;
                return this.tableThrough[idx];
            }

            //------------------------------------------------------------------
            // BlockData
            //------------------------------------------------------------------
            [Serializable]
            public class BlockData : IDisposable
            {
                // Direction.SOLID_MAX 個のテクスチャIDを格納する
                // 各 Map ごとに 256種類まで利用可能
                public byte[] idTip;

                // Direction.* bit目の On/Off で障壁あり/なしをあらわす
                // Direction.PLANE_MAX 分(平面方向のみ)の情報をもつ
                public uint collision;


                //------------------------------------------------------------------
                // Clone
                //------------------------------------------------------------------
                public BlockData Clone()
                {
                    BlockData data = null;
                    using (data = new BlockData())
                    {
                        int tipSize = 0;
                        if (this.idTip != null) tipSize = this.idTip.Length;
                        if (tipSize > 0)
                        {
                            try { data.idTip = new byte[tipSize]; }
                            catch { data = null; }
                        }

                        Array.Copy(this.idTip, data.idTip, tipSize);
                        data.collision = this.collision;
                    }
                    return data;
                }

                //------------------------------------------------------------------
                // IDisposable.Dispose
                //------------------------------------------------------------------
                public void Dispose()
                {
                    this.idTip = null;
                }

            } //class BlockData

        } //class EntityMapData

    } //namespace entity
} //namespace nangka
