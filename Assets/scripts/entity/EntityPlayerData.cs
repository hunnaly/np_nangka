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
        // PlayerData
        //------------------------------------------------------------------
        public class PlayerData
        {
            private int _x;
            public int x { get { return this._x; } }

            private int _y;
            public int y { get { return this._y; } }

            private Direction _dir;
            public Direction dir { get { return this._dir; } }


            //------------------------------------------------------------------
            // 各種設定メソッド
            //------------------------------------------------------------------

            public void SetPos(int x, int y) { this._x = x; this._y = y; }
            public void SetDirection(Direction dir) { this._dir = dir; }

            // リセット処理
            public void Reset()
            {
                this._x = 0;
                this._y = 0;
                this._dir = 0;
            }

        } //class PlayerData


        //------------------------------------------------------------------
        // IEntityPlayerData
        //------------------------------------------------------------------
        public interface IEntityPlayerData
        {
            void InitLogic();
            void ReadyLogic();
            void Reset();
            void Clear();

            PlayerData GetPlayerData();
            void LoadPlayerData();
            /*
            void SavePlayerData();
            */

            void Terminate();

        } //interface IEntityPlayerData


        //------------------------------------------------------------------
        // EntityPlayerData
        //------------------------------------------------------------------
        public class EntityPlayerData : NpEntity, IEntityPlayerData
        {
            //------------------------------------------------------------------
            // 初期化関連変数
            //------------------------------------------------------------------

            private bool _bInitializedLogic;
            private bool IsInitializedLogic() { return this._bInitializedLogic; }

            private PlayerData _data;

            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            private bool IsReadyLogic() { return this._bReadyLogic; }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool UpdateProc()
            {
                return false;
            }

            protected override void CleanUp()
            {
                this.Clear();
            }

            //------------------------------------------------------------------
            // ロジック初期化処理／ロジック終了処理
            //------------------------------------------------------------------

            public void InitLogic()
            {
                if (this.IsInitializedLogic()) return;

                bool b = false;
                this._data = new PlayerData();
                if (this._data != null) { b = true; }

                this._bInitializedLogic = b;
            }

            public void Clear()
            {
                if (!this.IsInitializedLogic()) return;

                this.Reset();

                this._data = null;
                this._bInitializedLogic = true;
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
                if (!this.IsReadyLogic()) return;

                this._data.Reset();
                this._bReadyLogic = false;
            }

            //------------------------------------------------------------------
            // PlayerData 関連
            //------------------------------------------------------------------

            public PlayerData GetPlayerData() { return this._data; }

            public void LoadPlayerData(/*セーブデータ指定*/)
            {
                if (!this.IsReadyLogic()) return;

                this.LoadDummyPlayerData();
            }

            private void LoadDummyPlayerData()
            {
                this._data.SetPos(0, 0);
                this._data.SetDirection(Direction.EAST);
            }

        } //class EntityPlayerData

    } //namespace entity
} //namespace nangka
