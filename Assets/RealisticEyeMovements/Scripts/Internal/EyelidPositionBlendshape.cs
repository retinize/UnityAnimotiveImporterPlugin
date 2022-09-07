using System;
using UnityEngine;

namespace RealisticEyeMovements
{
	public partial class ControlData
	{
		[Serializable]
		public class EyelidPositionBlendshapeForExport
		{
			public string skinnedMeshRendererPath;
			public float defaultWeight;
			public float positionWeight;
			public int index;
			public string name;
			public bool isUsedInEalierConfig;
		}


		[Serializable]
		public class EyelidPositionBlendshape
		{
			public SkinnedMeshRenderer skinnedMeshRenderer;
			public float defaultWeight;
			public float positionWeight;
			public int index;
			public string name;
			public bool isUsedInEalierConfig;

			public static bool CanImport(EyelidPositionBlendshapeForExport import, Transform startXform)
			{
				if ( string.IsNullOrEmpty(import.skinnedMeshRendererPath) )
					return false;

				Transform t = Utils.GetTransformFromPath(startXform, import.skinnedMeshRendererPath);

				if ( t == null )
					return false;

				SkinnedMeshRenderer meshRenderer = t.GetComponent<SkinnedMeshRenderer>();

				if ( meshRenderer == null )
					return false;

				if ( false == string.IsNullOrEmpty(import.name) )
				{
					bool containsName = false;
					for ( int i=0;  i<meshRenderer.sharedMesh.blendShapeCount;  i++ )
						if ( meshRenderer.sharedMesh.GetBlendShapeName(i).Equals( import.name ) )
						{
							containsName = true;
							break;
						}

					if ( false == containsName )
						return false;
				}									

				return true;
			}


			public EyelidPositionBlendshapeForExport GetExport(Transform startXform)
			{
				EyelidPositionBlendshapeForExport export = new EyelidPositionBlendshapeForExport
				{
					skinnedMeshRendererPath = skinnedMeshRenderer != null ? Utils.GetPathForTransform(startXform, skinnedMeshRenderer.transform) : null,
					defaultWeight = defaultWeight,
					positionWeight = positionWeight,
					index = index,
					name = name,
					isUsedInEalierConfig = isUsedInEalierConfig
				};

				return export;
			}


			public void Import(EyelidPositionBlendshapeForExport export, Transform startXform)
			{
				skinnedMeshRenderer = export.skinnedMeshRendererPath != null ? Utils.GetTransformFromPath(startXform, export.skinnedMeshRendererPath).GetComponent<SkinnedMeshRenderer>() : null;
				defaultWeight = export.defaultWeight;
				positionWeight = export.positionWeight;
				index = export.index;
				name = export.name;
				isUsedInEalierConfig = export.isUsedInEalierConfig;

				//*** If we imported a name for the blendshape, find the correct index, because during runtime we use the index to manipulate blendshapes
				{
					if ( false == string.IsNullOrEmpty(name) && skinnedMeshRenderer != null)
						for ( int i=0;  i<skinnedMeshRenderer.sharedMesh.blendShapeCount;  i++ )
							if ( skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i).Equals( name ) )
							{
								index = i;
								break;
							}
				}
			}
		}
		
	}

}