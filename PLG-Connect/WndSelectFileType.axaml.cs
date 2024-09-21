using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PLG_Connect;

public partial class WndSelectFileType : Window
{
    private List<string> _types;
    Display currentDisp;

    public WndSelectFileType(Display d)
    {
        InitializeComponent();
        _types = new List<string> { "PDF", "PPTX/ODP", "Text-File", "Image-List" };  // Beispielhafte Liste
        currentDisp = d;
        CreateButtons();


    }

    private void CreateButtons()
    {
        var buttonPanel = this.FindControl<StackPanel>("ButtonPanel");

        foreach (var type in _types)
        {
            var button = new Button
            {
                Content = type,
                Width = 150,
                Height = 50,
                Margin = new Thickness(5)
            };

            button.Click += async (sender, e) => await OnButtonClick(type);

            buttonPanel.Children.Add(button);
        }
    }

    private async Task OnButtonClick(string type)
    {
        if (type == "Image-List")
        {
            // Öffne OpenFolderDialog für den Typ "Image-List"
            var openFolderDialog = new OpenFolderDialog();
            var result = await openFolderDialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                // Prüfen, ob der Ordner Bilder enthält (.jpg, .png, .jpeg)
                var imageFiles = Directory.GetFiles(result)
                                          .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                      f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                      f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                          .ToList();

                if (imageFiles.Any())
                {
                    Open(type, result); // Übergebe den Ordnerpfad
                }
                else
                {
                    // await MessageBox.Avalonia.MessageBoxManager
                    //     .GetMessageBoxStandardWindow("Fehler", "Der ausgewählte Ordner enthält keine Bilder.")
                    //     .Show();
                }
            }
        }
        else
        {
            // Öffne OpenFileDialog für andere Typen mit entsprechenden Filtern
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = GetFileDialogFilters(type)
            };

            var result = await openFileDialog.ShowAsync(this);

            if (result != null && result.Length > 0)
            {
                string filePath = result[0];
                Open(type, filePath); // Übergebe den Dateipfad
            }
        }
    }

    private List<FileDialogFilter> GetFileDialogFilters(string type)
    {
        // Rückgabe der entsprechenden Dateitypenfilter
        switch (type)
        {
            case "PDF":
                return new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "PDF Files", Extensions = new List<string> { "pdf" } }
                    };
            case "PPTX/ODP":
                return new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "PowerPoint Files", Extensions = new List<string> { "pptx" } },
                        new FileDialogFilter { Name = "OpenDocument Presentation", Extensions = new List<string> { "odp" } }
                    };
            case "Text-File":
                return new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "Text Files", Extensions = new List<string> { "txt" } }
                    };
            default:
                return null;
        }
    }

    private void Open(string type, string filePath)
    {
        Console.WriteLine($"User selected Type: {type}, File/Folder Path: {filePath}");



        if (type == "PDF")
        {
            // Send files to system (currentDisp)
            // Run PDF Viewer (maybe own implementation with PDF-Library?)
        }
        else if (type == "PPTX/ODP")
        {
            // Send files to system (currentDisp)
            // Run LibreOffice and press F5
        }
        else if (type == "Text-File")
        {
            currentDisp.DisplayText(File.ReadAllText(filePath));
        }
    }
}