using System;
using System.Collections.Generic;
using DiffBlit.Core.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiffBlit.Core.Utilities
{
    public class ActionJsonConverter : JsonConverter
    {
        /// <summary>
        /// Supported action types with their names and associated default implementations.
        /// </summary>
        private readonly Dictionary<ActionType, IAction> _actions = new Dictionary<ActionType, IAction>
        {
            [ActionType.Add] = new AddAction(),
            [ActionType.Remove] = new RemoveAction(),
            [ActionType.Move] = new MoveAction(),
            [ActionType.Copy] = new CopyAction(),
            [ActionType.Patch] = new PatchAction()
            // TODO: prepend, insert, append, replace, regex, command, etc.
        };

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            if (!Enum.TryParse(json["Type"].Value<string>(), true, out ActionType actionType))
            {
                throw new NotSupportedException("Invalid action type.");
            }

            IAction action = _actions[actionType];
            serializer.Populate(json.CreateReader(), action);
            return action;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();

            // won't honor json attributes, there has to be a better way...

            //JObject jo = new JObject();
            //Type type = value.GetType();
            //jo.Add("Type", _actions.FirstOrDefault(x => x.Value.GetType() == type).Key);    // TODO: simplify with bidirectional dictionary

            //foreach (PropertyInfo prop in type.GetProperties())
            //{
            //    if (!prop.CanRead) continue;

            //    object propVal = prop.GetValue(value, null);
            //    if (propVal != null)
            //    {
            //        jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
            //    }
            //}
            //jo.WriteTo(writer);
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAction);
        }
    }
}
