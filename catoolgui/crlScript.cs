using System;

namespace compactCA
{
	public partial class crlScript : Gtk.Dialog
	{
		Gtk.FileFilter filter = new Gtk.FileFilter();
		msgWindow mWin;

		/*Konstruktur legt Filter für das Format des ausführbaren Scripts fest und
		 * ruft die Funktion auf, welche den Pfad für die CRL in das Entryfield
		 * schreibt.*/

		public crlScript ()
		{
			this.Build ();
			filter.AddPattern ("*.sh");
			filter.Name = "Bash Script";
			scriptChooser.AddFilter (filter);
			loadPath ();
		}

		protected void OnButtonCloseClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		//Schreibe Pfad der CRL in das Entryfield

		public void loadPath(){
			crlPath.Text = firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/crl/" +
			mainWindow.selectedCA + "-ca.crl";
		}

		//Führe das Script auf dem ausgewählten Pfad aus

		protected void OnRunScriptClicked (object sender, EventArgs e)
		{
			if (scriptChooser.Filename != null) {
				caHandling.runScript (scriptChooser.Filename);
			} else {
				mWin = new msgWindow ("Please select a valid Path","error");
			}
		}
	}
}