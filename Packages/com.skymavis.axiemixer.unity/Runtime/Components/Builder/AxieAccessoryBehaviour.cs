using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class AxieAccessoryBehaviour : MonoBehaviour
    {
        static Material TemplateMaterial;

        public SkeletonAnimation axieSkeleton;
        public float scale = .001f;
        public int bodyLayer = 0;
        public string air;
        public string cheek;
        public string ground;
        public string hip;
        public string neck;

        Dictionary<AxieAccessoryType, GameObject> _accessories = new Dictionary<AxieAccessoryType, GameObject>();

        void Reset()
        {
            if (axieSkeleton == null) axieSkeleton = GetComponentInParent<SkeletonAnimation>();
        }

        void Start()
        {
            Reset();
            
            Load(AxieAccessoryType.Cheek, cheek, bodyLayer + 1);
            Load(AxieAccessoryType.Hip, hip, bodyLayer + 2);
            Load(AxieAccessoryType.Neck, neck, bodyLayer + 3);
            Load(AxieAccessoryType.Air, air, bodyLayer + 4);
            Load(AxieAccessoryType.Ground, ground, bodyLayer + 5);
        }

        public void Load(AxieAccessoryType accessoryType, string accessoryName, int slotLayer)
        {
            Unload(accessoryType);

            if (axieSkeleton == null || string.IsNullOrWhiteSpace(accessoryName)) return;

            var bone = axieSkeleton.skeleton.FindBone($"@body-{accessoryType.ToString().ToLower()}");

            if (bone == null) return;

            var accessorySkeletonData = Resources.Load<SkeletonDataAsset>($"accessory/{accessoryType.ToString().ToLower()}/{accessoryName}/{accessoryName}_SkeletonData");
            if(accessorySkeletonData == null) return;
            accessorySkeletonData.scale = scale;
            var accessorySkeleton = SkeletonAnimation.NewSkeletonAnimationGameObject(accessorySkeletonData);
            accessorySkeleton.name = $"{accessoryType}-{accessoryName}";
            accessorySkeleton.transform.parent = transform;
            accessorySkeleton.state.SetAnimation(0, accessorySkeletonData.GetSkeletonData(true).Animations.Items[0], true);
            accessorySkeleton.GetComponent<MeshRenderer>().sortingOrder = slotLayer;
            var follower = accessorySkeleton.gameObject.AddComponent<BoneFollower>();
            follower.SkeletonRenderer = axieSkeleton;
            follower.bone = bone;

            _accessories[accessoryType] = accessorySkeleton.gameObject;
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
