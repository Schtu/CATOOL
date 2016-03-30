using System;
using System.IO;

namespace catoolgui
{
	public partial class viewLog : Gtk.Dialog
	{
		msgWindow mWin;
		string logText,certText;
		public viewLog (string path, bool cert)
		{
			this.Build ();
			try{
				if(cert){
					using (StreamReader sr = new StreamReader (path)){
						certText = sr.ReadToEnd();
						logView.Buffer.Text = certText;
					}
				}
				else{
					logText = File.ReadAllText (path);
					logView.Buffer.Text = logText;
				}
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

		public viewLog(string text){
			this.Build ();
			logView.Buffer.Text = text;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}