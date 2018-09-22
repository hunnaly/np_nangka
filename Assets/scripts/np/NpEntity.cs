using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public abstract class NpEntity
    {
        private bool __bValidate = false;
        private bool __bReady = false;
        private bool __bPaused = false;
        private bool __bTerminate = false;

        protected virtual bool StartProc() { return true; }
        protected virtual bool UpdateProc() { return false; }
        protected virtual bool TerminateProc() { return true; }
        protected virtual void LastUpdateProc() { }
        protected virtual void OnGUIProc() { }
        protected virtual void CleanUp() { }

        ~NpEntity()
        {
            this.CleanUp();
        }

        public void Start()
        {
            if (this.IsInvalidate())
            {
                this.__bTerminate = false;
                this.__bReady = true;
                this.__bValidate = true;
            }
        }

        public void Update()
        {
            if (this.IsInvalidate()) return;

            if (this.IsReady() == true)
            {
                if (this.StartProc() == true)
                {
                    this.__bReady = false;
                }
            }
            else
            {
                if (this.IsTerminating())
                {
                    if (this.TerminateProc() == true)
                    {
                        this.__bValidate = false;
                        this.__bTerminate = false;
                        this.CleanUp();
                    }
                }
                else
                {
                    if (this.__bPaused == false && this.UpdateProc() == true)
                    {
                        this.Terminate();
                    }
                }
            }
        }

        public void LastUpdate()
        {
            if (this.IsInvalidate() || this.IsTerminating() || this.IsReady()) return;
            this.LastUpdateProc();
        }

        public void OnGUI()
        {
            if (this.IsInvalidate() || this.IsTerminating() || this.IsReady()) return;
            this.OnGUIProc();
        }

        public void Pause(bool pause) { this.__bPaused = pause; }

        public void Terminate() { this.__bTerminate = true; }
        public void CleanUpForce() { this.CleanUp(); }

        public bool IsInvalidate() { return (this.__bValidate == false); }
        public bool IsReady() { return this.__bReady; }
        public bool IsTerminating() { return this.__bTerminate; }

        public U GetInterface<T, U>()
            where T : class, U
            where U : class
        {
            return this as T;
        }
    }

}
