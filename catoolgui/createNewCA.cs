using System;
using compactCA;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;

namespace compactCA
{
	public partial class createNewCA : Gtk.Window
	{
		public static List<string> usrList = new List<string> ();
		public static List<string> v3reqList = new List<string> ();
		public static List<string> v3caList = new List<string> ();
		public static List<string> crlList = new List<string> ();
		public static string crlDays = "30";

		msgWindow mWindow;
		crlExt sWindow;
		Action loadCA;

		public createNewCA (Action caLoad) :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			loadCA = caLoad;
		}
			
		protected void okButtonclicked (object sender, EventArgs e)
		{

			//Prüfen der verschiedenen Entryfelder anhand von Regex.

			List<string> errList = new List<string> ();
		
			if (!Regex.IsMatch (NameCA.Text, regExCases.storageRegex)) {
				errList.Add ("Entry: Storagename must only contain A-Z, a-z and/or 0-9, no whitespace allowed");
			}
			if (NameCA.Text.Equals ("")) {
				errList.Add ("Entry: Storagename must not be empty");
			} 
			if (NameCA.Text.Length > 100) {
				errList.Add ("Entry: Storagename max. length = 100 characters");
			} 

			if (!Regex.IsMatch (commonNameCA.Text, regExCases.commonRegex)) {
				errList.Add ("Entry: Commonname must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
			} 
			if (commonNameCA.Text.Equals ("")) {
				errList.Add ("Entry: Commonname must not be empty");
			} 
			if (commonNameCA.Text.Length > 100) {
				errList.Add ("Entry: Commonname max. length = 100 characters");
			} 

			if (!Regex.IsMatch (CountryCA.Text, regExCases.countryRegex)) {
				errList.Add ("Entry: Country must be a 2 letter code (A-Z and a-z)");
			}
			if (CountryCA.Text.Equals ("")) {
				errList.Add ("Entry: Country must not be empty");
			}
		
			if (StateProvinceCA.Text.Equals ("")) {
				errList.Add ("Entry: State/Province Name must not be empty");
			} 
			if(!Regex.IsMatch (StateProvinceCA.Text, regExCases.stateRegex)) {
				errList.Add ("Entry: State/Province Name must only contain A-Z,a-z and/or -"); 
			}

			if (!LocalityCA.Text.Equals ("")) {
				if (!Regex.IsMatch (LocalityCA.Text, regExCases.commonRegex)) {
					errList.Add ("Entry: Locality must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
				}
			}

			if (OrganizationCA.Text.Equals ("")) {
				errList.Add ("Entry: Organization must not be empty");
			}
			if (!Regex.IsMatch (OrganizationCA.Text, regExCases.commonRegex)) {
				errList.Add ("Entry: Organization must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
			}

			if (!OrganizationUnitCA.Text.Equals ("")) {
				if (!Regex.IsMatch (OrganizationUnitCA.Text, regExCases.commonRegex)) {
					errList.Add ("Entry: Organization Unit must only contain A-Z, a-z, 0-9 and/or &#45; &amp; &#46; &#58; &#47;");
				}
			}

			if (!eMailCA.Text.Equals ("")) {
				if (!Regex.IsMatch (eMailCA.Text, regExCases.emailRegex)) {
					errList.Add ("Entry: eMail has the wrong format");
				} 
				if (eMailCA.Text.Length >= 64) {
					errList.Add ("Entry: eMail max. length = 64 characters");
				} 
				if (eMailCA.Text.Length < 6) {
					errList.Add ("Entry: eMail min. length = 6 characters");
				}
			}

			if (!Regex.IsMatch (PasswordCA.Text, regExCases.passRegex)) {
				errList.Add ("Entry: Password must contain 4+ digits");
			} 
			if (!(PasswordCA.Text.Equals (PasswordCAConfirmation.Text))) {
				errList.Add ("Please Confirm with the same Password");
			}
				
			if (!Regex.IsMatch(ValidCA.Text,regExCases.validRegex)){
				errList.Add ("Entry: Days must contain 1 up to 4 decimal numbers");
			}

			if (errList.Count == 0) {

				//Lege Ordnerstruktur und Datenbanken der CA an

				caHandling.createDirectorys (NameCA.Text);
				firstSetup.createDB (firstSetup.mainDir + "/" + NameCA.Text + "-ca/certsdb/certDB.sqlite","create table certs (certName varchar(100) primary key, certNr varchar(10), certPath varchar(200), caName varchar(100), reqPath varchar(200))");
				firstSetup.createDB (firstSetup.mainDir + "/" + NameCA.Text + "-ca/certsdb/reqDB.sqlite","create table reqs (reqName varchar(100) primary key, caName varchar(100), reqPath varchar(200))");
				firstSetup.createDB (firstSetup.mainDir + "/" + NameCA.Text + "-ca/certsdb/importReqsDB.sqlite","create table importReqs (reqName varchar(100) primary key, caName varchar(100), reqPath varchar(200))");

				//Setze Extensions für das Configfile der CA

				setBasicExtensions ();
				createConfigFile.writeUsrExt(usrList);
				createConfigFile.writev3reqExt(v3reqList);
				createConfigFile.writev3caExt(v3caList);
				createConfigFile.writecrlExt(crlList);
				createConfigFile.writeConfig (NameCA.Text,DigestCA.ActiveText,KeySizeBoxCA.ActiveText,crlDays);

				//Erstelle Key, Request und Selfsigned Cert.

				caHandling.createRSACAKey (NameCA.Text,cipherAlgoBox.ActiveText,PasswordCA.Text,KeySizeBoxCA.ActiveText);
				caHandling.createReqCa (NameCA.Text, PasswordCA.Text, ValidCA.Text, CountryCA.Text,
					StateProvinceCA.Text, LocalityCA.Text, OrganizationCA.Text, OrganizationUnitCA.Text,
					commonNameCA.Text);
				caHandling.selfsignCa (NameCA.Text, ValidCA.Text, PasswordCA.Text, DigestCA.ActiveText);
				caHandling.genCRL (NameCA.Text, PasswordCA.Text);

				usrList.Clear();
				v3reqList.Clear();
				v3caList.Clear();
				crlList.Clear ();



				//Bei Errormeldung durch Openssl wird das aktuelle Verzeichnis komplett gelöscht


				if (!caHandling.lastLine.Contains ("error")) {
					mainWindow.selectedCA = NameCA.Text;
					mainWindow.clearCAStore ();
					mainWindow.clearREQStore ();
					mainWindow.clearCertStore ();
					try{
					insertIntoCA ();
					}
					catch(SqliteException ex){
						mWindow = new msgWindow (ex.Message,"error");
						return;
					}
					loadCA ();
					mWindow = new msgWindow ("CA: " + NameCA.Text + " was created", "success");
				} else {
					caHandling.callProc ("/bin/rm", "-r " + firstSetup.mainDir + "/" + NameCA.Text + "-ca/", "CA: " + NameCA.Text + " deleted, error detected");
				}
				crlExt.crlURL = "";
				this.Destroy ();
			} 
			else {
				mWindow = new msgWindow (errList,"error");
			}

			
		}

		protected void cancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();		
		}

		protected void OnExtButtonClicked (object sender, EventArgs e)
		{
			sWindow = new crlExt ();
		}
			
		public void insertIntoCA(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/caDB.sqlite")) {
				con.Open ();
				string sql = "insert into CA (caName, path) values ('" + NameCA.Text + "', '" +
				             firstSetup.mainDir + "/" + NameCA.Text + "-ca/certs/1000.pem" + "')";
				using (SqliteCommand cmd = new SqliteCommand (sql, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
		}

		public void setBasicExtensions(){
			usrList.Add ("basicConstraints = CA:FALSE");
			usrList.Add ("subjectKeyIdentifier = hash");
			usrList.Add ("authorityKeyIdentifier = keyid");
			v3reqList.Add ("basicConstraints = CA:FALSE");
			v3caList.Add ("basicConstraints = critical, CA:TRUE, pathlen:0");
			v3caList.Add ("subjectKeyIdentifier = hash");
			v3caList.Add ("authorityKeyIdentifier = keyid");
			v3caList.Add ("keyUsage = keyCertSign, cRLSign");

			if (!eMailCA.Text.Equals ("")) {
				v3caList.Add ("subjectAltName = email:" + eMailCA.Text);
			}

			crlList.Add ("authorityKeyIdentifier = keyid");
		}
	}
}