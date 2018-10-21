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
                public class SituationInputMapTitle : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationInputMapTitle.CreateRules()");
                        this.CreateRule<RuleInputMapTitleToMEConsole>();
                        this.CreateRule<RuleInputMapTitleToMapEditor>();
                        return true;
                    }

                } // class SituaionInputMapTitle

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
