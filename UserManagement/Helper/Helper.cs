using System.Text;

namespace UserManagement.Helper;

public class Helper
{
    public static string GetStringDate(TimeSpan date)
    {
        StringBuilder builder = new StringBuilder();
        
        if (date.Days > 0)
            builder.Append($"{date.Days} dni ");
        
        if (date.Hours > 0)
            builder.Append($"{date.Hours} godzin ");
        
        if (date.Minutes > 0)
            builder.Append($"{date.Minutes} minut ");
        
        if (date.Seconds > 0)
            builder.Append($"{date.Seconds} sekund");
        
        return builder.ToString();
    }
}