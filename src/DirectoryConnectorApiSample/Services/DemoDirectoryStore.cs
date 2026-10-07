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
            DirectoryUserId = "dir-user-rejected",
            Email = "rejected@somewhere.org",
            Phone = "+4511223355",
            Username = "rejected",
            Password = "testpass3",
            RejectLogin = true,
            ShowLoginRejectionMessage = true
        },
        new()
        {
            DirectoryUserId = "dir-user-rejected-no-message",
            Email = "rejected-no-message@somewhere.org",
            Phone = "+4511223366",
            Username = "rejected-no-message",
            Password = "testpass4",
            RejectLogin = true
        },
        new()
        {
            DirectoryUserId = "dir-user-disabled",
            Email = "disabled@somewhere.org",
            Phone = "+4599990000",
            Username = "disabled",
            Password = "disabledpass1",
            Disabled = true
        },
        new()
        {
            DirectoryUserId = "dir-user-deleted",
            Email = "deleted@somewhere.org",
            Username = "deleted",
            Password = "testpass5",
            Deleted = true
        },
        new()
        {
            DirectoryUserId = "dir-user-expired",
            Email = "expired@somewhere.org",
            Username = "expired",
            Password = "testpass6",
            PasswordExpired = true
        },
        new()
        {
            DirectoryUserId = "dir-user-setup-email",
            Email = "setup@somewhere.org",
            SetPasswordEmail = true
        },
        new()
        {
            DirectoryUserId = "dir-user-setup-sms",
            Phone = "+4511223377",
            SetPasswordSms = true
        }
    ];

    public DemoDirectoryUser Find(DirectorySynchronisationRequest request) =>
        !string.IsNullOrWhiteSpace(request.DirectoryUserId) ? Find(request.DirectoryUserId) : Find((DirectoryUserIdentifierRequest)request);

    public DemoDirectoryUser Find(string directoryUserId)
    {
        lock (syncRoot)
        {
            return users.FirstOrDefault(user => string.Equals(user.DirectoryUserId, directoryUserId, StringComparison.Ordinal));
        }
    }

    public DemoDirectoryUser Find(DirectoryUserIdentifierRequest request)
    {
        lock (syncRoot)
        {
            return users.FirstOrDefault(u =>
                (!string.IsNullOrWhiteSpace(request.Email) && string.Equals(u.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(request.Phone) && string.Equals(u.Phone, request.Phone.Trim(), StringComparison.Ordinal)) ||
                (!string.IsNullOrWhiteSpace(request.Username) && string.Equals(u.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase)));
        }
    }

    public bool ValidatePassword(DemoDirectoryUser user, string password)
    {
        lock (syncRoot)
        {
            return string.Equals(user.Password, password, StringComparison.Ordinal);
        }
    }

    public void SetPassword(DemoDirectoryUser user, string password)
    {
        lock (syncRoot)
        {
            user.Password = password;
            user.PasswordExpired = false;
            user.ChangePassword = false;
            user.SetPasswordEmail = false;
            user.SetPasswordSms = false;
            user.PasswordLastChanged = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    public DemoDirectoryUser Create(DirectoryCreateUserRequest request)
    {
        lock (syncRoot)
        {
            if (Find(request) != null)
            {
                return null;
            }
            var user = new DemoDirectoryUser
            {
                DirectoryUserId = $"dir-user-{Guid.NewGuid():N}",
                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),
                Username = request.Username?.Trim(),
                Password = request.Password,
                PasswordLastChanged = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ConfirmAccount = request.ConfirmAccount,
                RequireMultiFactor = request.RequireMultiFactor,
                Claims = request.Claims?.ToList() ?? []
            };
            users.Add(user);
            return user;
        }
    }
}
