using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
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
            void SetName(string name);
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
            EntityRecreator.IMapDataRecreator,
            EntityMapEditorConsole.IMapDataAccessor,
            EntitySaveMap.IMapDataAccessor
        {
            //------------------------------------------------------------------
            // MapData
            //------------------------------------------------------------------
            [Serializable]
            public class MapData : IMapRawDataAccessor
            {
                private string _name;
                public string GetName() { return this._name; }
                public void SetName(string name) { this._name = name; }

                private int _width;
                public int GetWidth() { return this._width; }

                private int _height;
                public int GetHeight() { return this._height; }

                [Serializable]
                public class TextureInfo
                {
                    private byte _idx;
                    public byte idx { get { return this._idx; } }

                    private string _filePathName;
                    public string filePathName { get { return this._filePathName; } }

                    private string _typeName;
                    public string typeName { get { return this._typeName; } }

                    public TextureInfo() : this(0, null, "None") { }

                    public TextureInfo(byte idx, string filePathName, string typeName)
                    {
                        this._idx = idx;
                        this._filePathName = filePathName;
                        this._typeName = typeName;
                    }
                }
                private Dictionary<byte, TextureInfo> _tableTextureInfo;

                private BlockData[] _tableBlockData;


                //------------------------------------------------------------------
                // EntityMapData からのアクセス用
                //------------------------------------------------------------------
                void IMapRawDataAccessor.SetName(string name) { this.SetName(name); }
                void IMapRawDataAccessor.SetWidth(int width) { this._width = width; }
                void IMapRawDataAccessor.SetHeight(int height) { this._height = height; }

                BlockData[] IMapRawDataAccessor.GetBlockData() { return this._tableBlockData; }
                BlockData[] IMapRawDataAccessor.NewBlockData(int num) { return (this._tableBlockData = new BlockData[num]); }
                void IMapRawDataAccessor.ClearBlockData() { this.ClearBlockData(); }
                void IMapRawDataAccessor.ReleaseBlockData() { this.ClearBlockData(); this._tableBlockData = null; }

                private void ClearBlockData()
                {
                    if (this._tableBlockData == null) return;
                    for (int i = 0; i < this._tableBlockData.Length; i++) { this._tableBlockData[i].idTip = null; }
                }

                Dictionary<byte, TextureInfo> IMapRawDataAccessor.GetTextureInfoTable() { return this._tableTextureInfo; }
                Dictionary<byte, TextureInfo> IMapRawDataAccessor.NewTextureInfoTable() { return (this._tableTextureInfo = new Dictionary<byte, TextureInfo>()); }
                void IMapRawDataAccessor.ClearTextureInfoTable() { this.ClearTextureInfoTable(); }
                void IMapRawDataAccessor.ReleaseTextureInfoTable() { this.ClearTextureInfoTable(); this._tableTextureInfo = null; }

                private void ClearTextureInfoTable()
                {
                    if (this._tableTextureInfo == null) return;
                    this._tableTextureInfo.Clear();
                }

            } //class MapData

            public interface IMapRawDataAccessor
            {
                void SetName(string name);
                void SetWidth(int width);
                void SetHeight(int height);

                BlockData[] GetBlockData();
                BlockData[] NewBlockData(int num);
                void ClearBlockData();
                void ReleaseBlockData();

                Dictionary<byte, MapData.TextureInfo> GetTextureInfoTable();
                Dictionary<byte, MapData.TextureInfo> NewTextureInfoTable();
                void ClearTextureInfoTable();
                void ReleaseTextureInfoTable();

            } //interface IMapRawDataAccessor


            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }


            private bool _bRecreating;
            public bool IsLoaded() { return (this._bRecreating == false); }

            private MapData _data;
            public void SetName(string name) { this._data.SetName(name); }
            public string GetName() { return this._data.GetName(); }
            public int GetWidth() { return this._data.GetWidth(); }
            public int GetHeight() { return this._data.GetHeight(); }

            private Dictionary<byte, Texture> tableTexture; // 0 => null をダミー値としてもたせる
            private bool[] tableThrough;

            private bool _bThroughWall;

            public EntityMapData GetOwnEntity() { return this; }


            //------------------------------------------------------------------
            // データ設定メソッド（EntitySaveMap用）
            //------------------------------------------------------------------

            void EntitySaveMap.IMapDataAccessor.Save(string fileName)
            {
                string path = Define.GetMapFilePath();
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                string fullPath = path + "/" + fileName;

                using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, this._data);
                }
            }

            //------------------------------------------------------------------
            // データ設定メソッド（EntityNewCreator用）
            //------------------------------------------------------------------

            void EntityRecreator.IMapDataRecreator.DeepCopy(MapData data)
            {
                if (this._bRecreating) return;

                this.Reset();
                this._bRecreating = true;

                this._data = data;
                int num = this._data.GetWidth() * this._data.GetHeight();
                this.tableThrough = new bool[num];
                this.ReadyTexture();

                this._bRecreating = false;
            }

            void EntityRecreator.IMapDataRecreator.Begin(string name, int width, int height)
            {
                if (this._bRecreating) return;

                this.Reset();
                this._bRecreating = true;

                int num = width * height;

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                acc.SetName(name);
                acc.SetWidth(width);
                acc.SetHeight(height);
                acc.NewTextureInfoTable();
                acc.NewBlockData(num);

                this.tableThrough = new bool[num];
            }

            void EntityRecreator.IMapDataRecreator.AddTexture(byte idx, string fileName, string typeName)
            {
                if (this._bRecreating == false) return;

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                Dictionary<byte, MapData.TextureInfo> table = acc.GetTextureInfoTable();
                if (table == null) return;
                if (idx == 0) return;

                MapData.TextureInfo info = new MapData.TextureInfo(idx, fileName, typeName);
                table.Add(idx, info);
            }

            void EntityRecreator.IMapDataRecreator.SetBlock(int idx, BlockData data)
            {
                if (this._bRecreating == false) return;

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                BlockData[] table = acc.GetBlockData();
                if (table == null) return;

                int width = this._data.GetWidth();
                int height = this._data.GetHeight();
                if (idx < 0 || idx >= width * height) return;

                table[idx] = data;
            }

            void EntityRecreator.IMapDataRecreator.End()
            {
                if (this._bRecreating == false) return;
                if (this.tableTexture == null) return;

                this.ReadyTexture();

                this._bRecreating = false;
            }

            private void ReadyTexture()
            {
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                Dictionary<byte, MapData.TextureInfo> table = acc.GetTextureInfoTable();
                if (table == null) return;

                IEntityTextureResources iTexRes = Utility.GetIEntityTextureResources();
                foreach (KeyValuePair<byte, MapData.TextureInfo> pair in table)
                {
                    MapData.TextureInfo info = pair.Value;
                    Texture tex = iTexRes.Load(info.filePathName);
                    this.tableTexture.Add(info.idx, tex);
                }
            }


            //------------------------------------------------------------------
            // データ設定メソッド（EntityMapEditorConsole用）
            //------------------------------------------------------------------

            void EntityMapEditorConsole.IMapDataAccessor.ThroughWall(bool bThrough)
            {
                this._bThroughWall = bThrough;
            }

            // Texture に登録されている順に切り替えていく
            // bBothSide = true のときは、両サイドに対して切替処理を行う
            bool EntityMapEditorConsole.IMapDataAccessor.ChangeWall(int x, int y, Direction dir, bool bBothSide)
            {
                bool bChanged = ((this.IsOutOfRange(x, y) == false) && this.CheckMoveInRange(x, y, dir));
                if (bChanged)
                {
                    IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                    BlockData[] table = acc.GetBlockData();

                    this.ChangeWall(this.GetBlock(table, x, y), dir);
                    if (bBothSide)
                    {
                        this.ChangeWall(this.GetAdvanceBlock(table, x, y, dir), Utility.GetOppositeDirection(dir));
                    }
                }
                return bChanged;
            }

            bool EntityMapEditorConsole.IMapDataAccessor.ChangeCollision(int x, int y, Direction dir, bool bBothSide)
            {
                bool bChanged = ((this.IsOutOfRange(x, y) == false) && this.CheckMoveInRange(x, y, dir));
                if (bChanged)
                {
                    IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                    BlockData[] table = acc.GetBlockData();

                    this.ChangeCollision(this.GetBlock(table, x, y), dir);
                    if (bBothSide)
                    {
                        this.ChangeCollision(this.GetAdvanceBlock(table, x, y, dir), Utility.GetOppositeDirection(dir));
                    }
                }
                return bChanged;
            }

            string EntityMapEditorConsole.IMapDataAccessor.GetWallTypeName(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return null;

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                BlockData[] tableBlock = acc.GetBlockData();
                byte idx = this.GetWallType(this.GetBlock(tableBlock, x, y), dir);

                Dictionary<byte, MapData.TextureInfo> tableTextureInfo = acc.GetTextureInfoTable();
                string name = "none";
                if (tableTextureInfo.ContainsKey(idx))
                {
                    name = tableTextureInfo[idx].typeName;
                }
                return name;
            }

            bool EntityMapEditorConsole.IMapDataAccessor.GetWallCollision(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return false;

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                BlockData[] tableBlock = acc.GetBlockData();
                BlockData data = this.GetBlock(tableBlock, x, y);

                return ((data.collision & (uint)(1 << (int)dir)) != 0);
            }

            private void ChangeWall(BlockData data, Direction dir)
            {
                byte idTip = this.GetNextWallType(this.GetWallType(data, dir));
                this.SetWall(data, dir, idTip);
            }

            private void ChangeCollision(BlockData data, Direction dir)
            {
                data.collision ^= (uint)(1 << (int)dir);
            }

            private void SetWall(BlockData data, Direction dir, byte idTip)
            {
                data.idTip[(int)dir] = idTip;
            }

            private BlockData GetBlock(BlockData[] table, int x, int y)
            {
                int idx = y * this._data.GetWidth() + x;
                return table[idx];
            }

            private BlockData GetAdvanceBlock(BlockData[] table, int x, int y, Direction dir)
            {
                switch (dir)
                {
                    case Direction.NORTH: --y; break;
                    case Direction.SOUTH: ++y; break;
                    case Direction.EAST: ++x; break;
                    case Direction.WEST: --x; break;
                    default: break;
                }
                return this.GetBlock(table, x, y);
            }

            private byte GetWallType(BlockData data, Direction dir)
            {
                return data.idTip[(int)dir];
            }

            private byte GetNextWallType(byte idTip)
            {
                int max = this.tableTexture.Count();
                return (byte)(((int)idTip + 1) % max);
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMapData.StartProc()");

                // TODO: 例外エラー対応が必要
                this.tableTexture = new Dictionary<byte, Texture>();
                this.tableTexture.Add(0, null);

                this._data = new MapData();

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
                this.ReleaseThroughTable();
                this.ReleaseBlockTable();
                this.ReleaseTextureTable();

                this._data = null;
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
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                acc.ReleaseBlockData();

                // テクスチャ情報
                this.ClearTextureTable();
                this.tableTexture.Add(0, null);

                // 基本情報
                acc.SetName("");
                acc.SetWidth(0);
                acc.SetHeight(0);
                this._bThroughWall = false;
                this._bRecreating = false;
            }

            private void ClearThroughTable()
            {
                if (this.tableThrough == null) return;
                for (int i = 0; i < this.tableThrough.Length; i++) { this.tableThrough[i] = false; }
            }
            private void ReleaseThroughTable()
            {
                this.ClearThroughTable();
                this.tableThrough = null;
            }

            private void ClearBlockTable()
            {
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                acc.ClearBlockData();
            }
            private void ReleaseBlockTable()
            {
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                acc.ReleaseBlockData();
            }

            private void ClearTextureTable()
            {
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                Dictionary<byte, MapData.TextureInfo> table = acc.GetTextureInfoTable();
                if (table != null)
                {
                    foreach (KeyValuePair<byte, MapData.TextureInfo> pair in table)
                    {
                        Utility.GetIEntityTextureResources().Unload(pair.Value.filePathName);
                    }
                }
                acc.ClearTextureInfoTable();

                if (this.tableTexture != null)
                {
                    this.tableTexture.Clear();
                }
            }
            private void ReleaseTextureTable()
            {
                this.ClearTextureTable();

                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                acc.ReleaseTextureInfoTable();

                this.tableTexture = null;
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
                IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                BlockData[] table = acc.GetBlockData();

                int idx = y * this._data.GetWidth() + x;
                return this.tableTexture[table[idx].idTip[(int)dir]];
            }

            public bool IsMovable(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return false;

                bool bMovable = false;
                if (this._bThroughWall)
                {
                    bMovable = this.CheckMoveInRange(x, y, dir);
                }
                else
                {
                    IMapRawDataAccessor acc = (IMapRawDataAccessor)this._data;
                    BlockData[] table = acc.GetBlockData();

                    int idx = y * this._data.GetWidth() + x;
                    bMovable = ((table[idx].collision & (1 << (int)dir)) == 0);
                }
                return bMovable;
            }

            private bool CheckMoveInRange(int x, int y, Direction dir)
            {
                switch (dir)
                {
                    case Direction.NORTH: --y; break;
                    case Direction.SOUTH: ++y; break;
                    case Direction.EAST: ++x; break;
                    case Direction.WEST: --x; break;
                    default: break;
                }
                return (this.IsOutOfRange(x, y) == false);
            }

            public bool IsOutOfRange(int x, int y)
            {
                return (x < 0 || y < 0 || x >= this._data.GetWidth() || y >= this._data.GetHeight()) ? true : false;
            }

            public void Through(int x, int y, bool bThrough = true)
            {
                if (this.IsOutOfRange(x, y)) return;

                int idx = y * this._data.GetWidth() + x;
                this.tableThrough[idx] = bThrough;
            }

            public bool IsThorough(int x, int y)
            {
                if (this.IsOutOfRange(x, y)) return false;

                int idx = y * this._data.GetWidth() + x;
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
