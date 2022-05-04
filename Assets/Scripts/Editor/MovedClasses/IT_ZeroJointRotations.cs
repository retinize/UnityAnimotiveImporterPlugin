using System.Collections;
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
            if (allBones.Any((Transform trans) => (trans.rotation.eulerAngles.magnitude > 0.1f)))
            {
                Debug.LogError("Joint Rotations not zeroed, forcing to zero with reskin");
                Dictionary<Renderer, Mesh> bakedMeshForRenderer = new Dictionary<Renderer, Mesh>();
                var skinnedMeshRenders = root.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var renderer in skinnedMeshRenders)
                {
                    Mesh bake = new Mesh();
                    renderer.BakeMesh(bake);
                    bake.name = "Bake";
                    float bakeScale = bake.vertices.Max((Vector3 vertex) => (vertex.y)) -
                                      bake.vertices.Min((Vector3 vertex) => (vertex.y));
                    float sharedScale = renderer.sharedMesh.vertices.Max((Vector3 vertex) => (vertex.y)) -
                                        renderer.sharedMesh.vertices.Min((Vector3 vertex) => (vertex.y));
                    float scaler = sharedScale / bakeScale;
                    renderer.sharedMesh.vertices =
                        bake.vertices.Select((Vector3 vertex) => (vertex * scaler)).ToArray();

                    bakedMeshForRenderer.Add(renderer, bake);
                }

                Transform[] transforms = allBones
                    .OrderByDescending((Transform tran) => (tran.GetComponentsInParent<Transform>().Length)).ToArray();
                Dictionary<Transform, Transform> transformToParent = new Dictionary<Transform, Transform>();
                //Stores reference to parent for each bone and then de-parents
                for (int i = 0; i < transforms.Length; i++)
                {
                    transformToParent.Add(transforms[i], transforms[i].parent);
                    transforms[i].parent = null;
                }
                //Forces rotation of all bones to 0
                for (int i = 0; i < transforms.Length; i++)
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
                    Matrix4x4[] oldBindPoses = new Matrix4x4[renderer.bones.Length];
                    for (int i = 0; i < renderer.bones.Length; i++)
                    {
                        oldBindPoses[i] = renderer.sharedMesh.bindposes[i];
                    }
                    float mag = oldBindPoses[0].lossyScale.magnitude;
                    float mag2 = (renderer.bones[0].worldToLocalMatrix * rendererTransform.localToWorldMatrix)
                        .lossyScale.magnitude;
                    float scale = mag / mag2;
                    rendererTransform.localScale = scale * rendererTransform.localScale;
                    Matrix4x4[] bindPoses = new Matrix4x4[renderer.bones.Length];
                    for (int i = 0; i < renderer.bones.Length; i++)
                    {
                        bindPoses[i] = renderer.bones[i].worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                    }
                    renderer.sharedMesh.bindposes = bindPoses;
                }
            }
        }
    }
}
