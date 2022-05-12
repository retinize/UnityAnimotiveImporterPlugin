namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    ///     Pose: All bone's rotation,position,scale values for one frame.
    ///     The main reason for this class to exist is to test if we can have the same visual pose of a character on another
    ///     regardless of their base rotation('orient' in maya) values. The reason why we needed to test this is because in
    ///     Animotive there's a bindpose reset process taking place before character import and we don't have it here so
    ///     the base characters doesn't have same t-pose and other frame values which disrupts the animation.
    ///     NOTE ! : This is temporary because bindpose reset process will be lifted in the future versions of Animotive.
    /// </summary>
    public class IT_PoseTestManager : MonoBehaviour
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
        public string Pose           = "";

        /// <summary>
        ///     Loads the current values of the animator bone's recursively from a JSON file
        /// </summary>
        [ContextMenu("Load Pose")]
        public void LoadPoseFromJson()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);

            string text = File.ReadAllText(string.Concat(path, $"\\{LoadJson}.json"));


            IT_TransformInfoList transformInfoList = JsonUtility.FromJson<IT_TransformInfoList>(text);

            foreach (IT_TransformsByString pair in transformInfoList.TransformsByStrings)
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

        /// <summary>
        ///     Loads the current values of the animator bone's recursively from a JSON file.and adds the t-pose
        ///     rotations to them.
        /// </summary>
        [ContextMenu("Fix And Load Pose")]
        public void LoadPoseFromJsonAdditively()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);

            string pluginTpose        = File.ReadAllText(string.Concat(path, $"\\{EditorTPose}.json"));
            string animotiveTposeText = File.ReadAllText(string.Concat(path, $"\\{AnimotiveTPose}.json"));
            string frankAnimotiveGestureText =
                File.ReadAllText(string.Concat(path, $"\\{Pose}.json"));

            IT_TransformInfoList pluginTPoseTransformInfoList = JsonUtility.FromJson<IT_TransformInfoList>(pluginTpose);
            IT_TransformInfoList animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(animotiveTposeText);
            IT_TransformInfoList frankGestureAnimotiveTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(frankAnimotiveGestureText);

            for (int i = 0;
                 i < pluginTPoseTransformInfoList
                     .TransformsByStrings.Count;
                 i++)
            {
                IT_TransformsByString pair = pluginTPoseTransformInfoList
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

        /// <summary>
        ///     Saves the current values of the animator bone's recursively to a JSON file.
        /// </summary>
        [ContextMenu("Save Pose")]
        public void SavePoseToJson()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);


            IT_TransformInfoList transformInfoList = new IT_TransformInfoList();

            foreach (HumanBodyBones pair in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (pair == HumanBodyBones.LastBone || !SaveAnimator.GetBoneTransform(pair))
                {
                    continue;
                }

                Transform boneTransform = SaveAnimator.GetBoneTransform(pair);
                transformInfoList.TransformsByStrings.Add(new IT_TransformsByString(boneTransform, pair));
            }


            string jsonString = JsonUtility.ToJson(transformInfoList, true);
            File.WriteAllText(string.Concat(path, $"\\{SaveJson}.json"), jsonString);
            AssetDatabase.Refresh();
        }
    }
}