using AspNetCoreSamlSample.Controllers;
using AspNetCoreSamlSample.Models;
using FoxIDs.SampleHelperLibrary.Models;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace AuthenticationParameterSamples.Tests;

public class SamlAuthenticationParameterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Login_ConfiguredAuthMethod_UsesQueryAndPreservesSignedMessage(bool enabled)
    {
        using var rsa = RSA.Create(2048);
        using var certificate = new CertificateRequest("CN=sample-auth-method-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            .CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));
        var destination = new Uri("https://foxids.example.test/tenant/environment/app(*)/saml/authn?partner=sample");
        var settings = new Settings
        {
            DownParty = "app",
            SendAuthMethodParameter = enabled,
            AuthMethod = "login entra+work",
            ParallelFoxIDsUpParty = "other",
            SendLoginParameter = true,
            LoginParameterProfileId = "profile+/&test",
            LoginParameterDepartments = "Sales Support"
        };
        var configuration = new Saml2Configuration
        {
            Issuer = "https://app.example.test/",
            SingleSignOnDestination = destination,
            SigningCertificate = certificate,
            SignAuthnRequest = true
        };
        var controller = new SamlController(new FixedOptions<Settings>(settings), new FixedOptions<Saml2Configuration>(configuration))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.Request.Scheme = "https";
        controller.Request.Host = new HostString("app.example.test");

        var result = controller.Login("/Home/Secure", LoginType.ParallelFoxIDs);

        var received = new DefaultHttpContext().Request;
        var requestUrl = new Uri(Assert.IsType<RedirectResult>(result).Url);
        received.Method = "GET";
        received.Scheme = requestUrl.Scheme;
        received.Host = new HostString(requestUrl.Host);
        received.Path = requestUrl.AbsolutePath;
        received.QueryString = new QueryString(requestUrl.Query);
        Assert.Equal(enabled, received.Query.ContainsKey("auth_method"));
        if (enabled) Assert.Equal("login entra+work", Assert.Single(received.Query["auth_method"]));
        Assert.Equal("profile+/&test", received.Query["profile_id"]);
        Assert.Equal("Sales Support", received.Query["departments"]);
        Assert.Equal("sample", received.Query["partner"]);
        Assert.Equal(destination, configuration.SingleSignOnDestination);

        var validation = new Saml2Configuration
        {
            AllowedIssuer = configuration.Issuer,
            CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None,
            SignAuthnRequest = true,
            SignatureValidationCertificates = [certificate]
        };
        var request = new Saml2AuthnRequest(validation);
        var httpRequest = received.ToGenericHttpRequest(validate: true);
        httpRequest.Binding.Unbind(httpRequest, request);

        Assert.Equal(enabled ? destination.OriginalString : destination.OriginalString.Replace("app(*)", "app(other)"), request.Destination.OriginalString);
        Assert.Equal("/Home/Secure", httpRequest.Binding.GetRelayStateQuery()["ReturnUrl"]);
        Assert.DoesNotContain("auth_method", request.XmlDocument.OuterXml);
        Assert.DoesNotContain("profile_id", request.XmlDocument.OuterXml);
        Assert.DoesNotContain("departments", request.XmlDocument.OuterXml);
    }

    private sealed class FixedOptions<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string name) => value;
        public IDisposable OnChange(Action<T, string> listener) => null;
    }
}
