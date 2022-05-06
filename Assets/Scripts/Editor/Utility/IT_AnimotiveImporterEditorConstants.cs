namespace AnimotiveImporterEditor
{
    public static class IT_AnimotiveImporterEditorConstants
    {
        public const string StreamingAssetsFolderName = "StreamingAssets";
        public const string UnityFilesFolderName      = "UnityFiles";
        public const string UnitySceneExtension       = ".unity";

#region Hardcoded PoC Paths

        public const string FBXPath =
            @"Assets\AnimotivePluginExampleStructure\SampleModels\FrankBshp_Export_Master.fbx";

        public const string BlendshapeJsonPath =
            @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Frank _FacialParametersAnimation_1_T00_01_00.json";

        public const string BinaryAnimPath =
            @"/Assets/AnimotivePluginExampleStructure/Example Data/Animation/Binary/FrankErtanTest Character Root_TransformClip_Take1";

        public const string BlendshapeAnimDirectory =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Blendshape/";

        public const string BlendShapeAnimCreatedPath =
            BlendshapeAnimDirectory + "blenshapeAnim.anim";

        public const string TransformAnimDirectory =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Transform/";

        public const string TransformsAnimController = TransformAnimDirectory  + "transforms.controller";
        public const string BlendshapeAnimController = BlendshapeAnimDirectory + "blenshapes.controller";

        public const string TransformAnimPath = TransformAnimDirectory + "transforms.anim";

        public const string PlayablesCreationPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Playables\";

#endregion
    }
}