using System;
using System.IO;
using DiffBlit.Core.Config;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;
using Path = DiffBlit.Core.IO.Path;

namespace DiffBlit.Core.Actions
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PatchAction : IAction
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path SourcePath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Path TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public PatchAlgorithmType Algorithm { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Content Content { get; private set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonConstructor]
        private PatchAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="algorithm"></param>
        /// <param name="content"></param>
        public PatchAction(Path sourcePath, Path targetPath, PatchAlgorithmType algorithm, Content content)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Algorithm = algorithm;
            Content = content;
        }

        public void Run(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // TODO: optional if SourcePath & TargetPath is absolute
            if (context.BasePath == null)
                throw new NullReferenceException("Base path must be specified.");

            // TODO: optional if Content paths are all absolute
            if (context.ContentBasePath == null)
                throw new NullReferenceException("Content base path must be specified.");
            
            if (SourcePath == null)
                throw new NullReferenceException("Source path must be specified.");

            if (TargetPath == null)
                throw new NullReferenceException("Target path must be specified.");

            if (Content == null)
                throw new NullReferenceException("Content must be specified.");

            // TODO: if absolute paths are specified, don't combine with context base info
            Path sourcePath = Path.Combine(context.BasePath, SourcePath);
            Path targetPath = Path.Combine(context.BasePath, TargetPath);

            string tempPatchPath = Utility.GetTempFilePath();
            string tempTargetCopyPath = Utility.GetTempFilePath();

            try
            {
                // write the patch file to a temp location
                Content.Save(Path.Combine(context.ContentBasePath, Content.Id + "\\"), tempPatchPath);

                IPatcher patcher = Utility.GetPatcher(Algorithm);

                // if source and target paths are the same, copy source to temp location to patch against
                if (SourcePath.Equals(TargetPath))
                {
                    File.Copy(sourcePath, tempTargetCopyPath);
                    patcher.Apply(tempTargetCopyPath, tempPatchPath, targetPath);
                }
                else
                {
                    patcher.Apply(sourcePath, tempPatchPath, targetPath);
                }
            }
            finally
            {
                File.Delete(tempPatchPath);
                File.Delete(tempTargetCopyPath);
            }
        }
    }
}
