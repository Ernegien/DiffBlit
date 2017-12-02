using System;
using System.IO;
using DiffBlit.Core.Delta;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class PatchAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const ActionType Type = ActionType.Patch;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string SourcePath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string TargetPath { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public PatchAlgorithmType Algorithm { get; set; }

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Content Content { get; } = new Content();

        /// <summary>
        /// TODO: description
        /// </summary>
        public PatchAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="algorithm"></param>
        /// <param name="content"></param>
        public PatchAction(string sourcePath, string targetPath, PatchAlgorithmType algorithm, Content content)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Algorithm = algorithm;
            Content = content;
        }

        public void Run(ActionContext context)
        {
            //if (context == null)
            //    throw new ArgumentNullException(nameof(context));

            // TODO: optional if SourcePath is absolute
            if (context?.SourceBasePath == null)
                throw new NullReferenceException("Source base path must be specified.");

            // TODO: optional if TargetPath is absolute
            if (context?.TargetBasePath == null)
                throw new NullReferenceException("Target base path must be specified.");

            // TODO: optional if Content paths are all absolute
            if (context?.ContentBasePath == null)
                throw new NullReferenceException("Content base path must be specified.");
            
            if (SourcePath == null)
                throw new NullReferenceException("Source path must be specified.");

            if (TargetPath == null)
                throw new NullReferenceException("Target path must be specified.");

            if (Content == null)
                throw new NullReferenceException("Content must be specified.");

            IPatcher patcher;
            switch (Algorithm)
            {
                case PatchAlgorithmType.BsDiff:
                    patcher = new BsDiffPatcher();
                    break;
                case PatchAlgorithmType.Fossil:
                    patcher = new FossilPatcher();
                    break;
                case PatchAlgorithmType.MsDelta:
                    patcher = new MsDeltaPatcher();
                    break;
                case PatchAlgorithmType.Octodiff:
                    patcher = new OctodiffPatcher();
                    break;
                case PatchAlgorithmType.PatchApi:
                    patcher = new PatchApiPatcher();
                    break;
                case PatchAlgorithmType.XDelta:
                    patcher = new XDeltaPatcher();
                    break;
                default:
                    throw new NotSupportedException("Invalid patch algorithm.");
            }

            // TODO: if absolute paths are specified, don't combine with context base info
            string sourcePath = Path.Combine(context.SourceBasePath, SourcePath);
            string targetPath = Path.Combine(context.TargetBasePath, TargetPath);

            string tempPatchPath = Utility.GetTempFilePath();
            string tempTargetCopyPath = Utility.GetTempFilePath();
            try
            {
                // write the patch file to a temp location
                Content.Save(Path.Combine(context.ContentBasePath, Content.Id.ToString()), tempPatchPath);

                // if source and target paths are the same, copy source to temp location to patch against
                if (SourcePath.Equals(TargetPath, StringComparison.OrdinalIgnoreCase))
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
