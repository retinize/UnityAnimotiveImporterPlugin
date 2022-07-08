using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Retinize
{
    public static class IT_ZeroJointRotations
    {
        public static void ZeroRotations(List<Transform> allBones, GameObject root)
        {
            //If the joint orientation are not zeroed
            if (allBones.Any(trans => trans.rotation.eulerAngles.magnitude > 0.1f))
            {
                Debug.LogError("Joint Rotations not zeroed, forcing to zero with reskin");
                var bakedMeshForRenderer = new Dictionary<Renderer, Mesh>();
                var skinnedMeshRenders = root.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinnedMeshRenders)
                {
                    var bake = new Mesh();
                    renderer.BakeMesh(bake);
                    bake.name = "Bake";
                    var bakeScale = bake.vertices.Max(vertex => vertex.y) -
                                    bake.vertices.Min(vertex => vertex.y);
                    var sharedScale = renderer.sharedMesh.vertices.Max(vertex => vertex.y) -
                                      renderer.sharedMesh.vertices.Min(vertex => vertex.y);
                    var scaler = sharedScale / bakeScale;
                    renderer.sharedMesh.vertices =
                        bake.vertices.Select(vertex => vertex * scaler).ToArray();

                    bakedMeshForRenderer.Add(renderer, bake);
                }

                var transforms = allBones
                    .OrderByDescending(tran => tran.GetComponentsInParent<Transform>().Length).ToArray();
                var transformToParent = new Dictionary<Transform, Transform>();
                //Stores reference to parent for each bone and then de-parents
                for (var i = 0; i < transforms.Length; i++)
                {
                    transformToParent.Add(transforms[i], transforms[i].parent);
                    transforms[i].parent = null;
                }

                //Forces rotation of all bones to 0
                for (var i = 0; i < transforms.Length; i++)
                {
                    transforms[i].rotation = Quaternion.identity;
                }

                foreach (var pair in transformToParent)
                {
                    pair.Key.parent = pair.Value;
                }

                foreach (var renderer in skinnedMeshRenders)
                {
                    var rendererTransform = renderer.transform;
                    var oldBindPoses = new Matrix4x4[renderer.bones.Length];
                    for (var i = 0; i < renderer.bones.Length; i++)
                    {
                        oldBindPoses[i] = renderer.sharedMesh.bindposes[i];
                    }

                    var mag = oldBindPoses[0].lossyScale.magnitude;
                    var mag2 = (renderer.bones[0].worldToLocalMatrix * rendererTransform.localToWorldMatrix)
                        .lossyScale.magnitude;
                    var scale = mag / mag2;
                    rendererTransform.localScale = scale * rendererTransform.localScale;
                    var bindPoses = new Matrix4x4[renderer.bones.Length];
                    for (var i = 0; i < renderer.bones.Length; i++)
                    {
                        bindPoses[i] = renderer.bones[i].worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                    }

                    renderer.sharedMesh.bindposes = bindPoses;
                }
            }
        }
    }
}