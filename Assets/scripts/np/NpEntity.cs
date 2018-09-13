using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public abstract class NpEntity
    {
        private bool bValidate = false;
        private bool bReady = false;
        private bool bPaused = false;
        private bool bTerminate = false;

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
                this.bTerminate = false;
                this.bReady = true;
                this.bValidate = true;
            }
        }

        public void Update()
        {
            if (this.IsInvalidate()) return;

            if (this.IsReady() == true)
            {
                if (this.StartProc() == true)
                {
                    this.bReady = false;
                }
            }
            else
            {
                if (this.IsTerminating())
                {
                    if (this.TerminateProc() == true)
                    {
                        this.bValidate = false;
                        this.bTerminate = false;
                        this.CleanUp();
                    }
                }
                else
                {
                    if (this.bPaused == false && this.UpdateProc() == true)
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

        public void Pause(bool pause) { this.bPaused = pause; }

        public void Terminate() { this.bTerminate = true; }
        public void CleanUpForce() { this.CleanUp(); }

        public bool IsInvalidate() { return (this.bValidate == false); }
        public bool IsReady() { return this.bReady; }
        public bool IsTerminating() { return this.bTerminate; }

        public U GetInterface<T, U>()
            where T : class, U
            where U : class
        {
            return this as T;
        }
    }

}
