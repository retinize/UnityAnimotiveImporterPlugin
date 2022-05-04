namespace AnimotiveImporterEditor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class Pose : MonoBehaviour
    {
        private static readonly string _posesBase =
            @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses";

        public string LoadJson = "";
        public string SaveJson = "";

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

            for (int i = 0; i < transformInfoList.TransformsByStrings.Count; i++)
            {
                Transform tr = transform.FindChildRecursively(transformInfoList.TransformsByStrings[i].Name);

                if (tr != null)
                {
                    tr.localPosition = transformInfoList.TransformsByStrings[i].LocalPosition;
                    tr.localRotation = transformInfoList.TransformsByStrings[i].LocalRotation;
                    tr.localScale    = transformInfoList.TransformsByStrings[i].LocalScale;
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

            for (int i = 0; i < pluginTPoseTransformInfoList.TransformsByStrings.Count; i++)
            {
                Transform tr = transform.FindChildRecursively(pluginTPoseTransformInfoList.TransformsByStrings[i].Name);

                if (tr != null)
                {
                    tr.localPosition = pluginTPoseTransformInfoList.TransformsByStrings[i].LocalPosition;

                    Quaternion inverseAnimotiveTpose =
                        Quaternion.Inverse(animotiveTPoseTransformInfoList.TransformsByStrings[i].GlobalRotation);

                    Quaternion poseRotation =
                        frankGestureAnimotiveTransformInfoList.TransformsByStrings[i].GlobalRotation;

                    Quaternion editorTPoseRotation = pluginTPoseTransformInfoList.TransformsByStrings[i].GlobalRotation;

                    tr.rotation = inverseAnimotiveTpose * poseRotation * editorTPoseRotation;

                    tr.localScale = pluginTPoseTransformInfoList.TransformsByStrings[i].LocalScale;
                }
            }
        }


        [ContextMenu("Save Pose")]
        public void SaveThisPose()
        {
            string path = string.Concat(Directory.GetCurrentDirectory(), _posesBase);


            List<Transform> transforms = new List<Transform>();

            TransformInfoList temp = new TransformInfoList();
            temp.TransformsByStrings.Add(new TransformsByString(transform));
            GetAllChildrenRecursively(ref transforms, transform, ref temp);


            string jsonString = JsonUtility.ToJson(temp, true);
            File.WriteAllText(string.Concat(path, $"\\{SaveJson}.json"), jsonString);
            AssetDatabase.Refresh();
        }

        private void GetAllChildrenRecursively(ref List<Transform>   transforms, Transform targetTransform,
                                               ref TransformInfoList transformInfoList)
        {
            for (int i = 0; i < targetTransform.childCount; i++)
            {
                Transform child = targetTransform.GetChild(i);
                if (!transforms.Contains(child))
                {
                    transforms.Add(child);
                    transformInfoList.TransformsByStrings.Add(new TransformsByString(child));
                }

                if (child.childCount > 0)
                {
                    GetAllChildrenRecursively(ref transforms, child, ref transformInfoList);
                }
            }
        }
    }
}