using System;
using DiffBlit.Core.Delta;
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

        /// <summary>
        /// TODO: description
        /// </summary>
        public void Run()
        {
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

            // TODO: if source and target paths are the same, patch to temp location then overwrite target

            // TODO:
            //Content.Save("", "");
            //patcher.Apply(SourcePath, "", TargetPath);

        }
    }
}
