using InventorySystem.Application.DTO.Request.Identity;
using InventorySystem.Application.DTO.Response;
using InventorySystem.Application.DTO.Response.Identity;
using InventorySystem.Application.Interface.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.Application.Service;
public class AccountService(IAccount account) : IAccountService
{
    public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model)
        => await account.CreateUserAsync(model);

    public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync()
        => await account.GetUsersWithClaimsAsync();

    public async Task<ServiceResponse> LogInAsync(LoginUserRequestDTO model)
        => await account.LogInAsync(model);

    public async Task SetUpAsync()
        => await account.SetUpAsync();

    public Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model)
        => account.UpdateUserAsync(model);

    //private async Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync()
    //    => await account.GetActivitiesAsync();

    //public Task SaveActivityAsync(ActivityTrackerRequestDTO model)
    //    => account.SaveActivityAsync(model);

    //public async Task<IEnumerable<IGrouping<DateTime, ActivityTrackerResponseDTO>>> ActivityTrackerResponseDTOs()
    //{
    //    var data = (await GetActivitiesAsync()).GroupBy(x => x.Date).AsEnumerable();
    //    return data;
    //}
}
