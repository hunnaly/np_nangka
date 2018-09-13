using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public class NpAppBase : MonoBehaviour
    {
        private bool bCreated = false;
        private NpEntityConductor entityConductor = null;
        static private NpAppBase instance;

        // インスペクター上で設定する
        public int m_screenWidth;
        public int m_screenHeight;
        public int m_targetFrameRate = -1;

        public INpEntityController GetEntityController() { return this.entityConductor; }

        // 派生クラスに提供
        protected virtual void OnStart() { }

        // オブジェクト生成時に一度だけ呼び出される
        void Awake()
        {
            if (!bCreated)
            {
                DontDestroyOnLoad(this.gameObject);

                // PC向けビルドのときにはサイズを強制的に変更
                // Memo: 前回起動時のサイズが記憶されて、それが利用されることがあるため
                if (IsPlatformForPC())
                {
                    Screen.SetResolution(m_screenWidth, m_screenHeight, false);
                }

                // フレームレートの設定
                if (m_targetFrameRate > 0)
                {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = m_targetFrameRate;
                }

                // Task 管理インスタンスの生成
                this.entityConductor = new NpEntityConductor();

                instance = this;

                bCreated = true;
            }

            Debug.Log("NpAppBase.Awake");
        }

        // Use this for initialization
        void Start()
        {
            this.entityConductor.Start();
            this.OnStart();
        }

        // Update is called once per frame
        void Update()
        {
            this.entityConductor.Update();
        }

        void LastUpdate()
        {
            this.entityConductor.LastUpdate();
        }

        void OnGUI()
        {
            this.entityConductor.OnGUI();
        }

        // PC向けビルドかどうか
        private bool IsPlatformForPC()
        {
            return (
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer
                );
        }

        // MonoBehaviour を継承していないクラスへ StartCoroutine を提供する
        static public Coroutine StartStaticCoroutine(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

    } //class NpAppBase

} //namespace np
