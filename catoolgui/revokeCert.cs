using System;
using Mono.Data.Sqlite;

namespace catoolgui
{
	public partial class revokeCert : Gtk.Dialog
	{
		string certPath="", certNum ="", caPass="";
		Action certLoad;
		msgWindow mWin;
		bool delCert=false;
		certParser parser = new certParser();

		public revokeCert (string path, string num, Action certAction, bool delete)
		{
			this.Build ();
			certPath = path;
			certNum = num;
			certLoad = certAction;
			delCert = delete;
			if (delCert) {
				revokeLabel.Visible = false;
				reasonBox.Visible = false;
			}
		}


		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{

			caHandling.checkPass (revokeCAPass.Text);

			if (!caHandling.lastLine.Contains("unable to load Private Key")) {
				if (!delCert) {
					caHandling.revokeCert (mainWindow.selectedCA, certNum, revokeCAPass.Text, reasonBox.ActiveText);
					caHandling.genCRL (mainWindow.selectedCA, revokeCAPass.Text);
					mainWindow.clearCertStore ();
					mainWindow.clearInfoCertStore ();
					certLoad ();
					this.Destroy ();
				} else {
					parser.checkValid(mainWindow.selectedCA,certNum);
					if (!parser.valid.Equals ("R")) {
						caHandling.revokeCert (mainWindow.selectedCA, certNum, revokeCAPass.Text, reasonBox.ActiveText);
						caHandling.genCRL (mainWindow.selectedCA, revokeCAPass.Text);
					}
					deleteCert ();
					mainWindow.clearCertStore ();
					mainWindow.clearInfoCertStore ();
					certLoad ();
					this.Destroy ();
				}
			} else {
				mWin = new msgWindow ("Wrong CA-Password", "error");
			}

		}

		public void deleteCert(){
			using (SqliteConnection con = new SqliteConnection ("Data Source=" + firstSetup.mainDir + "/" + mainWindow.selectedCA + "-ca/certsdb/certDB.sqlite")) {
				con.Open ();
				string stm = "DELETE FROM certs WHERE certNr = '" + certNum + "'";
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
			caHandling.callProc ("/bin/rm", certPath, "Cert: " + certNum + "removed");
		}
	}
}

