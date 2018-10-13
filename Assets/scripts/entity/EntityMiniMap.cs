using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityMapData
        //------------------------------------------------------------------
        public interface IEntityMiniMap : IEntity
        {
            void Flash(IEntityMapData iMapData);
            void Flash(IEntityMapData iMapData, int x, int y);
            void Rotate(float angleZ);
            void Move(int x, int y, Vector3 delta);

        } // interface IEntityMiniMap


        //------------------------------------------------------------------
        // EntityMapData
        //------------------------------------------------------------------
        public class EntityMiniMap : NpEntity, IEntityMiniMap
        {
            //------------------------------------------------------------------
            // 準備処理関連変数
            //------------------------------------------------------------------

            private bool _bReadyLogic;
            public bool IsReadyLogic() { return this._bReadyLogic; }

            private GameObject refObjMinimap;
            private GameObject prefabArrow;
            private GameObject objArrow;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityMiniMap.StartProc()");

                this.ReadyLogic();
                return true;
            }

            protected override bool TerminateProc()
            {
                Debug.Log("EntityMiniMap.TerminateProc()");

                this.ReleaseArrow();
                this._bReadyLogic = false;
                return true;
            }

            protected override void CleanUp()
            {
                this.ReleaseArrow();
            }

            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private void ReadyLogic()
            {
                this.refObjMinimap = this.GetMiniMapRoot();
                this.CreateArrow(this.refObjMinimap);

                this._bReadyLogic = true;
            }

            private GameObject GetMiniMapRoot()
            {
                var scene = SceneManager.GetSceneByName(Define.SCENE_NAME_FRAME);
                var canvas = scene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null).GetComponent<Canvas>();
                var component = canvas.GetComponent<ObjectTable>();
                return component.objectTable[2];
            }

            private void CreateArrow(GameObject parent)
            {
                this.prefabArrow = Resources.Load<GameObject>(Define.RES_PATH_PREFAB_ARROW);

                this.objArrow = (GameObject)Object.Instantiate(this.prefabArrow);
                this.objArrow.transform.SetParent(parent.transform, false);
            }

            private void ReleaseArrow()
            {
                if (this.objArrow != null)
                {
                    Object.Destroy(this.objArrow);
                    this.objArrow = null;
                }

                if (this.prefabArrow != null)
                {
                    Resources.UnloadAsset(this.prefabArrow);
                    this.prefabArrow = null;
                }
            }

            //------------------------------------------------------------------
            // インタフェース実装
            //------------------------------------------------------------------

            public void Flash(IEntityMapData iMapData)
            {
                MapData mapData = iMapData.GetMapData();

                for (int y = 0; y < mapData.width; y++) {
                    for (int x = 0; x < mapData.width; x++)
                    {
                        this.Flash(iMapData, x, y);
                    }
                }
            }

            public void Flash(IEntityMapData iMapData, int x, int y)
            {
                var component = this.refObjMinimap.GetComponent<ObjectTable>();
                GameObject obj = component.objectTable[y];

                component = obj.GetComponent<ObjectTable>();
                obj = component.objectTable[x];

                if (iMapData.IsThorough(x, y)) this.Show(obj, iMapData, x, y);
                else this.Hide(obj);
            }

            public void Rotate(float angleZ)
            {
                this.objArrow.transform.rotation =
                    Quaternion.AngleAxis(angleZ, new Vector3(0.0f, 0.0f, -1.0f));
            }

            public void Move(int x, int y, Vector3 delta)
            {
                Vector3 pos = this.objArrow.transform.localPosition;

                pos.x = -71 + x * 20 + delta.x;
                pos.y = 75 - y * 20 + delta.y;

                this.objArrow.transform.localPosition = pos;
            }


            private void Show(GameObject obj, IEntityMapData iMapData, int x, int y)
            {
                var component = obj.GetComponent<ObjectTable>();

                Texture[] table = iMapData.GetBlockTexture(x, y);
                for (int d = 0; d < (int)Direction.PLANE_MAX; d++)
                {
                    GameObject objWall = this.GetObjectWall(component, (Direction)d);
                    objWall.SetActive(table[d] != null);
                }
                this.GetObjectMask(component).SetActive(false);
            }

            private void Hide(GameObject obj)
            {
                var component = obj.GetComponent<ObjectTable>();

                for (int d = 0; d < (int)Direction.PLANE_MAX; d++) {
                    this.GetObjectWall(component, (Direction)d).SetActive(false);
                }
                this.GetObjectMask(component).SetActive(true);
            }

            private GameObject GetObjectWall(ObjectTable table, Direction dir)
            {
                GameObject obj = null;
                switch (dir)
                {
                    case Direction.NORTH: obj = table.objectTable[0]; break;
                    case Direction.SOUTH: obj = table.objectTable[1]; break;
                    case Direction.WEST: obj = table.objectTable[2]; break;
                    case Direction.EAST: obj = table.objectTable[3]; break;
                    default: break;
                }
                return obj;
            }

            private GameObject GetObjectMask(ObjectTable table)
            {
                return table.objectTable[4];
            }

        } //class EntityMiniMap

    } //namespace entity
} //namespace nangka
