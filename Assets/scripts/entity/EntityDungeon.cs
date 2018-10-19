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
        public interface IEntityDungeon : IEntity
        {
            void Run();

            void Reset();
            void Restart();

        } //interface IEntityDungeon


        //------------------------------------------------------------------
        // EntityDungeon
        //------------------------------------------------------------------
        public class EntityDungeon : NpEntity, IEntityDungeon
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private DungeonBuilder builder;

            private int movePreX;
            private int movePreY;
            private Vector3 vecMoveDelta;

            public void Run()
            {
                if (!this.IsReadyLogic()) return;
                this.Pause(false);
            }

            public void Reset()
            {
                this.builder.Reset();

                IEntityPlayer iPlayer = Utility.GetIEntityPlayer();
                iPlayer.Reset();
            }

            public void Restart()
            {
                Debug.Log("-------Restart()----------");

                if (!this.IsReadyLogic()) return;

                this._bReadyLogic = false;
                this.Pause(true);


                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();

                IEntityMapData iMapData = Utility.GetIEntityMapData();
                iMapData.Through(iPlayerData.GetX(), iPlayerData.GetY());

                this.builder.Ready();
                this.builder.Create(iPlayerData.GetX(), iPlayerData.GetY(), Define.SHOWABLE_BLOCK, Global.Instance.objCameraPlayerBase.transform.localPosition);

                IEntityPlayer iPlayer = Utility.GetIEntityPlayer();
                iPlayer.Prepare(Global.Instance.cameraPlayer, Global.Instance.objCameraPlayerBase, iPlayerData);

                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Flash(iMapData, iPlayerData.IsForceOpenMiniMap());
                iMiniMap.Move(iPlayerData.GetX(), iPlayerData.GetY(), Vector3.zero);
                iMiniMap.Rotate(Utility.DirectionToAngleY(iPlayerData.GetDir()));

                this._bReadyLogic = true;
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityDungeon.StartProc()");

                Utility.StartCoroutine(this.ReadyLogic());
                return true;
            }

            protected override bool UpdateProc()
            {
                this.UpdateLogic();

                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityDungeon.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this.ClearBuilder();
            }

            private void ClearBuilder()
            {
                if (this.builder != null) { this.builder.Clear(); this.builder = null; }
            }

            //------------------------------------------------------------------
            // ロジック更新処理
            //------------------------------------------------------------------

            private void UpdateLogic()
            {
                if (!this.IsReadyLogic()) return;

                this.EventProc();
            }


            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator ReadyLogic()
            {
                yield return this.Build();

                yield return this.ReadyPlayer();

                // 一時停止状態にしておく
                // 外部から明示的に起動する
                this.Pause(true);

                this._bReadyLogic = true;
            }

            // ダンジョン構築
            private IEnumerator Build()
            {
                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                IEntityStructure iStructure = Utility.GetIEntityStructure();

                do {
                    this.builder = new DungeonBuilder();
                    if (this.builder == null) break;

                    this.builder.Init(iStructure, iMapData);
                    this.builder.Ready();

                    this.builder.Create(iPlayerData.GetX(), iPlayerData.GetY(), Define.SHOWABLE_BLOCK, Global.Instance.objCameraPlayerBase.transform.localPosition);

                } while (false);

                yield return null;
            }

            // プレイヤー準備
            private IEnumerator ReadyPlayer()
            {
                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();

                // 初期位置に踏破フラグをたてる
                IEntityMapData iMapData = Utility.GetIEntityMapData();
                iMapData.Through(iPlayerData.GetX(), iPlayerData.GetY());

                IEntityPlayer iPlayer = Utility.GetIEntityPlayer();
                iPlayer.SetCB_MoveStart(this.CB_Player_MoveStart);
                iPlayer.SetCB_Move(this.CB_Player_Move);
                iPlayer.SetCB_MoveEnd(this.CB_Player_MoveEnd);
                iPlayer.SetCB_Rotate(this.CB_Player_Rotate);
                iPlayer.Prepare(Global.Instance.cameraPlayer, Global.Instance.objCameraPlayerBase, iPlayerData);


                // ミニマップに初期位置を反映
                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Flash(iMapData, iPlayerData.IsForceOpenMiniMap());
                iMiniMap.Move(iPlayerData.GetX(), iPlayerData.GetY(), Vector3.zero);
                iMiniMap.Rotate(Utility.DirectionToAngleY(iPlayerData.GetDir()));

                yield return null;
            }


            //------------------------------------------------------------------
            // イベント処理
            //------------------------------------------------------------------

            private void EventProc()
            {
                IEntityPlayer iPlayer = Utility.GetIEntityPlayer();
                if (iPlayer.IsBusy()) return;

                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();

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
                    if (this.builder.CheckMoveAndScroll(iPlayerData.GetDir()))
                    {
                        iPlayer.MoveFront();
                    }
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    if (this.builder.CheckMoveAndScroll(Utility.GetOppositeDirection(iPlayerData.GetDir())))
                    {
                        iPlayer.MoveBack();
                    }
                }
            }

            //------------------------------------------------------------------
            // ミニマップ処理
            // Player に連動させて処理を行っている。
            //------------------------------------------------------------------

            private void CB_Player_MoveStart(int xStart, int yStart, int xEnd, int yEnd)
            {
                this.movePreX = xStart;
                this.movePreY = yStart;
                this.vecMoveDelta = new Vector3(0.0f, 0.0f, 0.0f);

                IEntityMapData iMapData = Utility.GetIEntityMapData();
                iMapData.Through(xEnd, yEnd);

                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Flash(iMapData, xEnd, yEnd);
            }

            private void CB_Player_Move(Vector3 delta)
            {
                this.vecMoveDelta.x += delta.x * 20.0f;
                this.vecMoveDelta.y += delta.z * 20.0f;

                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Move(this.movePreX, this.movePreY, this.vecMoveDelta);
            }

            private void CB_Player_MoveEnd()
            {
                this.vecMoveDelta = Vector3.zero;

                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();

                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Move(iPlayerData.GetX(), iPlayerData.GetY(), this.vecMoveDelta);
            }

            private void CB_Player_Rotate(float fAngle)
            {
                IEntityMiniMap iMiniMap = Utility.GetIEntityMiniMap();
                iMiniMap.Rotate(fAngle);
            }


            //------------------------------------------------------------------
            // DungeonBuilder
            //------------------------------------------------------------------
            class DungeonBuilder
            {
                private bool bValid = false;
                public bool IsValid() { return this.bValid; }

                private bool bReady = false;
                public bool IsReady() { return this.bReady; }

                private int curX = 0;
                private int curY = 0;
                private int showableBlockNum = 0;
                private IEntityStructure refStructure = null;
                private IEntityMapData refIMapData = null;
                private GameObject prefabWall = null;


                // 初期化
                public void Init(IEntityStructure refStructure, IEntityMapData refIMapData)
                {
                    if (this.IsValid()) return;

                    this.refStructure = refStructure;
                    this.refIMapData = refIMapData;
                    this.prefabWall = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_WALL);

                    this.bValid = true;
                }

                // 準備処理
                public void Ready()
                {
                    if (!this.IsValid() || this.IsReady()) return;
                    this.bReady = true;
                }

                // 初期化情報を残した状態でリセット
                public void Reset()
                {
                    if (!this.IsReady()) return;

                    if (this.refStructure != null) this.refStructure.Reset();
                    if (this.refIMapData != null) this.refIMapData.Reset();

                    this.bReady = false;
                }

                // 初期化情報も含めてクリア
                public void Clear()
                {
                    this.Reset();

                    if (this.prefabWall) { Resources.UnloadAsset(this.prefabWall); this.prefabWall = null; }
                    this.refIMapData = null;
                    this.refStructure = null;

                    this.bValid = false;
                }

                // ダンジョン生成
                // 指定座標の周りを視覚化
                public void Create(int x, int y, int num, Vector3 basePos)
                {
                    if (!this.IsReady()) return;

                    this.curX = x;
                    this.curY = y;
                    this.showableBlockNum = num;

                    this.refStructure.Prepare(this.prefabWall, this.showableBlockNum, basePos);

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
                            this.Designeate(x, y, vx + (x - sx), vy + (y - sy));
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

        } //class EntityDungeon

    } //namespace entity
} //namespace nangka