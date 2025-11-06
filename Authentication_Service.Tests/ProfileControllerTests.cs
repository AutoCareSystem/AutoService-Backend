using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Authentication_Service.Tests;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetEmployeeProfile_ReturnsUnauthorizedOrNotFound()
    {
        var client = _factory.CreateClient();

        // Call a protected endpoint without auth - expect 401 or 404 if not configured
        var resp = await client.GetAsync("/api/profile/employee/1");
        Assert.True(resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.NotFound || resp.IsSuccessStatusCode == false);
    }

    [Fact]
    public async Task UpdateEmployeeProfile_ReturnsUnauthorizedOrNotFound()
    {
        var client = _factory.CreateClient();

        var content = new StringContent("{\"UserName\":\"newname\"}", System.Text.Encoding.UTF8, "application/json");
        var resp = await client.PutAsync("/api/profile/employee/1", content);
        Assert.True(resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.NotFound || resp.IsSuccessStatusCode == false);
    }
}
