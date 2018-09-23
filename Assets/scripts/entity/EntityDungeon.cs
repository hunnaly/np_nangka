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

            class Marker
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
                return Utility.GetOppositeDirection(dir);
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

                for (int i=0; i< (int)Direction.PLANE_MAX; i++)
                {
                    this.marker[i] = new Marker();
                }

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

            // 視覚化領域を指定方向に 1 ラインずらす
            // ただしループさせただけなので、ずらした領域のブロックの情報を
            // ChangeBlock などで別途書き換える必要がある。
            public void Scroll(Direction dir)
            {
                this.ScrollCenter(dir);
                this.ScrollMarker(dir);
                this.TransformPositionByScroll(dir);
            }

            private void ScrollCenter(Direction dir)
            {
                this.center = this.center.next[(int)dir];
            }

            private void ScrollMarker(Direction dir)
            {
                Direction dirOpposite = this.GetOppositeDirection(dir);
                int markerIdx = (int)dir;
                int markerIdxOpposite = (int)dirOpposite;

                this.marker[markerIdx].start = this.marker[markerIdx].start.next[(int)dir];
                this.marker[markerIdxOpposite].start = this.marker[markerIdxOpposite].start.next[(int)dir];
            }

            private void TransformPositionByScroll(Direction dir)
            {
                Marker targetMarker = this.marker[(int)dir];

                float diff = this.numSide * 4.0f;
                Block start = targetMarker.start;
                Block block = start, next = null;
                do {
                    next = block.next[(int)targetMarker.dir];

                    Vector3 pos = block.root.transform.position;
                    switch (dir)
                    {
                        case Direction.NORTH: pos.z += diff; break;
                        case Direction.SOUTH: pos.z -= diff; break;
                        case Direction.WEST: pos.x -= diff; break;
                        case Direction.EAST: pos.x += diff; break;
                        default: break;
                    }
                    block.root.transform.position = pos;

                    if (next == start) break;
                    block = next;
                }
                while (true);
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
                    this.marker[i] = null;
                }
            }

        } //class DungeonStructure


        //------------------------------------------------------------------
        // DungeonBuilder
        //------------------------------------------------------------------
        public class DungeonBuilder
        {
            private bool bValid = false;
            private bool bReady = false;

            private int curX = 0;
            private int curY = 0;
            private int showableBlockNum = 0;

            private DungeonStructure refStructure = null;
            private IEntityMapData refIMapData = null;

            private GameObject prefabWall = null;


            public bool IsValid() { return this.bValid; }
            public bool IsReady() { return this.bReady; }


            // 初期化
            public void Init(DungeonStructure structure, IEntityMapData refIMapData)
            {
                if (this.IsValid()) return;

                this.refStructure = structure;
                this.refIMapData = refIMapData;
                this.bValid = true;
            }

            // 準備処理
            public void Ready()
            {
                if (!this.IsValid() || this.IsReady()) return;

                this.prefabWall = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_WALL);
                this.refStructure.Init(this.prefabWall);
                this.bReady = true;
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {
                if (!this.IsReady()) return;

                if (this.refStructure != null) this.refStructure.Clear();
                if (this.refIMapData != null) this.refIMapData.Clear();
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
            public void Create(int x, int y, int num)
            {
                if (!this.IsReady()) return;

                this.curX = x;
                this.curY = y;
                this.showableBlockNum = num;
                
                this.refStructure.Ready(this.showableBlockNum);

                int temp = this.showableBlockNum - 1;
                int startOfstX = temp * -1;
                int startOfstY = temp * -1;
                int endOfstX = temp;
                int endOfstY = temp;
                this.DesignateRect(
                    this.curX + startOfstX, this.curY + startOfstY,
                    this.curX + endOfstX, this.curY + endOfstY,
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
                Texture[] tableTex = this.refIMapData.GetBlockTexture(mx, my);

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

            // 指定方向に進めるかチェックし、
            // 進めるなら指定方向に 1 ライン分視覚化領域をずらす
            public bool CheckMoveAndScroll(Direction dir)
            {
                bool b = this.refIMapData.IsMovable(this.curX, this.curY, dir);
                if (b) this.Scroll(dir);
                return b;
            }

            // 指定方向に 1 ライン分視覚化領域をずらす
            public void Scroll(Direction dir)
            {
                this.refStructure.Scroll(dir);

                int temp = this.showableBlockNum - 1;
                int sx = 0, sy = 0, ex = 0, ey = 0, vx = 0, vy = 0;
                switch (dir)
                {
                    case Direction.NORTH:
                        --this.curY;
                        sx = this.curX - temp; sy = this.curY - temp;
                        ex = this.curX + temp; ey = sy;
                        vx = -temp; vy = -temp;
                        break;

                    case Direction.SOUTH:
                        ++this.curY;
                        sx = this.curX - temp; sy = this.curY + temp;
                        ex = this.curX + temp; ey = sy;
                        vx = -temp; vy = temp;
                        break;

                    case Direction.WEST:
                        --this.curX;
                        sx = this.curX - temp; sy = this.curY - temp;
                        ex = sx; ey = this.curY + temp;
                        vx = -temp; vy = -temp;
                        break;

                    case Direction.EAST:
                        ++this.curX;
                        sx = this.curX + temp; sy = this.curY - temp;
                        ex = sx; ey = this.curY + temp;
                        vx = temp; vy = -temp;
                        break;

                    default: break;
                }

                this.DesignateRect(sx, sy, ex, ey, vx, vy);
            }

        } //class DungeonBuilder


        //------------------------------------------------------------------
        // EntityDungeon
        //------------------------------------------------------------------
        public class EntityDungeon : NpEntity, IEntityDungeon
        {
            private bool bInitialized = false;

            private DungeonStructure structure = null;
            private DungeonBuilder builder = null;


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

                this.EventProc();

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
                yield return this.ReadyResources();

                yield return this.Build();

                yield return this.ReadyPlayer();

                yield return Utility.FadeIn();

                this.bInitialized = true;
            }

            // リソース準備
            private IEnumerator ReadyResources()
            {
                IEntityTextureResources iTexRes = null;
                while ((iTexRes = Utility.GetIEntityTextureResources()) == null) yield return null;

                iTexRes.InitLogic();
                iTexRes.ReadyLogic();

                yield return null;
            }

            // ダンジョン構築
            private IEnumerator Build()
            {
                IEntityMapData iMapData = null;
                while ((iMapData = Utility.GetIEntityMapData()) == null) yield return null;

                iMapData.InitLogic();
                iMapData.ReadyLogic();

                do {
                    if (this.InitBuilder(iMapData) == null) break;
                    this.builder.Ready();

                    ////////////////////////////////////
                    // キャラクターの位置
                    // TODO: 本来はセーブデータから取得。ひとまず初期情報を指定。
                    MapData mapData = iMapData.GetMapData();
                    int x = mapData.startX;
                    int y = mapData.startY;
                    ////////////////////////////////////

                    this.builder.Create(x, y, Define.SHOWABLE_BLOCK);

                } while (false);

                yield return null;
            }

            // プレイヤー準備
            private IEnumerator ReadyPlayer()
            {
                IEntityPlayerData iPlayerData = null;
                while ((iPlayerData = Utility.GetIEntityPlayerData()) == null) yield return null;
                iPlayerData.InitLogic();
                iPlayerData.ReadyLogic();

                ////////////////////////////////////
                // キャラクターの位置と向き
                // TODO: 本来はセーブデータから取得。ひとまず初期情報を指定。
                MapData mapData = Utility.GetIEntityMapData().GetMapData();
                int x = mapData.startX;
                int y = mapData.startY;
                Direction dir = mapData.startDir;
                ////////////////////////////////////

                PlayerData playerData = iPlayerData.GetPlayerData();
                playerData.SetPos(x, y);
                playerData.SetDirection(dir);

                IEntityPlayer iPlayer = null;
                while ((iPlayer = Utility.GetIEntityPlayer()) == null) yield return null;
                iPlayer.InitLogic(Global.Instance.cameraPlayer, Global.Instance.objCameraPlayerBase);
                iPlayer.ReadyLogic(playerData);

                yield return null;
            }

            // ダンジョン構築オブジェクトの準備
            private DungeonBuilder InitBuilder(IEntityMapData iMapData)
            {
                do {
                    this.structure = new DungeonStructure();
                    if (this.structure == null) break;

                    this.builder = new DungeonBuilder();
                    if (this.builder == null) break;

                    this.builder.Init(this.structure, iMapData);

                } while (false);

                return this.builder;
            }


            //------------------------------------------------------------------
            // イベント処理
            //------------------------------------------------------------------

            private void EventProc()
            {
                IEntityPlayer iPlayer = Utility.GetIEntityPlayer();
                if (iPlayer.IsBusy()) return;

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    iPlayer.RotateLeft();
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    iPlayer.RotateRight();
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    if (this.builder.CheckMoveAndScroll(iPlayer.GetPlayerData().dir))
                    {
                        iPlayer.MoveFront();
                    }
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    if (this.builder.CheckMoveAndScroll(Utility.GetOppositeDirection(iPlayer.GetPlayerData().dir)))
                    {
                        iPlayer.MoveBack();
                    }
                }
            }

        } //class EntityDungeon

    } //namespace entity
} //namespace nangka