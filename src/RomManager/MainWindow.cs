using System;
using System.IO;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        // TODO: enable/disable secondary file picker on radio selection change
        //this.filechooserSecondary.Sensitive = false;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void ButtonProcess_Clicked(object sender, EventArgs e)
    {
        if (!File.Exists(this.filechooserMain.Filename))
        {
            return;
        }

        using (var mainReader = new CsvReader.MameCsvReader(this.filechooserMain.Filename))
        {
            var roms = mainReader.GetRoms();
        }
    }
}
