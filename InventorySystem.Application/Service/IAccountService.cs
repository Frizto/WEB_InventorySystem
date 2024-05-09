using InventorySystem.Application.DTO.Request.Identity;
using InventorySystem.Application.DTO.Response;
using InventorySystem.Application.DTO.Response.Identity;

namespace InventorySystem.Application.Service;
public interface IAccountService
{
    Task<ServiceResponse> LogInAsync(LoginUserRequestDTO model);
    Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model);
    Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync();
    Task SetUpAsync();
    Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model);

    //Task SaveActivityAsync(ActivityTrackerRequestDTO model);
    //Task<IEnumerable<IGrouping<DateTime, ActivityTrackerResponseDTO>>> GroupActivities();
}
