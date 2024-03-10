using UnityEngine;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Event Interface
    /// </summary>
    public interface IEventInput
    {
        /// <summary>
        /// Init event module
        /// </summary>
        /// <param name="parent">Initialize the parent of the interactor</param>
        public void Initialize(Transform parent);

        /// <summary>
        /// Release event module
        /// </summary>
        public void Release();

        /// <summary>
        /// Activate the module (only if the module has already been initialized to activate successfully)
        /// </summary>
        public void ActiveModule();

        /// <summary>
        /// Generated interactor
        /// </summary>
        /// <value></value>
        public Transform Interactor { get; set; }
    }
}

