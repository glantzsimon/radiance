namespace K9.WebApplication.Services
{
    public interface IMailChimpService
    {
        string Call(string method, string requestJson);
    }
}