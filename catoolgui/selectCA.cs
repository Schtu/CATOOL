using System;
using Mono.Data.Sqlite;

namespace catoolgui
{
	public partial class selectCA : Gtk.Dialog
	{
		Action  loadCA,loadReq, loadCert, loadLabel;
		String reason;


		/*Lademethoden des mainWindoe werden als Action übergeben und bei der Auswahl einer CA ausgeführt.
		 * Außerdem werden alle erstellten CA in eine Auswahlbox geladen*/

		public selectCA (string input,Action caAction, Action reqAction, Action certAction, Action labelCA) 
		{
			loadCA = caAction;
			loadReq = reqAction;
			loadCert = certAction;
			loadLabel = labelCA;
			reason = input;
			this.Build ();
			readDB ();
		}
		
		public void readDB(){
			using (SqliteConnection con = new SqliteConnection ("Data Source="+ firstSetup.mainDir + "/caDB.sqlite")) {
				con.Open ();
				string stm = "SELECT caName FROM CA";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					using (SqliteDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							caBox.AppendText(rdr.GetString(0));
						}
					}
				}
				caBox.Active = 0;
				con.Close ();
			}
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
			caBox.Clear ();
		}

		// Dieser Dialog wird sowohl zum laden als auch zum löschen von CA's verwendet
		 


		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (reason.Equals("open") && caBox.ActiveText != null) {
				mainWindow.selectedCA = caBox.ActiveText;
				caBox.Clear ();
				mainWindow.clearCAStore ();
				mainWindow.clearREQStore ();
				mainWindow.clearCertStore ();
				loadCA ();
				loadReq ();
				loadCert ();
				this.Destroy ();
			} 
			if(reason.Equals("delete") && caBox.ActiveText != null){
				mainWindow.deletedCA = caBox.ActiveText;
				caBox.Clear ();
				if (mainWindow.selectedCA.Equals (caBox.ActiveText)) {
					mainWindow.deleteCA ();
					mainWindow.clearCAStore ();
					mainWindow.clearREQStore ();
					mainWindow.clearCertStore ();
					mainWindow.selectedCA = "";
					loadLabel ();
				} else {
					mainWindow.deleteCA ();
				}
				this.Destroy ();
			}
		}
	}
}