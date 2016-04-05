using System;
using Gtk;

namespace compactCA
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();

			//Lege Ordnerstruktur für das Programm an, wenn nicht bereits vorhanden

			firstSetup.createMainDir ("CompactCA");

			mainWindow w = new mainWindow ();
			Application.Run ();
		}

		public static void terminateApp(){
			Application.Quit ();
		}
	}
}
