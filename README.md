## Overview

The `Axie Mixer` is built upon [Spine Animation](http://en.esotericsoftware.com/) but with some customization for our needs. The `AxieMixer` will help you to create the `SkeletonData` which you can use with Spine and use it as normal.

# Install

⚠️ The package requires `Spine Runtime Library` [(spine-unity 3.8 2021-11-10)](https://esotericsoftware.com/files/runtimes/unity/spine-unity-3.8-2021-11-10.unitypackage). You need to download it manualy, and put it on Plugins folder.

- [Install the package using the following Git URL:](https://docs.unity3d.com/Manual/upm-ui-giturl.html) `https://github.com/axieinfinity/mixer-unity.git?path=/Packages/com.skymavis.axiemixer.unity#v0.7.3`


# Usage

## Initialize Axie Mixer

To initialize the Axie Mixer, you need to call `Mixer.Init()` only once at the start of the game. The best place for this method is in the loading scene.

Calling `Mixer.Init()` multiple times will do nothing.



## Create Axie Spine

To create an axie spine, you will need to know the `Axie Id` and its `Genes`

Then create an object in scene with `SkeletonAnimation` component

```cs
var skeletonAnimation = GetComponent<SkeletonAnimation>();
Mixer.SpawnSkeletonAnimation(skeletonAnimation, axieId, genesStrting);
```

#### API

```c
Mixer.SpawnSkeletonAnimation(
    SkeletonAnimation skeletonAnimation, 
    string axieId, 
    string genesStr
)
```

- `skeletonAnimation`: Main spine component for rendering and controling animation

- `axieId`: Axie id

- `genesString`: Axie genes, must be gene 512



> Tips
> 
> You might need to scale down the `SkeletonAnimation` to the desired size.
> 
> To flip the Axie, please use the `scaleX` field of the skeleton. Example: `skeletonAnimation.skeleton.ScaleX = -1`



## Create Axie Spine For UICanvas

Creating Axie Spine for UICanvas is quite the same as above, the different is you will need to use `SkeletonGraphic` instead of `SkeletonAnimation`

```cs
Mixer.SpawnSkeletonAnimation(
    SkeletonGraphic skeletonGraphic, 
    string axieId, 
    string genesStr
)
```



## Playing Animation

You can get all available animation names using this snippet.

```cs
List<string> animationList = Mixer.Builder.axieMixerMaterials.GetMixerStuff(AxieFormType.Normal).GetAnimatioNames();
```



Playing animation.

```cs
skeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
// The above code will play Idle animation with looping 

// For Skeleton Graphic
skeletonGraphic.AnimationState.SetAnimation(0, "action/idle/normal", true);

// You can find more information about the function here http://en.esotericsoftware.com/spine-applying-animations
```

## Accessory
We change the accessory logic using bone follow.

### Pass accessory info from GraphQL response
```cs
    string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes, hasMysticSuit, equipment { attributes }}}";
    ...
    bool hasMysticSuit = (bool)jResult["data"]["axie"]["hasMysticSuit"];
    JArray jEquipments = jResult["data"]["axie"]["equipment"] as JArray;
    List<string> equipments = new List<string>();
    if (jEquipments != null)
    {
        for(int i=0;i< jEquipments.Count; i++)
        {
            string equipmentKey = (string)(jEquipments[i]["attributes"]["key"]);
            equipments.Add(equipmentKey);
        }
    }
    var meta = new Dictionary<string, string>();
    if (!hasMysticSuit)
    {
        meta.Add("accessory-suit-off", "1");
    }
    foreach (var p in equipments)
    {
        meta.Add(p, "accessory");
    }
    ...
    var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale);

```


### Spine Skeleton
```cs
    var accessory = new GameObject("accessory", typeof(AccessoryController)).GetComponent<AccessoryController>();
    accessory.transform.SetParent(runtimeSkeletonAnimation.transform, false);
    accessory.Init(builderResult.skeletonDataAsset.scale);
    builderResult.adultCombo.Where(p => p.Value == "accessory").Select(x => x.Key).ToList().ForEach(p => accessory.Equip(p));
```

### Spine For UICanvas
```cs
    var accessory = new GameObject("accessory", typeof(AccessoryGraphicController)).GetComponent<AccessoryGraphicController>();
    accessory.transform.SetParent(skeletonGraphic.transform, false);
    accessory.Init(builderResult.skeletonDataAsset.scale);
    builderResult.adultCombo.Where(p => p.Value == "accessory").Select(x => x.Key).ToList().ForEach(p => accessory.Equip(p));
```


## Building Project
You need add `AxieMixerShaderVariants` into Preloaded Shaders (Project Settings/Graphics/Shader Loading)

## Scene demo

In the asset, there are 2 demos that you can explore. You can find these scenes:

- `DemoMixer`: or so called PlayGround. 

- `FlappyAxie`: This is a minigame based on FlappyBird. In this demo, you can find the `AxieFigure` class which helps to setup `SkeletonAnimation` automatically. (The `axie id` and `genes` are static though)


