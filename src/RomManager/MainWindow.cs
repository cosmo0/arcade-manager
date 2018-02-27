using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        // disable secondary file picker
        //this.filechooserSecondary.Sensitive = false;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    protected void ButtonProcess_Clicked(object sender, EventArgs e)
    {
        using (var mainReader = new CsvReader.MameCsvReader(this.filechooserMain.Filename)) {
            
        }
    }
}
