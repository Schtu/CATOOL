using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace catoolgui
{
	public class certParser
	{
		public string basic,subKey,authKey,certUsage,keyusage,crlUrl,sigAlgo,iCountry,istate,ilocal,iorga,
		iorgaUnit,icommonName,notBefore,notAfter, pubKeyAlgo, sCountry,sstate, slocal,sorga,
		sorgaUnit,scommonName,valid;
		public List<string> subjAltList = new List<string> ();
		msgWindow mWin;

		//Lese die zu parsende Datei ein

		public void readAll(string path){

			subjAltList.Clear ();

			if (File.Exists(path)) {
					string s;
				using (StreamReader sr = new StreamReader (path)) {

					/*Da Requests nicht im Plaintext eingelesen werden können, wird auf den 
					 * Bashoutput zurück gegriffen*/
					
					if (path.Contains (".csr")) {
						caHandling.startBash ("openssl req -in " + path + " -text", "readreq");
						s = caHandling.getInfo;
					} else {
						s = sr.ReadToEnd ();
					}

					//Die einzelnen Bestandteile eines Cert./Req. werden per Regex ausgelesen

					Match match = Regex.Match (s, @"X509v3 Basic Constraints:\s*(\n|\r|\r\n|\bcritical\b)?\s*(?<Basic>.*)");
					this.basic = (match.Groups ["Basic"].Value);

					match = Regex.Match (s, @"X509v3 Subject Key Identifier:\s*(\n|\r|\r\n)?\s*(?<subKey>.*)");
					this.subKey = (match.Groups ["subKey"].Value);

					match = Regex.Match (s, @"X509v3 Authority Key Identifier:\s*(\n|\r|\r\n)?\s*(?<authKey>.*)");
					this.authKey = (match.Groups ["authKey"].Value);

					match = Regex.Match (s, @"X509v3 Subject Alternative Name:\s*(\n|\r|\r\n)?\s*(?<subjAlt>.*)");
					foreach(Match m in Regex.Matches(match.Groups ["subjAlt"].Value,@"(?<subjaltNames>((\bURI:\b.*?)|(\bDNS:\b.*?)|(\bemail:\b.*?)|(\bIP Address:\b).*?))(, |$)")){
						this.subjAltList.Add (m.Groups ["subjaltNames"].Value);
					}

					match = Regex.Match (s, @"X509v3 Key Usage:\s*(\n|\r|\r\n|\s*)(?<keyusage>.*)");
					this.keyusage = (match.Groups ["keyusage"].Value);

					match = Regex.Match (s, @"X509v3 Extended Key Usage:\s*(\n|\r|\r\n|\s*)(?<usage>.*)");
					this.certUsage = (match.Groups ["usage"].Value);

					match = Regex.Match (s, @"X509v3 CRL Distribution Points:(\s*|(\n|\r|\r\n)|Full Name:)*URI:(?<crlUrl>.*)");
					this.crlUrl = (match.Groups ["crlUrl"].Value);

					match = Regex.Match (s, @"Signature Algorithm: (?<sigAlgo>.*)");
					this.sigAlgo = (match.Groups ["sigAlgo"].Value);

					match = Regex.Match (s, @"Issuer: C=(?<iCountry>.*), ST=(?<istate>.*?)(, L=(?<ilocal>.*))?, O=(?<iorga>.*?)(, OU=(?<iorgaUnit>.*))?, CN=(?<icommonName>.*)");
					this.iCountry = match.Groups ["iCountry"].Value;
					this.istate = match.Groups ["istate"].Value;
					this.ilocal = match.Groups ["ilocal"].Value;
					this.iorga = match.Groups ["iorga"].Value;
					this.iorgaUnit = match.Groups ["iorgaUnit"].Value;
					this.icommonName = match.Groups ["icommonName"].Value;

					match = Regex.Match (s, @"Not Before: (?<notBefore>.*)");
					this.notBefore = (match.Groups ["notBefore"].Value);

					match = Regex.Match (s, @"Not After : (?<notAfter>.*)");
					this.notAfter = (match.Groups ["notAfter"].Value);

					match = Regex.Match (s, @"Public Key Algorithm: (?<pubKeyAlgo>.*)");
					this.pubKeyAlgo = (match.Groups ["pubKeyAlgo"].Value);
					match = Regex.Match (s, @"Subject: C=(?<sCountry>.*), ST=(?<sstate>.*?)(, L=(?<slocal>.*))?, O=(?<sorga>.*?)(, OU=(?<sorgaUnit>.*))?, CN=(?<scommonName>.*)");
					this.sCountry = match.Groups ["sCountry"].Value;
					this.sstate = match.Groups ["sstate"].Value;
					this.slocal = match.Groups ["slocal"].Value;
					this.sorga = match.Groups ["sorga"].Value;
					this.sorgaUnit = match.Groups ["sorgaUnit"].Value;
					this.scommonName = match.Groups ["scommonName"].Value;
				}
			} else {
				mWin = new msgWindow ("PARSER: Path does not exist", "error");
			}
		}

		/* Da die Information ob ein Zertifikat gültig ist oder widerrufen wurde nicht im 
		 * Zertifikat selber, sondern in der index.txt der jeweiligen CA aufgeführt wird,
		 * dient diese Methode dazu den Status des Zertifikats auszulesen*/

		public void checkValid (string selectedCa, string serial){

			using (StreamReader sr = new StreamReader (firstSetup.mainDir + "/" + selectedCa + "-ca/index.txt")) {
				string s = sr.ReadToEnd ();

				Match match = Regex.Match (s, @"(?<valid>[V|E|R]).*"+serial);
				this.valid = ((match.Groups ["valid"].Value));
			}
		}
	}
}