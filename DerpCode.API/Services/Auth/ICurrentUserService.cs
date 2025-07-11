using DerpCode.API.Constants;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services.Auth;

public interface ICurrentUserService
{
    int UserId { get; }

    string UserName { get; }

    bool EmailVerified { get; }

    bool IsInRole(string role);

    bool IsAdmin => this.IsInRole(UserRoleName.Admin);

    bool IsPremiumUser => this.IsInRole(UserRoleName.PremiumUser);

    bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true);

    void SetOverrideUser(User user);

    void ClearOverrideUser();
}