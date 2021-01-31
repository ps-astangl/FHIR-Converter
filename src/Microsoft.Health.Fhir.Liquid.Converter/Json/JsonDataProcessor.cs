using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotLiquid;
using Microsoft.Health.Fhir.Liquid.Converter.Exceptions;
using Microsoft.Health.Fhir.Liquid.Converter.Hl7v2.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Json.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.OutputProcessor;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Json
{
    public class JsonDataProcessor : IFhirConverter
    {
        private readonly JsonDataParser _dataParser = new JsonDataParser();
        private readonly ProcessorSettings _settings;

        public JsonDataProcessor(ProcessorSettings processorSettings = null)
        {
            _settings = processorSettings;
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, TraceInfo traceInfo = null)
        {
            if (string.IsNullOrEmpty(rootTemplate))
            {
                throw new RenderException(FhirConverterErrorCode.NullOrEmptyRootTemplate, Resources.NullOrEmptyRootTemplate);
            }

            if (templateProvider == null)
            {
                throw new RenderException(FhirConverterErrorCode.NullTemplateProvider, Resources.NullTemplateProvider);
            }

            var template = templateProvider.GetTemplate(rootTemplate);
            if (template == null)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateNotFound, string.Format(Resources.TemplateNotFound, rootTemplate));
            }

            var jsonData = _dataParser.Parse(data);

            // TODO: Unsure why a context is needed. It's possible to simply parse and render directly from this point.
            var context = CreateContext(templateProvider, jsonData);
            var foo = RenderTemplates(template, context);
            var rawResult = template.Render(jsonData.Data);
            var result = PostProcessor.Process(rawResult);
            if (traceInfo is JsonTraceInfo jsonTraceInfo)
            {
            }

            return result.ToString(Formatting.Indented);
        }

        public string Convert(string data, string rootTemplate, ITemplateProvider templateProvider, CancellationToken cancellationToken, TraceInfo traceInfo = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Convert(data, rootTemplate, templateProvider, traceInfo);
        }

        private Context CreateContext(ITemplateProvider templateProvider, JsonData jsonData)
        {
            // Load data and templates
            var timeout = _settings?.TimeOut ?? 0;
            var context = new Context(
                environments: new List<Hash> { jsonData.Data },
                outerScope: new Hash(),
                registers: Hash.FromDictionary(new Dictionary<string, object> { { "file_system", templateProvider.GetTemplateFileSystem() } }),
                errorsOutputMode: ErrorsOutputMode.Rethrow,
                maxIterations: 0,
                timeout: timeout,
                formatProvider: CultureInfo.InvariantCulture);

            // Load code system mapping
            var codeSystemMapping = templateProvider.GetTemplate("CodeSystem/CodeSystem");
            if (codeSystemMapping?.Root?.NodeList?.First() != null)
            {
                context["CodeSystemMapping"] = codeSystemMapping.Root.NodeList.First();
            }

            // Load filters
            context.AddFilters(typeof(Filters));

            return context;
        }

        private string RenderTemplates(Template template, Context context)
        {
            try
            {
                template.MakeThreadSafe();
                return template.Render(RenderParameters.FromContext(context, CultureInfo.InvariantCulture));
            }
            catch (TimeoutException ex)
            {
                throw new RenderException(FhirConverterErrorCode.TimeoutError, Resources.TimeoutError, ex);
            }
            catch (RenderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RenderException(FhirConverterErrorCode.TemplateRenderingError, string.Format(Resources.TemplateRenderingError, ex.Message), ex);
            }
        }
    }
}