
namespace Rokid.UXR.Interaction {
using UnityEngine;
	
	public interface ISelectSurface
	{
	    /// <summary>
	    /// A transform for the surface
	    /// </summary>
	    Transform Transform { get; }
	
	    /// <summary>
	    /// Raycast to the surface with an optional maximum distance value
	    /// </summary>
	    /// <param name="ray">The ray to cast</param>
	    /// <param name="hit">The returned hit data</param>
	    /// <param name="maxDistance">If greater than zero, maximum distance of raycast</param>
	    /// <returns>true if surface was hit</returns>
	    bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0);
	
	    /// <summary>
	    /// Find nearest point to surface
	    /// </summary>
	    /// <param name="ray">Point to check</param>
	    /// <param name="hit">The returned hit data</param>
	    /// <param name="maxDistance">If greater than zero, maximum distance of check</param>
	    /// <returns>true if nearest point was found</returns>
	    bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0);
	}
}
