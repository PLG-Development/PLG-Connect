using System;

namespace PLG_Connect;

// Not finished - long term plans - just ignore
public static class MessageBox
{
    // public static Result Show(string title, string message, MessageBoxButtons buttons){
    //     Window w = new Window();
    //     w.Title = title;
    //     Grid g = new Grid();
    //     g.Children.Add(new Label(){Content=message});
    //     if(buttons == MessageBoxButtons.OK)
    //     {
    //         g.Children.Add(new Button(){Content="OK"});
    //     } else {
    //         g.Children.Add(new Button(){Content="Yes"});
    //         g.Children.Add(new Button(){Content="No"});

    //     }
    //     w.Children = g;

    //     w.ShowDialog();
    //     return Result.Yes;

    // }
}

public enum MessageBoxButtons {
    OK,
    YesNo,
}

public enum Result{
    Yes,
    No,
    Cancel,
    OK
}