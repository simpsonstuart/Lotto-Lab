using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Xml;
using System.IO.IsolatedStorage;

namespace MyLottoLabWinPhone8
{
    public enum RegionTypeEnum
    {
        MULTISTATE=1
        , STATE=2
    }

    public class LotteryGameRec
    {
        public string GameName { get; set; }
        public string FileName { get; set; }
        public RegionTypeEnum RegionType { get; set; }
        public int LottoBallRegUB { get; set; }
        public int LottoBallSpecialUB { get; set; }
        public int LottoBallRegCount { get; set; }
        public List<int> LottoBallRegOrderingH2L 
        {
            get { return GetLottoBallRegOrderingH2L(); }
        }
        public List<int> LottoBallSpecialOrderingH2L
        {
            get { return GetLottoBallSpecialOrderingH2L(); }
        }
        public string LottoBallRegOrderingH2LStr { get; set; }
        public String LottoBallSpecialOrderingH2LStr { get; set; }
        public bool UseSpecialBall { get; set; }

        private List<int> GetLottoBallRegOrderingH2L()
        {
            List<int> oLottoBallRegOrderingH2L = new List<int>();
            try
            {
                string[] iaRegH2LNum = this.LottoBallRegOrderingH2LStr.Split(',');
                for (int iCount = 0; iCount < iaRegH2LNum.Count(); iCount++)
                {
                    int iResult = 0;
                    if (int.TryParse(iaRegH2LNum[iCount], out iResult))
                    {
                        oLottoBallRegOrderingH2L.Add(Convert.ToInt32(iaRegH2LNum[iCount]));
                    }
                }
            }
            catch { }

            return oLottoBallRegOrderingH2L;
        }

        public int LottoBallRegOrderingH2LCount()
        {
            string[] iaRegH2LNum = this.LottoBallRegOrderingH2LStr.Split(',');
            int iLottoBallRegOrderingH2LCount = iaRegH2LNum.Count();
            return iLottoBallRegOrderingH2LCount;
        }

        private List<int> GetLottoBallSpecialOrderingH2L()
        {
            List<int> oLottoBallSpecialOrderingH2L = new List<int>();
            try
            {
                string[] iaSpecialH2LNum = this.LottoBallSpecialOrderingH2LStr.Split(',');
                for (int iCount = 0; iCount < iaSpecialH2LNum.Count(); iCount++)
                {
                    int iResult = 0;
                    if (int.TryParse(iaSpecialH2LNum[iCount], out iResult))
                    {
                        oLottoBallSpecialOrderingH2L.Add(Convert.ToInt32(iaSpecialH2LNum[iCount]));
                    }
                }
            }
            catch { }
            return oLottoBallSpecialOrderingH2L;
        }

        public int LottoBallSpecialOrderingH2LCount()
        {
            string[] iaSpecialH2LNum = this.LottoBallSpecialOrderingH2LStr.Split(',');
            int iLottoBallSpecialOrderingH2LCount = iaSpecialH2LNum.Count();

            return iLottoBallSpecialOrderingH2LCount;
        }
    }

    public class LotteryGameList
    {
        public Dictionary<string, LotteryGameRec> RegionLottoGameDict { get; set; }
        public List<MultiRegionLottoGameMapping> MultiRegionLottoGameMapList { get; set; } 
    }

    public class InitConfiguration
    {
        public string InitRegion { get; set; }
        public string InitLottoGame { get; set; }
        public int InitPercentPopularLeast { get; set; }    
        public int InitPercentPopularMost { get; set; }
        public int InitPercentThowoutBall { get; set; }
        public double InitCountThowout { get; set; }
    }

    public class MultiRegionLottoGameMapping
    {
        public string GameName { get; set; }
        public string GameFileName{ get; set; }
    }

    public class LottoRegionMapping
    {
        public string Region { get; set; }
        public string FileName { get; set; }
    }
    
    /***************************************************************************************
     * Quick Pick Alogorithm
     * ************************************************************************************/
    public static class LottoAlgorithm
    {
        public static Dictionary<int, int> QuickPick(LotteryGameRec inout_oLotteryGameRec)
        {
            Dictionary<int, int> oBalls = new Dictionary<int, int>();
            Random oRand = new Random();

            try
            {
                for (int iCounter = 0; iCounter < inout_oLotteryGameRec.LottoBallRegCount; iCounter++)
                {
                    int iNewRegularBall;
                    int iRetryCounter = 0;

                    iNewRegularBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);

                    // Retry when ball exists
                    while (oBalls.Values.Contains(iNewRegularBall))
                    {
                        iNewRegularBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);
                        iRetryCounter++;

                        // Short Circuit in case of run away process
                        if (iRetryCounter > (inout_oLotteryGameRec.LottoBallRegUB * 5))
                        {
                            iNewRegularBall = 0;
                            break;
                        }
                    }
                    oBalls.Add(iCounter + 1, iNewRegularBall);
                }

                // Special Ball Logic
                if (inout_oLotteryGameRec.UseSpecialBall)
                {
                    int iNewSpecialBall;

                    iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);
                    oBalls.Add(oBalls.Count + 1, iNewSpecialBall);
                }
            }
            catch
            { 
            }

            return oBalls;
        }

        /***************************************************************************************
         * Through out by Pick Alogorithm
         * ************************************************************************************/
        public static Dictionary<int, int> ThrowoutByPick(LotteryGameRec inout_oLotteryGameRec
                , double in_iThowoutMax)
        {
            Dictionary<int, int> oBalls = null;
            Random oRand = new Random();

            try
            {
                for (int iThrowoutCount = 0; iThrowoutCount < in_iThowoutMax; iThrowoutCount++)
                {
                    // Reinitialize the dictionary
                    oBalls = null;
                    oBalls = new Dictionary<int, int>();

                    for (int iCounter = 0; iCounter < inout_oLotteryGameRec.LottoBallRegCount; iCounter++)
                    {
                        int iNewBall;
                        int iRetryCounter = 0;

                        iNewBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);

                        // Retry when ball exists
                        while (oBalls.Values.Contains(iNewBall))
                        {
                            iNewBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);
                            iRetryCounter++;

                            // Short Circuit in case of run away process
                            if (iRetryCounter > (inout_oLotteryGameRec.LottoBallRegUB * 5))
                            {
                                iNewBall = 0;
                                break;
                            }
                        }

                        oBalls.Add(iCounter + 1, iNewBall);
                    }

                    // Special Ball Logic
                    if (inout_oLotteryGameRec.UseSpecialBall)
                    {
                        int iNewSpecialBall;

                        iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);
                        oBalls.Add(oBalls.Count + 1, iNewSpecialBall);
                    }
                }
            }
            catch 
            { 
            }
            return oBalls;
        }

        /***************************************************************************************
         * Through out by Ball Alogorithm
         * ************************************************************************************/
        public static Dictionary<int, int> ThrowoutByBall(LotteryGameRec inout_oLotteryGameRec
            , int in_iThowoutByBallPercent)
        {
            Dictionary<int, int> oBalls = null;
            Random oRand = new Random();

            // Reinitialize the dictionary
            oBalls = null;
            oBalls = new Dictionary<int, int>();

            try
            {
                for (int iCounter = 0; iCounter < inout_oLotteryGameRec.LottoBallRegCount; iCounter++)
                {
                    int iNewBall = 0;
                    int iRetryCounter = 0;

                    int iRegBallsToThrowout = (inout_oLotteryGameRec.LottoBallRegCount * in_iThowoutByBallPercent) / 100;

                    for (int iThrowoutCounter = 0; iThrowoutCounter < iRegBallsToThrowout; iThrowoutCounter++)
                    {
                        iNewBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);

                        // Retry when ball exists
                        while (oBalls.Values.Contains(iNewBall))
                        {
                            iNewBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);
                            iRetryCounter++;

                            // Short Circuit in case of run away process
                            if (iRetryCounter > (inout_oLotteryGameRec.LottoBallRegUB * 500))
                            {
                                iNewBall = 0;
                                break;
                            }
                        }
                    }
                    oBalls.Add(iCounter + 1, iNewBall);
                }

                // Special Ball Logic
                if (inout_oLotteryGameRec.UseSpecialBall)
                {
                    int iNewSpecialBall = 0;
                    int iSpecialBallsToThrowout = 20;

                    iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);

                    for (int iThrowoutCounter = 0; iThrowoutCounter < iSpecialBallsToThrowout; iThrowoutCounter++)
                    {
                        iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);
                    }

                    oBalls.Add(oBalls.Count + 1, iNewSpecialBall);
                }
            }
            catch 
            { 
            }

            return oBalls;
        }

        /***************************************************************************************
         * Pick by Most Popular Balls Alogorithm
         * ************************************************************************************/
        public static Dictionary<int, int> Popular(LotteryGameRec inout_oLotteryGameRec
            , bool in_bPosDirection
            , int in_iPopularPercent)
        {
            Dictionary<int, int> oBalls = new Dictionary<int, int>();
            Random oRand = new Random();

            try
            {

                //  Build List of popular numbers for regular balls
                List<int> oPopularReg = new List<int>();
                int iPopularCountReg = (inout_oLotteryGameRec.LottoBallRegOrderingH2L.Count() * in_iPopularPercent) / 100;

                if (in_bPosDirection)
                {
                    for (int iCount = 0; iCount < iPopularCountReg; iCount++)
                    {
                        oPopularReg.Add(inout_oLotteryGameRec.LottoBallRegOrderingH2L[iCount]);
                    }
                }
                else
                {
                    int iRevPopularCount = inout_oLotteryGameRec.LottoBallRegOrderingH2L.Count() - iPopularCountReg;
                    for (int iCount = inout_oLotteryGameRec.LottoBallRegOrderingH2L.Count() - 1; iCount > iRevPopularCount; iCount--)
                    {
                        oPopularReg.Add(inout_oLotteryGameRec.LottoBallRegOrderingH2L[iCount]);
                    }
                }

                //  Build List of popular numbers for Special Balls
                List<int> oPopularSpecial = new List<int>();
                int iPopularCountSpecial = (inout_oLotteryGameRec.LottoBallSpecialOrderingH2L.Count() * in_iPopularPercent) / 100;

                if (in_bPosDirection)
                {
                    for (int iCount = 0; iCount < iPopularCountSpecial; iCount++)
                    {
                        oPopularSpecial.Add(inout_oLotteryGameRec.LottoBallSpecialOrderingH2L[iCount]);
                    }
                }
                else
                {
                    int iRevPopularCount = inout_oLotteryGameRec.LottoBallSpecialOrderingH2L.Count() - iPopularCountSpecial - 1;
                    for (int iCount = inout_oLotteryGameRec.LottoBallSpecialOrderingH2L.Count() - 1; iCount > iRevPopularCount; iCount--)
                    {
                        oPopularSpecial.Add(inout_oLotteryGameRec.LottoBallSpecialOrderingH2L[iCount]);
                    }
                }

                // Get List of regular balls
                for (int iCounter = 0; iCounter < inout_oLotteryGameRec.LottoBallRegCount; iCounter++)
                {
                    int iNewRegularBall;
                    int iRetryCounter = 0;

                    iNewRegularBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);
                    bool bIsPopular = oPopularReg.Contains(iNewRegularBall);

                    // Retry when ball exists in list of balls
                    while (oBalls.Values.Contains(iNewRegularBall) || !bIsPopular)
                    {
                        iNewRegularBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallRegUB);
                        bIsPopular = oPopularReg.Contains(iNewRegularBall);
                        iRetryCounter++;

                        // Short Circuit in case of run away process
                        if (iRetryCounter > (inout_oLotteryGameRec.LottoBallRegUB * 5))
                        {
                            iNewRegularBall = 0;
                            break;
                        }
                    }
                    oBalls.Add(iCounter + 1, iNewRegularBall);
                }

                // Special Ball Logic
                if (inout_oLotteryGameRec.UseSpecialBall)
                {
                    int iNewSpecialBall;
                    int iRetryCounter = 0;

                    iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);
                    bool bIsPopular = oPopularSpecial.Contains(iNewSpecialBall);

                    // Retry when ball exists in list of balls
                    while (!bIsPopular)
                    {
                        iNewSpecialBall = oRand.Next(1, inout_oLotteryGameRec.LottoBallSpecialUB);
                        bIsPopular = oPopularSpecial.Contains(iNewSpecialBall);
                        iRetryCounter++;

                        // Short Circuit in case of run away process
                        if (iRetryCounter > (inout_oLotteryGameRec.LottoBallSpecialUB * 5))
                        {
                            iNewSpecialBall = 0;
                            break;
                        }
                    }

                    oBalls.Add(oBalls.Count + 1, iNewSpecialBall);
                }
            }
            catch
            {
            }

            return oBalls;
        }

        /***************************************************************************************
         * Get Lottery Game Config File name
         *************************************************************************************/
        public static InitConfiguration GetInitConfiguration()
        {
            InitConfiguration oInitConfiguration
                = new InitConfiguration();

            try
            {
                // Read File from isolated storage
                IsolatedStorageFile oIsolatedStorageFile
                    = IsolatedStorageFile.GetUserStoreForApplication();

                string sPathAndFilename = @"GameConfig\DefaultConfig.xml";
                string sDirectory = "GameConfig";

                // Create Directory If Necessary
                if (!oIsolatedStorageFile.DirectoryExists(sDirectory))
                {
                    oIsolatedStorageFile.CreateDirectory(sDirectory);
                }

                // re-create file in isolated storage if necessary
                if (!oIsolatedStorageFile.FileExists(sPathAndFilename))
                {
                    // Copy file from app folder to isolated storage
                    XDocument oXDocTemp
                        = XDocument.Load(sPathAndFilename);

                    XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                    oWriterSettings.Indent = true;

                    using (IsolatedStorageFileStream oSourceStream
                        = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Create, oIsolatedStorageFile))
                    {
                        using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                        {
                            oXDocTemp.WriteTo(oWriter);
                            // Write the XML to the file.
                            oWriter.Flush();
                        }
                    } 
                }

                XDocument oXDocument = null;
                using (IsolatedStorageFileStream oIsoStream
                    = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Open, oIsolatedStorageFile))
                {
                    oXDocument
                        = XDocument.Load(oIsoStream);
                }

                try
                {
                    IEnumerable<XElement> oLotteryGameConfigRecords
                        = oXDocument.Descendants("DefaultConfig");

                    // Copy Records read from XML File into Record Table based on XML Schema
                    // Add Call Data records
                    foreach (XElement oRecord in oLotteryGameConfigRecords)
                    {
                        // Get record elements
                        IEnumerable<XElement> oRecordElements = oRecord.Elements();

                        foreach (XElement oElement in oRecordElements)
                        {
                            switch (oElement.Name.ToString())
                            {
                                case "Region":
                                    oInitConfiguration.InitRegion
                                        = oElement.Value;
                                    break;
                                case "Game":
                                    oInitConfiguration.InitLottoGame
                                        = oElement.Value;
                                    break;
                                case "PercentPopularLeast":
                                    oInitConfiguration.InitPercentPopularLeast
                                        = Convert.ToInt32(oElement.Value);
                                    break;
                                case "PercentPopularMost":
                                    oInitConfiguration.InitPercentPopularMost
                                        = Convert.ToInt32(oElement.Value);
                                    break;
                                case "CountThowout":
                                    oInitConfiguration.InitCountThowout
                                        = Convert.ToInt32(oElement.Value);
                                    break;
                                case "PercentThowoutBall":
                                    oInitConfiguration.InitPercentThowoutBall
                                        = Convert.ToInt32(oElement.Value);
                                    break;
                                default:
                                    break;
                            } // switch
                        } // foreach
                    } // foreach
                }
                catch { }
            }
            catch (Exception oEx)
            {
                string sErrorMessage = oEx.Message;
            }

            return oInitConfiguration;
        }


        /***************************************************************************************
         * Get Lottery Game Config File name
         *************************************************************************************/
        public static Dictionary<string, LottoRegionMapping> GetLotteryGameConfigFiles()
        {
            Dictionary<string, LottoRegionMapping> oGameConfigsDict
                = new Dictionary<string, LottoRegionMapping>();

            try
            {
                // Read File from isolated storage
                IsolatedStorageFile oIsolatedStorageFile
                    = IsolatedStorageFile.GetUserStoreForApplication();

                string sPathAndFilename = @"GameConfig\LotteryGameConfigFiles.xml";
                string sDirectory = "GameConfig";

                // Create Directory If Necessary
                if (!oIsolatedStorageFile.DirectoryExists(sDirectory))
                {
                    oIsolatedStorageFile.CreateDirectory(sDirectory);
                }

                // re-create file in isolated storage if necessary
                if (!oIsolatedStorageFile.FileExists(sPathAndFilename))
                {
                    // Copy file from app folder to isolated storage
                    XDocument oXDocTemp
                        = XDocument.Load(sPathAndFilename);

                    XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                    oWriterSettings.Indent = true;

                    using (IsolatedStorageFileStream oSourceStream
                        = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Create, oIsolatedStorageFile))
                    {
                        using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                        {
                            oXDocTemp.WriteTo(oWriter);
                            // Write the XML to the file.
                            oWriter.Flush();
                        }
                    }
                }

                XDocument oXDocument = null;
                using (IsolatedStorageFileStream oIsoStream
                    = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Open, oIsolatedStorageFile))
                {
                    oXDocument
                        = XDocument.Load(oIsoStream);
                }

                try
                {
                    IEnumerable<XElement> oLotteryGameConfigRecords
                        = oXDocument.Descendants("LotteryGameConfigRecord");

                    // Copy Records read from XML File into Record Table based on XML Schema
                    // Add Call Data records
                    foreach (XElement oRecord in oLotteryGameConfigRecords)
                    {
                        string sRegionName = "";
                        string sFileName = "";

                        // Get record elements
                        IEnumerable<XAttribute> oRecordAttributes = oRecord.Attributes();

                        foreach (XAttribute oAttribute in oRecordAttributes)
                        {
                            switch (oAttribute.Name.ToString())
                            {
                                case "RegionName":
                                    sRegionName = oAttribute.Value;
                                    break;
                                case "FileName":
                                    sFileName = oAttribute.Value;
                                    break;
                                default:
                                    break;
                            } // switch
                        } // foreach

                        // Add new dictionary entry
                        LottoRegionMapping oLottoRegionMapping = new LottoRegionMapping();
                        oLottoRegionMapping.FileName = sFileName;
                        oLottoRegionMapping.Region = sRegionName;
                        oGameConfigsDict.Add(sRegionName, oLottoRegionMapping);

                    } // foreach
                }
                catch { }
            }
            catch (Exception oEx)
            {
                string sErrorMessage = oEx.Message;
            }

            return oGameConfigsDict;
        }

        /***************************************************************************************
         * Get Lottery Games
         *************************************************************************************/
        public static LotteryGameList GetLotteryGames(string in_sGameConfigFileName)
        {
            LotteryGameList oLotteryGameList = new LotteryGameList();

            // Read File from isolated storage
            IsolatedStorageFile oIsolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication();

            string sPathAndFilename = @"GameConfig\" + in_sGameConfigFileName;
            string sDirectory = "GameConfig";

            // Create Directory If Necessary
            if (!oIsolatedStorageFile.DirectoryExists(sDirectory))
            {
                oIsolatedStorageFile.CreateDirectory(sDirectory);
            }

            // re-create file in isolated storage if necessary
            if (!oIsolatedStorageFile.FileExists(sPathAndFilename))
            {
                // Copy file from app folder to isolated storage
                XDocument oXDocTemp
                    = XDocument.Load(sPathAndFilename);

                XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                oWriterSettings.Indent = true;

                using (IsolatedStorageFileStream oSourceStream
                    = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Create, oIsolatedStorageFile))
                {
                    using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                    {
                        oXDocTemp.WriteTo(oWriter);
                        // Write the XML to the file.
                        oWriter.Flush();
                    }
                }
            }

            XDocument oXDocument = null;
            using (IsolatedStorageFileStream oIsoStream
                = new IsolatedStorageFileStream(sPathAndFilename, FileMode.Open, oIsolatedStorageFile))
            {
                oXDocument
                    = XDocument.Load(oIsoStream);
            }

            try
            {
                Dictionary<string, LotteryGameRec> oLotteryGamesDict
                    = new Dictionary<string, LotteryGameRec>();

                // Add State Games
                AddLotteryGames(oLotteryGamesDict
                    , oXDocument
                    , in_sGameConfigFileName
                    , RegionTypeEnum.STATE);

                // Get List Multi-Regional Games
                List<MultiRegionLottoGameMapping> oMultiRegionLottoGameMapList
                    = GetMultiRegionLottoGameList(oXDocument);

                // Create Directory If Necessary
                if (!oIsolatedStorageFile.DirectoryExists(sDirectory))
                {
                    oIsolatedStorageFile.CreateDirectory(sDirectory);
                }

                // Add Multi-Region Lottery Games
                foreach (MultiRegionLottoGameMapping oGame in oMultiRegionLottoGameMapList)
                {
                    // Get the multistate filename
                    string sMultiStateFileName = oGame.GameFileName;
                    string sPathAndMultiStateFilename = @"GameConfig\" + sMultiStateFileName;

                    // re-create file in isolated storage if necessary - Multistate
                    if (!oIsolatedStorageFile.FileExists(sPathAndMultiStateFilename))
                    {
                        // Copy file from app folder to isolated storage
                        XDocument oXDocTemp
                            = XDocument.Load(sPathAndMultiStateFilename);

                        XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                        oWriterSettings.Indent = true;

                        using (IsolatedStorageFileStream oSourceStream
                            = new IsolatedStorageFileStream(sPathAndMultiStateFilename, FileMode.Create, oIsolatedStorageFile))
                        {
                            using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                            {
                                oXDocTemp.WriteTo(oWriter);
                                // Write the XML to the file.
                                oWriter.Flush();
                            }
                        }
                    }

                    // Open the MultiState document
                    XDocument oXDocumentMulti = null;
                    using (IsolatedStorageFileStream oIsoStream
                        = new IsolatedStorageFileStream(sPathAndMultiStateFilename, FileMode.Open, oIsolatedStorageFile))
                    {
                        oXDocumentMulti
                            = XDocument.Load(oIsoStream);
                    }

                    // Add MultiState Game to Dictionary of Games
                    AddLotteryGames(oLotteryGamesDict
                        , oXDocumentMulti
                        , sMultiStateFileName
                        , RegionTypeEnum.MULTISTATE);
                }

                oLotteryGameList.MultiRegionLottoGameMapList = oMultiRegionLottoGameMapList;
                oLotteryGameList.RegionLottoGameDict = oLotteryGamesDict;
            }
            catch { }

            return oLotteryGameList;
        }

        /***************************************************************************************
         * Add Lottery Games
         *************************************************************************************/
        private static void AddLotteryGames(Dictionary<string, LotteryGameRec> oLotteryGamesDict
            , XDocument inout_oXDocument
            , string in_sFilename
            , RegionTypeEnum in_eRegionType)
        {
            try
            {
                IEnumerable<XElement> oLotteryGameRecords
                    = inout_oXDocument.Descendants("LotteryGames");

                // Copy Records read from XML File into Record Table based on XML Schema
                // Add Call Data records
                foreach (XElement oRecord in oLotteryGameRecords)
                {
                    // Get record elements
                    IEnumerable<XElement> oRecordElements = oRecord.Elements();
                    string sGameName = "";
                    int iLottoBallRegUB = 0;
                    int iLottoBallSpecialUB = 0;
                    int iLottoBallRegCount = 0;
                    bool bUseSpecialBall = true;
                    string sLottoBallRegOrderingH2L = "";
                    string sLottoBallSpecialOrderingH2L = "";

                    foreach (XElement oElement in oRecordElements)
                    {
                        switch (oElement.Name.ToString())
                        {
                            case "GameName":
                                sGameName = oElement.Value;
                                break;
                            case "LottoBallRegUB":
                                iLottoBallRegUB = Convert.ToInt32(oElement.Value);
                                break;
                            case "LottoBallSpecialUB":
                                iLottoBallSpecialUB = Convert.ToInt32(oElement.Value);
                                break;
                            case "LottoBallRegCount":
                                iLottoBallRegCount = Convert.ToInt32(oElement.Value);
                                break;
                            case "LottoBallRegOrderingH2L":
                                sLottoBallRegOrderingH2L = oElement.Value;
                                break;
                            case "LottoBallSpecialOrderingH2L":
                                sLottoBallSpecialOrderingH2L = oElement.Value;
                                break;
                            case "UseSpecialBall":
                                bUseSpecialBall = Convert.ToBoolean(oElement.Value);
                                break;
                            default:
                                break;
                        } // switch
                    } // foreach

                    LotteryGameRec oLotteryGameRec = new LotteryGameRec();
                    oLotteryGameRec.GameName = sGameName;
                    oLotteryGameRec.FileName = in_sFilename;
                    oLotteryGameRec.RegionType = in_eRegionType;
                    oLotteryGameRec.LottoBallRegCount = iLottoBallRegCount;
                    oLotteryGameRec.LottoBallRegUB = iLottoBallRegUB;
                    oLotteryGameRec.LottoBallSpecialUB = iLottoBallSpecialUB;
                    oLotteryGameRec.UseSpecialBall = bUseSpecialBall;
                    oLotteryGameRec.LottoBallRegOrderingH2LStr = sLottoBallRegOrderingH2L;
                    oLotteryGameRec.LottoBallSpecialOrderingH2LStr = sLottoBallSpecialOrderingH2L;

                    // Add new dictionary entry
                    oLotteryGamesDict.Add(sGameName, oLotteryGameRec);

                } // foreach
            }
            catch { }
        }

        /***************************************************************************************
         * Get GetMultiRegionLottoGameList
         *************************************************************************************/
        public static List<MultiRegionLottoGameMapping> GetMultiRegionLottoGameList(XDocument inout_oXDocument)
        {
            List<MultiRegionLottoGameMapping> oMultiRegionLottoGameList
                = new List<MultiRegionLottoGameMapping>();

            try
            {

                IEnumerable<XElement> oLotteryGameConfigRecords
                    = inout_oXDocument.Descendants("MultiRegionLottoGameRec");

                // Copy Records read from XML File into Record Table based on XML Schema
                // Add Call Data records
                foreach (XElement oRecord in oLotteryGameConfigRecords)
                {
                    string sMultiGameName = "";
                    string sFileName = "";

                    // Get record elements
                    IEnumerable<XAttribute> oRecordAttributes = oRecord.Attributes();

                    foreach (XAttribute oAttribute in oRecordAttributes)
                    {
                        switch (oAttribute.Name.ToString())
                        {
                            case "GameName":
                                sMultiGameName = oAttribute.Value;
                                break;
                            case "FileName":
                                sFileName = oAttribute.Value;
                                break;
                            default:
                                break;
                        } // switch
                    } // foreach

                    // Add new dictionary entry
                    MultiRegionLottoGameMapping oMultiRegionLottoGameMapping = new MultiRegionLottoGameMapping();
                    oMultiRegionLottoGameMapping.GameName = sMultiGameName;
                    oMultiRegionLottoGameMapping.GameFileName = sFileName;

                    oMultiRegionLottoGameList.Add(oMultiRegionLottoGameMapping);

                } // foreach
            }
            catch { }

            return oMultiRegionLottoGameList;
        }

        /***************************************************************************************
         * UpdateInitConfig
         *************************************************************************************/
        public static void UpdateInitConfig(InitConfiguration inout_oInitConfiguration)
        {
            try
            {
                XDocument oDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes")
                    , new XElement
                    ("DocumentElement"
                          , new XElement("DefaultConfig"
                            , new XElement("Region", inout_oInitConfiguration.InitRegion)
                            , new XElement("Game", inout_oInitConfiguration.InitLottoGame)
                            , new XElement("PercentPopularLeast", inout_oInitConfiguration.InitPercentPopularLeast)
                            , new XElement("PercentPopularMost", inout_oInitConfiguration.InitPercentPopularMost)
                            , new XElement("CountThowout", inout_oInitConfiguration.InitCountThowout)
                            , new XElement("PercentThowoutBall", inout_oInitConfiguration.InitPercentThowoutBall)
                            )));

                XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                oWriterSettings.Indent = true;

                IsolatedStorageFile oIsolatedStorageFile
                    = IsolatedStorageFile.GetUserStoreForApplication();
                string sFileName = @"GameConfig\DefaultConfig.xml";
                using (IsolatedStorageFileStream oSourceStream 
                    = new IsolatedStorageFileStream(sFileName, FileMode.Create, oIsolatedStorageFile))
                {
                    using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                    {
                        oDoc.WriteTo(oWriter);
                        // Write the XML to the file.
                        oWriter.Flush();
                    }
                }
            }
            catch { }
        }

        /***************************************************************************************
         * UpdateInitConfig
         *************************************************************************************/
        public static void UpdateLottoGames(LotteryGameList inout_LottoGames
            , LotteryGameRec inout_CurrentLotteryGame)
        {
            try
            {
                // Update the game record
                var oLottoGame =
                    (from c in inout_LottoGames.RegionLottoGameDict.Values
                     where c.GameName == inout_CurrentLotteryGame.GameName
                     select c).First();

                oLottoGame.LottoBallRegOrderingH2LStr
                    = inout_CurrentLotteryGame.LottoBallRegOrderingH2LStr;
                oLottoGame.LottoBallSpecialOrderingH2LStr
                    = inout_CurrentLotteryGame.LottoBallSpecialOrderingH2LStr;

                // Get game file
                string sFileName = oLottoGame.FileName;

                XDocument oDoc = null;
                if (oLottoGame.RegionType == RegionTypeEnum.STATE)
                {
                    // Create State Document
                    oDoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes")
                        , new XElement
                        ("DocumentElement"
                          , from r in inout_LottoGames.RegionLottoGameDict.Values
                            where r.RegionType == RegionTypeEnum.STATE
                            select
                              new XElement("LotteryGames"
                                , new XElement("GameName", r.GameName)
                                , new XElement("LottoBallRegUB", r.LottoBallRegUB)
                                , new XElement("LottoBallSpecialUB", r.LottoBallSpecialUB)
                                , new XElement("LottoBallRegCount", r.LottoBallRegCount)
                                , new XElement("LottoBallRegOrderingH2L", r.LottoBallRegOrderingH2LStr)
                                , new XElement("LottoBallSpecialOrderingH2L", r.LottoBallSpecialOrderingH2LStr)
                                , new XElement("UseSpecialBall", r.UseSpecialBall))
                            , new XElement("MultiRegionLottoGameList"
                                , from r in inout_LottoGames.MultiRegionLottoGameMapList
                                  select
                                  new XElement("MultiRegionLottoGameRec"
                                      , new XAttribute("FileName", r.GameFileName)
                                      , new XAttribute("MultiGameName", r.GameName)))
                        ));
                }
                else
                {
                    // Create MultiState Document
                    oDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes")
                    , new XElement
                    ("DocumentElement"
                      , from r in inout_LottoGames.RegionLottoGameDict.Values
                        where (r.RegionType == RegionTypeEnum.MULTISTATE
                            && r.FileName == sFileName)
                        select
                          new XElement("LotteryGames"
                            , new XElement("GameName", r.GameName)
                            , new XElement("LottoBallRegUB", r.LottoBallRegUB)
                            , new XElement("LottoBallSpecialUB", r.LottoBallSpecialUB)
                            , new XElement("LottoBallRegCount", r.LottoBallRegCount)
                            , new XElement("LottoBallRegOrderingH2L", r.LottoBallRegOrderingH2LStr)
                            , new XElement("LottoBallSpecialOrderingH2L", r.LottoBallSpecialOrderingH2LStr)
                            , new XElement("UseSpecialBall", r.UseSpecialBall))));
                }

                XmlWriterSettings oWriterSettings = new XmlWriterSettings();
                oWriterSettings.Indent = true;

                IsolatedStorageFile oIsolatedStorageFile 
                    = IsolatedStorageFile.GetUserStoreForApplication();
                string sPathAndFileName = System.IO.Path.Combine("GameConfig", sFileName);
                using (IsolatedStorageFileStream oSourceStream
                    = new IsolatedStorageFileStream(sPathAndFileName, FileMode.Create, oIsolatedStorageFile))
                {
                    using (XmlWriter oWriter = XmlWriter.Create(oSourceStream, oWriterSettings))
                    {
                        oDoc.WriteTo(oWriter);
                        // Write the XML to the file.
                        oWriter.Flush();
                    }
                }
            }
            catch { }
        }
    }
}
