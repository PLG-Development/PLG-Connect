namespace PLG_Connect_Network;


class DisplayTextMessage
{
    public required string Text { get; set; }
}


public class RunCommandMessage
{
    public required string Command { get; set; }
}


public class OpenSlideMessage
{
    public required string SlidePath { get; set; }
}
