using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.Extensions.Options;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

namespace AspNetCoreSamlIdPSample.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration saml2Config;
        private readonly Settings settings;

        public SamlController(IOptionsMonitor<Settings> settingsAccessor, IOptionsMonitor<Saml2Configuration> configAccessor)
        {
            saml2Config = configAccessor.CurrentValue;
            settings = settingsAccessor.CurrentValue;
        }

        [Route("Metadata")]
        public IActionResult Metadata()
        {
            var defaultSite = $"{Request.Scheme}://{Request.Host.ToUriComponent()}/";

            var entityDescriptor = new EntityDescriptor(saml2Config);
            entityDescriptor.ValidUntil = 365;
            entityDescriptor.IdPSsoDescriptor = new IdPSsoDescriptor
            {
                SigningCertificates = new X509Certificate2[]
                {
                    saml2Config.SigningCertificate
                },
                //EncryptionCertificates = new X509Certificate2[]
                //{
                //    config.DecryptionCertificate
                //},
                SingleSignOnServices = new SingleSignOnService[]
                {
                    new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = new Uri($"{defaultSite}/Saml/Login") }
                },
                SingleLogoutServices = new SingleLogoutService[]
                {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri($"{defaultSite}/Saml/Logout") }
                },
                NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
            };
            entityDescriptor.ContactPerson = new ContactPerson(ContactTypes.Administrative)
            {
                Company = "Some sample IdP",
            };
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        [Route("Login")]
        public IActionResult Login()
        {
            var requestBinding = new Saml2RedirectBinding();
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLoginRequest(requestBinding));

            var saml2AuthnRequest = new Saml2AuthnRequest(saml2Config);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnRequest);

                // ****  Handle user login e.g. in GUI ****
                // Test user with session index and claims
                var sessionIndex = Guid.NewGuid().ToString();
                var claims = CreateTestUserClaims();

                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, relyingParty, sessionIndex, claims);
            }
            catch (Exception exc)
            {
#if DEBUG
                Debug.WriteLine($"Saml 2.0 Authn Request error: {exc.ToString()}\nSaml Auth Request: '{saml2AuthnRequest.XmlDocument?.OuterXml}'\nQuery String: {Request.QueryString}");
#endif
                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, relyingParty);
            }
        }

        private IEnumerable<Claim> CreateTestUserClaims()
        {
            yield return new Claim(ClaimTypes.NameIdentifier, "12345");
            yield return new Claim(ClaimTypes.Upn, "12345@email.test");
            yield return new Claim(ClaimTypes.Email, "some@email.test");
        }

        [Route("Logout")]
        public IActionResult Logout()
        {
            var requestBinding = new Saml2PostBinding();
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLogoutRequest(requestBinding));

            var saml2LogoutRequest = new Saml2LogoutRequest(saml2Config);
            saml2LogoutRequest.SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate };
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2LogoutRequest);

                // **** Delete user session ****

                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
            catch (Exception exc)
            {
#if DEBUG
                Debug.WriteLine($"Saml 2.0 Logout Request error: {exc.ToString()}\nSaml Logout Request: '{saml2LogoutRequest.XmlDocument?.OuterXml}'");
#endif
                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
        }

        private string ReadRelyingPartyFromLoginRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(saml2Config))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2LogoutRequest(saml2Config))?.Issuer;
        }

        private IActionResult LoginResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, RelyingParty relyingParty, string sessionIndex = null, IEnumerable<Claim> claims = null)
        {
            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = relayState;

            var saml2AuthnResponse = new Saml2AuthnResponse(saml2Config)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = relyingParty.SingleSignOnDestination,
            };
            if (status == Saml2StatusCodes.Success && claims != null)
            {
                saml2AuthnResponse.SessionIndex = sessionIndex;

                var claimsIdentity = new ClaimsIdentity(claims);
                saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                var token = saml2AuthnResponse.CreateSecurityToken(relyingParty.Issuer);
            }

            return responsebinding.Bind(saml2AuthnResponse).ToActionResult();
        }

        private IActionResult LogoutResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, string sessionIndex, RelyingParty relyingParty)
        {
            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = relayState;

            var saml2LogoutResponse = new Saml2LogoutResponse(saml2Config)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = relyingParty.SingleLogoutResponseDestination,
                SessionIndex = sessionIndex
            };

            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }

        private RelyingParty ValidateRelyingParty(string issuer)
        {
            return settings.RelyingParties.Where(rp => rp.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase)).Single();
        }
    }
}