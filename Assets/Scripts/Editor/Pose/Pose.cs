namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class Pose : MonoBehaviour
    {
        private static readonly string _posesBase =
            @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses";

        [Header("Load")] public string   LoadJson = "";
        public                  Animator LoadAnimator;

        [Header("Save")] public string SaveJson = "";

        public Animator SaveAnimator;

        [Space] [Header("For Loading Fixed Pose")]
        public string EditorTPose = "";

        public string AnimotiveTPose = "";
        public string pose           = "";


        [ContextMenu("Load Pose")]
        public void LoadPose()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);

            string text = File.ReadAllText(string.Concat(path, $"\\{LoadJson}.json"));


            TransformInfoList transformInfoList = JsonUtility.FromJson<TransformInfoList>(text);

            foreach (TransformsByString pair in transformInfoList.TransformsByStrings)
            {
                Transform tr = LoadAnimator.GetBoneTransform(pair.Name);
                if (tr != null)
                {
                    tr.localPosition = pair.LocalPosition;
                    tr.localRotation = pair.LocalRotation;
                    tr.localScale    = pair.LocalScale;
                }
            }
        }


        [ContextMenu("Fix And Load Pose")]
        public void FixAndLoad()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);

            string pluginTpose        = File.ReadAllText(string.Concat(path, $"\\{EditorTPose}.json"));
            string animotiveTposeText = File.ReadAllText(string.Concat(path, $"\\{AnimotiveTPose}.json"));
            string frankAnimotiveGestureText =
                File.ReadAllText(string.Concat(path, $"\\{pose}.json"));

            TransformInfoList pluginTPoseTransformInfoList = JsonUtility.FromJson<TransformInfoList>(pluginTpose);
            TransformInfoList animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<TransformInfoList>(animotiveTposeText);
            TransformInfoList frankGestureAnimotiveTransformInfoList =
                JsonUtility.FromJson<TransformInfoList>(frankAnimotiveGestureText);

            for (int i = 0;
                 i < pluginTPoseTransformInfoList
                     .TransformsByStrings.Count;
                 i++)
            {
                TransformsByString pair = pluginTPoseTransformInfoList
                    .TransformsByStrings[i];
                Transform tr = LoadAnimator.GetBoneTransform(pair.Name);

                if (tr != null)
                {
                    tr.localPosition = pair.LocalPosition;

                    Quaternion inverseAnimotiveTpose =
                        Quaternion.Inverse(animotiveTPoseTransformInfoList.TransformsByStrings[i].GlobalRotation);

                    Quaternion poseRotation =
                        frankGestureAnimotiveTransformInfoList.TransformsByStrings[i].GlobalRotation;

                    Quaternion editorTPoseRotation = pair.GlobalRotation;

                    tr.rotation = inverseAnimotiveTpose * poseRotation * editorTPoseRotation;

                    tr.localScale = pair.LocalScale;
                }
            }
        }


        [ContextMenu("Save Pose")]
        public void SaveThisPose()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);


            TransformInfoList transformInfoList = new TransformInfoList();

            foreach (HumanBodyBones pair in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (pair == HumanBodyBones.LastBone || !SaveAnimator.GetBoneTransform(pair))
                {
                    continue;
                }

                Transform boneTransform = SaveAnimator.GetBoneTransform(pair);
                transformInfoList.TransformsByStrings.Add(new TransformsByString(boneTransform, pair));
            }


            string jsonString = JsonUtility.ToJson(transformInfoList, true);
            File.WriteAllText(string.Concat(path, $"\\{SaveJson}.json"), jsonString);
            AssetDatabase.Refresh();
        }
    }
}