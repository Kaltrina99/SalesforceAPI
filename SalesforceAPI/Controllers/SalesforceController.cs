using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SalesforceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesforceController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly SalesforceConfiguration _salesforceConfig;
        private readonly SalesforceService _salesforceService;

        public SalesforceController(IWebHostEnvironment environment, IOptions<SalesforceConfiguration> salesforceConfig, SalesforceService salesforceService)
        {
            _environment = environment;
            _salesforceConfig = salesforceConfig.Value;
            _salesforceService = salesforceService;
        }
        [HttpPost("uploadcsv")]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("Invalid file");
            }

            string objectName = Path.GetFileNameWithoutExtension(file.FileName); // Get the file name without extension

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.Configuration.HeaderValidated = null;
                csv.Context.Configuration.MissingFieldFound = null;
                csv.Context.Configuration.Delimiter = "\t"; // Use tab as the delimiter

                try
                {
                    var records = csv.GetRecords<dynamic>();

                    var customObjects = new List<CustomObject>();

                    foreach (var record in records)
                    {
                        if (record != null)
                        {
                            var customObject = new CustomObject();

                            var recordDictionary = (IDictionary<string, object>)record;

                            foreach (var propertyInfo in typeof(CustomObject).GetProperties())
                            {
                                if (recordDictionary.TryGetValue(propertyInfo.Name, out var value))
                                {
                                    propertyInfo.SetValue(customObject, value);
                                }
                                else
                                {
                                    propertyInfo.SetValue(customObject, null); // Set the property value to null if it's missing in the CSV record
                                }
                            }

                            customObjects.Add(customObject);
                        }
                    }

                    // Now you have the customObjects list containing dynamically mapped data with the object name
                    // Pass it to the SalesforceService to create the custom objects in Salesforce
                    await _salesforceService.CreateCustomObjectsAsync(objectName, customObjects);

                    return Ok("CSV file processed successfully");
                }
                catch (Exception ex)
                {
                    // Handle the exception and return an appropriate error message
                    return BadRequest($"Error processing CSV file: {ex.Message}");
                }
            }
        }
    }
}
