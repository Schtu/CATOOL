using System;
using Mono.Data.Sqlite;
using Gtk;
using System.IO;

namespace catoolgui
{
	public partial class mainWindow : Gtk.Window
	{
		createNewCA caWin;
		selectCA sWin;
		createNewRequest rWin;
		createCert cWin;
		exportCert exWIn;
		exportCACert excaWin;
		reqImport imWin;
		crlScript crlWin;
		static revokeCert rvWin;
		public static String selectedCA="", deletedCA = "",  selectedCert = "",
		selectedCertNum="", selectedCertPath = "",importedReqPath = "", importedReqName="";
		public int checkCaNum;

		static ListStore  reqStore,usedreqStore,certStore;
		static TreeStore caStore,reqInfoStore,usedreqInfoStore,certInfoStore;
		static TreeViewColumn caCategory,caValues, reqName, reqInfoCat, 
		reqInfoVal, certName, certNo, certvalid, certInfoCat, certInfoVal;
	
		CellRendererText cellRend;
		TreeIter iter,caIter,reqInfoIter,certInfoIter;
		TreeModel model;

		public static certParser parser = new certParser();

		public mainWindow () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			DeleteEvent += delegate {
				MainClass.terminateApp();
			};
				
			//Prüfe ob bereits CA's vorhanden sind anhand eines count()

			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/caDB.sqlite")) {
				con.Open ();
				string stm = "SELECT count(caName) FROM CA";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					checkCaNum = Convert.ToInt32 (cmd.ExecuteScalar ());
				}
				con.Close ();
			}

			//Wenn ja, kann beim Programm eine dieser CA's ausgewählt werden

			if (checkCaNum > 0) {
				sWin = new selectCA ("open",stateLoadCA,stateLoadReq,stateLoadCert,setLabel);
			}

			//Lege die einzelnen Tree-/Liststores und deren Spalten zur Darstellung der CA, Reqs. und Certs. an.

			mainNotebook.CurrentPage = 0;
			cellRend = new CellRendererText ();
			cellRend.Xalign = 0.0f;
			cellRend.Editable = true;
			caStore = new TreeStore (typeof(string),typeof(string));
			reqStore = new ListStore (typeof(string));
			reqInfoStore = new TreeStore (typeof(string), typeof(string));
			certStore = new ListStore (typeof(string) ,typeof(string),typeof(string));
			certInfoStore = new TreeStore (typeof(string), typeof(string));

			makeTreeColumn (caCategory, caStore, cellRend, 0, "Categorys", caTreeView);
			makeTreeColumn (caValues, caStore, cellRend, 1, "Values", caTreeView);

			makeListColumn (reqName, reqStore, cellRend, 0, "Imported Requests", reqTreeView);

			makeTreeColumn (reqInfoCat, reqInfoStore, cellRend, 0, "Categorys", reqInfoTreeView);
			makeTreeColumn (reqInfoVal, reqInfoStore, cellRend, 1, "Values", reqInfoTreeView);

			makeListColumn (certName, certStore, cellRend, 0, "Name", certTreeView);
			makeListColumn (certNo, certStore, cellRend, 1, "Serial", certTreeView);
			makeListColumn (certvalid, certStore, cellRend, 2, "Valid", certTreeView);

			makeTreeColumn (certInfoCat, certInfoStore, cellRend, 0, "Categorys", infoCertTreeView);
			makeTreeColumn (certInfoVal, certInfoStore, cellRend, 1, "Values", infoCertTreeView);
		


			caTreeView.Model = caStore;
			reqTreeView.Model = reqStore;
			reqInfoTreeView.Model = reqInfoStore;
			certTreeView.Model = certStore;
			infoCertTreeView.Model = certInfoStore;
		}

		//Funktionen zur Erstellung von Listcolumns

		public void makeListColumn(TreeViewColumn col, ListStore store, 
			CellRendererText cell, int colNum, string title, TreeView view){
			col = new TreeViewColumn ();
			col.PackStart (cell, true);
			col.Title = title;
			col.AddAttribute (cell, "text", colNum);
			view.AppendColumn (col);
		}

		//Funktionen zur Erstellung von Treecolumns

		public void makeTreeColumn(TreeViewColumn col, TreeStore store, 
			CellRendererText cell, int colNum, string title, TreeView view){
			col = new TreeViewColumn ();
			col.PackStart (cell, true);
			col.Title = title;
			col.AddAttribute (cell, "text", colNum);
			view.AppendColumn (col);
		}
			

		//Lade CA Informationen

		public void stateLoadCA(){

			mainNotebook.Sensitive = true;
			exportCA.Sensitive = true;
			publishCRL.Sensitive = true;
			CAlabel.Visible = true;
			mainNotebook.CurrentPage = 0;

			//Lade den Path vom CA-Zertifikat und rufe damit den Parser auf

			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/caDB.sqlite")) {
				con.Open ();
				string sql = "SELECT path FROM CA WHERE caName = '" + selectedCA + "'";
				using (SqliteCommand cmd = new SqliteCommand (sql, con)) {
					parser.readAll (cmd.ExecuteScalar ().ToString ());

					//Fürge dem caStore die gewünschten Informatioen hinzu

					caIter = caStore.AppendValues ("Issuer Data");
					caStore.AppendValues (caIter, "CommonName", parser.icommonName);
					caStore.AppendValues (caIter, "Country", parser.iCountry);
					caStore.AppendValues (caIter, "State", parser.istate);
					caStore.AppendValues (caIter, "Locality", parser.ilocal);
					caStore.AppendValues (caIter, "Organization", parser.iorga);
					caStore.AppendValues (caIter, "OrganizationUnit", parser.iorgaUnit);


					caIter = caStore.AppendValues ("Security Data");
					caStore.AppendValues (caIter, "Sign. Algorithm", parser.sigAlgo);
					caStore.AppendValues (caIter, "Pub. Key Algorithm", parser.pubKeyAlgo);
					caStore.AppendValues (caIter, "Valid from", parser.notBefore);
					caStore.AppendValues (caIter, "Valid until", parser.notAfter);
					caHandling.startBash("openssl x509 -in " + firstSetup.mainDir + "/" +
						selectedCA + "-ca/certs/1000.pem -noout -fingerprint","");
					caStore.AppendValues (caIter, "Fingerprint", caHandling.getInfo);

					caIter = caStore.AppendValues ("Extensions");
					caStore.AppendValues (caIter, "Basic Constraints", parser.basic);
					caStore.AppendValues (caIter, "Subject Key Identifier", parser.subKey);
					caStore.AppendValues (caIter, "Authority Key Identifier", parser.authKey);
					caStore.AppendValues (caIter, "Key Usage", parser.keyusage);
					if(!parser.crlUrl.Equals(""))
					caStore.AppendValues (caIter, "CRL - Distributionpoint", parser.crlUrl);
					foreach (var i in parser.subjAltList) {
						caStore.AppendValues (caIter, "SubjectAltName", i);
					}

					selCA.Text = selectedCA;
				}
				con.Close ();
			}
		}

		//Lade Informationen von importierten Requests

		public static void stateLoadReq(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/importReqsDB.sqlite")) {
				con.Open ();
				string stm = "SELECT * FROM importReqs WHERE caName = '" + selectedCA + "'";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					using (SqliteDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							reqStore.AppendValues (rdr.GetString (0));
						}
					}
				}
				con.Close ();
			}
		}

		//Lade Informationen von erstellten Zertifikaten

		public static void stateLoadCert(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/certDB.sqlite")) {
				con.Open ();
				string stm = "SELECT * FROM certs WHERE caName = '" + selectedCA + "'";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					using (SqliteDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							parser.checkValid (rdr.GetString (3), rdr.GetString (1));
							certStore.AppendValues (rdr.GetString(0),rdr.GetString(1),parser.valid);
						}
					}
				}
				con.Close ();
			}
		}

		//Funktionen zum leeren der einzelnen Infostores

		public static void clearCAStore(){
			caStore.Clear ();
		}

		public static void clearREQStore(){
			reqStore.Clear ();
		}

		public static void clearREQInfoStore(){
			reqInfoStore.Clear ();
		}

		public static void clearCertStore(){
			certStore.Clear ();
		}
			
		public static void clearInfoCertStore(){
			certInfoStore.Clear ();
		}

		public void setLabel(){
			selCA.Text = "";
		}

		//Löschfunktionen

		public static void deleteCA(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/caDB.sqlite")) {
				con.Open ();
				string stm = "DELETE FROM CA WHERE caName = '" + deletedCA + "'";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				caHandling.callProc ("/bin/rm","-r " + firstSetup.mainDir + "/" + deletedCA + "-ca", "CA deleted");
				con.Close ();
			}
		}

		public static void deleteReq(string reqPath, string serial){

				using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certsdb/reqDB.sqlite")) {
					con.Open ();
					string stm = "DELETE FROM reqs WHERE reqPath = '" + reqPath + "'";
					using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
						cmd.ExecuteNonQuery ();
					}
					caHandling.callProc ("/bin/rm", reqPath, " REQ deleted");
					caHandling.callProc ("/bin/rm", reqPath.Replace(".csr",".key"), " REQ - Key deleted");
					con.Close ();
				}
		}

		protected void OnDeleteimportedReqClicked (object sender, EventArgs e)
		{
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certsdb/importReqsDB.sqlite")) {
				con.Open ();
				string stm = "DELETE FROM importReqs WHERE reqPath = '" + importedReqPath + "'";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				caHandling.callProc ("/bin/rm", firstSetup.mainDir + "/" + selectedCA + "-ca/importedReqs/" + importedReqName + ".csr", " REQ: " + importedReqName + " deleted");
				con.Close ();
				clearREQStore ();
				stateLoadReq ();
			}
		}
			
		//GUI- Funktionen zum Erstellen neuer CA's

		protected void OnCreateCAButtonClicked (object sender, EventArgs e)
		{
			caWin = new createNewCA (stateLoadCA);
		}

		protected void OnNewCAActionActivated (object sender, EventArgs e)
		{
			caWin = new createNewCA (stateLoadCA);
		}

		//GUI-Funktion zum löschen von CA's

		protected void OnDeleteCAActionActivated (object sender, EventArgs e)
		{
			sWin = new selectCA ("delete",stateLoadCA,stateLoadReq,stateLoadCert,setLabel);
		}
			
		//GUI-Funktion zum Laden von CA's

		protected void OnOpenCAActionActivated (object sender, EventArgs e)
		{
			sWin = new selectCA ("open",stateLoadCA,stateLoadReq,stateLoadCert,setLabel);
		}

		//Öffne Exportdialog für CA's

		protected void OnExportCAClicked (object sender, EventArgs e)
		{
			excaWin = new exportCACert ();
		}

		protected void OnExitActionActivated (object sender, EventArgs e)
		{
			MainClass.terminateApp ();
		}
			
		//GUI-Funktion zum erstellen eines neuen Zertifikates

		protected void OnCreateCertButtonClicked (object sender, EventArgs e)
		{
			rWin = new createNewRequest (selectedCA,stateLoadCert);
		}

		//GUI-Funktion zum erstellen eines neuen Zertifikates aus einem importierten Request

		protected void OnCreateCert2Clicked (object sender, EventArgs e)
		{
			rWin = new createNewRequest (selectedCA,stateLoadCert);
		}

		/* Soll ein Zertifikat gelöscht werden, sollte dafür bestenfalls immer ein Grund angeben werden.
		 * Weiterhin sollte dieses Zertifikat auch widerrufen werden. Aus diesen Gründen wird ein Fenster
		 * geöffnet wo Grund der Löschung angegeben werden muss.*/

		protected void OnDelCertClicked (object sender, EventArgs e)
		{
			if (!selectedCertNum.Equals ("")) {
				rvWin = new catoolgui.revokeCert (selectedCertPath, selectedCertNum, stateLoadCert, true);
			}
		}

		//Öffne Export Dialog für Zertifikate

		protected void OnExportCertClicked (object sender, EventArgs e)
		{
			if(!selectedCertPath.Equals(""))
				exWIn = new exportCert (selectedCertPath,selectedCertNum);
		}

		//Öffne Revoke Dialog für Zertifikate

		protected void OnRevokeCertClicked (object sender, EventArgs e)
		{
			if (!selectedCertPath.Equals (""))
				rvWin = new revokeCert (selectedCertPath,selectedCertNum,stateLoadCert,false);
		}

		//Öffne Importdialog für Requests

		protected void OnImportButtonClicked (object sender, EventArgs e)
		{
			imWin = new reqImport (stateLoadReq);
		}

		//Öffne Dialog zum Erstellen von Zertifikaten

		protected void OnCreateCertfromReqButtonClicked (object sender, EventArgs e)
		{
			rWin = new createNewRequest (selectedCA, stateLoadCert, importedReqPath);
		}

		//Öffne Dialog zur Veröffentlichung der CRL

		protected void OnPublishCRLClicked (object sender, EventArgs e)
		{
			crlWin = new crlScript ();
		}

		//Bestimme ausgewählten importierten Request aus dem reqStore

		protected void OnReqTreeViewCursorChanged (object sender, EventArgs e)
		{
			reqInfoStore.Clear ();

			TreeSelection selection = (sender as TreeView).Selection;

			if(selection.GetSelected(out model, out iter)){

				createCertfromReqButton.Sensitive = true;

				/*Lese anhand des Pfads des importieren Request Informationen aus, lade diese in den Parser
				 * und stelle sie im ReqInfoStore dar*/

				using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/importReqsDB.sqlite")) {
					con.Open ();
					string stm = "SELECT reqPath FROM importReqs WHERE reqName = '" + reqStore.GetValue (iter, 0).ToString () + "'";
					using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
						importedReqPath = cmd.ExecuteScalar ().ToString ();
						importedReqName = reqStore.GetValue (iter, 0).ToString();
						parser.readAll (importedReqPath);

						reqInfoIter = reqInfoStore.AppendValues ("Subject");
						reqInfoStore.AppendValues (reqInfoIter, "CommonName",parser.scommonName);
						reqInfoStore.AppendValues (reqInfoIter, "Country", parser.sCountry);
						reqInfoStore.AppendValues (reqInfoIter, "State", parser.sstate);
						reqInfoStore.AppendValues (reqInfoIter, "Locality", parser.slocal);
						reqInfoStore.AppendValues (reqInfoIter, "Organization", parser.sorga);
						reqInfoStore.AppendValues (reqInfoIter, "OrganizationUnit", parser.sorgaUnit);
						reqInfoStore.AppendValues (reqInfoIter, "Country", parser.sCountry);

						reqInfoIter = reqInfoStore.AppendValues ("Security Data");
						reqInfoStore.AppendValues (reqInfoIter, "Signature Algorithm", parser.sigAlgo);
						reqInfoStore.AppendValues (reqInfoIter, "Pub. Key Algorithm", parser.pubKeyAlgo);
						reqInfoStore.AppendValues (reqInfoIter, "Valid from", parser.notBefore);
						reqInfoStore.AppendValues (reqInfoIter, "Valid until", parser.notAfter);
					}
					con.Close ();
				}
			}
		}
			
		//Lade Informationen des ausgewählten Zertifikates in den CertInfoStore

		protected void OnCertTreeViewCursorChanged (object sender, EventArgs e)
		{
			TreeSelection selection = (sender as TreeView).Selection;
			certInfoStore.Clear ();

			createCert2.Sensitive = true;
			revokeCert.Sensitive = true;
			exportCert.Sensitive = true;
			delCert.Sensitive = true;

			if (selection.GetSelected (out model, out iter)) {

				if (certStore.GetValue (iter, 2).ToString ().Equals ("R")) {
					revokeCert.Sensitive = false;
					exportCert.Sensitive = false;
				}

				using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/certDB.sqlite")) {
					con.Open ();
					string stm = "SELECT certPath FROM certs WHERE certName = '" + certStore.GetValue (iter, 0).ToString () + "'";         
					using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
						selectedCertPath = cmd.ExecuteScalar ().ToString ();
					}
					con.Close ();
				}
				selectedCertNum = certStore.GetValue (iter, 1).ToString ();
				selectedCert = certStore.GetValue (iter, 0).ToString ();
				parser.readAll (selectedCertPath);

				certInfoIter = certInfoStore.AppendValues ("Issuer");
				certInfoStore.AppendValues (certInfoIter, "CommonName", parser.icommonName);
				certInfoStore.AppendValues (certInfoIter, "Country", parser.iCountry);
				certInfoStore.AppendValues (certInfoIter, "State", parser.istate);
				certInfoStore.AppendValues (certInfoIter, "Locality", parser.ilocal);
				certInfoStore.AppendValues (certInfoIter, "Organization", parser.iorga);
				certInfoStore.AppendValues (certInfoIter, "OrganizationUnit", parser.iorgaUnit);
				certInfoStore.AppendValues (certInfoIter, "Country", parser.iCountry);

				certInfoIter = certInfoStore.AppendValues ("Subject");
				certInfoStore.AppendValues (certInfoIter, "CommonName", parser.scommonName);
				certInfoStore.AppendValues (certInfoIter, "Country",parser.sCountry);
				certInfoStore.AppendValues (certInfoIter, "State", parser.sstate);
				certInfoStore.AppendValues (certInfoIter, "Locality", parser.slocal);
				certInfoStore.AppendValues (certInfoIter, "Organization", parser.sorga);
				certInfoStore.AppendValues (certInfoIter, "OrganizationUnit", parser.sorgaUnit);

				certInfoIter = certInfoStore.AppendValues ("Security Data");
				certInfoStore.AppendValues (certInfoIter, "Signature Algorithm", parser.sigAlgo);
				certInfoStore.AppendValues (certInfoIter, "Pub. Key Algorithm", parser.pubKeyAlgo);
				certInfoStore.AppendValues (certInfoIter, "Signature Algorithm", parser.sigAlgo);
				certInfoStore.AppendValues (certInfoIter, "Valid from", parser.notBefore);
				certInfoStore.AppendValues (certInfoIter, "Valid until", parser.notAfter);
				caHandling.startBash ("openssl x509 -in " + selectedCertPath + " -noout -fingerprint", "");
				certInfoStore.AppendValues (certInfoIter, "Fingerprint", caHandling.getInfo);

				certInfoIter = certInfoStore.AppendValues ("Extensions");
				certInfoStore.AppendValues (certInfoIter, "Basic Constraints", parser.basic);
				certInfoStore.AppendValues (certInfoIter, "Subject Key Identifier", parser.subKey);
				certInfoStore.AppendValues (certInfoIter, "Authority Key Identifier", parser.authKey);
				certInfoStore.AppendValues (certInfoIter, "ExtendedKey Usage", parser.certUsage);
				if (!parser.crlUrl.Equals ("")) {
					certInfoStore.AppendValues (certInfoIter, "CRL - Distributionpoint", parser.crlUrl);
				}
				foreach (var i in parser.subjAltList) {
					certInfoStore.AppendValues (certInfoIter, "SubjectAltName", i);
				}
			}
		}
	}
}