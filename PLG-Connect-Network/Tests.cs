using Xunit;


namespace PLG_Connect_Network.Tests;


public class Tests
{
  [Theory]
  [InlineData("00:11:22:33:44:55", false)]
  [InlineData("00:11:22:33:44:55:66", true)]
  [InlineData("11:22:33:44:55:66", false)]
  [InlineData("11:22:33:44:55:66:77", true)]
  public void TestClientConnectionCreation(string macAddress, bool shouldThrow)
  {
    if (shouldThrow)
    {
      Assert.Throws<ArgumentException>(() => new ClientConnection("127.0.0.1", macAddress, "password"));
    }
    else
    {
      var client = new ClientConnection("127.0.0.1", macAddress, "password");
    }
  }

  [Fact]
  public async Task TestPing()
  {
    var server = new Server("password", 8081);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "wrongPassword", 8081);

    bool success = await client.Ping();
    Assert.True(success);

    server.Stop();
  }

  [Fact]
  public async Task TestDisplayText()
  {
    var server = new Server("password", 8082);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "password", 8082);

    string recivedText = "";
    server.displayTextHandlers.Add((text) => {recivedText = text;});
    await client.DisplayText("Hello World");
    Assert.Equal("Hello World", recivedText);

    server.Stop();
  }

  [Fact]
  public async Task TestToggleBlackScreen()
  {
    var server = new Server("password", 8083);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "password", 8083);

    bool blackScreenState;
    blackScreenState = await client.ToggleBlackScreen();
    Assert.True(blackScreenState);

    blackScreenState = await client.ToggleBlackScreen();
    Assert.False(blackScreenState);

    server.Stop();
  }

  [Fact]
  public async Task TestRunCommand()
  {
    var server = new Server("password", 8084);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "password", 8084);

    string recivedCommand = "";
    server.runCommandHandlers.Add((command) => {recivedCommand = command;});
    await client.RunCommand("ls");
    Assert.Equal("ls", recivedCommand);

    server.Stop();
  }

  [Fact]
  public async Task TestPreviousSlide()
  {
    var server = new Server("password", 8085);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "password", 8085);

    bool recivedMessage = false;
    server.previousSlideHandlers.Add(() => {recivedMessage = true;});
    await client.PreviousSlide();
    Assert.True(recivedMessage);

    server.Stop();
  }

  [Fact]
  public async Task TestNextSlide()
  {
    var server = new Server("password", 8086);
    var client = new ClientConnection("127.0.0.1", "00:11:22:33:44:55", "password", 8086);

    bool recivedMessage = false;
    server.nextSlideHandlers.Add(() => {recivedMessage = true;});
    await client.NextSlide();
    Assert.True(recivedMessage);

    server.Stop();
  }
}
