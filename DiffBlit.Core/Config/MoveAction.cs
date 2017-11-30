﻿using System.IO;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class MoveAction : IAction
    {
        /// <summary>
        /// The type name used to aid in json deserialization.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        private const ActionType Type = ActionType.Move;

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
        public MoveAction()
        {
            
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public MoveAction(string sourcePath, string targetPath)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        public void Run()
        {
            File.Move(SourcePath, TargetPath);
        }
    }
}
