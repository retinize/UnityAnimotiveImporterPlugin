using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_AnimotiveImporterEditorConstants
    {
        public const string UnitySceneExtension = ".unity";
        private const string _TransformClipName = "TransformClip";
        private const string _PropertyClipName = "PropertiesClip";
        private const string _AudioClipName = "AudioClip";

        public const string WarningTitle = " Animotive Reader Plugin : WARNING";


        public static readonly Dictionary<IT_ClipType, string> ClipNamesByType = new Dictionary<IT_ClipType, string>
        {
            { IT_ClipType.PropertiesClip, _PropertyClipName },
            { IT_ClipType.TransformClip, _TransformClipName },
            { IT_ClipType.AudioClip, _AudioClipName }
        };

        #region Hardcoded PoC Paths

        public const string FacialAnimationSourcePath =
            @"Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Frank _FacialParametersAnimation_1_T00_01_00.json";

        public const string FacialAnimationDirectory =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Blendshape/";

        public const string FacialAnimationCreatedPath =
            FacialAnimationDirectory + "blenshapeAnim.anim";

        public const string BodyAnimationDirectory =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Transform/";

        public const string FacialAnimationController = FacialAnimationDirectory + "blenshapes.controller";

        public const string PlayablesCreationPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Playables\";

        #endregion
    }
}