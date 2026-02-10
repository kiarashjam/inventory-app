namespace InventoryPro.Application.Dto.Common;

public class ServiceResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public static ServiceResponseDto Ok(string message = "Success") => new() { Success = true, Message = message };
    public static ServiceResponseDto Fail(string message) => new() { Success = false, Message = message };
}

public class ServiceResponseDto<T> : ServiceResponseDto
{
    public T? Data { get; set; }
    public static ServiceResponseDto<T> Ok(T data, string message = "Success") => new() { Success = true, Message = message, Data = data };
    public new static ServiceResponseDto<T> Fail(string message) => new() { Success = false, Message = message };
}
