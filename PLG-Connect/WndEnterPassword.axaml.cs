using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PLG_Connect;

public partial class WndEnterPassword : Window
{
    private string _hash;
    private TextBox _passwordBox;
    private TaskCompletionSource<bool> _tcs;

    public WndEnterPassword(string hash)
    {
        InitializeComponent();
        _hash = hash;
        _passwordBox = this.FindControl<TextBox>("PasswordBox");
        _passwordBox.PasswordChar = '•';
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public Task<bool> ShowDialogWithResult(Window owner)
    {
        _tcs = new TaskCompletionSource<bool>(); // Initialisierung der TaskCompletionSource
        this.ShowDialog(owner); // Öffne das Fenster
        return _tcs.Task; // Rückgabe des Tasks, der entweder true oder false enthält
    }

    // Event-Handler für den OK-Button
    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        string enteredPassword = _passwordBox.Text;

        if (VerifyPasswordHash(enteredPassword, _hash))
        {
            Logger.Log("Monitor unlocked!");
            _tcs.TrySetResult(true);
            this.Close();
        }
        else
        {
            Logger.Log("Wrong password...");
            _passwordBox.Clear();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        _tcs.TrySetResult(false);
        this.Close();
    }


    private bool VerifyPasswordHash(string password, string storedHash)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            string hash = Convert.ToBase64String(bytes);

            return hash == storedHash;
        }
    }
}