namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface ICommonPasswordBlocklist
{
    bool IsCommon(string password);
}
