//---------------------------------------------------------------------------------------------------------------------------------------------------
// Copyright Microsoft Corporation, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.
//---------------------------------------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsPhoneDriver
{
    /// <summary>
    /// Converts the response to JSON
    /// </summary>
    internal class WebDriverWireProtocolJsonConverter : JsonConverter
    {
        public static Dictionary<string, object> Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new WebDriverWireProtocolJsonConverter());
        }

        public static string Serialize(Dictionary<string, object> value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Checks if the object can be converted
        /// </summary>
        /// <param name="objectType">The object to be converted</param>
        /// <returns>True if it can be converted or false if can't be</returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Process the reader to return an object from JSON
        /// </summary>
        /// <param name="reader">A JSON reader</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="existingValue">The existing value of the object</param>
        /// <param name="serializer">JSON Serializer</param>
        /// <returns>Object created from JSON</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return this.ProcessToken(reader);
        }

        /// <summary>
        /// Writes objects to JSON. Currently not implemented
        /// </summary>
        /// <param name="writer">JSON Writer Object</param>
        /// <param name="value">Value to be written</param>
        /// <param name="serializer">JSON Serializer </param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (serializer != null)
            {
                serializer.Serialize(writer, value);
            }
        }

        private object ProcessToken(JsonReader reader)
        {
            // Recursively processes a token. This is required for elements that next other elements.
            object processedObject = null;
            if (reader != null)
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    Dictionary<string, object> dictionaryValue = new Dictionary<string, object>();
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        string elementKey = reader.Value.ToString();
                        reader.Read();
                        dictionaryValue.Add(elementKey, this.ProcessToken(reader));
                    }

                    processedObject = dictionaryValue;
                }
                else if (reader.TokenType == JsonToken.StartArray)
                {
                    List<object> arrayValue = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    {
                        arrayValue.Add(this.ProcessToken(reader));
                    }

                    processedObject = arrayValue.ToArray();
                }
                else
                {
                    processedObject = reader.Value;
                }
            }

            return processedObject;
        }
    }
}
