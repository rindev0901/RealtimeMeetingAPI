namespace RealtimeMeetingAPI.Helpers
{
    public class Response<T>
    {
        public bool Success => StatusCode >= 200 && StatusCode < 400;
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        public Response(string message, int statusCode, T? data = default)
        {
            Message = message;
            StatusCode = statusCode;
            Data = data;
        }

        public static Response<T> Result(T? data, string message, int statusCode)
        {
            return new Response<T>(message, statusCode, data);
        }
    }


}
