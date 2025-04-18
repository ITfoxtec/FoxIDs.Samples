using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using Saml2Http = ITfoxtec.Identity.Saml2.Http;
using System;
using System.Collections.Generic;
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
using FoxIDs.SampleHelperLibrary.Models;
using ITfoxtec.Identity.Util;
using ITfoxtec.Identity.Saml2.Claims;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using ITfoxtec.Identity;

namespace AspNetCoreSamlIdPSample.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        private readonly Saml2ConfigurationIdP saml2Config;
        private readonly Settings settings;
        private readonly ILogger<SamlController> logger;
        private readonly IdPSessionCookieRepository idPSessionCookieRepository;

        public SamlController(ILogger<SamlController> logger, IOptionsMonitor<Settings> settingsAccessor, IOptionsMonitor<Saml2ConfigurationIdP> configAccessor, IdPSessionCookieRepository idPSessionCookieRepository)
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
                SigningCertificates =
                [
                    saml2Config.SigningCertificate
                ],
                //EncryptionCertificates =
                //[
                //    saml2Config.DecryptionCertificate
                //],
                SingleSignOnServices =
                [
                    new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = new Uri(UrlCombine.Combine(defaultSite, "/Saml/Login")) }
                ],
                SingleLogoutServices =
                [
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = new Uri(UrlCombine.Combine(defaultSite, "/Saml/Logout")) }
                ],
                NameIDFormats = [NameIdentifierFormats.X509SubjectName],
            };
            entityDescriptor.ContactPersons = [
                new ContactPerson(ContactTypes.Administrative)
                {
                    Company = "Some sample IdP",
                } 
            ];
            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        [Route("Login")]
        public async Task<IActionResult> Login()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLoginRequest(httpRequest));

            var saml2AuthnRequest = new Saml2AuthnRequest(saml2Config);            
            try
            {
                httpRequest.Binding.Unbind(httpRequest, saml2AuthnRequest);

                var session = await GetSession(relyingParty);

                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Success, httpRequest.Binding.RelayState, relyingParty, session.SessionIndex, GetClaims(session));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"SAML 2.0 Authn Request error. Authn Request '{saml2AuthnRequest.XmlDocument?.OuterXml}', Query String '{Request.QueryString}'.");
                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Responder, httpRequest.Binding.RelayState, relyingParty);
            }
        }

        [Route("IdPInitiated")]
        public IActionResult IdPInitiated()
        {
            return base.View(new IdPInitiatedViewModel { RelyingPartyIssuers = GetRelyingPartyListItems(), OnlineSample = settings.OnlineSample });
        }

        [HttpPost("IdPInitiated")]
        public async Task<IActionResult> IdPInitiated(IdPInitiatedViewModel idPInitiatedViewModel)
        {
            if ("oidc".Equals(idPInitiatedViewModel.ApplicationType, StringComparison.OrdinalIgnoreCase) && idPInitiatedViewModel.ApplicationRedirectURL.IsNullOrWhiteSpace())
            {
                ModelState.AddModelError(nameof(idPInitiatedViewModel.ApplicationRedirectURL), $"The {nameof(idPInitiatedViewModel.ApplicationRedirectURL)} field is required for OpenID Connect (oidc)");
            }

            if (!ModelState.IsValid)
            {
                idPInitiatedViewModel.RelyingPartyIssuers = GetRelyingPartyListItems();
                idPInitiatedViewModel.OnlineSample = settings.OnlineSample;
                return View(idPInitiatedViewModel);
            }

            var relyingParty = ValidateRelyingParty(idPInitiatedViewModel.RelyingPartyIssuer);

            var binding = new Saml2PostBinding();
            binding.RelayState = string.Join('&', GetRelayState(idPInitiatedViewModel));

            var response = new Saml2AuthnResponse(GetLoginSaml2Config(relyingParty));
            response.Status = Saml2StatusCodes.Success;

            var session = await GetSession(relyingParty);

            return LoginResponse(null, Saml2StatusCodes.Success, binding.RelayState, relyingParty, session.SessionIndex, GetClaims(session));


            //var claimsIdentity = new ClaimsIdentity(GetClaims(session));
            //response.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
            //response.ClaimsIdentity = claimsIdentity;
            //var token = response.CreateSecurityToken(relyingParty.Issuer);

            //return binding.Bind(response).ToActionResult();
        }

        private IEnumerable<string> GetRelayState(IdPInitiatedViewModel idPInitiatedViewModel)
        {
            yield return $"app_name={idPInitiatedViewModel.ApplicationName.ToLower()}";
            yield return $"app_type={idPInitiatedViewModel.ApplicationType.ToLower()}";
            if (!idPInitiatedViewModel.ApplicationRedirectURL.IsNullOrWhiteSpace())
            {
                yield return $"app_redirect={HttpUtility.UrlEncode(idPInitiatedViewModel.ApplicationRedirectURL)}";
            }
        }

        private async Task<IdPSession> GetSession(RelyingParty relyingParty)
        {
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
                    CustomId = "123abc",
                    CustomName = "Test Users Custom Full Name",
                    SessionIndex = Guid.NewGuid().ToString()
                };
                await idPSessionCookieRepository.SaveAsync(session);
            }

            return session;
        }

        private IEnumerable<Claim> GetClaims(IdPSession idPSession)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, idPSession.NameIdentifier);
            yield return new Claim(ClaimTypes.Upn, idPSession.Upn);
            yield return new Claim(ClaimTypes.Email, idPSession.Email);

            yield return new Claim("http://schemas.test.org/claims/CustomID", idPSession.CustomId);
            yield return new Claim("http://schemas.test.org/claims/customname", idPSession.CustomName);

            yield return new Claim("space claim test", "Test value");

            yield return new Claim(Saml2ClaimTypes.NameId, idPSession.NameIdentifier);
            yield return new Claim(Saml2ClaimTypes.SessionIndex, idPSession.SessionIndex);
        }

        private IEnumerable<SelectListItem> GetRelyingPartyListItems() => GetRelyingParties().Select(r => new SelectListItem(r.SingleSignOnDestination.OriginalString, r.Issuer));

        private IActionResult LoginResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, RelyingParty relyingParty, string sessionIndex = null, IEnumerable<Claim> claims = null)
        {
            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = relayState;

            var saml2AuthnResponse = new Saml2AuthnResponse(GetLoginSaml2Config(relyingParty))
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

        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var httpRequest = Request.ToGenericHttpRequest(validate: true);
            if (httpRequest.Binding.IsRequest(httpRequest))
            {
                return await LogoutInternal(httpRequest);
            }
            else
            {
                return SingleLogoutResponseInternal(httpRequest);
            }
        }

        private async Task<IActionResult> LogoutInternal(Saml2Http.HttpRequest httpRequest)
        {
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLogoutRequest(httpRequest));

            var saml2LogoutRequest = new Saml2LogoutRequest(saml2Config);
            saml2LogoutRequest.SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate };
            try
            {
                httpRequest.Binding.Unbind(httpRequest, saml2LogoutRequest);

                await idPSessionCookieRepository.DeleteAsync();

                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Success, httpRequest.Binding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"SAML 2.0 Logout Request error. Logout Request '{saml2LogoutRequest.XmlDocument?.OuterXml}'.");
                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Responder, httpRequest.Binding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
        }

        private IActionResult SingleLogoutResponseInternal(Saml2Http.HttpRequest httpRequest)
        {
            var relyingParty = ValidateRelyingParty(ReadRelyingPartyFromLogoutResponse(httpRequest));

            var saml2LogoutResponse = new Saml2LogoutResponse(saml2Config);
            saml2LogoutResponse.SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate };
            httpRequest.Binding.Unbind(httpRequest, saml2LogoutResponse);

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SingleLogout()
        {
            var session = await idPSessionCookieRepository.GetAsync();
            var relyingParty = ValidateRelyingParty(session.RelyingPartyIssuer);

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = session == null ? new Saml2LogoutRequest(saml2Config) : new Saml2LogoutRequest(saml2Config, new ClaimsPrincipal(new ClaimsIdentity(GetClaims(session), "auth_session", ClaimTypes.NameIdentifier, ClaimTypes.Role)));
            saml2LogoutRequest.Destination = relyingParty.SingleLogoutDestination;

            await idPSessionCookieRepository.DeleteAsync();         

            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        private string ReadRelyingPartyFromLoginRequest(Saml2Http.HttpRequest httpRequest)
        {
            return httpRequest.Binding.ReadSamlRequest(httpRequest, new Saml2AuthnRequest(saml2Config))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutRequest(Saml2Http.HttpRequest httpRequest)
        {
            return httpRequest.Binding.ReadSamlRequest(httpRequest, new Saml2LogoutRequest(saml2Config))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutResponse(Saml2Http.HttpRequest httpRequest)
        {
            return httpRequest.Binding.ReadSamlResponse(httpRequest, new Saml2LogoutResponse(saml2Config))?.Issuer;
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

        private Saml2Configuration GetLoginSaml2Config(RelyingParty relyingParty)
        {
            var loginSaml2Config = new Saml2Configuration
            {
                Issuer = saml2Config.Issuer,
                SigningCertificate = saml2Config.SigningCertificate,
                SignatureAlgorithm = saml2Config.SignatureAlgorithm,
                CertificateValidationMode = saml2Config.CertificateValidationMode,
                RevocationMode = saml2Config.RevocationMode
            };
            loginSaml2Config.AllowedAudienceUris.AddRange(saml2Config.AllowedAudienceUris);
            loginSaml2Config.EncryptionCertificate = relyingParty.EncryptionCertificate;

            return loginSaml2Config;
        }

        private RelyingParty ValidateRelyingParty(string issuer)
        {
            try
            {
                return GetRelyingParties().Where(rp => rp.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase)).Single();

            }
            catch (Exception ex)
            {
                throw new Exception($"Requested RP Issuer '{issuer}' is not configured in settings.RelyingParties.");
            }
        }

        int metadataValidInSecunds = 10*60*60;
        private List<RelyingParty> GetRelyingParties()
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var rp in settings.RelyingParties)
            {
                if (rp.ValidUntil < now)
                {
                    var entityDescriptor = new EntityDescriptor();
                    entityDescriptor.ReadSPSsoDescriptorFromUrl(new Uri(rp.SpMetadata));
                    if (entityDescriptor.SPSsoDescriptor != null)
                    {
                        rp.Issuer = entityDescriptor.EntityId;
                        rp.SingleSignOnDestination = entityDescriptor.SPSsoDescriptor.AssertionConsumerServices.First().Location;

                        var singleLogoutService = entityDescriptor.SPSsoDescriptor.SingleLogoutServices.First();
                        rp.SingleLogoutDestination = singleLogoutService.Location;
                        rp.SingleLogoutResponseDestination = singleLogoutService.ResponseLocation ?? singleLogoutService.Location;

                        rp.SignatureValidationCertificate = entityDescriptor.SPSsoDescriptor.SigningCertificates.First();

                        if (entityDescriptor.SPSsoDescriptor.EncryptionCertificates?.Count() > 0)
                        {
                            rp.EncryptionCertificate = entityDescriptor.SPSsoDescriptor.EncryptionCertificates.First();
                        }
                    }
                    else
                    {
                        throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                    }

                    rp.ValidUntil = now.AddSeconds(metadataValidInSecunds);
                }
            }

            return settings.RelyingParties;
        }
    }
}