using DotLiquid;
using DotLiquid.FileSystems;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Models;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonTemplateProvider : ITemplateProvider
    {
        private readonly IFhirConverterTemplateFileSystem _fileSystem;

        public JsonTemplateProvider(string templateDirectory)
        {
            _fileSystem = new TemplateLocalFileSystem(templateDirectory, DataType.Json);
        }

        public Template GetTemplate(string templateName)
        {
            return _fileSystem.GetTemplate(templateName);
        }

        public ITemplateFileSystem GetTemplateFileSystem()
        {
            return _fileSystem;
        }
    }
}