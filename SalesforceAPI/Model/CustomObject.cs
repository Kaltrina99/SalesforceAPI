namespace SalesforceAPI
{
    public class CustomObject
    {
        public Dictionary<string, object> Properties { get; set; }

        public CustomObject()
        {
            Properties = new Dictionary<string, object>();
        }
    }
}
