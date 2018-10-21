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
                public class SituationInputMapFile : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationInputMapFile.CreateRules()");
                        this.CreateRule<RuleInputMapFileToMEConsole>();
                        this.CreateRule<RuleInputMapFileToMapEditor>();
                        return true;
                    }

                } // class SituaionInputMapFile

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
