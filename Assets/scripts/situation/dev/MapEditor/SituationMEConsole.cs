using UnityEngine;
using np;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {
            namespace mapeditor
            {
                public class SituationMEConsole : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationMEConsole.CreateRules()");
                        this.CreateRule<RuleMEConsoleToMapEditor>();
                        return true;
                    }

                } // class SituaionMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
