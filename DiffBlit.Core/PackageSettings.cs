using System;

namespace DiffBlit.Core
{
    /// <summary>
    /// TODO: description
    /// </summary>
    public class PackageSettings : ICloneable
    {
        /// <summary>
        /// The maximum cacheable file size supported by Cloudflare's free tier.
        /// </summary>
        public const int DefaultContentPartSize = 1024 * 1024 * 512;

        /// <summary>
        /// An archive file extension supported by Cloudflare's free tier.
        /// </summary>
        public const string DefaultContentPartExtension = ".jar";

        /// <summary>
        /// TODO: description
        /// </summary>
        public const PatchAlgorithmType DefaultPatchAlgorithmType = PatchAlgorithmType.XDelta;

        /// <summary>
        /// TODO: description
        /// </summary>
        public const bool DefaultCompressionEnabled = true;

        /// <summary>
        /// TODO: description
        /// </summary>
        public int PartSize { get; set; } = DefaultContentPartSize;

        /// <summary>
        /// TODO: description
        /// </summary>
        public string PartExtension { get; set; } = DefaultContentPartExtension;

        /// <summary>
        /// TODO: description
        /// </summary>
        public PatchAlgorithmType PatchAlgorithmType { get; set; } = DefaultPatchAlgorithmType;

        /// <summary>
        /// TODO: description
        /// </summary>
        public bool CompressionEnabled { get; set; } = DefaultCompressionEnabled;

        /// <summary>
        /// TODO: description
        /// </summary>
        public PackageSettings()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="partSize"></param>
        /// <param name="partExtension"></param>
        /// <param name="patchAlgorithm"></param>
        /// <param name="compressionEnabled"></param>
        public PackageSettings(int partSize = DefaultContentPartSize, string partExtension = DefaultContentPartExtension, PatchAlgorithmType patchAlgorithm = DefaultPatchAlgorithmType, bool compressionEnabled = DefaultCompressionEnabled)
        {
            PartSize = partSize;
            PartExtension = partExtension;
            PatchAlgorithmType = patchAlgorithm;
            CompressionEnabled = compressionEnabled;
        }

        /// <summary>
        /// Performs a deep clone of the package settings object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
           return new PackageSettings(PartSize, PartExtension, PatchAlgorithmType, CompressionEnabled);
        }
    }
}
