using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace SkyMavis.AxieMixer.Unity
{
    public class AccessoryController : MonoBehaviour
    {
        SkeletonAnimation skeletonAnimation;

        private float _scale = .001f;
        private string _rootSortingLayerName = "";
        private int _rootSortingOrder = 0;

        Dictionary<AxieAccessoryType, GameObject> _accessories = new Dictionary<AxieAccessoryType, GameObject>();

        public AccessoryController Init(float scale)
        {
            Reset();
            _scale = scale;
            return this;
        }

        public AccessoryController Reset()
        {
            skeletonAnimation = gameObject.GetComponentInParent<SkeletonAnimation>();
            foreach (var p in _accessories)
            {
                Destroy(p.Value);
            }
            _accessories.Clear();
            return this;
        }

        public AccessoryController Equip(string accessoryName)
        {
            int off = 0;
            while(off < accessoryName.Length)
            {
                if (char.IsDigit(accessoryName[off])) break;
                off++;
            }
            if(off < accessoryName.Length)
            {
                string slotName = accessoryName.Substring(0, off);
                if(System.Enum.TryParse<AxieAccessoryType>(slotName, true, out var accessoryType))
                {
                    Load(accessoryType, accessoryName);
                }
            }
            return this;
        }

        private void Load(AxieAccessoryType accessoryType, string accessoryName)
        {
            Unload(accessoryType);

            if (skeletonAnimation == null || string.IsNullOrWhiteSpace(accessoryName)) return;

            var bone = skeletonAnimation.skeleton.FindBone($"@body-{accessoryType.ToString().ToLower()}");

            if (bone == null) return;

            var accessorySkeletonData = Resources.Load<SkeletonDataAsset>($"accessory/{accessoryType.ToString().ToLower()}/{accessoryName}/{accessoryName}_SkeletonData");
            if (accessorySkeletonData == null) return;
            accessorySkeletonData.scale = _scale;
            var accessorySkeleton = SkeletonAnimation.NewSkeletonAnimationGameObject(accessorySkeletonData);
            accessorySkeleton.name = $"{accessoryType}-{accessoryName}";
            accessorySkeleton.transform.parent = transform;
            accessorySkeleton.state.SetAnimation(0, accessorySkeletonData.GetSkeletonData(true).Animations.Items[0], true);
            var follower = accessorySkeleton.gameObject.AddComponent<BoneFollower>();
            follower.SkeletonRenderer = skeletonAnimation;
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

        public void Update(){
            if(skeletonAnimation == null) return;
            SyncSortingOrder();
        }

        private void SyncSortingOrder(){
            //Sync soring order
            var meshRenderer = skeletonAnimation.GetComponent<MeshRenderer>();
            if(meshRenderer.sortingLayerName == _rootSortingLayerName && meshRenderer.sortingOrder == _rootSortingOrder) return;
            _rootSortingLayerName = meshRenderer.sortingLayerName;
            _rootSortingOrder = meshRenderer.sortingOrder;

            foreach (var p in _accessories)
            {
               var accessoryRenderer = p.Value.GetComponent<MeshRenderer>();
               accessoryRenderer.sortingLayerName = _rootSortingLayerName;
               accessoryRenderer.sortingOrder = _rootSortingOrder + (int)p.Key;
            }
        }
    }
}
