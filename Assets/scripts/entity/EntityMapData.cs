using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // MapData
        //------------------------------------------------------------------
        public class MapData
        {
            private string _name;
            public string name { get { return this._name; } }

            private int _width;
            public int width { get { return this._width; } }

            private int _height;
            public int height { get { return this._height; } }

            private int _startX;
            public int startX { get { return this._startX; } }

            private int _startY;
            public int startY { get { return this._startY; } }

            private Direction _startDir;
            public Direction startDir { get { return this._startDir; } }


            //------------------------------------------------------------------
            // 各種設定メソッド
            //------------------------------------------------------------------

            public void SetName(string name) { this._name = name; }
            public void SetSize(int w, int h) { this._width = w; this._height = h; }
            public void SetStartData(int x, int y, Direction dir) { this._startX = x; this._startY = y; this._startDir = dir; }

            // リセット処理
            public void Reset()
            {
                this._name = "";
                this._width = 0;
                this._height = 0;
                this._startX = 0;
                this._startY = 0;
                this._startDir = 0;
            }

        } //class MapData


        //------------------------------------------------------------------
        // IEntityMapData
        //------------------------------------------------------------------
        public interface IEntityMapData
        {
            bool IsReadyLogic();
            void Load(IEntityTextureResources iTexRes);
            void Reset();

            MapData GetMapData();
            Texture[] GetBlockTexture(int x, int y);
            Texture GetTexture(int x, int y, Direction dir);
            bool IsMovable(int x, int y, Direction dir);
            bool IsOutOfRange(int x, int y);

            void Terminate();

        } //interface IEntityMapData


        //------------------------------------------------------------------
        // EntityMapData
        //------------------------------------------------------------------
        public class EntityMapData : NpEntity, IEntityMapData
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private MapData _data;
            public MapData GetMapData() { return this._data; }

            private Dictionary<byte, string> tableTexturePath;
            private Dictionary<byte, Texture> tableTexture; // 0 => null をダミー値としてもたせる
            private BlockData[] tableData;



            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMapData.StartProc()");

                // TODO: 例外エラー対応が必要
                this._data = new MapData();
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
                this.ClearBlockTable();
                this.tableData = null;

                this.ClearTextureTable();
                this.tableTexturePath = null;
                this.tableTexture = null;

                this._data = null;
            }

            //------------------------------------------------------------------
            // ロジック準備処理／ロジックリセット処理
            //------------------------------------------------------------------

            public void Load(IEntityTextureResources iTexRes)
            {
                if (!this.IsReadyLogic()) return;

                this.SetDummyData(iTexRes);
            }

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                // コリジョン情報とデザイン情報
                this.ClearBlockTable();
                this.tableData = null;

                // テクスチャ情報
                this.ClearTextureTable();

                // 基本情報
                this._data.Reset();
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
                int idx = y * this._data.width + x;
                return this.tableTexture[this.tableData[idx].idTip[(int)dir]];
            }

            public bool IsMovable(int x, int y, Direction dir)
            {
                if (this.IsOutOfRange(x, y)) return false;

                int idx = y * this._data.width + x;
                return ((this.tableData[idx].collision & (1 << (int)dir)) == 0);
            }

            public bool IsOutOfRange(int x, int y)
            {
                return (x < 0 || y < 0 || x >= this._data.width || y >= this._data.height) ? true : false;
            }


            //------------------------------------------------------------------
            // ダミー処理
            //------------------------------------------------------------------

            // 仮データ
            private void SetDummyData(IEntityTextureResources iTexRes)
            {
                // 基本情報
                this._data.SetName("dummy");
                this._data.SetSize(8, 8);
                this._data.SetStartData(0, 0, Direction.EAST);

                // テクスチャ情報
                Texture tex = iTexRes.Load(Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                this.tableTexturePath.Add(1, Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                this.tableTexture.Add(1, tex);

                tex = iTexRes.Load(Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);
                this.tableTexturePath.Add(2, Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);
                this.tableTexture.Add(2, tex);

                // コリジョン情報とデザイン情報
                int num = this._data.width * this._data.height;
                this.tableData = new BlockData[num];
                for (int i = 0; i < num; i++)
                {
                    this.tableData[i] = new BlockData();
                    this.tableData[i].idTip = new byte[(int)Direction.SOLID_MAX];
                    this.tableData[i].collision = 0;
                }
                //   0   1   2   3   4   5   6   7
                // +---+---+---+---+---+---+---+---+
                //0| @                             |
                // +---+---+---+---+---+---+---+   +
                //1|   |   |                   |   |
                // +   +   +   +---+   +---+---+   +
                //2|   |           |   |       |   |
                // +   +   +---+---+   +---+   +   +
                //3|   |       |           |   |   |
                // +   +   +   +   +   +---+   +   +
                //4|   |   |   |   |   |       |   |
                // +   +---+   +   +---+   +---+   +
                //5|           |               |   |
                // +   +   +   +---+---+   +   +   +
                //6|               |   |           |
                // +   +   +   +---+   +---+---+   +
                //7|           |                   |
                // +---+---+---+---+---+---+---+---+

                this.DSetBlock(this.tableData[0], 2, 0, 2, 2, 1, 1, true, false, true, true);
                this.DSetBlock(this.tableData[1], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[2], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[3], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[4], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[5], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[6], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[7], 2, 2, 0, 0, 1, 1, true, true, false, false);

                this.DSetBlock(this.tableData[8], 2, 2, 0, 2, 1, 1, true, true, false, true);
                this.DSetBlock(this.tableData[9], 2, 2, 0, 2, 1, 1, true, true, false, true);
                this.DSetBlock(this.tableData[10], 2, 0, 0, 2, 1, 1, true, false, false, true);
                this.DSetBlock(this.tableData[11], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[12], 2, 0, 0, 0, 1, 1, true, false, false, false);
                this.DSetBlock(this.tableData[13], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[14], 2, 2, 2, 0, 1, 1, true, true, true, false);
                this.DSetBlock(this.tableData[15], 0, 2, 0, 2, 1, 1, false, true, false, true);

                this.DSetBlock(this.tableData[16], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[17], 0, 0, 0, 2, 1, 1, false, false, false, true);
                this.DSetBlock(this.tableData[18], 0, 0, 2, 0, 1, 1, false, false, true, false);
                this.DSetBlock(this.tableData[19], 2, 2, 2, 0, 1, 1, true, true, true, false);
                this.DSetBlock(this.tableData[20], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[21], 2, 0, 2, 2, 1, 1, true, false, true, true);
                this.DSetBlock(this.tableData[22], 2, 2, 0, 0, 1, 1, true, true, false, false);
                this.DSetBlock(this.tableData[23], 0, 2, 0, 2, 1, 1, false, true, false, true);

                this.DSetBlock(this.tableData[24], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[25], 0, 0, 0, 2, 1, 1, false, false, false, true);
                this.DSetBlock(this.tableData[26], 2, 2, 0, 0, 1, 1, true, true, false, false);
                this.DSetBlock(this.tableData[27], 2, 0, 0, 2, 1, 1, true, false, false, true);
                this.DSetBlock(this.tableData[28], 0, 0, 0, 0, 1, 1, false, false, false, false);
                this.DSetBlock(this.tableData[29], 2, 2, 2, 0, 1, 1, true, true, true, false);
                this.DSetBlock(this.tableData[30], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[31], 0, 2, 0, 2, 1, 1, false, true, false, true);

                this.DSetBlock(this.tableData[48], 0, 0, 0, 2, 1, 1, false, false, false, true);
                this.DSetBlock(this.tableData[32], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[33], 0, 2, 2, 2, 1, 1, false, true, true, true);
                this.DSetBlock(this.tableData[34], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[35], 0, 2, 0, 2, 1, 1, false, true, false, true);
                this.DSetBlock(this.tableData[36], 0, 2, 2, 2, 1, 1, false, true, true, true);
                this.DSetBlock(this.tableData[37], 2, 0, 0, 2, 1, 1, true, false, false, true);
                this.DSetBlock(this.tableData[38], 0, 2, 2, 0, 1, 1, false, true, true, false);
                this.DSetBlock(this.tableData[39], 0, 2, 0, 2, 1, 1, false, true, false, true);

                this.DSetBlock(this.tableData[40], 0, 0, 0, 2, 1, 1, false, false, false, true);
                this.DSetBlock(this.tableData[41], 2, 0, 0, 0, 1, 1, true, false, false, false);
                this.DSetBlock(this.tableData[42], 0, 2, 0, 0, 1, 1, false, true, false, false);
                this.DSetBlock(this.tableData[43], 0, 0, 2, 2, 1, 1, false, false, true, true);
                this.DSetBlock(this.tableData[44], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[45], 0, 0, 0, 0, 1, 1, false, false, false, false);
                this.DSetBlock(this.tableData[46], 2, 2, 0, 0, 1, 1, true, true, false, false);
                this.DSetBlock(this.tableData[47], 0, 2, 0, 2, 1, 1, false, true, false, true);

                this.DSetBlock(this.tableData[49], 0, 0, 0, 0, 1, 1, false, false, false, false);
                this.DSetBlock(this.tableData[50], 0, 0, 0, 0, 1, 1, false, false, false, false);
                this.DSetBlock(this.tableData[51], 2, 2, 2, 0, 1, 1, true, true, true, false);
                this.DSetBlock(this.tableData[52], 2, 2, 0, 2, 1, 1, true, true, false, true);
                this.DSetBlock(this.tableData[53], 0, 0, 2, 2, 1, 1, false, false, true, true);
                this.DSetBlock(this.tableData[54], 0, 0, 2, 0, 1, 1, false, false, true, false);
                this.DSetBlock(this.tableData[55], 0, 2, 0, 0, 1, 1, false, true, false, false);

                this.DSetBlock(this.tableData[56], 0, 0, 2, 2, 1, 1, false, false, true, true);
                this.DSetBlock(this.tableData[57], 0, 0, 2, 0, 1, 1, false, false, true, false);
                this.DSetBlock(this.tableData[58], 0, 2, 2, 0, 1, 1, false, true, true, false);
                this.DSetBlock(this.tableData[59], 2, 0, 2, 2, 1, 1, true, false, true, true);
                this.DSetBlock(this.tableData[60], 0, 0, 2, 0, 1, 1, false, false, true, false);
                this.DSetBlock(this.tableData[61], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[62], 2, 0, 2, 0, 1, 1, true, false, true, false);
                this.DSetBlock(this.tableData[63], 0, 2, 2, 0, 1, 1, false, true, true, false);
            }
            private void DSetBlock(BlockData data,
                byte n, byte e, byte s, byte w, byte u, byte d,
                bool bn, bool be, bool bs, bool bw)
            {
                this.DSetDesign(data, n, e, s, w, u, d);
                this.DSetCollision(data, bn, be, bs, bw);
            }
            private void DSetDesign(BlockData data, byte n, byte e, byte s, byte w, byte u, byte d)
            {
                data.idTip[(int)Direction.NORTH] = n;
                data.idTip[(int)Direction.EAST] = e;
                data.idTip[(int)Direction.SOUTH] = s;
                data.idTip[(int)Direction.WEST] = w;
                data.idTip[(int)Direction.UP] = u;
                data.idTip[(int)Direction.DOWN] = d;
            }
            private void DSetCollision(BlockData data, bool n, bool e, bool s, bool w)
            {
                uint flag = 0;
                if (n) flag |= (1 << (int)Direction.NORTH);
                if (e) flag |= (1 << (int)Direction.EAST);
                if (s) flag |= (1 << (int)Direction.SOUTH);
                if (w) flag |= (1 << (int)Direction.WEST);
                data.collision = flag;
            }


            //------------------------------------------------------------------
            // BlockData
            //------------------------------------------------------------------
            class BlockData
            {
                // Direction.SOLID_MAX 個のテクスチャIDを格納する
                // 各 Map ごとに 256種類まで利用可能
                public byte[] idTip;

                // Direction.* bit目の On/Off で障壁あり/なしをあらわす
                // Direction.PLANE_MAX 分(平面方向のみ)の情報をもつ
                public uint collision;

            } //class BlockData

        } //class EntityMapData

    } //namespace entity
} //namespace nangka
