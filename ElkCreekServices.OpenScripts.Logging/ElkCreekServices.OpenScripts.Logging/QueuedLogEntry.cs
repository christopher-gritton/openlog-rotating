namespace ElkCreekServices.OpenScripts.Logging;
internal class QueuedLogEntry
{
    private DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
    public bool IncludeDateTime { get; set; } = true;
    public bool IsUtcTime { get; set; } = true;

    public override string ToString()
    {
        if (IncludeDateTime)
        {
            if (IsUtcTime)
            {
                return $"{Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.DateTimeFormatInfo.InvariantInfo)} - {Message}";
            }
            else
            {
                return $"{Timestamp.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss (zz)", System.Globalization.DateTimeFormatInfo.InvariantInfo)} - {Message}";
            }     
        }
        else
        {
            return Message;
        }     
    }

}
