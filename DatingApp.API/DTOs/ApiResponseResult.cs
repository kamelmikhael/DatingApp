namespace DatingApp.API.DTOs
{
    public class ApiResponseResult<T>
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        public ApiResponseResult(T data)
        {
            Data = data;
            IsSuccess = true;
            ErrorMessage = null;
        }

        public ApiResponseResult(string error)
        {
            IsSuccess = false;
            ErrorMessage = error;
        }
    }
}