using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class AccessoryGraphicController : MonoBehaviour
    {
        SkeletonGraphic skeletonGraphic;

        private float _scale = .001f;

        Dictionary<AxieAccessoryType, GameObject> _accessories = new Dictionary<AxieAccessoryType, GameObject>();

        public AccessoryGraphicController Init(float scale)
        {
            Reset();
            _scale = scale;
            return this;
        }

        public AccessoryGraphicController Reset()
        {
            skeletonGraphic = gameObject.GetComponentInParent<SkeletonGraphic>();
            foreach (var p in _accessories)
            {
                Destroy(p.Value);
            }
            _accessories.Clear();
            return this;
        }

        public AccessoryGraphicController Equip(string accessoryName)
        {
            int off = 0;
            while (off < accessoryName.Length)
            {
                if (char.IsDigit(accessoryName[off])) break;
                off++;
            }
            if (off < accessoryName.Length)
            {
                string slotName = accessoryName.Substring(0, off);
                if (System.Enum.TryParse<AxieAccessoryType>(slotName, true, out var accessoryType))
                {
                    Load(accessoryType, accessoryName);
                }
            }
            return this;
        }

        public AccessoryGraphicController EquipFromCombo(Dictionary<string, string> adultCombo)
        {
            Reset();
            foreach (var p in adultCombo)
            {

            }
            return this;
        }

       

        private void Load(AxieAccessoryType accessoryType, string accessoryName)
        {
            Unload(accessoryType);

            if (skeletonGraphic == null || string.IsNullOrWhiteSpace(accessoryName)) return;

            string boneName = $"@body-{accessoryType.ToString().ToLower()}";
            var bone = skeletonGraphic.Skeleton.FindBone(boneName);

            if (bone == null) return;

            var accessorySkeletonData = Resources.Load<SkeletonDataAsset>($"accessory/{accessoryType.ToString().ToLower()}/{accessoryName}/{accessoryName}_SkeletonData");
            var accessoryMat = Resources.Load<Material>($"accessory/{accessoryType.ToString().ToLower()}/{accessoryName}/{accessoryName}_Material");
            if (accessorySkeletonData == null) return;
            accessorySkeletonData.scale = _scale;
            var accessorySkeleton = SkeletonGraphic.NewSkeletonGraphicGameObject(accessorySkeletonData, skeletonGraphic.transform, accessoryMat);
            accessorySkeleton.name = $"{accessoryType}-{accessoryName}";
            accessorySkeleton.transform.SetParent(transform, false); ;
            accessorySkeleton.AnimationState.SetAnimation(0, accessorySkeletonData.GetSkeletonData(true).Animations.Items[0], true);
            accessorySkeleton.raycastTarget = false;
            var follower = accessorySkeleton.gameObject.AddComponent<BoneFollowerGraphic>();
            follower.skeletonGraphic = skeletonGraphic;
            follower.SetBone(boneName);

            _accessories[accessoryType] = accessorySkeleton.gameObject;

            int layer = 0;
            for(int i = 1; i <= (int)AxieAccessoryType.Ground;i++)
            {
                var layerType = (AxieAccessoryType)i;
                if (_accessories.TryGetValue(layerType, out var accessoryGO))
                {
                    accessoryGO.transform.SetSiblingIndex(layer);
                    layer++;
                }
            }
        }

        public void Unload(AxieAccessoryType accessoryType)
        {
            if (_accessories.TryGetValue(accessoryType, out var accessoryGO))
            {
                Destroy(accessoryGO);
            }
        }
    }
}
