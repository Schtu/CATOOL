using System;
using Gtk;

namespace catoolgui
{
	public partial class importRequest : Gtk.Dialog
	{
		FileFilter filter = new FileFilter();
		System.Action loadReq;

		public importRequest (System.Action reqAction)
		{
			this.Build ();
			loadReq = reqAction;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			
		}
	}
}

