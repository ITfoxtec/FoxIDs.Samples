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
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FoxIDs.SampleHelperLibrary.Repository;
using ITfoxtec.Identity;
using FoxIDs.SampleHelperLibrary.Models;
using UrlCombineLib;

namespace AspNetCoreSamlIdPSample.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration saml2Config;
        private readonly Settings settings;
        private readonly ILogger<SamlController> logger;
        private readonly IdPSessionCookieRepository idPSessionCookieRepository;

        public SamlController(ILogger<SamlController> logger, IOptionsMonitor<Settings> settingsAccessor, IOptionsMonitor<Saml2Configuration> configAccessor, IdPSessionCookieRepository idPSessionCookieRepository)
        {
            saml2Config = configAccessor.CurrentValue;
            settings = settingsAccessor.CurrentValue;
            this.logger = logger;
            this.idPSessionCookieRepository = idPSessionCookieRepository;
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
                    new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = new Uri(UrlCombine.Combine(defaultSite, "/Saml/Login")) }
                },
                SingleLogoutServices = new SingleLogoutService[]
                {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri(UrlCombine.Combine(defaultSite, "/Saml/Logout")) }
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
        public async Task<IActionResult> Login()
        {
            var requestBinding = new Saml2RedirectBinding();
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLoginRequest(requestBinding));

            var saml2AuthnRequest = new Saml2AuthnRequest(saml2Config);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnRequest);

                // ****  Handle user login e.g. in GUI ****
                // Test user with session index and claims
                var session = await idPSessionCookieRepository.GetAsync();
                if (session == null)
                {
                    session = new IdPSession
                    {
                        RelyingPartyIssuer = relyingParty.Issuer,
                        NameIdentifier = "12345",
                        Upn = "12345@email.test",
                        Email = "some@email.test",
                        SessionIndex = Guid.NewGuid().ToString()
                    };
                    await idPSessionCookieRepository.SaveAsync(session);
                }
                var claims = CreateClaims(session);

                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, relyingParty, session.SessionIndex, claims);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"SAML 2.0 Authn Request error. Authn Request '{saml2AuthnRequest.XmlDocument?.OuterXml}', Query String '{Request.QueryString}'.");
                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, relyingParty);
            }
        }

        private IEnumerable<Claim> CreateClaims(IdPSession idPSession)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, idPSession.NameIdentifier);
            yield return new Claim(ClaimTypes.Upn, idPSession.Upn);
            yield return new Claim(ClaimTypes.Email, idPSession.Email);
        }

        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            if (new Saml2PostBinding().IsRequest(Request.ToGenericHttpRequest()))
            {
                return await LogoutInternal();
            }
            else
            {
                return SingleLogoutResponseInternal();
            }
        }

        private async Task<IActionResult> LogoutInternal()
        {
            var requestBinding = new Saml2PostBinding();
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLogoutRequest(requestBinding));

            var saml2LogoutRequest = new Saml2LogoutRequest(saml2Config);
            saml2LogoutRequest.SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate };
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2LogoutRequest);

                await idPSessionCookieRepository.DeleteAsync();

                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"SAML 2.0 Logout Request error. Logout Request '{saml2LogoutRequest.XmlDocument?.OuterXml}'.");
                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
        }

        private IActionResult SingleLogoutResponseInternal()
        {
            var responseBinding = new Saml2PostBinding();
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLogoutResponse(responseBinding));

            var saml2LogoutResponse = new Saml2LogoutResponse(saml2Config);
            saml2LogoutResponse.SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate };
            responseBinding.Unbind(Request.ToGenericHttpRequest(), saml2LogoutResponse);

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SingleLogout()
        {
            var session = await idPSessionCookieRepository.GetAsync();
            var relyingParty = ValidateRelyingParty(session.RelyingPartyIssuer);

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = new Saml2LogoutRequest(saml2Config, User)
            {
                Destination = relyingParty.SingleLogoutDestination
            };

            await idPSessionCookieRepository.DeleteAsync();         

            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        private string ReadRelyingPartyFromLoginRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(saml2Config))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2LogoutRequest(saml2Config))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutResponse<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlResponse(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(saml2Config))?.Issuer;
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
                //saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single());
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                _ = saml2AuthnResponse.CreateSecurityToken(relyingParty.Issuer);
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