using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json.Nodes;

namespace UserManagement.Services;

public static class CaptchaService
{
    private static readonly (string Question, string Answer)[] Questions = {
        ("Jaki kolor ma niebo?", "niebieski"),
        ("Ile jest 2 + 2?", "4"),
        ("Jakie jest pierwsze słowo alfabetu?", "a")
    };

    public static (byte[] ImageData, string Answer) GenerateCaptcha()
    {
        var random = new Random();
        var (question, answer) = Questions[random.Next(Questions.Length)];

        Font font = new Font("Arial", 14);
        int padding = 10; // odstęp wokół tekstu
        SizeF textSize;

        using (var tempBmp = new Bitmap(1, 1))
        using (var g = Graphics.FromImage(tempBmp))
        {
            textSize = g.MeasureString(question, font);
        }

        // Ustaw rozmiar bitmapy na podstawie tekstu i odstępu
        int width = (int)textSize.Width + padding * 2;
        int height = (int)textSize.Height + padding * 2;

        using var bmp = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bmp);

        // Wypełnij tło i narysuj tekst
        graphics.Clear(Color.Gray);
        graphics.DrawString(question, font, Brushes.Black, new PointF(padding, padding));

        using var stream = new MemoryStream();
        bmp.Save(stream, ImageFormat.Png);
        
        return (stream.ToArray(), answer);
    }

    public static async Task<bool> VerifyReCaptchaV3(string response, string secret, string verificationUrl)
    {
        using (var client = new HttpClient())
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(response), "response");
            content.Add(new StringContent(secret), "secret");
            
            var result = await client.PostAsync(verificationUrl, content);
            if (result.IsSuccessStatusCode)
            {
                var strResponse = await result.Content.ReadAsStringAsync();
                Console.WriteLine($"Captcha: {strResponse}");
                
                var jsonResponse = JsonNode.Parse(strResponse);
                if (jsonResponse != null)
                {
                    var success = ((bool?)jsonResponse["success"]);
                    if(success != null && success == true) return true;
                }
            }
        }
        return false;
    }
}