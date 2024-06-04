using PLG_Connect_Network;


namespace PLG_Connect;

class Display : ClientConnection
{
    public string Name;

    public Display(DisplaySetting setting): base(setting.IPAddress, setting.MacAddress)
    {
        Name = setting.Name;
    }

    public string GenerateConfig()
    {

    }
}


struct DisplaySetting {
    public string Name;
    public string IPAddress;
    public string MacAddress;
}
