﻿using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public enum AxieFormType
    {
        Normal,
        //Isometric,

        Count
    }
    public interface IAxieMixerMaterials
    {
        SpineAtlasAsset GetFullSplatAtlasAsset(AxieFormType formType);
        SpineAtlasAsset GetSingleSplatAtlasAsset(AxieFormType formType);
        IAxieGenesStuff GetGenesStuff(AxieFormType formType);
        IAxieMixerStuff GetMixerStuff(AxieFormType formType);
        Material GetSampleGraphicMaterial(AxieFormType formType);
        Material GetSampleLinearGraphicMaterial(AxieFormType formType, byte colorVariant, byte colorShift);
    }
}
