using System;
using System.IO;
using Mono.Data.Sqlite;

namespace catoolgui
{
	public class firstSetup
	{
		static string homeFolder = Environment.GetEnvironmentVariable ("HOME");
		public static string mainDir;
		public static msgWindow mWin;

		//Erstelle Hauptverzeichniss des Programms, falss dieses noch nicht existiert

		public static void createMainDir(string mainFolder){

			try{
				
			if (!Directory.Exists (homeFolder + "/" + mainFolder)) {
				mainDir = homeFolder + "/" + mainFolder;
				Directory.CreateDirectory (homeFolder + "/" + mainFolder);
				createDB (firstSetup.mainDir + "/caDB.sqlite", "create table CA (caName varchar(100) primary key, path varchar (100))");
			}

			//Ansonsten setze nur den Pfad zum Hauptverzeichniss

			else {
				mainDir = homeFolder + "/" + mainFolder;				
				return;
			}
			}
			catch(IOException ioex){
				mWin = new msgWindow (ioex.Message, "error");
			}
		}
			
		//Erstelle eine Sqlite-Datei und erstelle die möglichen Tabellen 

		public static void createDB(string filename,string sql){
			try{
			SqliteConnection.CreateFile (filename);

			using (SqliteConnection con = new SqliteConnection ("Data Source=" + filename)) {
				con.Open ();
				string stm = sql;
				using (SqliteCommand cmd = new SqliteCommand (stm, con)) {
					cmd.ExecuteNonQuery ();
				}
				con.Close ();
			}
			}
			catch(SqliteException sqlex){
				mWin = new msgWindow (sqlex.Message, "error");
			}
		}
	}
}