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
            }

            enum MarkerType : int
            {
                FRONT = 0,
                BACK = 1,
                LEFT = 2,
                RIGHT = 3,
                MAX = 4
            }

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
            private int numBlock = 0;
            private Marker[] marker = new Marker[(int)MarkerType.MAX];


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

                this.numSide = showableBlockNum * 2 + 1;
                this.numBlock = this.numSide * this.numSide;

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
            // ブロック構築
            //------------------------------------------------------------------

            private void Build()
            {
                // Direction.North 方向を向いている状態の
                // numBlock x numBlock 分の Texture なしかつ 非表示のブロックを作成する
                // 各ブロックは隣のブロックと繋がりをもっている
                // 両端同士も繋がりをもつループ構造とする
                // 構築処理には numBlock に適切な値が入っている必要があることに注意

                this.BuildBlockLine(0, null);

                // マーカーを頼りに両端のブロックを連結

            }

            private void BuildBlockLine(int line, Block backLineBlock)
            {
                if (line >= this.numBlock) return;

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
                if (num >= this.numBlock) return;

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

                float sx = (this.numBlock - 1) / 2 * -4.0f;
                float sz = (this.numBlock - 1) / 2 * 4.0f;
                float x = sx + num * 4.0f;
                float z = sz - line * 4.0f;
                Vector3 trans = new Vector3(x, 0.0f, z);
                obj.transform.localPosition = trans;

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

                // Left から Right への向きを Frontマーカーから取得
                Direction nextDir = this.marker[(int)MarkerType.FRONT].dir;

                // 初期位置と次ライン方向を Leftマーカーから取得
                Block start = this.marker[(int)MarkerType.LEFT].start;
                Direction nextLineDir = this.marker[(int)MarkerType.LEFT].dir;

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
                    this.marker[(int)MarkerType.FRONT].start = block;
                    this.marker[(int)MarkerType.FRONT].dir = Direction.EAST;
                }
            }
            private void CheckAndSetBackMarkerFirstly(Block block, int line, int num)
            {
                if (line == this.numBlock - 1 && num == 0)
                {
                    this.marker[(int)MarkerType.BACK].start = block;
                    this.marker[(int)MarkerType.BACK].dir = Direction.EAST;
                }
            }
            private void CheckAndSetLeftMarkerFirstly(Block block, int line, int num)
            {
                if (line == 0 && num == 0)
                {
                    this.marker[(int)MarkerType.LEFT].start = block;
                    this.marker[(int)MarkerType.LEFT].dir = Direction.SOUTH;
                }
            }
            private void CheckAndSetRightMarkerFirstly(Block block, int line, int num)
            {
                if (line == 0 && num == this.numBlock - 1)
                {
                    this.marker[(int)MarkerType.RIGHT].start = block;
                    this.marker[(int)MarkerType.RIGHT].dir = Direction.SOUTH;
                }
            }

            private void ResetMarker()
            {
                for (int i = 0; i < (int)MarkerType.MAX; i++)
                {
                    this.marker[i].start = null;
                }
            }


            //------------------------------------------------------------------
            // 壁オブジェクト構築処理
            //------------------------------------------------------------------

            /*
            private void ChangeTexture(GameObject obj, Texture tex)
            {
                obj.GetComponent<Renderer>().material.mainTexture = tex;
                obj.GetComponent<Renderer>().material.color = Color.white;
            }


            //------------------------------------------------------------------
            // セルの再構築処理
            //------------------------------------------------------------------

            public void ChangeWall(Direction dir, Texture tex)
            {
                switch (dir)
                {
                    case Direction.UP: this.ChangeWallUp(tex); break;
                    case Direction.DOWN: this.ChangeWallDown(tex); break;
                    case Direction.NORTH: this.ChangeWallNorth(tex); break;
                    case Direction.SOUTH: this.ChangeWallSouth(tex); break;
                    case Direction.EAST: this.ChangeWallEast(tex); break;
                    case Direction.WEST: this.ChangeWallWest(tex); break;
                    default: break;
                }
            }

            public void ChangeWallUp(Texture tex)
            {
                this.objWallUp = this.ChangeWallReal(Direction.UP, this.objWallUp, tex);
            }
            public void ChangeWallDown(Texture tex)
            {
                this.objWallDown = this.ChangeWallReal(Direction.DOWN, this.objWallDown, tex);
            }
            public void ChangeWallNorth(Texture tex)
            {
                this.objWallNorth = this.ChangeWallReal(Direction.NORTH, this.objWallNorth, tex);
            }
            public void ChangeWallSouth(Texture tex)
            {
                this.objWallSouth = this.ChangeWallReal(Direction.SOUTH, this.objWallSouth, tex);
            }
            public void ChangeWallEast(Texture tex)
            {
                this.objWallEast = this.ChangeWallReal(Direction.EAST, this.objWallEast, tex);
            }
            public void ChangeWallWest(Texture tex)
            {
                this.objWallWest = this.ChangeWallReal(Direction.WEST, this.objWallWest, tex);
            }

            private GameObject ChangeWallReal(Direction dir, GameObject obj, Texture tex)
            {
                GameObject retObject = obj;

                // 壁を消す
                if (tex == null)
                {
                    // 対象のオブジェクトがないと消せないので存在チェック
                    if (obj != null)
                    {
                        Object.Destroy(obj);
                    }
                }
                // 壁をつくる or 変更する
                else
                {
                    // 対象のオブジェクトがないとき作成
                    if (obj == null)
                    {
                        retObject = this.CreateWall(dir, obj, tex);
                    }

                    // テクスチャ変更
                    this.ChangeTexture(retObject, tex);
                }

                return retObject;
            }
            */


        } //class DungeonStructure


        //------------------------------------------------------------------
        // DungeonBuilder
        //------------------------------------------------------------------
        public class DungeonBuilder
        {
            private bool bValid = false;
            private DungeonStructure refStructure = null;
            private GameObject prefabWall = null;

            private Texture texWallCeiling = null;
            private Texture refTexWallSideWalk = null;
            private Texture texWallSideWall = null;


            // 初期化
            public void Init(DungeonStructure structure)
            {
                this.refStructure = structure;
                this.bValid = true;
            }

            // 準備処理
            // TODO: マップ情報(マップで利用されるテクスチャ情報が必要)を引数でとるべし！
            public void Ready()
            {
                this.prefabWall = (GameObject)Resources.Load(Define.RES_PATH_PREFAB_WALL);
                this.refStructure.Init(this.prefabWall);

                //TODO: マップ情報を読み込んで、必要なテクスチャをロードする
                this.texWallCeiling = (Texture)Resources.Load(Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                this.texWallSideWall = (Texture)Resources.Load(Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);
                this.refTexWallSideWalk = this.texWallCeiling;
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {
                if (this.refStructure != null) this.refStructure.Clear();

                if (this.texWallCeiling != null) { Resources.UnloadAsset(this.texWallCeiling); this.texWallCeiling = null; }
                if (this.texWallSideWall != null) { Resources.UnloadAsset(this.texWallSideWall); this.texWallSideWall = null; }
                this.refTexWallSideWalk = null;
            }

            // 初期化情報も含めてクリア
            public void Clear()
            {
                this.Reset();

                this.refStructure = null;
                this.bValid = false;
            }

            // 初期化情報に基づいてダンジョン生成
            // TODO: キャラクター位置と向きを引数でとるべし！
            public void Create()
            {
                if (!this.bValid) return;

                this.refStructure.Ready(Define.SHOWABLE_BLOCK);


                //TODO: キャラクター位置と向きとマップ情報から壁情報を変更する

                // キャラクター位置

            }

        } //class DungeonBuilder


        //------------------------------------------------------------------
        // EntityDungeon
        //------------------------------------------------------------------
        public class EntityDungeon : NpEntity, IEntityDungeon
        {
            private bool bValid = false;

            private DungeonStructure structure = null;
            private DungeonBuilder builder = null;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityDungeon.StartProc()");
                this.bValid = false;
                Utility.StartCoroutine(this.Ready());
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this.bValid == false) return false;
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
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator Ready()
            {
                yield return this.Build();

                yield return Utility.FadeIn();

                this.bValid = true;
            }

            // ダンジョン構築
            private IEnumerator Build()
            {
                do {
                    if (this.ReadyBuilder() == null) break;
                    this.builder.Create();

                } while (false);

                yield return null;
            }

            // ダンジョン構築オブジェクトの準備
            private DungeonBuilder ReadyBuilder()
            {
                do {
                    this.structure = new DungeonStructure();
                    if (this.structure == null) break;

                    this.builder = new DungeonBuilder();
                    if (this.builder == null) break;

                    this.builder.Init(this.structure);
                    this.builder.Ready();
                    this.builder.Create();

                } while (false);

                return this.builder;
            }

        } //class EntityDungeon

    } //namespace entity
} //namespace nangka