using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SecondaryMineableYield {
    public class ModExtension_SecondaryMineableYield : DefModExtension {
        public float mineableDropChance = 1f;
        public List<SecondaryYieldEntry> entries = new List<SecondaryYieldEntry>();

        float weightSum = -1;
        public float GetWeightSum { get {
                if (weightSum < 0) {
                    weightSum = entries.Sum(t => t.randomWeight);
                }
                return weightSum;
            } 
        }
    }
    public class SecondaryYieldEntry {
        public ThingDef mineableThing;
        public int mineableYield = 1;
        public float mineableScatterCommonality;
        public IntRange mineableScatterLumpSizeRange = new IntRange(20, 40);
        public bool mineableYieldWasteable = true;

        public float randomWeight = 1f;

        public int EffectiveMineableYield {
            get {
                return Mathf.RoundToInt((float)this.mineableYield * Find.Storyteller.difficulty.mineYieldFactor);
            }
        }
    }
}
