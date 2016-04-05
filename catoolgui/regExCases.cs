using System;
using System.Text.RegularExpressions;

namespace compactCA
{
	public class regExCases
	{

		/*Da die Expressions in mehreren Fenstern/Dialogen Verwendung finden werden sie in dieser 
		 * Klasse für alle zugänglich gemacht*/
	
		public static string commonRegex = @"(^[a-zA-Z0-9\s\-\&\.\:\\\/]+$)";
		public static string storageRegex = @"(^[a-zA-Z0-9]+$)";
		public static string countryRegex = @"([A-Za-z]{2})";
		public static string passRegex = @".{4,}";
		public static string stateRegex = @"(^[A-Za-z\s\-]+$)";
		public static string dnsRegex = @"^(([a-zA-Z0-9\*]|[a-zA-Z0-9\*][a-zA-Z0-9\-]*[a-zA-Z0-9\*])\.)*([A-Za-z0-9\*]|[A-Za-z0-9\*][A-Za-z0-9\-\*]*[A-Za-z0-9\*])$";
		public static string emailRegex = @"^[A-Za-z!#$%&'*+\-/=?\^_`{|}~]+(\.[A-Za-z!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" +
			@"((([\-A-Za-z]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
		public static string uriRegex = @"^(http|https|ftp)\://[a-zA-Z0-9\-\.\*]+\.[a-zA-Z\*]{2,3}(:[a-zA-Z0-9\*]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~\*])*$";
		public static string validRegex = @"^[0-9]{1,4}";
		public static string ipRegex = @"^(((([1]?\d)?\d|2[0-4]\d|25[0-5])\.){3}(([1]?\d)?\d|2[0-4]\d|25[0-5]))|" + 
			@"([\da-fA-F]{1,4}(\:[\da-fA-F]{1,4}){7})|(([\da-fA-F]{1,4}:){0,5}::([\da-fA-F]{1,4}:){0,5}[\da-fA-F]{1,4})$";
	}
}