using System;
using System.IO;

namespace catoolgui
{
	public partial class viewLog : Gtk.Dialog
	{
		msgWindow mWin;

		public viewLog ()
		{
			this.Build ();
			try{
			string logText = File.ReadAllText (firstSetup.mainDir + "/log.txt");
			logView.Buffer.Text = logText;
			}
			catch(FileNotFoundException e1){
				mWin = new msgWindow (e1.Message, "error");
				this.Destroy ();
			}
			catch(FileLoadException e2){
				mWin = new msgWindow (e2.Message, "error");
				this.Destroy ();
			}
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}