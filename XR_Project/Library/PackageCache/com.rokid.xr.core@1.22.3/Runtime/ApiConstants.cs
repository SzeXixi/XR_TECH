//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

namespace Rokid.XR.Core
{
    internal static class ApiConstants
    {
#if UNITY_ANDROID
        public const string UXR_GFX_PLUGIN = "GfxPluginRokidXRLoader";
#else
        public const string UXR_GFX_PLUGIN = "NOT_AVAILABLE";
#endif
    }
}
