namespace MemberService.Tests.Newsletter;

using System.Net;

using MemberService.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

[TestFixture]
public class NewsletterServiceTests
{
    [TestCase("Ola Nordmann", "Ola", "Nordmann")]
    [TestCase("Ola Kari Nordmann", "Ola", "Kari Nordmann")]
    [TestCase("Ola", "Ola", "")]
    [TestCase("  ", "", "")]
    [TestCase(null, "", "")]
    public void SplitName_SplitsOnFirstSpace(string fullName, string first, string last)
        => MailchimpNewsletterService.SplitName(fullName).ShouldBe((first, last));

    [Test]
    public async Task Subscribe_PostsFormFieldsToMailchimp()
    {
        var handler = new FakeHandler(HttpStatusCode.OK);
        var service = CreateService(handler);

        var result = await service.Subscribe("ola@example.com", "Ola Nordmann");

        result.ShouldBeTrue();
        handler.Request.Method.ShouldBe(HttpMethod.Post);
        handler.Request.RequestUri.AbsoluteUri.ShouldBe("https://example.list-manage.com/subscribe/post?u=1&id=2");
        handler.Body.ShouldBe("EMAIL=ola%40example.com&FNAME=Ola&LNAME=Nordmann&b_1_2=");
    }

    [Test]
    public async Task Subscribe_ReturnsFalseOnServerError()
    {
        var service = CreateService(new FakeHandler(HttpStatusCode.InternalServerError));

        (await service.Subscribe("ola@example.com", "Ola Nordmann")).ShouldBeFalse();
    }

    [Test]
    public async Task Subscribe_ReturnsFalseWhenRequestThrows()
    {
        var service = CreateService(new FakeHandler(new HttpRequestException("down")));

        (await service.Subscribe("ola@example.com", "Ola Nordmann")).ShouldBeFalse();
    }

    [Test]
    public async Task Subscribe_DoesNothingWithoutEmail()
    {
        var handler = new FakeHandler(HttpStatusCode.OK);

        (await CreateService(handler).Subscribe("", "Ola Nordmann")).ShouldBeFalse();
        handler.Request.ShouldBeNull();
    }

    private static MailchimpNewsletterService CreateService(FakeHandler handler)
        => new(
            new HttpClient(handler),
            Options.Create(new NewsletterSettings
            {
                SubscribeUrl = "https://example.list-manage.com/subscribe/post?u=1&id=2",
                HoneypotField = "b_1_2"
            }),
            NullLogger<MailchimpNewsletterService>.Instance);

    private class FakeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly Exception _exception;

        public FakeHandler(HttpStatusCode status) => _status = status;

        public FakeHandler(Exception exception) => _exception = exception;

        public HttpRequestMessage Request { get; private set; }

        public string Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = await request.Content.ReadAsStringAsync(cancellationToken);

            if (_exception != null) throw _exception;

            return new HttpResponseMessage(_status);
        }
    }
}
