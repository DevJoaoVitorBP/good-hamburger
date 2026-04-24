namespace GoodHamburger.Web.Integration;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = "https://localhost:7228";
}
