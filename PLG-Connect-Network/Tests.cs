using Xunit;
using PLG_Connect_Network;

namespace PLG_Connect_Network.Tests;

public class Tests
{
  [Theory]
  [InlineData("00:11:22:33:44:55", false)]
  [InlineData("00:11:22:33:44:55:66", true)]
  [InlineData("11:22:33:44:55:66", false)]
  [InlineData("11:22:33:44:55:66:77", true)]
  private void TestClientConnectionCreation(string macAddress, bool shouldThrow)
  {
    if (shouldThrow)
    {
      Assert.Throws<ArgumentException>(() => new ClientConnection("127.0.0.1", macAddress, "password"));
    }
    else
    {
      new ClientConnection("127.0.0.1", macAddress, "password");
    }
  }
}
