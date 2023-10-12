# Animotive Animation Transfer Tools for Unity

1. Download the latest release from releases page and download it.
2. Once downloaded, drag & drop the .unitypackage into your Unity project.
3. Once imported you'll see a new tab appearing in top menu called "Animotive"
![tab](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/c16e7850-80ba-41b6-a271-8c066cc954b2)
4. Clicking to that tab will open a new window called "Animotive Importer" 
![image](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/08ba0c9d-0cdb-47a2-80e5-c5e40962c0f7)
5. To select the "Unity Export" click to "Choose Folder to Import"
![image](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/24a57119-4291-47cb-b197-95ee1960ae69)
6. It'll prompt a file browser window from where you can choose the exported folder.
![import_browser](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/d5640ebe-118e-41e0-a524-e4528cf72b60)
7. After selecting the export folder the editor-system will run validations and if any fail it'll show you a warning with an explanation as to why it has failed. If all goes good it'll start importing FBXes. 
IMPORTANT NOTE: Importing FBX files into Unity may take a while depending on how big are the source files and how many of them are there.
 - From now on the importer won't re-import the same FBXes again and again instead it'll use the existing ones for future. If you want to re-import the FBXes into your project to overwrite the existing ones you can check "Reimport Characters" checkbox
   ![image](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/3fad3b96-63ef-4752-9e70-0c18a3cf1f99)

9. Once the validation is done you'll see "Import Animotive Scene" button appearing on the window
![image](https://github.com/retinize/UnityAnimotiveImporterPlugin/assets/98883482/cca2c915-e1ec-4ffb-8cb6-57bac8082d4c)
10. 
