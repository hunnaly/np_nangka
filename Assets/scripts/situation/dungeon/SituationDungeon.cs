using UnityEngine;
using np;

namespace nangka
{
    namespace situation
    {
        namespace dungeon
        {

            public class SituationDungeon : NpSituation
            {
                protected override bool CreateRules()
                {
                    Debug.Log("SituationDungeon.CreateRules()");
                    INpRule rule = this.CreateRule<RuleDungeonToBattle>();
                    return (rule != null);
                }

            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
