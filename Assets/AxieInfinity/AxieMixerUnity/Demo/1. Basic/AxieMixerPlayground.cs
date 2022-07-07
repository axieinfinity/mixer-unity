using System.Collections;
using System.Collections.Generic;
using AxieCore.AxieMixer;
using AxieMixer.Unity;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Game
{
    public class AxieMixerPlayground : MonoBehaviour
    {
        [SerializeField] Button mixBtn;
        [SerializeField] Button leftAnimBtn;
        [SerializeField] Button rightAnimBtn;
        [SerializeField] Dropdown animationDropDown;
        [SerializeField] InputField axieIdInputField;
        [SerializeField] Dropdown bodyDropDown;
        [SerializeField] Toggle allAxieToggle;
        [SerializeField] Toggle customIdToggle;
        [SerializeField] RectTransform rootTF;

        Axie2dBuilder builder => Mixer.Builder;

        const bool USE_GRAPHIC = false;

        private void OnEnable()
        {
            mixBtn.onClick.AddListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            customIdToggle.onValueChanged.AddListener((b) => { if (b) OnSwitch(); });
            animationDropDown.onValueChanged.AddListener((_) => OnAnimationChanged());
            leftAnimBtn.onClick.AddListener(() => OnAnimationStep(-1));
            rightAnimBtn.onClick.AddListener(() => OnAnimationStep(1));
        }

        private void OnDisable()
        {
            mixBtn.onClick.RemoveListener(OnMixButtonClicked);
            allAxieToggle.onValueChanged.RemoveAllListeners();
            customIdToggle.onValueChanged.RemoveAllListeners();
            animationDropDown.onValueChanged.RemoveAllListeners();
            leftAnimBtn.onClick.RemoveAllListeners();
            rightAnimBtn.onClick.RemoveAllListeners();
        }

        void Start()
        {
            Mixer.Init();
            List<string> animationList = builder.axieMixerMaterials.GetMixerStuff(AxieFormType.Normal).GetAnimatioNames();
            animationDropDown.ClearOptions();
            animationDropDown.AddOptions(animationList);

            TestAll();
        }

        void OnSwitch()
        {
            bodyDropDown.gameObject.SetActive(allAxieToggle.isOn);
            axieIdInputField.gameObject.SetActive(customIdToggle.isOn);
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

        void TestAll()
        {
            ClearAll();
            List<(string, string, int, int)> bodies = new List<(string, string, int, int)>();
            string[] specialBodys = new[]
            {
                "body-normal",
                "body-bigyak",
                "body-curly",
                "body-fuzzy",
                "body-spiky",
                "body-sumo",
                "body-wetdog",
            };

            int k = 0;
            string bodyMode = bodyDropDown.options[bodyDropDown.value].text.ToLower().Replace("body ", "");
            for (int classIdx = 0;classIdx < 6;classIdx++)
            {
                var characterClass = (CharacterClass)classIdx;
                for (int classValue = 2;classValue <= 12;classValue += 2)
                {
                    string key = $"{characterClass}-{classValue:00}";
                    //
                    if (bodyMode == "random")
                    {
                        bodies.Add((key, specialBodys[(k++) % specialBodys.Length], classIdx, classValue));
                    }
                    else
                    {
                        bodies.Add((key, $"body-{bodyMode}", classIdx, classValue));
                    }
                }
            }

            for (int classIdx = 0;classIdx < 6;classIdx++)
            {
                var characterClass = (CharacterClass)classIdx;
                string key = $"{characterClass}-mystic-02";
                bodies.Add((key, (classIdx % 2 == 0) ? "body-mystic-normal" : "body-mystic-fuzzy", classIdx, 2));
            }

            {
                for (int classValue = 1;classValue <= 2;classValue += 1)
                {
                    string key = $"xmas-{classValue:00}";
                    bodies.Add((key, "body-frosty", 0, classValue));
                }
            }
            {
                for (int classValue = 1;classValue <= 3;classValue += 1)
                {
                    string key = $"japan-{classValue:00}";
                    bodies.Add((key, "body-normal", 0, classValue));
                }
            }
            {
                for (int classValue = 0;classValue <= 1;classValue += 1)
                {
                    string key = $"agamo-{classValue:00}";
                    bodies.Add((key, "body-agamo", 0, classValue));
                }
            }

            int total = 0;
            foreach (var (key, body, classIdx, classValue) in bodies)
            {
                var characterClass = (CharacterClass)classIdx;
                string finalBody = body;
                string keyAdjust = key.Replace("-06", "-02").Replace("-12", "-04");
                var adultCombo = new Dictionary<string, string> {
                    {"back", key },
                    {"body", finalBody },
                    {"ears", key },
                    {"ear", key },
                    {"eyes", keyAdjust },
                    {"horn", key },
                    {"mouth", keyAdjust },
                    {"tail", key },
                    {"body-class", characterClass.ToString() },
                    {"body-id", " 2727 " },
                };

                float scale = 0.0018f;
                byte colorVariant = (byte)builder.GetSampleColorVariant(characterClass, classValue);

                var builderResult = builder.BuildSpineAdultCombo(adultCombo, colorVariant, scale);

                //Test
                GameObject go = new GameObject("DemoAxie");
                int row = total / 6;
                int col = total % 6;
                go.transform.localPosition = new Vector3(row * 1.85f, col * 1.5f) - new Vector3(7.9f, 4.8f, 0);

                SkeletonAnimation runtimeSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
                runtimeSkeletonAnimation.gameObject.layer = LayerMask.NameToLayer("Player");
                runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
                runtimeSkeletonAnimation.transform.localScale = Vector3.one;
                var meshRenderer = runtimeSkeletonAnimation.GetComponent<MeshRenderer>();
                meshRenderer.sortingOrder = 10 * total;
                total++;

                runtimeSkeletonAnimation.gameObject.AddComponent<AutoBlendAnimController>();
                runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

                runtimeSkeletonAnimation.state.TimeScale = 0.5f;
                runtimeSkeletonAnimation.skeleton.FindSlot("shadow").Attachment = null;
                if (builderResult.adultCombo.ContainsKey("body") &&
                      builderResult.adultCombo["body"].Contains("mystic") &&
                      builderResult.adultCombo.TryGetValue("body-class", out var bodyClass) &&
                      builderResult.adultCombo.TryGetValue("body-id", out var bodyId))
                {
                    runtimeSkeletonAnimation.gameObject.AddComponent<MysticIdController>().Init(bodyClass, bodyId);
                }
            }
            Debug.Log("Done");
        }

        void ProcessMixer(string axieId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }
            float scale = 0.01f;

            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, scale, isGraphic);


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
            var skeletonAnimations = FindObjectsOfType<SkeletonAnimation>();
            foreach (var p in skeletonAnimations)
            {
                Destroy(p.transform.parent.gameObject);
            }
            var skeletonGraphics = FindObjectsOfType<SkeletonGraphic>();
            foreach (var p in skeletonGraphics)
            {
                Destroy(p.transform.gameObject);
            }
        }

        void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult)
        {
            ClearAll();
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
            ClearAll();

            var skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(builderResult.skeletonDataAsset, rootTF, builderResult.sharedGraphicMaterial);
            skeletonGraphic.rectTransform.sizeDelta = new Vector2(1, 1);
            skeletonGraphic.rectTransform.localScale = Vector3.one;
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
                TestAll();
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
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC);
                }
            }
            isFetchingGenes = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //ScreenCapture.CaptureScreenshot(string.Format("Screenshots/capture_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")));
                var cam = Camera.main;
                // Set a mask to only draw only elements in this layer. e.g., capture your player with a transparent background.
                cam.cullingMask = LayerMask.GetMask("Player");

                string filename = string.Format("Screenshots/capture_{0}.png", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
                int width = Screen.width;
                int height = Screen.height;
                if (true)
                {
                    CaptureScreenshot.SimpleCaptureTransparentScreenshot(cam, width, height, filename);
                }
                else
                {
                    CaptureScreenshot.CaptureTransparentScreenshot(cam, width, height, filename);
                }
            }
        }
    }


    public static class CaptureScreenshot
    {
        public static void CaptureTransparentScreenshot(Camera cam, int width, int height, string screengrabfile_path)
        {
            // This is slower, but seems more reliable.
            var bak_cam_targetTexture = cam.targetTexture;
            var bak_cam_clearFlags = cam.clearFlags;
            var bak_RenderTexture_active = RenderTexture.active;

            var tex_white = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var tex_black = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Must use 24-bit depth buffer to be able to fill background.
            var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var grab_area = new Rect(0, 0, width, height);

            RenderTexture.active = render_texture;
            cam.targetTexture = render_texture;
            cam.clearFlags = CameraClearFlags.SolidColor;

            cam.backgroundColor = Color.black;
            cam.Render();
            tex_black.ReadPixels(grab_area, 0, 0);
            tex_black.Apply();

            cam.backgroundColor = Color.white;
            cam.Render();
            tex_white.ReadPixels(grab_area, 0, 0);
            tex_white.Apply();

            // Create Alpha from the difference between black and white camera renders
            for (int y = 0;y < tex_transparent.height;++y)
            {
                for (int x = 0;x < tex_transparent.width;++x)
                {
                    float alpha = tex_white.GetPixel(x, y).r - tex_black.GetPixel(x, y).r;
                    alpha = 1.0f - alpha;
                    Color color;
                    if (alpha == 0)
                    {
                        color = Color.clear;
                    }
                    else
                    {
                        color = tex_black.GetPixel(x, y) / alpha;
                    }
                    color.a = alpha;
                    tex_transparent.SetPixel(x, y, color);
                }
            }

            // Encode the resulting output texture to a byte array then write to the file
            byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
            System.IO.File.WriteAllBytes(screengrabfile_path, pngShot);

            cam.clearFlags = bak_cam_clearFlags;
            cam.targetTexture = bak_cam_targetTexture;
            RenderTexture.active = bak_RenderTexture_active;
            RenderTexture.ReleaseTemporary(render_texture);

            Texture2D.Destroy(tex_black);
            Texture2D.Destroy(tex_white);
            Texture2D.Destroy(tex_transparent);
        }

        public static void SimpleCaptureTransparentScreenshot(Camera cam, int width, int height, string screengrabfile_path)
        {
            // Depending on your render pipeline, this may not work.
            var bak_cam_targetTexture = cam.targetTexture;
            var bak_cam_clearFlags = cam.clearFlags;
            var bak_RenderTexture_active = RenderTexture.active;

            var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Must use 24-bit depth buffer to be able to fill background.
            var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var grab_area = new Rect(0, 0, width, height);

            RenderTexture.active = render_texture;
            cam.targetTexture = render_texture;
            cam.clearFlags = CameraClearFlags.SolidColor;

            // Simple: use a clear background
            cam.backgroundColor = Color.clear;
            cam.Render();
            tex_transparent.ReadPixels(grab_area, 0, 0);
            tex_transparent.Apply();

            // Encode the resulting output texture to a byte array then write to the file
            byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
            System.IO.File.WriteAllBytes(screengrabfile_path, pngShot);

            cam.clearFlags = bak_cam_clearFlags;
            cam.targetTexture = bak_cam_targetTexture;
            RenderTexture.active = bak_RenderTexture_active;
            RenderTexture.ReleaseTemporary(render_texture);

            Texture2D.Destroy(tex_transparent);
        }
    }
}
