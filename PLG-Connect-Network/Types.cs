namespace PLG_Connect_Network;


class DisplayTextMessage
{
    public required string Text { get; set; }
}


public class RunCommandMessage
{
    public required string Command { get; set; }
}


public class ChangePasswordMessage
{
    public required string NewPassword { get; set; }
}

public class ToggleBlackScreenReturnMessage
{
    public required bool BlackScreenEnabled { get; set; }
}

public class HasFileResponse
{
    public required bool HasFile { get; set; }
}