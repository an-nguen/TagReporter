using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TagReporter.Models;

[JsonObject(MemberSerialization.OptIn)]
public class WstAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    [JsonProperty("password")]
    public string? Password { get; set; }

    [NotMapped]
    public string? SessionId { get; set; }

    public WstAccount() { }
    public WstAccount(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public async Task<bool> SignIn(Uri baseAddress)
    {
        using var client = new HttpClient() { BaseAddress = baseAddress };
        var jsonString = JsonConvert.SerializeObject(this);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var result = await client.PostAsync("/ethAccount.asmx/SignIn", content);
        var setCookieHeader = result.Headers.FirstOrDefault(kv => kv.Key == "Set-Cookie");
        if (setCookieHeader.Equals(default))
        {
            return false;
        }

        var cookies = setCookieHeader.Value;
        if (cookies == null)
        {
            return false;
        }

        var cookie = cookies.FirstOrDefault(c => c.StartsWith("WTAG="));
        if (cookie == null)
        {
            return false;
        }
        var sessionId = cookie.Split(';')[0];

        if (string.IsNullOrEmpty(sessionId))
        {
            return false;
        }

        sessionId = sessionId.Substring("WTAG=".Length, sessionId.Length - "WTAG=".Length);
        SessionId = sessionId;
        return true;
    }
}

