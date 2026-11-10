namespace Server.Config;

// Ánh xạ mục "Email" trong appsettings.json
public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;     
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Nhà hàng";
}