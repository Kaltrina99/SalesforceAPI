using Microsoft.Extensions.Options;
using Salesforce.Common;
using Salesforce.Force;

namespace SalesforceAPI
{
    public class SalesforceService
    {
        private readonly SalesforceConfiguration _salesforceConfig;

        public SalesforceService(IOptions<SalesforceConfiguration> salesforceConfig)
        {
            _salesforceConfig = salesforceConfig.Value;
        }

        public async Task CreateCustomObjectsAsync(string objectName, IEnumerable<CustomObject> customObjects)
        {
            var auth = new AuthenticationClient();

            await auth.UsernamePasswordAsync(_salesforceConfig.ClientId, _salesforceConfig.ClientSecret, _salesforceConfig.Username, _salesforceConfig.Password + _salesforceConfig.SecurityToken);

            var client = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

            // Example: Create objects in Salesforce using customObjects and the ForceClient
            foreach (var customObject in customObjects)
            {
                var properties = new Dictionary<string, object>();

                foreach (var propertyInfo in customObject.GetType().GetProperties())
                {
                    var value = propertyInfo.GetValue(customObject);
                    properties[propertyInfo.Name] = value;
                }

                await client.CreateAsync(objectName, properties);
            }
        }
    }
}
