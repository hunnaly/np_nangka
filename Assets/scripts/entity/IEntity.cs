namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntity
        //------------------------------------------------------------------
        public interface IEntity
        {
            bool IsReadyLogic();
            void Terminate();
            void Pause(bool bPause);

        } //interface IEntity

    } //namespace entity
} //namespace nangka