//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

#if XR_MGMT_GTE_320

namespace Rokid.XR.Core.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.XR.Management.Metadata;
    using UnityEngine;

    /// <summary>
    /// XR Metadata for XR Plugin.
    /// Required by XR Management package.
    /// </summary>
    public class UXRCorePackage : IXRPackage
    {
        /// <summary>
        /// Package metadata instance.
        /// </summary>
        public IXRPackageMetadata metadata => new PackageMetadata();

        /// <summary>
        /// Populates package settings instance.
        /// </summary>
        ///
        /// <param name="obj">
        /// Settings object.
        /// </param>
        /// <returns>Settings analysis result. Given that nothing is done, returns true.</returns>
        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            return true;
        }

        private class LoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName => "Rokid XR Core";

            public string loaderType => typeof(Rokid.XR.Core.XRLoader).FullName;

            public List<BuildTargetGroup> supportedBuildTargets => new List<BuildTargetGroup>()
            {
                BuildTargetGroup.Android,
               
            };
        }

        private class PackageMetadata : IXRPackageMetadata
        {
            public string packageName => "Rokid XR Core";

            public string packageId => "com.rokid.xr.core";

            public string settingsType => typeof(Rokid.XR.Core.XRSettings).FullName;

            public List<IXRLoaderMetadata> loaderMetadata => new List<IXRLoaderMetadata>()
            {
                new LoaderMetadata()
            };
        }
    }
}

#endif // XR_MGMT_GTE_320
