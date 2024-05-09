using InventorySystem.Application.DTO.Request.Identity;
using InventorySystem.Application.DTO.Response;
using InventorySystem.Application.DTO.Response.Identity;
using InventorySystem.Application.Extension.Identity;
using InventorySystem.Application.Interface.Identity;
using InventorySystem.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventorySystem.Infrastructure.Repository;
public class Account(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context) : IAccount
{
    // Create a new user using Async Method in the DB
    public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model)
    {
        var user = await FindUserByEmail(model.Email);
        if (user != null)
        {
            return new ServiceResponse(false, "User Already Exists");
        }

        var newUser = new ApplicationUser
        {
            UserName = model.Email,
            PasswordHash = model.Password,
            Email = model.Email,
            Name = model.Name,
        };

        var result = CheckResult(await userManager.CreateAsync(newUser, model.Password));
        if (!result.Flag)
        {
            return result;
        }
        else
        {
            return await CreateUserClaims(model);
        }
    }

    // Create the user claims according to the policy
    private async Task<ServiceResponse> CreateUserClaims(CreateUserRequestDTO model)
    {
        if (string.IsNullOrEmpty(model.Policy)) return new ServiceResponse(false, "Policy is required");
        Claim[] userClaims = [];

        if (model.Policy.Equals(Policy.AdminPolicy, StringComparison.OrdinalIgnoreCase))
        {
            userClaims =
                [
                    new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "true"),
                        new Claim("Read", "true"),
                        new Claim("Update", "true"),
                        new Claim("Delete", "true"),
                        new Claim("ManageUser", "true")
                ];
        }
        else if (model.Policy.Equals(Policy.ManagerPolicy, StringComparison.OrdinalIgnoreCase))
        {
            userClaims =
                [
                    new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "Manager"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "true"),
                        new Claim("Read", "true"),
                        new Claim("Update", "true"),
                        new Claim("Delete", "false"),
                        new Claim("ManageUser", "false")
                ];
        }
        else if (model.Policy.Equals(Policy.UserPolicy, StringComparison.OrdinalIgnoreCase))
        {
            userClaims =
                [
                    new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim("Name", model.Name),
                        new Claim("Create", "false"),
                        new Claim("Read", "false"),
                        new Claim("Update", "false"),
                        new Claim("Delete", "false"),
                        new Claim("ManageUser", "false")
                ];
        }

        var result = CheckResult(await userManager.AddClaimsAsync(await FindUserByEmail(model.Email), userClaims));
        if (result.Flag)
        {
            return new ServiceResponse(true, "User Created Successfully");
        }
        else
        {
            return result;
        }
    }

    public async Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model)
    {
        // Search for the user in the DB
        var user = await FindUserByEmail(model.Email);
        if (user == null)
        {
            return new ServiceResponse(false, "User Not Found");
        }

        // Verifies the user password to sign in
        var verifyPassword = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!verifyPassword.Succeeded)
        {
            return new ServiceResponse(false, "Incorrect credentials provided");
        }

        // Try to log in with the password
        var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!result.Succeeded)
        {
            return new ServiceResponse(false, "Unknown error occured while logging your account.");
        }
        else
        {
            return new ServiceResponse(true, null);
        }
    }

    // Try to find the user using the Email provided
    private async Task<ApplicationUser> FindUserByEmail(string email)
        => await userManager.FindByEmailAsync(email);

    // Try to find the user using the Id provided
    private async Task<ApplicationUser> FindUserById(string id)
        => await userManager.FindByIdAsync(id);

    // Check the result of the operation
    private static ServiceResponse CheckResult(IdentityResult result)
    {
        if (result.Succeeded) return new ServiceResponse(true, null);

        var errors = result.Errors.Select(e => e.Description);
        return new ServiceResponse(false, string.Join(Environment.NewLine, errors));
    }

    public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync()
    {
        var userLists = new List<GetUserWithClaimResponseDTO>();
        var allUsers = await userManager.Users.ToListAsync();
        if (allUsers.Count == 0) return userLists;

        foreach (var user in allUsers)
        {
            var currentUser = await userManager.FindByIdAsync(user.Id);
            var getCurrentUserClaims = await userManager.GetClaimsAsync(currentUser);
            if (getCurrentUserClaims.Any())
            {
                userLists.Add(new GetUserWithClaimResponseDTO
                {
                    UserId = user.Id,
                    Email = getCurrentUserClaims.FirstOrDefault(_ => _.Type == ClaimTypes.Email).Value,
                    RoleName = getCurrentUserClaims.FirstOrDefault(_ => _.Type == ClaimTypes.Role).Value,
                    Name = getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Name").Value,
                    ManageUser = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "ManageUser").Value),
                    Create = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Create").Value),
                    Read = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Read").Value),
                    Update = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Update").Value),
                    Delete = Convert.ToBoolean(getCurrentUserClaims.FirstOrDefault(_ => _.Type == "Delete").Value)
                });
            }
        }

        return userLists;
    }

    // Seed the master admin user
    public async Task SetUpAsync() => await CreateUserAsync(new CreateUserRequestDTO()
    {
        Name = "Master Admin",
        Email = "masteradmin@inventorysystem.com",
        Password = "MasterAdmin@123",
        Policy = Policy.AdminPolicy
    });

    public async Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null) return new ServiceResponse(false, "User Not Found");

        var oldUserClaims = await userManager.GetClaimsAsync(user);

        Claim[] newUserClaims =
            [
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, model.RoleName),
                new Claim("Name", model.Name),
                new Claim("Create", model.Create.ToString()),
                new Claim("Read", model.Read.ToString()),
                new Claim("Update", model.Update.ToString()),
                new Claim("Delete", model.Delete.ToString()),
                new Claim("ManageUser", model.ManageUser.ToString())
            ];

        var result = await userManager.RemoveClaimsAsync(user, oldUserClaims);
        var response = CheckResult(result);
        if (!response.Flag)
        {
            return new ServiceResponse(false, response.Message);
        }

        var addNewClaims = await userManager.AddClaimsAsync(user, newUserClaims);
        var outcome = CheckResult(addNewClaims);
        if (outcome.Flag)
        {
            return new ServiceResponse(true, "User Updated Successfully");
        }
        else
        {
            return outcome;
        }
    }

    //public async Task SaveActivityAsync(ActivityTrackerRequestDTO model)
    //{
    //    context.ActivityTracker.Add(model.Adapt(new Tracker()));
    //    await context.SaveChangesAsync();
    //}

    //public async Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync()
    //{
    //    var list = new List<ActivityTrackerResponseDTO>();
    //    var data = (await context.ActivityTracker.ToListAsync()).Adapt<List<ActivityTrackerResponseDTO>>();

    //    foreach (var activity in data)
    //    {
    //        activity.UserName = (await userManager.FindByIdAsync(activity.UserId)).Name;
    //        list.Add(activity);
    //    }
    //    return data;
    //}
}