using System;
using System.IO;
using NUnit.Framework;
using Retinize.Editor.AnimotiveImporter;

namespace DefaultNamespace
{
    [TestFixture]
    public class IT_AnimotiveImporterEditorUtilitiesTests
    {
        // arrange, act assert


        private const string _SystemPath =
            @"C:\Users\jack\Desktop\Unity\UnityAnimotiveImporterPlugin\Assets\UnityFiles\Animations\test.anim";

        private const string _AssetDatabasePath = @"Assets\UnityFiles\Animations\test.anim";


        [Test]
        public void ConvertSystemPathToAssetDatabasePath_WhenCalled_ReturnsAssetDatabasePath()
        {
            string result = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(_SystemPath);


            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(_AssetDatabasePath));
            Assert.That(
                delegate { IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(_SystemPath); },
                Throws.Nothing);
        }

        [Test]
        public void ConvertSystemPathToAssetDatabasePath_WhenCalledWithEmptyString_ThrowsIndexOutOfRangeException()
        {
            Assert.That(
                delegate { IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(String.Empty); },
                Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void ConvertAssetDatabasePathToSystemPath_WhenCalledWithValidPath_ReturnsFullSystemPath()
        {
            string result = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(_SystemPath);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(_AssetDatabasePath));
            Assert.That(
                delegate { IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(_SystemPath); },
                Throws.Nothing);
        }

        [Test]
        public void ConvertAssetDatabasePathToSystemPath_WhenCalledWithEmptyString_ThrowsException()
        {
            Assert.That(
                delegate { IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(string.Empty); },
                Throws.Exception);
        }

        [Test]
        public void DoesAssetExist_WhenCalledWithANonExistingAssetName_ReturnsFalse()
        {
            bool result = IT_AnimotiveImporterEditorUtilities.DoesAssetExist("NoAssetWithThatNameExists.noExtension");
            Assert.That(result, Is.False);
            Assert.That(
                delegate
                {
                    IT_AnimotiveImporterEditorUtilities.DoesAssetExist("NoAssetWithThatNameExists.noExtension");
                }, Throws.Nothing);
        }

        [Test]
        public void CreateAssetsFolders_WhenCalled_CreatesFolders()
        {
            IT_AnimotiveImporterEditorUtilities.CreateAssetsFolders();

            string baseDir = Directory.GetCurrentDirectory();


            bool directory1 = Directory.Exists(Path.Combine(baseDir,
                IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory));

            bool directory2 = Directory.Exists(Path.Combine(baseDir,
                IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory));

            bool directory3 = Directory.Exists(Path.Combine(baseDir,
                IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory));

            bool directory4 = Directory.Exists(Path.Combine(baseDir,
                IT_AnimotiveImporterEditorConstants.UnityFilesCameraAnimationDirectory));

            bool directory5 =
                Directory.Exists(Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory));

            Assert.That(baseDir, Is.Not.Empty);
            Assert.That(baseDir, Is.Not.Null);

            Assert.That(directory1, Is.True);
            Assert.That(directory2, Is.True);
            Assert.That(directory3, Is.True);
            Assert.That(directory4, Is.True);
            Assert.That(directory5, Is.True);
        }


        [Test]
        [TestCase("Charles Character Root_TransformClip_Capture1", IT_ClipType.TransformAnimationClip)]
        [TestCase("Charles Holder_PropertiesClip_Capture1", IT_ClipType.PropertiesClip)]
        [TestCase("Charles Holder_AudioClip_Capture1_InitTime_00_00_00", IT_ClipType.AudioClip)]
        [TestCase("Charles_FacialParametersAnimation_Clip_1_Group 1_Take_0.json", IT_ClipType.FacialAnimationClip)]
        [TestCase("ajskhdjkash bdjkabyuqdbjay hsgudyqwgduyqgwud", IT_ClipType.None)]
        [TestCase("", IT_ClipType.None)]
        [TestCase(null, IT_ClipType.None)]
        public void GetClipTypeFromClipName_WhenGivenInput_ReturnsCorrectClipType(string clipName,
            IT_ClipType expectedClipType)
        {
            var result = IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(clipName);

            Assert.That(result, Is.EqualTo(expectedClipType));
        }
    }
}