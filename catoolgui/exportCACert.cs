using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace compactCA
{
	public partial class exportCACert : Gtk.Dialog
	{
		List<string> errList = new List<string> ();
		msgWindow mWin;

		public exportCACert ()
		{
			this.Build ();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			//Prüfe auf die Korrektheit der Entrys

			if (caCertChooser.Filename == null) {
				errList.Add ("Please choose a valid path to export");
			}
		
			if (!Regex.IsMatch (caCertExportEntry.Text, regExCases.storageRegex)) {
				errList.Add ("Filename must only contain A-Z, a-z and/or 0-9");
			}

			/*Rufe Exportfunktion auf. Hier wird nur im PEM-Format exportiert, da 
			 * der private Schlüssel der CA nicht weitergegeben werden darf*/

			if (errList.Count == 0) {
				caHandling.exportCaCert (mainWindow.selectedCA,caCertChooser.Filename + "/" + caCertExportEntry.Text + ".pem");
				mWin = new msgWindow ("CA-Certificate: " + caCertExportEntry.Text + " exported to \n " +
				"path: " + caCertChooser.Filename, "succes");
				this.Destroy ();
			} else {
				mWin = new msgWindow (errList, "error");
				errList.Clear ();
			}
		}
	}
}