using System.Collections;
using UnityEngine;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityNewCreator
        //------------------------------------------------------------------
        public interface IEntityRecreator : IEntity
        {
            void Run(EntityRecreator.MODE mode = EntityRecreator.MODE.NORMAL);
            bool IsFinished();

        } //interface IEntityNewCreator


        //------------------------------------------------------------------
        // EntityNewCreator
        // [Need Entity] EntityPlayerData, EntityMapData
        //------------------------------------------------------------------
        public class EntityRecreator : NpEntity, IEntityRecreator
        {
            public enum MODE
            {
                NORMAL,
                EMPTY_MAP,
                DUMMY_MAP

            } // enum MODE

            //------------------------------------------------------------------
            // EntityMapData にマップデータを設定するためのインタフェース
            // EntityMapData は本インタフェースを明示的に継承する
            //------------------------------------------------------------------
            public interface IMapDataRecreator
            {
                void Begin(string name, int width, int height);
                void AddTexture(byte id, string path);
                void SetBlock(int idx, EntityMapData.BlockData data);
                void End();
            }

            //------------------------------------------------------------------
            // EntityPlayerData にマップデータを設定するためのインタフェース
            // EntityPlayerData は本インタフェースを明示的に継承する
            //------------------------------------------------------------------
            public interface IPlayerDataRecreator
            {
                void Begin();
                void SetMap(MAP_ID id);
                void SetPos(int x, int y);
                void SetDir(Direction dir);
                void End();
            }


            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private bool _bRunning;

            private bool _bFinished;
            public bool IsFinished() { return this._bFinished; }


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityNewCreator.StartProc()");

                this._bReadyLogic = true;
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityNewCreator.TerminateProc()");

                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
            }

            //------------------------------------------------------------------
            // データ再設定実行処理
            //------------------------------------------------------------------

            public void Run(EntityRecreator.MODE mode = EntityRecreator.MODE.NORMAL)
            {
                if (!this.IsReadyLogic()) return;
                if (this._bRunning) return;

                this._bRunning = true;
                this._bFinished = false;
                Utility.StartCoroutine(this.Recreate(mode));
            }

            private IEnumerator Recreate(EntityRecreator.MODE mode)
            {
                //TODO: mode によって切り替える

                yield return this.CreateNewData();
            }

            private IEnumerator CreateNewData()
            {
                IEntityPlayerData iPlayerData = Utility.GetIEntityPlayerData();
                IEntityMapData iMapData = Utility.GetIEntityMapData();

                // PlayerData
                //this.SetEmptyPlayerData((IPlayerDataRecreator)(iPlayerData.GetOwnEntity()));
                this.SetDummyPlayerData((IPlayerDataRecreator)(iPlayerData.GetOwnEntity()));

                // MapData
                //this.SetEmptyMapData((IMapDataRecreator)(iMapData.GetOwnEntity()));
                this.SetDummyMapData((IMapDataRecreator)(iMapData.GetOwnEntity()));

                this._bFinished = true;
                yield return null;
            }

            private void SetEmptyPlayerData(IPlayerDataRecreator recreator)
            {
                recreator.Begin();
                recreator.SetMap(MAP_ID.MAP_DUMMY);
                recreator.SetPos(0, 0);
                recreator.SetDir(Direction.EAST);
                recreator.End();
            }

            private void SetDummyPlayerData(IPlayerDataRecreator recreator)
            {
                this.SetEmptyPlayerData(recreator);
            }

            private void SetEmptyMapData(IMapDataRecreator recreator)
            {
                recreator.Begin("new map", 8, 8);

                recreator.AddTexture(1, Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                recreator.AddTexture(2, Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);

                for (int y=0; y<8; y++)
                {
                    for (int x=0; x<8; x++)
                    {
                        int id = y * 8 + x;

                        if (y == 0)
                        {
                            if (x == 0) recreator.SetBlock(id, DNewBlock(2, 0, 0, 2, 1, 1, true, false, false, true));
                            else if (x == 7) recreator.SetBlock(id, DNewBlock(2, 2, 0, 0, 1, 1, true, true, false, false));
                            else recreator.SetBlock(id, DNewBlock(2, 0, 0, 0, 1, 1, true, false, false, false));
                        }
                        else if (y == 7)
                        {
                            if (x == 0) recreator.SetBlock(id, DNewBlock(0, 0, 2, 2, 1, 1, false, false, true, true));
                            else if (x == 7) recreator.SetBlock(id, DNewBlock(0, 2, 2, 0, 1, 1, false, true, true, false));
                            else recreator.SetBlock(id, DNewBlock(0, 0, 2, 0, 1, 1, false, false, true, false));
                        }
                        else
                        {
                            if (x == 0) recreator.SetBlock(id, DNewBlock(0, 0, 0, 2, 1, 1, false, false, false, true));
                            else if (x == 7) recreator.SetBlock(id, DNewBlock(0, 2, 0, 0, 1, 1, false, true, false, false));
                            else recreator.SetBlock(id, DNewBlock(0, 0, 0, 0, 1, 1, false, false, false, false));
                        }
                    }
                }

                recreator.End();
            }

            private void SetDummyMapData(IMapDataRecreator recreator)
            {
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

                recreator.Begin("dummy", 8, 8);

                recreator.AddTexture(1, Define.RES_PATH_TEXTURE_WALL_BRICK_CEILING);
                recreator.AddTexture(2, Define.RES_PATH_TEXTURE_WALL_BRICK_SIDEWALL);

                recreator.SetBlock(0, DNewBlock(2, 0, 2, 2, 1, 1, true, false, true, true));
                recreator.SetBlock(1, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(2, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(3, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(4, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(5, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(6, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(7, DNewBlock(2, 2, 0, 0, 1, 1, true, true, false, false));

                recreator.SetBlock(8, DNewBlock(2, 2, 0, 2, 1, 1, true, true, false, true));
                recreator.SetBlock(9, DNewBlock(2, 2, 0, 2, 1, 1, true, true, false, true));
                recreator.SetBlock(10, DNewBlock(2, 0, 0, 2, 1, 1, true, false, false, true));
                recreator.SetBlock(11, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(12, DNewBlock(2, 0, 0, 0, 1, 1, true, false, false, false));
                recreator.SetBlock(13, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(14, DNewBlock(2, 2, 2, 0, 1, 1, true, true, true, false));
                recreator.SetBlock(15, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));

                recreator.SetBlock(16, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(17, DNewBlock(0, 0, 0, 2, 1, 1, false, false, false, true));
                recreator.SetBlock(18, DNewBlock(0, 0, 2, 0, 1, 1, false, false, true, false));
                recreator.SetBlock(19, DNewBlock(2, 2, 2, 0, 1, 1, true, true, true, false));
                recreator.SetBlock(20, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(21, DNewBlock(2, 0, 2, 2, 1, 1, true, false, true, true));
                recreator.SetBlock(22, DNewBlock(2, 2, 0, 0, 1, 1, true, true, false, false));
                recreator.SetBlock(23, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));

                recreator.SetBlock(24, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(25, DNewBlock(0, 0, 0, 2, 1, 1, false, false, false, true));
                recreator.SetBlock(26, DNewBlock(2, 2, 0, 0, 1, 1, true, true, false, false));
                recreator.SetBlock(27, DNewBlock(2, 0, 0, 2, 1, 1, true, false, false, true));
                recreator.SetBlock(28, DNewBlock(0, 0, 0, 0, 1, 1, false, false, false, false));
                recreator.SetBlock(29, DNewBlock(2, 2, 2, 0, 1, 1, true, true, true, false));
                recreator.SetBlock(30, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(31, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));

                recreator.SetBlock(32, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(33, DNewBlock(0, 2, 2, 2, 1, 1, false, true, true, true));
                recreator.SetBlock(34, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(35, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));
                recreator.SetBlock(36, DNewBlock(0, 2, 2, 2, 1, 1, false, true, true, true));
                recreator.SetBlock(37, DNewBlock(2, 0, 0, 2, 1, 1, true, false, false, true));
                recreator.SetBlock(38, DNewBlock(0, 2, 2, 0, 1, 1, false, true, true, false));
                recreator.SetBlock(39, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));

                recreator.SetBlock(40, DNewBlock(0, 0, 0, 2, 1, 1, false, false, false, true));
                recreator.SetBlock(41, DNewBlock(2, 0, 0, 0, 1, 1, true, false, false, false));
                recreator.SetBlock(42, DNewBlock(0, 2, 0, 0, 1, 1, false, true, false, false));
                recreator.SetBlock(43, DNewBlock(0, 0, 2, 2, 1, 1, false, false, true, true));
                recreator.SetBlock(44, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(45, DNewBlock(0, 0, 0, 0, 1, 1, false, false, false, false));
                recreator.SetBlock(46, DNewBlock(2, 2, 0, 0, 1, 1, true, true, false, false));
                recreator.SetBlock(47, DNewBlock(0, 2, 0, 2, 1, 1, false, true, false, true));

                recreator.SetBlock(48, DNewBlock(0, 0, 0, 2, 1, 1, false, false, false, true));
                recreator.SetBlock(49, DNewBlock(0, 0, 0, 0, 1, 1, false, false, false, false));
                recreator.SetBlock(50, DNewBlock(0, 0, 0, 0, 1, 1, false, false, false, false));
                recreator.SetBlock(51, DNewBlock(2, 2, 2, 0, 1, 1, true, true, true, false));
                recreator.SetBlock(52, DNewBlock(2, 2, 0, 2, 1, 1, true, true, false, true));
                recreator.SetBlock(53, DNewBlock(0, 0, 2, 2, 1, 1, false, false, true, true));
                recreator.SetBlock(54, DNewBlock(0, 0, 2, 0, 1, 1, false, false, true, false));
                recreator.SetBlock(55, DNewBlock(0, 2, 0, 0, 1, 1, false, true, false, false));

                recreator.SetBlock(56, DNewBlock(0, 0, 2, 2, 1, 1, false, false, true, true));
                recreator.SetBlock(57, DNewBlock(0, 0, 2, 0, 1, 1, false, false, true, false));
                recreator.SetBlock(58, DNewBlock(0, 2, 2, 0, 1, 1, false, true, true, false));
                recreator.SetBlock(59, DNewBlock(2, 0, 2, 2, 1, 1, true, false, true, true));
                recreator.SetBlock(60, DNewBlock(0, 0, 2, 0, 1, 1, false, false, true, false));
                recreator.SetBlock(61, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(62, DNewBlock(2, 0, 2, 0, 1, 1, true, false, true, false));
                recreator.SetBlock(63, DNewBlock(0, 2, 2, 0, 1, 1, false, true, true, false));

                recreator.End();
            }
            private EntityMapData.BlockData DNewBlock(
                byte n, byte e, byte s, byte w, byte u, byte d,
                bool bn, bool be, bool bs, bool bw)
            {
                EntityMapData.BlockData data = new EntityMapData.BlockData();
                data.idTip = new byte[(int)Direction.SOLID_MAX];

                this.DSetDesign(data, n, e, s, w, u, d);
                this.DSetCollision(data, bn, be, bs, bw);

                return data;
            }
            private void DSetDesign(EntityMapData.BlockData data, byte n, byte e, byte s, byte w, byte u, byte d)
            {
                data.idTip[(int)Direction.NORTH] = n;
                data.idTip[(int)Direction.EAST] = e;
                data.idTip[(int)Direction.SOUTH] = s;
                data.idTip[(int)Direction.WEST] = w;
                data.idTip[(int)Direction.UP] = u;
                data.idTip[(int)Direction.DOWN] = d;
            }
            private void DSetCollision(EntityMapData.BlockData data, bool n, bool e, bool s, bool w)
            {
                uint flag = 0;
                if (n) flag |= (1 << (int)Direction.NORTH);
                if (e) flag |= (1 << (int)Direction.EAST);
                if (s) flag |= (1 << (int)Direction.SOUTH);
                if (w) flag |= (1 << (int)Direction.WEST);
                data.collision = flag;
            }


        } //class EntityNewCreator

    } //namespace entity
} //namespace nangka
