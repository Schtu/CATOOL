using System;
using System.IO;
using Mono.Data.Sqlite;

namespace compactCA
{
	public partial class reqImport : Gtk.Dialog
	{
		Action reqLoad;
		Gtk.FileFilter filter = new Gtk.FileFilter();
		msgWindow mWin;

		//Setze Filter für die zu importierenden Dateien

		public reqImport (Action reqAction)
		{
			this.Build ();
			reqLoad = reqAction;
			filter.AddPattern ("*.csr");
			filter.Name = "Cert. Request";
			reqChooser.AddFilter (filter);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{

			try{
			if (reqChooser.Filename != null) {

				/*Füge den importierten Request der Datenbank hinzu und copiere diesen in den
				 * importedReq Ordner. Anschließen laden den reqStore neu.*/

				caHandling.checkImportReq(reqChooser.Filename);

				if(!caHandling.lastLine.Contains("error:0906D06C"))
					{
					insertImportedReq ();
					caHandling.callProc ("/bin/cp", reqChooser.Filename + " " + firstSetup.mainDir + "/" +
					mainWindow.selectedCA + "-ca/importedReqs/", "REQ: " + mainWindow.importedReqName + " imported");
					mainWindow.clearREQStore ();
					mainWindow.clearREQInfoStore ();
					reqLoad ();
					mWin = new msgWindow("Request: " + getFilename() + " imported","succes");
					this.Destroy ();
					}
					else{
						mWin = new msgWindow ("Request: " + getFilename() + " must be in PEM-Format","error");
					}
			} else {
				mWin = new msgWindow ("No Request for import selected", "error");
			}
			}
			catch(SqliteException ex){
				mWin = new msgWindow (ex.Message,"error");
				return;
			}
		}

		public void insertImportedReq(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA +"-ca/certsdb/importReqsDB.sqlite")) {
				con.Open ();
				string stm = "insert into importReqs (reqName, caName, reqPath) values ('" +
					getFilename () + "','" + mainWindow.selectedCA + "','" + firstSetup.mainDir + "/" +
					mainWindow.selectedCA + "-ca/importedReqs/" + getFilename() + ".csr')";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
		}

		string getFilename(){
			return System.IO.Path.GetFileNameWithoutExtension (reqChooser.Filename);
		}
	}
}