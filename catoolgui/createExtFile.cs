using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace catoolgui
{
	public class createExtFile
	{
		static StreamWriter swriter;
		static string certType;

		/* Eigentlich werden alle Extensions die die CA den Zertifikaten zur Verfügung stellen
		 * soll bereits im Configfile festgelegt. Um das dynamische erweitern von z.Bsp. 
		 * SubjectAltNames zu ermöglichen, werden bei die zertifizierung nur die Extensions 
		 * aus dem Extfile betrachtet. Damit kann der Inhalt der Extensions individuell beim
		 * jeweiligen Zertifikat angepasst werden.*/


		public static void writeFile(List<string> extlist, string usage){
			certType = usage;
			List<string> list = extlist.Distinct ().ToList ();

			caHandling.callProc("/usr/bin/touch", firstSetup.mainDir + "/temp.ext", "Extension File created");

			string path = Path.Combine (firstSetup.mainDir, "temp.ext");
			swriter = new StreamWriter (path, true);

			//Standartextensions

			swriter.WriteLine ("basicConstraints = CA:FALSE");
			swriter.WriteLine ("subjectKeyIdentifier = hash");
			swriter.WriteLine ("authorityKeyIdentifier = keyid");

			//Legt Zertifikatstyp fest

			if (certType.Equals ("Clientcertificate")) {
				swriter.WriteLine ("extendedKeyUsage = clientAuth");
			} else {
				swriter.WriteLine ("extendedKeyUsage = serverAuth");
			}
				
			//Setze SubjectAltNames

			foreach (var i in list) {
				swriter.WriteLine (i);
			}
			swriter.Close ();
			extlist.Clear ();
		}
	}
}