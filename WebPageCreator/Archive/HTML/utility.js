//****************************************************************************************
//****************************************************************************************
//			ADJUSTABLE VARIABLES
//****************************************************************************************
//****************************************************************************************
var usingGZip = false; 

// This is the start year of the Dynasty. If you adjust the starting year
// in the NCAA2014_Webpage_Maker.exe.config you need to do so here as well.
var startingYear = 2013;

// Determines how many players are on a teams All-Time Greats Lists
var topPlayers = 15;

function getPlayoffGames(year) {
    // 0 == rose/sugar, 1 = orange/cotton, 2 = peach/fiesta
    var rotationSpot = year % 3;

    if (rotationSpot == 0) {
        return [39, 12, 26];
    }

    if (rotationSpot == 1) {
        return [39, 25, 27];
    }

    if (rotationSpot == 2) {
        return [39, 28, 17];
    }
}
//****************************************************************************************
//****************************************************************************************
//			GLOBAL VARIABLES - Do Not Adjust These
//****************************************************************************************
//****************************************************************************************
var conferences = null;
var divisions = [];
var currentYear = 0;
var teamId = 0;
var loadCareerStats = false;
var currentData = null;
var seasons = null;
var teamawards = [];
var allamericans = [];
var htmlDir = "../HTML";
var currentCoachName = "";
var currentCoachId = 0;
var teamsLoaded = 0;
var currentTeam = null;
var currentConferenceId = 14;
var headToHeadResults = [];
var isCoachH2H = false;
var seasonsAsHeadCoach = null;
var currentFilter = null;

function evalJson(str) {
    var json = str;

    if (usingGZip) {
        json = JXG.decompress(json);
    }

    return eval("(" + json + ")");
}

function loadHSAAData() {
    $.ajax({
        url: "hsaa-east",
        success: function (json) {
            east = evalJson(json);
            loadHSAARosterTable(east, "hsaaeast");
        }
    });

    $.ajax({
        url: "hsaa-west",
        success: function (json) {
            west = evalJson(json);
            loadHSAARosterTable(west, "hsaawest");
        }
    });
}

function loadaaacData(confId, file) {
    currentConferenceId = confId;

    if (currentData == null) {
        $.ajax({
            url: file,
            success: function (data) {
                currentData = csvJSON(data);
                parseAllAmericanData();
            }
        });
    }
    else {
        parseAllAmericanData();
    }

    return false;
}

function parseAllAmericanData() {
    var teams = [[], []];

    // all americans have a freshman team too
    if (currentConferenceId == 14) {
        teams[2] = [];
    }

    for (var jj = 0 ; jj < currentData.length ; jj++) {
        if (Number(currentData[jj].ConfId) == currentConferenceId) {
            var currentTeam = teams[currentData[jj].TeamNum];
            currentTeam[currentTeam.length] = currentData[jj];
        }
    }

    loadAllAmericanTable(teams[0], "team1", 0);
    loadAllAmericanTable(teams[1], "team2", 1);

    if (teams.length == 3)
        loadAllAmericanTable(teams[2], "teamfrosh", 2);
    else
        loadAllAmericanTable([], "teamfrosh", 2);
}

function loadAllAmericanTable(roster, table, headerType) {
    var rosterTable = document.getElementById(table);

    var rows = rosterTable.rows.length;
    for (var i = rows - 1 ; rosterTable.rows.length > 2 ; i--) {
        rosterTable.deleteRow(i);
    }

    for (ridx = 0 ; ridx < roster.length ; ridx++) {
        var player = roster[ridx];
        var cells = [player.DisplayPosition, player.PlayerName,
            createTeamLinkTableCell(player.TeamId, player.PlayerTeam),
            player.Height, player.Weight, player.PlayerYear, player.PositionName, player.Ovr];

        if (player.Committed == true) {
            cells[3] = createTeamLinkTableCell(player.TopTeamId, "<b><i>" + player.TopTeam + "</i></b>");
        }

        // insert the rows
        addBasicRowsToTable(rosterTable, cells, "c3");
    }
}

function loadHSAARosterTable(roster, table) {
    var rosterTable = document.getElementById(table);

    for (ridx = 0 ; ridx < roster.length ; ridx++) {
        var player = roster[ridx];
        var cells = [player.PositionRank, player.PositionName, player.FirstName + " " + player.LastName, createTeamLinkTableCell(player.TopTeamId, player.TopTeam), player.Hometown];

        if (player.Committed == true) {
            cells[3] = createTeamLinkTableCell(player.TopTeamId, "<b><i>" + player.TopTeam + "</i></b>");
        }

        // insert the rows
        addBasicRowsToTable(rosterTable, cells, "c3");
    }
}

function addServerRowsToTable(table, rows) {
    for (var w = 0 ; w < rows.length; w++) {
        addBasicRowsToTable(table, rows[w].Cells, "c3");
    }
}

function loadAwardsFromServer(yr, teamId) {
    var uri = "ncaa.svc/awards?yr=" + yr + "&id=" + teamId;

    $.ajax({
        url: uri,
        success: function (data) {
            addServerRowsToTable(document.getElementById("awardDataTable"), data.Awards.Rows);
            addServerRowsToTable(document.getElementById("aaDataTable"), data.AllAmericans.Rows);

            var table = document.getElementById("awardDataTableBowl");
            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Trophy"], ["30%", "Bowl"], ["30%", "Team"]];
            addCellsWithWidth(table, cells, "c7");

            addServerRowsToTable(table, data.BowlWins.Rows);

            table = document.getElementById("awardDataTableConf");
            row = table.insertRow(-1);
            cells = [["10%", "Year"], ["30%", "Trophy"], ["30%", "Conference"], ["30%", "Team"]];
            addCellsWithWidth(table, cells, "c7");
            addServerRowsToTable(table, data.ConferenceChampionships.Rows);
        }
    });
}

function loadAwardsHistory() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    teamId = Number(getQueryVariable("id"));

    var table = document.getElementById("awardDataTable");
    var row = table.insertRow(-1);
    var cells = [["10%", "Year"], ["30%", "Trophy"], ["20%", "Award"], ["10%", "Year"], ["10%", "Pos"], ["20%", "Name"]];
    addCellsWithWidth(table, cells, "c7");

    if (usingGZip) {
        loadAwardsFromServer(currentYear, teamId);
        return;
    }

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(currentYear, "awards.csv", parseAwardsFile, allAmericanCall);
        },
        "./Seasons");

}

//	calls this next and will run through until all season files have been searched
function parseAwardsFile(data, status, jqXHR) {
    var awards = csvJSON(data);
    var teamName = "";

    for (var currAward = 0; currAward < awards.length ; currAward++) {
        if (awards[currAward].AwardName != "" && Number(awards[currAward].TeamId) == teamId) {

            var season = getSeasonForYear(jqXHR.Year);
            teamawards.push({
                AwardId: Number(awards[currAward].AwardId),
                AwardName: awards[currAward].AwardName,
                Class: awards[currAward].Year,
                Name: awards[currAward].Name,
                Position: awards[currAward].Position,
                TeamId: Number(awards[currAward].TeamId),
                Year: Number(jqXHR.Year),
                Directory: season.Directory
            }
            );
        }
    }
}

function allAmericanCall() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    teamId = Number(getQueryVariable("id"));

    var table = document.getElementById("aaDataTable");
    var row = table.insertRow(-1);
    var cells = [["8%", "Year"], ["32%", "Team"], ["20%", "Name"], ["8%", "Height"], ["8%", "Weight"], ["8%", "Class"], ["8%", "Position"], ["8%", "Ovr"]];
    addCellsWithWidth(table, cells, "c7");

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(currentYear, "aaac.csv", parseAllAmericanFile, teamAwardsSeasonsLoaded);
        },
        "./Seasons");

}

//	calls this next and will run through until all season files have been searched
function parseAllAmericanFile(data, status, jqXHR) {
    var aas = csvJSON(data);
    var teamName = "";

    for (var currPlayer = 0; currPlayer < aas.length ; currPlayer++) {
        if (aas[currPlayer].ConfId == 14 && Number(aas[currPlayer].TeamId) == teamId) {

            var season = getSeasonForYear(jqXHR.Year);
            allamericans.push({
                AATeam: Number(aas[currPlayer].TeamNum),
                Class: aas[currPlayer].PlayerYear,
                Name: aas[currPlayer].PlayerName,
                Position: aas[currPlayer].DisplayPosition,
                TeamId: Number(aas[currPlayer].TeamId),
                OVR: Number(aas[currPlayer].Ovr),
                Height: aas[currPlayer].Height,
                Weight: aas[currPlayer].Weight,
                Year: Number(jqXHR.Year),
                Directory: season.Directory
            }
            );
        }
    }
}


//	once the looping of parseAwardsfile is complete this will be called
function teamAwardsSeasonsLoaded() {
    var logo = document.getElementById("currentSchool");
    var logo1 = document.getElementById("currentLogo");
    var logo2 = document.getElementById("currentLogo1");

    logo.src = htmlDir + "/Logos/helmet/team" + teamId + ".png";
    logo1.src = htmlDir + "/Logos/256/team" + teamId + ".png";
    logo2.src = htmlDir + "/Logos/256/team" + teamId + ".png";
    awardTeamLoaded();
}

function awardTeamLoaded() {
    var table = document.getElementById("awardDataTable");
    var currentSeasonDirectory = "";

    teamawards.sort(function (a, b) { return a.Name.localeCompare(b.Name); });
    teamawards.sort(function (a, b) { return Number(a.Year) - Number(b.Year); });

    allamericans.sort(function (a, b) {
        var n = b.AATeam - a.AATeam;
        if (n != 0) {
            return n;
        }
        return a.Year - b.Year;
    });

    for (var hh = seasons.Season.length - 1 ; hh >= 0 ; hh--) {
        var currYearDir = seasons.Season[hh];

        if (currentYear == currYearDir.Year) {
            currentSeasonDirectory = currYearDir.Directory.slice(9);
            break;
        }
    }

    for (var ii = teamawards.length - 1 ; ii >= 0 ; ii--) {
        var currSeason = teamawards[ii];

        if (currSeason.AwardName == null || currSeason.AwardName == undefined)
            continue;

        var directory = currSeason.Directory.replace('/Archive', '');
        var summary = [];
        var team = currSeason.Team;
        var cells = [currSeason.Year, createTrophyCaseAwardLogoLink(currSeason.AwardId), currSeason.AwardName, currSeason.Class, currSeason.Position, currSeason.Name];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");

    }

    table = document.getElementById("aaDataTable");

    for (var iii = allamericans.length - 1 ; iii >= 0 ; iii--) {
        var currSeason = allamericans[iii];

        if (currSeason.AATeam == null || currSeason.AATeam == undefined)
            continue;

        var directory = currSeason.Directory.replace('/Archive', '');
        var summary = [];
        var team = "";
        if (currSeason.AATeam == 0) { team = "1st Team All-American"; }
        if (currSeason.AATeam == 1) { team = "2nd Team All-American"; }
        if (currSeason.AATeam == 2) { team = "Freshman All-American"; }
        var cells = [currSeason.Year, team, currSeason.Name, currSeason.Height, currSeason.Weight, currSeason.Class, currSeason.Position, currSeason.OVR];



        // insert the rows
        addBasicRowsToTable(table, cells, "c3");

    }


    $.ajax({
        url: "." + currentSeasonDirectory + "/bowlchamps.csv",
        success: function (data) {
            var bowlTeamId = getQueryVariable("id");
            var bc = csvJSON(data, "TeamId", bowlTeamId);

            var table = document.getElementById("awardDataTableBowl");
            var currentBowl = -1;

            bc.sort(function (a, b) { return Number(b.Year) - Number(a.Year); });

            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Trophy"], ["30%", "Bowl"], ["30%", "Team"]];
            addCellsWithWidth(table, cells, "c7");

            for (var i = 0 ; i < bc.length ; i++) {
                var champ = bc[i];

                var cells = [Number(startingYear) + Number(bc[i].Year), createTrophyCaseBowlTrophyLink(champ.BowlId), createTrophyCaseBowlLogoLink(champ.BowlId), createTeamLogoLink(champ.TeamId, 65)];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");

            }
        }
    });

    $.ajax({
        url: "." + currentSeasonDirectory + "/cc.csv",
        success: function (data) {
            var bowlTeamId = getQueryVariable("id");
            var bc = csvJSON(data, "TeamId", bowlTeamId);

            var table = document.getElementById("awardDataTableConf");
            var currentBowl = -1;

            bc.sort(function (a, b) { return Number(b.Year) - Number(a.Year); });

            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Trophy"], ["30%", "Conference"], ["30%", "Team"]];
            addCellsWithWidth(table, cells, "c7");

            for (var i = 0 ; i < bc.length ; i++) {
                var champ = bc[i];

                var cells = [Number(startingYear) + Number(bc[i].Year), createTrophyCaseConfTrophyLink(champ.ConferenceId), createTrophyCaseConferenceLogo(champ.ConferenceId), createTeamLogoLink(champ.TeamId, 65)];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");

            }
        }
    });
}

function loadPreSeasonReviewData(year) {
    $.ajax({
        url: "ps-team",
        success: function (data) {
            var teams = evalJson(data);
            var table = document.getElementById("preseasonTop25");

            for (var i = 0 ; i < 25 ; i++) {

                var team = teams[i];
                var record = team.PriorSeasonWin + "-" + team.PriorSeasonLoss;
                var cells = [1 + i, createTeamLogoLinkToTeams(team.Id, 55, "PreSeasonTeam.html"), record, team.MediaCoverage[randNext(0, team.MediaCoverage.length)].Content];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadCCPredictions(year) {
    $.ajax({
        url: "PredictedCC",
        success: function (data) {
            var predictedCC = evalJson(data);
            var table = document.getElementById("ccTable");

            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Conference"], ["30%", "Team"], ["30%", "Team"]];

            for (var j = 0 ; j < cells.length ; j++) {
                var cell = row.insertCell(-1);
                cell.className = "c7";
                cell.width = cells[j][0];
                cell.innerHTML = cells[j][1];
            }

            for (var ii = 0 ; ii < predictedCC.length ; ii++) {
                var team = predictedCC[ii];

                var cells = [year, '<a href="CC.html?id=' + team.ConferenceId + '"><img src="../HTML/Logos/conferences/65/' + team.ConferenceId + '.jpg" /></a>', createTeamLogoLink(team.TeamId, 65), createTeamLinkTableCell(team.TeamId, team.TeamName, true)];

                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadSeasonReviewData(year) {
    startingYear = year;
    $.ajax({
        url: "bcs.csv",
        success: function (data) {
            var teams = csvJSON(data);
            var table = document.getElementById("bcsTable");

            for (var i = 0 ; i < 25 ; i++) {

                var team = teams[i];
                var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Team), team.Record, team.Media, team.Coaches, team.BCSPrevious];

                if (i < 10) {
                    cells[1] = createTeamLogoLinkToTeams(team.TeamId, 55);
                }

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });

    $.ajax({
        url: "cc.csv",
        success: function (data) {
            var confId = getQueryVariable("id");
            var teams = csvJSON(data, "ConferenceId", confId);
            var table = document.getElementById("ccTable");
            var currentConf = -1;

            teams.sort(function (a, b) { return Number(b.Year) - Number(a.Year); });
            var recentYear = teams[0].Year;

            var conferences = 0;
            for (var i = 0; i < teams.length; ++i) {
                if (teams[i].Year == recentYear)
                    conferences++;
            }

            teams = teams.slice(0, conferences);

            //Need to determine how to sort the array by Conference Name
            teams.sort(function (a, b) { return a.Conference.localeCompare(b.Conference); });

            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Conference"], ["30%", "Team"], ["30%", "Team"]];

            for (var j = 0 ; j < cells.length ; j++) {
                var cell = row.insertCell(-1);
                cell.className = "c7";
                cell.width = cells[j][0];
                cell.innerHTML = cells[j][1];
            }

            for (var i = 0 ; i < teams.length ; i++) {
                var team = teams[i];

                var cells = [startingYear, '<a href="CC.html?id=' + team.ConferenceId + '"><img src="../HTML/Logos/conferences/65/' + team.ConferenceId + '.jpg" /></a>', createTeamLogoLink(team.TeamId, 65), createTeamLinkTableCell(team.TeamId, team.Team)];


                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });

    $.ajax({
        url: "bowlchamps.csv",
        success: function (data) {
            var bowlId = getQueryVariable("id");
            var bc = csvJSON(data, "BowlId", bowlId);

            var table = document.getElementById("bowlChampTable");
            var currentBowl = -1;


            bc.sort(function (a, b) { return Number(b.Year) - Number(a.Year); });
            var recentYear = bc[0].Year;

            var bowls = 0;
            for (var i = 0; i < bc.length; ++i) {
                if (bc[i].Year == recentYear)
                    bowls++;
            }

            bc = bc.slice(0, bowls);
            bc.sort(function (a, b) { return Number(b.BowlId) - Number(a.BowlId); });

            var row = table.insertRow(-1);
            var cells = [["10%", "Year"], ["30%", "Bowl"], ["30%", "Team"], ["30%", "Team"]];
            addCellsWithWidth(table, cells, "c7");

            for (var i = 0 ; i < bc.length ; i++) {


                var champ = bc[i];

                if (champ.BowlId == 39 || champ.BowlId == 28 || champ.BowlId == 27 || champ.BowlId == 26 || champ.BowlId == 25 || champ.BowlId == 17 || champ.BowlId == 12) {

                    var cells = [startingYear, createBowlLogoLink(champ.BowlId), createTeamLogoLink(champ.TeamId, 65), createTeamLinkTableCell(champ.TeamId, champ.Team)];

                    // insert the rows
                    addBasicRowsToTable(table, cells, "c3");
                }
            }
        }
    });


    $.ajax({
        url: "awards.csv",
        success: function (data) {
            var award = getQueryVariable("id");
            var d = csvJSON(data);
            var table = document.getElementById("awardTable");
            var awardName = null;


            var row = table.insertRow(-1);
            var cell = row.insertCell(-1);
            cell.className = "c2";
            cell.colSpan = 13;
            cell.innerHTML = "<b>Player Awards</b>";

            row = table.insertRow(-1);
            var cells = [["15%", "Award"], ["25%", "Trophy"], ["30%", "Team"], ["7%", "Year"], ["7%", "Pos"], ["16%", "Name"]];
            addCellsWithWidth(table, cells, "C10");

            d.sort(function (a, b) { return Number(a.AwardId) - Number(b.AwardId); });

            for (var i = 0 ; i < d.length ; i++) {

                if (d[i].AwardName != "") {
                    var cells = [d[i].AwardName, createAwardLogoLink(d[i].AwardId), createTeamLogoLinkToStats(d[i].TeamId, 65), d[i].Year, d[i].Position, d[i].Name];
                    addBasicRowsToTable(table, cells, "c3");
                }


            }
        }
    });
}


function loadSeasonsJsonData(funcToRun, seasonsLocation, state) {

    if (seasonsLocation == null || seasonsLocation == undefined) {
        seasonsLocation = "../Seasons";
    }

    if (seasons == null || seasons == undefined) {
        $.ajax({
            url: seasonsLocation,
            success: function (json) {
                seasons = eval("(" + json + ")");
                funcToRun(state);
            }
        });
    }
    else {
        funcToRun(state);
    }
}

function isYearInSeasonsData(year) {
    var season = getSeasonForYear(year);
    return season == null ? null : season.Directory.replace('/Archive', '');
}

function getSeasonForYear(year) {
    for (gsfy = 0 ; gsfy < seasons.Season.length ; gsfy++) {
        if (year == seasons.Season[gsfy].Year)
            return seasons.Season[gsfy];
    }

    return null;
}

function getCurrentYear() {
    var loc = window.location.pathname.split("/");
    return Number(loc[loc.length - 2].substring(0, 4));
}

function csvJSON(csv, header, headerValue, skipDecompress) {
    if (skipDecompress != true && usingGZip) {
        csv = JXG.decompress(csv);
    }

    var lines = csv.split("\n");

    var result = [];

    var headers = lines[0].replace("\r", "").split(",");

    for (var i = 1; i < lines.length - 1; i++) {

        var obj = {};
        var currentline = lines[i].replace("\r", "").split(",");

        for (var j = 0; j < headers.length; j++) {
            obj[headers[j]] = currentline[j];
        }

        obj.headers = headers;

        if (header == undefined || headerValue == false || Number(obj[header]) == Number(headerValue)) {
            result.push(obj);
        }
    }

    return result; //JavaScript object
}

function findTeam(json, teamId) {
    var teams = evalJson(json);
    for (var i = 0 ; i < teams.length ; i++) {
        if (teams[i].Id == teamId)
            return teams[i];
    }

    return null;
}


function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}

function getQueryVariable(variable) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == variable) { return pair[1]; }
    }
    return (false);
}

function writeRosterHeader(table) {
    var cells = ["#", "Name", "Year", "Pos", "Hgt", "Lbs", "Ovr", "Spd", "Acc", "Agl", "Str", "Awr", "Home Town"];
    var width = ["3%", "17%", "8%", "8%", "5%", "5%", "5%", "5%", "5%", "5%", "5%", "5%", "22%"];
    addBasicRowsToTable(table, cells, "c10", width);
}

function getYearIndex(year) {
    if (year.indexOf("Fr") > -1)
        return 0;

    if (year.indexOf("So") > -1)
        return 1;

    if (year.indexOf("Jr") > -1)
        return 2;

    return 3;
}

function isOffPlayer(position) {
    var pos = ['QB', 'HB', 'FB', 'WR', 'TE', 'OT', 'OG', 'C', 'LT', 'LG', 'RG', 'RT'];

    for (var i = 0 ; i < pos.length ; i++) {
        if (pos[i] == position)
            return true;
    }

    return false;
}

function isSTPlayer(position) {
    return position == "ST" || position == "K" || position == "P";
}

function getGeneralPosition(position) {
    if (position == "LE" || position == "RE") {
        return "DE";
    }
    else if (position == "LT" || position == "RT") {
        return "OT";
    }
    else if (position == "LG" || position == "RG") {
        return "OG";
    }
    else if (position == "ROLB" || position == "LOLB") {
        return "OLB";
    }
    else if (position == "K" || position == "P") {
        return "ST";
    }

    return position;
}

function getStarterThreshold(position) {
    switch (position) {
        case "HB":
        case "TE":
        case "OT":
        case "OG":
        case "DE":
        case "DT":
        case "OLB":
        case "MLB":
        case "ST":
            return 2;
        case "WR":
        case "CB":
            return 3;
        default:
            return 1;
    }
}

function seasonsContainsYear(seasonFilter) {
    if (seasonFilter == undefined)
        return true;


}

function fanOutCallToSeasons(year, file, successCallback, callsCompleteCallback, fileSelector, objectToPass, objectName, seasonFilter) {
    var calls = [];
    for (var i = 0 ; i < seasons.Season.length && seasons.Season[i].Year <= year; i++) {
        if (!seasonsContainsYear(seasonFilter, seasons.Season[i].Year))
            continue;

        if (fileSelector != null && fileSelector != undefined && file == null) {
            file = fileSelector(seasons.Season[i]);
        }

        var fileUri = seasons.Season[i].Directory.replace('/Archive', '') + "/" + file;
        calls[i] = $.ajax({
            url: fileUri,
            beforeSend: function (xhr) {
                xhr.Year = Number(fileUri.substring(2, 6));
                xhr.Uri = fileUri;
                if (objectName != undefined && objectToPass != undefined) {
                    xhr[objectName] = objectToPass;
                }
            },
            success: successCallback
        });
    }

    $.when.apply(null, calls).then(callsCompleteCallback);
}

String.prototype.getHashCode = function (suffix) {
    var hash = 5381;
    for (var strIdx = 0; strIdx < this.length; strIdx++) {
        var char = this.charCodeAt(strIdx);
        hash = ((hash << 5) + hash) + char;
    }

    if (suffix != undefined)
        hash = hash + suffix;

    return "Z_" + hash;
}

function createSortableHashTable() {
    var obj = new Object();
    obj.List = [];
    return obj;
}

function loadTopSeasons() {
    htmlDir = "./HTML";
    currentData = new Object();
    currentData.Passing = createSortableHashTable();
    currentData.RushingQB = createSortableHashTable();
    currentData.Rushing = createSortableHashTable();
    currentData.Receiving = createSortableHashTable();
    currentData.Defense = createSortableHashTable();
    var year = Number(getQueryVariable("yr"));
    if (year == 0)
        year = 9999;
    topPlayers = getQueryVariable("top") == false ? topPlayers : getQueryVariable("top");

    loadSeasonsJsonData(
    function () {
        fanOutCallToSeasons(year, "leaders.csv", parseBestPlayerOnTeamStats, allTimeStatsLoaded);
    },
    "./Seasons");

}

function parseBestPlayerOnTeamStats(data, status, jqXHR) {
    var stats = csvJSON(data);
    var player = null;

    for (var statIdx = 0 ; statIdx < stats.length ; statIdx++) {
        var current = stats[statIdx];
        var suffix = jqXHR.Year + "-" + current.TeamId;
        player = null;

        // do passing stats first TableIdx=0
        if (current.TableIdx == "0") {
            player = getCurrentPlayer(currentData.Passing, current, jqXHR.Year, suffix);
            foundPassing = true;
        }
        else if (current.TableIdx == "1") {
            var hashTable = null;

            hashTable = currentData.RushingQB;
            foundQBRushing = true;
            player = getCurrentPlayer(hashTable, current, jqXHR.Year, suffix);
        }
        else if (current.TableIdx == "2") {
            var hashTable = null;

            hashTable = currentData.Rushing;
            foundRushing = true;

            player = getCurrentPlayer(hashTable, current, jqXHR.Year, suffix);
        }
        else if (current.TableIdx == "4") {
            player = getCurrentPlayer(currentData.Receiving, current, jqXHR.Year, suffix);
        }
        else if (current.TableIdx == "6" || current.TableIdx == "7" || current.TableIdx == "8") {
            current.Stat3 = Number(current.Stat3) / 10;
            player = getCurrentPlayer(currentData.Defense, current, jqXHR.Year, suffix);
        }

        if (player != null)
            player.Team = { Name: current.Team, Id: current.TeamId };
    }
}

function loadTopPlayersFromServer(cy, teamId, top) {
    var uri = "ncaa.svc/teamGreats?yr=" + cy + "&id=" + teamId + "&top=" + top;

    $.ajax({
        url: uri,
        success: function (data) {
            addServerRowsToTable(document.getElementById("passingTable"), data.AllTimeGreats[0].Rows);
            addServerRowsToTable(document.getElementById("qbrushingTable"), data.AllTimeGreats[1].Rows);
            addServerRowsToTable(document.getElementById("rushingTable"), data.AllTimeGreats[2].Rows);
            addServerRowsToTable(document.getElementById("receivingTable"), data.AllTimeGreats[3].Rows);
            addServerRowsToTable(document.getElementById("tackleTable"), data.AllTimeGreats[4].Rows);
            addServerRowsToTable(document.getElementById("sackTable"), data.AllTimeGreats[5].Rows);
            addServerRowsToTable(document.getElementById("intTable"), data.AllTimeGreats[6].Rows);
        }
    });
}


function loadTopPlayers() {
    htmlDir = "./HTML";
    currentData = new Object();
    currentData.Passing = createSortableHashTable();
    currentData.RushingQB = createSortableHashTable();
    currentData.Rushing = createSortableHashTable();
    currentData.Receiving = createSortableHashTable();
    currentData.Defense = createSortableHashTable();
    var year = Number(getQueryVariable("yr"));
    var teamId = getQueryVariable("id");
    topPlayers = getQueryVariable("top") == false ? topPlayers : getQueryVariable("top");
    var table = document.getElementById("logoTable");
    var row = table.insertRow(-1);
    var cell = row.insertCell(-1);
    cell.className = 'c8';
    cell.width = '100%';
    cell.innerHTML = '<center><img border="0" src="' + createTeamLogoSrc(teamId, 256) + '" /></center>';

    if (usingGZip) {
        loadTopPlayersFromServer(year, teamId, topPlayers);
        return;
    }

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(year, "team" + teamId + "pstat.csv", parseAllTimeStats, allTimeStatsLoaded);
        },
        "./Seasons");
}

function parseAllTimeStats(data, status, jqXHR) {
    var stats = csvJSON(data);
    for (var statIdx = 0 ; statIdx < stats.length ; statIdx++) {
        var current = stats[statIdx];

        // do passing stats first TableIdx=1
        if (current.TableIdx == "1") {
            getCurrentPlayer(currentData.Passing, current, jqXHR.Year);
        }
        else if (current.TableIdx == "2") {
            var hashTable = current.Position == "QB" ? currentData.RushingQB : currentData.Rushing;
            getCurrentPlayer(hashTable, current, jqXHR.Year);
        }
        else if (current.TableIdx == "3") {
            getCurrentPlayer(currentData.Receiving, current, jqXHR.Year);
        }
        else if (current.TableIdx == "5") {
            current.Stat3 = Number(current.Stat3) / 10;
            getCurrentPlayer(currentData.Defense, current, jqXHR.Year);
        }
    }
}

function addValue(obj, field, value) {
    if (obj[field] == undefined) {
        obj[field] = value;
    }
    else {
        obj[field] += value;
    }
}

function getCurrentPlayer(dict, current, year, suffix) {
    var cp = dict[current.Name.getHashCode(suffix)];

    if (cp == undefined) {
        cp = { Name: current.Name, No: current.No, Height: current.Height, Weight: current.Weight, Position: current.Position, Years: [] };
        dict[current.Name.getHashCode()] = cp;
        dict.List.push(cp);
    }

    addValue(cp, "Stat1", Number(current.Stat1));
    addValue(cp, "Stat2", Number(current.Stat2));
    addValue(cp, "Stat3", Number(current.Stat3));
    addValue(cp, "Stat4", Number(current.Stat4));
    addValue(cp, "Stat5", Number(current.Stat5));
    addValue(cp, "Stat6", Number(current.Stat6));
    addValue(cp, "Games", Number(current.Games));
    cp.Years.push(year);

    return cp;
}
// sacks are multiplied by 10
function allTimeStatsLoaded() {
    // Stat3 is passing yards
    currentData.Passing.List.sort(function (a, b) { return b.Stat3 - a.Stat3; });
    currentData.Passing.List = trimTopPlayersList(currentData.Passing);
    fillInAllTimeStatsTable(currentData.Passing.List, document.getElementById("passingTable"), false);

    // Stat2 is rushing yards
    currentData.RushingQB.List.sort(function (a, b) { return b.Stat2 - a.Stat2; });
    currentData.RushingQB.List = trimTopPlayersList(currentData.RushingQB);
    fillInAllTimeStatsTable(currentData.RushingQB.List, document.getElementById("qbrushingTable"), true);

    currentData.Rushing.List.sort(function (a, b) { return b.Stat2 - a.Stat2; });
    currentData.Rushing.List = trimTopPlayersList(currentData.Rushing);
    fillInAllTimeStatsTable(currentData.Rushing.List, document.getElementById("rushingTable"), true);

    // Stat1 is receptions
    currentData.Receiving.List.sort(function (a, b) { return b.Stat1 - a.Stat1; });
    currentData.Receiving.List = trimTopPlayersList(currentData.Receiving);
    fillInAllTimeStatsTable(currentData.Receiving.List, document.getElementById("receivingTable"), false);

    // stat1 is tackles
    currentData.Defense.List.sort(function (a, b) { return b.Stat1 - a.Stat1; });
    fillInAllTimeStatsTable(trimTopPlayersList(currentData.Defense), document.getElementById("tackleTable"), true);

    // stat3 is sacks
    currentData.Defense.List.sort(function (a, b) { return b.Stat3 - a.Stat3; });
    fillInAllTimeStatsTable(trimTopPlayersList(currentData.Defense), document.getElementById("sackTable"), true);

    // stat4 is int
    currentData.Defense.List.sort(function (a, b) { return b.Stat4 - a.Stat4; });
    fillInAllTimeStatsTable(trimTopPlayersList(currentData.Defense), document.getElementById("intTable"), true);
}

function trimTopPlayersList(playersList) {
    return playersList.List.slice(0, playersList.List.length < topPlayers ? playersList.List.length - 1 : topPlayers);
}

function setYearsPlayed(player) {
    if (player.Years.length == 1) {
        player.YearsPlayed = player.Years[0];
    }
    else {
        player.Years.sort(function (a, b) { a - b; });
        player.YearsPlayed = player.Years[0] + "-" + player.Years[player.Years.length - 1];
    }
}

function fillInAllTimeStatsTable(players, table, addStat6) {

    for (var i = 0 ; i < players.length ; i++) {
        var player = players[i];
        setYearsPlayed(player);
        // create our data for our cells
        var cells = [player.YearsPlayed, player.No, "<b>" + player.Name + "</b>", player.Position, player.Height, player.Weight, player.Stat1, player.Stat2];

        cells[cells.length] = player.Stat3;
        cells[cells.length] = player.Stat4;
        cells[cells.length] = player.Stat5;
        if (addStat6 == true) cells[cells.length] = player.Stat6;

        if (player.Team != undefined) {
            cells[cells.length] = "<a href='" + isYearInSeasonsData(player.YearsPlayed) + "/team.html?id=" + player.Team.Id + "'>" + player.Team.Name + "</a>";
        }
        else {
            cells[cells.length] = player.Games;
        }

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadTeamCoachHistoryFromServer(cy, teamId, t1, t2) {
    var uri = "ncaa.svc/teamHistory?yr=" + cy + "&id=" + teamId;

    $.ajax({
        url: uri,
        success: function (data) {
            addServerRowsToTable(t1, data.CoachHistory.Rows);
            addServerRowsToTable(t2, data.TeamHistory.Rows);
        }
    });
}

function loadTeamCoachHistory() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    teamId = Number(getQueryVariable("id"));
    name = unescape(getQueryVariable("name"));
    var table = document.getElementById("coachDataTable");

    var row = table.insertRow(-1);
    var cells = [["4%", "Year"], ["11%", "Head Coach"], ["11%", "Off Coordinator"], ["11%", "Def Coordinator"], ["4%", "Record"], ["8%", "Coaches Poll"], ["8%", "AP Poll"], ["43%", "Summary"]];
    addCellsWithWidth(table, cells, "c7");


    var table2 = document.getElementById("yearlyTalent");

    row = table.insertRow(-1);
    cells = [["5%", "Year"], ["11%", "Head Coach"], ["9%", "Record"], ["9%", "Recruiting"], ["4%", "OVR"], ["4%", "Off"], ["4%", "QB"], ["4%", "RB"], ["4%", "REC"], ["4%", "OL"], ["4%", "DEF"], ["4%", "DL"], ["4%", "LB"], ["4%", "DB"], ["4%", "ST"], ["3%", "Off"], ["3%", "Pass"], ["3%", "Run"], ["3%", "Def"], ["3%", "Pass"], ["3%", "Run"]];
    addCellsWithWidth(table2, cells, "c7");

    if (usingGZip) {
        loadTeamCoachHistoryFromServer(currentYear, teamId, table, table2);
        return;
    }

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(currentYear, "coaches.csv", parseCoachFileForTeam, coachHistoryForTeamLoaded);
        },
        "./Seasons");
}

function coachHistoryForTeamLoaded() {
    var logo = document.getElementById("currentSchool");
    var logo1 = document.getElementById("currentLogo");
    var logo2 = document.getElementById("currentLogo1");

    fanOutCallToSeasons(currentYear, "team",
        function (data, status, jqXHR) {
            var currentSeason = getSeasonForYear(jqXHR.Year);

            var team = findTeam(data, teamId);
            currentSeason.Team = team;

            if (jqXHR.Year == Number(getQueryVariable("yr"))) {
                logo.src = htmlDir + "/Logos/helmet/team" + teamId + ".png";
                logo1.src = htmlDir + "/Logos/256/team" + teamId + ".png";
                logo2.src = htmlDir + "/Logos/256/team" + teamId + ".png";

            }
        },
        teamHistoryYearLoaded);
}

function teamHistoryYearLoaded() {
    var table = document.getElementById("coachDataTable");
    var table2 = document.getElementById("yearlyTalent");

    for (var ii = seasons.Season.length - 1 ; ii >= 0 ; ii--) {
        var currSeason = seasons.Season[ii];

        if (currSeason.Year <= currentYear) {
            var directory = currSeason.Directory.replace('/Archive', '');
            var summary = [];
            var team = currSeason.Team;
            var mediaRank = "-";
            var coachRank = "-";
            var recruitingRank = "-";

            if (team.MediaPollRank <= 25) {
                mediaRank = "#" + team.MediaPollRank;
            }

            if (team.CoachesPollRank <= 25) {
                coachRank = "#" + team.CoachesPollRank;
            }

            if (team.IsNationalChampion == true) {
                summary.push("<b>National Champions</b>");
            }

            if (team.BowlWinsThisYear != undefined && team.BowlWinsThisYear != "" && team.BowlWinsThisYear != null) {
                summary.push("<b>" + team.BowlWinsThisYear + "</b>");
            }

            if (team.ConferenceOrDivisionChampionship != undefined && team.ConferenceOrDivisionChampionship != "" && team.ConferenceOrDivisionChampionship != null) {
                summary.push("<b>" + team.ConferenceOrDivisionChampionship + "</b>");
            }

            if (team.RecruitClassRank != undefined) {
                recruitingRank = team.RecruitClassRank;
            }

            var cells = ["<a href=" + directory + "/Index.html>" + currSeason.Year + "</a>",
                createCoachLink(".", currentYear, currSeason.HC.Name, currSeason.HC.CoachId, ""),
                currSeason.OC == undefined ? "" : createCoachLink(".", currentYear, currSeason.OC.Name, currSeason.OC.CoachId, ""),
                currSeason.DC == undefined ? "" : createCoachLink(".", currentYear, currSeason.DC.Name, currSeason.DC.CoachId, ""),
                //team.Win + "-" + team.Loss,
                createTeamHrefForRecentMeetings(directory, team.Id, "", team.Win, team.Loss, null, true),
                coachRank,
                mediaRank,
                summary.join(", ")
            ];

            // insert the rows
            addBasicRowsToTable(table, cells, "c3");


            cells = ["<a href=" + directory + "/Index.html>" + currSeason.Year + "</a>",
            createCoachLink(".", currentYear, currSeason.HC.Name, currSeason.HC.CoachId, ""),
            createTeamHrefForRecentMeetings(directory, team.Id, "", team.Win, team.Loss, null, true),
            recruitingRank,
            "<b>" + team.TeamRatingOVR + "</b>", "<b>" + team.TeamRatingOFF + "</b>", team.TeamRatingQB, team.TeamRatingRB, team.TeamRatingWR, team.TeamRatingOL, "<b>" + team.TeamRatingDEF + "</b>", team.TeamRatingDL, team.TeamRatingLB, team.TeamRatingDB, "<B>" + team.TeamRatingST + "</b>", team.OffensiveRankings.Overall, team.OffensiveRankings.Passing, team.OffensiveRankings.Rushing, team.DefensiveRankings.Overall, team.DefensiveRankings.Passing, team.DefensiveRankings.Rushing
            ];

            // insert the rows
            addBasicRowsToTable(table2, cells, "c3");
        }
    }
}

function parseCoachFileForTeam(data, status, jqXHR) {
    var coaches = csvJSON(data);

    // find the team we are looking for
    for (var currCoach = 0 ; currCoach < coaches.length ; currCoach++) {
        if (coaches[currCoach].TeamId == teamId) {
            var idx = 0;
            var season = getSeasonForYear(jqXHR.Year);
            season.HC = coaches[currCoach];
            idx++;
            if (coaches.length > (currCoach + idx) && coaches[currCoach + idx].TeamId == season.HC.TeamId && coaches[currCoach + idx].Position == "1") {
                season.OC = coaches[currCoach + idx];
                idx++;
            }

            if (coaches.length > (currCoach + idx) && coaches[currCoach + idx].TeamId == season.HC.TeamId && coaches[currCoach + idx].Position == "2") {
                season.DC = coaches[currCoach + idx];
            }
            break;
        }
    }
}

function loadCoachH2HFromServer(cy, cid, name) {
    var uri = "ncaa.svc/coachH2H?yr=" + cy + "&id=" + cid + "&name=" + escape(name);

    $.ajax({
        url: uri,
        success: function (data) {
            fillBioTable(data.CoachBio);
            var table = document.getElementById("meetingTable");

            for (var v = 0 ; v < data.CoachCareer.Rows.length; v++) {
                addBasicRowsToTable(table, data.CoachCareer.Rows[v].Cells, "c3");
            }
        }
    });

}

function loadCoachH2H() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    currentCoachName = unescape(getQueryVariable("name"));
    currentCoachId = Number(getQueryVariable("id"));

    var header = document.getElementById("coachName");
    header.innerText = currentCoachName;

    var logo = document.getElementById("careerLink");
    logo.href += "?yr=" + currentYear + "&id=" + currentCoachId + "&name=" + escape(currentCoachName);

    if (usingGZip) {
        loadCoachH2HFromServer(currentYear, currentCoachId, currentCoachName);
        return;
    }

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(currentYear, "coaches.csv", parseHeadCoachCareerFile, coachSingleSeasonTeamInfoLoaded);
        },
        "./Seasons");
}

function parseHeadCoachCareerFile(data, status, jqXHR) {
    parseCoachCareerFile(data, status, jqXHR);
}

function coachSingleSeasonTeamInfoLoaded() {

    fanOutCallToSeasons(currentYear, "team",
    function (data, status, jqXHR) {
        var currentSeason = getSeasonForYear(jqXHR.Year);

        if (currentSeason.TeamId != null && currentSeason.TeamId != undefined) {
            var team = findTeam(data, currentSeason.TeamId);
            currentSeason.Team = team;
        }
    },
    coachH2HSeasonsLoaded);
}


function coachH2HSeasonsLoaded() {
    seasonsAsHeadCoach = loadCoachCareerData();

    if (seasonsAsHeadCoach.length > 0) {
        isCoachH2H = true;
        loadSeasonsJsonData(
            function (state) {
                fanOutCallToSeasons(currentYear, "tsch.csv", function (a, b, c) { parseHeadToHeadSchedule(a, b, c, state) }, populateHeadToHeadMeetings, state, true, "IsCoachH2H");
            },
            "./Seasons",
            seasonsAsHeadCoach);
    }
}

function loadCoachPostSeason() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    currentCoachName = unescape(getQueryVariable("name"));
    currentCoachId = Number(getQueryVariable("id"));
    currentFilter = getQueryVariable("filter");


    var append = "yr=" + currentYear + "&id=" + currentCoachId + "&name=" + escape(currentCoachName);

    var logo = document.getElementById("h2hLink");
    logo.href += "?" + append;

    logo = document.getElementById("bowlLink");
    logo.href += append;

    logo = document.getElementById("playoffLink");
    logo.href += append;

    logo = document.getElementById("ccgLink");
    logo.href += append;

    logo = document.getElementById("kogLink");
    logo.href += append;

    var uri = "ncaa.svc/coachpostseason?" + append + "&filter=" + currentFilter;

    $.ajax({
        url: uri,
        success: function (data) {
            for (var x = 0 ; x < data.Rows.length; x++) {
                addBasicRowsToTable(document.getElementById('meetingTable'), data.Rows[x].Cells, 'c3');
            }

            var seriesResultsRow = meetingTable.insertRow(0);
            seriesResultsRow.innerHTML = "<td class=c2 colspan=8><br>" + data.Description + "</b><br><br>";
        }
    });
}

function loadCoachCareer() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    currentCoachName = unescape(getQueryVariable("name"));
    currentCoachId = Number(getQueryVariable("id"));

    var table = document.getElementById("coachDataTable");
    var header = document.getElementById("coachName");
    header.innerText = currentCoachName;

    var append = "yr=" + currentYear + "&id=" + currentCoachId + "&name=" + escape(currentCoachName);

    var logo = document.getElementById("h2hLink");
    logo.href += "?" + append;

    logo = document.getElementById("bowlLink");
    logo.href += append;

    logo = document.getElementById("playoffLink");
    logo.href += append;

    logo = document.getElementById("ccgLink");
    logo.href += append;

    logo = document.getElementById("kogLink");
    logo.href += append;

    var row = table.insertRow(-1);
    var cells = [["4%", "Year"], ["4%", "Age"], ["4%", "Pos"], ["14%", "Team"], ["7%", ""], ["4%", "AP Poll"], ["39%", "Summary"], ["12%", ""], ["12%", ""]];
    addCellsWithWidth(table, cells, "c7");

    if (usingGZip) {
        loadCoachCareerFromServer(currentYear, currentCoachId, currentCoachName);
        return;
    }

    loadSeasonsJsonData(
        function () {
            fanOutCallToSeasons(currentYear, "coaches.csv", parseCoachCareerFile, coachSeasonsLoaded);
        },
        "./Seasons");
}

function loadCoachCareerFromServer(cy, cid, name) {
    var uri = "ncaa.svc/coachCareer?yr=" + cy + "&id=" + cid + "&name=" + escape(name);

    $.ajax({
        url: uri,
        success: function (data) {
            fillBioTable(data.CoachBio);
            var table = document.getElementById("coachDataTable");

            for (var v = 0 ; v < data.CoachCareer.Rows.length; v++) {
                addBasicRowsToTable(table, data.CoachCareer.Rows[v].Cells, "c3");
            }
        }
    });
}

function parseCoachCareerFile(data, status, jqXHR) {
    var coaches = csvJSON(data);
    var teamId = 0;
    var teamName = "";

    // find the coach we are looking for
    for (var currCoach = 0 ; currCoach < coaches.length ; currCoach++) {
        if (coaches[currCoach].Name == currentCoachName && Number(coaches[currCoach].CoachId) == currentCoachId) {
            teamId = coaches[currCoach].TeamId;
            teamName = coaches[currCoach].Team;
            var season = getSeasonForYear(jqXHR.Year);
            season.TeamId = Number(teamId);
            season.Position = Number(coaches[currCoach].Position);

            switch (season.Position) {
                case 0:
                    season.Coach0 = coaches[currCoach];
                    season.Coach1 = coaches[currCoach + 1];
                    season.Coach2 = coaches[currCoach + 2];
                    break;
                case 1:
                    season.Coach0 = coaches[currCoach];
                    season.Coach1 = coaches[currCoach - 1];
                    season.Coach2 = coaches[currCoach + 1];
                    break;
                case 2:
                    season.Coach0 = coaches[currCoach];
                    season.Coach1 = coaches[currCoach - 2];
                    season.Coach2 = coaches[currCoach - 1];
                    break;
            }

            break;
        }
    }
}

// we know where the coach was, what his position was and what team he was on
function coachSeasonsLoaded() {
    var logo = document.getElementById("currentSchool");
    var logo1 = document.getElementById("currentLogo");
    var logo2 = document.getElementById("currentLogo1");


    fanOutCallToSeasons(currentYear, "team",
        function (data, status, jqXHR) {
            var currentSeason = getSeasonForYear(jqXHR.Year);

            if (currentSeason.TeamId != null && currentSeason.TeamId != undefined) {
                var team = findTeam(data, currentSeason.TeamId);
                currentSeason.Team = team;

                if (jqXHR.Year == Number(getQueryVariable("yr"))) {
                    logo.src = htmlDir + "/Logos/helmet/team" + currentSeason.TeamId + ".png";
                    logo1.src = htmlDir + "/Logos/256/team" + currentSeason.TeamId + ".png";
                    logo2.src = htmlDir + "/Logos/256/team" + currentSeason.TeamId + ".png";
                }
            }
        },
        coachTeamLoaded);
}

function getDefaultIntValue(candidate) {
    if (candidate === undefined)
        return 0;

    return candidate;
}

function coachCareerInfoLoaded() {
    var currentSeason = null;
    var foundYear = false;
    for (var si = seasons.Season.length - 1 ; si >= 0 ; si--) {

        if (currentYear >= seasons.Season[si].Year && seasons.Season[si].Team != undefined && currentCoachId == seasons.Season[si].Team.HeadCoach.Id) {

            currentSeason = seasons.Season[si];
            break;
        }
    }

    if (currentSeason != null) {
        fillBioTable(currentSeason.Team.HeadCoach);
    }
}

function fillBioTable(headCoach) {
    var stats = document.getElementById("statsTable");
    var header = document.getElementById("coachName");
    header.innerHTML = headCoach.Name;

    var vsTop25 = getDefaultIntValue(headCoach.Top25Win) + "-" + getDefaultIntValue(headCoach.Top25Loss);
    var vsRivals = getDefaultIntValue(headCoach.RivalWin) + "-" + getDefaultIntValue(headCoach.RivalLoss);
    var careerBowlRecord = getDefaultIntValue(headCoach.CoachBowlWin) + "-" + getDefaultIntValue(headCoach.CoachBowlLoss);
    var allAmericans = getDefaultIntValue(headCoach.AllAmericans);
    var top25Classes = getDefaultIntValue(headCoach.Top25Classes);
    var cotyAwards = getDefaultIntValue(headCoach.CoachOfYearAwards);
    var longestWinStreak = getDefaultIntValue(headCoach.LongestWinStreak);
    var heismanWinners = getDefaultIntValue(headCoach.HeismanWinners);
    var careerCC = getDefaultIntValue(headCoach.CareerConferenceChampionships);
    var careerNC = getDefaultIntValue(headCoach.CareerNationalChampionships);

    addCellsWithWidth(statsTable, [["25%", "<b>Age</b>"], ["25%", headCoach.Age], ["25%", "<b>Alma Mater</b>"], ["25%", headCoach.AlmaMaterName]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>Conference Championships</b>"], ["25%", careerCC], ["25%", "<b>National Championships</b>"], ["25%", careerNC]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>Career Record</b>"], ["25%", headCoach.CareerRecord], ["25%", "<b>Team Record</b>"], ["25%", headCoach.TeamRecord]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>Career Bowl Record</b>"], ["25%", careerBowlRecord]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>vs Rivals</b>"], ["25%", vsRivals], ["25%", "<b>vs Top 25</b>"], ["25%", vsTop25]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>All Americans</b>"], ["25%", allAmericans], ["25%", "<b>Top 25 Recruiting Classes</b>"], ["25%", top25Classes]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>Heisman Winners</b>"], ["25%", heismanWinners], ["25%", "<b>Coach of the Year Awards</b>"], ["25%", cotyAwards]], "c3");
    addCellsWithWidth(statsTable, [["25%", "<b>Longest Win Streak</b>"], ["25%", longestWinStreak]], "c3");
}

// we know all about the coaches team, so now we get the following data for position
// HC : Record, Media Poll/Coaches Poll, Conference Championship Status, Bowl Game Win, Recruit Rank
// OC : Record, Total Off Rank, Pass Off Rank, Rush Off Rank  
// DC : Record, Total Def Rank, Pass Def rank, Rush Def Rank  
function coachTeamLoaded() {
    var table = document.getElementById("coachDataTable");
    coachCareerInfoLoaded();

    for (var ii = seasons.Season.length - 1 ; ii >= 0 ; ii--) {
        var currSeason = seasons.Season[ii];

        if (currSeason.TeamId == null || currSeason.TeamId == undefined)
            continue;

        var directory = currSeason.Directory.replace('/Archive', '');
        var summary = [];
        var team = currSeason.Team;
        var mediaRank = "-";

        if (team.MediaPollRank <= 25) {
            mediaRank = "#" + team.MediaPollRank;
        }

        if (team.IsNationalChampion == true) {
            summary.push("<b>National Champions</b>");
        }

        if (team.BowlWinsThisYear != undefined && team.BowlWinsThisYear != "" && team.BowlWinsThisYear != null) {
            summary.push("<b>" + team.BowlWinsThisYear + "</b>");
        }

        if (team.ConferenceOrDivisionChampionship != undefined && team.ConferenceOrDivisionChampionship != "" && team.ConferenceOrDivisionChampionship != null) {
            summary.push("<b>" + team.ConferenceOrDivisionChampionship + "</b>");
        }

        var job = "HC";

        // 0 is head coach, 1 is oc, 2 is dc
        if (currSeason.Position == 0) {

            if (team.RecruitClassRank != undefined) {
                summary.push("#" + team.RecruitClassRank + " Recruiting Class");
            }
        }
        else if (currSeason.Position == 1) {
            summary.push("#" + team.OffensiveRankings.Overall + " Offense");
            summary.push("#" + team.OffensiveRankings.Passing + " Passing Offense");
            summary.push("#" + team.OffensiveRankings.Rushing + " Rushing Offense");
            job = "OC";
        }
        else if (currSeason.Position == 2) {
            summary.push("#" + team.DefensiveRankings.Overall + " Defense");
            job = "DC";
        }

        var teamName = currSeason.Team.Name;
        if (team.CoachesPollRank <= 25) {
            teamName = "#" + team.CoachesPollRank + " " + teamName;
        }

        var coach1 = currSeason.Coach1 == undefined ? "" : createCoachLink(".", currentYear, currSeason.Coach1.Name, currSeason.Coach1.CoachId, currSeason.Coach1.PositionName.split(' ')[0].charAt(0) + currSeason.Coach1.PositionName.split(' ')[1].charAt(0) + ": ");
        var coach2 = currSeason.Coach2 == undefined ? "" : createCoachLink(".", currentYear, currSeason.Coach2.Name, currSeason.Coach2.CoachId, currSeason.Coach2.PositionName.split(' ')[0].charAt(0) + currSeason.Coach2.PositionName.split(' ')[1].charAt(0) + ": ");


        var cells = ["<a href=" + directory + "/Index.html>" + currSeason.Year + "</a>", currSeason.Coach0.Age,
            job,
            createTeamHrefForRecentMeetings(directory, currSeason.Team.Id, teamName, currSeason.Team.Win, currSeason.Team.Loss, null, true),
            "<a href='./TeamHistory.html?yr=" + currentYear + "&id=" + currSeason.Team.Id + "'><img border='0' src='" + createTeamLogoSrc(currSeason.Team.Id, 35) + "' /></a>",
            mediaRank,
            summary.join(", "),
            coach1,
            coach2
        ];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function findSeason(seasonsForCoach, year) {
    for (ii = 0 ; ii < seasonsForCoach.length; ii++) {
        if (seasonsForCoach[ii].Year == year)
            return seasonsForCoach[ii];
    }

    return null;
}

function parseHeadToHeadSchedule(data, status, jqXHR, seasonsForCoach) {
    var singleTeam = currentTeam;

    if (seasonsForCoach != undefined) {
        var season = findSeason(seasonsForCoach, jqXHR.Year);
        if (season == null)
            return;

        singleTeam = season.TeamId;
    }

    var scheduleForYear = csvJSON(data);
    var foundTeam = false;

    for (j = 0 ; j < scheduleForYear.length ; j++) {
        if (scheduleForYear[j].TeamId == singleTeam) {
            foundTeam = true;
        }
        else if (foundTeam) {
            // no longer on that teams schedule
            break;
        }

        if (scheduleForYear[j].OppId == "")
            continue;

        if (foundTeam) {
            scheduleForYear[j].Year = Number(jqXHR.Year);
            var key = "T" + scheduleForYear[j].OppId;

            // add to hash table if not exists
            if (headToHeadResults[key] == undefined) {
                headToHeadResults[key] = [];
                headToHeadResults[key].Win = 0;
                headToHeadResults[key].Loss = 0;
                headToHeadResults[key].Opponent = scheduleForYear[j].Opponent.replace(/[^a-zA-Z &]+/g, '');
                headToHeadResults[key].OpponentId = scheduleForYear[j].OppId;
            }

            headToHeadResults[key].push(scheduleForYear[j]);
            if (scheduleForYear[j].Result == "Win")
                headToHeadResults[key].Win++;
            else
                headToHeadResults[key].Loss++;
        }
    }
}

function sortByRecentMeeting() {
    window.location = window.location + "&sort=true";
}

function sortByH2H() {
    window.location = window.location.href.replace("&sort=true", "");
}

function populateHeadToHeadMeetings() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    teamId = Number(getQueryVariable("id"));
    var sort = getQueryVariable("sort");

    var table = document.getElementById("meetingTable");
    var results = [];
    for (var prop in headToHeadResults) {
        results[results.length] = headToHeadResults[prop];
    }

    if (sort == "true") {
        results.sort(function (a, b) { return b[b.length - 1].Year - a[a.length - 1].Year; });
    }
    else {
        results.sort(function (a, b) { return (b.Win + b.Loss) - (a.Win + a.Loss); });
    }

    var directory = seasons.Season[seasons.Season.length - 1].Directory.replace("Archive", "");

    for (var kk = 0 ; kk < results.length ; kk++) {
        var current = results[kk];
        var lastMeeting = results[kk][results[kk].length - 1];
        var directory = isYearInSeasonsData(lastMeeting.Year);
        var lmLink = "<a href=" + directory + "/boxscore.html?id=" + lastMeeting.Week + "-" + lastMeeting.Game + ">" + lastMeeting.Year + "</a>";

        var firstCell = null;
        var lastCell = "";

        if (isCoachH2H) {
            var name = getQueryVariable("name");
            lastCell = "<a href='coachrecentmeetings.html?id=" + teamId + "&opp=" + current.OpponentId + "&yr=" + currentYear + "&name=" + name + "'>Recent Meetings</a>";
        }
        else {
            firstCell = createTeamHrefForRecentMeetings(directory, teamId, null, null, null, 35);
            lastCell = "<a href='recentmeetings.html?id=" + teamId + "&opp=" + current.OpponentId + "&yr=" + currentYear + "'>Recent Meetings</a>";
        }

        var cells = [
                            current.Win + "-" + current.Loss,
                            createTeamHrefForRecentMeetings(directory, current.OpponentId, null, null, null, 35),
                            "<a href='HeadToHead.html?id=" + current.OpponentId + "&yr=" + currentYear + "'>" + current.Opponent + "</a>",
                            lmLink,
                            lastCell,

        ];

        if (firstCell != null) {
            cells.splice(0, 0, firstCell);
        }

        addBasicRowsToTable(document.getElementById('meetingTable'), cells, 'c3');
    }
}

function loadHeadToHeadMeetingsFromServer(cy, teamId) {
    var uri = "ncaa.svc/teamH2H?yr=" + cy + "&id=" + teamId + "&sort=" + getQueryVariable("sort");

    $.ajax({
        url: uri,
        success: function (data) {
            var table = document.getElementById("meetingTable");
            for (var v = 0 ; v < data.Rows.length; v++) {
                addBasicRowsToTable(table, data.Rows[v].Cells, "c3");
            }
        }
    });
}


function loadHeadToHeadMeetings() {
    htmlDir = "./HTML";
    currentYear = Number(getQueryVariable("yr"));
    currentTeam = getQueryVariable("id");
    var logo = document.getElementById("currentSchool");
    logo.src = htmlDir + "/Logos/256/team" + currentTeam + ".png";
    var results = [];
    var lookback = Number(getQueryVariable("past"));
    var calls = [];

    if (usingGZip) {
        loadHeadToHeadMeetingsFromServer(currentYear, currentTeam);
        return;
    }

    loadSeasonsJsonData(
function () {
    fanOutCallToSeasons(currentYear, "tsch.csv", parseHeadToHeadSchedule, populateHeadToHeadMeetings);
},
"./Seasons");
}

function loadCoachRecentMeetingsFromServer(cy, cid, name, filter) {
    var uri = "ncaa.svc/coachH2H?yr=" + cy + "&id=" + cid + "&name=" + escape(name) + "&filter=" + filter;

    $.ajax({
        url: uri,
        success: function (data) {
            fillBioTable(data.CoachBio);
            var table = document.getElementById("meetingTable");

            fillInSeriesResult(data.CoachCareer.Description, table);

            for (var v = 0 ; v < data.CoachCareer.Rows.length; v++) {
                addBasicRowsToTable(table, data.CoachCareer.Rows[v].Cells, "c3");
            }
        }
    });
}

function loadCoachRecentMeetings() {
    htmlDir = "./HTML";
    var year = Number(getQueryVariable("yr"));
    currentYear = year;
    currentCoachName = unescape(getQueryVariable("name"));
    currentCoachId = Number(getQueryVariable("id"));
    var teamId = getQueryVariable("id");
    var oppId = getQueryVariable("opp");
    var name = unescape(getQueryVariable("name"));

    var results = [];
    var calls = [];
    var oppTeam = null;
    var wins = 0;
    var losses = 0;
    var logo = document.getElementById("h2hLink");
    logo.href += "?yr=" + year + "&id=" + teamId + "&name=" + escape(name);

    if (usingGZip) {
        loadCoachRecentMeetingsFromServer(currentYear, currentCoachId, currentCoachName, oppId);
        return;
    }

    loadSeasonsJsonData(
    function () {
        fanOutCallToSeasons(currentYear, "coaches.csv", parseHeadCoachCareerFile, coachRecentMeetingsSingleSeasonTeamInfoLoaded);
    },
    "./Seasons");
}

function coachRecentMeetingsSingleSeasonTeamInfoLoaded() {

    fanOutCallToSeasons(currentYear, "team",
    function (data, status, jqXHR) {
        var currentSeason = getSeasonForYear(jqXHR.Year);

        if (currentSeason.TeamId != null && currentSeason.TeamId != undefined) {
            var team = findTeam(data, currentSeason.TeamId);
            currentSeason.Team = team;
        }
    },
    coachH2HSeasonsForRecentMeetingsLoaded);
}

function loadCoachCareerData() {
    coachCareerInfoLoaded();

    seasonsAsHeadCoach = [];

    for (var ii = seasons.Season.length - 1 ; ii >= 0 ; ii--) {
        var currSeason = seasons.Season[ii];

        if (currSeason.TeamId == null || currSeason.TeamId == undefined)
            continue;

        // if head coach we need to get the schedule for that year
        if (currSeason.Position == 0) {
            seasonsAsHeadCoach.push(currSeason);
        }
    }

    return seasonsAsHeadCoach;
}

function coachH2HSeasonsForRecentMeetingsLoaded() {
    seasonsAsHeadCoach = loadCoachCareerData();
    var oppId = getQueryVariable("opp");

    if (seasonsAsHeadCoach.length > 0) {
        isCoachH2H = true;

        loadMeetingsBetweenTeams(
            currentYear,
            function (yearGamePlayed) {
                for (var jj = 0 ; jj < seasonsAsHeadCoach.length ; jj++) {
                    if (seasonsAsHeadCoach[jj].Year == yearGamePlayed) {
                        return seasonsAsHeadCoach[jj].TeamId;
                    }
                }

                return null;
            },
            oppId)
    }
}


function loadRecentMeetingsFromServer(cy, teamId, oppId) {
    var uri = "ncaa.svc/teamH2H?yr=" + cy + "&id=" + teamId + "&filter=" + oppId;

    $.ajax({
        url: uri,
        success: function (data) {
            var table = document.getElementById("meetingTable");
            fillInSeriesResult(data.Description, table);

            for (var v = 0 ; v < data.Rows.length; v++) {
                addBasicRowsToTable(table, data.Rows[v].Cells, "c3");
            }
        }
    });
}


function loadRecentMeetings() {
    htmlDir = "./HTML";
    var year = Number(getQueryVariable("yr"));
    var teamId = getQueryVariable("id");
    var oppId = getQueryVariable("opp");
    populateLogoTable(oppId, teamId);
    var logo = document.getElementById("h2hLink");
    logo.href += "?yr=" + year + "&id=" + teamId;

    if (usingGZip) {
        loadRecentMeetingsFromServer(year, teamId, oppId);
        return;
    }

    loadMeetingsBetweenTeams(year, function (yearGamePlayed) { return teamId; }, oppId);
}

function loadMeetingsBetweenTeams(year, getTeamId, oppId) {
    var results = [];
    var calls = [];
    var oppTeam = null;
    var wins = 0;
    var losses = 0;

    loadSeasonsJsonData(
        function () {
            for (i = 0 ; i < seasons.Season.length && seasons.Season[i].Year <= year; i++) {

                var scheduleUri = seasons.Season[i].Directory.replace('/Archive', '') + "/tsch.csv";
                calls[i] = $.ajax({
                    url: scheduleUri,
                    beforeSend: function (xhr) {
                        xhr.Year = scheduleUri.substring(2, 6);
                    },
                    success: function (data, status, jqXHR) {
                        var scheduleForYear = csvJSON(data);
                        var foundTeam = false;
                        var yearForGame = Number(jqXHR.Year);

                        for (j = 0 ; j < scheduleForYear.length ; j++) {
                            var teamId = getTeamId(yearForGame);

                            if (teamId == null) {
                                continue;
                            }

                            if (scheduleForYear[j].TeamId == teamId) {
                                foundTeam = true;
                            }
                            else if (foundTeam == true) {
                                break;
                            }
                            if (scheduleForYear[j].OppId == oppId && foundTeam) {
                                scheduleForYear[j].Year = yearForGame;
                                results.push(scheduleForYear[j]);
                            }

                        }
                    }
                });
            }

            var getLatestTeam = isYearInSeasonsData(year) + "/team";

            calls[calls.length] = $.ajax({
                url: getLatestTeam,
                success: function (data) {
                    myTeam = findTeam(data, getTeamId(year));
                    oppTeam = findTeam(data, oppId);
                }
            });

            $.when.apply(null, calls).then(
                function () {

                    results.sort(function (a, b) {
                        var diff = b.Year - a.Year;
                        if (diff == 0) {
                            diff = Number(b.Week) - Number(a.Week);
                        }

                        return diff;
                    });
                    for (k = 0 ; k < results.length ; k++) {
                        var directory = isYearInSeasonsData(results[k].Year);
                        var site = "";
                        var boldCellIndex = 0;

                        if (results[k].Result != "Win") {
                            boldCellIndex = 4;
                            losses++;
                        }
                        else {
                            wins++;
                        }

                        site = results[k].Location;
                        var tw = null;
                        var tl = null;

                        if (myTeam != null) {
                            tw = myTeam.Win;
                            tl = myTeam.Loss;
                        }

                        var cells = [
                            createTeamHrefForRecentMeetings(directory, getTeamId(results[k].Year), results[k].TeamName, tw, tl),
                            createTeamHrefForRecentMeetings(directory, getTeamId(results[k].Year), null, null, null, 35),
                            "<a href=" + directory + "/boxscore.html?id=" + results[k].Week + "-" + results[k].Game + ">" + results[k].Score + "</a>",
                            createTeamHrefForRecentMeetings(directory, oppId, null, null, null, 35),
                            createTeamHrefForRecentMeetings(directory, oppId, results[k].Opponent, oppTeam.Win, oppTeam.Loss),
                            site,
                            "<a href=" + directory + "/index.html>" + results[k].Year + "</a>"
                        ];

                        cells[boldCellIndex] = "<b><font size=2>" + cells[boldCellIndex] + "</font></b>";

                        var header = "";

                        table = document.getElementById('breakdownTable');
                        addBasicRowsToTable(document.getElementById('meetingTable'), cells, 'c3');
                    }

                    var myName = isCoachH2H ? currentCoachName : myTeam.Name;

                    if (wins != losses) { header = (wins > losses ? myName + " leads the series " + wins + "-" + losses : oppTeam.Name + " leads the series " + losses + "-" + wins); }
                    else { header = "Series is tied " + wins + "-" + losses; }

                    var seriesResultsRow = meetingTable.insertRow(0);

                    seriesResultsRow.innerHTML = "<td class=c2 colspan=8><br>" + header + "</b><br><br>";

                });

        },
        "./Seasons");

}

function fillInSeriesResult(header, table) {
    var seriesResultsRow = table.insertRow(0);
    seriesResultsRow.innerHTML = "<td class=c2 colspan=8><br>" + header + "</b><br><br>";
}

function loadNewBowlRecords() {
    htmlDir = "./HTML";
    var year = currentYear = Number(getQueryVariable("yr"));
    year -= startingYear;

    loadSeasonsJsonData(
    function () {
        $.ajax({
            url: "bowlrecords",
            success: function (json) {
                records = evalJson(json);

                var meetingTable = document.getElementById('meetingTable');

                for (var xx = 0 ; xx < records.Records.length; xx++) {
                    var bowl = records.Records[xx];

                    calculateYardAvg(bowl, ["PlayerRushingYPC", "PlayerYardsPerPass", "PlayerYardsPerRec", "BestKRAvg", "BestPRAvg"]);
                    writeDescription(bowl, records.Teams);
                    preparePassComp(bowl["PlayerCompletionPct"]);

                    for (var record in bowl) {
                        if (bowl[record] == null) continue;

                        for (var c = 0 ; c < bowl[record].length ; c++) {
                            var actualRecord = bowl[record][c];

                            if (actualRecord.Year == year) {

                                addNewRecord(meetingTable, bowl, record, actualRecord)

                                break;
                            }
                        }
                    }
                }
            }
        });
    }, "./Seasons");
}

function addNewRecord(recordTable, bowl, recordName, record) {
    realYear = record.Year + startingYear;
    directory = isYearInSeasonsData(realYear);
    var isPlayer = record.Player != undefined;
    var stat = record.Value;

    if (stat == undefined) {
        stat = record.PointDiff;
        if (stat == undefined) {
            stat = record.Points;
            if (stat == undefined) stat = record.Yards;
        }
    }

    var cells = [
        "<a href=BowlRecords.html?id=" + bowl.BowlId + ">" + bowl.Name + " " + recordName + "</a>",
        createTeamHrefForRecentMeetings(directory, record.By, null, null, null, 35),
        "<a href=" + directory + "/boxscore.html?id=" + record.GameId + ">" + record.Score + "</a>",
        createTeamHrefForRecentMeetings(directory, record.Against, null, null, null, 35),
        record.Description == undefined ? "" : record.Description,
        stat,
        "<a href=" + directory + "/index.html>" + realYear + "</a>"
    ];

    addBasicRowsToTable(recordTable, cells, 'c3');
}

function loadBowlRecords() {
    htmlDir = "./HTML";
    var year = currentYear = Number(getQueryVariable("yr"));
    var bowlId = Number(getQueryVariable("id"));
    //    var divide = getQueryVariable("sep");
    loadBowlInfoPage();

    loadSeasonsJsonData(
    function () {
        $.ajax({
            url: "bowlrecords",
            success: function (json) {
                var isPlayer = getQueryVariable("player");
                var filter = getQueryVariable("filter");
                records = evalJson(json);
                br = findBowlRecords(records, bowlId);
                var meetingTable = document.getElementById('meetingTable');
                calculateYardAvg(br, ["PlayerRushingYPC", "PlayerYardsPerPass", "PlayerYardsPerRec", "BestKRAvg", "BestPRAvg"]);
                writeDescription(br, records.Teams);
                preparePassComp(br["PlayerCompletionPct"]);

                if (filter == null || filter == undefined || filter == false) {
                    addTeamRecord(meetingTable, "Biggest Win", br, "BiggestWins", "PointDiff", "Large Point Differential");
                    addTeamRecord(meetingTable, "Most Points", br, "MostPoints", "Points", "Most Points Scored");
                    addTeamRecord(meetingTable, "Least Points", br, "LeastPoints", "Points", "Least Points Allowed");
                    addTeamRecord(meetingTable, "Most Combined Points", br, "MostCombinedPoints", "Points", "Most Combined Points Scored");
                    addTeamRecord(meetingTable, "Least Combined Points", br, "FewestCombinedPoints", "Points", "Least Combined Points Scored");
                    addTeamRecord(meetingTable, "Offensive Yards", br, "MostOffensiveYards", "Yards", "Most Yards of Offense");
                    addTeamRecord(meetingTable, "Passing Yards", br, "MostPassingYards", "Yards", "Most Yards Passing");
                    addTeamRecord(meetingTable, "Rushing Yards", br, "MostRushingYards", "Yards", "Most Yards Rushing");
                    addTeamRecord(meetingTable, "Least Offensive Yards", br, "LeastOffensiveYards", "Yards", "Least Yards of Offense Allowed");
                    addTeamRecord(meetingTable, "Least Passing Yards", br, "LeastPassingYards", "Yards", "Least Yards Passing Allowed");
                    addTeamRecord(meetingTable, "Least Rushing Yards", br, "LeastRushingYards", "Yards", "Least Yards Rushing Allowed");

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Total Offense", br, "PlayerTotalOffense", records);
                    addPlayerRecord(meetingTable, "Offensive TD", br, "PlayerOffensiveTD", records);
                    addPlayerRecord(meetingTable, "All Purpose Yards", br, "AllPurposeYards", records);
                    addPlayerRecord(meetingTable, "All Purpose TD", br, "AllPurposeTD", records);

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Passing Attempts", br, "PlayerPassAtt", records);
                    addPlayerRecord(meetingTable, "Passing Completions", br, "PlayerCompletions", records);
                    addPlayerRecord(meetingTable, "Passing Yards", br, "PlayerPassingYards", records);
                    addPlayerRecord(meetingTable, "Passing TD", br, "PlayerPassingTD", records);
                    addPlayerRecord(meetingTable, "Passing Completion %", br, "PlayerCompletionPct", records);
                    addPlayerRecord(meetingTable, "Passing YPA", br, "PlayerYardsPerPass", records)
                    addPlayerRecord(meetingTable, "Longest Pass", br, "LongestPass", records)

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Rushing Attempts", br, "PlayerRushingAtt", records);
                    addPlayerRecord(meetingTable, "Rushing Yards QB", br, "PlayerRushingYdsQB", records);
                    addPlayerRecord(meetingTable, "Rushing Yards", br, "PlayerRushingYdsNonQB", records);
                    addPlayerRecord(meetingTable, "Rushing YPA", br, "PlayerRushingYPC", records);
                    addPlayerRecord(meetingTable, "Rushing TD", br, "PlayerRushingTD", records);
                    addPlayerRecord(meetingTable, "Longest Rush", br, "LongestRush", records);

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Receptions", br, "PlayerReceptions", records);
                    addPlayerRecord(meetingTable, "Receiving Yards", br, "PlayerRecYds", records);
                    addPlayerRecord(meetingTable, "Receiving TD", br, "PlayerRecTD", records);
                    addPlayerRecord(meetingTable, "Receiving YPC", br, "PlayerYardsPerRec");
                    addPlayerRecord(meetingTable, "Longest Reception", br, "LongestRec", records);

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Tackles", br, "PlayerTackles", records);
                    addPlayerRecord(meetingTable, "Tackles for Loss", br, "PlayerTFL", records);
                    addPlayerRecord(meetingTable, "Sacks", br, "PlayerSacks", records);
                    addPlayerRecord(meetingTable, "Passes Defended", br, "PlayerPassDef", records);
                    addPlayerRecord(meetingTable, "Interceptions", br, "PlayerINT", records);
                    addPlayerRecord(meetingTable, "Int Return Yards", br, "MostIntReturnYards", records);
                    addPlayerRecord(meetingTable, "Int Return TD", br, "MostIntTD", records);
                    addPlayerRecord(meetingTable, "Int Return Long", br, "LongestIntReturn", records);
                    addPlayerRecord(meetingTable, "Fumble Recovery Yards", br, "MostFumbleRecYds", records);
                    addPlayerRecord(meetingTable, "Fumble Return TD", br, "MostFumbleRecTD", records);

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Kick Return Yards", br, "MostKRYards", records);
                    addPlayerRecord(meetingTable, "Highest Kick Return Average", br, "BestKRAvg");
                    addPlayerRecord(meetingTable, "Longest Kick Return", br, "LongestKR", records);
                    addPlayerRecord(meetingTable, "Kick Return TD", br, "MostKRTD", records);

                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    addPlayerRecord(meetingTable, "Punt Return Yards", br, "MostPRYards", records);
                    addPlayerRecord(meetingTable, "Highest Punt Return Average", br, "BestPRAvg", records);
                    addPlayerRecord(meetingTable, "Longest Punt Return", br, "LongestPR", records);
                    addPlayerRecord(meetingTable, "Punt Return TD", br, "MostPRTD", records);
                }
                else {
                    var name = unescape(getQueryVariable("name"));
                    var statName = unescape(getQueryVariable("stat"));
                    var desc = unescape(getQueryVariable("desc"));

                    if (isPlayer == "true") {
                        addTeamRecord(meetingTable, name, br, filter, "Value", null, true);
                    }
                    else {
                        addTeamRecord(meetingTable, name, br, filter, statName, desc, true);
                    }
                }
            }
        });
    }, "./Seasons");
}

function writeDescription(br, map) {
    for (var prop in br) {

        if (br[prop] == null) continue;

        for (var jj = 0 ; jj < br[prop].length; jj++) {
            if (br[prop][jj].Player != undefined) {
                br[prop][jj].Description = createPlayerDescription(br[prop][jj], map);
            }
        }
    }
}

function preparePassComp(br) {
    for (var jj = 0 ; jj < br.length; jj++) {
        br[jj].Value += "%";
    }
}

function calculateYardAvg(br, statNames) {

    for (var jj = 0 ; jj < statNames.length ; jj++) {
        var statName = statNames[jj];

        for (var ii = 0 ; ii < br[statName].length ; ii++) {
            var stat = br[statName][ii];
            stat.Value = stat.Value / 10;
        }
    }
}

function addPlayerRecord(rt, name, br, column, r) {
    addTeamRecord(rt, name, br, column, "Value");
}

function getTeamNameFromMap(id, map) {
    for (var mm = 0 ; mm < map.length ; mm++) {
        if (map[mm].TeamId == id)
            return map[mm].School;
    }
}

function createPlayerDescription(record, map) {
    return "#" + record.Number + " " + record.Position + " " + record.Player + ", " + getTeamNameFromMap(record.By, map);
}

function addTeamRecord(recordTable, title, br, column, statName, desc, all) {
    var bowlId = Number(getQueryVariable("id"));

    if (all == undefined)
        length = 1;
    else
        length = br[column].length;

    var realYear = null;
    var directory = null;

    for (var jj = 0 ; jj < length; jj++) {
        if (br[column] == undefined)
            return;

        var record = br[column][jj];
        if (record == undefined) return;

        var stat = record[statName];

        realYear = record.Year + startingYear;
        directory = isYearInSeasonsData(realYear);
        var isPlayer = record.Player != undefined;

        var cells = [
            "<a href=BowlRecords.html?id=" + bowlId + "&filter=" + column + "&name=" + escape(title) + "&stat=" + escape(statName) + "&desc=" + escape(desc) + "&player=" + isPlayer + ">" + title + "</a>",
            createTeamHrefForRecentMeetings(directory, record.By, null, null, null, 35),
            "<a href=" + directory + "/boxscore.html?id=" + record.GameId + ">" + record.Score + "</a>",
            createTeamHrefForRecentMeetings(directory, record.Against, null, null, null, 35),
            (desc == null || desc == undefined) ? record.Description : desc,
            stat,
            "<a href=" + directory + "/index.html>" + realYear + "</a>"
        ];

        addBasicRowsToTable(recordTable, cells, 'c3');
    }
}

function findBowlRecords(records, bowlId) {
    for (var xx = 0 ; xx < records.Records.length; xx++) {
        if (records.Records[xx].BowlId == bowlId) {
            return records.Records[xx];
        }
    }
}

function loadBowlInfoPage() {
    htmlDir = "./HTML";
    var year = currentYear = Number(getQueryVariable("yr"));
    var bowlId = Number(getQueryVariable("id"));
    var divide = getQueryVariable("sep");

    // TODO load the bowl logo
    if (bowlId != undefined) {
        var logo = document.getElementById("currentBowl");
        logo.src = "./HTML/Logos/bowls/" + bowlId + ".jpg";
        var trophy = document.getElementById("currentBowlTrophy");
        trophy.src = "./HTML/Logos/bowl_trophies/" + bowlId + ".png";
    }
    if (bowlId == -1) {
        var logo = document.getElementById("currentBowl");
        logo.src = "./HTML/Logos/bowls/39.jpg";
        var trophy = document.getElementById("currentBowlTrophy");
        trophy.src = "./HTML/Logos/bowl_trophies/39.png";
    }

    var recordLink = document.getElementById("recordLink");
    recordLink.href += "?id=" + bowlId + "&yr=" + year;

    var newRecordLink = document.getElementById("newRecordLink");
    if (newRecordLink != undefined && newRecordLink != null) {
        newRecordLink.href += "?yr=" + year;
    }
}

function loadBowlHistoryFromServer(yr, bowlId) {
    var uri = null;

    if (bowlId < 0) {
        uri = "ncaa.svc/groupHistory?yr=" + yr + "&group=" + (bowlId == -1 ? "playoff" : "kickoff");
    }
    else {
        uri = "ncaa.svc/bowlHistory?yr=" + yr + "&id=" + bowlId;
    }

    $.ajax({
        url: uri,
        success: function (data) {
            var meetingTable = document.getElementById('meetingTable');
            var currYear = data.Rows[0].Year;
            for (var x = 0 ; x < data.Rows.length; x++) {
                if (currYear != data.Rows[x].Year) {
                    addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                    currYear = data.Rows[x].Year;
                }

                addBasicRowsToTable(meetingTable, data.Rows[x].Cells, 'c3');
            }
        }
    });
}

function loadBowlHistory() {
    htmlDir = "./HTML";
    var year = currentYear = Number(getQueryVariable("yr"));
    var bowlId = Number(getQueryVariable("id"));
    var divide = getQueryVariable("sep");
    loadBowlInfoPage();

    if (usingGZip) {
        loadBowlHistoryFromServer(year, bowlId);
        return;
    }

    // start
    var results = [];
    var calls = [];

    var weekCheck = function (gp) { return gp.Week >= 16 };

    if (bowlId > 200 || bowlId == -2) {
        weekCheck = function (gp) { return gp.Week <= 2; }
    }

    loadSeasonsJsonData(
        function () {
            for (i = 0 ; i < seasons.Season.length && seasons.Season[i].Year <= year; i++) {

                var scheduleUri = seasons.Season[i].Directory.replace('/Archive', '') + "/tsch.csv";
                calls[i] = $.ajax({
                    url: scheduleUri,
                    beforeSend: function (xhr) {
                        xhr.Year = scheduleUri.substring(2, 6);
                    },
                    success: function (data, status, jqXHR) {
                        var scheduleForYear = csvJSON(data);
                        var foundTeam = false;
                        var bowlFound = [];
                        var bowls = [];

                        if (bowlId == -1) {
                            bowls = getPlayoffGames(jqXHR.Year);
                            for (y = 0 ; y < bowls.length ; y++)
                                bowlFound[y] = false;
                        }
                        else if (bowlId == -2) {
                            bowls = [273, 271, 272, 276];
                            for (y = 0 ; y < bowls.length ; y++)
                                bowlFound[y] = false;
                        }
                        else {
                            bowls = [bowlId];
                            bowlFound = [false];
                        }

                        for (j = 0 ; j < scheduleForYear.length ; j++) {
                            if (!weekCheck(scheduleForYear[j]))
                                continue;

                            var match = bowls.indexOf(Number(scheduleForYear[j].BowlId));
                            if (match >= 0) {
                                scheduleForYear[j].Year = Number(jqXHR.Year);

                                // add only if we haven't found it yet
                                if (bowlFound[match] == false) {
                                    results.push(scheduleForYear[j]);
                                    bowlFound[match] = true;
                                }

                                if (bowlFound.indexOf(false) == -1) {
                                    break;
                                }
                            }
                        }
                    }
                });
            }

            var getLatestTeam = isYearInSeasonsData(year) + "/team";

            $.when.apply(null, calls).then(
                function () {

                    results.sort(function (a, b) {
                        var diff = b.Year - a.Year;

                        if (diff == 0) {
                            if (Number(b.BowlId) > 200) {
                                return a.BowlId - b.BowlId;
                            }
                            else if (b.BowlId == "39") {
                                return 1;
                            }
                            else {
                                return -1;
                            }
                        }

                        return diff;
                    });

                    var currYear = results[0].Year;
                    var meetingTable = document.getElementById('meetingTable');
                    for (k = 0 ; k < results.length ; k++) {

                        if (k > 0 && results[k].Year != currYear && divide == "true") {
                            addBasicRowsToTable(meetingTable, ["", "", "", "", "", "", ""], 'c10');
                            currYear = results[k].Year;
                        }

                        var directory = isYearInSeasonsData(results[k].Year);
                        var site = "";
                        var boldCellIndex = 0;
                        var week = Number(results[k].Week);

                        if (results[k].Result != "Win") {
                            boldCellIndex = 4;
                        }

                        var teamB = results[k].OppId;
                        var teamA = results[k].TeamId;

                        site = results[k].Location;

                        var cells = [
                            createTeamHrefForRecentMeetings(directory, teamA, results[k].TeamName, null, null),
                            createTeamHrefForRecentMeetings(directory, teamA, null, null, null, 35),
                            "<a href=" + directory + "/boxscore.html?id=" + results[k].Week + "-" + results[k].Game + ">" + results[k].Score + "</a>",
                            createTeamHrefForRecentMeetings(directory, teamB, null, null, null, 35),
                            createTeamHrefForRecentMeetings(directory, teamB, results[k].Opponent, null, null),
                            "<a href=BowlHistory.html?id=" + results[k].BowlId + "&yr=" + results[k].Year + ">" + site + "</a>",
                            "<a href=" + directory + "/index.html>" + results[k].Year + "</a>"
                        ];

                        cells[boldCellIndex] = "<b><font size=2>" + cells[boldCellIndex] + "</font></b>";

                        var header = "";

                        table = document.getElementById('breakdownTable');
                        addBasicRowsToTable(meetingTable, cells, 'c3');
                    }
                });

        },
        "./Seasons");

}

function loadPostSeasonFromServer(yr, teamId, playoffsOnly, ccgOnly) {
    var uri = "ncaa.svc/bowls?yr=" + yr + "&id=" + teamId;

    if (playoffsOnly == "true") {
        uri += "&playoffs=true";
    }
    else if (ccgOnly == "true") {
        uri += "&ccg=true";
    }

    $.ajax({
        url: uri,
        success: function (data) {
            for (var x = 0 ; x < data.Rows.length; x++) {
                addBasicRowsToTable(document.getElementById('meetingTable'), data.Rows[x].Cells, 'c3');
            }

            var seriesResultsRow = meetingTable.insertRow(0);
            seriesResultsRow.innerHTML = "<td class=c2 colspan=8><br>" + data.Description + "</b><br><br>";
        }
    });
}

function loadPostSeason() {
    htmlDir = "./HTML";
    var year = Number(getQueryVariable("yr"));
    var teamId = getQueryVariable("id");
    var playoffsOnly = getQueryVariable("playoffs");
    var ccgOnly = getQueryVariable("ccg");
    var logo = document.getElementById("currentSchool");
    logo.src = htmlDir + "/Logos/256/team" + teamId + ".png";

    // start
    var results = [];
    var calls = [];
    var logo = document.getElementById("h2hLink");
    logo.href += "?yr=" + year + "&id=" + teamId;

    var playoff = document.getElementById("playoffLink");
    playoff.href += "&yr=" + year + "&id=" + teamId;

    var ccg = document.getElementById("ccgLink");
    ccg.href += "&yr=" + year + "&id=" + teamId;

    var bl = document.getElementById("bowlLink");
    bl.href += "yr=" + year + "&id=" + teamId;


    var wins = 0;
    var losses = 0;

    if (usingGZip) {
        loadPostSeasonFromServer(year, teamId, playoffsOnly, ccgOnly);
        return;
    }

    loadSeasonsJsonData(
        function () {
            for (i = 0 ; i < seasons.Season.length && seasons.Season[i].Year <= year; i++) {

                var scheduleUri = seasons.Season[i].Directory.replace('/Archive', '') + "/tsch.csv";
                calls[i] = $.ajax({
                    url: scheduleUri,
                    beforeSend: function (xhr) {
                        xhr.Year = scheduleUri.substring(2, 6);
                    },
                    success: function (data, status, jqXHR) {
                        var scheduleForYear = csvJSON(data);
                        var foundTeam = false;

                        for (j = 0 ; j < scheduleForYear.length ; j++) {
                            if (scheduleForYear[j].TeamId == teamId) {
                                foundTeam = true;
                            }
                            else if (foundTeam == true) {
                                break;
                            }

                            var gameWeek = Number(scheduleForYear[j].Week);

                            if (foundTeam && ((gameWeek == 16 && ccgOnly) || (ccgOnly == false && gameWeek > 16)) &&
                                shouldAddGame(playoffsOnly, scheduleForYear[j], jqXHR.Year)) {
                                scheduleForYear[j].Year = Number(jqXHR.Year);
                                results.push(scheduleForYear[j]);
                            }
                        }
                    }
                });
            }

            var getLatestTeam = isYearInSeasonsData(year) + "/team";

            calls[calls.length] = $.ajax({
                url: getLatestTeam,
                success: function (data) {
                    myTeam = findTeam(data, teamId);
                }
            });

            $.when.apply(null, calls).then(
                function () {

                    results.sort(function (a, b) {
                        if (b.Year == a.Year) {
                            return b.Week - a.Week;
                        }

                        return b.Year - a.Year;
                    });

                    for (k = 0 ; k < results.length ; k++) {
                        var directory = isYearInSeasonsData(results[k].Year);
                        var site = "";
                        var boldCellIndex = 0;
                        var week = Number(results[k].Week);

                        if (results[k].Result != "Win") {
                            boldCellIndex = 4;

                            losses++;
                        }
                        else {
                            wins++;
                        }

                        var oppId = results[k].OppId;

                        site = results[k].Location;

                        var cells = [
                            createTeamHrefForRecentMeetings(directory, teamId, results[k].TeamName, myTeam.Win, myTeam.Loss),
                            createTeamHrefForRecentMeetings(directory, teamId, null, null, null, 35),
                            "<a href=" + directory + "/boxscore.html?id=" + results[k].Week + "-" + results[k].Game + ">" + results[k].Score + "</a>",
                            createTeamHrefForRecentMeetings(directory, oppId, null, null, null, 35),
                            createTeamHrefForRecentMeetings(directory, oppId, results[k].Opponent, null, null),
                            "<a href='BowlHistory.html?yr=" + results[k].Year + "&id=" + results[k].BowlId + "'/>" + site + "</a>",
                            "<a href=" + directory + "/index.html>" + results[k].Year + "</a>"
                        ];

                        cells[boldCellIndex] = "<b>" + cells[boldCellIndex] + "</b>";

                        var header = "";

                        table = document.getElementById('breakdownTable');
                        addBasicRowsToTable(document.getElementById('meetingTable'), cells, 'c3');
                    }

                    var header = "Bowl Record ";

                    if (ccgOnly) {
                        header = "Conference Championship Game Record "
                    }
                    else if (playoffsOnly) {
                        header = "Playoff Record ";
                    }

                    header = header + wins + "-" + losses;

                    var seriesResultsRow = meetingTable.insertRow(0);

                    seriesResultsRow.innerHTML = "<td class=c2 colspan=8><br>" + header + "</b><br><br>";

                });

        },
        "./Seasons");

}

function createTeamHrefForRecentMeetings(directory, teamId, name, win, loss, logoSize, setRecord) {
    var opening = "<a href=" + directory + "/team.html?id=" + teamId + ">";
    var middle = "";

    if (logoSize == undefined || logoSize == null) {
        middle = name;//+" ("+win+"-"+loss+")";

        if (setRecord == true) {
            middle = middle + " (" + win + "-" + loss + ")";
        }
    }
    else {
        middle = '<img border="0" src="' + createTeamLogoSrc(teamId, logoSize) + '" />';
    }

    return opening + middle + "</a>";
}

function loadRecruitRanks() {
    $.ajax({
        url: "RecruitRankings",
        success: function (data) {
            var recruits = evalJson(data);
            var table = document.getElementById("recruitRankTable");
            for (var i = 0 ; i < recruits.length ; i++) {
                var team = recruits[i];

                var cells = [i + 1,
                    "<a href=team.html?id=" + team.TeamId + ">" + team.Team + "</a>",
                    team.Wins + "-" + team.Losses,
                    team.Points,
                    team.Star5,
                    team.Star4,
                    team.Star3,
                    team.Star2,
                    team.Star1];

                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadCoachChangeData() {
    $.ajax({
        url: "coachingChanges.csv",
        success: function (data) {
            var coaches = csvJSON(data);
            var table = document.getElementById("coachTable");

            // add header first
            var row = table.insertRow(-1);
            var cells = ["Team", "Position", 'New Coach', "Former Job", "Old Coach"];
            var width = ['15%', '15%', '25%', '20%', '25%'];
            addBasicRowsToTable(table, cells, 'C10', width);

            for (var i = 0 ; i < coaches.length ; i++) {
                var coach = coaches[i];
                var coachLink = createCoachLink("..", getCurrentYear(), coach.Name, coach.CoachId, "");
                var oldCoachLink = createCoachLink("..", getCurrentYear(), coach.OldCoachName, coach.OldCoachCoachId, "");

                var cells = [createTeamLinkTableCell(coach.TeamId, coach.Team), coach.PositionName, coachLink, "", oldCoachLink]

                if (coach.FormerTeamId != "") {
                    cells[3] = createTeamLinkTableCell(coach.FormerTeamId, coach.FormerTeam + " - " + coach.FormerPosition);
                }

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadCoachHistory() {
    var uri = "ncaa.svc/coachingGreats";

    $.ajax({
        url: uri,
        success: function (data) {
            var coaches = data;

            var table = document.getElementById("topCoachesTable");

            // add header first
            var row = table.insertRow(-1);
            var cells = ['Name', "Career Record", "Team Record", "Bowl Record", "Conference Championships", "National Championships", "Alma Mater"];
            var width = ['25%', '10%', '10%', '10%', '5%', '5%', '15%'];
            addBasicRowsToTable(table, cells, 'C10', width);

            for (var i = 0 ; i < coaches.length ; i++) {
                var coach = coaches[i].Coach;
                var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
                var cells = [
                    createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
                    createTeamHistoryLink(coaches[i].TeamId),
                    coachLink,
                    coach.CareerRecord,
                    coach.TeamRecord,
                    coach.CareerConferenceChampionships == undefined ? 0 : coach.CareerConferenceChampionships,
                    coach.CareerNationalChampionships == undefined ? 0 : coach.CareerNationalChampionships,
                    coach.AlmaMaterName,
                    coach.Age
                ];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadTopCoachData(year) {
    currentYear = year;
    $.ajax({
        url: "ps-topcoaches",
        success: function (data) {
            var coaches = evalJson(data).HeadCoaches;

            var table = document.getElementById("topCoachesTable");

            // add header first
            var row = table.insertRow(-1);
            var cells = ["Team", '', 'Name', "Career Record", "Team Record", "Conference Championships", "National Championships", "Alma Mater", "Age"];
            var width = ['15%', '10%', '20%', '10%', '10%', '5%', '5%', '15%', '10%'];
            addBasicRowsToTable(table, cells, 'C10', width);

            for (var i = 0 ; i < coaches.length ; i++) {
                var coach = coaches[i].Coach;
                var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
                var cells = [
                    createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
                    createTeamHistoryLink(coaches[i].TeamId),
                    coachLink,
                    coach.CareerRecord,
                    coach.TeamRecord,
                    coach.CareerConferenceChampionships == undefined ? 0 : coach.CareerConferenceChampionships,
                    coach.CareerNationalChampionships == undefined ? 0 : coach.CareerNationalChampionships,
                    coach.AlmaMaterName,
                    coach.Age
                ];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadCoachCarouselData(year) {
    currentYear = year;
    $.ajax({
        url: "ps-coachpage",
        success: function (data) {
            var coaches = evalJson(data);
            loadLongTimeCoaches(coaches.LongTermCoaches, "longTimeCoachTable");
            loadNewHeadCoaches(coaches.NewCoaches);
            loadHotSeatCoaches(coaches.HotSeatCoaches);
            loadTopCoordinators(coaches.TopCoordinators);
            loadLongTimeCoaches(coaches.TopMidMajorHC, "topMidMajorHCTable");
        }
    });
}

function loadTopCoordinators(coaches) {
    var table = document.getElementById("topOCDCTable");

    var row = table.insertRow(-1);
    var cells = ["Rank", "Team", '', 'Name', "Age", "Career Record", "Job"];
    var width = ['5%', '18%', '10%', '28%', '10%', '8%', "22%"];
    addBasicRowsToTable(table, cells, 'C10', width);

    for (var i = 0 ; i < coaches.length ; i++) {
        var coach = coaches[i].Coach;
        var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
        var cells = [
            i + 1,
            createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
            createTeamHistoryLink(coaches[i].TeamId),
            coachLink, coach.Age, coach.CareerRecord, coach.Position == 1 ? "Offensive Coordinator" : "Defensive Coordinator"];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadHotSeatCoaches(coaches) {
    var table = document.getElementById("topHotSeatTable");

    var row = table.insertRow(-1);
    var row = table.insertRow(-1);
    var cells = ["Team", '', 'Name', "Years", "Team Record", "Conference Championships", "National Championships"];
    var width = ['15%', '10%', '20%', '10%', '15%', '10%', '10%'];
    addBasicRowsToTable(table, cells, 'C10', width);

    for (var i = 0 ; i < coaches.length ; i++) {
        var coach = coaches[i].Coach;
        var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
        var cells = [
            createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
            createTeamHistoryLink(coaches[i].TeamId),
            coachLink, coach.YearsWithTeam - 1, coach.TeamRecord, coach.ConferenceChampionships, coach.NationalChampionships];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadNewHeadCoaches(coaches) {
    var table = document.getElementById("newCoachTable");

    var row = table.insertRow(-1);
    var cells = ["Rank", "Team", '', 'Name', "Age", "Career Record", "Previous Job"];
    var width = ['5%', '10%', '10%', '20%', '5%', '10%', '40%'];
    addBasicRowsToTable(table, cells, 'C10', width);

    for (var i = 0 ; i < coaches.length ; i++) {
        var coach = coaches[i].Coach;
        var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
        var cells = [
            i + 1,
            createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
            createTeamHistoryLink(coaches[i].TeamId),
            coachLink, coach.Age, coach.CareerRecord, coaches[i].OldJob.replace("Off", "Offensive").replace("Def", "Defensive").replace("Coord", "Coordinator")];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadLongTimeCoaches(coaches, table) {
    // long term coaches
    var table = document.getElementById(table);

    // add header first
    var row = table.insertRow(-1);
    var cells = ["Team", '', 'Name', "Age", "Years", "Team Record", "Conference Championships", "National Championships"];
    var width = ['15%', '15%', '20%', '5%', '5%', '10%', '10%', '10%'];
    addBasicRowsToTable(table, cells, 'C10', width);

    for (var i = 0 ; i < coaches.length ; i++) {
        var coach = coaches[i].Coach;
        var coachLink = createCoachLink("..", currentYear - 1, coach.Name, coach.Id, "");
        var cells = [
            createTeamLinkTableCell(coaches[i].TeamId, coaches[i].TeamName, true),
            createTeamHistoryLink(coaches[i].TeamId),
            coachLink, coach.Age, coach.YearsWithTeam - 1, coach.TeamRecord, coach.ConferenceChampionships, coach.NationalChampionships];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function createTeamHistoryLink(teamId) {
    return "<a href='../TeamHistory.html?yr=" + (currentYear - 1) + "&id=" + teamId + "'><img border='0' src='" + createTeamLogoSrc(teamId, 35) + "' /></a>";
}


function loadCoachData() {
    $.ajax({
        url: "coaches.csv",
        success: function (data) {
            var coaches = csvJSON(data);
            var table = document.getElementById("coachTable");

            for (var i = 0 ; i < coaches.length ; i++) {
                var coach = coaches[i];
                var coachLink = createCoachLink("..", getCurrentYear(), coach.Name, coach.CoachId, "");
                var cells = [coachLink, createTeamLinkTableCell(coach.TeamId, coach.Team), coach.PositionName, coach.Age, coach.YWT, coach.CoachRating, coach.Level, coach.CareerRecord, coach.TeamRecord];
                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function createCoachLink(location, lookbackYear, coachName, coachId, title) {
    return "<a href='" + location + "/CoachCareer.html?yr=" + lookbackYear + "&name=" + escape(coachName) + "&id=" + coachId + "'>" + title + coachName + "</a>";;
}

function loadTeamRatingsData() {
    $.ajax({
        url: "teamratings.csv",
        success: function (data) {
            var teams = csvJSON(data);
            currentData = teams;
            writeTeamRatingsTable(teams);
        }
    });
}

function writeTeamRatingsTable(teams) {
    var table = document.getElementById("ratingsTable");

    for (var i = 0 ; i < teams.length ; i++) {
        var team = teams[i];
        var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Name), team.OVR, team.OFF, team.QB, team.RB, team.WR, team.OL, team.DEF, team.DL, team.LB, team.DB, team.ST];
        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByTeamOvr() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.OVR) - Number(a.OVR); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamOff() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.OFF) - Number(a.OFF); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamQb() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.QB) - Number(a.QB); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamRb() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.RB) - Number(a.RB); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamRec() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.WR) - Number(a.WR); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamOl() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.OL) - Number(a.OL); });
    writeTeamRatingsTable(currentData);
}
function sortByTeamDef() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.DEF) - Number(a.DEF); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamDl() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.DL) - Number(a.DL); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamLb() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.LB) - Number(a.LB); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamDb() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.DB) - Number(a.DB); });
    writeTeamRatingsTable(currentData);
}

function sortByTeamST() {
    cleanTable("ratingsTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.ST) - Number(a.ST); });
    writeTeamRatingsTable(currentData);
}

function analyzePlayer(player, roster) {
    var playerPosition = getGeneralPosition(player.Position);
    var position = roster[playerPosition];
    var playerOverall = Number(player.Ovr);
    var playerYear = getYearIndex(player.Year);
    var starterThreshold = getStarterThreshold(playerPosition);
    var ratingIdx = Math.floor(playerOverall / 10) - 4;
    if (ratingIdx < 0)
        ratingIdx = 0;

    roster.Ratings[ratingIdx]++;

    if (position == undefined) {
        position = { Count: 0, Points: 0 };
        roster[playerPosition] = position;
        position.PlayerCounts = [0, 0, 0, 0];
        position.Best = 0;
    }

    if (position.Count < starterThreshold) {
        if (isOffPlayer(playerPosition)) {
            roster.OffStarterCount++;
            roster.OffStarterPts += playerOverall;
        }
        else if (isSTPlayer(playerPosition)) {
            roster.STStarterCount++;
            roster.STStarterPts += playerOverall;
        }
        else {
            roster.DefStarterCount++;
            roster.DefStarterPts += playerOverall;
        }
    }

    if (isOffPlayer(playerPosition)) {
        roster.OffCount++;
        roster.OffPts += playerOverall;
        roster.OffClassCount[playerYear]++;
        if (playerOverall > roster.OffBest) roster.OffBest = playerOverall;
    }
    else if (isSTPlayer(playerPosition)) {
        roster.STCount++;
        roster.STPts += playerOverall;
        roster.STClassCount[playerYear]++;
        if (playerOverall > roster.STBest) roster.STBest = playerOverall;
    }
    else {
        roster.DefCount++;
        roster.DefPts += playerOverall;
        roster.DefClassCount[playerYear]++;
        if (playerOverall > roster.DefBest) roster.DefBest = playerOverall;
    }

    if (playerOverall > position.Best)
        position.Best = playerOverall;

    position.Points += playerOverall;
    position.Count++;
    position.PlayerCounts[playerYear]++;
}

function loadTeamRosterData(isPreseason) {
    teamId = getQueryVariable("id");
    loadTeamPage(isPreseason);

    $.ajax({
        url: "teamroster" + teamId + ".csv",
        success: function (data) {
            var records = csvJSON(data);
            var table = document.getElementById("rosterTable");

            var currentPosition = "";
            roster = {
                OffBest: 0, DefBest: 0, STBest: 0,
                OffClassCount: [0, 0, 0, 0],
                DefClassCount: [0, 0, 0, 0],
                STClassCount: [0, 0, 0, 0],
                OffPts: 0, OffCount: 0, DefPts: 0, DefCount: 0, STPts: 0, STCount: 0, OffStarterPts: 0, OffStarterCount: 0, DefStarterPts: 0, DefStarterCount: 0, STStarterPts: 0, STStarterCount: 0,
                Ratings: [0, 0, 0, 0, 0, 0]  //40s,50s,60s,70s,80s,90s
            };
            for (var i = 0 ; i < records.length ; i++) {
                var p = records[i];

                // write a new header
                if (currentPosition != p.Position) {
                    writeRosterHeader(table);
                    currentPosition = p.Position;
                }

                var cells = [p.No, p.Name, p.Year, p.Position, p.Height, p.Weight, "<b>" + p.Ovr + "</b>", p.Spd, p.Acc, p.Agl, p.Str, p.Awr, p.City + ", " + p.State];
                addBasicRowsToTable(table, cells, "c3");
            }

            records.sort(function (a, b) { return Number(b.Ovr) - Number(a.Ovr); });
            for (var i = 0 ; i < records.length ; i++) {
                analyzePlayer(records[i], roster);
            }

            table = document.getElementById('breakdownTable');
            var pos = [null, "QB", "HB", "FB", "WR", "TE", "OT", "OG", "C", null, "DE", "DT", "OLB", "MLB", "CB", "FS", "SS", null];
            for (var i = 0 ; i < pos.length ; i++) {
                if (pos[i] == null) {
                    insertRosterBreakdownHeader(table);
                }
                else {
                    var position = roster[pos[i]];
                    var cells = [pos[i], position.Points, position.Count, position.Best, position.PlayerCounts[0], position.PlayerCounts[1], position.PlayerCounts[2], position.PlayerCounts[3], (position.Points / position.Count).toFixed(1)];
                    addBasicRowsToTable(table, cells, 'c3');
                }
            }

            addBasicRowsToTable(table,
                ["A.OFF", roster.OffPts, roster.OffCount, roster.OffBest, roster.OffClassCount[0], roster.OffClassCount[1], roster.OffClassCount[2], roster.OffClassCount[3], (roster.OffPts / roster.OffCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["A.DEF", roster.DefPts, roster.DefCount, roster.DefBest, roster.DefClassCount[0], roster.DefClassCount[1], roster.DefClassCount[2], roster.DefClassCount[3], (roster.DefPts / roster.DefCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["A.ST", roster.STPts, roster.STCount, roster.STBest, roster.STClassCount[0], roster.STClassCount[1], roster.STClassCount[2], roster.STClassCount[3], (roster.STPts / roster.STCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["TEAM", roster.DefPts + roster.OffPts + roster.STPts, roster.OffCount + roster.DefCount + roster.STCount,
                    Math.max(roster.STBest, Math.max(roster.DefBest, roster.OffBest)),
                    roster.OffClassCount[0] + roster.STClassCount[0] + roster.DefClassCount[0],
                    roster.OffClassCount[1] + roster.STClassCount[1] + roster.DefClassCount[1],
                    roster.OffClassCount[2] + roster.STClassCount[2] + roster.DefClassCount[2],
                    roster.OffClassCount[3] + roster.STClassCount[3] + roster.DefClassCount[3],
                    ((roster.DefPts + roster.OffPts + roster.STPts) / (roster.OffCount + roster.DefCount + roster.STCount)).toFixed(1)],
                'c3');

            insertRosterBreakdownHeader(table, "STARTERS");
            addBasicRowsToTable(table,
                ["<b>OFF</b>", roster.OffStarterPts, roster.OffStarterCount, '', '', '', '', '', (roster.OffStarterPts / roster.OffStarterCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["<b>DEF</b>", roster.DefStarterPts, roster.DefStarterCount, '', '', '', '', '', (roster.DefStarterPts / roster.DefStarterCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["<b>ST</b>", roster.STStarterPts, roster.STStarterCount, '', '', '', '', '', (roster.STStarterPts / roster.STStarterCount).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ["<b>TEAM</b>", roster.STStarterPts + roster.OffStarterPts + roster.DefStarterPts,
                    roster.DefStarterCount + roster.OffStarterCount + roster.STStarterCount,
                    '', '', '', '', '',
                    ((roster.STStarterPts + roster.OffStarterPts + roster.DefStarterPts) / (roster.DefStarterCount + roster.OffStarterCount + roster.STStarterCount)).toFixed(1)],
                'c3');

            addBasicRowsToTable(table,
                ['', '', '40s', '50s', '60s', '70s', '80s', '90s', ''],
                'C10',
                ['10%', '10%', '10%', '10%', '10%', '10%', '10%', '10%', '10%']);

            addBasicRowsToTable(table,
                ['Players by<br>OVR', '', roster.Ratings[0], roster.Ratings[1], roster.Ratings[2], roster.Ratings[3], roster.Ratings[4], roster.Ratings[5], ''],
                'c3');
        }
    });
}

function insertRosterBreakdownHeader(table, firstHeader) {
    var row = table.insertRow(-1);
    var cells = ["POS", "PTS", 'COUNT', "BEST", "FR", "SO", "JR", "SR", "AVG"];
    if (firstHeader != undefined)
        cells[0] = firstHeader;
    var width = ['10%', '10%', '10%', '10%', '10%', '10%', '10%', '10%', '10%'];
    addBasicRowsToTable(table, cells, 'C10', width);
}

function getGameWeek(GameKey) {
    var week = GameKey;
    week = week.substring(0, week.indexOf('-'));
    return week;
}

function loadTeamTopPerfData() {
    teamId = getQueryVariable("id");
    loadTeamPage();

    document.getElementById("topPerfLink").href = "teamtopperf.html?id=" + teamId;
    document.getElementById("careerStatsLink").href = "teampstat.html?career=true&id=" + teamId;
    document.getElementById("playerStatsLink2").href = "teampstat.html?id=" + teamId;

    $.ajax({
        url: "gtppass.csv",
        success: function (data) {
            var cc = csvJSON(data, "TeamId", teamId);

            var table = document.getElementById("passTable");

            for (var i = 0 ; i < cc.length ; i++) {
                var p = cc[i];

                var cells = [p.Team, p.No, "<b>" + p.Name + "</b>", p.PlayerClass, p.Position, p.Height, p.Weight, p.Comp, p.Att, "<b>" + p.Yards + "</b>", p.TD, p.Int, "<a href='boxscore.html?id=" + p.GameKey + "'>Week " + getGameWeek(p.GameKey) + "</a>"];
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });

    $.ajax({
        url: "gtprush.csv",
        success: function (data) {
            var cc = csvJSON(data, "TeamId", teamId);

            var table = document.getElementById("rushTable");

            for (var i = 0 ; i < cc.length ; i++) {
                var p = cc[i];

                var cells = [p.Team, p.No, "<b>" + p.Name + "</b>", p.PlayerClass, p.Position, p.Height, p.Weight, p.Att, "<b>" + p.Yards + "</b>", p.TD, "<a href='boxscore.html?id=" + p.GameKey + "'>Week " + getGameWeek(p.GameKey) + "</a>"];
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });

    $.ajax({
        url: "gtprec.csv",
        success: function (data) {
            var cc = csvJSON(data, "TeamId", teamId);

            var table = document.getElementById("recTable");

            for (var i = 0 ; i < cc.length ; i++) {
                var p = cc[i];

                var cells = [p.Team, p.No, "<b>" + p.Name + "</b>", p.PlayerClass, p.Position, p.Height, p.Weight, p.Rec, "<b>" + p.Yards + "</b>", p.TD, "<a href='boxscore.html?id=" + p.GameKey + "'>Week " + getGameWeek(p.GameKey) + "</a>"];
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadBoxScoreData() {
    boxId = getQueryVariable("id").split("-");

    $.ajax({
        url: "box" + boxId[0] + "",
        // async: false,
        success: function (data) {
            var boxData = evalJson(data).Games[boxId[1]];
            var statData = csvJSON(boxData.StatsTable, null, false, true);
            loadBoxScoreLogos(boxData);
            loadScoreSummary(boxData);
            loadTeamBoxScore(boxData);
            loadBoxScoreStatTables(boxData, statData);
        }
    });
}

function loadTeamBoxScore(box) {
    var table = document.getElementById("teamStatsTable");
    var home = box.HomeTeamBoxScore;
    var away = box.AwayTeamBoxScore;
    var cells = [
        [home.FirstDowns, "1st Downs", away.FirstDowns],
        [home.ThirdDownConversions + " - " + home.ThirdDownAttempts, "3rd Down efficiency", away.ThirdDownConversions + " - " + away.ThirdDownAttempts],
        [home.FourthDownConversions + " - " + home.FourthDownAttempts, "4th Down efficiency", away.FourthDownConversions + " - " + away.FourthDownAttempts],
        [home.OffensiveYards, "Offensive Yards", away.OffensiveYards, null],
        [home.TotalYards, "TotalYards", away.TotalYards],
        [home.PassYards, "Passing Yards", away.PassYards, null],
        [home.PassCompletions + " - " + home.PassAttempts, "Comp-Att", away.PassCompletions + " - " + away.PassAttempts],
        [Math.round(100 * home.PassYards / home.PassAttempts) / 100, "Yards per Pass", Math.round(100 * away.PassYards / away.PassAttempts) / 100],
        [home.PassTD, "Passing Touchdowns", away.PassTD],
        [home.RushYards, "Rushing Yards", away.RushYards, null],
        [home.RushAttempts, "Carries", away.RushAttempts],
        [Math.round(100 * home.RushYards / home.RushAttempts) / 100, "Yards per Carry", Math.round(100 * away.RushYards / away.RushAttempts) / 100],
        [home.RushTD, "Rushing Touchdowns", away.RushTD],
        [home.Penalties + " - " + home.PenaltyYards, "Penalties", away.Penalties + " - " + away.PenaltyYards, null],
        [home.Turnovers, "Turnovers", away.Turnovers],
        [home.FumblesLost, "Fumbles Lost", away.FumblesLost],
        [home.IntThrown, "Interceptions Thrown", away.IntThrown],
        [formatSeconds(home.TimeOfPossesion).substring(3), "Time of Possessions", formatSeconds(away.TimeOfPossesion).substring(3), null]
    ];

    for (var i = 0 ; i < cells.length ; i++) {
        addTeamStatRow(table, cells[i]);
    }
}

function addTeamStatRow(table, cells) {
    var row = table.insertRow(-1);
    var cell = row.insertCell(-1);
    cell.className = cells.length == 4 ? "c11" : "c3";
    cell.width = "40%";
    cell.innerHTML = cells[0];

    cell = row.insertCell(-1);
    cell.className = cells.length == 4 ? "c7" : "c11";
    cell.width = "20%";
    cell.innerHTML = cells[1];

    cell = row.insertCell(-1);
    cell.className = cells.length == 4 ? "c11" : "c3";
    cell.width = "40%";
    cell.innerHTML = cells[2];
}

function formatSeconds(seconds) {
    var date = new Date(1970, 0, 1);
    date.setSeconds(seconds);
    return date.toTimeString().replace(/.*(\d{2}:\d{2}:\d{2}).*/, "$1");
}

function loadScoreSummary(box) {
    var table = document.getElementById("scoringTable");

    var currentQtr = -1;
    var home = 0;
    var away = 0;
    for (var i = 0 ; i < box.Scores.length ; i++) {
        var score = box.Scores[i];

        if (score.Quarter != currentQtr) {
            var row = table.insertRow(-1);
            var cell = row.insertCell(-1);
            var cells = ["Play", "Home", "Score", "Away", "Quarter", "Time"];
            var widths = ["35%", "10%", "10%", "10%", "17%", "18%"];
            addBasicRowsToTable(table, cells, "c7", widths);
            currentQtr = score.Quarter;
        }

        var qtr = 0;
        var time = "";

        if (score.Quarter <= 4) {
            qtr = score.Quarter;
            time = formatSeconds(score.Time).substring(3);
        }
        else {
            qtr = "OT";
            time = 1 + Math.floor((score.Time - 12000) / 2);
        }

        var qtr = score.Quarter <= 4 ? score.Quarter : "OT";

        var cells = [score.Description, null, null, null, qtr, time];

        if (score.TeamId == box.HomeTeamId) {
            home += score.Points;
            cells[1] = createTeamLogoLink(box.HomeTeamId, 35);
            cells[3] = "";
        }
        else {
            away += score.Points;
            cells[3] = createTeamLogoLink(box.AwayTeamId, 35);
            cells[1] = "";
        }

        cells[2] = "<b>" + home + "-" + away + "</b>";

        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadBoxScoreLogos(box) {
    document.title = box.HomeTeamName + " " + box.HomeTeamMascot + " vs " + box.AwayTeamName + " " + box.AwayTeamMascot;
    populateLogoTable(box.AwayTeamId, box.HomeTeamId);
}

function populateLogoTable(awayTeamId, homeTeamId) {
    var table = document.getElementById("logoTable");
    var row = table.insertRow(-1);
    var cell = row.insertCell(-1);
    cell.className = 'c8';
    cell.width = '50%';
    cell.innerHTML = '<center><a href="team.html?id=' + homeTeamId + '"><img border="0" src="' + createTeamLogoSrc(homeTeamId, 256) + '" /></a></center>';

    cell = row.insertCell(-1);
    cell.className = 'c8';
    cell.width = '50%';
    cell.innerHTML = '<center><a href="team.html?id=' + awayTeamId + '"><img border="0" src="' + createTeamLogoSrc(awayTeamId, 256) + '" /></a></center>';
}

function loadBoxScoreStatTables(box, stats) {
    var tables = [
        [document.getElementById("homePassingTable"), "Passing"],
        [document.getElementById("homeRushingTable"), "Rushing"],
        [document.getElementById("homeReceivingTable"), "Receiving"],
        [document.getElementById("homeDefTable"), "Defensive"],
        [document.getElementById("awayPassingTable"), "Passing"],
        [document.getElementById("awayRushingTable"), "Rushing"],
        [document.getElementById("awayReceivingTable"), "Receiving"],
        [document.getElementById("awayDefTable"), "Defensive"],
    ];

    // add the header to each table
    for (var i = 0 ; i < tables.length ; i++) {
        var team = 0;
        var name = 0;
        // home table
        if (i < tables.length / 2) {
            team = box.HomeTeamId;
            name = box.HomeTeamName;
        }
        else { //away team
            team = box.AwayTeamId;
            name = box.AwayTeamName;
        }

        var table = tables[i][0];
        var row = table.insertRow(0);
        var cell = row.insertCell(-1);
        cell.className = "c2";
        cell.colSpan = "14";
        cell.innerHTML = '<b><img border="0" align="left" src="' + createTeamLogoSrc(team, 35) + '" />' + name + ' ' + tables[i][1] + ' Stats' + '<img border="0" align="right" src="' + createTeamLogoSrc(team, 35) + '" /></b>';
    }

    // now write the stats to each table
    for (var i = 0 ; i < stats.length ; i++) {
        var table = null;

        // No, Name, PlayerClass, Position, Height, Weight, Stat1, Stat2, Stat3, Stat4, Stat5, Stat6, Stat7, TeamId, TableId
        // create our data for our cells
        var cells = [stats[i].No, "<b>" + stats[i].Name + "</b>", stats[i].PlayerClass, stats[i].Position, stats[i].Height, stats[i].Weight, stats[i].Stat1, stats[i].Stat2, stats[i].Stat3];

        if (stats[i].TableId == 3 || stats[i].TableId == 7) {
            // sacks are multiplied by 10
            cells[cells.length - 1] = Number(stats[i].Stat3) / 10;
        }

        if (stats[i].Stat4 != "") cells[cells.length] = stats[i].Stat4;
        if (stats[i].Stat5 != "") cells[cells.length] = stats[i].Stat5;
        if (stats[i].Stat6 != "") cells[cells.length] = stats[i].Stat6;
        if (stats[i].Stat7 != "") cells[cells.length] = stats[i].Stat7;

        // get the table to insert into
        var table = tables[Number(stats[i].TableId)][0];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadTeamPage(isPreseason) {
    teamId = getQueryVariable("id");
    var teamFile = isPreseason != undefined && isPreseason != null && isPreseason == true ? "ps-team" : "team";

    $.ajax({
        url: teamFile,
        // async: false,
        success: function (data) {
            var team = findTeam(data, teamId);
            loadTeamPageTemplate(team);
            var header = document.getElementById("schoolNameHeader");
            if (header != null && header != undefined) {
                header.innerHTML = "<b>" + team.Name + " " + team.Mascot + "</b>";
            }
        }
    });
}

function leadNationalStatLeaderData(year, baseYear) {
    currentYear = year;
    startingYear = baseYear;

    $.ajax({
        url: "leaders.csv",
        success: function (data) {
            var stats = csvJSON(data, null, null, true);

            var tables =
                [
                    document.getElementById("passingTable"),  //0
                    document.getElementById("qbRushingTable"),  //1
                    document.getElementById("hbRushingTable"),  //2
                    document.getElementById("recTable"),  //3
                    document.getElementById("recYdsTable"),  //4
                    document.getElementById("olTable"),  //5 
                    document.getElementById("tklTable"),  //6
                    document.getElementById("sackTable"),  //7
                    document.getElementById("intTable"),  //8
                    document.getElementById("kickTable"),  //9
                    document.getElementById("puntTable"),  //10
                    document.getElementById("krTable"),  //11
                    document.getElementById("prTable"),  //12
                ];

            for (var i = 0 ; i < stats.length ; i++) {
                var table = null;

                var year = Number(currentYear) + Number(startingYear);

                // create our data for our cells
                var cells = [year, stats[i].No, "<b>" + stats[i].Name + "</b>", stats[i].PlayerClass, stats[i].Position, stats[i].Height, stats[i].Weight, stats[i].Stat1, stats[i].Stat2];
                if (stats[i].Stat3 != "" && (stats[i].TableIdx == 6 || stats[i].TableIdx == 7 || stats[i].TableIdx == 8)) {
                    // sacks are multiplied by 10
                    cells[cells.length] = Number(stats[i].Stat3) / 10;
                }

                else if (stats[i].Stat3 != "") {
                    cells[cells.length] = stats[i].Stat3;
                }
                if (stats[i].Stat4 != "") cells[cells.length] = stats[i].Stat4;
                if (stats[i].Stat5 != "") cells[cells.length] = stats[i].Stat5;
                if (stats[i].Stat6 != "") cells[cells.length] = stats[i].Stat6;
                cells[cells.length] = createTeamLinkTableCell(stats[i].TeamId, stats[i].Team);

                // get the table to insert into
                var table = tables[Number(stats[i].TableIdx)];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadTeamFroshData(isPreseason) {
    teamId = getQueryVariable("id");
    loadTeamPage(isPreseason);

    $.ajax({
        url: "teamfrosh.csv",
        success: function (data) {
            var records = csvJSON(data, "TeamId", teamId);
            var table = document.getElementById("froshTable");

            for (var i = 0 ; i < records.length ; i++) {
                var p = records[i];

                var cells = [p.No, p.Name, p.Year, p.Position, p.Height, p.Weight, "<b>" + p.Ovr + "</b>", p.Spd, p.Acc, p.Agl, p.Str, p.Awr, p.City + ", " + p.State];
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadTeamRecordData(isPreseason) {
    teamId = getQueryVariable("id");
    loadTeamPage(isPreseason);

    $.ajax({
        url: "teamrecords.csv",
        success: function (data) {
            var records = csvJSON(data, "TeamId", teamId);
            var table = document.getElementById("recordTable");

            for (var i = 0 ; i < records.length ; i++) {
                var record = records[i];

                var cells = [record.RecordType, record.Description, record.Holder, record.Value, record.Opp];
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadTeamDefData() {
    $.ajax({
        url: "defstats.csv",
        success: function (data) {
            var teams = csvJSON(data);
            currentData = teams;
            writeDefDataTable(teams, 3);
        }
    });
}

function writeDefDataTable(teams, boldIndex) {
    var table = document.getElementById("defTable");

    for (var i = 0 ; i < teams.length ; i++) {

        var team = teams[i];

        var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Team), team.Record,
            Number(team.DefYds) / 10, Number(team.PassYds) / 10, Number(team.RushYds) / 10, ];

        cells[boldIndex] = "<b>" + cells[boldIndex] + "</b>";

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByDefYds() {
    cleanTable("defTable", currentData.length);
    currentData.sort(function (a, b) { return Number(a.DefYds) - Number(b.DefYds); });
    writeDefDataTable(currentData, 3);
}

function sortByDefPassYds() {
    cleanTable("defTable", currentData.length);
    currentData.sort(function (a, b) { return Number(a.PassYds) - Number(b.PassYds); });
    writeDefDataTable(currentData, 4);
}

function sortByDefRushYds() {
    cleanTable("defTable", currentData.length);
    currentData.sort(function (a, b) { return Number(a.RushYds) - Number(b.RushYds); });
    writeDefDataTable(currentData, 5);
}

function loadTeamOffData() {
    $.ajax({
        url: "offstats.csv",
        success: function (data) {
            var teams = csvJSON(data);
            currentData = teams;
            writeOffDataTable(teams, 3);
        }
    });

    $.ajax({
        url: "offstats.csv",
        success: function (data) {
            var teams = csvJSON(data);

            var table = document.getElementById("ppgTable");
            for (var j = 0 ; j < teams.length ; j++) {
                teams[j].PPG = Number(teams[j].PassAtt) + Number(teams[j].RushAtt);
            }

            teams.sort(function (a, b) { return b.PPG - a.PPG; });

            for (var i = 0 ; i < teams.length ; i++) {
                var team = teams[i];
                var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Team), team.Record,
                    Number(team.OffYds) / 10, Number(team.PassAtt) / 10, Number(team.PassYds) / 10,
                    Number(team.RushAtt) / 10, Number(team.RushYds) / 10, Number(team.PPG) / 10];

                cells[cells.length - 1] = "<b>" + cells[cells.length - 1] + "</b>";

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function writeOffDataTable(teams, boldIndex) {

    var table = document.getElementById("offTable");

    for (var i = 0 ; i < teams.length ; i++) {

        var team = teams[i];

        var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Team), team.Record,
            Number(team.OffYds) / 10, Number(team.PassAtt) / 10, Number(team.PassYds) / 10, Number(team.PassTD) / 10,
            Number(team.RushAtt) / 10, Number(team.RushYds) / 10, Number(team.RushTD) / 10];

        cells[boldIndex] = "<b>" + cells[boldIndex] + "</b>";

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByTotalOffense() {
    cleanTable("offTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.OffYds) - Number(a.OffYds) });
    writeOffDataTable(currentData, 3);
}

function sortByRushYds() {
    cleanTable("offTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.RushYds) - Number(a.RushYds) });
    writeOffDataTable(currentData, 8);
}

function sortByPassYds() {
    cleanTable("offTable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.PassYds) - Number(a.PassYds) });
    writeOffDataTable(currentData, 5);
}

function loadAwardData() {
    $.ajax({
        url: "awards.csv",
        success: function (data) {
            var award = getQueryVariable("id");
            var d = csvJSON(data);
            var table = document.getElementById("awardTable");
            var awardName = null;
            var logo = document.getElementById("awardLogo");
            logo.src = htmlDir + "/Logos/awards/" + award + ".png";

            for (var i = 0 ; i < d.length ; i++) {

                if (d[i].AwardId == award) {
                    // we need to insert the first two rows
                    if (awardName == null) {
                        awardName = d[i].AwardName;
                        document.title = awardName;
                        var row = table.insertRow(-1);
                        var cell = row.insertCell(-1);
                        cell.className = "c2";
                        cell.colSpan = 13;
                        cell.innerHTML = "<b>" + awardName + "</b>";

                        row = table.insertRow(-1);
                        var cells = [["10%", "#"], ["15%", "Name"], ["10%", "Year"], ["10%", "Pos"], ["10%", "Hgt"], ["10%", "Lbs"], ["10%", "Ovr"], ["15%", "Team"]];
                        addCellsWithWidth(table, cells, "C10");
                    }

                    var cells = [d[i].Number, d[i].Name, d[i].Year, d[i].Position, d[i].Height, d[i].Weight, d[i].Ovr, createTeamStatLinkTableCell(d[i].TeamId, d[i].Team)];
                    addBasicRowsToTable(table, cells, "c3");
                }
            }
        }
    });
}

function loadBowlChampionData(baseYear) {
    startingYear = baseYear;

    $.ajax({
        url: "bowlchamps.csv",
        success: function (data) {
            var bowlId = getQueryVariable("id");
            var bc = csvJSON(data, "BowlId", bowlId);

            var table = document.getElementById("bowlChampTable");
            var currentBowl = -1;

            // we have a bowl so we should use a logo
            if (bowlId.length != 0) {
                var logo = document.getElementById("bowlChampLogo");
                logo.src = htmlDir + "/Logos/bowls/" + bowlId + ".jpg";


                var trophy = document.getElementById("bowlChampTrophy");
                trophy.src = htmlDir + "/Logos/bowl_trophies/" + bowlId + ".png";
            }

            for (var i = 0 ; i < bc.length ; i++) {
                var champ = bc[i];

                // add a divider for a new confeerence
                if (currentBowl != champ.BowlId) {
                    var row = table.insertRow(-1);
                    var cells = [["20%", "Year"], ["30%", "Bowl"], ["30%", "Team"], ["20%", "Team"]];
                    addCellsWithWidth(table, cells, "c7");
                    currentBowl = champ.BowlId;
                }

                var cells = [startingYear + Number(champ.Year), createBowlLinkTableCell(champ.BowlId, champ.Name), createTeamLogoLink(champ.TeamId, 35), createTeamLinkTableCell(champ.TeamId, champ.Team)];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function addCellsWithWidth(table, cells, className) {
    var row = table.insertRow(-1);
    for (var j = 0 ; j < cells.length ; j++) {
        var cell = row.insertCell(-1);
        cell.className = className;
        cell.width = cells[j][0];
        cell.innerHTML = cells[j][1];
    }
}

function loadAttendanceData() {
    $.ajax({
        url: "att.csv",
        success: function (data) {
            var teams = csvJSON(data);
            currentData = teams;
            writeAttendanceTable(teams, 4);
        }
    });
}

function writeAttendanceTable(teams, boldIndex) {

    var table = document.getElementById("attTable");

    for (var i = 0 ; i < teams.length ; i++) {

        var cells = [1 + i, createTeamLinkTableCell(teams[i].TeamId, teams[i].Team), teams[i].Stadium, teams[i].Record,
            addCommas(teams[i].AvgAtt), addCommas(teams[i].Capacity), Number(teams[i].PctCapacity) / 100 + "%"];

        cells[boldIndex] = "<b>" + cells[boldIndex] + "</b>";

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function cleanAttendanceTable() {
    cleanTable("attTable", currentData.length);
}

function cleanTable(tableName, len) {
    var table = document.getElementById(tableName);
    for (var i = 0 ; i < len ; i++) {
        table.deleteRow(-1);
    }
}

function sortByCapacity() {
    cleanAttendanceTable();
    currentData.sort(function (a, b) { return Number(b.PctCapacity) - Number(a.PctCapacity) });
    writeAttendanceTable(currentData, 6);
}

function sortByAttendance() {
    cleanAttendanceTable();
    currentData.sort(function (a, b) { return Number(b.AvgAtt) - Number(a.AvgAtt) });
    writeAttendanceTable(currentData, 4);
}

function populateDivisions(conferences) {
    for (var i = 0 ; i < conferences.length ; i++) {
        for (var j = 0 ; j < conferences[i].Divisions.length ; j++) {
            divisions[conferences[i].Divisions[j].Id] = conferences[i].Divisions[j];
        }
    }
}

function addStandingsTable(table, name) {
    // set the name of our table and our header
    var header = name;
    var linkName = name.replace(" ", "");
    currentTableName = linkName;

    header = header + " Standings";
    currentTableName = currentTableName + "Table";
    createStandingsTable(table, currentTableName, header, linkName, true);
    currentTable = document.getElementById(currentTableName);
    return currentTable;
}

function writeStandings(currentTable, teamStandings) {
    var rank = 1;

    for (var jj = 0 ; jj < teamStandings.length; jj++) {
        var team = teamStandings[jj];
        var cells = [rank, createTeamLogoLink(team.TeamId, 35), createTeamLinkTableCell(team.TeamId, team.Team, true), team.Win + "-" + team.Loss, team.ConferenceWin + "-" + team.ConferenceLoss];
        addBasicRowsToTable(currentTable, cells, "c3");
        rank++;
    }
}

function loadStandingsData() {
    var isPreseason = getQueryVariable("preseason") == "true";

    $.ajax({
        url: "conf",
        // async: false,
        success: function (data) {
            conferences = evalJson(data);
            populateDivisions(conferences);

            if (isPreseason) {
                $.ajax({
                    url: "PredictedStandings",
                    success: function (confJson) {
                        var teams = evalJson(confJson).dictionary;
                        teams.sort(function (a, b) { return a.Value.ConferenceName.localeCompare(b.Value.ConferenceName); });
                        var table = document.getElementById('mainTable');

                        for (var kk = 0 ; kk < teams.length ; kk++) {
                            var conf = teams[kk];

                            if (conf.Value.Teams != undefined) {
                                var currTable = addStandingsTable(table, conf.Value.ConferenceName);
                                writeStandings(currTable, conf.Value.Teams)
                            }
                            else {
                                currTable = addStandingsTable(table, conf.Value.DivisionA.DivisionName);
                                writeStandings(currTable, conf.Value.DivisionA.Teams)
                                currTable = addStandingsTable(table, conf.Value.DivisionB.DivisionName);
                                writeStandings(currTable, conf.Value.DivisionB.Teams)
                            }
                        }
                    }
                });
            }
            else {
                $.ajax({
                    url: "standings.csv",
                    success: function (data) {
                        var teams = csvJSON(data);
                        var table = document.getElementById('mainTable');

                        var currentConf = -1;
                        var currentDiv = -1;
                        var currentTableName = null;
                        var newConference = false;
                        var rank = 1;
                        var currentTable;
                        var currentDivision = "";

                        for (var i = 0 ; i < teams.length ; i++) {
                            var team = teams[i];

                            // add a new table
                            if (team.Conference != currentConf || (team.Conference == currentConf && team.Division != currentDiv)) {
                                rank = 1;
                                currentDivision = "";
                                newConference = currentConf != team.Conference;
                                currentConf = team.Conference;
                                currentDiv = team.Division;

                                // set the name of our table and our header
                                var header = conferences[team.Conference].Name;
                                var linkName = conferences[team.Conference].Name.replace(" ", "");
                                currentTableName = linkName;

                                // 30 is no division, so if we have a division append the SubName to the table name
                                if (team.Division != 30) {
                                    currentTableName = currentTableName + divisions[team.Division].SubName;
                                    header = header + " " + divisions[team.Division].SubName;
                                    currentDivision = divisions[team.Division].Name;
                                }

                                header = header + " Standings";
                                currentTableName = currentTableName + "Table";
                                createStandingsTable(table, currentTableName, header, newConference ? linkName : "");
                                currentTable = document.getElementById(currentTableName);
                            }

                            var cells = [rank, createTeamLogoLink(team.TeamId, 35), createTeamLinkTableCell(team.TeamId, team.Team), team.Record, team.ConfRecord, team.DivRecord, conferences[team.Conference].Name, currentDivision];
                            addBasicRowsToTable(currentTable, cells, "c3");
                            rank++;
                        }
                    }
                });
            }
        }
    });
}

function createStandingsTable(table, tableName, header, linkName, isPreason) {
    var row = table.insertRow(-1);
    var cell = row.insertCell(-1);
    cell.width = 800;
    cell.align = "center";
    cell.colSpan = 8;
    cell.innerHTML = '<table id="' + tableName + '" cellspacing=1 cellpadding=2 width="80%" class=standard></table><br>';


    // set the header for our new table
    var hasLinkName = linkName.length > 0;
    var standingsTable = document.getElementById(tableName);
    row = standingsTable.insertRow(-1);
    cell = row.insertCell(-1);
    cell.className = "c2";
    cell.colSpan = 8;
    cell.innerHTML = '<b>' + (hasLinkName ? '<a name="' + linkName + '">' : '') + '<center>' + header + '</center>' + (hasLinkName ? '</a>' : "") + '</b>';

    // set the row
    row = standingsTable.insertRow(-1);
    var cells = [["8%", "Rank"], ["8%", ""], ["15%", "Team"], ["9%", "W-L"], ["9%", "Conf W-L"]];

    if (isPreason == false || isPreason == undefined) {
        cells.push(["9%", "Div W-L"]);
        cells.push(["20%", "Conference"]);
        cells.push(["9%", "Division"]);
    }
    for (var i = 0 ; i < cells.length; i++) {
        cell = row.insertCell(-1);
        cell.className = "c7";
        cell.width = cells[i][0];
        cell.innerHTML = cells[i][1];
    }
}

function loadCCData(year) {
    startingYear = year;

    $.ajax({
        url: "cc.csv",
        success: function (data) {
            var confId = getQueryVariable("id");
            var teams = csvJSON(data, "ConferenceId", confId);
            var table = document.getElementById("ccTable");
            var currentConf = -1;
            // we have a Conf Logo
            if (confId.length != 0) {
                var logo = document.getElementById("ccChampLogo");
                logo.src = "../HTML/Logos/conferences/" + confId + ".jpg";

                var trophy = document.getElementById("ccChampTrophy");
                trophy.src = htmlDir + "/Logos/conference_trophies/" + confId + ".png";
            }

            for (var i = 0 ; i < teams.length ; i++) {
                var team = teams[i];

                // add a divider for a new confeerence
                if (currentConf != team.ConferenceId) {
                    var row = table.insertRow(-1);
                    var cells = [["20%", "Year"], ["20%", "Conference"], ["20%", "Team"], ["40%", "Team"]];

                    for (var j = 0 ; j < cells.length ; j++) {
                        var cell = row.insertCell(-1);
                        cell.className = "c7";
                        cell.width = cells[j][0];
                        cell.innerHTML = cells[j][1];
                    }

                    currentConf = team.ConferenceId;
                }

                var cells = [startingYear + Number(team.Year), createConfLinkTableCell(team.ConferenceId, team.Conference), createTeamLogoLink(team.TeamId, 35), createTeamLinkTableCell(team.TeamId, team.Team)];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadConfRoundData() {
    $.ajax({
        url: "confround.csv",
        success: function (data) {
            currentData = csvJSON(data);
            writeConfRankTable(currentData);
        }
    });
}

function writeConfRankTable(conferences) {
    var table = document.getElementById("crtable");

    for (var i = 0 ; i < conferences.length ; i++) {

        var conf = conferences[i];
        var cells = [1 + i, createConferenceLogoLink(conf.ConfId), conf.Conference, Math.floor(conf.TopSix / 10) / 10, Math.floor(conf.All / 10) / 10, conf.BowlRecord, conf.OOCRecord, conf.POOCRecord];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByTop6() {
    cleanTable("crtable", currentData.length);
    currentData.sort(function (a, b) { return a.TopSix - b.TopSix });
    writeConfRankTable(currentData);
}

function sortByWholeConf() {
    cleanTable("crtable", currentData.length);
    currentData.sort(function (a, b) { return a.All - b.All });
    writeConfRankTable(currentData);
}

function sortByBowlRecord() {
    cleanTable("crtable", currentData.length);
    currentData.sort(function (a, b) { return b.BowlPct - a.BowlPct });
    writeConfRankTable(currentData);
}

function sortByOOCRecord() {
    cleanTable("crtable", currentData.length);
    currentData.sort(function (a, b) { return b.OOCPct - a.OOCPct });
    writeConfRankTable(currentData);
}

function sortByPOOCRecord() {
    cleanTable("crtable", currentData.length);
    currentData.sort(function (a, b) { return b.POOCPct - a.POOCPct });
    writeConfRankTable(currentData);
}

function loadHFAData() {
    $.ajax({
        url: "team",
        success: function (json) {
            currentData = evalJson(json);
            currentData.sort(
                function (a, b) {
                    if (a.HWP == undefined) {
                        // hfa rating is win pct + homestreak raw
                        a.HWP = (a.HomeWin * 1000 / (a.HomeWin + a.HomeLoss + a.HomeTie))
                        a.HFA = a.HWP + a.HomeStreakRaw;
                    }

                    if (b.HWP == undefined) {
                        // hfa rating is win pct + homestreak raw
                        b.HWP = b.HomeWin * 1000 / (b.HomeWin + b.HomeLoss + b.HomeTie);
                        b.HFA = b.HWP + b.HomeStreakRaw;
                    }

                    return b.HFA - a.HFA;
                });

            writeHFATable(currentData);
        }
    });
}

function writeHFATable(teams) {
    var table = document.getElementById("nctable");
    for (var i = 0 ; i < teams.length ; i++) {
        var team = teams[i];

        if (isFCS(team.Id))
            continue;

        var cells = [1 + i, createTeamLogoLink(team.Id, 35), createTeamLinkTableCell(team.Id, team.Name), team.HomeWin + "-" + team.HomeLoss + "-" + team.HomeTie, "." + Math.floor(team.HWP), team.HomeStreak];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByHFAPct() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return b.HWP - a.HWP });
    writeHFATable(currentData);
}

function sortByHFAWins() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return b.HomeWin - a.HomeWin });
    writeHFATable(currentData);
}


function loadNCData() {
    $.ajax({
        url: "nc.csv",
        success: function (data) {
            currentData = csvJSON(data);
            writeNCTable(currentData);
        }
    });
}

function writeNCTable(teams) {
    var table = document.getElementById("nctable");

    for (var i = 0 ; i < teams.length ; i++) {

        var team = teams[i];
        var cells = [1 + i, createTeamLogoLink(team.TeamId, 35), createTeamLinkTableCell(team.TeamId, team.Team), team.Record, "." + team.WinPct, team.National, team.Conference];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function sortByAllTimeWins() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return getWinsFromRecord(b.Record) - getWinsFromRecord(a.Record) });
    writeNCTable(currentData);
}

function sortByNC() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.National) - Number(a.National) });
    writeNCTable(currentData);
}

function sortByCC() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.Conference) - Number(a.Conference) });
    writeNCTable(currentData);
}

function sortByAllTimeWinPct() {
    cleanTable("nctable", currentData.length);
    currentData.sort(function (a, b) { return Number(b.WinPct) - Number(a.WinPct) });
    writeNCTable(currentData);
}

function getWinsFromRecord(record) {
    return Number(record.substring(0, record.indexOf("-")));
}

function loadPollData() {
    $.ajax({
        url: "polls.csv",
        success: function (data) {
            var teams = csvJSON(data);

            // figure out which poll it is and set the title
            var pollId = getQueryVariable("type") == "coach" ? 2 : 1; // 1 is media, 2 is coaches
            var title = pollId == 1 ? "Media Poll" : "Coaches Poll";
            document.title = title;

            // set the header
            var header = document.getElementById("pollHeader");   // either Coaches Poll or Media Poll
            header.innerText = title;

            // set the image correctly
            var pollImage = document.getElementById("pollimage"); // it's either  ../HTML/Logos/MediaPoll.jpg or ../HTML/Logos/CoachesPoll.jpg
            pollImage.src = htmlDir + "/Logos/" + title.replace(" ", "") + ".jpg";

            var table = document.getElementById("pollTable");

            var rank = 1;
            for (var i = 0 ; i < teams.length ; i++) {
                var team = teams[i];

                if (team.Table == pollId) {
                    var cells = [rank, createTeamLinkTableCell(team.TeamId, team.Team), team.Record, team.Points, team.FPV, team.Previous];

                    // insert the rows
                    addBasicRowsToTable(table, cells, "c3");
                    rank++;
                }
            }
        }
    });
}

function loadBCSData() {
    $.ajax({
        url: "bcs.csv",
        success: function (data) {
            var teams = csvJSON(data);
            var table = document.getElementById("bcsTable");

            for (var i = 0 ; i < teams.length ; i++) {

                var team = teams[i];
                var cells = [1 + i, createTeamLinkTableCell(team.TeamId, team.Team), team.Record, team.Media, team.Coaches, team.BCSPrevious];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadSOSData() {
    $.ajax({
        url: "sos.csv",
        success: function (data) {
            var cc = csvJSON(data);
            currentData = cc;
            writeSOSTable(cc, 3);
        }
    });
}

// boldIndex should be 4 for oppRank or 7 for winpct
function writeSOSTable(teams, boldIndex) {
    var table = document.getElementById("sosTable");

    for (var i = 0 ; i < teams.length ; i++) {

        var cells = [1 + i, createTeamLinkTableCell(teams[i].TeamId, teams[i].Team), teams[i].Record,
            Number(teams[i].OppAvgRank) / 10, teams[i].OppWin, teams[i].OppLoss, "." + teams[i].OppWinPct,
           teams[i].BCS, teams[i].Coaches, teams[i].Media];

        cells[boldIndex] = "<b>" + cells[boldIndex] + "</b>";

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function cleanSosTable() {
    var table = document.getElementById("sosTable");
    for (var i = 0 ; i < currentData.length ; i++) {
        table.deleteRow(-1);
    }
}

function sortByAvgRank() {
    cleanSosTable();
    currentData.sort(function (a, b) { return Number(a.OppAvgRank) - Number(b.OppAvgRank) });
    writeSOSTable(currentData, 3);
}

function sortByWinPct() {
    cleanSosTable();
    currentData.sort(function (a, b) { return Number(b.OppWinPct) - Number(a.OppWinPct) });
    writeSOSTable(currentData, 6);
}

function randNext(inclusiveFrom, exclusiveTo) {
    var rand = Math.floor((Math.random() * 100000));
    return inclusiveFrom + (rand % (exclusiveTo - inclusiveFrom));
}

function loadTeamPageTemplate(team) {
    document.title = team.Name + " " + team.Mascot;
    var logo = document.getElementById("teamLogoImg");
    if (logo != null) {
        logo.src = createTeamLogoSrc(teamId, 256);
    }

    // check to see if we have a headline element
    var headline = document.getElementById("headlineContent");
    var headlineText = document.getElementById("headlineText");
    if (headline != null && team.MediaCoverage != undefined && team.MediaCoverage != null) {
        var randIdx = randNext(0, team.MediaCoverage.length);
        var mc = team.MediaCoverage[randIdx];
        headline.innerText = mc.Headline;
        headlineText.innerHTML = mc.Content + "<br/><br/>" + team.Article;
    }

    document.getElementById("teamScheduleLink").href += "?id=" + teamId;

    // stats link may not always be there
    var statsLink = document.getElementById("playerStatsLink");
    if (statsLink != null) {
        statsLink.href = "teampstat.html?id=" + teamId;
    }

    document.getElementById("rosterLink").href += "?id=" + teamId;
    document.getElementById("freshmanLink").href += "?id=" + teamId;
    document.getElementById("recordBookLink").href += "?id=" + teamId;

    if (document.getElementById("topAllTime") != null && document.getElementById("topAllTime") != undefined) {
        document.getElementById("topAllTime").href = "../TopPlayers.html?id=" + teamId + "&yr=" + getCurrentYear() + "&top=" + topPlayers;
    }

    if (document.getElementById("trophyCase") != null && document.getElementById("trophyCase") != undefined) {
        document.getElementById("trophyCase").href = "../AwardHistory.html?id=" + teamId + "&yr=" + getCurrentYear() + "";
    }

    if (document.getElementById("recruitLink") != null && document.getElementById("recruitLink") != undefined) {
        document.getElementById("recruitLink").href += "?id=" + teamId;
    }
}

function loadTeamStatsData(year, baseYear) {
    currentYear = year;
    startingYear = baseYear;
    teamId = getQueryVariable("id");


    document.getElementById("topPerfLink").href = "teamtopperf.html?id=" + teamId;
    document.getElementById("careerStatsLink").href = "teampstat.html?career=true&id=" + teamId;
    document.getElementById("playerStatsLink2").href = "teampstat.html?id=" + teamId;

    $.ajax({
        url: "team", // "team" + teamId + ".json",
        // async: false,
        success: function (data) {
            var team = findTeam(data, teamId);
            loadTeamPageTemplate(team);
        }
    });

    loadCareerStats = getQueryVariable("career") == "true";

    $.ajax({
        url: "team" + teamId + (loadCareerStats ? "cstat.csv" : "pstat.csv"),
        success: function (data) {
            var cc = csvJSON(data);
            loadStatsTables(cc);
        }
    });
}

function loadStatsTables(stats) {
    var tables =
        [
            document.getElementById("passingTable"),
            document.getElementById("rushingTable"),
            document.getElementById("receivingTable"),
            document.getElementById("olTable"),
            document.getElementById("defTable"),
            document.getElementById("kickTable"),
            document.getElementById("puntTable"),
            document.getElementById("returnTable"),
        ];

    fillInStatsTables(stats, tables);
}

function fillInStatsTables(stats, tables) {

    for (var i = 0 ; i < stats.length ; i++) {
        var table = null;

        //var year = loadCareerStats ? (Number(stats[i].Year) + startingYear) : currentYear;

        var year = (Number(stats[i].Year) + startingYear); // need to fix bug when v3.03 is release, currently team season stats year is wrong when viewing older seasons

        // create our data for our cells
        var cells = [year, stats[i].No, "<b>" + stats[i].Name + "</b>", stats[i].PlayerClass, stats[i].Position, stats[i].Height, stats[i].Weight, stats[i].Stat1, stats[i].Stat2];
        if (stats[i].Stat3 != "" && stats[i].TableIdx == 5) {
            // sacks are multiplied by 10
            cells[cells.length] = Number(stats[i].Stat3) / 10;
        }
        else if (stats[i].Stat3 != "") {
            cells[cells.length] = stats[i].Stat3;
        }
        if (stats[i].Stat4 != "") cells[cells.length] = stats[i].Stat4;
        if (stats[i].Stat5 != "") cells[cells.length] = stats[i].Stat5;
        if (stats[i].Stat6 != "") cells[cells.length] = stats[i].Stat6;
        cells[cells.length] = stats[i].Games;

        // get the table to insert into
        var table = tables[Number(stats[i].TableIdx) - 1];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function addBasicRowsToTable(table, cells, className, widths) {
    var row = table.insertRow(-1);
    for (var j = 0 ; j < cells.length ; j++) {
        var cell = row.insertCell(-1);
        cell.className = className;
        cell.innerHTML = cells[j];

        if (widths != undefined) {
            cell.width = widths[j];
        }
    }
}

function loadTopProgramConfData() {
    $.ajax({
        url: "conf",
        // async: false,
        success: function (data) {
            conferences = evalJson(data);

            $.ajax({
                url: "topprogramsconf.csv",
                // async: false,
                success: function (data) {
                    var cc = csvJSON(data);
                    createTopProgramConfTable(cc);
                }
            });

            $.ajax({
                url: "hotprogramsconf.csv",
                // async: false,
                success: function (data) {
                    var cc = csvJSON(data);
                    createHotProgramConfTable(cc);
                }
            });
        }
    });
}

function createTopProgramConfTable(cc) {
    var table = document.getElementById("topPrograms");
    var currentConference = -1;
    var currentRank = 1;

    for (var i = 0 ; i < cc.length ; i++) {

        // check to see if we need to add a conference row
        if (cc[i].ConfId != currentConference) {
            currentConference = cc[i].ConfId;
            rank = 1;
            addTopProgramConfRow(table, currentConference);
        }

        var cells = [rank, createTeamLinkTableCell(cc[i].TeamId, cc[i].Name), cc[i].Record, formatPct(cc[i].Pct), cc[i].Last3Yr];
        addBasicRowsToTable(table, cells, "c3");
        rank++;
    }
}

function addConferencesRowsForTopOrHotPrograms(table, cellWidth, cellInner) {
    var row = table.insertRow(-1);
    for (var i = 0 ; i < cellWidth.length; i++) {
        var cell = row.insertCell(-1);
        cell.className = "c7";
        cell.width = cellWidth[i];
        cell.innerHTML = "<center><b>" + cellInner[i] + "</b></center>";
    }
}

function addTopProgramConfRow(table, confId) {
    var conf = conferences[confId];
    var cellWidth = ["10%", "36%", "18%", "18%", "18%"];
    var cellInner = ["Rank", conf.Name, "Current<br/>W-L", "Win %", "3yr<br/>W-L"];
    addConferencesRowsForTopOrHotPrograms(table, cellWidth, cellInner);
}

function createHotProgramConfTable(cc) {
    var table = document.getElementById("hotPrograms");
    var currentConference = -1;
    var currentRank = 1;

    for (var i = 0 ; i < cc.length ; i++) {

        // check to see if we need to add a conference row
        if (cc[i].ConfId != currentConference) {
            currentConference = cc[i].ConfId;
            rank = 1;
            addHotProgramConfRow(table, currentConference);
        }

        var row = table.insertRow(-1);
        var cells = [rank, createTeamLinkTableCell(cc[i].TeamId, cc[i].Name), cc[i].Last3Yr, formatPct(cc[i].Pct), cc[i].Record, cc[i].Trend];
        for (var j = 0 ; j < cells.length ; j++) {
            var cell = row.insertCell(-1);
            cell.className = "c3";
            cell.innerHTML = cells[j];
        }
        rank++;
    }
}

function addHotProgramConfRow(table, confId) {
    var conf = conferences[confId];
    var cellWidth = ["10%", "36%", "18%", "18%", "18%", "12%"];
    var cellInner = ["Rank", conf.Name, "3yr<br/>W-L", "3yr<br/>Win %", "Current<br/>W-L", "Trend"];
    addConferencesRowsForTopOrHotPrograms(table, cellWidth, cellInner);
}

function loadTopProgramData() {

    $.ajax({
        url: "topprograms.csv",
        // async: false,
        success: function (data) {
            var cc = csvJSON(data);
            addTopProgramsRows(cc);
        }
    });

    $.ajax({
        url: "hotprograms.csv",
        // async: false,
        success: function (data) {
            var cc = csvJSON(data);
            addHotProgramsRows(cc);
        }
    });
}

function addHotProgramsRows(cc, name, id) {
    var table = document.getElementById("hotPrograms");
    for (var i = 0 ; i < cc.length ; i++) {
        var row = table.insertRow(-1);
        var cells = [i + 1, createTeamLinkTableCell(cc[i].TeamId, cc[i].Name), cc[i].Record, formatPct(cc[i].Pct), cc[i].Year3, cc[i].Year2, cc[i].Year1, cc[i].Trend];
        for (var j = 0 ; j < cells.length ; j++) {
            var cell = row.insertCell(-1);
            cell.className = "c3";
            cell.innerHTML = cells[j];
        }
    }
}

function addTopProgramsRows(cc, name, id) {
    var table = document.getElementById("topPrograms");
    for (var i = 0 ; i < cc.length ; i++) {
        var row = table.insertRow(-1);
        var cells = [i + 1, createTeamLinkTableCell(cc[i].TeamId, cc[i].Name), cc[i].Record, formatPct(cc[i].Pct)];
        for (var j = 0 ; j < cells.length ; j++) {
            var cell = row.insertCell(-1);
            cell.className = "c3";
            cell.innerHTML = cells[j];
        }
    }
}

function createConfLinkTableCell(id, name) {
    return '<a href="CC.html?id=' + id + '">' + name + '</a>';
}

function createBowlLinkTableCell(id, name) {
    return '<a href="bowlchampions.html?id=' + id + '">' + name + '</a>';
}

function createTeamLinkTableCell(id, name, isPreseason) {
    var teamPage = "team.html";
    if (isPreseason) {
        teamPage = "PreSeasonTeam.html"; // + teamPage;
    }
    return '<a href="' + teamPage + '?id=' + id + '">' + name + '</a>';
}

function createTeamStatLinkTableCell(id, name) {
    return '<a href="teampstat.html?id=' + id + '">' + name + '</a>';
}

function formatPct(str) {
    if (str.length == 1) {
        str = "00" + str;
    }
    else if (str.length == 2) {
        str = "0" + str;
    }

    return "." + str;
}

function loadTeamMainData(isPreseason) {
    teamId = getQueryVariable("id");
    var file = isPreseason ? "ps-team" : "team";
    $.ajax({
        url: file, // "team" + teamId + ".json",
        // async: false,
        success: function (data) {
            var team = findTeam(data, teamId);
            addTeamData(team, isPreseason);
        }
    });
}

function addConfRows(cc, name, id) {
    var foundTeam = false;
    var table = document.getElementById("confChamps");
    for (var i = 0 ; i < cc.length ; i++) {
        if (Number(cc[i].TeamId) == id) {
            foundTeam = true;
            var row = table.insertRow(-1);
            var cells = [startingYear + Number(cc[i].Year), createConfTrophyLink(cc[i].ConfId), '<a href="CC.html?id=' + cc[i].ConfId + '"><img src="../HTML/Logos/conferences/65/' + cc[i].ConfId + '.jpg" /></a>', createTeamLogoLink(id, 55), name];
            for (var j = 0 ; j < cells.length ; j++) {
                var cell = row.insertCell(-1);
                cell.className = "c3";
                cell.innerHTML = cells[j];
            }
        }
        else if (foundTeam == true) {
            break;
        }
    }
}

function addAnnualRecordRows(records, teamId) {
    var foundTeam = false;
    var table = document.getElementById("annualtable");
    for (var i = 0 ; i < records.length ; i++) {

        if (Number(records[i].TeamId) == teamId) {
            foundTeam = true;
            var row = table.insertRow(-1);

            var yearToCheck = startingYear + Number(records[i].Year);
            var directory = isYearInSeasonsData(yearToCheck);

            //the year
            var cell = row.insertCell(0);
            cell.className = "c3";
            if (directory == null) {
                cell.innerHTML = yearToCheck;
            }
            else {
                cell.innerHTML = "<a href='" + "../" + directory + "/team.html?id=" + teamId + "'><b>" + yearToCheck + "</b></a>";
            }

            //the record
            var cell = row.insertCell(1);
            cell.className = "c3";
            cell.innerHTML = records[i].Record;
        }
        else if (foundTeam == true) {
            break;
        }
    }
}

function addBowlRows(cc, name, id) {
    var foundTeam = false;
    var table = document.getElementById("bowlChampionships");
    for (var i = 0 ; i < cc.length ; i++) {
        if (Number(cc[i].TeamId) == id) {
            foundTeam = true;
            var row = table.insertRow(-1);
            var cells = [startingYear + Number(cc[i].Year), createBowlTrophyLink(cc[i].BowlId), createBowlLogoLink(cc[i].BowlId), createTeamLogoLink(id, 55), name];
            for (var j = 0 ; j < cells.length ; j++) {
                var cell = row.insertCell(-1);
                cell.className = "c3";
                cell.innerHTML = cells[j];
            }
        }
        else if (foundTeam == true) {
            break;
        }
    }
}

function createConferenceLogoLink(id) {
    return '<img src="' + createConferenceLogoSrc(id) + '" />';
}

function createConferenceLogoSrc(id) {
    return '../HTML/Logos/conferences/65/' + id + '.jpg';
}

function createTrophyCaseConferenceLogo(id) {
    return '<img src="./HTML/Logos/conferences/65/' + id + '.jpg" />';
}

function createTeamLogoLink(id, size) {
    return '<img src="' + createTeamLogoSrc(id, size) + '" />';
}

function createTeamLogoLinkToTeams(id, size, teamPage) {
    if (teamPage == null || teamPage == undefined)
        teamPage = "team.html";

    return '<a href="' + teamPage + '?id=' + id + '"> <img src="' + createTeamLogoSrc(id, size) + '" /> </a>';
}

function createTeamLogoLinkToStats(id, size) {
    return '<a href="teampstat.html?id=' + id + '"> <img src="' + createTeamLogoSrc(id, size) + '" /> </a>';
}

function createTeamLogoSrc(id, size) {
    return htmlDir + '/Logos/' + size + '/team' + id + '.png';
}

function createBowlLogoLink(bowlId) {
    return '<a href="bowlchampions.html?id=' + bowlId + '"><img src="../HTML/Logos/bowls/65/' + bowlId + '.jpg" /></a>';
}

function createTrophyCaseBowlLogoLink(awardId) {
    return '<img src="./HTML/Logos/bowls/65/' + awardId + '.jpg" /></a>';
}

function createAwardLogoLink(awardId) {
    return '<a href="award.html?id=' + awardId + '"><img src="../HTML/Logos/awards/65/' + awardId + '.png" /></a>';
}

function createTrophyCaseAwardLogoLink(awardId) {
    return '<a href="./HTML/Logos/awards/' + awardId + '.png"> <img src="./HTML/Logos/awards/65/' + awardId + '.png" /></a>';
}

function createBowlTrophyLink(bowlId) {
    return '<a href="bowlchampions.html?id=' + bowlId + '"><img src="../HTML/Logos/bowl_trophies/65/' + bowlId + '.png" /></a>';
}

function createTrophyCaseBowlTrophyLink(awardId) {
    return '<a href="./HTML/Logos/bowl_trophies/' + awardId + '.png"><img src="./HTML/Logos/bowl_trophies/65/' + awardId + '.png" /></a>';
}

function createTrophyCaseConfTrophyLink(awardId) {
    return '<a href="./HTML/Logos/conference_trophies/' + awardId + '.png""> <img src="./HTML/Logos/conference_trophies/65/' + awardId + '.png" /></a>';
}

function createConfTrophyLink(confId) {
    return '<a href="CC.html?id=' + confId + '"><img src="../HTML/Logos/conference_trophies/65/' + confId + '.png" /></a>';
}

function addTeamSchedule(tg, name, id, isPreseason) {
    currentYear = getCurrentYear();

    var table = document.getElementById("scheduleTable");
    for (var i = 0 ; i < tg.length; i++) {

        var result = tg[i].Result == "Win" ? "<b>Win</b>" : "<font color = red><b>Loss</b></font>";
        result = "<a href='../recentmeetings.html?yr=" + currentYear + "&id=" + id + "&opp=" + tg[i].OppId + "'>" + result + "</a>";

        var row = table.insertRow(-1);

        var locale = tg[i].Location;

        if (tg[i].BowlId != undefined && tg[i].BowlId != null && tg[i].BowlId != "") {
            locale = "<a href='../BowlHistory.html?yr=" + currentYear + "&id=" + tg[i].BowlId + "'/>" + locale + "</a>";
        }

        var cells = [
            1 + Number(tg[i].Week),
            locale,
            createTeamLogoLink(tg[i].OppId, 35),
            createTeamLinkTableCell(tg[i].OppId, tg[i].Opponent, isPreseason)];

        if (!isPreseason) {
            cells[cells.length] = tg[i].Result = result,
            cells[cells.length] = '<a href="boxscore.html?id=' + tg[i].Week + '-' + tg[i].Game + '">' + tg[i].Score + '</a>';
        }
        else {
            cells[cells.length] = (tg[i].WinPredicted == "true" ? "-" : "+") + Number(tg[i].Spread) / 10;
            cells[cells.length] = "<a href='../recentmeetings.html?yr=" + (currentYear - 1) + "&id=" + id + "&opp=" + tg[i].OppId + "'>Recent Meetings</a>";
        }

        if (i == tg.length - 1) {
            cells = ['', '', '', tg[i].Opponent, '', ''];
        }

        for (var j = 0 ; j < cells.length ; j++) {
            var cell = row.insertCell(-1);
            cell.className = "c3";
            cell.innerHTML = cells[j];
        }
    }
}

function addTeamData(team, isPreseason) {
    currentYear = getCurrentYear();
    var table = document.getElementById("topTable");
    loadTeamPageTemplate(team);
    var scheduleFile = isPreseason ? "ps-tsch.csv" : "tsch.csv";
    var yearMod = isPreseason ? -1 : 0;

    $.ajax({
        url: scheduleFile, //"tsch" + team.Id + ".csv",
        // async: false,
        success: function (data) {
            var cc = csvJSON(data, "TeamId", team.Id);
            addTeamSchedule(cc, team.Name, team.Id, isPreseason);
        }
    });

    // we're going to have a different view for preasons
    var thrFile = isPreseason ? "ps-thr.csv" : "thr.csv";
    $.ajax({
        url: thrFile,
        // async: false,
        success: function (data) {
            loadSeasonsJsonData(
                function () {
                    var cc = csvJSON(data, "TeamId", team.Id);
                    addAnnualRecordRows(cc, team.Id);
                })
        }
    });

    // add the helmet
    var row = table.insertRow(-1);
    var cell = row.insertCell(-1);
    cell.className = "c8";
    cell.width = 400;
    cell.innerHTML = "<center><img border=0 align=center src=../HTML/Logos/256/team" + team.Id + ".png></center><br><center><img border=0 align=center src=../HTML/Logos/helmet/team" + team.Id + ".png></center>"

    // add another table
    cell = row.insertCell(-1);
    cell.className = "c13";
    cell.width = 300;
    cell.align = "right";
    cell.innerHTML = "<table id='recordTable' cellspacing=1 cellpadding=2 width=300 class=standard align='right'>";

    // add a row with the teamHeader
    var recordTable = document.getElementById("recordTable");
    var innerRow = recordTable.insertRow(-1);
    var innerRowCell = innerRow.insertCell(0);
    innerRowCell.className = "c2";
    innerRowCell.colSpan = "2";
    innerRowCell.innerHTML = "<b><center> " + team.Name + " " + team.Mascot + "</center></b>";

    // add historic header
    addHeaderRow(recordTable, "Historic");

    // add all time record row
    var ncYr = "";
    var ccYr = "";
    if (team.LastNationalChampionshipYear != undefined)
        ncYr = " (" + team.LastNationalChampionshipYear + ")";
    if (team.LastConferenceChampionshipYear != undefined)
        ccYr = " (" + team.LastConferenceChampionshipYear + ")";
    addRecordRow(recordTable, "All-Time Record", team.AllTimeWin + "-" + team.AllTimeLoss + "-" + team.AllTimeTie);
    addRecordRow(recordTable, "<a href='../PostSeasonGames.html?yr=" + (yearMod + getCurrentYear()) + "&id=" + team.Id + "'>Bowl Record</a>", team.BowlWin + "-" + team.BowlLoss + "-" + team.BowlTie);
    addRecordRow(recordTable, "<a href=NC.html>National Titles</a>", team.NationalTitles + ncYr);
    addRecordRow(recordTable, "Conference Titles", team.ConferenceTitles + ccYr);

    if (!isPreseason) {
        // add current header
        addHeaderRow(recordTable, "Current");
        addRecordRow(recordTable, "<a href=Standings.html>Current Season</a>", team.Win + "-" + team.Loss);
        addRecordRow(recordTable, "<a href=Standings.html>Conference Record</a>", team.ConferenceWin + "-" + team.ConferenceLoss);
        addRecordRow(recordTable, "<a href=Standings.html>Division Record</a>", team.DivisionWin + "-" + team.DivisionLoss);
        addRecordRow(recordTable, "<a href=Poll.html?type=coach>Coaches Poll</a>", team.CoachesPollRank);
        addRecordRow(recordTable, "<a href=Poll.html?type=media>Media Poll</a>", team.MediaPollRank);
        addRecordRow(recordTable, "Avg Attendance/Capacity", addCommas(team.AverageAttendance) + " / " + addCommas(team.StadiumCapacity));

        // add current header
        addHeaderRow(recordTable, "Team Rankings");
        addRecordRow(recordTable, "<b>Offense</b>", "<b>" + team.OffensiveRankings.Overall + "</b>");
        addRecordRow(recordTable, "Passing - (TD's)", team.OffensiveRankings.Passing + " - (" + team.OffensiveRankings.PassingTD + ")");
        addRecordRow(recordTable, "Rushing - (TD's)", team.OffensiveRankings.Rushing + " - (" + team.OffensiveRankings.RushingTD + ")");
        addRecordRow(recordTable, "Turnovers", team.OffensiveRankings.Turnovers);
        addRecordRow(recordTable, "<b>Defense</b>", "<b>" + team.DefensiveRankings.Overall + "</b>");
        addRecordRow(recordTable, "Passing", team.DefensiveRankings.Passing);
        addRecordRow(recordTable, "Rushing", team.DefensiveRankings.Rushing);
        addRecordRow(recordTable, "Turnovers", team.DefensiveRankings.Turnovers);
    }

    // add current header
    addHeaderRow(recordTable, "Team Ratings");
    addRecordRow(recordTable, "<b>Overall<b>", "<b>" + team.TeamRatingOVR + "</b>");
    addRecordRow(recordTable, "<b>Offense<b>", "<b>" + team.TeamRatingOFF + "</b>");
    addRecordRow(recordTable, "Quarterback", team.TeamRatingQB);
    addRecordRow(recordTable, "Running Backs", team.TeamRatingRB);
    addRecordRow(recordTable, "Receivers", team.TeamRatingWR);
    addRecordRow(recordTable, "Offensive Line", team.TeamRatingOL);
    addRecordRow(recordTable, "<b>Defense<b>", "<b>" + team.TeamRatingDEF + "</b>");
    addRecordRow(recordTable, "Defensive Line", team.TeamRatingDL);
    addRecordRow(recordTable, "Linebackers", team.TeamRatingLB);
    addRecordRow(recordTable, "Defensive Backs", team.TeamRatingDB);
    addRecordRow(recordTable, "<b>Special Teams<b>", "<b>" + team.TeamRatingST + "</b>");

    // add coach header
    addHeaderRow(recordTable, "<a href='../CoachCareer.html?yr=" + (yearMod + getCurrentYear()) + "&name=" + escape(team.HeadCoach.Name) + "&id=" + team.HeadCoach.Id + "'> Head Coach " + team.HeadCoach.Name + "</a>");
    addRecordRow(recordTable, "Career Record", team.HeadCoach.CareerRecord);
    addRecordRow(recordTable, "Team Record", team.HeadCoach.TeamRecord);
    addRecordRow(recordTable, "Seasons", seasonsWithTeam(team.HeadCoach.YearsAsHeadCoach, isPreseason));
    addRecordRow(recordTable, "Winning Seasons", team.HeadCoach.WinningSeasons);
    addRecordRow(recordTable, "Bowl Wins", team.HeadCoach.BowlWins);
    addRecordRow(recordTable, "Conference Championships", team.HeadCoach.ConferenceChampionships);
    addRecordRow(recordTable, "National Championships", team.HeadCoach.NationalChampionships);

    addHeaderRow(recordTable, "<a href='../CoachCareer.html?yr=" + (yearMod + getCurrentYear()) + "&name=" + escape(team.OffensiveCoordinator.Name) + "&id=" + team.OffensiveCoordinator.Id + "'> Offensive Coordinator " + team.OffensiveCoordinator.Name + "</a>");
    addRecordRow(recordTable, "Seasons", seasonsWithTeam(team.OffensiveCoordinator.YearsWithTeam, isPreseason));

    addHeaderRow(recordTable, "<a href='../CoachCareer.html?yr=" + (yearMod + getCurrentYear()) + "&name=" + escape(team.DefensiveCoordinator.Name) + "&id=" + team.DefensiveCoordinator.Id + "'> Defensive Coordinator " + team.DefensiveCoordinator.Name + "</a>");
    addRecordRow(recordTable, "Seasons", seasonsWithTeam(team.DefensiveCoordinator.YearsWithTeam, isPreseason));

    var header = document.getElementById("yearlyHistory");
    header.innerHTML = "<a href='../TeamHistory.html?yr=" + (currentYear + yearMod) + "&id=" + team.Id + "'>Team History</a>";
}

function seasonsWithTeam(value, isPreseason) {
    if (!isPreseason)
        return value;

    var n = Number(value);

    if (n % 10 == 1) {
        return value + "st";
    }

    if (n % 10 == 2) {
        return value + "nd";
    }
    if (n % 10 == 3) {
        return value + "rd";
    }

    return value + "th";
}

function addHeaderRow(table, data) {
    var innerRow = table.insertRow(-1);
    var innerRowCell = innerRow.insertCell(-1);
    innerRowCell.className = "c7";
    innerRowCell.colSpan = "2";
    innerRowCell.innerHTML = "<b><center>" + data + "</center></b>";
}

function addRecordRow(table, name, data) {
    var row = table.insertRow(-1);

    //left side
    var cell = row.insertCell(-1);
    cell.className = "c3";
    cell.colspan = "2";
    cell.width = "50%";
    cell.innerHTML = name;

    //right side
    cell = row.insertCell(-1);
    cell.className = "c3";
    cell.width = "50%";
    cell.innerHTML = data;
}

function loadRecruitData() {
    teamId = Number(getQueryVariable("id"));
    var stateFilter = getQueryVariable("state");

    // we have a Team Logo
    if (teamId != 0) {
        var logo = document.getElementById("currentSchoolLogo");
        logo.src = "../HTML/Logos/500/team" + teamId + ".png";
    }

    $.ajax({
        url: "gems.csv",
        // async: false,
        success: function (data) {
            var gems = csvJSON(data);
            addRecruitRows(gems, "gemsTable", teamId, stateFilter);
        }
    });

    $.ajax({
        url: "busts.csv",
        // async: false,
        success: function (data) {
            var gems = csvJSON(data);
            addRecruitRows(gems, "bustsTable", teamId, stateFilter);
        }
    });

    $.ajax({
        url: "recruits.csv",
        // async: false,
        success: function (data) {
            var gems = csvJSON(data);
            addRecruitRows(gems, "recruitsTable", teamId, stateFilter);
        }
    });
}

function addRecruitRows(recruits, tableName, teamFilter, stateFilter) {
    var headerPct = ["3%", "3%", "16%", "3%", "3%", "3%", "3%", "3%", "3%", "13%", "13%", "13%", "18%"];
    var headerText = ["<b><center>Rnk</center></b>", "<b><center>Pos<br>Rnk</center></b>", "<b><center>Recruit</center></b>", "<b><center>Pos</center></b>", "<b><center>ATH</center></b>", "<b><center>Star</center></b>", "<b><center>Pre<br>Scout</center></b>", "<b><center>Act<br>Ovr</center></b>", "<b><center>Dif</center></b>", "<b><center>Team #1</center></b>", "<b><center>Team #2</center></b>", "<b><center>Team #3</center></b>", "<b><center>Hometown</center></b>"];
    var table = document.getElementById(tableName);
    var hashTable = new Object();

    for (var i = 0 ; i < recruits.length  ; i++) {

        // recruit is committed, that means we need to bold his top choice
        var commitedTeam = Number(recruits[i].CommittedTeam);
        var homeTown = recruits[i]["Hometown"].replace("%x2C", ",");
        var state = homeTown.slice(-2);

        if (stateFilter != false && stateFilter != state)
            continue;

        recruits[i]["Hometown"] = "<a href='recruits.html?state=" + state + "'>" + homeTown + "</a>";

        if (hashTable[state] == undefined)
            hashTable[state] = 1;
        else
            hashTable[state] = 1 + hashTable[state];

        if (teamFilter > 0 && teamFilter != commitedTeam) {
            continue;
        }

        if (commitedTeam > 0) {
            recruits[i].Team1 = "<b>" + recruits[i].Team1 + "</b>";
            recruits[i].Team1 = "<a href='recruits.html?id=" + commitedTeam + "'>" + recruits[i].Team1 + "</a>"
        }

        var row = table.insertRow(-1);

        // add a new header
        if (i > 0 && (i % 100 == 0)) {
            for (var k = 0 ; k < headerPct.length ; k++) {
                var cell = row.insertCell(k);
                cell.className = "c7";
                cell.width = headerPct[k];
                cell.innerHTML = headerText[k];
            }

            // create a new row for the recruit
            row = table.insertRow(-1);
        }

        //add a recruit
        var j = 1;
        for (j = 1 ; j < recruits[i].headers.length - 1 ; j++) {

            var cell = row.insertCell(j - 1);
            cell.className = "c3";
            cell.innerHTML = recruits[i][recruits[i].headers[j]];
        }

        var cell = row.insertCell(j - 1);
        cell.className = "c3";
        cell.innerHTML = "#" + hashTable[state];
    }
}

function loadTopUnitsData() {
    var isPreseason = getQueryVariable("preseason") == "true";
    var teamPage = "team.html";
    if (isPreseason) {
        teamPage = "PreSeasonTeam.html"; // + teamPage;
    }

    loadSeasonsJsonData(
    function () {
        $.ajax({
            url: "topunits",
            success: function (json) {
                var units = evalJson(json);

                addTopUnit(units["TopQB"], "qbTable", teamPage);
                addTopUnit(units["TopHB"], "hbTable", teamPage);
                addTopUnit(units["TopRec"], "recTable", teamPage);
                addTopUnit(units["TopOL"], "olTable", teamPage);
                addTopUnit(units["TopDL"], "dlTable", teamPage);
                addTopUnit(units["TopLB"], "lbTable", teamPage);
                addTopUnit(units["TopDB"], "dbTable", teamPage);
            }
        });
    },
    "../Seasons");
}

function addTopUnit(units, tableName, teamPage) {
    var table = document.getElementById(tableName);
    var lastYear = null;

    if (seasons.Season.length >= 2) {
        lastYear = seasons.Season[seasons.Season.length - 2];
    }

    var lastYear = window.location.href.substring(0, window.location.href.lastIndexOf("/")).replace(seasons.Season[seasons.Season.length - 1].Directory.substring(10), lastYear.Directory.substring(10)) + "/teampstat.html?career=true&id=";

    for (var i = 0 ; i < units.length ; i++) {

        var unit = units[i];
        var cells = [1 + i, createTeamLogoLinkToTeams(unit.TeamId, 55, teamPage), unit.TopPlayer, ""];

        if (lastYear != null) {
            cells[cells.length - 1] = "<a href='" + lastYear + unit.TeamId + "'>Career Stats</a>";
        }

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

function loadTopClassesData() {
    $.ajax({
        url: "teamrecruitranks.csv",
        success: function (data) {
            var bc = csvJSON(data);

            var table = document.getElementById("rankTable");

            for (var i = 0 ; i < bc.length ; i++) {
                var team = bc[i];


                var cells = [
                    i + 1,
                    createTeamLogoLinkToTeams(team.TeamId, 55, "PreSeasonTeam.html"),
                    team["Record"],
                    (Number(team["Pct"]) / 1000).toFixed(3).toString().substring(1),
                    team["Pts"],
                    team["CC"],
                    team["NC"],
                    team["BW"]
                ];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function loadBowlGames(year) {
    $.ajax({
        url: "bowlgames.csv",
        success: function (data) {
            var bc = csvJSON(data);

            var table = document.getElementById("bowlTable");
            var currentBowl = -1;

            for (var i = 0 ; i < bc.length ; i++) {
                var bowl = bc[i];
                var cells = [
                    "<a href='../BowlHistory.html?yr=" + year + "&id=" + bowl.BowlId + "'/>" + bowl.Name + "</a>",
                    bowl.HomeRank,
                    "<a href=team.html?id=" + bowl.HomeTeamId + ">" + bowl.HomeTeam + "</a>",
                    createTeamLogoLink(bowl.HomeTeamId, 35),
                    "<a href=boxscore.html?id=" + bowl.GameId + ">" + bowl.Score + "</a>",
                    createTeamLogoLink(bowl.AwayTeamId, 35),
                    bowl.AwayRank,
                    "<a href=team.html?id=" + bowl.AwayTeamId + ">" + bowl.AwayTeam + "</a>"
                ];

                // insert the rows
                addBasicRowsToTable(table, cells, "c3");
            }
        }
    });
}

function shouldAddGame(playoffsOnly, game, year) {
    if (playoffsOnly == false)
        return true;

    var games = getPlayoffGames(year);
    return games.indexOf(Number(game.BowlId)) >= 0;
}

function loadPlayoffHistory() {
    window.location = "BowlHistory.html?yr=" + currentYear + "&id=-1&sep=true";
}

function loadKickOffHistory() {
    window.location = "BowlHistory.html?yr=" + currentYear + "&id=-2&sep=true";
}

function loadTopGames(year) {
    $.ajax({
        url: "topgames",
        success: function (json) {
            var games = evalJson(json);
            addTopGames(games.ConferenceGames, "confGames");
            addTopGames(games.NonConferenceGames, "nonConfGames");
        }
    });
}

function loadKickoffWeek(year) {
    $.ajax({
        url: "kickoffweek",
        success: function (json) {
            var games = evalJson(json);
            addTopGames(games.KickoffGames, "koweek");
        }
    });
}

function isFCS(id) {
    return id >= 160 && id <= 164;
}

function addTopGames(games, tableName) {
    var table = document.getElementById(tableName);

    for (var i = 0 ; i < games.length ; i++) {

        var game = games[i];
        var matchup = "";
        var away = "";
        var home = "";

        if (game.AwayRank <= 25) {
            away = "#" + game.AwayRank + "  "
        }

        away += game.AwayTeam;
        matchup += createTeamLinkTableCell(game.AwayTeamId, away, true);
        matchup += "<b>";
        if (game.IsNeutral) {
            matchup += "  vs  "
        }
        else {
            matchup += "  at  "
        }
        matchup += "</b>";

        if (game.HomeRank <= 25) {
            home += " #" + game.HomeRank + " "
        }

        home += game.HomeTeam;
        matchup += createTeamLinkTableCell(game.HomeTeamId, home, true);

        var cells = [
            game.Week,
            createTeamLogoLink(game.AwayTeamId, 35),
            matchup,
            createTeamLogoLink(game.HomeTeamId, 35),
            game.SiteName
        ];

        // insert the rows
        addBasicRowsToTable(table, cells, "c3");
    }
}

/*
    Copyright 2008-2013
        Matthias Ehmann,
        Michael Gerhaeuser,
        Carsten Miller,
        Bianca Valentin,
        Alfred Wassermann,
        Peter Wilfahrt


    Dual licensed under the Apache License Version 2.0, or LGPL Version 3 licenses.

    You should have received a copy of the GNU Lesser General Public License
    along with JSXCompressor.  If not, see <http://www.gnu.org/licenses/>.

    You should have received a copy of the Apache License along with JSXCompressor.
    If not, see <http://www.apache.org/licenses/>.
*/
(function () { var e, r, n; (function (t) { function o(e, r) { return C.call(e, r) } function i(e, r) { var n, t, o, i, a, u, c, f, s, l, p = r && r.split("/"), h = k.map, d = h && h["*"] || {}; if (e && "." === e.charAt(0)) if (r) { for (p = p.slice(0, p.length - 1), e = p.concat(e.split("/")), f = 0; e.length > f; f += 1) if (l = e[f], "." === l) e.splice(f, 1), f -= 1; else if (".." === l) { if (1 === f && (".." === e[2] || ".." === e[0])) break; f > 0 && (e.splice(f - 1, 2), f -= 2) } e = e.join("/") } else 0 === e.indexOf("./") && (e = e.substring(2)); if ((p || d) && h) { for (n = e.split("/"), f = n.length; f > 0; f -= 1) { if (t = n.slice(0, f).join("/"), p) for (s = p.length; s > 0; s -= 1) if (o = h[p.slice(0, s).join("/")], o && (o = o[t])) { i = o, a = f; break } if (i) break; !u && d && d[t] && (u = d[t], c = f) } !i && u && (i = u, a = c), i && (n.splice(0, a, i), e = n.join("/")) } return e } function a(e, r) { return function () { return h.apply(t, v.call(arguments, 0).concat([e, r])) } } function u(e) { return function (r) { return i(r, e) } } function c(e) { return function (r) { b[e] = r } } function f(e) { if (o(m, e)) { var r = m[e]; delete m[e], y[e] = !0, p.apply(t, r) } if (!o(b, e) && !o(y, e)) throw Error("No " + e); return b[e] } function s(e) { var r, n = e ? e.indexOf("!") : -1; return n > -1 && (r = e.substring(0, n), e = e.substring(n + 1, e.length)), [r, e] } function l(e) { return function () { return k && k.config && k.config[e] || {} } } var p, h, d, g, b = {}, m = {}, k = {}, y = {}, C = Object.prototype.hasOwnProperty, v = [].slice; d = function (e, r) { var n, t = s(e), o = t[0]; return e = t[1], o && (o = i(o, r), n = f(o)), o ? e = n && n.normalize ? n.normalize(e, u(r)) : i(e, r) : (e = i(e, r), t = s(e), o = t[0], e = t[1], o && (n = f(o))), { f: o ? o + "!" + e : e, n: e, pr: o, p: n } }, g = { require: function (e) { return a(e) }, exports: function (e) { var r = b[e]; return r !== void 0 ? r : b[e] = {} }, module: function (e) { return { id: e, uri: "", exports: b[e], config: l(e) } } }, p = function (e, r, n, i) { var u, s, l, p, h, k, C = []; if (i = i || e, "function" == typeof n) { for (r = !r.length && n.length ? ["require", "exports", "module"] : r, h = 0; r.length > h; h += 1) if (p = d(r[h], i), s = p.f, "require" === s) C[h] = g.require(e); else if ("exports" === s) C[h] = g.exports(e), k = !0; else if ("module" === s) u = C[h] = g.module(e); else if (o(b, s) || o(m, s) || o(y, s)) C[h] = f(s); else { if (!p.p) throw Error(e + " missing " + s); p.p.load(p.n, a(i, !0), c(s), {}), C[h] = b[s] } l = n.apply(b[e], C), e && (u && u.exports !== t && u.exports !== b[e] ? b[e] = u.exports : l === t && k || (b[e] = l)) } else e && (b[e] = n) }, e = r = h = function (e, r, n, o, i) { return "string" == typeof e ? g[e] ? g[e](r) : f(d(e, r).f) : (e.splice || (k = e, r.splice ? (e = r, r = n, n = null) : e = t), r = r || function () { }, "function" == typeof n && (n = o, o = i), o ? p(t, e, r, n) : setTimeout(function () { p(t, e, r, n) }, 4), h) }, h.config = function (e) { return k = e, k.deps && h(k.deps, k.callback), h }, n = function (e, r, n) { r.splice || (n = r, r = []), o(b, e) || o(m, e) || (m[e] = [e, r, n]) }, n.amd = { jQuery: !0 } })(), n("../node_modules/almond/almond", function () { }), n("jxg", [], function () { var e = {}; return "object" != typeof JXG || JXG.extend || (e = JXG), e.extend = function (e, r, n, t) { var o, i; n = n || !1, t = t || !1; for (o in r) (!n || n && r.hasOwnProperty(o)) && (i = t ? o.toLowerCase() : o, e[i] = r[o]) }, e.extend(e, { boards: {}, readers: {}, elements: {}, registerElement: function (e, r) { e = e.toLowerCase(), this.elements[e] = r }, registerReader: function (e, r) { var n, t; for (n = 0; r.length > n; n++) t = r[n].toLowerCase(), "function" != typeof this.readers[t] && (this.readers[t] = e) }, shortcut: function (e, r) { return function () { return e[r].apply(this, arguments) } }, getRef: function (e, r) { return e.select(r) }, getReference: function (e, r) { return e.select(r) }, debugInt: function () { var e, r; for (e = 0; arguments.length > e; e++) r = arguments[e], "object" == typeof window && window.console && console.log ? console.log(r) : "object" == typeof document && document.getElementById("debug") && (document.getElementById("debug").innerHTML += r + "<br/>") }, debugWST: function () { var r = Error(); e.debugInt.apply(this, arguments), r && r.stack && (e.debugInt("stacktrace"), e.debugInt(r.stack.split("\n").slice(1).join("\n"))) }, debugLine: function () { var r = Error(); e.debugInt.apply(this, arguments), r && r.stack && e.debugInt("Called from", r.stack.split("\n").slice(2, 3).join("\n")) }, debug: function () { e.debugInt.apply(this, arguments) } }), e }), n("utils/zip", ["jxg"], function (e) { var r = [0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240, 8, 136, 72, 200, 40, 168, 104, 232, 24, 152, 88, 216, 56, 184, 120, 248, 4, 132, 68, 196, 36, 164, 100, 228, 20, 148, 84, 212, 52, 180, 116, 244, 12, 140, 76, 204, 44, 172, 108, 236, 28, 156, 92, 220, 60, 188, 124, 252, 2, 130, 66, 194, 34, 162, 98, 226, 18, 146, 82, 210, 50, 178, 114, 242, 10, 138, 74, 202, 42, 170, 106, 234, 26, 154, 90, 218, 58, 186, 122, 250, 6, 134, 70, 198, 38, 166, 102, 230, 22, 150, 86, 214, 54, 182, 118, 246, 14, 142, 78, 206, 46, 174, 110, 238, 30, 158, 94, 222, 62, 190, 126, 254, 1, 129, 65, 193, 33, 161, 97, 225, 17, 145, 81, 209, 49, 177, 113, 241, 9, 137, 73, 201, 41, 169, 105, 233, 25, 153, 89, 217, 57, 185, 121, 249, 5, 133, 69, 197, 37, 165, 101, 229, 21, 149, 85, 213, 53, 181, 117, 245, 13, 141, 77, 205, 45, 173, 109, 237, 29, 157, 93, 221, 61, 189, 125, 253, 3, 131, 67, 195, 35, 163, 99, 227, 19, 147, 83, 211, 51, 179, 115, 243, 11, 139, 75, 203, 43, 171, 107, 235, 27, 155, 91, 219, 59, 187, 123, 251, 7, 135, 71, 199, 39, 167, 103, 231, 23, 151, 87, 215, 55, 183, 119, 247, 15, 143, 79, 207, 47, 175, 111, 239, 31, 159, 95, 223, 63, 191, 127, 255], n = [3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0], t = [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 99, 99], o = [1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577], i = [0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13], a = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15], u = 256; return e.Util = e.Util || {}, e.Util.Unzip = function (c) { function f() { return R += 8, O > X ? c[X++] : -1 } function s() { B = 1 } function l() { var e; try { return R++, e = 1 & B, B >>= 1, 0 === B && (B = f(), e = 1 & B, B = 128 | B >> 1), e } catch (r) { throw r } } function p(e) { var n = 0, t = e; try { for (; t--;) n = n << 1 | l(); e && (n = r[n] >> 8 - e) } catch (o) { throw o } return n } function h() { J = 0 } function d(e) { j++, G[J++] = e, z.push(String.fromCharCode(e)), 32768 === J && (J = 0) } function g() { this.b0 = 0, this.b1 = 0, this.jump = null, this.jumppos = -1 } function b() { for (; ;) { if (M[H] >= x) return -1; if (U[M[H]] === H) return M[H]++; M[H]++ } } function m() { var e, r = P[F]; if (17 === H) return -1; if (F++, H++, e = b(), e >= 0) r.b0 = e; else if (r.b0 = 32768, m()) return -1; if (e = b(), e >= 0) r.b1 = e, r.jump = null; else if (r.b1 = 32768, r.jump = P[F], r.jumppos = F, m()) return -1; return H--, 0 } function k(e, r, n) { var t; for (P = e, F = 0, U = n, x = r, t = 0; 17 > t; t++) M[t] = 0; return H = 0, m() ? -1 : 0 } function y(e) { for (var r, n, t, o = 0, i = e[o]; ;) if (t = l()) { if (!(32768 & i.b1)) return i.b1; for (i = i.jump, r = e.length, n = 0; r > n; n++) if (e[n] === i) { o = n; break } } else { if (!(32768 & i.b0)) return i.b0; o++, i = e[o] } } function C() { var u, c, b, m, C, v, A, j, w, U, x, S, z, I, E, L, O; do if (u = l(), b = p(2), 0 === b) for (s(), U = f(), U |= f() << 8, S = f(), S |= f() << 8, 65535 & (U ^ ~S) && e.debug("BlockLen checksum mismatch\n") ; U--;) c = f(), d(c); else if (1 === b) for (; ;) if (C = r[p(7)] >> 1, C > 23 ? (C = C << 1 | l(), C > 199 ? (C -= 128, C = C << 1 | l()) : (C -= 48, C > 143 && (C += 136))) : C += 256, 256 > C) d(C); else { if (256 === C) break; for (C -= 257, w = p(t[C]) + n[C], C = r[p(5)] >> 3, i[C] > 8 ? (x = p(8), x |= p(i[C] - 8) << 8) : x = p(i[C]), x += o[C], C = 0; w > C; C++) c = G[32767 & J - x], d(c) } else if (2 === b) { for (A = Array(320), I = 257 + p(5), E = 1 + p(5), L = 4 + p(4), C = 0; 19 > C; C++) A[C] = 0; for (C = 0; L > C; C++) A[a[C]] = p(3); for (w = q.length, m = 0; w > m; m++) q[m] = new g; if (k(q, 19, A, 0)) return h(), 1; for (z = I + E, m = 0, O = -1; z > m;) if (O++, C = y(q), 16 > C) A[m++] = C; else if (16 === C) { if (C = 3 + p(2), m + C > z) return h(), 1; for (v = m ? A[m - 1] : 0; C--;) A[m++] = v } else { if (C = 17 === C ? 3 + p(3) : 11 + p(7), m + C > z) return h(), 1; for (; C--;) A[m++] = 0 } for (w = T.length, m = 0; w > m; m++) T[m] = new g; if (k(T, I, A, 0)) return h(), 1; for (w = T.length, m = 0; w > m; m++) q[m] = new g; for (j = [], m = I; A.length > m; m++) j[m - I] = A[m]; if (k(q, E, j, 0)) return h(), 1; for (; ;) if (C = y(T), C >= 256) { if (C -= 256, 0 === C) break; for (C -= 1, w = p(t[C]) + n[C], C = y(q), i[C] > 8 ? (x = p(8), x |= p(i[C] - 8) << 8) : x = p(i[C]), x += o[C]; w--;) c = G[32767 & J - x], d(c) } else d(C) } while (!u); return h(), s(), 0 } function v() { var e, r, n, t, o, i, a, c, s = []; try { if (z = [], L = !1, s[0] = f(), s[1] = f(), 120 === s[0] && 218 === s[1] && (C(), E[I] = [z.join(""), "geonext.gxt"], I++), 31 === s[0] && 139 === s[1] && (S(), E[I] = [z.join(""), "file"], I++), 80 === s[0] && 75 === s[1] && (L = !0, s[2] = f(), s[3] = f(), 3 === s[2] && 4 === s[3])) { for (s[0] = f(), s[1] = f(), A = f(), A |= f() << 8, c = f(), c |= f() << 8, f(), f(), f(), f(), a = f(), a |= f() << 8, a |= f() << 16, a |= f() << 24, i = f(), i |= f() << 8, i |= f() << 16, i |= f() << 24, o = f(), o |= f() << 8, o |= f() << 16, o |= f() << 24, t = f(), t |= f() << 8, n = f(), n |= f() << 8, e = 0, N = []; t--;) r = f(), "/" === r | ":" === r ? e = 0 : u - 1 > e && (N[e++] = String.fromCharCode(r)); for (w || (w = N), e = 0; n > e;) r = f(), e++; j = 0, 8 === c && (C(), E[I] = Array(2), E[I][0] = z.join(""), E[I][1] = N.join(""), I++), S() } } catch (l) { throw l } } var A, j, w, U, x, S, z = [], I = 0, E = [], G = Array(32768), J = 0, L = !1, O = c.length, X = 0, B = 1, R = 0, T = Array(288), q = Array(32), F = 0, P = null, H = (Array(64), Array(64), 0), M = Array(17), N = []; M[0] = 0, S = function () { var e, r, n, t, o, i, a = []; if (8 & A && (a[0] = f(), a[1] = f(), a[2] = f(), a[3] = f(), 80 === a[0] && 75 === a[1] && 7 === a[2] && 8 === a[3] ? (e = f(), e |= f() << 8, e |= f() << 16, e |= f() << 24) : e = a[0] | a[1] << 8 | a[2] << 16 | a[3] << 24, r = f(), r |= f() << 8, r |= f() << 16, r |= f() << 24, n = f(), n |= f() << 8, n |= f() << 16, n |= f() << 24), L && v(), a[0] = f(), 8 === a[0]) { if (A = f(), f(), f(), f(), f(), f(), t = f(), 4 & A) for (a[0] = f(), a[2] = f(), H = a[0] + 256 * a[1], o = 0; H > o; o++) f(); if (8 & A) for (o = 0, N = [], i = f() ; i;) ("7" === i || ":" === i) && (o = 0), u - 1 > o && (N[o++] = i), i = f(); if (16 & A) for (i = f() ; i;) i = f(); 2 & A && (f(), f()), C(), e = f(), e |= f() << 8, e |= f() << 16, e |= f() << 24, n = f(), n |= f() << 8, n |= f() << 16, n |= f() << 24, L && v() } }, e.Util.Unzip.prototype.unzipFile = function (e) { var r; for (this.unzip(), r = 0; E.length > r; r++) if (E[r][1] === e) return E[r][0]; return "" }, e.Util.Unzip.prototype.unzip = function () { return v(), E } }, e.Util }), n("utils/encoding", ["jxg"], function (e) { var r = 0, n = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 10, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 3, 3, 11, 6, 6, 6, 5, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 0, 12, 24, 36, 60, 96, 84, 12, 12, 12, 48, 72, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 0, 12, 12, 12, 12, 12, 0, 12, 0, 12, 12, 12, 24, 12, 12, 12, 12, 12, 24, 12, 24, 12, 12, 12, 12, 12, 12, 12, 12, 12, 24, 12, 12, 12, 12, 12, 24, 12, 12, 12, 12, 12, 12, 12, 24, 12, 12, 12, 12, 12, 12, 12, 12, 12, 36, 12, 36, 12, 12, 12, 36, 12, 12, 12, 12, 12, 36, 12, 36, 12, 12, 12, 36, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12]; return e.Util = e.Util || {}, e.Util.UTF8 = { encode: function (e) { var r, n, t = "", o = e.length; if (e = e.replace(/\r\n/g, "\n"), "function" == typeof unescape && "function" == typeof encodeURIComponent) return unescape(encodeURIComponent(e)); for (r = 0; o > r; r++) n = e.charCodeAt(r), 128 > n ? t += String.fromCharCode(n) : n > 127 && 2048 > n ? (t += String.fromCharCode(192 | n >> 6), t += String.fromCharCode(128 | 63 & n)) : (t += String.fromCharCode(224 | n >> 12), t += String.fromCharCode(128 | 63 & n >> 6), t += String.fromCharCode(128 | 63 & n)); return t }, decode: function (e) { var t, o, i, a = 0, u = 0, c = r, f = [], s = e.length, l = []; for (t = 0; s > t; t++) o = e.charCodeAt(t), i = n[o], u = c !== r ? 63 & o | u << 6 : 255 >> i & o, c = n[256 + c + i], c === r && (u > 65535 ? f.push(55232 + (u >> 10), 56320 + (1023 & u)) : f.push(u), a++, 0 === a % 1e4 && (l.push(String.fromCharCode.apply(null, f)), f = [])); return l.push(String.fromCharCode.apply(null, f)), l.join("") }, asciiCharCodeAt: function (e, r) { var n = e.charCodeAt(r); if (n > 255) switch (n) { case 8364: n = 128; break; case 8218: n = 130; break; case 402: n = 131; break; case 8222: n = 132; break; case 8230: n = 133; break; case 8224: n = 134; break; case 8225: n = 135; break; case 710: n = 136; break; case 8240: n = 137; break; case 352: n = 138; break; case 8249: n = 139; break; case 338: n = 140; break; case 381: n = 142; break; case 8216: n = 145; break; case 8217: n = 146; break; case 8220: n = 147; break; case 8221: n = 148; break; case 8226: n = 149; break; case 8211: n = 150; break; case 8212: n = 151; break; case 732: n = 152; break; case 8482: n = 153; break; case 353: n = 154; break; case 8250: n = 155; break; case 339: n = 156; break; case 382: n = 158; break; case 376: n = 159; break; default: } return n } }, e.Util.UTF8 }), n("utils/base64", ["jxg", "utils/encoding"], function (e, r) { function n(e, r) { return 255 & e.charCodeAt(r) } function t(e, r) { var n = o.indexOf(e.charAt(r)); if (-1 === n) throw Error("JSXGraph/utils/base64: Can't decode string (invalid character)."); return n } var o = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", i = "="; return e.Util = e.Util || {}, e.Util.Base64 = { encode: function (e) { var t, a, u, c, f, s = []; for (f = r.encode(e), u = f.length, c = u % 3, t = 0; u - c > t; t += 3) a = n(f, t) << 16 | n(f, t + 1) << 8 | n(f, t + 2), s.push(o.charAt(a >> 18), o.charAt(63 & a >> 12), o.charAt(63 & a >> 6), o.charAt(63 & a)); switch (c) { case 1: a = n(f, u - 1), s.push(o.charAt(a >> 2), o.charAt(63 & a << 4), i, i); break; case 2: a = n(f, u - 2) << 8 | n(f, u - 1), s.push(o.charAt(a >> 10), o.charAt(63 & a >> 4), o.charAt(63 & a << 2), i) } return s.join("") }, decode: function (e, n) { var o, a, u, c, f, s, l = [], p = []; if (o = e.replace(/[^A-Za-z0-9\+\/=]/g, ""), u = o.length, 0 !== u % 4) throw Error("JSXGraph/utils/base64: Can't decode string (invalid input length)."); for (o.charAt(u - 1) === i && (c = 1, o.charAt(u - 2) === i && (c = 2), u -= 4), a = 0; u > a; a += 4) f = t(o, a) << 18 | t(o, a + 1) << 12 | t(o, a + 2) << 6 | t(o, a + 3), p.push(f >> 16, 255 & f >> 8, 255 & f), 0 === a % 1e4 && (l.push(String.fromCharCode.apply(null, p)), p = []); switch (c) { case 1: f = t(o, u) << 12 | t(o, u + 1) << 6 | t(o, u + 2), p.push(f >> 10, 255 & f >> 2); break; case 2: f = t(o, a) << 6 | t(o, a + 1), p.push(f >> 4) } return l.push(String.fromCharCode.apply(null, p)), s = l.join(""), n && (s = r.decode(s)), s }, decodeAsArray: function (e) { var r, n = this.decode(e), t = [], o = n.length; for (r = 0; o > r; r++) t[r] = n.charCodeAt(r); return t } }, e.Util.Base64 }), n("../build/compressor.deps.js", ["jxg", "utils/zip", "utils/base64"], function (e, r, n) { return e.decompress = function (e) { return unescape(new r.Unzip(n.decodeAsArray(e)).unzip()[0][0]) }, e }), window.JXG = r("../build/compressor.deps.js") })();