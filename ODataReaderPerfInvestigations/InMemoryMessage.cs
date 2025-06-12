using Microsoft.OData;

namespace ODataReaderPerfInvestigations
{
    internal class InMemoryMessage(Stream stream, bool leaveOpen = false) : IODataResponseMessage, IDisposable
    {
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private int statusCode = 200;
        public Stream Stream { get; set; } = stream;

        public IEnumerable<KeyValuePair<string, string>> Headers => headers;

        public int StatusCode 
        { 
            get => statusCode; 
            set => statusCode = value; 
        }


        public string GetHeader(string headerName)
        {
            return headers.TryGetValue(headerName, out var value) ? value : null;
        }

        public Stream GetStream()
        {
            return Stream;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            headers[headerName] = headerValue;
        }

        public void Dispose()
        {
            if (!leaveOpen)
            {
                Stream.Dispose();
            }
        }
    }
}