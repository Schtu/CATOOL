using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using Gtk;

namespace catoolgui
{
	public partial class exportCert : Gtk.Dialog
	{
		string selectedPath="",selectedNum="",checkPath="";
		List<string> errList = new List<string> ();
		msgWindow mWin;
		FileFilter keyFilter = new FileFilter ();
		bool import = false;

		/*Hier wird wieder zwischen zwei Typen unterschieden. Wurde das Zertitfikat von einem
		 * importierten Request erstellt liegt der private Schlüssel des Zertifikats nicht vor.
		 * Somit kann das Zertifikat auch nur in das Format PEM exportiert werden, welches
		 * lediglich der öffentlichen Schlüssel des Antragsstellers enthält.*/

		public exportCert (string path, string num)
		{
			this.Build ();
			selectedPath = path;
			selectedNum = num;

			/*Die Datenbank der Zertifikate enthält ein Feld, welches den Pfad zum verwendeten
			 * Request angibt. chekcReq() prüft, ob dieses Pfad das Wort "import" enthält.*/

			checkReq ();

			//Felder wie Keypasswort usw. werden bei Importreqs. nicht benötigt

			if(checkPath.Contains("import")){
				import = true;
				privPassLabel.Visible = false;
				privPass.Visible = false;
				passLabel.Visible = false;
				certPass.Visible = false;
				this.Resize (400, 125);
			}
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			//Prüfe die Entryfelder per Regex

			if (filechooserbuttonCert.Filename == null) {
				errList.Add ("Path for export must not be empty");
			}
			if (!Regex.IsMatch (nameExCert.Text, regExCases.storageRegex)) {
				errList.Add ("Export name must only contain A-Z, a-z and 0-9");
			} 
			if (nameExCert.Text.Equals ("")) {
				errList.Add ("Export name must not be empty");
			} 
			if (nameExCert.Text.Length >= 100) {
				errList.Add ("Export name max. length = 100 characters");
			} 
				
			if (errList.Count == 0) {

				/* Würde ein normales Export stattfinden, wird ins PKCS12 Format exportiert. Dieses
				 * Format enthält das Zertifikat, den öffentlichen und den privaten Schlüssel. Um 
				 * letzteren verwenden zu können wird allerdings das Password benötigt. Bei einer
				 * nicht korrekten Eingabe wird von Openssl eine Errormeldung ausgegeben, welche hier 
				 * wieder zum prüfen der Korrektheit des Passwortes verwendet wird.*/

				caHandling.checkPass (privPass.Text);

				if (!caHandling.lastLine.Contains ("unable to load Private Key")) {
					if (!import) {

						caHandling.exportpkcs12Cert (nameExCert.Text, selectedPath, filechooserbuttonCert.Filename, selectedNum,
							certPass.Text, privPass.Text, firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certreqs/" +
						selectedNum + ".key");
						this.Destroy ();
					} else {
						caHandling.exportImportCert (nameExCert.Text, selectedPath, filechooserbuttonCert.Filename);
						this.Destroy ();
					}
				} else {
					mWin = new msgWindow ("Wrong password for private key", "error");
				}
			} else {
				mWin = new msgWindow (errList, "error");
				errList.Clear ();
			}	
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		public void checkReq(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/certDB.sqlite")) {
				con.Open ();
				string sql = "SELECT reqPath FROM certs WHERE certPath = '" + selectedPath + "'";
				using (SqliteCommand cmd = new SqliteCommand (sql, con)) {
					checkPath = cmd.ExecuteScalar ().ToString();
				}
				con.Close ();
			}
		}
	}
}