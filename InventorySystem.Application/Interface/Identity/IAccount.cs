using InventorySystem.Application.DTO.Request.Identity;
using InventorySystem.Application.DTO.Response;
using InventorySystem.Application.DTO.Response.Identity;

namespace InventorySystem.Application.Interface.Identity;
public interface IAccount
{
    Task<ServiceResponse> LogInAsync(LoginUserRequestDTO model);
    Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model);
    Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimAsync();
    Task SetUpAsync();
    Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model);
    //Task SaveActivityAsync(ActivityTrackerRequestDTO model);
    //Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync();
}
