using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace VRC.SDK3.Editor
{
    /// <summary>
    /// This is the public interface you can use to interact with the Worlds SDK Builder
    /// </summary>
    public interface IVRCSdkWorldBuilderApi: IVRCSdkBuilderApi
    {
        /// <summary>
        /// Builds the currently open scene and returns a path to the built world bundle.
        /// Make sure the world has a valid SceneDescriptor and PipelineManager components present.
        /// </summary>
        /// <returns>Path to the bundle</returns>
        /// <exception cref="BuilderException">Build process has encountered an error</exception>
        /// <exception cref="BuildBlockedException">Build was blocked by the SDK Callback</exception>
        /// <exception cref="ValidationException">Content has validation errors</exception>
        Task<string> Build();
        
        /// <summary>
        /// Builds and uploads the currently open scene for the VRCWorld specified.
        /// Make sure the world has a valid SceneDescriptor and PipelineManager components present
        /// </summary>
        /// <param name="world">VRCWorld object with world info. Must have a Name for world creation</param>
        /// <param name="thumbnailPath">Path to the thumbnail image on disk. Must be specified for world creation</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="BuilderException">Build process has encountered an error</exception>
        /// <exception cref="BuildBlockedException">Build was blocked by the SDK Callback</exception>
        /// <exception cref="ValidationException">Content has validation errors</exception>
        /// <exception cref="OwnershipException">Current User does not own the target content</exception>
        /// <exception cref="UploadException">Content failed to upload</exception>
        Task BuildAndUpload(VRCWorld world, string thumbnailPath = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Builds the currently open scene and launches / reloads a test client
        /// </summary>
        /// <exception cref="BuilderException">Build process has encountered an error</exception>
        /// <exception cref="BuildBlockedException">Build was blocked by the SDK Callback</exception>
        /// <exception cref="ValidationException">Content has validation errors</exception>
        Task BuildAndTest();
        
        /// <summary>
        /// Reopens / reloads the test clients with the last built world
        /// </summary>
        /// <exception cref="BuilderException">Build process has encountered an error</exception>
        /// <exception cref="BuildBlockedException">Build was blocked by the SDK Callback</exception>
        /// <exception cref="ValidationException">Content has validation errors</exception>
        Task TestLastBuild();
        
        /// <summary>
        /// Uploads the last built world for the VRCWorld specified
        /// </summary>
        /// <param name="world">VRCWorld object with world info. Must have a Name for world creation</param>
        /// <param name="thumbnailPath">Path to the thumbnail image on disk. Must be specified for world creation</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OwnershipException">Current User does not own the target content</exception>
        /// <exception cref="UploadException">Content failed to upload</exception>
        Task UploadLastBuild(VRCWorld world, string thumbnailPath = null, CancellationToken cancellationToken = default);
    }
}