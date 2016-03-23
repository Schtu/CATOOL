using System;

namespace catoolgui
{
	public partial class genCRL : Gtk.Dialog
	{
		msgWindow mWin;

		public genCRL ()
		{
			this.Build ();
		}
			
		protected void OnCancelCRLClicked (object sender, EventArgs e)
		{
			this.Destroy();
		}

		protected void OnButtonGenerateClicked (object sender, EventArgs e)
		{
			if (!caPass.Text.Equals ("")) {
				caHandling.checkPass (caPass.Text);
			} else {
				mWin = new msgWindow ("Entry: Password must not be empty!", "error");
			}

			if (!caHandling.lastLine.Contains ("unable to load Private Key")) {
				caHandling.genCRL (mainWindow.selectedCA, caPass.Text);
				this.Destroy ();
			}
			else {
				mWin = new msgWindow ("Wrong CA-Password", "error");
			}
		}
	}
}