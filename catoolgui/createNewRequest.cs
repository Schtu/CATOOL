using System;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace catoolgui
{
	public partial class createNewRequest : Gtk.Window
	{
		string selectedCA;
		createCert cWin;
		msgWindow mWindow;
		certParser parser = new certParser ();
		Dictionary<string,string> importedInfo = new Dictionary<string, string> ();
		Action certLoad;
		string reqPath;
		bool isimport=false;

		//Konstruktur bei nicht importierten Requests

		public createNewRequest (string input, Action certAction) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			selectedCA = input;
			certLoad = certAction;
		}

		/*Bei importierten Requests wird das Entryfield Key nicht benötitgt, da der
		 * importierte Request bereits mit einem privaten Schlüssel gesigned wurde.
		 * Die Checkbox zum einstellen der Schlüssellänge wird ebenfalls nicht
		 * angezeigt.
		 * Weiterhin werden die Bestandteile des Subject Names aus dem importierten
		 * Req. ausgelsen und in einem Dictionary gespeichert. Dadurch können die veränderten
		 * Daten beim erstellen des Zertifikats wieder in dieses eingepflegt werden.*/

		public createNewRequest (string input, Action certAction, string path) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			selectedCA = input;
			certLoad = certAction;
			reqPath = path;
			isimport = true;
			setStates ();
			keyLabel.Visible = false;
			KeyLabelConf.Visible = false;
			reqKeyPass.Visible = false;
			reqKeyPassConf.Visible = false;
			keyBoxLabel.Visible = false;
			reqKeySizeBox.Visible = false;
		}
			
		protected void OnOKButtonReqClicked (object sender, EventArgs e)
		{
			List<string> errList = new List<string> ();
		
			//Prüfen der verschiedenen Entryfelder anhand von Regex.

			if (!Regex.IsMatch (reqName.Text, regExCases.storageRegex)) {
				errList.Add ("Entry: Storagename must only contain A-Z, a-z and/or 0-9, no whitespace allowed");
			}
			if (reqName.Text.Equals ("")) {
				errList.Add ("Entry: Storagename must not be empty");
			} 
			if (reqName.Text.Length > 100) {
				errList.Add ("Entry: Storagename max. length = 100 characters");
			}

			if (!Regex.IsMatch (reqCommon.Text, regExCases.commonRegex)) {
				errList.Add ("Entry: Commonname must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
			} 
			if (reqCommon.Text.Equals ("")) {
				errList.Add ("Entry: Commonname must not be empty");
			} 
			if (reqCommon.Text.Length > 100) {
				errList.Add ("Entry: Commonname max. length = 100 characters");
			} 

			if (!Regex.IsMatch (reqCountry.Text, regExCases.countryRegex)) {
				errList.Add ("Entry: Country must be a 2 letter code (A-Z and a-z)");
			}
			if (reqCountry.Text.Equals ("")) {
				errList.Add ("Entry: Country must not be empty");
			}

			if (reqState.Text.Equals ("")) {
				errList.Add ("Entry: State/Province Name must not be empty");
			} 
			if(!Regex.IsMatch (reqState.Text, regExCases.stateRegex)) {
				errList.Add ("Entry: State/Province Name must only contain A-Z,a-z and/or -"); 
			}

			if (!reqLocality.Text.Equals ("")) {
				if (!Regex.IsMatch (reqLocality.Text, regExCases.commonRegex)) {
					errList.Add ("Entry: Locality must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
				}
			}

			if (reqOrga.Text.Equals ("")) {
				errList.Add ("Entry: Organization must not be empty");
			}
			if (!Regex.IsMatch (reqOrga.Text, regExCases.commonRegex)) {
				errList.Add ("Entry: Organization must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
			}

			if (!reqOrgaUnit.Text.Equals ("")) {
				if (!Regex.IsMatch (reqOrgaUnit.Text, regExCases.commonRegex)) {
					errList.Add ("Entry: Organization Unit must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
				}
			}

			if (!reqeMail.Text.Equals ("")) {
				if (!Regex.IsMatch (reqeMail.Text, regExCases.emailRegex)) {
					errList.Add ("Entry: eMail has the wrong format");
				} 
				if (reqeMail.Text.Length >= 64) {
					errList.Add ("Entry: eMail max. length = 64 characters");
				} 
				if (reqeMail.Text.Length < 6) {
					errList.Add ("Entry: eMail min. length = 6 characters");
				}
			}
				
			if (!Regex.IsMatch(reqDays.Text,regExCases.validRegex)){
				errList.Add ("Entry: Days must contain 1 up to 4 decimal numbers");
			}

			if (!isimport) {
				if (!Regex.IsMatch (reqKeyPass.Text, regExCases.passRegex)) {
					errList.Add ("Entry: Password must contain 4 digits at least");
				} 
				if (!(reqKeyPass.Text.Equals (reqKeyPassConf.Text))) {
					errList.Add ("Please Confirm with the same Password");
				}
			}	
			if (errList.Count == 0) {
				try{

					//Lese aktuelle Seriennummer der CA

					using (StreamReader sr = new StreamReader (firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/serial")) {
					string serial = sr.ReadLine ();
						sr.Close ();

						/*Handel es sich um einen nicht importierten Request, wird zuerst ein privater
						 * RSA-Schlüssel erstellt. Anschließen wird mit diesem Schlüssel ein Request 
						 * erstellt. Anschließen wird der Datenbank ein neuer Eintrag hinzugefügt. Mit
						diesem Request wird im nächsten Schritt das Fenster zur Erstellung des Zertifikats
						geöffnet.*/

						if(!isimport){
							
						caHandling.createRSAREQKey(selectedCA,serial,reqCipherBox.ActiveText,
								reqKeyPass.Text,reqKeySizeBox.ActiveText);

						caHandling.createReqCert(selectedCA,serial,reqCommon.Text,reqKeyPass.Text,reqDays.Text,reqCountry.Text,
						reqState.Text,reqLocality.Text,reqOrga.Text,reqOrgaUnit.Text);
						insertReq(serial);
							cWin = new createCert(certLoad,firstSetup.mainDir + "/" +  mainWindow.selectedCA +"-ca/certreqs/"+serial+".csr",reqName.Text,reqeMail.Text);
							Console.WriteLine(reqeMail.Text);
							this.Destroy();
						}

						/*Bei einem importierten Request werden lediglich die Informationen bezüglich
						 * des Subjectnames ausgelesen, in ein Dictionary geschrieben und im nächsten
						 * Schritt an der Fenster zur Erstellung eines Zertifikats weitergereicht.*/

						else{
							collectImportInfo();
							cWin = new createCert(certLoad,reqPath,importedInfo,reqeMail.Text);
							this.Destroy();
						}
					}
				}
				catch (SqliteException sqle){
					mWindow = new msgWindow (sqle.Message,"error");
					return;
				}
			} else {
				mWindow = new msgWindow (errList,"error");
			}

		}
			
		protected void OnCancelButtonReqClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		/*Funktion wird nur bei nicht importierten Requests aufgerufen, da importierte Reqs. bereits
		 * beim Importvorgang in eine Datenbank aufgenommen werden*/
	
		public void insertReq(string serial){

			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/reqDB.sqlite")) {
				con.Open ();
				string stm = "insert into reqs (reqName, caName, reqPath) values ('" +
					reqName.Text +"','"+mainWindow.selectedCA+"','" + firstSetup.mainDir + 
					"/" + mainWindow.selectedCA + "-ca/certreqs/" + serial + ".csr')";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
		}

		//Liest per Parser Daten aus dem importierten Request

		public void setStates(){
			parser.readAll (reqPath);
			reqCommon.Text = parser.scommonName;
			reqCountry.Text = parser.sCountry;
			reqState.Text = parser.sstate;
			reqLocality.Text = parser.slocal;
			reqOrga.Text = parser.sorga;
			reqOrgaUnit.Text = parser.sorgaUnit;
		}

		//Schreibe die evtl. abgeänderten Daten in ein Dictionary

		public void collectImportInfo(){
			importedInfo.Add ("Storagename", reqName.Text);
			importedInfo.Add ("Commonname", reqCommon.Text);
			importedInfo.Add ("Country", reqCountry.Text);
			importedInfo.Add ("State", reqState.Text);
			importedInfo.Add ("Locality", reqLocality.Text);
			importedInfo.Add ("Organization", reqOrga.Text);
			importedInfo.Add ("Organizationunit", reqOrgaUnit.Text);
			importedInfo.Add ("Pass", reqKeyPass.Text);
		}
	}
}