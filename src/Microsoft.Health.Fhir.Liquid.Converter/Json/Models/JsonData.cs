using System.Collections.Generic;
using DotLiquid;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json.Models
{
    public class JsonData : Drop
    {
        public JsonData(string value = null)
        {
            Value = value;
            Data = new Hash();
        }

        public string Value { get; set; }

        public Hash Data { get; set; }
    }
}