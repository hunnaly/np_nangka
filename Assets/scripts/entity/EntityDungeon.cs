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
        // IEntityDungeon
        //------------------------------------------------------------------
        public interface IEntityDungeon
        {
            void Pause(bool pause);
            void Terminate();

        } //interface IEntityDungeon


        //------------------------------------------------------------------
        // 方向
        //------------------------------------------------------------------
        public enum Direction : int
        {
            NORTH = 0,
            EAST = 1,
            SOUTH = 2,
            WEST = 3,
            PLANE_MAX = 4,
            UP = 4,
            DOWN = 5,
            SOLID_MAX = 6

        } //enum Direction


        //------------------------------------------------------------------
        // DungeonStructure
        //------------------------------------------------------------------
        public class DungeonStructure
        {
            class Block
            {
                public GameObject root;
                public GameObject[] wall = new GameObject[(int)Direction.SOLID_MAX];
                public Block[] next = new Block[(int)Direction.PLANE_MAX];
            }

            struct Marker
            {
                public Block start;
                public Direction dir;
            }


            private bool bValid = false;
            private GameObject refPrefabPlane = null;

            private GameObject objRoot = null;
            private int numSide = 0;
            private Marker[] marker = new Marker[(int)Direction.PLANE_MAX];
            private Block center = null;


            // 準備処理済みかどうか
            public bool IsReady() { return (this.objRoot != null); }

            // 反対方向の取得
            private Direction GetOppositeDirection(Direction dir)
            {
                Direction retDir = dir;
                switch (dir)
                {
                    case Direction.NORTH: retDir = Direction.SOUTH; break;
                    case Direction.SOUTH: retDir = Direction.NORTH; break;
                    case Direction.EAST: retDir = Direction.WEST; break;
                    case Direction.WEST: retDir = Direction.EAST; break;
                    case Direction.UP: retDir = Direction.DOWN; break;
                    case Direction.DOWN: retDir = Direction.UP; break;
                    default: break;
                }
                return retDir;
            }

            // 初期化
            public void Init(GameObject prefabPlane)
            {
                if (this.bValid) return;

                this.refPrefabPlane = prefabPlane;
                this.ResetMarker();
                this.bValid = true;
            }

            // 準備処理
            public void Ready(int showableBlockNum)
            {
                if (this.IsReady()) return;

                this.numSide = (showableBlockNum - 1) * 2 + 1;

                this.objRoot = new GameObject(Define.OBJ_NAME_DUNGEON_ROOT);
                this.Build();
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {
                if (!this.IsReady()) return;

                this.Destroy();
            }

            // 初期化情報も含めてクリア
            public void Clear()
            {
                this.Reset();

                this.refPrefabPlane = null;
                this.bValid = false;
            }


            //------------------------------------------------------------------
            // デザイン変更処理
            //------------------------------------------------------------------

            public void ActiveBlock(int ofstX, int ofstY, bool bActive)
            {
                Block block = this.GetBlock(ofstX, ofstY);
                if (block == null) return;

                block.root.SetActive(bActive);
            }

            public void ChangeBlock(int ofstX, int ofstY, Texture[] aTex)
            {
                Block block = this.GetBlock(ofstX, ofstY);
                if (block == null) return;
                if (aTex.Length != (int)Direction.SOLID_MAX) return;

                for (int i = 0; i < (int)Direction.SOLID_MAX; i++)
                {
                    this.ChangeWallReal(block.wall[i], aTex[i]);
                }
            }

            public void ChangeWall(int ofstX, int ofstY, Direction dir, Texture tex)
            {
                Block block = this.GetBlock(ofstX, ofstY);
                if (block == null) return;

                this.ChangeWallReal(block.wall[(int)dir], tex);
            }

            private void ChangeWallReal(GameObject obj, Texture tex)
            {
                if (tex == null)
                {
                    obj.GetComponent<Renderer>().material.mainTexture = null;
                    obj.SetActive(false);
                }
                else
                {
                    obj.GetComponent<Renderer>().material.mainTexture = tex;
                    obj.GetComponent<Renderer>().material.color = Color.white;
                    obj.SetActive(true);
                }
            }

            private Block GetBlock(int ofstX, int ofstY)
            {
                Block block = null;
                do
                {
                    int val = (this.numSide - 1) / 2;
                    int countX = System.Math.Abs(ofstX);
                    int countY = System.Math.Abs(ofstY);
                    if (countX > val) break;
                    if (countY > val) break;

                    Direction dirX = (ofstX < 0) ? Direction.WEST : Direction.EAST;
                    Direction dirY = (ofstY < 0) ? Direction.NORTH : Direction.SOUTH;

                    block = center;
                    while (countX-- > 0) block = block.next[(int)dirX];
                    while (countY-- > 0) block = block.next[(int)dirY];

                } while (false);
                return block;
            }


            //------------------------------------------------------------------
            // ブロック構築
            //------------------------------------------------------------------

            private void Build()
            {
                // Direction.North 方向を向いている状態の
                // numSide x numDise 分の Texture なしかつ 非表示のブロックを作成する
                // 各ブロックは隣のブロックと繋がりをもっている
                // 両端同士も繋がりをもつループ構造とする
                // 構築処理には numSide に適切な値が入っている必要があることに注意

                this.BuildBlockLine(0, null);

                // マーカーを頼りに両端のブロックを連結
                this.SetLoopRelation();
            }

            private void BuildBlockLine(int line, Block backLineBlock)
            {
                if (line >= this.numSide) return;

                // １ブロックずつ構築
                // 再帰的に本ラインすべてのブロックを構築する
                // saveBlock には、本ラインの始めのブロックが返ってくる
                Block saveBlock = null;
                this.BuildBlock(line, 0, backLineBlock, null, ref saveBlock);

                // 次ラインの構築処理へ
                this.BuildBlockLine(line+1, saveBlock);
            }

            private void BuildBlock(int line, int num, Block backLineBlock, Block backBlock, ref Block refSaveBlock)
            {
                if (num >= this.numSide) return;

                // ブロックの構築
                Block block = new Block();
                this.BuildBlockObject(block, line, num);

                // 隣接する構築済みブロックと連結
                this.SetBlockRelation(block, backBlock, Direction.WEST);
                this.SetBlockRelation(block, backLineBlock, Direction.NORTH);

                // 次ライン構築時に利用する前ラインの開始ブロックを保存
                if (num == 0) refSaveBlock = block;

                // 必要があればマーカーをセット
                this.CheckAndSetMarkerFirstly(block, line, num);

                // 中心地のチェック＆セット
                this.CheckAndSetCenterFirstly(block, line, num);

                // 次ブロックの構築処理へ
                Block dummy = null;
                if (backLineBlock != null) backLineBlock = backLineBlock.next[(int)Direction.EAST];
                this.BuildBlock(line, num+1, backLineBlock, block, ref dummy);
            }

            private void SetBlockRelation(Block target, Block relativeBlock, Direction dir)
            {
                target.next[(int)dir] = relativeBlock;

                if (relativeBlock == null) return;
                relativeBlock.next[(int)this.GetOppositeDirection(dir)] = target;
            }

            private void SetLoopRelation()
            {
                Direction dirProcessN2S = Direction.EAST;
                Direction dirProcessW2E = Direction.SOUTH;

                Block startN = this.marker[(int)Direction.NORTH].start;
                Block startS = this.marker[(int)Direction.SOUTH].start;
                Block startW = this.marker[(int)Direction.WEST].start;
                Block startE = this.marker[(int)Direction.EAST].start;

                Block endN = null; Block temp = startN; while ((temp = temp.next[(int)dirProcessN2S]) != null) endN = temp;
                Block endW = null; temp = startW; while ((temp = temp.next[(int)dirProcessW2E]) != null) endW = temp;

                this.SetLoopRelationReal(dirProcessN2S, startN, endN, startS, Direction.NORTH);
                this.SetLoopRelationReal(dirProcessW2E, startW, endW, startE, Direction.WEST);
            }
            private void SetLoopRelationReal(Direction dirProcess, 
                Block a, Block endA, Block b, Direction dirA2b)
            {
                Direction dirOpposite = this.GetOppositeDirection(dirA2b);
                a.next[(int)dirA2b] = b;
                b.next[(int)dirOpposite] = a;

                if (a == endA) return;

                Block nextA = a.next[(int)dirProcess];
                Block nextB = b.next[(int)dirProcess];
                this.SetLoopRelationReal(dirProcess, nextA, endA, nextB, dirA2b);
            }

            private void BuildBlockObject(Block block, int line, int num)
            {
                block.root = CreateBlockRootObject(line, num);

                for (int i = 0; i < (int)Direction.SOLID_MAX; i++)
                {
                    block.wall[i] = this.CreateBlockObject((Direction)i, block.root);
                }
            }

            private GameObject CreateBlockRootObject(int line, int num)
            {
                GameObject obj = new GameObject(Define.OBJ_NAME_DUNGEON_BLOCK_ROOT);
                obj.transform.SetParent(this.objRoot.transform, false);

                float sx = (this.numSide - 1) / 2 * -4.0f;
                float sz = (this.numSide - 1) / 2 * 4.0f;
                float x = sx + num * 4.0f;
                float z = sz - line * 4.0f;
                Vector3 trans = new Vector3(x, 0.0f, z);
                obj.transform.localPosition = trans;

                obj.SetActive(false);

                return obj;
            }

            private GameObject CreateBlockObject(Direction dir, GameObject parent)
            {
                GameObject obj = null;
                switch (dir)
                {
                    case Direction.UP: obj = this.CreateWallUp(parent); break;
                    case Direction.DOWN: obj = this.CreateWallDown(parent); break;
                    case Direction.NORTH: obj = this.CreateWallNorth(parent); break;
                    case Direction.SOUTH: obj = this.CreateWallSouth(parent); break;
                    case Direction.EAST: obj = this.CreateWallEast(parent); break;
                    case Direction.WEST: obj = this.CreateWallWest(parent); break;
                    default: break;
                }
                return obj;
            }

            private GameObject CreateWallUp(GameObject parent)
            {
                Vector3 trans = new Vector3(0.0f, 4.0f, 0.0f);
                Quaternion rote = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                return this.CreateWallReal(parent, trans, rote);
            }

            private GameObject CreateWallDown(GameObject parent)
            {
                return this.CreateWallReal(parent, Vector3.zero, Quaternion.identity);
            }

            private GameObject CreateWallNorth(GameObject parent)
            {
                Vector3 trans = new Vector3(0.0f, 2.0f, 2.0f);
                Quaternion rote = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                return this.CreateWallReal(parent, trans, rote);
            }

            private GameObject CreateWallSouth(GameObject parent)
            {
                Vector3 trans = new Vector3(0.0f, 2.0f, -2.0f);
                Quaternion rote = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
                return this.CreateWallReal(parent, trans, rote);
            }

            private GameObject CreateWallEast(GameObject parent)
            {
                Vector3 trans = new Vector3(2.0f, 2.0f, 0.0f);
                Quaternion rote = Quaternion.Euler(-90.0f, 90.0f, 0.0f);
                return this.CreateWallReal(parent, trans, rote);
            }

            private GameObject CreateWallWest(GameObject parent)
            {
                Vector3 trans = new Vector3(-2.0f, 2.0f, 0.0f);
                Quaternion rote = Quaternion.Euler(-90.0f, -90.0f, 0.0f);
                return this.CreateWallReal(parent, trans, rote);
            }

            private GameObject CreateWallReal(GameObject parent, Vector3 trans, Quaternion rote)
            {
                GameObject objWall = (GameObject)Object.Instantiate(this.refPrefabPlane, trans, rote);
                objWall.transform.SetParent(parent.transform, false);
                objWall.SetActive(false);
                return objWall;
            }


            //------------------------------------------------------------------
            // ブロック破棄
            //------------------------------------------------------------------

            private void Destroy()
            {
                this.DestroyBlockTable();

                // 子階層オブジェクトも破棄される
                Object.Destroy(this.objRoot);
                this.objRoot = null;
            }

            // ブロック管理情報テーブルの破棄
            // Object は Object.Destroy() によって連鎖的に破棄されるので、
            // Object の管理情報のみを破棄する。
            private void DestroyBlockTable()
            {
                // マーカーを頼りに順にブロック管理情報を破棄していく
                // FrontラインからBackラインに向けて、それぞれLeftからRightの向きに破棄
                // 最後にマーカー情報をリセット

                // LEFT から Right への向きを NORTHマーカーから取得
                Direction nextDir = this.marker[(int)Direction.NORTH].dir;

                // 初期位置と次ライン方向を Leftマーカーから取得
                Block start = this.marker[(int)Direction.WEST].start;
                Direction nextLineDir = this.marker[(int)Direction.WEST].dir;

                // １ライン分ずつブロック管理情報を破棄
                // 再帰的に全ライン分の処理を行う
                Direction backDir = this.GetOppositeDirection(nextDir);
                Direction backLineDir = this.GetOppositeDirection(nextLineDir);
                this.DestroyBlockLine(start, nextDir, backDir, nextLineDir, backLineDir);

                // マーカーのリセット
                this.ResetMarker();
            }

            private void DestroyBlockLine(Block start, Direction nextDir, Direction backDir,
                Direction nextLineDir, Direction backLineDir)
            {
                if (start == null) return;

                // 破棄される前に次ラインのブロックを覚えておく
                Block next = start.next[(int)nextLineDir];

                // １ブロックずつ破棄
                // 再帰的に本ラインの全ブロック分の処理を行う
                this.DestroyBlock(start, nextDir, backDir);

                // 次ラインの処理へ
                this.DestroyBlockLine(next, nextDir, backDir, nextLineDir, backLineDir);
            }

            private void DestroyBlock(Block target, Direction nextDir, Direction backDir)
            {
                if (target == null) return;

                // 破棄される前に次ブロックを覚えておく
                Block next = target.next[(int)nextDir];

                // 関係性の破棄
                this.DestroyBlockRelation(target);

                // 管理情報の破棄
                target.root = null;
                for (int i = 0; i < (int)Direction.SOLID_MAX; i++) target.wall[i] = null;

                // 次ブロックの処理へ
                this.DestroyBlock(next, nextDir, backDir);
            }

            private void DestroyBlockRelation(Block target)
            {
                this.DestroyRelativeBlockRelation(target, Direction.NORTH);
                this.DestroyRelativeBlockRelation(target, Direction.SOUTH);
                this.DestroyRelativeBlockRelation(target, Direction.EAST);
                this.DestroyRelativeBlockRelation(target, Direction.WEST);
            }

            private void DestroyRelativeBlockRelation(Block target, Direction dir)
            {
                Block relativeBlock = target.next[(int)dir];
                if (relativeBlock == null) return;

                relativeBlock.next[(int)this.GetOppositeDirection(dir)] = null;
                target.next[(int)dir] = null;
            }


            //------------------------------------------------------------------
            // マーカー処理
            //------------------------------------------------------------------

            private void CheckAndSetCenterFirstly(Block block, int line, int num)
            {
                int val = (this.numSide - 1) / 2;
                if (line == val && num == val) { this.center = block; }
            }

            private void CheckAndSetMarkerFirstly(Block block, int line, int num)
            {
                this.CheckAndSetFrontMarkerFirstly(block, line, num);
                this.CheckAndSetBackMarkerFirstly(block, line, num);
                this.CheckAndSetLeftMarkerFirstly(block, line, num);
                this.CheckAndSetRightMarkerFirstly(block, line, num);
            }
            private void CheckAndSetFrontMarkerFirstly(Block block, int line, int num)
            {
                if (line == 0 && num == 0)
                {
                    this.marker[(int)Direction.NORTH].start = block;
                    this.marker[(int)Direction.NORTH].dir = Direction.EAST;
                }
            }
            private void CheckAndSetBackMarkerFirstly(Block block, int line, int num)
            {
                if (line == this.numSide - 1 && num == 0)
                {
                    this.marker[(int)Direction.SOUTH].start = block;
                    this.marker[(int)Direction.SOUTH].dir = Direction.EAST;
                }
            }
            private void CheckAndSetLeftMarkerFirstly(Block block, int line, int num)
            {
                if (line == 0 && num == 0)
                {
                    this.marker[(int)Direction.WEST].start = block;
                    this.marker[(int)Direction.WEST].dir = Direction.SOUTH;
                }
            }
            private void CheckAndSetRightMarkerFirstly(Block block, int line, int num)
            {
                if (line == 0 && num == this.numSide - 1)
                {
                    this.marker[(int)Direction.EAST].start = block;
                    this.marker[(int)Direction.EAST].dir = Direction.SOUTH;
                }
            }

            private void ResetMarker()
            {
                for (int i = 0; i < (int)Direction.PLANE_MAX; i++)
                {
                    this.marker[i].start = null;
                }
            }

        } //class DungeonStructure


        //------------------------------------------------------------------
        // IMapData
        //------------------------------------------------------------------
        public interface IMapData
        {
            Texture LoadTexture(string path);
            void UnloadTexture(string path);

        } //interface IMapData

        //------------------------------------------------------------------
        // MapData
        //------------------------------------------------------------------
        public class MapData
        {
            public struct BlockData
            {
                // Direction.SOLID_MAX 個のテクスチャIDを格納する
                // 各 Map ごとに 256種類まで利用可能
                public byte[] idTip;

                // Direction.* bit目の On/Off で障壁あり/なしをあらわす
                // Direction.PLANE_MAX 分(平面方向のみ)の情報をもつ
                public uint collision;
            }

            private bool bValid = false;
            private IMapData intfLoader = null;

            private bool bLoaded = false;
            private string _name = "";
            private int _width = 0;
            private int _height = 0;
            private int _startX = 0;
            private int _startY = 0;
            private Direction _startDir = 0;
            private Dictionary<byte, string> tableTexturePath = null;
            private Dictionary<byte, Texture> tableTexture = null; // 0 => null をダミー値としてもたせる
            private BlockData[] tableData = null;

            public string name { get { return this._name; } }
            public int width { get { return this._width; } }
            public int height { get { return this._height; } }
            public int startX { get { return this._startX; } }
            public int startY { get { return this._startY; } }
            public Direction startDir { get { return this._startDir; } }


            //------------------------------------------------------------------
            // 基本処理
            //------------------------------------------------------------------

            public void Init(IMapData intf)
            {
                this.intfLoader = intf;
                this.bValid = true;
            }

            // マップデータを読み込む
            public void Ready(/*string name*/)
            {
                this.SetDummyData();
            }

            public void Reset()
            {
                // コリジョン情報とデザイン情報
                int num = this._width * this._height;
                for (int i = 0; i < num; i++) { this.tableData[i].idTip = null; }
                this.tableData = null;

                // テクスチャ情報
                foreach (KeyValuePair<byte, string> pair in this.tableTexturePath)
                {
                    intfLoader.UnloadTexture(pair.Value);
                }
                this.tableTexturePath.Clear();
                this.tableTexture.Clear();
                this.tableTexturePath = null;
                this.tableTexture = null;

                // 基本情報
                this._name = "";
                this._width = 0;
                this._height = 0;
                this._startX = 0;
                this._startY = 0;
                this._startDir = 0;
            }

            public void Clear()
            {
                this.Reset();

                this.intfLoader = null;
                this.bValid = false;
            }


            //------------------------------------------------------------------
            // 処理
            //------------------------------------------------------------------

            public Texture[] GetBlockTexture(int x, int y)
            {
                if (this.IsOutOfRange(x, y)) return null;

                Texture[] tableTex = new Texture[(int)Direction.SOLID_MAX];
                for (int i=0; i<(int)Direction.SOLID_MAX; i++)
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


            //------------------------------------------------------------------
            // ダミー処理
            //------------------------------------------------------------------

            // 仮データ
            private void SetDummyData()
            {
                if (this.intfLoader == null) return;

                // 基本情報
                this._name = "dummy";
                this._width = 8;
                this._height = 8;
                this._startX = 0;
                this._startY = 0;
                this._startDir = Direction.EAST;

                // テクスチャ情報
                Texture tex = null;
                this.tableTexturePath = new Dictionary<byte, string>();
                this.tableTexture = new Dictionary<byte, Texture>();
                this.tableTexture.Add(0, null);

                tex = intfLoader.LoadTexture(Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                this.tableTexturePath.Add(1, Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                this.tableTexture.Add(1, tex);

                tex = intfLoader.LoadTexture(Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);
                this.tableTexturePath.Add(2, Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);
                this.tableTexture.Add(2, tex);

                // コリジョン情報とデザイン情報
                int num = this.width * this.height;
                this.tableData = new BlockData[num];
                for (int i = 0; i < num; i++)
                {
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
                this.DSetBlock(this.tableData[7], 2, 2, 2, 0, 1, 1, true, true, true, false);

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
                if (n) flag |= 1 << (int)Direction.NORTH;
                if (e) flag |= 1 << (int)Direction.EAST;
                if (s) flag |= 1 << (int)Direction.SOUTH;
                if (w) flag |= 1 << (int)Direction.WEST;
                data.collision = flag;
            }
        }


        //------------------------------------------------------------------
        // DungeonBuilder
        //------------------------------------------------------------------
        public class DungeonBuilder
        {
            private bool bValid = false;
            private bool bReady = false;

            private DungeonStructure refStructure = null;
            private MapData refMapData = null;
            private IMapData intfLoader = null;

            private GameObject prefabWall = null;


            public bool IsValid() { return this.bValid; }
            public bool IsReady() { return this.bReady; }


            // 初期化
            public void Init(DungeonStructure structure, MapData mapData, IMapData intf)
            {
                if (this.IsValid()) return;

                this.refStructure = structure;
                this.refMapData = mapData;
                this.intfLoader = intf;
                this.bValid = true;
            }

            // 準備処理
            public void Ready()
            {
                if (!this.IsValid() || this.IsReady()) return;

                this.prefabWall = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_WALL);
                this.refMapData.Init(this.intfLoader);
                this.refStructure.Init(this.prefabWall);
                this.bReady = true;
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {
                if (!this.IsReady()) return;

                if (this.refStructure != null) this.refStructure.Clear();
                if (this.refMapData != null) this.refMapData.Clear();
                if (this.prefabWall) { Resources.UnloadAsset(this.prefabWall); this.prefabWall = null; }
                this.bReady = false;
            }

            // 初期化情報も含めてクリア
            public void Clear()
            {
                this.Reset();

                this.refStructure = null;
                this.bValid = false;
            }

            // ダンジョン生成
            // 指定座標の周りを視覚化
            public void Create(int x, int y)
            {
                if (!this.IsReady()) return;

                this.refMapData.Ready();
                
                this.refStructure.Ready(Define.SHOWABLE_BLOCK);

                int temp = Define.SHOWABLE_BLOCK - 1;
                int startOfstX = temp * -1;
                int startOfstY = temp * -1;
                int endOfstX = temp;
                int endOfstY = temp;
                this.DesignateRect(
                    x + startOfstX, y + startOfstY, x + endOfstX, y + endOfstY,
                    startOfstX, startOfstY);
            }

            // 矩形領域に対してダンジョン視覚化指示
            public void DesignateRect(int sx, int sy, int ex, int ey, int vx, int vy)
            {
                if (!this.IsReady()) return;

                for (int x = sx; x <= ex; x++)
                {
                    for (int y = sy; y <= ey; y++)
                    {
                        this.Designeate(x, y, vx+(x-sx), vy+(y-sy));
                    }
                }
            }

            private void Designeate(int mx, int my, int vx, int vy)
            {
                // 指定ブロックのテクスチャ情報を取得
                // マップ範囲外のときは null が返される
                Texture[] tableTex = this.refMapData.GetBlockTexture(mx, my);

                // マップ範囲外のときは無効化
                if (tableTex == null)
                {
                    this.refStructure.ActiveBlock(vx, vy, false);
                }
                // 範囲内のときはマップ情報に基づいて視覚化
                else
                {
                    this.refStructure.ActiveBlock(vx, vy, true);
                    this.refStructure.ChangeBlock(vx, vy, tableTex);
                }
            }

        } //class DungeonBuilder


        //------------------------------------------------------------------
        // Player
        //------------------------------------------------------------------
        public class Player
        {
            private bool bValid = false;
            private bool bReady = false;

            private int _x = 0;
            private int _y = 0;
            private Direction _dir = 0;
            private bool _bMovable = false;
            private Camera camera = null;


            public int x { get { return this._x; } }
            public int y { get { return this._y; } }
            public Direction dir { get { return this._dir; } }
            public bool bMovable { get { return this._bMovable; } }

            public bool IsValid() { return this.bValid; }
            public bool IsReady() { return this.bReady; }
            public void Movable(bool vMovable) { this._bMovable = bMovable; }


            // 初期化
            public void Init(Camera camera)
            {
                if (this.IsValid()) return;

                this.camera = camera;
                this.bValid = true;
            }

            // 準備処理
            public void Ready(int x, int y, Direction dir)
            {
                if (!this.IsValid() || this.IsReady()) return;

                this._x = x;
                this._y = y;
                this.Rotate(this._dir);
                this.bReady = true;
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {
                if (!this.IsReady()) return;

                this._x = 0;
                this._y = 0;
                this._dir = 0;
                this._bMovable = false;
                this.bReady = false;
            }

            // 初期化情報も含めてクリア
            public void Clear()
            {
                this.Reset();

                this.camera = null;
                this.bValid = false;
            }

            public void Rotate(Direction dir)
            {
                this._dir = dir;
                this.camera.transform.rotation = Quaternion.AngleAxis(90.0f, new Vector3(0.0f, 1.0f, 0.0f));
            }

        } //class Player


        //------------------------------------------------------------------
        // EntityDungeon
        //------------------------------------------------------------------
        public class EntityDungeon : NpEntity, IEntityDungeon, IMapData
        {
            private bool bInitialized = false;

            private DungeonStructure structure = null;
            private DungeonBuilder builder = null;
            private MapData mapData = null;
            private Player player = null;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityDungeon.StartProc()");
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
                Debug.Log("EntityDungeon.CleanUp()");

                if (this.structure != null) { this.structure.Clear(); this.structure = null; }
                if (this.builder != null) { this.builder.Clear(); this.builder = null; }
                if (this.mapData != null) { this.mapData.Clear(); this.mapData = null; }
                if (this.player != null) { this.player.Clear(); this.player = null; }
            }


            //------------------------------------------------------------------
            // IMapData
            //------------------------------------------------------------------

            public Texture LoadTexture(string path)
            {
                return Utility.GetIEntityTextureResources().Load(path);
            }

            public void UnloadTexture(string path)
            {
                Utility.GetIEntityTextureResources().Unload(path);
            }

            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator Ready()
            {
                while (Utility.GetIEntityTextureResources() == null) yield return null;
                while (Utility.GetIEntityTextureResources().IsInitialized() == false) yield return null;

                yield return this.Build();

                yield return this.ReadyPlayer();

                yield return Utility.FadeIn();

                this.bInitialized = true;
            }

            // ダンジョン構築
            private IEnumerator Build()
            {
                do {
                    if (this.InitBuilder() == null) break;
                    this.builder.Ready();

                    // キャラクターの位置
                    // TODO: 本来はセーブデータから取得。ひとまず初期情報を指定。
                    int x = this.mapData.startX;
                    int y = this.mapData.startY;

                    this.builder.Create(x, y);

                } while (false);

                yield return null;
            }

            // プレイヤー準備
            private IEnumerator ReadyPlayer()
            {
                this.player = new Player();
                this.player.Init(Global.Instance.cameraPlayer);

                // キャラクターの位置と向き
                // TODO: 本来はセーブデータから取得。ひとまず初期情報を指定。
                int x = this.mapData.startX;
                int y = this.mapData.startY;
                Direction dir = this.mapData.startDir;
                this.player.Ready(x, y, dir);

                yield return null;
            }

            // ダンジョン構築オブジェクトの準備
            private DungeonBuilder InitBuilder()
            {
                do {
                    this.mapData = new MapData();
                    if (this.mapData == null) break;

                    this.structure = new DungeonStructure();
                    if (this.structure == null) break;

                    this.builder = new DungeonBuilder();
                    if (this.builder == null) break;

                    this.builder.Init(this.structure, this.mapData, this);

                } while (false);

                return this.builder;
            }

        } //class EntityDungeon

    } //namespace entity
} //namespace nangka