using UnityEngine;
using np;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {

            public class SituationDevEntrance : NpSituation
            {
                protected override bool CreateRules()
                {
                    Debug.Log("SituationDevEntrance.CreateRules()");
                    this.CreateRule<RuleDevEntranceToMapEditor>();
                    this.CreateRule<RuleDevEntranceToDungeon>();
                    return true;
                }

            }

        } //namespace dev
    } //namespace situation
} //namespace nangka
