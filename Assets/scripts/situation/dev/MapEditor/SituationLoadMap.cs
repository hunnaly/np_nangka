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
                public class SituationLoadMap : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationLoadMap.CreateRules()");
                        this.CreateRule<RuleLoadMapToMEConsole>();
                        this.CreateRule<RuleLoadMapToMapEditor>();
                        return true;
                    }

                } // class SituaionLoadMap

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
