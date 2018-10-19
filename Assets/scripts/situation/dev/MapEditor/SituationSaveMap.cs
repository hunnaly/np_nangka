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
                public class SituationSaveMap : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationSaveMap.CreateRules()");
                        this.CreateRule<RuleSaveMapToMEConsole>();
                        this.CreateRule<RuleSaveMapToMapEditor>();
                        return true;
                    }

                } // class SituaionSaveMap

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
