// EyeRotationLimiter.cs
// Tore Knabe
// Copyright 2020 tore.knabe@gmail.com

using System;
using UnityEngine;

namespace RealisticEyeMovements
{
	[Serializable]
	public class EyeRotationLimiter
	{

		[Serializable]
		public class EyeRotationLimiterForExport
		{
			public string transformPath;
			public SerializableQuaternion defaultQ;
			public SerializableQuaternion lookUpQ;
			public SerializableQuaternion lookDownQ;

			public bool isLookUpSet;
			public bool isLookDownSet;
		}

		#region fields

			public Transform transform;
			[SerializeField] Quaternion defaultQ;
			[SerializeField] Quaternion lookUpQ;
			[SerializeField] Quaternion lookDownQ;

			public float maxUpAngle;
			public float maxDownAngle;

			[SerializeField] bool isLookUpSet;
			[SerializeField] bool isLookDownSet;

		#endregion


		public bool CanImport(EyeRotationLimiterForExport import, Transform startXform, string targetNameForErrorMessage=null)
		{
			return Utils.CanGetTransformFromPath(startXform, import.transformPath, targetNameForErrorMessage);
		}

		public float ClampAngle( float angle )
		{
			return Mathf.Clamp( Utils.NormalizedDegAngle(angle), -maxUpAngle, maxDownAngle );
		}



		public EyeRotationLimiterForExport GetExport(Transform startXform)
		{
			EyeRotationLimiterForExport export = new EyeRotationLimiterForExport
			{
				transformPath = Utils.GetPathForTransform(startXform, transform),
				defaultQ = defaultQ,
				lookUpQ = lookUpQ,
				lookDownQ = lookDownQ,
				isLookUpSet = isLookUpSet,
				isLookDownSet = isLookDownSet
			};

			return export;
		}


		public float GetEyeUp01( float angle )
		{
			return angle >= 0 ? 0 : Mathf.InverseLerp(0, maxUpAngle, -angle);
		}


		public float GetEyeDown01( float angle )
		{
			return angle <= 0 ? 0 : Mathf.InverseLerp(0, maxDownAngle, angle);
		}


		public void Import( EyeRotationLimiterForExport import, Transform targetXform )
		{
			transform = targetXform;
			defaultQ = import.defaultQ;
			lookUpQ = import.lookUpQ;
			lookDownQ = import.lookDownQ;
			isLookUpSet = import.isLookUpSet;
			isLookDownSet = import.isLookDownSet;

			UpdateMaxAngles();
		}



		public void RestoreDefault()
		{
			transform.localRotation = defaultQ;
		}


		public void RestoreLookDown()
		{
			transform.localRotation = lookDownQ;
		}


		public void RestoreLookUp()
		{
			transform.localRotation = lookUpQ;
		}


		public void SaveDefault( Transform t )
		{
			transform = t;
			defaultQ = t.localRotation;
			if (false == isLookDownSet)
				lookDownQ = defaultQ * Quaternion.Euler(20, 0, 0);
			if (false == isLookUpSet)
				lookUpQ = defaultQ * Quaternion.Euler(-8, 0, 0);
			UpdateMaxAngles();
		}


		public void SaveLookDown()
		{
			lookDownQ = transform.localRotation;
			UpdateMaxAngles();
			isLookDownSet = true;
		}

		
		public void SaveLookUp()
		{
			lookUpQ = transform.localRotation;
			UpdateMaxAngles();
			isLookUpSet = true;
		}
		

		void UpdateMaxAngles()
		{
			Vector3 lookUpEulerInDefaultSpace = (Quaternion.Inverse( defaultQ ) * lookUpQ).eulerAngles;
			maxUpAngle = Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.x)),
													Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.y)),
																		Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.z))));

			Vector3 lookDownEulerInDefaultSpace = (Quaternion.Inverse( defaultQ ) * lookDownQ).eulerAngles;
			maxDownAngle = Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.x)),
													Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.y)),
																		Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.z))));
		}
	}

}