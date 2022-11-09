using System.Collections.Generic;
using System.IO;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_AnimotiveImporterEditorConstants
    {
        public const string UnitySceneExtension = ".unity";
        public const string AudioExtension = ".wav";
        public const string AnimationExtension = ".anim";
        public const string PlayableExtension = ".playable";
        public const string ModelExtension = ".fbx";
        public const string FacialAnimationFileExtension = ".json";

        private const string _TransformClipName = "TransformClip";
        private const string _PropertyClipName = "PropertiesClip";
        private const string _AudioClipName = "AudioClip";
        public const string FacialAnimationClipContentString = "FacialParametersAnimation";

        public const string WarningTitle = " Animotive Reader Plugin : WARNING";

        public const string HolderPositionString = "HolderPosition";
        public const string HolderRotationString = "HolderRotation";
        public const string RootPositionString = "RootPosition";
        public const string RootRotationString = "RootRotation";
        public const string DepthOfFieldFocalLength = "DepthOfFieldFocalLength";

        public static string UnityFilesBase = Path.Combine(Directory.GetCurrentDirectory(),
            "Assets",
            "UnityFiles");

        public static string UnityFilesAudioDirectory = Path.Combine(UnityFilesBase, "Audio");


        public static string UnityFilesCharactersDirectory = Path.Combine(UnityFilesBase, "Characters");
        public static string UnityFilesScenesDirectory = Path.Combine(UnityFilesBase, "Scenes");

        public static readonly string UnityFilesPlayablesDirectory = Path.Combine(UnityFilesBase, "Playables");

        public static readonly string UnityFilesAnimationDirectory = Path.Combine(UnityFilesBase, "Animations");

        public static string UnityFilesBodyAnimationDirectory =
            Path.Combine(UnityFilesAnimationDirectory, "BodyAnimations");

        public static string UnityFilesFacialAnimationDirectory =
            Path.Combine(UnityFilesAnimationDirectory, "FacialAnimations");

        public static readonly Dictionary<IT_ClipType, string> ClipNamesByType = new Dictionary<IT_ClipType, string>
        {
            {IT_ClipType.PropertiesClip, _PropertyClipName},
            {IT_ClipType.TransformAnimationClip, _TransformClipName},
            {IT_ClipType.AudioClip, _AudioClipName},
            {IT_ClipType.FacialAnimationClip, FacialAnimationClipContentString}
        };

        public static readonly Dictionary<IT_EntityType, string> EntityTypesByKeyword =
            new Dictionary<IT_EntityType, string>
            {
                {IT_EntityType.Camera, "Camera"}, {IT_EntityType.Spotlight, "Spot Light"}
            };
    }
}