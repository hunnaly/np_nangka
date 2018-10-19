using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;

namespace nangka {

    //---------------------------------------------------------------
    // マップ識別子
    //---------------------------------------------------------------
    public enum MAP_ID : int
    {
        MAP_EMPTY,      // 新規作成用
        MAP_01,
        MAP_02,
        MAP_03,
        MAP_04,
        MAP_05,
        MAP_06,
        MAP_07,
        MAP_08,
        MAP_09,
        MAP_ID_MAX,

        MAP_DUMMY       // ダミーデータ（頃合いをみていらなくする）

    } //enum MAP_ID

    //---------------------------------------------------------------
    // Game ロジック用の共有参照データ置き場
    // np.* はこれを利用しない。
    //---------------------------------------------------------------
    public sealed class Global
    {
        //---------------------------------------------------------------
        // Gloabl インスタンス
        //---------------------------------------------------------------
        private Global() { }
        private static Global instance = new Global();
        public static Global Instance { get { return instance; } }

        //---------------------------------------------------------------
        // 初期化
        //---------------------------------------------------------------
        public void Initialize(NpAppBase app)
        {
            this.entityController = app.GetEntityController();
        }

        //---------------------------------------------------------------
        // PlayerCamera
        //---------------------------------------------------------------
        private Camera _cameraPlayer;
        public Camera cameraPlayer { get { return this._cameraPlayer; } }

        private GameObject _objCameraPlayerBase;
        public GameObject objCameraPlayerBase { get { return this._objCameraPlayerBase; } }

        public void SetPlayerCamera(Camera camera, GameObject cameraBase=null)
        {
            this._cameraPlayer = camera;
            this._objCameraPlayerBase = (cameraBase == null) ? (GameObject)camera.gameObject : cameraBase;
        }

        //---------------------------------------------------------------
        // 共有参照データ
        //---------------------------------------------------------------
        private INpEntityController entityController = null;
        public INpEntityController EntityCtrl { get { return this.entityController; } }

        //---------------------------------------------------------------
        // 共有データ
        // むやみに利用しないこと！
        //---------------------------------------------------------------



    } //class Global

} //namespace nangka
