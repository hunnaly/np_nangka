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
                public class SituationNewMap : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationNewMap.CreateRules()");
                        this.CreateRule<RuleNewMapToMEConsole>();
                        this.CreateRule<RuleNewMapToMapEditor>();
                        return true;
                    }

                } // class SituaionMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
