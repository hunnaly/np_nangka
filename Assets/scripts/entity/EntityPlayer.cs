using UnityEngine;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityPlayer
        //------------------------------------------------------------------
        public interface IEntityPlayer
        {
            bool IsReadyLogic();
            void Prepare(Camera camera, GameObject objBase, PlayerData data);
            void Reset();

            PlayerData GetPlayerData();

            void RotateRight();
            void RotateLeft();
            void MoveFront();
            void MoveBack();
            bool IsBusy();

            void Pause(bool pause);
            void Terminate();

        } //interface IEntityPlayer


        //------------------------------------------------------------------
        // EntityPlayer
        //------------------------------------------------------------------
        public class EntityPlayer : NpEntity, IEntityPlayer
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private CameraControl _cameraCtrl;
            private PlayerData _refPlayerData;
            public PlayerData GetPlayerData() { return this._refPlayerData; }

            private bool IsPrepared() { return (this._refPlayerData != null); }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityPlayer.StartProc()");

                // TODO: 例外エラー対応を行う必要がある
                this._cameraCtrl = new CameraControl();

                this._bReadyLogic = true;
                return true;
            }

            protected override bool UpdateProc()
            {
                this.UpdateLogic();
                return false;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityPlayer.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this._cameraCtrl = null;
            }


            //------------------------------------------------------------------
            // ロジック準備処理／ロジックリセット処理
            //------------------------------------------------------------------

            public void Prepare(Camera camera, GameObject objBase, PlayerData data)
            {
                if (!this.IsReadyLogic()) return;
                if (this.IsPrepared()) return;

                this._refPlayerData = data;
                this._cameraCtrl.SetCamera(camera, objBase);
                this._cameraCtrl.SetDir(data.dir);
            }

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                this._refPlayerData = null;

                this._bReadyLogic = false;
            }

            //------------------------------------------------------------------
            // ロジック更新処理
            //------------------------------------------------------------------

            private void UpdateLogic()
            {
                if (!this.IsPrepared()) return;

                this._cameraCtrl.MoveProc();
                this._cameraCtrl.RotateProc();
            }

            //------------------------------------------------------------------
            // プレイヤー回転処理
            //------------------------------------------------------------------

            public void RotateRight()
            {
                if (!this.IsPrepared()) return;

                Direction dir = this._refPlayerData.dir;
                this.Rotate(dir, false, Utility.DirectionRight(dir));
            }

            public void RotateLeft()
            {
                if (!this.IsPrepared()) return;

                Direction dir = this._refPlayerData.dir;
                this.Rotate(dir, true, Utility.DirectionLeft(dir));
            }

            private void Rotate(Direction dirFrom, bool bRotMinus, Direction dirTo)
            {
                if (!this.IsReadyLogic()) return;

                if (this._cameraCtrl.Rotate(Utility.DirectionToAngleY(dirFrom), bRotMinus))
                {
                    this._refPlayerData.SetDirection(dirTo);
                }
            }

            //------------------------------------------------------------------
            // プレイヤー移動処理
            //------------------------------------------------------------------

            public void MoveFront()
            {
                if (!this.IsPrepared()) return;

                this.Move(this._refPlayerData.dir);
            }

            public void MoveBack()
            {
                if (!this.IsPrepared()) return;

                this.Move(Utility.GetOppositeDirection(this._refPlayerData.dir));
            }

            private void Move(Direction dir)
            {
                if (!this.IsReadyLogic()) return;

                if (this._cameraCtrl.Move(dir))
                {
                    this.MovePlayerPos(dir);
                }
            }

            private void MovePlayerPos(Direction dir)
            {
                int x = this._refPlayerData.x;
                int y = this._refPlayerData.y;

                switch (dir)
                {
                    case Direction.NORTH: --y; break;
                    case Direction.SOUTH: ++y; break;
                    case Direction.WEST: --x; break;
                    case Direction.EAST: ++x; break;
                    default: break;
                }
                this._refPlayerData.SetPos(x, y);
            }

            //------------------------------------------------------------------
            // プレイヤー処理状態の取得
            //------------------------------------------------------------------

            public bool IsBusy()
            {
                if (!this.IsPrepared()) return false;
                return (this._cameraCtrl.IsMoving() || this._cameraCtrl.IsRotating());
            }


            //------------------------------------------------------------------
            // プレイヤーカメラ制御クラス
            //------------------------------------------------------------------
            class CameraControl
            {
                private GameObject _refCamera;
                private Camera _refCameraReal; //使わないかも？

                //------------------------------------------------------------------
                // 移動処理関連変数
                //------------------------------------------------------------------

                private bool _bMoving;
                public bool IsMoving() { return this._bMoving; }

                private int _moveCount;
                private Direction _moveDir;
                private float _fCameraX;
                private float _fCameraY;
                private float _fCameraZ;
                private float[] _tableMoveDelta = { 2.0f, 1.0f, 0.5f, 0.25f, 0.25f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

                //------------------------------------------------------------------
                // 回転処理関連変数
                //------------------------------------------------------------------

                private bool _bRotating;
                public bool IsRotating() { return this._bRotating; }

                private int _rotCount;
                private float _fAngle;
                private bool _bRotMinus;
                private float[] _tableRotDelta = { 40, 0f, 20.0f, 15.0f, 10.0f, 5.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };


                //------------------------------------------------------------------
                // 各種設定処理
                //------------------------------------------------------------------

                public void SetCamera(Camera camera, GameObject objBase)
                {
                    this._refCameraReal = camera;
                    this._refCamera = objBase;

                    Vector3 pos = this._refCamera.transform.position;
                    this.SetPosBuf(pos);
                }

                //------------------------------------------------------------------
                // カメラ設定
                //------------------------------------------------------------------

                public Vector3 GetPos() { return this._refCamera.transform.position; }

                public void SetPos(Vector3 pos, bool bUpdateBuf=false)
                {
                    this._refCamera.transform.position = pos;
                    if (bUpdateBuf) this.SetPosBuf(pos);
                }
                private void SetPosBuf(Vector3 pos)
                {
                    this._fCameraX = pos.x;
                    this._fCameraY = pos.y;
                    this._fCameraZ = pos.z;
                }

                public void SetDir(Direction dir) { this.SetCameraAngleY(Utility.DirectionToAngleY(dir)); }

                private void SetCameraAngleY(float angle)
                {
                    this._refCamera.transform.rotation =
                        Quaternion.AngleAxis(angle, new Vector3(0.0f, 1.0f, 0.0f));
                }

                //------------------------------------------------------------------
                // カメラ回転
                //------------------------------------------------------------------

                public bool RotateRight(float curAngleY) { return this.StartRotate(curAngleY, false); }

                public bool RotateLeft(float curAngleY) { return this.StartRotate(curAngleY, true); }

                public bool Rotate(float curAngleY, bool bRotMinus) { return this.StartRotate(curAngleY, bRotMinus); }

                private bool StartRotate(float curAngleY, bool bRotMinus)
                {
                    if (this.IsMoving() || this.IsRotating()) return false;

                    this._fAngle = curAngleY;
                    this._bRotMinus = bRotMinus;
                    this._rotCount = 0;
                    this._bRotating = true;

                    return true;
                }

                //------------------------------------------------------------------
                // カメラ移動
                //------------------------------------------------------------------

                public bool Move(Direction dir) { return this.StartMove(dir); }

                private bool StartMove(Direction dir)
                {
                    if (this.IsMoving() || this.IsRotating()) return false;

                    this._moveDir = dir;
                    this._moveCount = 0;
                    this._bMoving = true;

                    return true;
                }

                //------------------------------------------------------------------
                // 更新処理
                //------------------------------------------------------------------

                public void MoveProc()
                {
                    if (!this.IsMoving()) return;

                    float delta = this._tableMoveDelta[this._moveCount];
                    switch (this._moveDir)
                    {
                        case Direction.NORTH: this._fCameraZ += delta; break;
                        case Direction.SOUTH: this._fCameraZ -= delta; break;
                        case Direction.WEST: this._fCameraX -= delta; break;
                        case Direction.EAST: this._fCameraX += delta; break;
                        default: break;
                    }
                    Vector3 pos = this.GetPos();
                    pos.x = this._fCameraX; pos.y = this._fCameraY; pos.z = this._fCameraZ;
                    this.SetPos(pos);

                    if (++this._moveCount >= this._tableMoveDelta.Length)
                    {
                        this._moveCount = 0;
                        this._bMoving = false;
                    }
                }

                public void RotateProc()
                {
                    if (!this.IsRotating()) return;

                    float delta = this._tableRotDelta[this._rotCount];
                    this._fAngle += delta * (this._bRotMinus ? -1.0f : 1.0f);
                    this.SetCameraAngleY(this._fAngle);

                    if (++this._rotCount >= this._tableRotDelta.Length)
                    {
                        this._rotCount = 0;
                        this._bRotating = false;
                    }
                }

            } // class CameraControl


        } //class EntityPlayer

    } //namespace entity
} //namespace nangka
