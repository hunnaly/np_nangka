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

        } //interface IEntity

    } //namespace entity
} //namespace nangka