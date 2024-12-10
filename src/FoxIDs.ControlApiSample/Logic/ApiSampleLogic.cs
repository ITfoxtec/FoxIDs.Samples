using FoxIDs.ControlApiSample.Models;
using FoxIDs.ControlApiSample.ServiceAccess;
using FoxIDs.ControlApiSample.ServiceAccess.Contracts;
using System;
using System.Threading.Tasks;

namespace FoxIDs.ControlApiSample.Logic
{
    public class ApiSampleLogic
    {
        private readonly ApiSampleSettings settings;
        private readonly FoxIDsApiClient foxIDsApiClient;

        public ApiSampleLogic(ApiSampleSettings settings, FoxIDsApiClient foxIDsApiClient)
        {
            this.settings = settings;
            this.foxIDsApiClient = foxIDsApiClient;
        }

        public async Task ChangeUsersPasswordAsync()
        {
            Console.WriteLine($"Change a user's password in [tenant: '{settings.Tenant}', track (environment): '{settings.Track}']");
            Console.WriteLine("Add the user's email, current and new password");
            Console.Write("Email: ");
            var email = Console.ReadLine();
            Console.Write("Current password: ");
            var currentPassword = Console.ReadLine();
            Console.Write("New password: ");
            var newPassword = Console.ReadLine();
            Console.WriteLine(string.Empty);

            var user = await foxIDsApiClient.PutUserChangePasswordAsync(new UserChangePasswordRequest
            {
                Email = email,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            });

            Console.WriteLine(string.Empty);
            Console.WriteLine($"The user's password has been changed");
        }
    }
}
