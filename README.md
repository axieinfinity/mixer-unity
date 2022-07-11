## Overview

The `Axie Mixer` is built upon [Spine Animation](http://en.esotericsoftware.com/) but with some customization for our needs. The `AxieMixer` will help you to create the `SkeletonData` which you can use with Spine and use it as normal.

# Install

⚠️ The package requires `Spine Runtime Library` [(spine-unity 3.8 2021-11-10)](https://esotericsoftware.com/files/runtimes/unity/spine-unity-3.8-2021-11-10.unitypackage). You need to download it manualy, and put it on Plugins folder.

- Download and install [mixer-unity](https://github.com/axieinfinity/mixer-unity) from github.


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

## Building Project
You need add `AxieMixerShaderVariants` into Preloaded Shaders (Project Settings/Graphics/Shader Loading)

## Scene demo

In the asset, there are 2 demos that you can explore. You can find these scenes:

- `DemoMixer`: or so called PlayGround. 

- `FlappyAxie`: This is a minigame based on FlappyBird. In this demo, you can find the `AxieFigure` class which helps to setup `SkeletonAnimation` automatically. (The `axie id` and `genes` are static though)


