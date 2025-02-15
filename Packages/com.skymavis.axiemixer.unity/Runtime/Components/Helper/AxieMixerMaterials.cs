﻿using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class SplatAtlasStuff
    {
        public string tag;
        public string atlasAssetText;
        public Dictionary<string, Texture2D> textures;
    }

    public class AxieMixerMaterials : IAxieMixerMaterials {
        public class SingleStuff {
            public List<SplatAtlasStuff> atlasStuffs;
            public SpineAtlasAsset fullSplatAtlasAsset;
            public SpineAtlasAsset singleSplatAtlasAsset;
            public IAxieGenesStuff axieGenesStuff;
            public IAxieMixerStuff axieMixerStuff;
            public Material sampleGraphicMaterial;
            public Material sampleGraphicLinearMaterial;
            public Dictionary<string, Material> materials = new Dictionary<string, Material>();
            public Dictionary<string, Material> variantMaterials = new Dictionary<string, Material>();
        }
        SingleStuff[ ] stuffs = new SingleStuff[(int)AxieFormType.Count];

        public SpineAtlasAsset GetFullSplatAtlasAsset(AxieFormType formType) {
            if (stuffs[(int)formType] == null) return null;
            if(stuffs[(int)formType].fullSplatAtlasAsset == null)
            {
                return stuffs[(int)formType].singleSplatAtlasAsset;
            }
            return stuffs[(int)formType].fullSplatAtlasAsset;
        }

        public SpineAtlasAsset GetSingleSplatAtlasAsset(AxieFormType formType)
        {
            if (stuffs[(int)formType] == null) return null;
            return stuffs[(int)formType].singleSplatAtlasAsset;
        }

        public Material GetSampleGraphicMaterial(AxieFormType formType)
        {
            if (stuffs[(int)formType] == null) return null;
            return stuffs[(int)formType].sampleGraphicMaterial;
        }

        public Material GetSampleLinearGraphicMaterial(AxieFormType formType, byte colorVariant, byte colorShift)
        {
            if (stuffs[(int)formType] == null) return null;
            var stuff = stuffs[(int)formType];
            string key = $"linear_{colorVariant}-{colorShift}";
            Material ret;
            if(stuff.variantMaterials.TryGetValue(key, out ret))
            {
                return ret;
            }
            ret = new Material(stuffs[(int)formType].sampleGraphicLinearMaterial);
            ret.SetFloat("_ColorVariant", colorVariant);
            ret.SetFloat("_ColorShift", colorShift);
            stuff.variantMaterials.Add(key, ret);
            return ret;
        }

        public IAxieGenesStuff GetGenesStuff(AxieFormType formType) {
            if (stuffs[(int)formType] == null) return null;
            return stuffs[(int)formType].axieGenesStuff;
        }
        public IAxieMixerStuff GetMixerStuff(AxieFormType formType) {
            if (stuffs[(int)formType] == null) return null;
            return stuffs[(int)formType].axieMixerStuff;
        }

        public Dictionary<string, Material> GetMaterials(AxieFormType formType) {
            if (stuffs[(int)formType] == null) return null;
            return stuffs[(int)formType].materials;
        }

        public void InstallStuff(
            AxieFormType formType,
            SplatAtlasStuff atlasStuffSingle,
            SplatAtlasStuff atlasStuffHD,
            IAxieGenesStuff axieGenesStuff,
            IAxieMixerStuff axieMixerStuff,
            Dictionary<string, Material> baseMaterials
            )
        {
            UnityEngine.Assertions.Assert.IsNotNull(atlasStuffSingle);
            UnityEngine.Assertions.Assert.IsTrue(baseMaterials.ContainsKey("default"));

            List<SplatAtlasStuff> atlasStuffs = new List<SplatAtlasStuff>();
            atlasStuffs.Add(atlasStuffSingle);
            SpineAtlasAsset singleSplatAtlasAsset = LoadAtlas(atlasStuffSingle, baseMaterials["default"]);
            SpineAtlasAsset fullSplatAtlasAsset;
            if (atlasStuffHD != null)
            {
                fullSplatAtlasAsset = LoadAtlas(atlasStuffHD, baseMaterials["default"]);
                atlasStuffs.Add(atlasStuffHD);
            }
            else
            {
                fullSplatAtlasAsset = singleSplatAtlasAsset;
            }

            Material sampleGraphicMaterial = null;
            if (baseMaterials.TryGetValue("graphic", out var baseGraphicMaterial))
            {
                List<Material> materials = LoadMaterials(atlasStuffSingle, baseGraphicMaterial);
                UnityEngine.Assertions.Assert.IsTrue(materials.Count == 1);
                sampleGraphicMaterial = materials[0];
            }

            Material sampleGraphicLinearMaterial = null;
            if (baseMaterials.TryGetValue("graphic-linear", out var baseGraphicLinearMaterial))
            {
                List<Material> materials = LoadMaterials(atlasStuffSingle, baseGraphicLinearMaterial);
                UnityEngine.Assertions.Assert.IsTrue(materials.Count == 1);
                sampleGraphicLinearMaterial = materials[0];
            }

            stuffs[(int)formType] = new SingleStuff
            {
                atlasStuffs = atlasStuffs,
                fullSplatAtlasAsset = fullSplatAtlasAsset,
                singleSplatAtlasAsset = singleSplatAtlasAsset,
                axieGenesStuff = axieGenesStuff,
                axieMixerStuff = axieMixerStuff,
                sampleGraphicMaterial = sampleGraphicMaterial,
                sampleGraphicLinearMaterial = sampleGraphicLinearMaterial,
                materials = baseMaterials
            };
        }

        static List<Material> LoadMaterials(SplatAtlasStuff atlasStuff, Material baseMaterial)
        {
            List<Material> materials = new List<Material>();
            string atlasStr = atlasStuff.atlasAssetText;
            atlasStr = atlasStr.Replace("\r", "");

            string[] texList = atlasStr.Split('\n').Where(x => x.Contains(".png")).Select(x => x.Replace(".png", "")).ToArray();
            foreach (var texName in texList)
            {
                var material = new Material(baseMaterial);
                foreach (var p in atlasStuff.textures)
                {
                    string texKey = p.Key.Replace($"[{texName}]", "");
                    material.SetTexture(texKey, p.Value);
                }
                materials.Add(material);

            }
            return materials;
        }

        static SpineAtlasAsset LoadAtlas(SplatAtlasStuff  atlasStuff, Material baseMaterial)
        {
            List<Material> materials = LoadMaterials(atlasStuff, baseMaterial);
            var fullAtlasAsset = SpineAtlasAsset.CreateRuntimeInstance(new TextAsset(atlasStuff.atlasAssetText), materials.ToArray(), true);
            return fullAtlasAsset;
        }
    }
}
