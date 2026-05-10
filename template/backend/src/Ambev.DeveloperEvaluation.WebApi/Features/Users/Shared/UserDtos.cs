namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.Shared;

public class UserNameDto
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}

public class UserGeolocationDto
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}

public class UserAddressDto
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public UserGeolocationDto Geolocation { get; set; } = new();
}
