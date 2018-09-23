﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;

namespace nangka {

    // Game ロジック用の共有参照データ置き場
    // np.* はこれを利用しない。
    public sealed class Global
    {
        // Gloabl インスタンス
        private static Global instance = new Global();

        // PlayerCamera
        private Camera _cameraPlayer;
        public Camera cameraPlayer { get { return this._cameraPlayer; } }

        private GameObject _objCameraPlayerBase;
        public GameObject objCameraPlayerBase { get { return this._objCameraPlayerBase; } }

        public void SetPlayerCamera(Camera camera, GameObject cameraBase=null)
        {
            this._cameraPlayer = camera;
            this._objCameraPlayerBase = (cameraBase == null) ? (GameObject)camera.gameObject : cameraBase;
        }


        // 共有参照データ
        private INpEntityController entityController = null;
        public INpEntityController EntityCtrl
        {
            get { return this.entityController; }
        }


        // 外部からのインスタンス生成禁止
        private Global() { }

        // Global インスタンス提供
        public static Global Instance
        {
            get { return instance; }
        }

        public void Initialize(NpAppBase app)
        {
            this.entityController = app.GetEntityController();
        }

    } //class Global

} //namespace nangka
