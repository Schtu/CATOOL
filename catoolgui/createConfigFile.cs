using System;
using System.IO;
using System.Collections.Generic;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using IniParser.Parser;

namespace compactCA
{
	public class createConfigFile
	{
		public static StreamWriter swriter;
		public static FileIniDataParser parser = new FileIniDataParser ();
		public static IniData parsedData = new IniData ();
		public static List<string> extList = new List<string> ();


		//Schreibe die Daten des Configfiles in das INI-Parser-Dictionary

		public static void writeCaConf(string name, string digest, string bits, string crlDays){

			parsedData.Sections.AddSection ("ca");
			parsedData ["ca"].AddKey ("default_ca", "CA_default");
			parsedData.Sections.AddSection ("CA_default");
			parsedData ["CA_default"].AddKey ("dir", firstSetup.mainDir + "/" + name + "-ca");
			parsedData ["CA_default"].AddKey ("certs", "$dir/certsdb");
			parsedData ["CA_default"].AddKey ("new_certs_dir", "$dir/certs");
			parsedData ["CA_default"].AddKey ("database", "$dir/index.txt");
			parsedData ["CA_default"].AddKey ("certificate", "$dir/" + name + "-ca_cacert.crt");
			parsedData ["CA_default"].AddKey ("private_key", "$dir/private/" + name + "_ca_privkey.key");
			parsedData ["CA_default"].AddKey ("serial", "$dir/serial");
			parsedData ["CA_default"].AddKey ("crldir", "$dir/crl");
			parsedData ["CA_default"].AddKey ("crlnumber", "$dir/crlnumber");
			parsedData ["CA_default"].AddKey ("crl", "$dir/" + name + "-ca.crl");

			parsedData ["CA_default"].AddKey ("utf8", "yes");
			parsedData ["CA_default"].AddKey ("string_mask", "utf8only");
			parsedData ["CA_default"].AddKey ("nameopt", "multiline,utf8,-esc_msb");
			parsedData ["CA_default"].AddKey ("x509_extensions", "usr_cert");
			parsedData ["CA_default"].AddKey ("copy_extensions", "copy");

			parsedData ["CA_default"].AddKey ("default_days", "365");
			parsedData ["CA_default"].AddKey ("default_crl_days", crlDays);
			parsedData ["CA_default"].AddKey ("default_md", digest);
			parsedData ["CA_default"].AddKey ("preserve", "no");
			parsedData ["CA_default"].AddKey ("policy", "policy_match");
			parsedData ["CA_default"].AddKey ("unique_subject", "no");

			parsedData.Sections.AddSection ("policy_match");
			parsedData ["policy_match"].AddKey ("countryName", "optional");
			parsedData ["policy_match"].AddKey ("stateOrProvinceName", "optional");
			parsedData ["policy_match"].AddKey ("localityName", "optional");
			parsedData ["policy_match"].AddKey ("organizationName", "optional");
			parsedData ["policy_match"].AddKey ("organizationalUnitName", "optional");
			parsedData ["policy_match"].AddKey ("commonName", "optional");

			parsedData.Sections.AddSection ("policy_anything");
			parsedData ["policy_anything"].AddKey ("countryName", "optional");
			parsedData ["policy_anything"].AddKey ("stateOrProvinceName", "optional");
			parsedData ["policy_anything"].AddKey ("localityName", "optional");
			parsedData ["policy_anything"].AddKey ("organizationName", "optional");
			parsedData ["policy_anything"].AddKey ("organizationalUnitName", "optional");
			parsedData ["policy_anything"].AddKey ("commonName", "supplied");

			parsedData.Sections.AddSection ("req");
			parsedData ["req"].AddKey ("default_bits", bits);
			parsedData ["req"].AddKey ("default_keyfile", name + "_ca_privkey.pem");
			parsedData ["req"].AddKey ("distinguished_name", "req_distinguished_name");
			parsedData ["req"].AddKey ("attributes", "req_attributes");
			parsedData ["req"].AddKey ("utf8", "yes");
			parsedData ["req"].AddKey ("string_mask", "utf8only");
			parsedData ["req"].AddKey ("nameopt", "multiline,utf8,-esc_msb");
			parsedData ["req"].AddKey ("x509_extensions", "v3_ca");
			parsedData ["req"].AddKey ("req_extensions", "v3_req");

			parsedData.Sections.AddSection ("req_distinguished_name");
			parsedData ["req_distinguished_name"].AddKey ("countryName", "Country Name (2 letter code)");
			parsedData ["req_distinguished_name"].AddKey ("countryName_default", "DE");
			parsedData ["req_distinguished_name"].AddKey ("countryName_min", "2");
			parsedData ["req_distinguished_name"].AddKey ("countryName_max", "2");
			parsedData ["req_distinguished_name"].AddKey ("stateOrProvinceName", "State or Province Name (full name)");
			parsedData ["req_distinguished_name"].AddKey ("localityName", "Locality Name (eg, city)");
			parsedData ["req_distinguished_name"].AddKey ("organizationName", "Organization Name (eg, company)");
			parsedData ["req_distinguished_name"].AddKey ("organizationUnitName", "Organization Unit Name (eg, section)");
			parsedData ["req_distinguished_name"].AddKey ("commonName", "Common Name (eg, Your Name)");
			parsedData ["req_distinguished_name"].AddKey ("commonName_max", "100");

			parsedData.Sections.AddSection ("req_attributes");
			parsedData ["req_attributes"].AddKey ("challengePassword", "A challenge password");
			parsedData ["req_attributes"].AddKey ("challengePassword_min", "4");
			parsedData ["req_attributes"].AddKey ("challengePassword_max", "20");
		
			//Schreibe Daten der CA "name" in die festgelegte Configdatei

			setStreamerWriter (name);
			parser.WriteData (swriter,parsedData);
			parsedData.Sections.Clear ();
			swriter.Close();
		}

		//Extensions für usr_certs

		public static void writeUsrExt(List<string> input){
			extList.Add ("\n[usr_cert]\n");
			foreach (var i in input) {
				extList.Add (i);
			}
		}

		//Extensions für Requests

		public static void writev3reqExt(List<string> input){
			extList.Add ("\n[v3_req]\n");
			foreach (var i in input) {
				extList.Add (i);
			}
		}

		//Extensions für CA

		public static void writev3caExt(List<string> input){
			extList.Add ("\n[v3_ca]\n");
			foreach (var i in input) {
				extList.Add (i);
			}
		}

		//Extensions für CRL

		public static void writecrlExt(List<string> input){
			extList.Add ("\n[crl_ext]\n");
			foreach (var i in input) {
				extList.Add (i);
			}
		}

		//Setze Streamwriter zum schreiben der Configdatei

		public static void setStreamerWriter(string caName){
			string temp = firstSetup.mainDir +"/"+ caName+ "-ca";
			string path = Path.Combine (temp, "openssl.conf");
			swriter = new StreamWriter (path,true);
		}
			
		public static void writeConfig(string name, string digest, string bits, string crlDays){

			//Schreibe Configdatei

			writeCaConf (name,digest,bits,crlDays);
			setStreamerWriter (name);

			//Schreibe Extensions

			foreach (var entry in extList) {
				swriter.WriteLine (entry);
			}
			extList.Clear ();
			swriter.Close ();
		}
	}
}