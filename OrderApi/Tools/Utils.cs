using System.Text;

namespace OrderApi.Tools
{
    public static class Utils
    {
        public static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }

        public static async Task<string> GetRawBodyAsync(this HttpResponse response, Encoding encoding = null)
        {
            //response.Body.Position = 0;

            var reader = new StreamReader(response.Body, encoding ?? Encoding.UTF8);

            var body = await reader.ReadToEndAsync().ConfigureAwait(false);

            response.Body.Position = 0;

            return body;
        }
    }
}
