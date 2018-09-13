using np;

namespace nangka {
    namespace situation
    {

        public abstract class RuleBase
        {
            private NpSituation _nextSituation = null;

            protected NpSituation nextSituation
            {
                get { return this._nextSituation; }
                set { this._nextSituation = value; }
            }

            public NpSituation CompletedReadyNextSituation()
            {
                return this._nextSituation;
            }
        }

    } //namespace situation
} //namespace nangka
