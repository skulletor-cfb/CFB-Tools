using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace EA_DB_Editor
{

    class DirectoryCopyEx
    {
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubs, string year, bool overwrite=false)
        {
            int seasonYear = year.ToInt32();
            bool existingSeason = false;
            if (string.IsNullOrEmpty(year) == false && (destDirName.EndsWith("EOY")||destDirName.EndsWith("Season")))
            {
                var seasonsFile = Seasons.SeasonsFile ;
                Seasons seasons = Seasons.FromFile(seasonsFile);

                var seasonAlreadyExisting = seasons.SeasonList.Where(season => season.Year == seasonYear).SingleOrDefault();
                if (seasonAlreadyExisting != null)
                {
                    destDirName = seasonAlreadyExisting.Directory.Replace('/','\\');
                    existingSeason = true; 
                }
                else
                {
                    seasons.AddYearAndWriteToFile(seasonsFile, seasonYear, destDirName);
                }
            }

            //get subdir for the specified dir
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source Directory DNE or not found:" + sourceDirName);
            }

            //if the destination directory doesnt exist create it
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // get files in the directory and copy them to new location
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                try
                {
                    file.CopyTo(temppath, overwrite);
                }
                catch
                {

                }
            }

            //if copying subs copy them and their contents to new location
            if (copySubs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubs, null);
                }
            }

            // if the folder is already there don't do anything
            if (existingSeason)
                return; 

            //CODE TO MAKE INDEX.HTML Webpage of the Archived websites
            TextWriter tw = null;
            TextReader Nav_Bar = null;

            tw = new StreamWriter("./Archive/Seasons.html", false);
            Nav_Bar = new StreamReader("./Archive/HTML/Nav_Bar_Arch");

                tw.Write("<html><head><title>Archived Seasons</title><link rel=stylesheet type=text/css href=./HTML/styles.css></head>");
            tw.Write(Nav_Bar.ReadToEnd()); Nav_Bar.Close();
            tw.Write("<table>");
            tw.Write("<tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=./HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
            tw.Write("<tr><td class='c3' colspan='13'><center><b>| <a href ='BowlHistory.html?id=-1&sep=true&yr=" + seasonYear + "'>Playoff History</a> | <a href ='BowlHistory.html?id=-2&sep=true&yr=" + seasonYear + "'>Kickoff Game History</a> | <a href ='CoachHistory.html'>Coach History</a> |</b></center></td></tr>");
            tw.Write("<tr><td width=900 align=center colspan=8><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Seasons</b></td></tr>");


            DirectoryInfo dir2 = new DirectoryInfo("./Archive");
            DirectoryInfo[] dirs2 = dir2.GetDirectories()
                .Where(d=>!d.Name.ToUpperInvariant().Contains("app_code".ToUpperInvariant()))
                .Where(d => !d.Name.ToUpperInvariant().Contains("BIN".ToUpperInvariant()))
                .Where(d => !d.Name.ToUpperInvariant().Contains("REPORTS".ToUpperInvariant()))
                .Where(d => !d.Name.ToUpperInvariant().Contains("Continuation".ToUpperInvariant()))
                    .OrderByDescending(dirInfo => dirInfo.Name).ToArray();

            foreach (DirectoryInfo subdir in dirs2)
            {
                if ((subdir.ToString() != "HTML") && (subdir.ToString() != "Reports"))
                {
                    tw.Write("<tr>");
                    tw.Write("<td class=C3 width=15%><a href=./" + subdir + "/Index.html>" + subdir + "</a></td>");
                    tw.Write("</tr>");
                }
                if (subdir.ToString() == "Reports")
                {
                    tw.Write("<tr>");
                    tw.Write("<td class=C3 width=15%><a href=./" + subdir + "/Index.html>Current Season</a></td>");
                    tw.Write("</tr>");
                }
            }

            tw.Write("</table>");


            tw.Close();


        }
    }
}