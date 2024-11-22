using System;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class Axie2dBuilderResult
    {
        public string error;
        public SkeletonDataAsset skeletonDataAsset;
        public Dictionary<string, string> adultCombo;
        public Material sharedGraphicMaterial;
    }

    public class Axie2dBuilder
    {
        public IAxieMixerMaterials axieMixerMaterials { get; private set; }
        public bool isGraphicLinear { get; private set; }

        public void Init(IAxieMixerMaterials axieMixerMaterials)
        {
            this.axieMixerMaterials = axieMixerMaterials;
            this.isGraphicLinear = false;// QualitySettings.activeColorSpace == ColorSpace.Linear;
        }

        public int GetSampleColorVariant(CharacterClass characterClass, int colorValue)
        {
            var lst = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal).axieSkinColors.Where(x => x.@class == characterClass && x.skin == 0).ToList();
            var axieSkinColor = lst[colorValue % lst.Count];
            return axieMixerMaterials.GetGenesStuff(AxieFormType.Normal).axieSkinColors.IndexOf(axieSkinColor);
        }

        private string OverridePartSample(AxiePartType partType, Dictionary<string, string> combo, IAxieGenesStuff genesStuff)
        {
            var partTypeStr = partType.ToString().ToLower();
            if (!combo.TryGetValue(partTypeStr, out var sample)) return sample;
            var stageKey = $"{partTypeStr}.stage";
            var skinKey = $"{partTypeStr}.skin";
            int partStage = -1;
            int partSkin = -1;
            if (combo.TryGetValue(stageKey, out var stageVal) && int.TryParse(stageVal, out int stage))
            {
                partStage = stage;
            }
            if (combo.TryGetValue(skinKey, out var skinVal) && int.TryParse(skinVal, out int skin))
            {
                partSkin = skin;
            }
            bool fallbackDefault = false;
            if (combo.ContainsKey("fallback-default"))
            {
                fallbackDefault = true;
            }
            
            string finalPartSample = genesStuff.OverridePartSample(partTypeStr, sample, partStage, partSkin, fallbackDefault);
            return finalPartSample;
        }

        public Axie2dBuilderResult BuildSpineAdultCombo(Dictionary<string, string> adultCombo, byte colorVariant, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);
            string bodySkinKey = "body.skin";
            if (adultCombo.TryGetValue(bodySkinKey, out var bodySkinVal) && int.TryParse(bodySkinVal, out var bodySkin))
            {
                if (bodySkin == 1)
                {
                    adultCombo["body"] = "body-frosty";
                }
                else if (bodySkin == 2)
                {
                    adultCombo["body"] = "body-summer";
                }
                else if (bodySkin == 3)
                {
                    adultCombo["body"] = "body-nightmare";
                }
                if (bodySkin != 0 && adultCombo.TryGetValue("body-class", out var bodyClass))
                {
                    colorVariant = (byte)axieGenesStuff.GetAxieColorVariant(0, bodySkin, bodyClass);
                }
            }
            adultCombo["back"] = OverridePartSample(AxiePartType.back, adultCombo, axieGenesStuff);
            adultCombo["ears"] = OverridePartSample(AxiePartType.ears, adultCombo, axieGenesStuff);
            adultCombo["ear"] = adultCombo["ears"];
            adultCombo["eyes"] = OverridePartSample(AxiePartType.eyes, adultCombo, axieGenesStuff);
            adultCombo["horn"] = OverridePartSample(AxiePartType.horn, adultCombo, axieGenesStuff);
            adultCombo["mouth"] = OverridePartSample(AxiePartType.mouth, adultCombo, axieGenesStuff);
            adultCombo["tail"] = OverridePartSample(AxiePartType.tail, adultCombo, axieGenesStuff);

            var accessories = adultCombo.Where(x => x.Key.StartsWith("accessory-")).ToList();
            foreach(var p in accessories)
            {
                string accessorySlot = p.Key.Replace("accessory-", "body-");
                string accessoryName = p.Value.Replace("accessory-", "body-");
                adultCombo[accessorySlot] = accessoryName;
            }
           
            Axie2dBuilderResult builderResult = new Axie2dBuilderResult();
            builderResult.adultCombo = adultCombo;
            var axieMixerStuff = axieMixerMaterials.GetMixerStuff(AxieFormType.Normal);
            
            List<(BoneComboType, byte, byte)> colorVariants = new List<(BoneComboType, byte, byte)>();
            int partColorShift = axieGenesStuff.GetAxieColorPartShift(colorVariant);
            for (int i = 0;i < (int)BoneComboType.count;i++)
            {
                BoneComboType boneType = (BoneComboType)i;
                byte shiftValue = 0;
                if ((partColorShift & (1 << ((int)BoneComboType.count - i - 1))) != 0)
                {
                    shiftValue = 2;
                }
                colorVariants.Add((boneType, colorVariant, shiftValue));
            }
            var jMixed = axieMixerStuff.GenerateAssetLite(adultCombo, colorVariants, "");
            var skeletonDataAsset = CreateMixedSkeletonDataAsset(jMixed, scale, isGraphic);
            
            if (skeletonDataAsset == null)
            {
                builderResult.error = "GenerateAsset Failed";
            }
            else
            {
                builderResult.skeletonDataAsset = skeletonDataAsset;
                builderResult.sharedGraphicMaterial = isGraphicLinear ? axieMixerMaterials.GetSampleLinearGraphicMaterial(AxieFormType.Normal, colorVariant, 0) : //phuongnk - tmp solution shift value is not correct
                                                                        axieMixerMaterials.GetSampleGraphicMaterial(AxieFormType.Normal);
            }
            return builderResult;
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, string genesStr, Dictionary<string, string> meta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);

            if (genesStr.StartsWith("0x"))
            {
                genesStr = genesStr.Substring(2);
            }
            string finalGenes512 = genesStr;
            if (finalGenes512.Length < 128)
            {
                finalGenes512 = finalGenes512.PadLeft(128, '0');
            }
            System.Numerics.BigInteger.TryParse(finalGenes512, System.Globalization.NumberStyles.HexNumber, null, out var genes);
            var bodyStructure = axieGenesStuff.GetAxieBodyStructure512(genes);

            return BuildSpineFromGene(axieId, bodyStructure, meta, scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, string genesStr, float scale, bool isGraphic = false)
        {
            return BuildSpineFromGene(axieId, genesStr, new Dictionary<string, string>(), scale, isGraphic);
        }

        public Axie2dBuilderResult BuildSpineFromGene(string axieId, AxieBodyStructure bodyStructure, Dictionary<string, string> meta, float scale, bool isGraphic = false)
        {
            var axieGenesStuff = axieMixerMaterials.GetGenesStuff(AxieFormType.Normal);
            var adultCombo = axieGenesStuff.GetAdultCombo(bodyStructure);
            if (axieId.Length < 6)
            {
                axieId = axieId.PadLeft(axieId.Length + (7 - axieId.Length) / 2);
            }
            adultCombo.Add("body-id", axieId);
            foreach(var p in meta)
            {
                if (!adultCombo.ContainsKey(p.Key))
                {
                    adultCombo.Add(p.Key, p.Value);
                }
            }

         
            byte colorVariant = (byte)axieGenesStuff.GetAxieColorVariant(bodyStructure.primaryColors[0], bodyStructure.bodySkin, bodyStructure.@class.ToString());

            return BuildSpineAdultCombo(adultCombo, colorVariant, scale, isGraphic);
        }

        SkeletonDataAsset CreateMixedSkeletonDataAsset(MixedSkeletonData mixed, float scale, bool isGraphic)
        {
            if (mixed == null) return null;
            try
            {
                var atlasAsset = isGraphic
                    ? axieMixerMaterials.GetSingleSplatAtlasAsset(AxieFormType.Normal)
                    : axieMixerMaterials.GetFullSplatAtlasAsset(AxieFormType.Normal);
                SkeletonDataAsset skeletonDataAsset = ScriptableObject.CreateInstance<SkeletonDataAsset>();
                skeletonDataAsset.Clear();
                skeletonDataAsset.atlasAssets = new[] { atlasAsset };
                skeletonDataAsset.scale = scale;

                var atlasArray = skeletonDataAsset.atlasAssets.Select(x => x.GetAtlas()).ToArray();
                var skeletonMixed = new SkeletonMixed(atlasArray)
                {
                    Scale = scale
                };
                var loadedSkeletonData = skeletonMixed.ReadSkeletonData(mixed, true);
                skeletonDataAsset.InitializeWithData(loadedSkeletonData);

                return skeletonDataAsset;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
            return null;
        }
    }
}
