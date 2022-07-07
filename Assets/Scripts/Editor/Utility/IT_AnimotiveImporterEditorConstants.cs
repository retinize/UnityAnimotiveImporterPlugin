namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_AnimotiveImporterEditorConstants
    {
        public const string UnitySceneExtension = ".unity";
        public const string TransformClipName = "TransformClip";

        public const string WarningTitle = " Animotive Reader Plugin : WARNING";

        #region Hardcoded PoC Paths

        public const string FBXPath =
            @"Assets\AnimotivePluginExampleStructure\SampleModels\FrankBshp_Export_Master.fbx";


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