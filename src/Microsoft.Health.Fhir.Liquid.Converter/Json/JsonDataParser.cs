using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Json.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonDataParser
    {
        public JsonData Parse(string jsonString)
        {
            var result = new JsonData(jsonString);
            try
            {
                if (string.IsNullOrEmpty(jsonString))
                {
                    throw new DataParseException(FhirConverterErrorCode.NullOrEmptyInput, Resources.NullOrEmptyInput);
                }

                IDictionary<string, object> json =
                    JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonString, new DictionaryConverter());
                Hash jsonHash = Hash.FromDictionary(json);
                result.Data = jsonHash;
            }
            catch (Exception ex)
            {
                throw new DataParseException(FhirConverterErrorCode.InputParsingError, string.Format(Resources.InputParsingError, ex.Message), ex);
            }

            return result;
        }
    }
}