using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreSamlSample.Identity;
using Microsoft.Extensions.Options;
using System.Security.Authentication;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using AspNetCoreSamlSample.Models;
using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreSamlSample.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Settings settings;
        private readonly Saml2Configuration saml2Config;

        public SamlController(IOptionsMonitor<Settings> Settings, IOptionsMonitor<Saml2Configuration> configAccessor)
        {
            settings = Settings.CurrentValue;
            saml2Config = configAccessor.CurrentValue;
        }

        private string DefaultSite => $"{Request.Scheme}://{Request.Host.ToUriComponent()}";

        [Route("Metadata")]
        public IActionResult Metadata()
        {
            var entityDescriptor = new EntityDescriptor(saml2Config);
            entityDescriptor.ValidUntil = 365;
            entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor
            {
                WantAssertionsSigned = true,
                SigningCertificates =
                [
                    saml2Config.SigningCertificate
                ],
                //EncryptionCertificates =
                //[
                //    saml2Config.DecryptionCertificate
                //],
                SingleLogoutServices =
                [
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri($"{DefaultSite}/Saml/SingleLogout"), ResponseLocation = new Uri($"{DefaultSite}/Saml/LoggedOut") }
                ],
                NameIDFormats = [NameIdentifierFormats.X509SubjectName],
                AssertionConsumerServices =
                [
                    new AssertionConsumerService {  Binding = ProtocolBindings.HttpPost, Location = new Uri($"{DefaultSite}/Saml/AssertionConsumerService") }
                ],
                AttributeConsumingServices =
                [
                    new AttributeConsumingService { ServiceNames = [new LocalizedNameType("Some SP", "en")], RequestedAttributes = CreateRequestedAttributes() }
                ],
            };
            entityDescriptor.ContactPerson = new ContactPerson(ContactTypes.Administrative)
            {
                Company = "Some Company",
                GivenName = "Some Given Name",
                SurName = "Some Sur Name",
                EmailAddress = "some@somedomain.com",
                TelephoneNumber = "11111111",
            };
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        private IEnumerable<RequestedAttribute> CreateRequestedAttributes()
        {
            yield return new RequestedAttribute("urn:oid:2.5.4.4");
            yield return new RequestedAttribute("urn:oid:2.5.4.3", false);
        }


        [Route("Login")]
        public IActionResult Login(string returnUrl = null, LoginType? loginType = null)
        {
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string>
            {
                { relayStateReturnUrl, returnUrl ?? Url.Content("~/") },
            });

            var saml2AuthnRequest = new Saml2AuthnRequest(saml2Config)
            {
                AssertionConsumerServiceUrl= new Uri($"{DefaultSite}/Saml/AssertionConsumerService"),
                
                //ForceAuthn = true,
                //NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent" },

                // To require MFA
                //RequestedAuthnContext = new RequestedAuthnContext
                //{
                //    Comparison = AuthnContextComparisonTypes.Exact,
                //    AuthnContextClassRef = new string[] { "urn:foxids:mfa" },
                //}

                //RequestedAuthnContext = new RequestedAuthnContext
                //{
                //    Comparison = AuthnContextComparisonTypes.Exact,
                //    AuthnContextClassRef = new string[] { "urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport" },
                //}
            };

            saml2AuthnRequest.Destination = AddUpParty(saml2AuthnRequest.Destination, loginType.HasValue ? loginType.Value : LoginType.FoxIDsLogin);

            return binding.Bind(saml2AuthnRequest).ToActionResult();
        }

        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            var saml2AuthnResponse = new Saml2AuthnResponse(saml2Config);

            httpRequest.Binding.ReadSamlResponse(httpRequest, saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
            }
            httpRequest.Binding.Unbind(httpRequest, saml2AuthnResponse);

            await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: ClaimsTransform.Transform);

            var relayStateQuery = httpRequest.Binding.GetRelayStateQuery();
            var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");
            return Redirect(returnUrl);
        }

        [Route("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = new Saml2LogoutRequest(saml2Config, User);

            await saml2LogoutRequest.DeleteSession(HttpContext);

            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [Route("LoggedOut")]
        public IActionResult LoggedOut()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            httpRequest.Binding.Unbind(httpRequest, new Saml2LogoutResponse(saml2Config));

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        public async Task<IActionResult> SingleLogout()
        {
            Saml2StatusCodes status;
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            var logoutRequest = new Saml2LogoutRequest(saml2Config, User);
            try
            {
                httpRequest.Binding.Unbind(httpRequest, logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                Debug.WriteLine("SingleLogout error: " + exc.ToString());
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = httpRequest.Binding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(saml2Config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }

        private Uri AddUpParty(Uri destination, LoginType loginType)
        {         
            var upParty = GetUpParty(loginType);
            return new Uri(destination.OriginalString.Replace($"/{settings.DownParty}(*)/", $"/{settings.DownParty}({upParty})/"));
        }

        private string GetUpParty(LoginType loginType)
        {
            switch (loginType)
            {
                case LoginType.FoxIDsLogin:
                    return settings.FoxIDsLoginUpParty;
                case LoginType.ParallelFoxIDs:
                    return settings.ParallelFoxIDsUpParty;
                case LoginType.IdentityServer:
                    return settings.IdentityServerUpParty;
                case LoginType.SamlIdPSample:
                    return settings.SamlIdPSampleUpParty;
                case LoginType.SamlAdfs:
                    return settings.SamlIdPAdfsUpParty;
                default:
                    throw new NotImplementedException("LoginType not implemented.");
            }
        }
    }
}
