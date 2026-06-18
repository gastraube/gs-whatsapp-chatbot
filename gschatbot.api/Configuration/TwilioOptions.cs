namespace gschatbot.api.Configuration;

public class TwilioOptions
{
    public const string Section = "Twilio";

    public string AccountSid { get; set; } = "";
    public string AuthToken { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
}
