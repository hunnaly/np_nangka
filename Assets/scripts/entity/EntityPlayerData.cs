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
        // IEntityPlayerData
        //------------------------------------------------------------------
        public interface IEntityPlayerData : IEntity
        {
            void Reset();
            bool IsLoaded();

            MAP_ID GetMapID();
            int GetX();
            int GetY();
            Direction GetDir();
            void SetDir(Direction dir);
            void SetPos(int x, int y);

            EntityPlayerData GetOwnEntity();

        } //interface IEntityPlayerData


        //------------------------------------------------------------------
        // EntityPlayerData
        //------------------------------------------------------------------
        public class EntityPlayerData : NpEntity, IEntityPlayerData,
            EntityRecreator.IPlayerDataRecreator
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }


            private bool _bRecreating;

            private bool _bLoaded;
            public bool IsLoaded() { return this._bLoaded; }

            private MAP_ID _idMap;
            public MAP_ID GetMapID() { return this._idMap; }

            private int _x;
            private int _y;
            public int GetX() { return this._x; }
            public int GetY() { return this._y; }
            public void SetPos(int x, int y) { this._x = x; this._y = y; }

            private Direction _dir;
            public Direction GetDir() { return this._dir; }
            public void SetDir(Direction dir) { this._dir = dir; }

            public EntityPlayerData GetOwnEntity() { return this; }


            //------------------------------------------------------------------
            // データ設定メソッド（EntityNewCreator用）
            //------------------------------------------------------------------

            void EntityRecreator.IPlayerDataRecreator.Begin()
            {
                if (this._bRecreating) return;

                this.Reset();

                this._bRecreating = true;
                this._bLoaded = false;
            }

            void EntityRecreator.IPlayerDataRecreator.SetMap(MAP_ID id)
            {
                if (this._bRecreating == false) return;

                this._idMap = id;
            }

            void EntityRecreator.IPlayerDataRecreator.SetPos(int x, int y)
            {
                if (this._bRecreating == false) return;

                this.SetPos(x, y);
            }

            void EntityRecreator.IPlayerDataRecreator.SetDir(Direction dir)
            {
                if (this._bRecreating == false) return;

                this.SetDir(dir);
            }

            void EntityRecreator.IPlayerDataRecreator.End()
            {
                if (this._bRecreating == false) return;

                this._bRecreating = false;
                this._bLoaded = true;
            }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityPlayerData.StartProc()");

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
            }

            public void Reset()
            {
                if (!this.IsReadyLogic()) return;

                this._x = 0;
                this._y = 0;
                this._dir = 0;
            }

        } //class EntityPlayerData

    } //namespace entity
} //namespace nangka
