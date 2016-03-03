using System;
using System.Text.RegularExpressions;

namespace catoolgui
{
	public partial class crlExt : Gtk.Window
	{
		public static string crlURL="";
		msgWindow mWin;

		public crlExt () :
			base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			crlURIEntry.Text = crlURL;
			if (!crlURL.Equals ("")) {
				crlAdd.Sensitive = false;
				crlDel.Sensitive = true;
			}
		}

		protected void OnCrlAddClicked (object sender, EventArgs e)
		{
			if (Regex.IsMatch (crlURIEntry.Text, regExCases.uriRegex)) {

				createNewCA.usrList.Add ("crlDistributionPoints = URI:" + crlURIEntry.Text);
				createNewCA.v3reqList.Add ("crlDistributionPoints = URI:" + crlURIEntry.Text);
				createNewCA.v3caList.Add ("crlDistributionPoints = URI:" + crlURIEntry.Text);
				createNewCA.crlList.Add ("crlDistributionPoints = URI:" + crlURIEntry.Text);
				crlURIEntry.IsEditable = false;
				crlURL = crlURIEntry.Text;
				crlLabel.Text = "URL added";
				crlDel.Sensitive = true;
				crlAdd.Sensitive = false;

			}
			else{
				mWin = new msgWindow("URI in CRLDistributionpoint not valid","error");
				return;
			}
		}

		protected void OnCrlDelClicked (object sender, EventArgs e)
		{	
				crlLabel.Text = "URL deleted";
				crlURL = "";
				crlURIEntry.Text = "";
				crlURIEntry.IsEditable = true;
				crlAdd.Sensitive = true;
				crlDel.Sensitive = false;
				createNewCA.usrList.RemoveAll (a => a.Contains("URI:"));
				createNewCA.v3reqList.RemoveAll (a => a.Contains("URI:"));
				createNewCA.v3caList.RemoveAll (a => a.Contains("URI:"));
				createNewCA.crlList.RemoveAll (a => a.Contains("URI:"));
		}

		protected void OnCrlWinCloseClicked (object sender, EventArgs e)
		{
			if (Regex.IsMatch (crlDays.Text, regExCases.validRegex)) {
				this.Destroy ();
				createNewCA.crlDays = crlDays.Text;
			} else {
				mWin = new msgWindow ("Entry: Days must contain 1 up to 4 decimal numbers", "error");
			}
		}
	}
}

