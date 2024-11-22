using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SkyMavis.AxieMixer.Unity.Demo
{
    public class AxieMixerPlayground : MonoBehaviour
    {
        [SerializeField] Button mixBtn;
        [SerializeField] GameObject customSkinLayer;
        [SerializeField] Button leftAnimBtn;
        [SerializeField] Button rightAnimBtn;
        [SerializeField] Dropdown animationDropDown;
        [SerializeField] InputField axieIdInputField;
        [SerializeField] Dropdown bodyDropDown;
        [SerializeField] Toggle allAxieToggle;
        [SerializeField] Toggle customIdToggle;
        [SerializeField] Toggle fallbackDefaultToggle;
        [SerializeField] Text skinName;
        [SerializeField] Slider customSkinSlider;
        [SerializeField] Toggle customLVToggle;
        [SerializeField] Toggle[] customPartToggles;
        [SerializeField] RectTransform rootTF;
        [SerializeField] TextAsset axieCombosAsset;

        Axie2dBuilder builder => Mixer.Builder;

        Dictionary<string, List<Dictionary<string, string>>> axieCombos;

        const bool USE_GRAPHIC = false;
        int accessoryIdx = 1;

        static string[] ACCESSORY_SLOTS = new[]
          {
                "accessory-air",
                "accessory-cheek",
                "accessory-ground",
                "accessory-hip",
                "accessory-neck",
            };

        static string[] SKIN_NAMES = new[]
        {
            "Normal",
            "Mystic",
            "Bionic",
            "Japan",
            "Xmas2018",
            "Xmas2019",
            "Summer2022a",
            "Summer2022b",
            "Summer2022c",
            "Summer2022a Shinny",
            "Summer2022b Shinny",
            "Summer2022c Shinny",
            "Nightmare",
            "Nightmare Shinny",
        };

        private void OnEnable()
        {
            mixBtn.onClick.AddListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            customIdToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            customLVToggle.onValueChanged.AddListener((b) => { OnCustomLvSwitch(); });
            animationDropDown.onValueChanged.AddListener((_) => OnAnimationChanged());
            bodyDropDown.onValueChanged.AddListener((_) => OnAxieTagChanged());
            leftAnimBtn.onClick.AddListener(() => OnAnimationStep(-1));
            rightAnimBtn.onClick.AddListener(() => OnAnimationStep(1));
            customSkinSlider.onValueChanged.AddListener((_) => OnSkinChanged());
        }

        private void OnDisable()
        {
            mixBtn.onClick.RemoveListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.RemoveAllListeners();
            customIdToggle.onValueChanged.RemoveAllListeners();
            customLVToggle.onValueChanged.RemoveAllListeners();
            animationDropDown.onValueChanged.RemoveAllListeners();
            bodyDropDown.onValueChanged.RemoveAllListeners();
            leftAnimBtn.onClick.RemoveAllListeners();
            rightAnimBtn.onClick.RemoveAllListeners();
            customSkinSlider.onValueChanged.RemoveAllListeners();
        }

        void Start()
        {
            Mixer.Init();
            List<string> animationList = builder.axieMixerMaterials.GetMixerStuff(AxieFormType.Normal).GetAnimatioNames();
            animationDropDown.ClearOptions();
            animationDropDown.AddOptions(animationList);

            JObject jAxieCombos = JObject.Parse(axieCombosAsset.text);
            axieCombos = jAxieCombos["items"].ToObject<Dictionary<string, List<Dictionary<string, string>>>>();
            List<string> axieTags = axieCombos.Keys.ToList();
            bodyDropDown.ClearOptions();
            bodyDropDown.AddOptions(axieTags);

            OnAxieTagChanged();
        }

        void OnSkinChanged()
        {
            skinName.text = $"Skin: {SKIN_NAMES[(int)customSkinSlider.value]}";
        }

        void OnSwitch()
        {
            bodyDropDown.gameObject.SetActive(allAxieToggle.isOn);
            axieIdInputField.gameObject.SetActive(customIdToggle.isOn);
            mixBtn.gameObject.SetActive(axieIdInputField.gameObject.activeSelf);
            customSkinLayer.gameObject.SetActive(axieIdInputField.gameObject.activeSelf);
        }

        void OnCustomLvSwitch()
        {
            for(int i = 0; i < customPartToggles.Length; i++)
            {
                customPartToggles[i].interactable = customLVToggle.isOn;
            }
        }

        void OnAnimationChanged()
        {
            var animName = animationDropDown.options[animationDropDown.value].text;
            var skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
            foreach (var p in skeletonAnimations)
            {
                p.state.SetAnimation(0, animName, true);
            }

            var skeletonGraphics = FindObjectsOfType<SkeletonGraphic>();
            foreach (var p in skeletonGraphics)
            {
                p.AnimationState.SetAnimation(0, animName, true);
            }
        }

        void OnAnimationStep(int step)
        {
            animationDropDown.value = (animationDropDown.value + step + animationDropDown.options.Count) % animationDropDown.options.Count;
        }

        void OnAxieTagChanged()
        {
            ClearAll();
            string sampleTag = bodyDropDown.options[bodyDropDown.value].text;

            var combos = axieCombos[sampleTag];
            for(int i=0;i< combos.Count; i++)
            {
                var combo = combos[i];
                float scale = 0.0018f;
                byte colorVariant = 0;
                if (combo.TryGetValue("colorVariant", out var colorStr) && int.TryParse(colorStr, out var colorInt))
                {
                    colorVariant = (byte)colorInt;
                }
                {
                    var builderResult = builder.BuildSpineAdultCombo(combo, colorVariant, scale);

                    //Test
                    GameObject go = new GameObject("DemoAxie");
                    int row = i / 6;
                    int col = i % 6;
                    //go.transform.localPosition = new Vector3(row * 1.6f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);
                    go.transform.localPosition = new Vector3(row * 1.85f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);

                    SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
                    runtimeSkeletonAnimation.gameObject.layer = LayerMask.NameToLayer("Player");
                    runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
                    runtimeSkeletonAnimation.transform.localScale = Vector3.one;
                    var meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
                    meshRenderer.sortingOrder = 10 * i;


                    runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
                    runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

                    runtimeSkeletonAnimation.state.TimeScale = 0.25f;
                    //runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
                    if (builderResult.adultCombo.ContainsKey("body") &&
                          builderResult.adultCombo["body"].Contains("mystic") &&
                          builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                          builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
                    {
                        runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
                    }
                }
            }
        }

        void ProcessMixer(string axieId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }
            float scale = 0.017f;

            var meta = new Dictionary<string, string>();
            //foreach (var accessorySlot in ACCESSORY_SLOTS)
            //{
            //    meta.Add(accessorySlot, $"{accessorySlot}1{System.Char.ConvertFromUtf32((int)('a') + accessoryIdx - 1)}");
            //}

            int skin = (int)customSkinSlider.value;
            bool fallbackDefault = fallbackDefaultToggle.isOn;
           
            if (fallbackDefault)
            {
                meta.Add("fallback-default", "1");
            }
            for (int i = 0; i < 6; i++)
            {
                AxiePartType partType = (AxiePartType)i;
                if (customLVToggle.isOn)
                {
                    if (customPartToggles[i].isOn)
                    {
                        meta.Add($"{partType}.stage", "1");
                    }
                    else
                    {
                        meta.Add($"{partType}.stage", "0");
                    }
                }
                //if (this.skinToggle.isChecked)
                {
                    meta.Add($"{ partType}.skin".ToLower(), skin.ToString());
                }
            }

            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            //Test
            if (isGraphic)
            {
                SpawnSkeletonGraphic(builderResult);
            }
            else
            {
                SpawnSkeletonAnimation(builderResult);
            }
        }

        void ClearAll()
        {
            ClearAllSkeletonAnimations();
            ClearAllSkeletonGraphics();
        }

        void ClearAllSkeletonAnimations()
        {
            var skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
            foreach (var p in skeletonAnimations)
            {
                Destroy(p.transform.parent.gameObject);
            }
        }

        void ClearAllSkeletonGraphics()
        {
            var skeletonGraphics = FindObjectsOfType<SkeletonGraphic>();
            foreach (var p in skeletonGraphics)
            {
                Destroy(p.transform.gameObject);
            }
        }

        void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult)
        {
            ClearAllSkeletonAnimations();
            GameObject go = new GameObject("DemoAxie");
            go.transform.localPosition = new Vector3(0f, -2.4f, 0f);
            SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.gameObject.layer = LayerMask.NameToLayer("Player");
            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
            runtimeSkeletonAnimation.transform.localScale = Vector3.one;

            runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            if (builderResult.adultCombo.ContainsKey("body") &&
                builderResult.adultCombo["body"].Contains("mystic") &&
                builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
            }
            runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
        }

        void SpawnSkeletonGraphic(Axie2dBuilderResult builderResult)
        {
            ClearAllSkeletonGraphics();

            var skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(builderResult.skeletonDataAsset, rootTF, builderResult.sharedGraphicMaterial);
            skeletonGraphic.rectTransform.sizeDelta = new Vector2(1, 1);
            skeletonGraphic.rectTransform.localScale = Vector3.one * 0.1f;
            skeletonGraphic.rectTransform.anchoredPosition = new Vector2(0f, -260f);
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Skeleton.SetSkin("default");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            skeletonGraphic.gameObject.AddComponent<AutoBlendAnimGraphicController>();
            skeletonGraphic.AnimationState.SetAnimation(0, "action/idle/normal", true);

            if (builderResult.adultCombo.ContainsKey("body") &&
             builderResult.adultCombo["body"].Contains("mystic") &&
             builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
             builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
            {
                skeletonGraphic.gameObject.AddComponent<MysticIdGraphicController>().Init(bodyClass, bodyId);
            }
        }

        bool isFetchingGenes = false;
        public void OnMixButtonClicked()
        {
            if (allAxieToggle.isOn)
            {
                return;
            }
            else
            {
                if (isFetchingGenes) return;
                StartCoroutine(GetAxiesGenes(axieIdInputField.text));
            }
        }

        public IEnumerator GetAxiesGenes(string axieId)
        {
            isFetchingGenes = true;
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            //var wr = new UnityWebRequest("https://testnet-graphql.skymavis.one/graphql", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    Debug.Log(genesStr);
                    ClearAll();
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC);
                }
            }
            isFetchingGenes = false;
        }
    }
}
