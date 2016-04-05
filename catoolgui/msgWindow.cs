using System;
using System.Collections.Generic;

namespace compactCA
{
	public partial class msgWindow : Gtk.Dialog
	{
		string msg ="";

		//Konstrukter zum darstellen von mehrer Meldungen als Liste

		public msgWindow (List<string> input, string reason)
		{
			this.Build ();
			foreach (var i in input) {
				msg += "\t" + i + "\t\n";
			}

			setlabel (msg, reason);
		}

		//Konstruktor zum darstellen einzelner Meldungen

		public msgWindow(string msg, string reason){
			this.Build ();
			setlabel (msg, reason);
		}

		//Inhalt des Messagedialogs

		public void setlabel(string msg, string reason){

			if (reason.Equals ("error")) {
				msgText.Text = "\t<b>The following erros occured:\n\n" + msg + "</b>\t";
				msgText.UseMarkup = true;
				this.Title = "Warning";
				return;
			} else {
				msgText.Text = "<b>" + msg + "</b>";
				msgText.UseMarkup = true;
				this.Title = "Success";
				return;
			}
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}