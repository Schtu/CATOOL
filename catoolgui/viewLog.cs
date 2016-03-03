using System;

namespace catoolgui
{
	public partial class viewLog : Gtk.Dialog
	{
		public viewLog ()
		{
			this.Build ();
			string logText = System.IO.File.ReadAllText (firstSetup.mainDir + "/log.txt");
			logView.Buffer.Text = logText;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

