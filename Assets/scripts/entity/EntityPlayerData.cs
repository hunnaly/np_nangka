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
        public interface IEntityPlayerData : IEntity
        {
            void Reset();

            PlayerData GetPlayerData();
            void LoadPlayerData();
            /*
            void SavePlayerData();
            */

        } //interface IEntityPlayerData


        //------------------------------------------------------------------
        // EntityPlayerData
        //------------------------------------------------------------------
        public class EntityPlayerData : NpEntity, IEntityPlayerData
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private PlayerData _data;
            public PlayerData GetPlayerData() { return this._data; }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityPlayerData.StartProc()");

                // TODO: 例外エラー対応が必要
                this._data = new PlayerData();

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityPlayerData.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this._data = null;
            }

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                this._data.Reset();
            }

            //------------------------------------------------------------------
            // PlayerData 関連
            //------------------------------------------------------------------

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
