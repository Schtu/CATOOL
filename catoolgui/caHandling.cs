using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace catoolgui
{
	public class caHandling
	{
		
		static string[] directorysToCreate = new string[] {"certsdb",
			"certreqs", "importedReqs", "certs", "crl", "private"};
		static msgWindow mWindow;
		static List<string> logLines = new List<string> ();
		static Process procBash = new System.Diagnostics.Process ();
		public static string getInfo="", lastLine="";

		/*Starte Prozess mit Befehl cmd (z.Bsp. /usr/bin/openssl), Argument (z.Bsp x509 -in ....)
		 * und reason (Eintrag fürs Logfile). Da Openssl den Großteil der Informationen über den 
		 * Errorstream leitet wird hier nur dieser delegiert um den Output abfangen zu können.
		 * Anschließen wird der Output in das Logfile geschrieben.*/

		public static void callProc(String cmd, String argument, string reason){
			logLines.Clear ();
			lastLine = "";
			Process proc = new System.Diagnostics.Process ();
			proc.StartInfo.FileName = cmd;
			proc.StartInfo.Arguments = argument;
			proc.StartInfo.UseShellExecute = false; 
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.EnableRaisingEvents = false;
			proc.ErrorDataReceived += captureError;
			proc.Start ();
			proc.BeginErrorReadLine ();
			proc.WaitForExit ();
			if (proc.HasExited) {
				writeLog (reason,logLines);
				proc.Dispose ();
			}
		}

		/* Um den wenigen Output von Openssl abfangen zu können, welcher nicht über den Errorstream
		 * gesendet wird, greife ich hier auf die normale Bash zurück. Beim auslesen von Requests, welche
		 * welche für den Parser im Plaintext benötigt werden, wird hier auf den Output der Bash zurückge-
		 * griffen.*/

		public static void startBash(string cmd, string reason){
			procBash.StartInfo.FileName = "/bin/bash";
			procBash.StartInfo.UseShellExecute = false; 
			procBash.StartInfo.RedirectStandardOutput = true;
			procBash.StartInfo.RedirectStandardInput = true;
			procBash.Start();
			procBash.StandardInput.WriteLine (cmd);

			if (reason.Equals ("readreq")) {
				getInfo = "";
				while (!getInfo.Contains ("-----END CERTIFICATE REQUEST-----")) {
					getInfo += procBash.StandardOutput.ReadLine () + "\n";
				}
			}
			else {
				getInfo = "";
				getInfo += procBash.StandardOutput.ReadLine ();
			}
			procBash.Kill ();
		}
			
		/* Der Output vom Errorstream wird jeweils in eine Liste (loglines) für das Logfile
		 * geschrieben und die jeweils letzte eingetragene wird seperat gespeichert, um auf
		 * Openssl Fehlermeldungen reagieren zu können*/

		public static void captureError(object sender, DataReceivedEventArgs e){

			logLines.Add(e.Data);
			lastLine += e.Data;
		}
			
		public static void createDirectorys(string caName){

			caName = caName.Replace (" ", string.Empty);

			try{
				if (!Directory.Exists(firstSetup.mainDir +"/"+caName+"-ca")) {

					Directory.CreateDirectory (firstSetup.mainDir+"/"+caName+"-ca");
					DirectoryInfo di = new DirectoryInfo (firstSetup.mainDir+"/"+caName+"-ca");

					for (int i = 0; i < directorysToCreate.Length; i++) {
						di.CreateSubdirectory (directorysToCreate [i]);
					}
					callProc("/usr/bin/touch",firstSetup.mainDir +"/"+caName+"-ca/openssl.conf", "Config File Written");
					callProc("/usr/bin/touch",firstSetup.mainDir +"/"+caName+"-ca/index.txt","Index File Written");
					File.WriteAllText(firstSetup.mainDir + "/"+caName+"-ca/serial","1000");
					File.WriteAllText(firstSetup.mainDir + "/"+caName+"-ca/crlnumber","1000");
				} 
				else{
				return;
				}
			}
			catch(IOException exIO){
				mWindow = new msgWindow (exIO.Message,"error");
			}
		}
			
		//RSA Schlüsselgenerierung für die CA, mit Trennung in Public/Private - Key

		public static void createRSACAKey(string caName, string cipherAlgo, string passphrase, string bits){
			
			callProc ("/usr/bin/openssl", "genrsa " + "-" + cipherAlgo + " -passout pass:" + passphrase + " -out " + firstSetup.mainDir + "/" +
			caName + "-ca/private/" + caName + "_ca_privkey.key " + bits, "CA Private Key generated");
			callProc("/usr/bin/openssl", "rsa -in " + firstSetup.mainDir + "/" + caName +"-ca/private/" + caName + "_ca_privkey.key -passin pass:" +passphrase+" -pubout " +
				"-out " + firstSetup.mainDir + "/" +caName + "-ca/private/" + caName + "_ca_pubkey.key", "CA Public Key generated");
		}

		//RSA Schlüsselgenerierung für einen von der CA generierten Request

		public static void createRSAREQKey(string caName, string serial, string cipherAlgo, string passphrase, string bits){

				callProc ("/usr/bin/openssl", "genrsa " + "-" + cipherAlgo + " -passout pass:" + passphrase + " -out " +
				firstSetup.mainDir + "/" + caName + "-ca/certreqs/" + serial + ".key " + bits, "REQ Key generated");		
		}
			

		/*Request zum erstellen eines Selfsigned CA Certificates wird mit dem vorher generierten
		 * RSA-CA-Key erstellt*/


		public static void createReqCa(string caName,string passphrase,string days,string country, string state,
			string location,string organization, string ounit, string commonname){

			callProc ("/usr/bin/openssl","req -new -key " + firstSetup.mainDir + "/" + caName + "-ca/private/" + caName + "_ca_privkey.key" +
			" -passin pass:" + passphrase + " -days " + days + " -subj \"/C=" + country + "/ST=" + state +
				"/L=" + location + "/O=" + organization + "/OU=" + ounit + "/CN=" + commonname + "\" -nodes" +
				" -out " + firstSetup.mainDir + "/" + caName + "-ca/" + caName + "-ca_req.csr -config " + firstSetup.mainDir + "/" + caName + "-ca/openssl.conf","REQ for CA generated");		
			}

		//Erstellen des (selfsigned) CA-Root-Certificates

		public static void selfsignCa(string caName, string days, string passphrase, string digest){

			callProc("/usr/bin/openssl","ca -batch -out " + firstSetup.mainDir + "/" + caName + "-ca/" + caName + "-ca_cacert.crt -days " + days +
				" -md "+ digest +" -keyfile " + firstSetup.mainDir + "/" + caName + "-ca/private/" + caName + "_ca_privkey.key -passin pass:" + passphrase +
				" -selfsign -extensions v3_ca -config " + firstSetup.mainDir + "/" + caName + "-ca/openssl.conf -infiles " + firstSetup.mainDir + "/" + caName +
				"-ca/" + caName + "-ca_req.csr","CA selfsigned");
		}

		//Generierung ein Certificate Revocation List für das spätere widerrufen von Zertifikaten

		public static void genCRL(string caName, string passphrase){

			callProc ("/usr/bin/openssl","ca -gencrl -config " + firstSetup.mainDir + "/" + caName + "-ca/openssl.conf -crlexts crl_ext" +
				" -passin pass:" + passphrase + " -out " + firstSetup.mainDir + "/" + caName + "-ca/crl/" + caName + "-ca.crl","CRL generated");
		}


		//Funktion zur Erstellung eines nicht importierten Requests

		public static void createReqCert(string caName,string serial,string commonName,string passphrase,string days,string country, string state,
			string location,string organization, string ounit){

			callProc ("/usr/bin/openssl","req -new -key " + firstSetup.mainDir + "/" + caName + 
				"-ca/certreqs/" + serial + ".key " + "-passin pass:" + passphrase + " -days " + days + " -subj \"/C=" + country + "/ST=" + state + "/L=" + location +
				"/O=" + organization + "/OU=" + ounit + "/CN=" + commonName + "\" -out " + firstSetup.mainDir + "/" + caName + 
				"-ca/certreqs/" + serial + ".csr -config " + firstSetup.mainDir + "/" + caName + "-ca/openssl.conf -extensions v3_req","Certificate Request generated");
		}

		//Durch die CA werden hiermit die Zertifikate gesigned

		public static void signCert(string config, string extfile, string req, string pass){
			
			callProc ("/usr/bin/openssl","ca -batch -config " + config + " -passin pass:" + pass +
				" -extfile " + extfile + " -infiles " + req,"Certificate signed");
			}

		/*Ebenfalls eine Funktion zum signieren, allerdings von importierten Requests, durch diese
		 * Funktion wird sichergestellt, dass der Public des Importrequests erhalten bleibt, und 
		 * ggf. die Subjectdaten bei der Zertifizierung nochmals abgeändert werden können*/

		public static void signCertImportedReq(string config, string extfile, string req, string pass,
			string commonName,string country, string state,string location,
			string organization, string ounit){
			callProc ("/usr/bin/openssl","ca -batch -config " + config + " -passin pass:" + pass +
				" -extfile " + extfile + " -subj \"/C=" + country + "/ST=" + state + "/L=" + location +
				"/O=" + organization + "/OU=" + ounit + "/CN=" + commonName + "\"" +
				" -in " + req ,"Certificate signed");
		}

		//Funktion zum Widerrufen von Zertifikaten mit Angabe eines Grundes

		public static void revokeCert(string caName, string certNo, string pass, string reason){

			callProc ("/usr/bin/openssl", "ca -batch -config " + firstSetup.mainDir + "/" + caName + "-ca/openssl.conf " +
			"-passin pass:" + pass + " -crl_reason " + reason + " -revoke " + firstSetup.mainDir + "/" + caName + "-ca/certs/" + certNo + ".pem" +
			" -keyfile " + firstSetup.mainDir + "/" + caName + "-ca/private/" + caName + "_ca_privkey.key -cert " +
			firstSetup.mainDir + "/" + caName + "-ca/" + caName + "-ca_cacert.crt","Certificate Revoked");
		}

		/*Exportfunktion für Zertifikate, deren Request ebenfalls von der eigenen CA erstellt wurde.
		 * Dadurch ist der Private Key jedes Zertifikats vorhandnen und kann ins PKCS12 Format
		 * zusammen mit dem Zertifikat exportiert werden*/

		public static void exportpkcs12Cert(string certName, string inpath, string outpath, string num, string passout, string passin, string keypath){
			callProc ("/usr/bin/openssl", "pkcs12 -export -passin pass:" + passin + " -password pass:" + passout
				+ " -out " + outpath + "/" + certName + ".pfx -inkey " + keypath + " -in " + inpath, "Certificate: " + certName + " exported");
		}

		/*Exportfunktion für das Root-Zertifikat. Hier wird nur ins PEM-Format exportiert, da bei
		 * diesem Format der private Schlüssel nicht mitexportiert wird. Private Schlüssel von Root-
		 * Zertifikaten sollten immer auch privat bleiben*/

		public static void exportCaCert(string caName, string path){
			callProc ("/usr/bin/openssl", "x509 -in " + firstSetup.mainDir + "/" + caName + "-ca/" + caName + "-ca_cacert.crt" +
				" -outform PEM -out " + path, "CA - Certificate from "+caName + " to "+ path + " exported");
		}

		/*Da bei einem importierten Request nur der öffentliche Schlüssel, kodiert im jeweiligen
		 * Request vorliegt, kann auch hier nur in das Format PEM exportiert werden*/

		public static void exportImportCert(string certName, string inpath, string outpath){
			callProc ("/usr/bin/openssl", "x509 -in " + inpath + " -outform PEM -out " + outpath + "/" + certName + ".pem", "Certificate: " + certName + " exported");
		}

		/*Funktion zur Überprüfung des Passworts vom Privatekey der CA anhand von einer Errormeldung
		 * von Openssl, welche bei Falscheingabe ausgegeben wird*/

		public static void checkPass(string pass){
			callProc("/usr/bin/openssl","rsa -in " + firstSetup.mainDir + "/" + mainWindow.selectedCA +
				"-ca/private/" + mainWindow.selectedCA + "_ca_privkey.key -check -passin pass:" + pass + " -noout", "readkey");
		}

		//Funktion zur Ausführung eines Bash Scripts

		public static void runScript(string path){
			callProc ("/bin/bash", path, "Run Script: " + path);
		}

		//Funktion um dem Logfile Einträge hinzuzufügen

		public static void writeLog(string arg, List<string> input){
			TextWriter w = new StreamWriter (firstSetup.mainDir + "/log.txt",true);
			w.WriteLine("LOG: " + DateTime.Now.ToLongTimeString() + ", " + DateTime.Now.ToLongDateString()+", "+ arg +" ");
			foreach (var i in input) {
				w.WriteLine (i);
			}
			w.Close();
		}
	}
}
	