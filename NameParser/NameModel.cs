using System;
using System.Collections.Generic;
using System.Text;

namespace NameParser
{
    public class NamesFile
    {
        public List<string> First { get; set; }

        public List<string> Last{ get; set; }

        public List<string> HIFN{ get; set; }

        public List<string> HILN { get; set; }

        public List<string> WFN { get; set; }

        public List<string> WLN { get; set; }

        public List<string> CLN { get; set; }
    }

    public interface IName {  public string name { get; } }

    public class NameModel
    {
        public int schemaVersion { get; set; }
        public Source source { get; set; }
        public Generation generation { get; set; }
        public Summary summary { get; set; }
        public Whitefirstname[] WhiteFirstNames { get; set; }
        public Blackfirstname[] BlackFirstNames { get; set; }
        public Americanindianalaskanativefirstname[] AmericanIndianAlaskaNativeFirstNames { get; set; }
        public Asianpacificfirstname[] AsianPacificFirstNames { get; set; }
        public Multiracialfirstname[] MultiracialFirstNames { get; set; }
        public Hispanicfirstname[] HispanicFirstNames { get; set; }
        public Whitelastname[] WhiteLastNames { get; set; }
        public Blacklastname[] BlackLastNames { get; set; }
        public Americanindianalaskanativelastname[] AmericanIndianAlaskaNativeLastNames { get; set; }
        public Asianpacificlastname[] AsianPacificLastNames { get; set; }
        public Multiraciallastname[] MultiracialLastNames { get; set; }
        public Hispaniclastname[] HispanicLastNames { get; set; }
        public Hawaiianfirstname[] HawaiianFirstNames { get; set; }
        public Hawaiianlastname[] HawaiianLastNames { get; set; }
        public Cajunname[] CajunNames { get; set; }
        public Utahpolynesianfirstname[] UtahPolynesianFirstNames { get; set; }
        public Utahpolynesianlastname[] UtahPolynesianLastNames { get; set; }
    }

    public class Source
    {
        public string publisher { get; set; }
        public string dataset { get; set; }
        public string releaseDate { get; set; }
        public string datasetUrl { get; set; }
        public Censuspopulationreference censusPopulationReference { get; set; }
        public Profileselection profileSelection { get; set; }
        public Files files { get; set; }
        public Legacybackup legacyBackup { get; set; }
        public Skintonenamecalibration skinToneNameCalibration { get; set; }
        public Firstnamecohorts firstNameCohorts { get; set; }
        public Statenameflavor stateNameFlavor { get; set; }
        public Utahpolynesianinfluence utahPolynesianInfluence { get; set; }
    }

    public class Censuspopulationreference
    {
        public string dataset { get; set; }
        public string url { get; set; }
        public int totalPopulation { get; set; }
        public string note { get; set; }
    }

    public class Profileselection
    {
        public string publisher { get; set; }
        public string dataset { get; set; }
        public string datasetUrl { get; set; }
        public string file { get; set; }
        public string fileUrl { get; set; }
        public string sha256 { get; set; }
        public string sheet { get; set; }
        public string sport { get; set; }
        public string academicYear { get; set; }
        public Categorycounts categoryCounts { get; set; }
        public int knownProfileTotal { get; set; }
        public int allCategoryTotal { get; set; }
        public Excludedcategorycounts excludedCategoryCounts { get; set; }
        public string note { get; set; }
    }

    public class Categorycounts
    {
        public int AmericanIndianAlaskaNative { get; set; }
        public int Asian { get; set; }
        public int Black { get; set; }
        public int HispanicLatino { get; set; }
        public int International { get; set; }
        public int NativeHawaiianPacificIslander { get; set; }
        public int TwoorMoreRaces { get; set; }
        public int Unknown { get; set; }
        public int White { get; set; }
    }

    public class Excludedcategorycounts
    {
        public int International { get; set; }
        public int Unknown { get; set; }
    }

    public class Files
    {
        public Firstnamesex firstNameSex { get; set; }
        public Firstnameracehispanic firstNameRaceHispanic { get; set; }
        public Lastnameracehispanic lastNameRaceHispanic { get; set; }
    }

    public class Firstnamesex
    {
        public string file { get; set; }
        public string url { get; set; }
        public string sha256 { get; set; }
    }

    public class Firstnameracehispanic
    {
        public string file { get; set; }
        public string url { get; set; }
        public string sha256 { get; set; }
    }

    public class Lastnameracehispanic
    {
        public string file { get; set; }
        public string url { get; set; }
        public string sha256 { get; set; }
    }

    public class Legacybackup
    {
        public string file { get; set; }
        public string sha256 { get; set; }
    }

    public class Skintonenamecalibration
    {
        public Recruitingdata recruitingData { get; set; }
        public Skintonesettings skinToneSettings { get; set; }
    }

    public class Recruitingdata
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int rowCount { get; set; }
    }

    public class Skintonesettings
    {
        public string file { get; set; }
        public string sha256 { get; set; }
    }

    public class Firstnamecohorts
    {
        public string publisher { get; set; }
        public string dataset { get; set; }
        public int[] years { get; set; }
        public string sex { get; set; }
        public Annualprovenance annualProvenance { get; set; }
        public Nationalsource nationalSource { get; set; }
        public Statesource stateSource { get; set; }
        public string[] artifactExclusions { get; set; }
        public string artifactExclusionRule { get; set; }
        public Artifactbirthcounts artifactBirthCounts { get; set; }
        public Artifactrowcounts artifactRowCounts { get; set; }
    }

    public class Annualprovenance
    {
        public _2000 _2000 { get; set; }
        public _2001 _2001 { get; set; }
        public _2002 _2002 { get; set; }
        public _2003 _2003 { get; set; }
        public _2004 _2004 { get; set; }
        public _2005 _2005 { get; set; }
        public _2006 _2006 { get; set; }
        public _2007 _2007 { get; set; }
        public _2008 _2008 { get; set; }
        public _2009 _2009 { get; set; }
        public _2010 _2010 { get; set; }
        public _2011 _2011 { get; set; }
        public _2012 _2012 { get; set; }
        public _2013 _2013 { get; set; }
        public _2014 _2014 { get; set; }
        public _2015 _2015 { get; set; }
        public _2016 _2016 { get; set; }
        public _2017 _2017 { get; set; }
        public _2018 _2018 { get; set; }
        public _2019 _2019 { get; set; }
        public _2020 _2020 { get; set; }
        public _2021 _2021 { get; set; }
        public _2022 _2022 { get; set; }
        public _2023 _2023 { get; set; }
        public _2024 _2024 { get; set; }
    }

    public class _2000
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2001
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2002
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2003
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2004
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2005
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2006
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2007
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2008
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2009
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2010
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2011
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2012
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2013
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2014
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2015
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2016
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2017
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2018
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2019
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2020
    {
        public string source { get; set; }
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _2021
    {
        public string source { get; set; }
        public int selectedMaleNameCount { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
        public string stateFilesInputSha256 { get; set; }
        public string suppressionNote { get; set; }
    }

    public class _2022
    {
        public string source { get; set; }
        public int selectedMaleNameCount { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
        public string stateFilesInputSha256 { get; set; }
        public string suppressionNote { get; set; }
    }

    public class _2023
    {
        public string source { get; set; }
        public int selectedMaleNameCount { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
        public string stateFilesInputSha256 { get; set; }
        public string suppressionNote { get; set; }
    }

    public class _2024
    {
        public string source { get; set; }
        public int selectedMaleNameCount { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
        public string stateFilesInputSha256 { get; set; }
        public string suppressionNote { get; set; }
    }

    public class Nationalsource
    {
        public string dataset { get; set; }
        public string officialDownloadUrl { get; set; }
        public string mirrorRepositoryUrl { get; set; }
        public string mirrorPinnedCommit { get; set; }
        public string directory { get; set; }
        public int[] availableYears { get; set; }
        public int fileCount { get; set; }
        public string inputSha256 { get; set; }
        public Files1 files { get; set; }
    }

    public class Files1
    {
        public _20001 _2000 { get; set; }
        public _20011 _2001 { get; set; }
        public _20021 _2002 { get; set; }
        public _20031 _2003 { get; set; }
        public _20041 _2004 { get; set; }
        public _20051 _2005 { get; set; }
        public _20061 _2006 { get; set; }
        public _20071 _2007 { get; set; }
        public _20081 _2008 { get; set; }
        public _20091 _2009 { get; set; }
        public _20101 _2010 { get; set; }
        public _20111 _2011 { get; set; }
        public _20121 _2012 { get; set; }
        public _20131 _2013 { get; set; }
        public _20141 _2014 { get; set; }
        public _20151 _2015 { get; set; }
        public _20161 _2016 { get; set; }
        public _20171 _2017 { get; set; }
        public _20181 _2018 { get; set; }
        public _20191 _2019 { get; set; }
        public _20201 _2020 { get; set; }
    }

    public class _20001
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20011
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20021
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20031
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20041
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20051
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20061
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20071
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20081
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20091
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20101
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20111
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20121
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20131
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20141
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20151
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20161
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20171
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20181
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20191
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class _20201
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class Statesource
    {
        public string dataset { get; set; }
        public string datasetUrl { get; set; }
        public string officialDownloadUrl { get; set; }
        public string mirrorDatasetUrl { get; set; }
        public string mirrorFileUrlPattern { get; set; }
        public int[] fallbackYears { get; set; }
        public string fallbackRule { get; set; }
        public string suppressionNote { get; set; }
        public int fileCount { get; set; }
        public string inputSha256 { get; set; }
        public Files2 files { get; set; }
    }

    public class Files2
    {
        public AK AK { get; set; }
        public AL AL { get; set; }
        public AR AR { get; set; }
        public AZ AZ { get; set; }
        public CA CA { get; set; }
        public CO CO { get; set; }
        public CT CT { get; set; }
        public DC DC { get; set; }
        public DE DE { get; set; }
        public FL FL { get; set; }
        public GA GA { get; set; }
        public HI HI { get; set; }
        public IA IA { get; set; }
        public ID ID { get; set; }
        public IL IL { get; set; }
        public IN IN { get; set; }
        public KS KS { get; set; }
        public KY KY { get; set; }
        public LA LA { get; set; }
        public MA MA { get; set; }
        public MD MD { get; set; }
        public ME ME { get; set; }
        public MI MI { get; set; }
        public MN MN { get; set; }
        public MO MO { get; set; }
        public MS MS { get; set; }
        public MT MT { get; set; }
        public NC NC { get; set; }
        public ND ND { get; set; }
        public NE NE { get; set; }
        public NH NH { get; set; }
        public NJ NJ { get; set; }
        public NM NM { get; set; }
        public NV NV { get; set; }
        public NY NY { get; set; }
        public OH OH { get; set; }
        public OK OK { get; set; }
        public OR OR { get; set; }
        public PA PA { get; set; }
        public RI RI { get; set; }
        public SC SC { get; set; }
        public SD SD { get; set; }
        public TN TN { get; set; }
        public TX TX { get; set; }
        public UT UT { get; set; }
        public VA VA { get; set; }
        public VT VT { get; set; }
        public WA WA { get; set; }
        public WI WI { get; set; }
        public WV WV { get; set; }
        public WY WY { get; set; }
    }

    public class AK
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class AL
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class AR
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class AZ
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class CA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class CO
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class CT
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class DC
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class DE
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class FL
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class GA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class HI
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class IA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class ID
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class IL
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class IN
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class KS
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class KY
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class LA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MD
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class ME
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MI
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MN
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MO
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MS
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class MT
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NC
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class ND
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NE
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NH
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NJ
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NM
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NV
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class NY
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class OH
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class OK
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class OR
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class PA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class RI
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class SC
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class SD
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class TN
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class TX
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class UT
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class VA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class VT
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class WA
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class WI
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class WV
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class WY
    {
        public string file { get; set; }
        public string sha256 { get; set; }
        public int inputRows { get; set; }
        public int selectedMaleRows { get; set; }
        public int selectedMaleBirthCount { get; set; }
        public int excludedArtifactRows { get; set; }
        public int excludedArtifactBirthCount { get; set; }
    }

    public class Artifactbirthcounts
    {
        public int BABY { get; set; }
        public int BABYBOY { get; set; }
        public int BOY { get; set; }
        public int CHILD { get; set; }
        public int INFANT { get; set; }
        public int INFANTBOY { get; set; }
        public int INFANTMALE { get; set; }
        public int MALE { get; set; }
        public int NAME { get; set; }
        public int NOGIVENNAME { get; set; }
        public int NONAME { get; set; }
        public int NONAMEGIVEN { get; set; }
        public int NONE { get; set; }
        public int NOTNAMED { get; set; }
        public int NULL { get; set; }
        public int UNKNOWN { get; set; }
        public int UNNAMED { get; set; }
    }

    public class Artifactrowcounts
    {
        public int BABY { get; set; }
        public int BABYBOY { get; set; }
        public int BOY { get; set; }
        public int CHILD { get; set; }
        public int INFANT { get; set; }
        public int INFANTBOY { get; set; }
        public int INFANTMALE { get; set; }
        public int MALE { get; set; }
        public int NAME { get; set; }
        public int NOGIVENNAME { get; set; }
        public int NONAME { get; set; }
        public int NONAMEGIVEN { get; set; }
        public int NONE { get; set; }
        public int NOTNAMED { get; set; }
        public int NULL { get; set; }
        public int UNKNOWN { get; set; }
        public int UNNAMED { get; set; }
    }

    public class Statenameflavor
    {
        public string publisher { get; set; }
        public string sourceReference { get; set; }
        public int[] years { get; set; }
        public string sex { get; set; }
        public string factorRule { get; set; }
        public string expectedCountRule { get; set; }
        public float priorCount { get; set; }
        public float[] factorClip { get; set; }
        public int minimumEvidenceCount { get; set; }
        public float minimumFactorDifference { get; set; }
        public int maximumEntriesPerState { get; set; }
    }

    public class Utahpolynesianinfluence
    {
        public string[] sourceUrls { get; set; }
        public string note { get; set; }
    }

    public class Generation
    {
        public string firstNameRule { get; set; }
        public string firstNameWeightRule { get; set; }
        public Firstnameprofilelift firstNameProfileLift { get; set; }
        public string lastNameWeightRule { get; set; }
        public string profileSelectionRule { get; set; }
        public Regionalprofilenotes regionalProfileNotes { get; set; }
        public string[] profileOrder { get; set; }
        public Profiles profiles { get; set; }
        public Firstnamecohorts1 firstNameCohorts { get; set; }
        public Namediversity nameDiversity { get; set; }
        public Skintonenamecalibration1 skinToneNameCalibration { get; set; }
        public Regionalboostpercentagepoints regionalBoostPercentagePoints { get; set; }
        public Specialregions specialRegions { get; set; }
        public Utahpolynesianinfluence1 utahPolynesianInfluence { get; set; }
        public Statenameflavor1 stateNameFlavor { get; set; }
    }

    public class Firstnameprofilelift
    {
        public string profileProbabilityDenominator { get; set; }
        public float[] clip { get; set; }
        public int liftPrecisionDecimalPlaces { get; set; }
        public float censusMissingLift { get; set; }
        public string weightRounding { get; set; }
    }

    public class Regionalprofilenotes
    {
        public string hispanic { get; set; }
        public string asianPacific { get; set; }
        public string hawaiiFirstNames { get; set; }
        public string utahPolynesianInfluence { get; set; }
    }

    public class Profiles
    {
        public White white { get; set; }
        public Black black { get; set; }
        public Americanindianalaskanative americanIndianAlaskaNative { get; set; }
        public Asianpacific asianPacific { get; set; }
        public Multiracial multiracial { get; set; }
        public Hispanic hispanic { get; set; }
    }

    public class White
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Black
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Americanindianalaskanative
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Asianpacific
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Multiracial
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Hispanic
    {
        public string label { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public int selectionCount { get; set; }
        public float selectionPercent { get; set; }
        public int censusPopulationCount { get; set; }
        public float censusPopulationPercent { get; set; }
    }

    public class Firstnamecohorts1
    {
        public int[] availableYears { get; set; }
        public int fallbackCenterYear { get; set; }
        public int windowRadius { get; set; }
        public string encoding { get; set; }
        public string[] profileOrder { get; set; }
        public string[] nameCatalog { get; set; }
        public Annualmalenamecountsbyyear annualMaleNameCountsByYear { get; set; }
        public float[][] censusProfileLiftsByNameIndex { get; set; }
    }

    public class Annualmalenamecountsbyyear
    {
        public int[][] _2000 { get; set; }
        public int[][] _2001 { get; set; }
        public int[][] _2002 { get; set; }
        public int[][] _2003 { get; set; }
        public int[][] _2004 { get; set; }
        public int[][] _2005 { get; set; }
        public int[][] _2006 { get; set; }
        public int[][] _2007 { get; set; }
        public int[][] _2008 { get; set; }
        public int[][] _2009 { get; set; }
        public int[][] _2010 { get; set; }
        public int[][] _2011 { get; set; }
        public int[][] _2012 { get; set; }
        public int[][] _2013 { get; set; }
        public int[][] _2014 { get; set; }
        public int[][] _2015 { get; set; }
        public int[][] _2016 { get; set; }
        public int[][] _2017 { get; set; }
        public int[][] _2018 { get; set; }
        public int[][] _2019 { get; set; }
        public int[][] _2020 { get; set; }
        public int[][] _2021 { get; set; }
        public int[][] _2022 { get; set; }
        public int[][] _2023 { get; set; }
        public int[][] _2024 { get; set; }
    }

    public class Namediversity
    {
        public float firstNameRepeatPenaltyExponent { get; set; }
        public float lastNameRepeatPenaltyExponent { get; set; }
        public int maxAttempts { get; set; }
        public string rule { get; set; }
    }

    public class Skintonenamecalibration1
    {
        public string method { get; set; }
        public string rule { get; set; }
        public string note { get; set; }
        public Baselinetonepercent baselineTonePercent { get; set; }
        public Profiletonelikelihoods profileToneLikelihoods { get; set; }
        public Expectedprofilepercentbytone expectedProfilePercentByTone { get; set; }
    }

    public class Baselinetonepercent
    {
        public float Light { get; set; }
        public float Medium { get; set; }
        public float Dark { get; set; }
    }

    public class Profiletonelikelihoods
    {
        public Light Light { get; set; }
        public Medium Medium { get; set; }
        public Dark Dark { get; set; }
    }

    public class Light
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Medium
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Dark
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Expectedprofilepercentbytone
    {
        public Light1 Light { get; set; }
        public Medium1 Medium { get; set; }
        public Dark1 Dark { get; set; }
    }

    public class Light1
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Medium1
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Dark1
    {
        public float white { get; set; }
        public float black { get; set; }
        public float americanIndianAlaskaNative { get; set; }
        public float asianPacific { get; set; }
        public float multiracial { get; set; }
        public float hispanic { get; set; }
    }

    public class Regionalboostpercentagepoints
    {
        public Alaska Alaska { get; set; }
        public Texas Texas { get; set; }
        public California California { get; set; }
        public Arizona Arizona { get; set; }
        public NewMexico NewMexico { get; set; }
        public Hawaii Hawaii { get; set; }
        public Washington Washington { get; set; }
        public Oregon Oregon { get; set; }
        public Oklahoma Oklahoma { get; set; }
        public NorthDakota NorthDakota { get; set; }
        public SouthDakota SouthDakota { get; set; }
        public Montana Montana { get; set; }
        public Florida Florida { get; set; }
        public Nevada Nevada { get; set; }
        public Colorado Colorado { get; set; }
    }

    public class Alaska
    {
        public int americanIndianAlaskaNative { get; set; }
        public int asianPacific { get; set; }
    }

    public class Texas
    {
        public int hispanic { get; set; }
    }

    public class California
    {
        public int hispanic { get; set; }
        public int asianPacific { get; set; }
    }

    public class Arizona
    {
        public int hispanic { get; set; }
    }

    public class NewMexico
    {
        public int hispanic { get; set; }
    }

    public class Hawaii
    {
        public int asianPacific { get; set; }
    }

    public class Washington
    {
        public int asianPacific { get; set; }
    }

    public class Oregon
    {
        public int asianPacific { get; set; }
    }

    public class Oklahoma
    {
        public int americanIndianAlaskaNative { get; set; }
    }

    public class NorthDakota
    {
        public int americanIndianAlaskaNative { get; set; }
    }

    public class SouthDakota
    {
        public int americanIndianAlaskaNative { get; set; }
    }

    public class Montana
    {
        public int americanIndianAlaskaNative { get; set; }
    }

    public class Florida
    {
        public int hispanic { get; set; }
    }

    public class Nevada
    {
        public int hispanic { get; set; }
        public int asianPacific { get; set; }
    }

    public class Colorado
    {
        public int hispanic { get; set; }
        public int asianPacific { get; set; }
    }

    public class Specialregions
    {
        public Hawaii1 Hawaii { get; set; }
    }

    public class Hawaii1
    {
        public int chancePercent { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
        public string[] profileIds { get; set; }
    }

    public class Utahpolynesianinfluence1
    {
        public string state { get; set; }
        public int targetPercent { get; set; }
        public int firstNameChancePercent { get; set; }
        public string firstPool { get; set; }
        public string lastPool { get; set; }
    }

    public class Statenameflavor1
    {
        public bool enabledByDefault { get; set; }
        public string firstNameRule { get; set; }
        public string surnameRule { get; set; }
        public Statefirstnamefactors stateFirstNameFactors { get; set; }
        public Regionalsurnamerule[] regionalSurnameRules { get; set; }
    }

    public class Statefirstnamefactors
    {
        public Alaska1[] Alaska { get; set; }
        public Alabama[] Alabama { get; set; }
        public Arkansa[] Arkansas { get; set; }
        public Arizona1[] Arizona { get; set; }
        public California1[] California { get; set; }
        public Colorado1[] Colorado { get; set; }
        public Connecticut[] Connecticut { get; set; }
        public DistrictOfColumbia[] DistrictofColumbia { get; set; }
        public Delaware[] Delaware { get; set; }
        public Florida1[] Florida { get; set; }
        public Georgia[] Georgia { get; set; }
        public Hawaii2[] Hawaii { get; set; }
        public Iowa[] Iowa { get; set; }
        public Idaho[] Idaho { get; set; }
        public Illinois[] Illinois { get; set; }
        public Indiana[] Indiana { get; set; }
        public Kansa[] Kansas { get; set; }
        public Kentucky[] Kentucky { get; set; }
        public Louisiana[] Louisiana { get; set; }
        public Massachusett[] Massachusetts { get; set; }
        public Maryland[] Maryland { get; set; }
        public Maine[] Maine { get; set; }
        public Michigan[] Michigan { get; set; }
        public Minnesota[] Minnesota { get; set; }
        public Missouri[] Missouri { get; set; }
        public Mississippi[] Mississippi { get; set; }
        public Montana1[] Montana { get; set; }
        public NorthCarolina[] NorthCarolina { get; set; }
        public NorthDakota1[] NorthDakota { get; set; }
        public Nebraska[] Nebraska { get; set; }
        public NewHampshire[] NewHampshire { get; set; }
        public NewJersey[] NewJersey { get; set; }
        public NewMexico1[] NewMexico { get; set; }
        public Nevada1[] Nevada { get; set; }
        public NewYork[] NewYork { get; set; }
        public Ohio[] Ohio { get; set; }
        public Oklahoma1[] Oklahoma { get; set; }
        public Oregon1[] Oregon { get; set; }
        public Pennsylvania[] Pennsylvania { get; set; }
        public RhodeIsland[] RhodeIsland { get; set; }
        public SouthCarolina[] SouthCarolina { get; set; }
        public SouthDakota1[] SouthDakota { get; set; }
        public Tennessee[] Tennessee { get; set; }
        public Texa[] Texas { get; set; }
        public Utah[] Utah { get; set; }
        public Virginia[] Virginia { get; set; }
        public Vermont[] Vermont { get; set; }
        public Washington1[] Washington { get; set; }
        public Wisconsin[] Wisconsin { get; set; }
        public WestVirginia[] WestVirginia { get; set; }
        public Wyoming[] Wyoming { get; set; }
    }

    public class Alaska1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Alabama
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Arkansa
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Arizona1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class California1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Colorado1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Connecticut
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class DistrictOfColumbia
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Delaware
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Florida1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Georgia
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Hawaii2
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Iowa
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Idaho
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Illinois
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Indiana
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Kansa
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Kentucky
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Louisiana
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Massachusett
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Maryland
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Maine
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Michigan
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Minnesota
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Missouri
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Mississippi
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Montana1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NorthCarolina
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NorthDakota1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Nebraska
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NewHampshire
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NewJersey
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NewMexico1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Nevada1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class NewYork
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Ohio
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Oklahoma1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Oregon1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Pennsylvania
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class RhodeIsland
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class SouthCarolina
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class SouthDakota1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Tennessee
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Texa
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Utah
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Virginia
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Vermont
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Washington1
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Wisconsin
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class WestVirginia
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Wyoming
    {
        public string name { get; set; }
        public float factor { get; set; }
    }

    public class Regionalsurnamerule
    {
        public string id { get; set; }
        public string[] states { get; set; }
        public int chancePercent { get; set; }
        public Profilepools profilePools { get; set; }
        public string note { get; set; }
    }

    public class Profilepools
    {
        public White1[] white { get; set; }
        public Black1[] black { get; set; }
        public Multiracial1[] multiracial { get; set; }
        public Hispanic1[] hispanic { get; set; }
    }

    public class White1
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Black1
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Multiracial1
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Hispanic1
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Summary
    {
        public int censusFirstNameRows { get; set; }
        public int censusFirstNamesWithProfileLift { get; set; }
        public int censusLastNameRows { get; set; }
        public int firstFrequencyMismatchRows { get; set; }
        public int lastFrequencyMismatchRows { get; set; }
        public Firstnamecohorts2 firstNameCohorts { get; set; }
        public Ncaadivisionifootball ncaaDivisionIFootball { get; set; }
        public Pools pools { get; set; }
        public Statenameflavor2 stateNameFlavor { get; set; }
    }

    public class Firstnamecohorts2
    {
        public int[] availableYears { get; set; }
        public int yearCount { get; set; }
        public int uniqueMaleNames { get; set; }
        public int namesWithCensusProfileData { get; set; }
        public int namesWithoutCensusProfileData { get; set; }
        public Fallback fallback { get; set; }
        public Annual annual { get; set; }
        public int excludedArtifactBirthCount { get; set; }
        public int excludedArtifactRowCount { get; set; }
    }

    public class Fallback
    {
        public int centerYear { get; set; }
        public int windowRadius { get; set; }
        public int[] years { get; set; }
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools1 profilePools { get; set; }
    }

    public class Profilepools1
    {
        public White2 white { get; set; }
        public Black2 black { get; set; }
        public Americanindianalaskanative1 americanIndianAlaskaNative { get; set; }
        public Asianpacific1 asianPacific { get; set; }
        public Multiracial2 multiracial { get; set; }
        public Hispanic2 hispanic { get; set; }
    }

    public class White2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative1
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific1
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Annual
    {
        public _20002 _2000 { get; set; }
        public _20012 _2001 { get; set; }
        public _20022 _2002 { get; set; }
        public _20032 _2003 { get; set; }
        public _20042 _2004 { get; set; }
        public _20052 _2005 { get; set; }
        public _20062 _2006 { get; set; }
        public _20072 _2007 { get; set; }
        public _20082 _2008 { get; set; }
        public _20092 _2009 { get; set; }
        public _20102 _2010 { get; set; }
        public _20112 _2011 { get; set; }
        public _20122 _2012 { get; set; }
        public _20132 _2013 { get; set; }
        public _20142 _2014 { get; set; }
        public _20152 _2015 { get; set; }
        public _20162 _2016 { get; set; }
        public _20172 _2017 { get; set; }
        public _20182 _2018 { get; set; }
        public _20192 _2019 { get; set; }
        public _20202 _2020 { get; set; }
        public _20211 _2021 { get; set; }
        public _20221 _2022 { get; set; }
        public _20231 _2023 { get; set; }
        public _20241 _2024 { get; set; }
    }

    public class _20002
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools2 profilePools { get; set; }
    }

    public class Profilepools2
    {
        public White3 white { get; set; }
        public Black3 black { get; set; }
        public Americanindianalaskanative2 americanIndianAlaskaNative { get; set; }
        public Asianpacific2 asianPacific { get; set; }
        public Multiracial3 multiracial { get; set; }
        public Hispanic3 hispanic { get; set; }
    }

    public class White3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific2
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20012
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools3 profilePools { get; set; }
    }

    public class Profilepools3
    {
        public White4 white { get; set; }
        public Black4 black { get; set; }
        public Americanindianalaskanative3 americanIndianAlaskaNative { get; set; }
        public Asianpacific3 asianPacific { get; set; }
        public Multiracial4 multiracial { get; set; }
        public Hispanic4 hispanic { get; set; }
    }

    public class White4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific3
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20022
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools4 profilePools { get; set; }
    }

    public class Profilepools4
    {
        public White5 white { get; set; }
        public Black5 black { get; set; }
        public Americanindianalaskanative4 americanIndianAlaskaNative { get; set; }
        public Asianpacific4 asianPacific { get; set; }
        public Multiracial5 multiracial { get; set; }
        public Hispanic5 hispanic { get; set; }
    }

    public class White5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific4
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20032
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools5 profilePools { get; set; }
    }

    public class Profilepools5
    {
        public White6 white { get; set; }
        public Black6 black { get; set; }
        public Americanindianalaskanative5 americanIndianAlaskaNative { get; set; }
        public Asianpacific5 asianPacific { get; set; }
        public Multiracial6 multiracial { get; set; }
        public Hispanic6 hispanic { get; set; }
    }

    public class White6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific5
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20042
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools6 profilePools { get; set; }
    }

    public class Profilepools6
    {
        public White7 white { get; set; }
        public Black7 black { get; set; }
        public Americanindianalaskanative6 americanIndianAlaskaNative { get; set; }
        public Asianpacific6 asianPacific { get; set; }
        public Multiracial7 multiracial { get; set; }
        public Hispanic7 hispanic { get; set; }
    }

    public class White7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific6
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20052
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools7 profilePools { get; set; }
    }

    public class Profilepools7
    {
        public White8 white { get; set; }
        public Black8 black { get; set; }
        public Americanindianalaskanative7 americanIndianAlaskaNative { get; set; }
        public Asianpacific7 asianPacific { get; set; }
        public Multiracial8 multiracial { get; set; }
        public Hispanic8 hispanic { get; set; }
    }

    public class White8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific7
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20062
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools8 profilePools { get; set; }
    }

    public class Profilepools8
    {
        public White9 white { get; set; }
        public Black9 black { get; set; }
        public Americanindianalaskanative8 americanIndianAlaskaNative { get; set; }
        public Asianpacific8 asianPacific { get; set; }
        public Multiracial9 multiracial { get; set; }
        public Hispanic9 hispanic { get; set; }
    }

    public class White9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific8
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20072
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools9 profilePools { get; set; }
    }

    public class Profilepools9
    {
        public White10 white { get; set; }
        public Black10 black { get; set; }
        public Americanindianalaskanative9 americanIndianAlaskaNative { get; set; }
        public Asianpacific9 asianPacific { get; set; }
        public Multiracial10 multiracial { get; set; }
        public Hispanic10 hispanic { get; set; }
    }

    public class White10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific9
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20082
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools10 profilePools { get; set; }
    }

    public class Profilepools10
    {
        public White11 white { get; set; }
        public Black11 black { get; set; }
        public Americanindianalaskanative10 americanIndianAlaskaNative { get; set; }
        public Asianpacific10 asianPacific { get; set; }
        public Multiracial11 multiracial { get; set; }
        public Hispanic11 hispanic { get; set; }
    }

    public class White11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific10
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20092
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools11 profilePools { get; set; }
    }

    public class Profilepools11
    {
        public White12 white { get; set; }
        public Black12 black { get; set; }
        public Americanindianalaskanative11 americanIndianAlaskaNative { get; set; }
        public Asianpacific11 asianPacific { get; set; }
        public Multiracial12 multiracial { get; set; }
        public Hispanic12 hispanic { get; set; }
    }

    public class White12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific11
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20102
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools12 profilePools { get; set; }
    }

    public class Profilepools12
    {
        public White13 white { get; set; }
        public Black13 black { get; set; }
        public Americanindianalaskanative12 americanIndianAlaskaNative { get; set; }
        public Asianpacific12 asianPacific { get; set; }
        public Multiracial13 multiracial { get; set; }
        public Hispanic13 hispanic { get; set; }
    }

    public class White13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific12
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20112
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools13 profilePools { get; set; }
    }

    public class Profilepools13
    {
        public White14 white { get; set; }
        public Black14 black { get; set; }
        public Americanindianalaskanative13 americanIndianAlaskaNative { get; set; }
        public Asianpacific13 asianPacific { get; set; }
        public Multiracial14 multiracial { get; set; }
        public Hispanic14 hispanic { get; set; }
    }

    public class White14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific13
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20122
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools14 profilePools { get; set; }
    }

    public class Profilepools14
    {
        public White15 white { get; set; }
        public Black15 black { get; set; }
        public Americanindianalaskanative14 americanIndianAlaskaNative { get; set; }
        public Asianpacific14 asianPacific { get; set; }
        public Multiracial15 multiracial { get; set; }
        public Hispanic15 hispanic { get; set; }
    }

    public class White15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific14
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20132
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools15 profilePools { get; set; }
    }

    public class Profilepools15
    {
        public White16 white { get; set; }
        public Black16 black { get; set; }
        public Americanindianalaskanative15 americanIndianAlaskaNative { get; set; }
        public Asianpacific15 asianPacific { get; set; }
        public Multiracial16 multiracial { get; set; }
        public Hispanic16 hispanic { get; set; }
    }

    public class White16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific15
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20142
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools16 profilePools { get; set; }
    }

    public class Profilepools16
    {
        public White17 white { get; set; }
        public Black17 black { get; set; }
        public Americanindianalaskanative16 americanIndianAlaskaNative { get; set; }
        public Asianpacific16 asianPacific { get; set; }
        public Multiracial17 multiracial { get; set; }
        public Hispanic17 hispanic { get; set; }
    }

    public class White17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific16
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20152
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools17 profilePools { get; set; }
    }

    public class Profilepools17
    {
        public White18 white { get; set; }
        public Black18 black { get; set; }
        public Americanindianalaskanative17 americanIndianAlaskaNative { get; set; }
        public Asianpacific17 asianPacific { get; set; }
        public Multiracial18 multiracial { get; set; }
        public Hispanic18 hispanic { get; set; }
    }

    public class White18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific17
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20162
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools18 profilePools { get; set; }
    }

    public class Profilepools18
    {
        public White19 white { get; set; }
        public Black19 black { get; set; }
        public Americanindianalaskanative18 americanIndianAlaskaNative { get; set; }
        public Asianpacific18 asianPacific { get; set; }
        public Multiracial19 multiracial { get; set; }
        public Hispanic19 hispanic { get; set; }
    }

    public class White19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific18
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20172
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools19 profilePools { get; set; }
    }

    public class Profilepools19
    {
        public White20 white { get; set; }
        public Black20 black { get; set; }
        public Americanindianalaskanative19 americanIndianAlaskaNative { get; set; }
        public Asianpacific19 asianPacific { get; set; }
        public Multiracial20 multiracial { get; set; }
        public Hispanic20 hispanic { get; set; }
    }

    public class White20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific19
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20182
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools20 profilePools { get; set; }
    }

    public class Profilepools20
    {
        public White21 white { get; set; }
        public Black21 black { get; set; }
        public Americanindianalaskanative20 americanIndianAlaskaNative { get; set; }
        public Asianpacific20 asianPacific { get; set; }
        public Multiracial21 multiracial { get; set; }
        public Hispanic21 hispanic { get; set; }
    }

    public class White21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific20
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20192
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools21 profilePools { get; set; }
    }

    public class Profilepools21
    {
        public White22 white { get; set; }
        public Black22 black { get; set; }
        public Americanindianalaskanative21 americanIndianAlaskaNative { get; set; }
        public Asianpacific21 asianPacific { get; set; }
        public Multiracial22 multiracial { get; set; }
        public Hispanic22 hispanic { get; set; }
    }

    public class White22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific21
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20202
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools22 profilePools { get; set; }
    }

    public class Profilepools22
    {
        public White23 white { get; set; }
        public Black23 black { get; set; }
        public Americanindianalaskanative22 americanIndianAlaskaNative { get; set; }
        public Asianpacific22 asianPacific { get; set; }
        public Multiracial23 multiracial { get; set; }
        public Hispanic23 hispanic { get; set; }
    }

    public class White23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific22
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20211
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools23 profilePools { get; set; }
    }

    public class Profilepools23
    {
        public White24 white { get; set; }
        public Black24 black { get; set; }
        public Americanindianalaskanative23 americanIndianAlaskaNative { get; set; }
        public Asianpacific23 asianPacific { get; set; }
        public Multiracial24 multiracial { get; set; }
        public Hispanic24 hispanic { get; set; }
    }

    public class White24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific23
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20221
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools24 profilePools { get; set; }
    }

    public class Profilepools24
    {
        public White25 white { get; set; }
        public Black25 black { get; set; }
        public Americanindianalaskanative24 americanIndianAlaskaNative { get; set; }
        public Asianpacific24 asianPacific { get; set; }
        public Multiracial25 multiracial { get; set; }
        public Hispanic25 hispanic { get; set; }
    }

    public class White25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific24
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20231
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools25 profilePools { get; set; }
    }

    public class Profilepools25
    {
        public White26 white { get; set; }
        public Black26 black { get; set; }
        public Americanindianalaskanative25 americanIndianAlaskaNative { get; set; }
        public Asianpacific25 asianPacific { get; set; }
        public Multiracial26 multiracial { get; set; }
        public Hispanic26 hispanic { get; set; }
    }

    public class White26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific25
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class _20241
    {
        public int maleBirthCount { get; set; }
        public int nameCount { get; set; }
        public Profilepools26 profilePools { get; set; }
    }

    public class Profilepools26
    {
        public White27 white { get; set; }
        public Black27 black { get; set; }
        public Americanindianalaskanative26 americanIndianAlaskaNative { get; set; }
        public Asianpacific26 asianPacific { get; set; }
        public Multiracial27 multiracial { get; set; }
        public Hispanic27 hispanic { get; set; }
    }

    public class White27
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Black27
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanative26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacific26
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracial27
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanic27
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Ncaadivisionifootball
    {
        public Categorycounts1 categoryCounts { get; set; }
        public Profilecounts profileCounts { get; set; }
        public int knownProfileTotal { get; set; }
        public int allCategoryTotal { get; set; }
        public Excludedcategorycounts1 excludedCategoryCounts { get; set; }
    }

    public class Categorycounts1
    {
        public int AmericanIndianAlaskaNative { get; set; }
        public int Asian { get; set; }
        public int Black { get; set; }
        public int HispanicLatino { get; set; }
        public int International { get; set; }
        public int NativeHawaiianPacificIslander { get; set; }
        public int TwoorMoreRaces { get; set; }
        public int Unknown { get; set; }
        public int White { get; set; }
    }

    public class Profilecounts
    {
        public int white { get; set; }
        public int black { get; set; }
        public int americanIndianAlaskaNative { get; set; }
        public int asianPacific { get; set; }
        public int multiracial { get; set; }
        public int hispanic { get; set; }
    }

    public class Excludedcategorycounts1
    {
        public int International { get; set; }
        public int Unknown { get; set; }
    }

    public class Pools
    {
        public Whitefirstnames WhiteFirstNames { get; set; }
        public Blackfirstnames BlackFirstNames { get; set; }
        public Americanindianalaskanativefirstnames AmericanIndianAlaskaNativeFirstNames { get; set; }
        public Asianpacificfirstnames AsianPacificFirstNames { get; set; }
        public Multiracialfirstnames MultiracialFirstNames { get; set; }
        public Hispanicfirstnames HispanicFirstNames { get; set; }
        public Whitelastnames WhiteLastNames { get; set; }
        public Blacklastnames BlackLastNames { get; set; }
        public Americanindianalaskanativelastnames AmericanIndianAlaskaNativeLastNames { get; set; }
        public Asianpacificlastnames AsianPacificLastNames { get; set; }
        public Multiraciallastnames MultiracialLastNames { get; set; }
        public Hispaniclastnames HispanicLastNames { get; set; }
        public Hawaiianfirstnames HawaiianFirstNames { get; set; }
        public Hawaiianlastnames HawaiianLastNames { get; set; }
        public Cajunnames CajunNames { get; set; }
        public Utahpolynesianfirstnames UtahPolynesianFirstNames { get; set; }
        public Utahpolynesianlastnames UtahPolynesianLastNames { get; set; }
    }

    public class Whitefirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Blackfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanativefirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacificfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiracialfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispanicfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Whitelastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Blacklastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Americanindianalaskanativelastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Asianpacificlastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Multiraciallastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hispaniclastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hawaiianfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Hawaiianlastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Cajunnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Utahpolynesianfirstnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Utahpolynesianlastnames
    {
        public int count { get; set; }
        public int totalWeight { get; set; }
    }

    public class Statenameflavor2
    {
        public Statefirstnamefactors1 stateFirstNameFactors { get; set; }
        public int stateFirstNameEntryCount { get; set; }
        public int regionalSurnameRuleCount { get; set; }
    }

    public class Statefirstnamefactors1
    {
        public Alaska2 Alaska { get; set; }
        public Alabama1 Alabama { get; set; }
        public Arkansas Arkansas { get; set; }
        public Arizona2 Arizona { get; set; }
        public California2 California { get; set; }
        public Colorado2 Colorado { get; set; }
        public Connecticut1 Connecticut { get; set; }
        public DistrictOfColumbia1 DistrictofColumbia { get; set; }
        public Delaware1 Delaware { get; set; }
        public Florida2 Florida { get; set; }
        public Georgia1 Georgia { get; set; }
        public Hawaii3 Hawaii { get; set; }
        public Iowa1 Iowa { get; set; }
        public Idaho1 Idaho { get; set; }
        public Illinois1 Illinois { get; set; }
        public Indiana1 Indiana { get; set; }
        public Kansas Kansas { get; set; }
        public Kentucky1 Kentucky { get; set; }
        public Louisiana1 Louisiana { get; set; }
        public Massachusetts Massachusetts { get; set; }
        public Maryland1 Maryland { get; set; }
        public Maine1 Maine { get; set; }
        public Michigan1 Michigan { get; set; }
        public Minnesota1 Minnesota { get; set; }
        public Missouri1 Missouri { get; set; }
        public Mississippi1 Mississippi { get; set; }
        public Montana2 Montana { get; set; }
        public NorthCarolina1 NorthCarolina { get; set; }
        public NorthDakota2 NorthDakota { get; set; }
        public Nebraska1 Nebraska { get; set; }
        public NewHampshire1 NewHampshire { get; set; }
        public NewJersey1 NewJersey { get; set; }
        public NewMexico2 NewMexico { get; set; }
        public Nevada2 Nevada { get; set; }
        public NewYork1 NewYork { get; set; }
        public Ohio1 Ohio { get; set; }
        public Oklahoma2 Oklahoma { get; set; }
        public Oregon2 Oregon { get; set; }
        public Pennsylvania1 Pennsylvania { get; set; }
        public RhodeIsland1 RhodeIsland { get; set; }
        public SouthCarolina1 SouthCarolina { get; set; }
        public SouthDakota2 SouthDakota { get; set; }
        public Tennessee1 Tennessee { get; set; }
        public Texas1 Texas { get; set; }
        public Utah1 Utah { get; set; }
        public Virginia1 Virginia { get; set; }
        public Vermont1 Vermont { get; set; }
        public Washington2 Washington { get; set; }
        public Wisconsin1 Wisconsin { get; set; }
        public WestVirginia1 WestVirginia { get; set; }
        public Wyoming1 Wyoming { get; set; }
    }

    public class Alaska2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Alabama1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Arkansas
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Arizona2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class California2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Colorado2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Connecticut1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class DistrictOfColumbia1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Delaware1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Florida2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Georgia1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Hawaii3
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Iowa1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Idaho1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Illinois1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Indiana1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Kansas
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Kentucky1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Louisiana1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Massachusetts
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Maryland1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Maine1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Michigan1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Minnesota1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Missouri1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Mississippi1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Montana2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NorthCarolina1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NorthDakota2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Nebraska1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NewHampshire1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NewJersey1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NewMexico2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Nevada2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class NewYork1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Ohio1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Oklahoma2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Oregon2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Pennsylvania1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class RhodeIsland1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class SouthCarolina1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class SouthDakota2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Tennessee1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Texas1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Utah1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Virginia1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Vermont1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Washington2
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Wisconsin1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class WestVirginia1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Wyoming1
    {
        public int count { get; set; }
        public int boostedCount { get; set; }
        public int reducedCount { get; set; }
        public float minimumFactor { get; set; }
        public float maximumFactor { get; set; }
        public int selectedMaleBirthCount { get; set; }
    }

    public class Whitefirstname: IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Blackfirstname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Americanindianalaskanativefirstname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Asianpacificfirstname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Multiracialfirstname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Hispanicfirstname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Whitelastname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Blacklastname: IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Americanindianalaskanativelastname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Asianpacificlastname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Multiraciallastname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Hispaniclastname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Hawaiianfirstname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Hawaiianlastname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Cajunname : IName
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Utahpolynesianfirstname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

    public class Utahpolynesianlastname
    {
        public string name { get; set; }
        public int weight { get; set; }
    }

}
