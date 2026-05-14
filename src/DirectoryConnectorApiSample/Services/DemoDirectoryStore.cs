using DirectoryConnectorApiSample.Models.Api;

namespace DirectoryConnectorApiSample.Services;

public class DemoDirectoryStore
{
    private readonly object syncRoot = new();
    private readonly List<DemoDirectoryUser> users =
    [
        new()
        {
            DirectoryUserId = "dir-user-1",
            Email = "user1@somewhere.org",
            Phone = "+4511223344",
            Username = "user1",
            Password = "testpass1",
            EmailVerified = true,
            PhoneVerified = true,
            Claims =
            [
                new ClaimValue { Type = "name", Value = "User One" },
                new ClaimValue { Type = "role", Value = "read_access" }
            ]
        },
        new()
        {
            DirectoryUserId = "dir-user-2",
            Email = "user2@somewhere.org",
            Phone = "+4555667788",
            Username = "user2",
            Password = "testpass2",
            EmailVerified = true,
            PhoneVerified = true,
            Claims =
            [
                new ClaimValue { Type = "name", Value = "User Two" },
                new ClaimValue { Type = "role", Value = "admin_access" },
                new ClaimValue { Type = "role", Value = "read_access" },
                new ClaimValue { Type = "role", Value = "write_access" }
            ]
        },
        new()
        {
            DirectoryUserId = "dir-user-disabled",
            Email = "disabled@somewhere.org",
            Phone = "+4599990000",
            Username = "disabled",
            Password = "disabledpass1",
            Disabled = true
        }
    ];

    public DemoDirectoryUser Find(DirectoryUserIdentifierRequest request)
    {
        lock (syncRoot)
        {
            return users.FirstOrDefault(u =>
                (!string.IsNullOrWhiteSpace(request.DirectoryUserId) && u.DirectoryUserId.Equals(request.DirectoryUserId, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(request.Email) && u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(request.Phone) && u.Phone.Equals(request.Phone, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(request.Username) && u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public bool ValidatePassword(DemoDirectoryUser user, string password)
    {
        lock (syncRoot)
        {
            return user.Password.Equals(password, StringComparison.Ordinal);
        }
    }

    public void SetPassword(DemoDirectoryUser user, string password)
    {
        lock (syncRoot)
        {
            user.Password = password;
        }
    }

    public DemoDirectoryUser Create(DirectoryCreateUserRequest request)
    {
        lock (syncRoot)
        {
            var user = new DemoDirectoryUser
            {
                DirectoryUserId = $"dir-user-{Guid.NewGuid():N}",
                Email = request.Email,
                Phone = request.Phone,
                Username = request.Username,
                Password = request.Password,
                ConfirmAccount = request.ConfirmAccount,
                RequireMultiFactor = request.RequireMultiFactor,
                Claims = request.Claims?.ToList() ?? []
            };
            users.Add(user);
            return user;
        }
    }
}
