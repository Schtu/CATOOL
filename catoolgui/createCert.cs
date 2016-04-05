using System;
using System.IO;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace compactCA
{
	public partial class createCert : Gtk.Window
	{
	 	public static List<string> extList = new List<string> ();
		List<string> dnsList = new List<string> ();
		List<string> uriList = new List<string> ();
		List<string> mailList = new List<string> ();
		List<string> ipList = new List<string> ();
		msgWindow mWin;
		public static string serial="",reqPath="",storageName="",subjMail="";
		public static bool isImport=false, hasMail=false;
		Action certLoad;
		Dictionary<string,string> importInfo = new Dictionary<string, string> ();

		//Konstruktur bei Erstellung von Zertifikaten mit nicht importiertem Request

		public createCert (Action certAction, string path, string sName,string mail) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			subAltremove.Sensitive = false;
			certLoad = certAction; 
			reqPath = path;
			storageName = sName;
			subjMail = mail;

			if (!subjMail.Equals ("")) {
				extList.Add ("email.0 =" + mail);
				hasMail = true;
			}

			//Lösche altes temporäres Extensionfile

			caHandling.callProc ("/bin/rm",firstSetup.mainDir + "/temp.ext", "Extension File removed");
		}

		/* Konstruktor zur Erstellung von Zertifikaten mit importiertem Request. Die evtl
		 * abgeänderten Subjectinformationen des Requests werden in einem Dictionary über-
		 * geben und dem Zertifikat beigefügt*/

		public createCert (Action certAction, string path, Dictionary<string,string> infoDic, string mail) :
		base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			subAltremove.Sensitive = false;
			certLoad = certAction; 
			reqPath = path;
			importInfo = infoDic;
			storageName = importInfo["Storagename"];
			isImport = true;
			subjMail = mail;

			if (!subjMail.Equals ("")) {
				extList.Add ("email.0 =" + mail);
				hasMail = true;
			}
			
			caHandling.callProc ("/bin/rm",firstSetup.mainDir + "/temp.ext", "Extension File removed");
		}
			
		protected void OnOkCertButtonClicked (object sender, EventArgs e)
		{
			caHandling.checkPass (certCaPass.Text);

			if (!caHandling.lastLine.Contains("unable to load Private Key")) { 

				//Bei korrektem CA-Passwort werden die Extensions ins Exfile geschrieben

				if (hasMail) {
					extList.AddRange (genAltNames ("email", mailList, 1));
				} else {
					extList.AddRange (genAltNames ("email", mailList, 0));
				}
					
				extList.AddRange (genAltNames ("DNS", dnsList,0));
				extList.AddRange (genAltNames ("URI", uriList,0));
				extList.AddRange (genAltNames ("IP", ipList, 0));
			
				createExtFile.writeFile (extList,certTypeBox.ActiveText);

				//Die aktuelle Serial-Nummer wird aus dem Serial File zur Speicherung ausgelesen

				using (StreamReader sr = new StreamReader (firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/serial")) {
					serial = sr.ReadLine ();
					sr.Close ();
				}

				//Die jeweilige Funktion wird bei Import/Nichtimport ausgeführt

				if (!isImport) {
					caHandling.signCert (firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/openssl.conf",
						firstSetup.mainDir + "/temp.ext", reqPath, certCaPass.Text);
				} else {
					caHandling.signCertImportedReq (firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/openssl.conf",
						firstSetup.mainDir + "/temp.ext", reqPath, certCaPass.Text, importInfo ["Commonname"],
						importInfo ["Country"], importInfo ["State"], importInfo ["Locality"], importInfo ["Organization"],
						importInfo ["Organizationunit"]);
				}

				/*Fange Openssl Errormeldungen und SQL Errormeldungen ab, wenn keine vorhanden:
				 * Eintrag in Datenbannk durch insertCert-Methode*/

				if (!caHandling.lastLine.Contains ("error")){
					try {
						insertCert ();
					} catch (SqliteException ex) {
						mWin = new msgWindow (ex.Message, "error");
					}

					mWin = new msgWindow ("Certificate: " + storageName + " signed", "succes");

					//Aktualisiere Certstore im Mainwindow

					mainWindow.clearCertStore ();
					certLoad ();
					this.Destroy ();
				} else {

					/*Da Openssl trotz Errormeldungen in manchen Fällen eine nicht korrekt codierte
					 * Datei erstellt, wird bei nichtgelingen des Erstellungs/Insertvorgangs die aktuelle
					 * Zertifikatdatei wieder gelöscht*/

					caHandling.callProc ("/bin/rm",firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certs/" +
						serial + ".pem", "Certificate deleted: Name " + serial);
					mWin = new msgWindow ("REQ already used", "error");
					mainWindow.clearCertStore ();
					certLoad ();
					this.Destroy ();
				}
			} else {
				mWin = new msgWindow ("Wrong Password", "error");
			}
		}

		protected void OnCancelCertButtonClicked (object sender, EventArgs e)
		{
			//Bei einem Abbruch der Zertifikaterstellung wird auch der dazugehörige Request gelöscht

			using (StreamReader sr = new StreamReader (firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/serial")) {
				serial = sr.ReadLine ();
				sr.Close ();
			}
			mainWindow.deleteReq (reqPath, serial);
			this.Destroy ();
		}

		public void insertCert(){
			
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/certDB.sqlite")) {
				con.Open ();
				string sql = "insert into certs (certName,certNr,certPath,caName,reqPath) values ('" + storageName + "','" + serial + "','" +
					firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certs/"+ serial +".pem','" + mainWindow.selectedCA + "','" + reqPath + "')";
				using (SqliteCommand cmd = new SqliteCommand (sql, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
		}
			
		protected void OnaddSubjAltClicked (object sender, EventArgs e)
		{
			switch (subjAltReason.ActiveText) {
			case "DNS":
				if (Regex.IsMatch (subjAltEntry.Text, regExCases.dnsRegex)) {
					dnsList.Add (subjAltEntry.Text);
					subjAltBox.AppendText ("DNS:" + subjAltEntry.Text);
					subjAltBox.Active = 0;
					subAltremove.Sensitive = true;
				} else {
					mWin = new msgWindow ("DNS not valid","error");
				}

				break;
			
			case "URI":
				if (Regex.IsMatch (subjAltEntry.Text, regExCases.uriRegex)) {
					uriList.Add (subjAltEntry.Text);
					subjAltBox.AppendText ("URI:" + subjAltEntry.Text);
					subjAltBox.Active = 0;
					subAltremove.Sensitive = true;
				} else {
					mWin = new msgWindow ("URI not valid","error");
				}

				break;

			case "eMail":
				if (Regex.IsMatch (subjAltEntry.Text, regExCases.emailRegex)) {
					mailList.Add (subjAltEntry.Text);
					subjAltBox.AppendText ("email:" + subjAltEntry.Text);
					subjAltBox.Active = 0;
					subAltremove.Sensitive = true;
				} else {
					mWin = new msgWindow ("eMail not valid","error");
				}
				break;

			case "IP":
				if (Regex.IsMatch (subjAltEntry.Text, regExCases.ipRegex)) {
					ipList.Add (subjAltEntry.Text);
					subjAltBox.AppendText ("IP:" + subjAltEntry.Text);
					subjAltBox.Active = 0;
					subAltremove.Sensitive = true;
				} else {
					mWin = new msgWindow ("IP not valid","error");
				}
				break;
			default:
				break;
			}
		}

		protected void OnSubAltremoveClicked (object sender, EventArgs e)
		{
			
			if (subjAltBox.ActiveText != null) {

				if (subjAltBox.ActiveText.Contains ("DNS")) {
					dnsList.Remove (subjAltBox.ActiveText.Substring(4));
					subjAltBox.RemoveText (subjAltBox.Active);
					subjAltBox.Active = 0;

				}
				else if (subjAltBox.ActiveText.Contains ("URI")) {
					uriList.Remove (subjAltBox.ActiveText.Substring(4));
					subjAltBox.RemoveText (subjAltBox.Active);

				}
				else if (subjAltBox.ActiveText.Contains ("email")) {
					Console.WriteLine (subjAltBox.ActiveText.Substring (6));
					mailList.Remove (subjAltBox.ActiveText.Substring(6));
					Console.WriteLine (subjAltBox.Active);
					subjAltBox.RemoveText (subjAltBox.Active);
					subjAltBox.Active = 0;

				}
				else if (subjAltBox.ActiveText.Contains ("IP")) {
					ipList.Remove (subjAltBox.ActiveText.Substring(3));
					subjAltBox.RemoveText (subjAltBox.Active);
				}
			}
			if (subjAltBox.ActiveText == null) {
				subAltremove.Sensitive = false;
			}
		}

		public static List<string> genAltNames (string type, List<string> inputList, int count){
			List<string> list = inputList.Distinct ().ToList ();
			List<string> returnList = new List<string> ();

			foreach (var s in list) {
				returnList.Add (type + "." + count + " = " + s);
				count++;
			}
			return returnList;
		}
	}
}