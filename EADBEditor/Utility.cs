using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EA_DB_Editor
{

    public static class Utility
    {
        public static int Next(int inclusive, int exclusive)
        {
            var guid = Guid.NewGuid().ToByteArray().Take(3);
            var list = new List<byte>(guid);
            list.Add(0);
            var rand = BitConverter.ToInt32(list.ToArray(), 0);
            var diff = exclusive - inclusive;
            return inclusive + rand % diff;
        }
        public static void ToJsonFile<T>(this T obj, string file)
        {
            string json = null;

            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
                js.WriteObject(ms, obj);

                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    json = sr.ReadToEnd();
                }
            }

            File.WriteAllText(file, json);
        }

        public static T FromJsonFile<T>(this string file)
        {
            var json = File.ReadAllText(file);
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            try
            {
                var js = new DataContractJsonSerializer(typeof(T));
                return (T)js.ReadObject(ms);
            }
            finally
            {
                ms.Dispose();
            }
        }


        public static string ToTeamName(this int Team_ID)
        {
            String Team_Name;
            if (Team_ID == 1) { Team_Name = "Air Force"; }
            else if (Team_ID == 2) { Team_Name = "Akron"; }
            else if (Team_ID == 3) { Team_Name = "Alabama"; }
            else if (Team_ID == 4) { Team_Name = "Arizona"; }
            else if (Team_ID == 5) { Team_Name = "Arizona State"; }
            else if (Team_ID == 6) { Team_Name = "Arkansas"; }
            else if (Team_ID == 7) { Team_Name = "Arkansas State"; }
            else if (Team_ID == 8) { Team_Name = "Army"; }
            else if (Team_ID == 9) { Team_Name = "Auburn"; }
            else if (Team_ID == 10) { Team_Name = "Ball State"; }
            else if (Team_ID == 11) { Team_Name = "Baylor"; }
            else if (Team_ID == 12) { Team_Name = "Boise State"; }
            else if (Team_ID == 13) { Team_Name = "Boston College"; }
            else if (Team_ID == 14) { Team_Name = "Bowling Green"; }
            else if (Team_ID == 15) { Team_Name = "Buffalo"; }
            else if (Team_ID == 16) { Team_Name = "BYU"; }
            else if (Team_ID == 17) { Team_Name = "California"; }
            else if (Team_ID == 18) { Team_Name = "UCF"; }
            else if (Team_ID == 19) { Team_Name = "Central Michigan"; }
            else if (Team_ID == 20) { Team_Name = "Cincinnati"; }
            else if (Team_ID == 21) { Team_Name = "Clemson"; }
            else if (Team_ID == 22) { Team_Name = "Colorado"; }
            else if (Team_ID == 23) { Team_Name = "Colorado State"; }
            else if (Team_ID == 24) { Team_Name = "Duke"; }
            else if (Team_ID == 25) { Team_Name = "ECU"; }
            else if (Team_ID == 26) { Team_Name = "Eastern Michigan"; }
            else if (Team_ID == 27) { Team_Name = "Florida"; }
            else if (Team_ID == 28) { Team_Name = "Florida State"; }
            else if (Team_ID == 29) { Team_Name = "Fresno State"; }
            else if (Team_ID == 30) { Team_Name = "Georgia"; }
            else if (Team_ID == 31) { Team_Name = "Georgia Tech"; }
            else if (Team_ID == 32) { Team_Name = "Hawai'i"; }
            else if (Team_ID == 33) { Team_Name = "Houston"; }
            else if (Team_ID == 34) { Team_Name = "Idaho"; }
            else if (Team_ID == 35) { Team_Name = "Illinois"; }
            else if (Team_ID == 36) { Team_Name = "Indiana"; }
            else if (Team_ID == 37) { Team_Name = "Iowa"; }
            else if (Team_ID == 38) { Team_Name = "Iowa State"; }
            else if (Team_ID == 39) { Team_Name = "Kansas"; }
            else if (Team_ID == 40) { Team_Name = "Kansas State"; }
            else if (Team_ID == 41) { Team_Name = "Kent State"; }
            else if (Team_ID == 42) { Team_Name = "Kentucky"; }
            else if (Team_ID == 43) { Team_Name = "Louisiana Tech"; }
            else if (Team_ID == 44) { Team_Name = "Louisville"; }
            else if (Team_ID == 45) { Team_Name = "LSU"; }
            else if (Team_ID == 46) { Team_Name = "Marshall"; }
            else if (Team_ID == 47) { Team_Name = "Maryland"; }
            else if (Team_ID == 48) { Team_Name = "Memphis"; }
            else if (Team_ID == 49) { Team_Name = "Miami"; }
            else if (Team_ID == 50) { Team_Name = "Miami University"; }
            else if (Team_ID == 51) { Team_Name = "Michigan"; }
            else if (Team_ID == 52) { Team_Name = "Michigan State"; }
            else if (Team_ID == 53) { Team_Name = "Mid Tenn State"; }
            else if (Team_ID == 54) { Team_Name = "Minnesota"; }
            else if (Team_ID == 55) { Team_Name = "Mississippi State"; }
            else if (Team_ID == 56) { Team_Name = "Missouri"; }
            else if (Team_ID == 57) { Team_Name = "Navy"; }
            else if (Team_ID == 58) { Team_Name = "Nebraska"; }
            else if (Team_ID == 59) { Team_Name = "Nevada"; }
            else if (Team_ID == 60) { Team_Name = "New Mexico"; }
            else if (Team_ID == 61) { Team_Name = "New Mexico State"; }
            else if (Team_ID == 62) { Team_Name = "North Carolina"; }
            else if (Team_ID == 63) { Team_Name = "NC State"; }
            else if (Team_ID == 64) { Team_Name = "North Texas"; }
            else if (Team_ID == 65) { Team_Name = "UL Monroe"; }
            else if (Team_ID == 66) { Team_Name = "Northern Illinois"; }
            else if (Team_ID == 67) { Team_Name = "Northwestern"; }
            else if (Team_ID == 68) { Team_Name = "Notre Dame"; }
            else if (Team_ID == 69) { Team_Name = "Ohio"; }
            else if (Team_ID == 70) { Team_Name = "Ohio State"; }
            else if (Team_ID == 71) { Team_Name = "Oklahoma"; }
            else if (Team_ID == 72) { Team_Name = "Oklahoma State"; }
            else if (Team_ID == 73) { Team_Name = "Ole Miss"; }
            else if (Team_ID == 74) { Team_Name = "Oregon"; }
            else if (Team_ID == 75) { Team_Name = "Oregon State"; }
            else if (Team_ID == 76) { Team_Name = "Penn State"; }
            else if (Team_ID == 77) { Team_Name = "Pittsburgh"; }
            else if (Team_ID == 78) { Team_Name = "Purdue"; }
            else if (Team_ID == 79) { Team_Name = "Rice"; }
            else if (Team_ID == 80) { Team_Name = "Rutgers"; }
            else if (Team_ID == 81) { Team_Name = "San Diego State"; }
            else if (Team_ID == 82) { Team_Name = "San Jose State"; }
            else if (Team_ID == 83) { Team_Name = "SMU"; }
            else if (Team_ID == 84) { Team_Name = "South Carolina"; }
            else if (Team_ID == 85) { Team_Name = "Southern Miss"; }
            else if (Team_ID == 86) { Team_Name = "UL Lafayette"; }
            else if (Team_ID == 87) { Team_Name = "Stanford"; }
            else if (Team_ID == 88) { Team_Name = "Syracuse"; }
            else if (Team_ID == 89) { Team_Name = "TCU"; }
            else if (Team_ID == 90) { Team_Name = "Temple"; }
            else if (Team_ID == 91) { Team_Name = "Tennessee"; }
            else if (Team_ID == 92) { Team_Name = "Texas"; }
            else if (Team_ID == 93) { Team_Name = "Texas A&M"; }
            else if (Team_ID == 94) { Team_Name = "Texas Tech"; }
            else if (Team_ID == 95) { Team_Name = "Toledo"; }
            else if (Team_ID == 96) { Team_Name = "Tulane"; }
            else if (Team_ID == 97) { Team_Name = "Tulsa"; }
            else if (Team_ID == 98) { Team_Name = "UAB"; }
            else if (Team_ID == 99) { Team_Name = "UCLA"; }
            else if (Team_ID == 100) { Team_Name = "Connecticut"; }
            else if (Team_ID == 101) { Team_Name = "UNLV"; }
            else if (Team_ID == 102) { Team_Name = "USC"; }
            else if (Team_ID == 103) { Team_Name = "Utah"; }
            else if (Team_ID == 104) { Team_Name = "Utah State"; }
            else if (Team_ID == 105) { Team_Name = "UTEP"; }
            else if (Team_ID == 106) { Team_Name = "Vanderbilt"; }
            else if (Team_ID == 107) { Team_Name = "Virginia"; }
            else if (Team_ID == 108) { Team_Name = "Virginia Tech"; }
            else if (Team_ID == 109) { Team_Name = "Wake Forest"; }
            else if (Team_ID == 110) { Team_Name = "Washington"; }
            else if (Team_ID == 111) { Team_Name = "Washington State"; }
            else if (Team_ID == 112) { Team_Name = "West Virginia"; }
            else if (Team_ID == 113) { Team_Name = "Western Michigan"; }
            else if (Team_ID == 114) { Team_Name = "Wisconsin"; }
            else if (Team_ID == 115) { Team_Name = "Wyoming"; }
            else if (Team_ID == 143) { Team_Name = "Troy"; }
            else if (Team_ID == 144) { Team_Name = "USF"; }
            else if (Team_ID == 160) { Team_Name = "FCS East"; }
            else if (Team_ID == 161) { Team_Name = "FCS West"; }
            else if (Team_ID == 162) { Team_Name = "FCS Northwest"; }
            else if (Team_ID == 163) { Team_Name = "FCS Midwest"; }
            else if (Team_ID == 164) { Team_Name = "FCS Southeast"; }
            else if (Team_ID == 181) { Team_Name = "UMass"; }
            else if (Team_ID == 211) { Team_Name = "Western Kentucky"; }
            else if (Team_ID == 218) { Team_Name = "Texas State"; }
            else if (Team_ID == 229) { Team_Name = "Florida Atlantic"; }
            else if (Team_ID == 230) { Team_Name = "FIU"; }
            else if (Team_ID == 232) { Team_Name = "UTSA"; }
            else if (Team_ID == 233) { Team_Name = "Georgia State"; }
            else if (Team_ID == 234) { Team_Name = "Old Dominion"; }
            else if (Team_ID == 235) { Team_Name = "South Alabama"; }
            else { Team_Name = "Unemployeed"; }
            return Team_Name;
        }

        public static string ToPositionName(this int POS)
        {
            String POS_NAME = "";
            if (POS == 0) { POS_NAME = "QB"; }
            if (POS == 1) { POS_NAME = "HB"; }
            if (POS == 2) { POS_NAME = "FB"; }
            if (POS == 3) { POS_NAME = "WR"; }
            if (POS == 4) { POS_NAME = "TE"; }
            if (POS == 5) { POS_NAME = "LT"; }
            if (POS == 6) { POS_NAME = "LG"; }
            if (POS == 7) { POS_NAME = "C"; }
            if (POS == 8) { POS_NAME = "RG"; }
            if (POS == 9) { POS_NAME = "RT"; }
            if (POS == 10) { POS_NAME = "LE"; }
            if (POS == 11) { POS_NAME = "RE"; }
            if (POS == 12) { POS_NAME = "DT"; }
            if (POS == 13) { POS_NAME = "LOLB"; }
            if (POS == 14) { POS_NAME = "MLB"; }
            if (POS == 15) { POS_NAME = "ROLB"; }
            if (POS == 16) { POS_NAME = "CB"; }
            if (POS == 17) { POS_NAME = "FS"; }
            if (POS == 18) { POS_NAME = "SS"; }
            if (POS == 19) { POS_NAME = "K"; }
            if (POS == 20) { POS_NAME = "P"; }
            return POS_NAME;
        }
    }
}