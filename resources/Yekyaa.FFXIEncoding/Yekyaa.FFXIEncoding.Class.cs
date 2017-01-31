using System;  // Required for everything.
using System.IO; // for BinaryReader
using System.Threading; // Required for the SplashScreen as well as for future Threading of file loads
using Microsoft.Win32;  // for accessing the registry
using System.Collections.Generic; // for the arrays/Lists
namespace Yekyaa.FFXIEncoding
{
    /// <summary>
    /// Base class which is the basis for storing Auto-Translate Phrase information.
    /// </summary>
    public class FFXIATPhrase : Object
    {
        #region FFXIATPhrase Variables
        public byte StringResource;
        public byte Language;
        public byte GroupID;
        public byte MessageID;
        public string value;
        public string shortvalue;
        public UInt16 BaseType;
        public UInt16 Type;
        public UInt16 ActualType;
        public UInt16 Flags;
        #endregion

        #region FFXIATPhrase Methods
        public void Copy(FFXIATPhrase from)
        {
            this.StringResource = from.StringResource;
            this.Language = from.Language;
            this.GroupID = from.GroupID;
            this.MessageID = from.MessageID;
            this.value = from.value;
            this.shortvalue = from.shortvalue;
            this.BaseType = from.BaseType;
            this.Type = from.Type;
            this.ActualType = from.ActualType;
            this.Flags = from.Flags;
        }

        public override string ToString()
        {
            return String.Format("{0}{1:X2}{2:X2}{3:X2}{4:X2}{5}{6}{7}",
                FFXIEncoding.StartMarker, (uint)this.StringResource, (uint)this.Language,
                (uint)this.GroupID, (uint)this.MessageID, FFXIEncoding.MiddleMarker, this.value,
                FFXIEncoding.EndMarker);
        }
        public string ToString(string format)
        {
            return String.Format(format,
                FFXIEncoding.StartMarker, (uint)this.StringResource, (uint)this.Language,
                (uint)this.GroupID, (uint)this.MessageID, FFXIEncoding.MiddleMarker, this.value,
                FFXIEncoding.EndMarker);
        }
        public string GetCSVInfo()
        {
            return String.Format("{0},{1},{2},{3},{4}", 
                (this.StringResource == 0x07) ? 
                (int)(((int)this.GroupID << 8) + (int)this.MessageID) : 0,
                    this.value, this.shortvalue, this.BaseType, this.ActualType);
                    
        }
        public string GetInfo()
        {
            return String.Format("{0}\t{5}\t\\xFD\\x{1:X2}\\x{2:X2}\\x{3:X2}\\x{4:X2}\\xFD{6}", this.value,
                    this.StringResource, this.Language, this.GroupID, this.MessageID,
                    ((this.Language != 0x01) && (this.StringResource != 0x02)) ? this.shortvalue : " ",
                    (this.StringResource == 0x07) ? "\tItemID: " + (int)(((int)this.GroupID << 8) + (int)this.MessageID) : "");
        }
        #endregion

        #region FFXIATPhrase Constructor
        public FFXIATPhrase()
        {
            StringResource = 0x00;
            Language = 0x00;
            GroupID = 0x00;
            MessageID = 0x00;
            value = String.Empty;
            shortvalue = String.Empty;
            Flags = 0;  
            Type = 0;
        }
        #endregion
    }

    /// <summary>
    /// Class which contains the ATPhrases list as well as the Loading functions for such.
    /// </summary>
    public class FFXIATPhraseLoader : Object
    {
        #region FFXIATPhraseLoader Variables
        private FFXIATPhrase[] JobList = null;
        private FFXIATPhrase[] AreaList = null;
        private FFXIATPhrase[] AbilityInfo = null;
        private FFXIATPhrase[] SpellInfo = null;

        private FFXIATPhrase[] _ATPhrases = null;
        private FFXIATPhrase[] _ATKeys_Items = null;

        private bool LoadItems = true;
        private bool LoadKeyItems = true;
        private bool LoadAutoTranslatePhrases = true;

        /// <summary>
        /// Internal instance of FFXIEncoding for use during conversions.
        /// </summary>
        private FFXIEncoding _FFXIConvert = null;

        /// <summary>
        /// Local copy of the current Preferences.Language
        /// </summary>
        private int LanguagePreference = ffxiLanguages.LANG_ENGLISH;

        /// <summary>
        /// Stores the directory location for every file in _fileNumberArray.
        /// </summary>
        private string[] _fileNumberArrayList = null;

        /// <summary>
        /// Private static class of ints containing information for use with _fileNumberArray.
        /// Because enums suck.
        /// </summary>
        private static class ffxiFileTypes
        {
            public static readonly int ITEMS = 0;
            public static readonly int OBJS = 1;
            public static readonly int WPNS = 2;
            public static readonly int ARMR = 3;
            public static readonly int PUPP = 4;
            public static readonly int GIL = 5;
            public static readonly int JOBS = 6;
            public static readonly int AREA = 7;
            public static readonly int ABIL = 8;
            public static readonly int SPEL = 9;
            public static readonly int KEY = 10;
            public static readonly int AT_P = 11;
        }


        /// <summary>
        /// Static class of Languages for use with FFXIPhraseLoader.
        /// </summary>
        public static class ffxiLanguages
        {
            public static readonly int NUM_LANG_MIN = 1; // Japanese
            public static readonly int NUM_LANG_MAX = 4; // French (currently 4 languages)
            public static readonly int LANG_ALL = int.MaxValue; // Special, Indicates All Languages
            // BEGIN: DO NOT CHANGE THESE
            public static readonly int LANG_JAPANESE = 1;
            public static readonly int LANG_ENGLISH = 2;
            public static readonly int LANG_DEUTSCH = 3;
            public static readonly int LANG_FRENCH = 4;
            // END: DO NOT CHANGE THESE

            // if you change any of the following numbers, be sure to modify the order
            // of each set of numbers in _fileNumberArray
        }
        /*
        0 - Nothing
        1 - Item
        2 - Quest Item
        3 - Fish
        4 - Weapon
        5 - Armor
        6 - Linkshell
        7 - Usable Item
        8 - Crystal
        10 - Furnishing
        11 - Plant
        12 - Flowerpot
        13 - Puppet Item
        14 - Mannequin
        15 - Book         }
        */
        public enum ItemTypes
        {
            Nothing = 0, 
            Item, 
            Quest_Item, 
            Fish, 
            Weapon, 
            Armor, 
            Linkshell, 
            Usable_Item,
            Crystal,
            Furnishing,
            Plant,
            Puppet_Item,
            Mannequin,
            Book
        };

        [Flags]
        public enum ItemFlags
        {
            None = 0x0000,
            Rare = 0x8000, // Rare 
            Inscribable = 0x0020, // Can be synthed using guild crystals (i.e. can be inscribed)
            NoAuction = 0x0040, // Cannot be sold at the AH
            Scroll = 0x0080, // Is a scroll
            Linkshell = 0x0100, // Is a linkshell
            CanUse = 0x0200, // Can be used
            CanTrade = 0x0400, // Can be traded to an NPC
            CanEquip = 0x0800, // Can be equipped
            NoSale = 0x1000, // Cannot be sold to NPC
            NoDelivery = 0x2000, // Cannot be sent to mog house
            NoTrade = 0x4000,  // Cannot be traded to a player (includes bazaar)
            Ex = (NoAuction | NoDelivery | NoTrade), //0x6040, // NoAuction, NoDelivery, NoTrade
            Nothing = (Linkshell | NoSale | Ex | Rare)
        }

        /// <summary>
        /// Current array of exact file numbers that contain the information for each file type.
        /// </summary>
        private int[] _fileNumberArray = { -1,                               // Nil, ignore
                // To access, do (NUM_LANG_MAX * file_type) + Pref.Language
                // General Items                                            = 0
                4,	73,	55815,  56235,
                // Usable Objects                                           = 1
                5,	74,	55816,	56236,
                // Weapons                                                  = 2
                6,	75,	55817,	56237,
                // Armor                                                    = 3
                7,	76,	55818,	56238,
                // Puppet                                                   = 4
                8,	77,	55819,	56239,
                // Gil                                                      = 5
                9,	91,	55820,	56240,
                // JOB Lists in their native language, JP, EN, DE, FR order = 6
                55536, 55467, 55776, 56196,
                // AREA Lists (JP, EN, DE, FR)                              = 7
                55535, 55465, 55775, 56195,
                // ABILITY Info (JP, EN, DE, FR)                            = 8
                55581, 55701, 55821, 56241, // Use String 2 for French(?)
                // SPELL Info (JP, EN, DE, FR)                              = 9
                55582, 55702, 55822, 56242,
                // Key Items (JP, EN, DE, FR)                               = 10
                55575, 55695, 55825, 56245, 
                // Auto-Translate Files (JP, EN, DE, FR)                    = 11
                55545, 55665, 55785, 56205
            };

        /// <summary>
        /// Just a list of valid languages in their text format.
        /// </summary>
        public static readonly String[] Languages = { "All", "Japanese", "English", "Deutsch", "French", "Load All" };

        #endregion

        #region FFXIATPhraseLoader Properties
        public FFXIATPhrase[] ATPhrases
        {
            get { return _ATPhrases; }
        }

        public String[] fileNumberArrayList
        {
            get { return _fileNumberArrayList; }
        }

        /// <summary>
        /// Property that returns the internal instance of FFXIEncoding.
        /// </summary>
        public FFXIEncoding FFXIConvert
        {
            get { return _FFXIConvert; }
        }
        #endregion

        #region FFXIATPhraseLoader Methods
        #region public string GetRegistryKey
        /// <summary>Locates the Registry Key for the FFXI Installation on this computer.</summary>
        /// <returns>Directory to the FFXI Installation with a trailing backslash.</returns>
        public string GetRegistryKey()
        {
            string s = String.Empty;
            // Attempt to open the key
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\PlayOnlineUS\InstallFolder");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\PlayOnlineEU\InstallFolder");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\PlayOnline\InstallFolder");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\PlayOnlineUS\InstallFolder");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\PlayOnlineEU\InstallFolder");
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\PlayOnline\InstallFolder");

            // Attempt to retrieve the value "0001"; if null is returned, the value
            // doesn't exist in the registry.
            if ((key != null) && (key.GetValue("0001") != null))
            {
                s = (string)key.GetValue("0001");
                return (String.Format("{0}\\", s.TrimEnd('\\')));
            }
            return String.Empty;
        }
        #endregion

        #region Decoding Methods and related.
        /// <summary>Rotates the bits to the right by a set number of places.</summary>
        /// <param name="b">The byte whose bits we want to shift with rotation (preserving all set bits).</param>
        /// <param name="count">The number of places we want to shift the byte by.</param>
        /// <returns>The newly rotated byte.</returns>
        private byte RotateRight(byte b, int count)
        {
            for (; count > 0; count--)
            {
                if ((b & 0x01) == 0x01)
                {
                    b >>= 1; // if the last bit is 1 (ex. 00000001, it needs to be dropped
                    b |= 0x80; // and then set as the first bit (ex. 10000000)
                }
                else b >>= 1; // if the last bit is not 1 (set), just rotate as normal.
            }
            return b;
        }

        /// <summary>
        /// Decodes a data/text block by reversing the set bit rotation.
        /// </summary>
        /// <param name="bytes">Array of bytes to decode.</param>
        /// <param name="shiftcount">The number of bits to shift right by.</param>
        /// <returns>The modified bytes array.</returns>
        private byte[] DecodeBlock(byte[] bytes, int shiftcount)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = RotateRight(bytes[i], shiftcount);
            return bytes;
        }
        #endregion

        #region _fileNumberArray related stuff.
        /// <summary>
        /// Returns the offset for a given File Type (Items, Objs, etc) based on current language.
        /// </summary>
        /// <param name="filetype">File type that we're attempting to retrieve the file number for.</param>
        /// <returns>Zero on failure (_fileNumberArray[0] is -1, INVALID FILE), the index of the file number in the _fileNumberArray on success.</returns>
        private int GetFileNumber(int filetype)
        {
            if ((this.LanguagePreference >= ffxiLanguages.NUM_LANG_MIN) && (this.LanguagePreference <= ffxiLanguages.NUM_LANG_MAX))
            {
                if ((filetype < 0) || (filetype >= _fileNumberArrayList.Length))
                    return 0;
                return (int)((filetype * ffxiLanguages.NUM_LANG_MAX) + this.LanguagePreference);
            }
            return 0;
        }

        private int GetFileNumber(int filetype, int language)
        {
            if ((language >= ffxiLanguages.NUM_LANG_MIN) && (language <= ffxiLanguages.NUM_LANG_MAX))
            {
                if ((filetype < 0) || (filetype >= _fileNumberArrayList.Length))
                    return 0;
                return (int)((filetype * ffxiLanguages.NUM_LANG_MAX) + language);
            }
            return 0;
        }

        /// <summary>
        /// Returns a string in the format ROM\dir\file.DAT given a UInt16 fileID.
        /// </summary>
        /// <param name="fileID">The file ID found in FTABLE.DAT and VTABLE.DAT</param>
        /// <returns>String location of the file that fileID references.</returns>
        private string InterpretPath(UInt16 fileID)
        {
            // all files for the FINAL FANTASY XI\FTABLE & VTABLE.DAT files are in ROM\
            // further files in the ROM2, ROM3, ROM4 folders have a separate FTABLE/VTABLE file
            // in their subdirectory.
            return String.Format("ROM\\{0}\\{1}.DAT", fileID >> 7, fileID & 0x007F);
        }

        public void LoadAllFiles(string s)
        {
            if (s == String.Empty)
                return;
            if (!File.Exists(s + "VTABLE.DAT"))
                throw new FileNotFoundException("File does not exist!", s + "VTABLE.DAT");
            if (!File.Exists(s + "FTABLE.DAT"))
                throw new FileNotFoundException("File does not exist!", s + "FTABLE.DAT");

            String filenameToCheck = String.Empty;

            BinaryReader vtable = new BinaryReader(File.Open(s + "VTABLE.DAT", FileMode.Open));
            BinaryReader ftable = new BinaryReader(File.Open(s + "FTABLE.DAT", FileMode.Open));
            long num = 0;

            if (ftable.BaseStream.Length != (vtable.BaseStream.Length * 2))
                throw new FileNotFoundException("FTable is not twice the size of the Vtable!", s + "VTABLE.DAT");
            if (_fileNumberArrayList == null)
                _fileNumberArrayList = new string[vtable.BaseStream.Length];
            while (vtable.BaseStream.Position < vtable.BaseStream.Length)
            {

                num = vtable.BaseStream.Position;
                if (vtable.ReadByte() == 0x00)
                {
                    _fileNumberArrayList[num] = String.Empty;
                    continue;
                }
                ftable.BaseStream.Position = num * 2;
                filenameToCheck = s + InterpretPath(ftable.ReadUInt16());
                if (!File.Exists(filenameToCheck))
                    throw new FileNotFoundException("File does not exist!", filenameToCheck);
                _fileNumberArrayList[num] = filenameToCheck;
            }
            vtable.Close();
            ftable.Close();
        }

        /// <summary>
        /// Fills the _fileNumberArrayList using the numbers in _fileNumberArray and FTABLE.DAT and VTABLE.DAT as reference.
        /// </summary>
        /// <param name="s">The path to FTABLE.DAT and VTABLE.DAT</param>
        private void LoadFileIDs(string s)
        {
            if (s == String.Empty)
                return;
            if (!File.Exists(s + "VTABLE.DAT"))
                throw new FileNotFoundException("File does not exist!", s + "VTABLE.DAT");
            if (!File.Exists(s + "FTABLE.DAT"))
                throw new FileNotFoundException("File does not exist!", s + "FTABLE.DAT");

            String filenameToCheck = String.Empty;

            BinaryReader vtable = new BinaryReader(File.Open(s + "VTABLE.DAT", FileMode.Open, FileAccess.Read));
            BinaryReader ftable = new BinaryReader(File.Open(s + "FTABLE.DAT", FileMode.Open, FileAccess.Read));
            int step = SplashScreen.ComputeStep(_fileNumberArray.Length);
            foreach (int num in _fileNumberArray)
            {
                SplashScreen.SetStatus(String.Empty, step); // 49 * 10 = 490/10000 = 5%
                if (_fileNumberArrayList == null)
                    _fileNumberArrayList = new string[1];
                else Array.Resize(ref _fileNumberArrayList, _fileNumberArrayList.Length + 1);
                if (num > 0)
                {
                    vtable.BaseStream.Position = num;
                    if (vtable.ReadByte() == 0x00)
                    {
                        _fileNumberArrayList[_fileNumberArrayList.Length - 1] = "INVALID_FILE - num less than 0";
                        continue;
                    }
                    ftable.BaseStream.Position = num * 2;
                    filenameToCheck = s + InterpretPath(ftable.ReadUInt16());
                    if (!File.Exists(filenameToCheck))
                        throw new FileNotFoundException("File does not exist!", filenameToCheck);

                    _fileNumberArrayList[_fileNumberArrayList.Length - 1] = filenameToCheck;
                }
                else _fileNumberArrayList[_fileNumberArrayList.Length - 1] = "INVALID_FILE - num less than 0";
            }
            vtable.Close();
            ftable.Close();
        }
        #endregion

        #region Load Jobs, Areas, Abilities, Spells, Items, Key Items, Auto-Translate Phrases.
        /// <summary>
        /// Loads the Job names into a temporary array for reference by LoadAutoTranslateFile().
        /// </summary>
        private void LoadJobList(int language)
        {
            d_msgFile en_d_msg = new d_msgFile(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.JOBS, language)], this.FFXIConvert);

            if ((en_d_msg == null) || (en_d_msg.EntryList == null) || (en_d_msg.Header == null))
                return;

            if (JobList == null)
                JobList = new FFXIATPhrase[en_d_msg.Header.EntryCount];
            else if (JobList.Length != en_d_msg.Header.EntryCount)
            {
                Array.Resize(ref JobList, (int)en_d_msg.Header.EntryCount);
            }

            int step = SplashScreen.ComputeStep((int)en_d_msg.Header.EntryCount);

            // for each entry
            for (int i = 0; i < JobList.Length; i++)
            {
                SplashScreen.SetStatus(String.Empty, step); // 21 * 10 = 210/10000 = 2.5%
                JobList[i] = new FFXIATPhrase();
                if ((en_d_msg.EntryList[i] == null) ||
                    (en_d_msg.EntryList[i].data == null) ||
                    (en_d_msg.EntryList[i].data[0] == null) ||
                    (en_d_msg.EntryList[i].data[0].Length <= 0))
                    continue;

                // to save memory, only create a structure for a valid Spell Info block
                if (en_d_msg.EntryList[i].data[0].Length > 0)
                    JobList[i].value = new String(en_d_msg.EntryList[i].data[0].ToCharArray());
                else JobList[i].value = "<<UNKNOWN>>";
                JobList[i].Language = (byte)language;
            }
        }
        private void LoadJobList()
        {
            LoadJobList(this.LanguagePreference);
        }

        /// <summary>
        /// Loads the Area names into a temporary array for reference by LoadAutoTranslateFile().
        /// </summary>
        private void LoadAreaList(int language)
        {
            d_msgFile en_d_msg = new d_msgFile(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.AREA, language)], this.FFXIConvert);

            if ((en_d_msg == null) || (en_d_msg.EntryList == null) || (en_d_msg.Header == null))
                return;

            if (AreaList == null)
            {
                AreaList = new FFXIATPhrase[en_d_msg.Header.EntryCount];
            }
            else if (AreaList.Length != en_d_msg.Header.EntryCount)
            {
                Array.Resize(ref AreaList, (int)en_d_msg.Header.EntryCount);
            }
            int step = SplashScreen.ComputeStep((int)en_d_msg.Header.EntryCount);

            // for each entry
            for (int i = 0; i < AreaList.Length; i++)
            {
                AreaList[i] = new FFXIATPhrase();

                SplashScreen.SetStatus(String.Empty, step); // 255 * 10 = 2550/10000 = 25%

                if ((en_d_msg.EntryList[i] == null) ||
                    (en_d_msg.EntryList[i].data == null) ||
                    (en_d_msg.EntryList[i].data[0] == null) ||
                    (en_d_msg.EntryList[i].data[0].Length <= 0))
                    continue;

                // to save memory, only create a structure for a valid Spell Info block
                if (en_d_msg.EntryList[i].data[0].Length > 0)
                    AreaList[i].value = new String(en_d_msg.EntryList[i].data[0].ToCharArray());
                else AreaList[i].value = "<<UNKNOWN>>";
                AreaList[i].Language = (byte)language;
            }
        }
        private void LoadAreaList()
        {
            LoadAreaList(this.LanguagePreference);
        }

        /// <summary>
        /// Loads the Ability names into a temporary array for reference by LoadAutoTranslateFile().
        /// </summary>
        private void LoadAbilityInfo(int language)
        {
            d_msgFile en_d_msg = new d_msgFile(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.ABIL, language)], this.FFXIConvert);

            if ((en_d_msg == null) || (en_d_msg.EntryList == null) || (en_d_msg.Header == null))
                return;

            if (AbilityInfo == null)
            {
                AbilityInfo = new FFXIATPhrase[en_d_msg.Header.EntryCount];
            }
            else if (AbilityInfo.Length != en_d_msg.Header.EntryCount)
            {
                Array.Resize(ref AbilityInfo, (int)en_d_msg.Header.EntryCount);
            }

            int step = SplashScreen.ComputeStep((int)en_d_msg.Header.EntryCount);

            // for each entry
            for (int i = 0; i < AbilityInfo.Length; i++)
            {
                AbilityInfo[i] = new FFXIATPhrase();

                SplashScreen.SetStatus(String.Empty, step); // 49 * 10 = 490/10000 = 5%

                if ((en_d_msg.EntryList[i] == null) ||
                    (en_d_msg.EntryList[i].data == null) ||
                    (en_d_msg.EntryList[i].data[0] == null) ||
                    (en_d_msg.EntryList[i].data[0].Length <= 0))
                    continue;

                // to save memory, only create a structure for a valid Spell Info block
                if (en_d_msg.EntryList[i].data[0].Length > 0)
                    AbilityInfo[i].value = new String(en_d_msg.EntryList[i].data[0].ToCharArray());
                else AbilityInfo[i].value = "<<UNKNOWN>>";
                AbilityInfo[i].Language = (byte)language;
            }
        }
        private void LoadAbilityInfo()
        {
            LoadAbilityInfo(this.LanguagePreference);
        }

        /// <summary>
        /// Loads the Spell names into a temporary array for reference by LoadAutoTranslateFile().
        /// </summary>
        private void LoadSpellInfo(int language)
        {
            d_msgFile en_d_msg = new d_msgFile(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.SPEL, language)], this.FFXIConvert);

            if ((en_d_msg == null) || (en_d_msg.EntryList == null) || (en_d_msg.Header == null))
                return;

            // Setup Spell Info structure for program
            if (SpellInfo == null)
            {
                SpellInfo = new FFXIATPhrase[en_d_msg.Header.EntryCount];
            }
            else if (SpellInfo.Length != en_d_msg.Header.EntryCount)
            {
                Array.Resize(ref SpellInfo, (int)en_d_msg.Header.EntryCount);
            }

            int step = SplashScreen.ComputeStep(SpellInfo.Length > 0 ? SpellInfo.Length : 1);

            for (int i = 0; i < SpellInfo.Length; i++)
            {
                // to save memory, only create a structure for a valid Spell Info block
                SpellInfo[i] = new FFXIATPhrase();
                SplashScreen.SetStatus(String.Empty, step); // 49 * 10 = 490/10000 = 5%

                if ((en_d_msg.EntryList[i] == null) ||
                    (en_d_msg.EntryList[i].data == null) ||
                    (en_d_msg.EntryList[i].data[0] == null) ||
                    (en_d_msg.EntryList[i].data[0].Length <= 0))
                    continue;

                if (en_d_msg.EntryList[i].data[0].Length > 0)
                    SpellInfo[i].value = new String(en_d_msg.EntryList[i].data[0].ToCharArray());
                else SpellInfo[i].value = "<<UNKNOWN>>";
                SpellInfo[i].Language = (byte)language;
            }
        }
        private void LoadSpellInfo()
        {
            LoadSpellInfo(this.LanguagePreference);
        }

        /// <summary>
        /// Generic loader for loading Item names into a permanent array for use by GetPhrases().
        /// </summary>
        /// <param name="file_num">File number to load (OBJ, ITEMS, PUPP, ARMR, WEPN, GIL)</param>
        private void LoadItemFile(int file_num, int language)
        {
            if (_fileNumberArrayList == null)
                return;
            else if ((file_num < 0) || (file_num >= _fileNumberArrayList.Length))
                return;

            FileInfo fi = new FileInfo(_fileNumberArrayList[file_num]);
            Boolean file_error = false;

            if (!fi.Exists) return;
            if ((fi.Length % 0xC00) != 0) return;

            int items_in_file = (int)(fi.Length / 0xC00);
            BinaryReader iteminfo = null;
            try
            {
                iteminfo = new BinaryReader(File.Open(_fileNumberArrayList[file_num], FileMode.Open, FileAccess.Read));
            }
            catch (IOException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\r\nSome Auto-Translate phrases may not be available.");
                file_error = true;
            }
            if (file_error == true) return;  // Attempt a Sanity Check
            for (int item_counter = 0; item_counter < items_in_file; item_counter++)
            {
                SplashScreen.SetStatus(String.Empty, 7); // 49 * 10 = 490/10000 = 5%
                iteminfo.BaseStream.Position = 0xC00 * item_counter;
                byte[] readbytes = DecodeBlock(iteminfo.ReadBytes(0x200), 5);
                BinaryReader data = new BinaryReader(new MemoryStream(readbytes, false));
                itemFormat itemObjects = new itemFormat(data, FFXIConvert);
                // INSERT ITEM CHECK DATA HERE
                data.Close();
                //if ((itemObjects.itemHeader.ID < 0xFFFF) && (itemObjects.itemHeader.ID > 0x6FFF))
                //    continue;
                //else if (itemObjects.text == String.Empty)
                //    continue;
                //else 
                if ((itemObjects.itemHeader.Flags & (ushort)ItemFlags.Nothing) == (ushort)ItemFlags.Nothing)
                    continue;
                else if (itemObjects.itemHeader.ID == 0x00)
                    continue;

                // 0x0100 0x0040 0x1000
                /* UINT32 ID
                 * UINT16 Flags
                 * UINT16 Stack Size
                 * UINT16 Type
                 * UINT16 ResourceID
                 * UINT16 ValidTargets 
                 * 14 Bytes - Common Header Size
                 */
                int atp = 0;

                if (_ATKeys_Items == null)
                    _ATKeys_Items = new FFXIATPhrase[1];
                else Array.Resize(ref _ATKeys_Items, (int)(_ATKeys_Items.Length + 1));

                atp = _ATKeys_Items.Length - 1;

                _ATKeys_Items[atp] = new FFXIATPhrase();
                _ATKeys_Items[atp].StringResource = 0x07;
                _ATKeys_Items[atp].Language = (byte)language;
                _ATKeys_Items[atp].GroupID = (byte)((itemObjects.itemHeader.ID & 0xFF00) >> 8);
                _ATKeys_Items[atp].MessageID = (byte)(itemObjects.itemHeader.ID & 0x00FF);
                _ATKeys_Items[atp].Flags = itemObjects.itemHeader.Flags;
                _ATKeys_Items[atp].ActualType = itemObjects.itemHeader.Type;
                if ((itemObjects.itemHeader.ID >= 0x0000) && (itemObjects.itemHeader.ID <= 0x0FFF))
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.ITEMS;
                else if ((itemObjects.itemHeader.ID >= 0x1000) && (itemObjects.itemHeader.ID <= 0x1FFF))
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.OBJS;
                else if ((itemObjects.itemHeader.ID >= 0x2000) && (itemObjects.itemHeader.ID <= 0x2FFF))
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.PUPP;
                else if ((itemObjects.itemHeader.ID >= 0x2C00) && (itemObjects.itemHeader.ID <= 0x3FFF))
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.ARMR;
                else if ((itemObjects.itemHeader.ID >= 0x4000) && (itemObjects.itemHeader.ID <= 0x6FFF))
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.WPNS;
                else if (itemObjects.itemHeader.ID == 0xFFFF) // Gil
                    _ATKeys_Items[atp].BaseType = (ushort)FFXIATPhraseLoader.ffxiFileTypes.GIL;

                if (itemObjects.itemHeader.Type != 0)  // if it's nothing, categorize at as something else
                {
                    _ATKeys_Items[atp].Type = itemObjects.itemHeader.Type;
                }
                else
                {
                    if ((itemObjects.itemHeader.ID <= 0x2FFF) && (itemObjects.itemHeader.ID >= 0x2000))
                        _ATKeys_Items[atp].Type = 13; // Puppet Items
                    else if ((itemObjects.itemHeader.ID <= 0x3FFF) && (itemObjects.itemHeader.ID >= 0x3000))
                        _ATKeys_Items[atp].Type = 5; // Armor Items
                    else if ((itemObjects.itemHeader.ID <= 0x6FFF) && (itemObjects.itemHeader.ID >= 0x4000))
                        _ATKeys_Items[atp].Type = 4; // Weapon Items
                }
                _ATKeys_Items[atp].value = itemObjects.text;
                _ATKeys_Items[atp].shortvalue = itemObjects.logtext; // misleading i know
            }
            iteminfo.Close();
        }
        private void LoadItemFile(int file_num)
        {
            LoadItemFile(file_num, this.LanguagePreference);
        }

        /// <summary>
        /// Loads all 6 types (OBJ, ITEMS, ARMR, PUPP, WPNS, GIL) by subbing the responsibilities out to LoadItemFile.
        /// </summary>
        private void LoadItemInfo(int language)
        {
            LoadItemFile(GetFileNumber(ffxiFileTypes.GIL, language), language);
            LoadItemFile(GetFileNumber(ffxiFileTypes.ARMR, language), language);
            LoadItemFile(GetFileNumber(ffxiFileTypes.WPNS, language), language);
            LoadItemFile(GetFileNumber(ffxiFileTypes.PUPP, language), language);
            LoadItemFile(GetFileNumber(ffxiFileTypes.OBJS, language), language);
            LoadItemFile(GetFileNumber(ffxiFileTypes.ITEMS, language), language);
        }
        private void LoadItemInfo()
        {
            LoadItemInfo(this.LanguagePreference);
        }

        /// <summary>
        /// Loads the Key Item names into a permanent array for use by GetPhrases().
        /// </summary>
        private void LoadKeyItemInfo(int language)
        {
            // Initialize Streams
            d_msgFile en_d_msg = new d_msgFile(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.KEY, language)], this.FFXIConvert);

            if ((en_d_msg == null) || (en_d_msg.EntryList == null) || (en_d_msg.Header == null))
                return;

            int ATKeys_Items_Array_Pos = -1;
            if (_ATKeys_Items == null)
                _ATKeys_Items = new FFXIATPhrase[en_d_msg.Header.EntryCount];
            else
            {
                // Append to it
                ATKeys_Items_Array_Pos = _ATKeys_Items.Length - 1;
                Array.Resize(ref _ATKeys_Items, (int)(_ATKeys_Items.Length + en_d_msg.Header.EntryCount));
            }

            int step = SplashScreen.ComputeStep((int)en_d_msg.Header.EntryCount);
            // for each entry
            for (int i = 0; i < en_d_msg.Header.EntryCount; i++)
            {
                SplashScreen.SetStatus(String.Empty, step); // 49 * 10 = 490/10000 = 5%

                if ((en_d_msg.EntryList[i] == null) ||
                    (en_d_msg.EntryList[i].data == null) ||
                    (en_d_msg.EntryList[i].data.Length == 0))
                    continue;

                int data_cnt = 0;
                // Length - 1 b/c I do NOT want to include DESCRIPTIONS... ugh, stupid German and French
                for (; data_cnt < (en_d_msg.EntryList[i].data.Length - 1); data_cnt++)
                {
                    if ((en_d_msg.EntryList[i].data[data_cnt] == null) ||
                        (en_d_msg.EntryList[i].data[data_cnt].Trim().Trim("\0\u0001.".ToCharArray()) == String.Empty) ||
                        (en_d_msg.EntryList[i].data[data_cnt].Length <= 0))
                        continue;
                    else break;
                }
                // Length - 1 b/c I do NOT want to include DESCRIPTIONS... ugh, stupid German and French
                if (data_cnt >= (en_d_msg.EntryList[i].data.Length - 1))
                    continue;

                ATKeys_Items_Array_Pos++;

                // to save memory, only create a structure for a valid Spell Info block
                _ATKeys_Items[ATKeys_Items_Array_Pos] = new FFXIATPhrase();
                _ATKeys_Items[ATKeys_Items_Array_Pos].StringResource = 0x13;
                _ATKeys_Items[ATKeys_Items_Array_Pos].Language = (byte)language;
                _ATKeys_Items[ATKeys_Items_Array_Pos].MessageID = en_d_msg.EntryList[i].MessageID;
                _ATKeys_Items[ATKeys_Items_Array_Pos].GroupID = en_d_msg.EntryList[i].GroupID;

                if (en_d_msg.EntryList[i].data.Length >= 3)
                {
                    if ((language >= ffxiLanguages.NUM_LANG_MIN) && (language <= ffxiLanguages.NUM_LANG_MAX))
                        _ATKeys_Items[ATKeys_Items_Array_Pos].value = new String(en_d_msg.EntryList[i].data[data_cnt].ToCharArray());
                }
                else _ATKeys_Items[ATKeys_Items_Array_Pos].value = "<<UNKNOWN>>";
            }
        }
        private void LoadKeyItemInfo()
        {
            LoadKeyItemInfo(this.LanguagePreference);
        }

        /// <summary>
        /// Loads the Auto-Translate phrases into a permanent array for use by GetPhrases().
        /// </summary>
        private void LoadAutoTranslateFile(int language)
        {
            if (language == 4)
            {
                int xivi = 0;
                xivi++;
            }
            atphraseFileFormat at_phrase_file = new atphraseFileFormat(this._fileNumberArrayList[GetFileNumber(ffxiFileTypes.AT_P, language)]);

            if ((at_phrase_file == null) || (at_phrase_file.AtEntry == null) || (at_phrase_file.AtGroup == null))
                return;

            int step = SplashScreen.ComputeStep(at_phrase_file.AtGroup.Length + at_phrase_file.AtEntry.Length);

            int value = 0;
            string valuetoconvert;
            int cnt = -1;
            if (_ATPhrases != null)
                cnt = _ATPhrases.Length - 1;
            if (_ATPhrases == null)
                _ATPhrases = new FFXIATPhrase[at_phrase_file.AtGroup.Length + at_phrase_file.AtEntry.Length];
            else Array.Resize(ref _ATPhrases, _ATPhrases.Length + (at_phrase_file.AtGroup.Length + at_phrase_file.AtEntry.Length));
            // Read Groups first.
            int g_cnt = 0, e_cnt = 0;
            for (g_cnt = 0; g_cnt < at_phrase_file.AtGroup.Length; g_cnt++)
            {
                SplashScreen.SetStatus(String.Empty, step);
                cnt++;
                if (_ATPhrases[cnt] == null)
                    _ATPhrases[cnt] = new FFXIATPhrase();
                _ATPhrases[cnt].StringResource = at_phrase_file.AtGroup[g_cnt].StringResourceType;
                _ATPhrases[cnt].Language = at_phrase_file.AtGroup[g_cnt].LanguageCode;
                _ATPhrases[cnt].GroupID = at_phrase_file.AtGroup[g_cnt].GroupID;
                _ATPhrases[cnt].MessageID = at_phrase_file.AtGroup[g_cnt].MessageID;
                _ATPhrases[cnt].value = this.FFXIConvert.GetString(at_phrase_file.AtGroup[g_cnt].Text).Trim().Trim("\0\u0001.".ToCharArray());
                _ATPhrases[cnt].shortvalue = this.FFXIConvert.GetString(at_phrase_file.AtGroup[g_cnt].CompletionText).Trim().Trim("\0\u0001.".ToCharArray());
            }
            for (e_cnt = 0; e_cnt < at_phrase_file.AtEntry.Length; e_cnt++)
            {
                SplashScreen.SetStatus(String.Empty, step);
                cnt++;
                if (_ATPhrases[cnt] == null)
                    _ATPhrases[cnt] = new FFXIATPhrase();
                _ATPhrases[cnt].StringResource = at_phrase_file.AtEntry[e_cnt].StringResourceType;
                _ATPhrases[cnt].Language = at_phrase_file.AtEntry[e_cnt].LanguageCode;
                _ATPhrases[cnt].GroupID = at_phrase_file.AtEntry[e_cnt].GroupID;
                _ATPhrases[cnt].MessageID = at_phrase_file.AtEntry[e_cnt].MessageID;
                _ATPhrases[cnt].value = this.FFXIConvert.GetString(at_phrase_file.AtEntry[e_cnt].Text).Trim().Trim("\0\u0001.".ToCharArray());
                _ATPhrases[cnt].shortvalue = this.FFXIConvert.GetString(at_phrase_file.AtEntry[e_cnt].CompletionText).Trim().Trim("\0\u0001.".ToCharArray());
                if ((_ATPhrases[cnt].value != String.Empty) && (_ATPhrases[cnt].value[0] == '@'))
                {
                    switch (_ATPhrases[cnt].value[1])
                    {
                        case 'J':
                            if (JobList != null)
                            {
                                valuetoconvert = "0x" + _ATPhrases[cnt].value.Substring(2);
                                value = System.Convert.ToInt32(valuetoconvert, 16);
                                if (value < JobList.Length)
                                {
                                    _ATPhrases[cnt].value = JobList[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].shortvalue = JobList[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].Language = JobList[value].Language;
                                }
                                else
                                {
                                    _ATPhrases[cnt].value = "EN Job #" + value;
                                    _ATPhrases[cnt].shortvalue = "EN Job #" + value;
                                }
                            }
                            break;
                        case 'A':
                            if (AreaList != null)
                            {
                                valuetoconvert = "0x" + _ATPhrases[cnt].value.Substring(2);
                                value = System.Convert.ToInt32(valuetoconvert, 16);
                                if (value < AreaList.Length)
                                {
                                    _ATPhrases[cnt].value = AreaList[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].shortvalue = AreaList[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].Language = AreaList[value].Language;
                                }
                                else
                                {
                                    _ATPhrases[cnt].value = "EN Area #" + value;
                                    _ATPhrases[cnt].shortvalue = "EN Area #" + value;
                                }
                            }
                            break;
                        case 'Y':
                            if (AbilityInfo != null)
                            {
                                valuetoconvert = "0x" + _ATPhrases[cnt].value.Substring(2);
                                value = System.Convert.ToInt32(valuetoconvert, 16);
                                if (value < AbilityInfo.Length)
                                {
                                    _ATPhrases[cnt].value = AbilityInfo[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].shortvalue = AbilityInfo[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].Language = AbilityInfo[value].Language;
                                }
                                else
                                {
                                    _ATPhrases[cnt].value = "EN Ability #" + value;
                                    _ATPhrases[cnt].shortvalue = "EN Ability #" + value;
                                }
                            }
                            break;
                        case 'C':
                            if (SpellInfo != null)
                            {
                                valuetoconvert = "0x" + _ATPhrases[cnt].value.Substring(2);
                                value = System.Convert.ToInt32(valuetoconvert, 16);
                                if (value < SpellInfo.Length)
                                {
                                    _ATPhrases[cnt].value = SpellInfo[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].shortvalue = SpellInfo[value].value.Trim().Trim("\0\u0001.".ToCharArray());
                                    _ATPhrases[cnt].Language = SpellInfo[value].Language;
                                }
                                else
                                {
                                    _ATPhrases[cnt].value = "EN Spell #" + value;
                                    _ATPhrases[cnt].shortvalue = "EN Spell #" + value;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            Array.Sort(_ATPhrases, new FFXIATPhraseLoader.ATPhraseCompareByID());
            int x_abcdefg = 0;
            x_abcdefg++;
        }
        private void LoadAutoTranslateFile()
        {
            LoadAutoTranslateFile(this.LanguagePreference);
        }
        #endregion

        #region GetPhrases() overloads
        public FFXIATPhrase[] GetPhrases()
        {
            return GetPhrases(0, 0, 0, 0, true);
        }

        public FFXIATPhrase[] GetPhrases(bool shownum)
        {
            return GetPhrases(0, 0, 0, 0, shownum);
        }

        public FFXIATPhrase[] GetPhrases(int sr)
        {
            return GetPhrases(sr, 0, 0, 0, true);
        }

        public FFXIATPhrase[] GetPhrases(int sr, bool shownum)
        {
            return GetPhrases(sr, 0, 0, 0, shownum);
        }

        public FFXIATPhrase[] GetPhrases(int sr, int language)
        {
            return GetPhrases(sr, language, 0, 0, true);
        }

        public FFXIATPhrase[] GetPhrases(int sr, int language, bool shownum)
        {
            return GetPhrases(sr, language, 0, 0, shownum);
        }

        public FFXIATPhrase[] GetPhrases(int sr, int language, int group)
        {
            return GetPhrases(sr, language, group, 0, true);
        }

        public FFXIATPhrase[] GetPhrases(int sr, int language, int group, bool shownum)
        {
            return GetPhrases(sr, language, group, 0, shownum);
        }

        public FFXIATPhrase[] GetPhrases(int sr, int language, int group, int messageid)
        {
            return GetPhrases(sr, language, group, messageid, true);
        }

        /// <summary>
        /// Gets an array of phrases based on set of codes.
        /// </summary>
        /// <param name="s">Search string.</param>
        /// <returns></returns>
        public FFXIATPhrase[] GetPhrases(int sr, int language, int group, int messageid, bool shownum)
        {
            int count = 0;
            FFXIATPhrase[] ret = null;

            if (shownum == true)
            {
                if (ret == null)
                    ret = new FFXIATPhrase[1];
                else Array.Resize(ref ret, ret.Length + 1);
                if (ret[count] == null)
                    ret[count] = new FFXIATPhrase();
                ret[count].StringResource = 0;
                ret[count].Language = 0;
                ret[count].GroupID = 0;
                ret[count].MessageID = 0;
                ret[count].Flags = 0;
                ret[count].value = String.Empty;
                ret[count].shortvalue = null;
                count++;
            }

            if (((sr == 0x00) || (sr == 0x07) || (sr == 0x13)) && (_ATKeys_Items != null) && (_ATKeys_Items.Length >= 1))
            {
                for (int i = 0; i < _ATKeys_Items.Length; i++)
                {
                    if (_ATKeys_Items[i] == null) continue;
                    else if (_ATKeys_Items[i].value == String.Empty) continue;
                    else if (((sr == 0) || (sr == _ATKeys_Items[i].StringResource)) &&
                            ((language == ffxiLanguages.LANG_ALL) || (language == 0) || (language == _ATKeys_Items[i].Language)) &&
                            ((group == 0) || (group == _ATKeys_Items[i].GroupID)) &&
                            ((messageid == 0) || (messageid == _ATKeys_Items[i].MessageID)))
                    {
                        if (ret == null)
                            ret = new FFXIATPhrase[1];
                        else Array.Resize(ref ret, ret.Length + 1);
                        if (ret[count] == null)
                            ret[count] = new FFXIATPhrase();
                        ret[count].Copy(_ATKeys_Items[i]);
                        /*
                        ret[count].StringResource = _ATKeys_Items[i].StringResource;
                        ret[count].Language = _ATKeys_Items[i].Language;
                        ret[count].GroupID = _ATKeys_Items[i].GroupID;
                        ret[count].MessageID = _ATKeys_Items[i].MessageID;
                        ret[count].Flags = _ATKeys_Items[i].Flags;
                        ret[count].value = _ATKeys_Items[i].value;
                        ret[count].shortvalue = _ATKeys_Items[i].shortvalue;
                        ret[count].BaseType = _ATKeys_Items[i].BaseType;
                        ret[count].ActualType = _ATKeys_Items[i].ActualType;
                        ret[count].Type = _ATKeys_Items[i].Type;
                         */
                        count++;
                    }
                }
            }
            if (((sr == 0) || ((sr != 0x07) && (sr != 0x13))) && (_ATPhrases != null) && (_ATPhrases.Length >= 1))
            {
                for (int i = 0; i < _ATPhrases.Length; i++)
                {
                    if (_ATPhrases[i] == null) continue;
                    else if (_ATPhrases[i].value == String.Empty) continue;
                    else if (((sr == 0) || (sr == _ATPhrases[i].StringResource)) &&
                            ((language == ffxiLanguages.LANG_ALL) || (language == 0) || (language == _ATPhrases[i].Language)) &&
                            ((group == 0) || (group == _ATPhrases[i].GroupID)) &&
                            ((messageid == 0) || (messageid == _ATPhrases[i].MessageID)))
                    {
                        if (ret == null)
                            ret = new FFXIATPhrase[1];
                        else Array.Resize(ref ret, ret.Length + 1);
                        if (ret[count] == null)
                            ret[count] = new FFXIATPhrase();
                        ret[count].Copy(_ATPhrases[i]);
                        ret[count].shortvalue = null;
                        /*ret[count].StringResource = _ATPhrases[i].StringResource;
                        ret[count].Language = _ATPhrases[i].Language;
                        ret[count].GroupID = _ATPhrases[i].GroupID;
                        ret[count].MessageID = _ATPhrases[i].MessageID;
                        ret[count].Flags = _ATPhrases[i].Flags;
                        ret[count].value = _ATPhrases[i].value;
                        ret[count].shortvalue = null;*/
                        count++;
                    }
                }
            }
            if (shownum == true)
            {
                count--; // acct for this extra one phrase sent
                if (count == 0)
                {
                    ret[0].value = "What is this, a wild goose chase? Type something in.";
                }
                else
                {
                    ret[0].value = String.Format("Found {0} similar phrase{1} matching '{2:X2}{3:X2}{4:X2}{5:X2}'.",
                        count, (count == 1) ? "" : "s", sr, language, group, messageid);
                }
            }
            return ret;
        }

        public FFXIATPhrase[] GetPhraseViaRegEx(string pattern, bool shownum)
        {
            List<FFXIATPhrase> ret = new List<FFXIATPhrase>();

            if ((pattern == null) || (pattern.Trim() == String.Empty))
                return null;

            if (System.Text.RegularExpressions.Regex.IsMatch(String.Empty, pattern))
                return null;

            if (shownum == true)
            {
                FFXIATPhrase header = new FFXIATPhrase();
                header.StringResource = 0;
                header.Language = 0;
                header.GroupID = 0;
                header.MessageID = 0;
                header.Flags = 0;
                header.value = String.Empty;
                header.shortvalue = null;
                ret.Add(header);
            }
            foreach (FFXIATPhrase x in this._ATKeys_Items)
            {
                if ((x.value != null) && (x.value.Trim() != String.Empty))
                    if (System.Text.RegularExpressions.Regex.IsMatch(x.value, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        ret.Add(x);
                    }
            }
            foreach (FFXIATPhrase x in this._ATPhrases)
            {
                if ((x.value != null) && (x.value.Trim() != String.Empty))
                    if (System.Text.RegularExpressions.Regex.IsMatch(x.value, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        ret.Add(x);
                    }
            }
            if (shownum == true)
            {
                ret[0].value = String.Format("Found {0} similar phrase{1} matching '{2}'.",
                    ret.Count - 1, ((ret.Count - 1) == 1) ? "" : "s", pattern);
            }
            if (ret.Count == 0)
                return null;

            return ret.ToArray();
        }

        public FFXIATPhrase[] GetPhrases(string s)
        {
            return GetPhrases(s, this.LanguagePreference, true);
        }

        public FFXIATPhrase[] GetPhrases(string s, bool shownum)
        {
            return GetPhrases(s, this.LanguagePreference, shownum);
        }

        /// <summary>
        /// Gets an array of phrases based on a search string.
        /// </summary>
        /// <param name="s">Search string.</param>
        /// <returns></returns>
        public FFXIATPhrase[] GetPhrases(string s, int language)
        {
            // backward compatibility, show information about phrases found
            return GetPhrases(s, language, true);
        }
        /// <summary>
        /// Gets an array of phrases based on a search string.
        /// </summary>
        /// <param name="s">Search string.</param>
        /// <param name="shownum">True to show number of found objects as first object in list.</param>
        /// <returns>Array of FFXIATPhrases objects.</returns>
        public FFXIATPhrase[] GetPhrases(string s, int language, bool shownum)
        {
            int count = 0;
            FFXIATPhrase[] ret = null;

            if (shownum == true)
            {
                if (ret == null)
                    ret = new FFXIATPhrase[1];
                else Array.Resize(ref ret, ret.Length + 1);
                if (ret[count] == null)
                    ret[count] = new FFXIATPhrase();
                ret[count].StringResource = 0;
                ret[count].Language = 0;
                ret[count].GroupID = 0;
                ret[count].MessageID = 0;
                ret[count].Flags = 0;
                ret[count].value = String.Empty;
                ret[count].shortvalue = null;
                count++;
            }

            if (s.Trim() != String.Empty)
            {
                String checkNumbers = s.Trim(FFXIEncoding.StartMarker).Trim(FFXIEncoding.EndMarker);
                if (checkNumbers.Length == 8)
                {
                    UInt32 number = 0xFFFFFFFF;
                    if (UInt32.TryParse(checkNumbers, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out number))
                    {
                        return GetPhrases((int)((number & 0xFF000000) >> 24),
                                          FFXIATPhraseLoader.ffxiLanguages.LANG_ALL, // if it was only numbers, find all related ones regardless of language
                                          (int)((number & 0x0000FF00) >> 8),
                                          (int)(number & 0x000000FF), shownum);
                    }

                }

                if ((_ATKeys_Items != null) && (_ATKeys_Items.Length >= 1))
                {
                    for (int i = 0; i < _ATKeys_Items.Length; i++)
                    {
                        if (_ATKeys_Items[i] == null) continue;
                        else if (_ATKeys_Items[i].value == String.Empty) continue;
                        else if ((_ATKeys_Items[i].Language != language) && (language != ffxiLanguages.LANG_ALL)) continue;
                        else if ((String.Compare(s, 0, _ATKeys_Items[i].value, 0, s.Length, true) == 0) ||
                                 ((_ATKeys_Items[i].value[0] == '/') &&
                                  (String.Compare(s, 0, _ATKeys_Items[i].value, 1, s.Length, true) == 0)))
                        {
                            if (ret == null)
                                ret = new FFXIATPhrase[1];
                            else Array.Resize(ref ret, ret.Length + 1);
                            if (ret[count] == null)
                                ret[count] = new FFXIATPhrase();
                            ret[count].Copy(_ATKeys_Items[i]);
                            /*
                            ret[count].StringResource = _ATKeys_Items[i].StringResource;
                            ret[count].Language = _ATKeys_Items[i].Language;
                            ret[count].Flags = _ATKeys_Items[i].Flags;
                            ret[count].GroupID = _ATKeys_Items[i].GroupID;
                            ret[count].MessageID = _ATKeys_Items[i].MessageID;
                            ret[count].value = _ATKeys_Items[i].value;
                            ret[count].shortvalue = _ATKeys_Items[i].shortvalue;
                            ret[count].Type = _ATKeys_Items[i].Type;
                             */
                            count++;
                        }
                    }
                }
                if ((_ATPhrases != null) && (_ATPhrases.Length >= 1))
                {
                    for (int i = 0; i < _ATPhrases.Length; i++)
                    {
                        if (_ATPhrases[i] == null) continue;
                        else if (_ATPhrases[i].MessageID == 0x00) continue; // disable groups?
                        else if (_ATPhrases[i].value == String.Empty) continue;
                        else if ((_ATPhrases[i].Language != language) && (language != ffxiLanguages.LANG_ALL)) continue;
                        else if ((String.Compare(s, 0, _ATPhrases[i].value, 0, s.Length, true) == 0) ||
                                 ((_ATPhrases[i].value[0] == '/') &&
                                  (String.Compare(s, 0, _ATPhrases[i].value, 1, s.Length, true) == 0)))
                        {
                            if (ret == null)
                                ret = new FFXIATPhrase[1];
                            else Array.Resize(ref ret, ret.Length + 1);
                            if (ret[count] == null)
                                ret[count] = new FFXIATPhrase();
                            ret[count].Copy(_ATPhrases[i]);
                            /*
                            ret[count].StringResource = _ATPhrases[i].StringResource;
                            ret[count].Language = _ATPhrases[i].Language;
                            ret[count].GroupID = _ATPhrases[i].GroupID;
                            ret[count].MessageID = _ATPhrases[i].MessageID;
                            ret[count].value = _ATPhrases[i].value;
                            ret[count].Flags = _ATPhrases[i].Flags;*/
                            ret[count].shortvalue = null;
                            count++;
                        }
                        else if ((_ATPhrases[i].shortvalue != null) && (_ATPhrases[i].shortvalue != String.Empty) &&
                           ((String.Compare(s, 0, _ATPhrases[i].shortvalue, 0, s.Length, true) == 0) ||
                           ((_ATPhrases[i].shortvalue[0] == '/') && (String.Compare(s, 0, _ATPhrases[i].shortvalue, 1, s.Length, true) == 0)))) // equal or it's a shorthand
                        {
                            if (ret == null)
                                ret = new FFXIATPhrase[1];
                            else Array.Resize(ref ret, ret.Length + 1);
                            if (ret[count] == null)
                                ret[count] = new FFXIATPhrase();
                            ret[count].Copy(_ATPhrases[i]);
                            /*
                            ret[count].StringResource = _ATPhrases[i].StringResource;
                            ret[count].Language = _ATPhrases[i].Language;
                            ret[count].GroupID = _ATPhrases[i].GroupID;
                            ret[count].MessageID = _ATPhrases[i].MessageID;
                            ret[count].value = _ATPhrases[i].shortvalue;
                            ret[count].Flags = _ATPhrases[i].Flags;
                             */
                            ret[count].shortvalue = null;
                            count++;
                        }
                    }
                }
            }
            if (shownum == true)
            {
                count--; // acct for this extra one phrase sent
                if ((s == String.Empty) || (s == null))
                {
                    ret[0].value = "What is this, a wild goose chase? Type something in.";
                }
                else
                {
                    ret[0].value = String.Format("Found {0} similar phrase{1} matching '{2}'.",
                        count, (count == 1) ? "" : "s", s);
                }
            }
            return ret;
        }
        #endregion

        #region ByteToHex()
        /// <summary>
        /// Converts a given given byte into the decimal value it's letter is associated with.
        /// </summary>
        /// <param name="b">The byte which should be 0-9 or A-F or a-f.</param>
        /// <returns>Zero if byte is not 0-9, A-F, or a-f, else it returns the decimal equivalent.</returns>
        private int ByteToHex(byte b)
        {
            char[] map = { '0', '1', '2', '3', '4', '5', '6', '7', 
                           '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int c = 0;
            for (c = 0; c < map.Length; c++)
                if (map[c] == Char.ToUpper((char)b))
                    return c;
            return 0;
        }
        #endregion

        #region GetPhraseByID() overloads
        public int GetIndexInItemsByID(byte StringResource, byte Language, byte GroupID, byte MessageID)
        {
            if (((StringResource == 0x07) || (StringResource == 0x13)) &&
                ((_ATKeys_Items == null) || (_ATKeys_Items.Length < 1)))
                return -1;
            else if ((StringResource != 0x07) && (StringResource != 0x13))
                return -1;

            FFXIATPhrase[] phraseArray = _ATKeys_Items;

            if ((phraseArray == null) || (phraseArray.Length <= 0))
                return -1;

            for (int i = 0; i < phraseArray.Length; i++)
            {
                if (phraseArray[i] == null)
                    continue;
                else if ((phraseArray[i].StringResource == StringResource) &&
                    (phraseArray[i].Language == Language) &&
                    (phraseArray[i].GroupID == GroupID) &&
                    (phraseArray[i].MessageID == MessageID))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the Auto-Translate Phrase or Item/Key Item that matches the given parameters exactly.
        /// </summary>
        /// <param name="StringResource">The String Resource to match.</param>
        /// <param name="Language">The Language Code to match.</param>
        /// <param name="GroupID">The Group ID to match.</param>
        /// <param name="MessageID">The Message ID to match.</param>
        /// <returns>Returns a reference to the FFXIATPhrase in the array, null if not found.</returns>
        public FFXIATPhrase GetPhraseByID(byte StringResource, byte Language, byte GroupID, byte MessageID)
        {
            if (((StringResource == 0x07) || (StringResource == 0x13)) &&
                ((_ATKeys_Items == null) || (_ATKeys_Items.Length < 1)))
                return null;
            else if (((StringResource != 0x07) && (StringResource != 0x13)) &&
                ((_ATPhrases == null) || (_ATPhrases.Length < 1)))
                return null;

            FFXIATPhrase[] phraseArray = null;

            if ((StringResource == 0x07) || (StringResource == 0x13))
                phraseArray = _ATKeys_Items;
            else if ((StringResource != 0x07) && (StringResource != 0x13))
                phraseArray = _ATPhrases;

            if ((phraseArray == null) || (phraseArray.Length <= 0))
                return null;

            for (int i = 0; i < phraseArray.Length; i++)
            {
                if (phraseArray[i] == null)
                    continue;
                else if ((phraseArray[i].StringResource == StringResource) &&
                    // if all phrases are loaded, match exact, if not, find closest match to language
                    (phraseArray[i].Language == Language) &&
                    (phraseArray[i].GroupID == GroupID) &&
                    (phraseArray[i].MessageID == MessageID))
                {
                    return phraseArray[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the Auto-Translate Phrase Group that matches the given parameters (Not valid for items or key items).
        /// </summary>
        /// <param name="StringResource">The String Resource to match.</param>
        /// <param name="Language">The Language Code to match.</param>
        /// <param name="GroupID">The Group ID to match.</param>
        /// <returns>Returns a reference to the FFXIATPhrase Group in the array, null if items, key items, or not found.</returns>
        public FFXIATPhrase GetPhraseByID(byte StringResource, byte Language, byte GroupID)
        {
            if ((StringResource == 0x07) || (StringResource == 0x13))
                return null; // no group indexes for items/key items
            return this.GetPhraseByID(StringResource, Language, GroupID, 0); // return the group Index
        }
        #endregion

        #region ToString() override
        public override string ToString()
        {
            int length1 = 0, length2 = 0;
            if (this._ATPhrases != null)
                length1 = this._ATPhrases.Length;
            if (this._ATKeys_Items != null)
                length2 = this._ATKeys_Items.Length;
            int total = length1 + length2;
            return String.Format("Total {0} item{1}: ATPhrases ({2}) -- Keys and Items ({3}).",
                total, (total == 1) ? "" : "s",
                length1, length2);
        }
        #endregion
        #endregion

        #region FFXIATPhraseLoader Constructor
        public FFXIATPhraseLoader() : this(ffxiLanguages.LANG_ENGLISH, true, true, true, String.Empty) { }
        public FFXIATPhraseLoader(int langpref, string text) : this(langpref, true, true, true, text) { }
        public FFXIATPhraseLoader(int langpref) : this(langpref, true, true, true, String.Empty) { }
        public FFXIATPhraseLoader(int langpref, bool loadobjs, bool loadkeys, bool loadatphrases, String text)
        {
            if (this._FFXIConvert == null)
                this._FFXIConvert = new FFXIEncoding();

            if (((langpref >= ffxiLanguages.NUM_LANG_MIN) && (langpref <= ffxiLanguages.NUM_LANG_MAX)) || (langpref == ffxiLanguages.LANG_ALL))
                this.LanguagePreference = langpref;

            this.LoadItems = loadobjs;
            this.LoadKeyItems = loadkeys;
            this.LoadAutoTranslatePhrases = loadatphrases;

            // Load autotranslate phrases here.
            string s = GetRegistryKey();
            if ((s == String.Empty) || (s == null))
            {
                if (LanguagePreference == ffxiLanguages.LANG_ALL)
                    System.Windows.Forms.MessageBox.Show("Unable to load " + Languages[0] + " Auto-Translate Phrases. FFXI Installation Not Found!", "Error While Loading Phrases");
                else System.Windows.Forms.MessageBox.Show("Unable to load " + Languages[LanguagePreference] + " Auto-Translate Phrases. FFXI Installation Not Found!", "Error While Loading Phrases");
                return;
            }

            if (text.CompareTo("LOAD FILE LIST") == 0)
            {
                LoadAllFiles(s);
                return;
            }

            try
            {
                SplashScreen.EncodingVersion = String.Format("Yekyaa's Encoding v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                if (text != String.Empty)
                    SplashScreen.TextToDisplay = text;

                SplashScreen.ProgressVisibility = false;
                if (langpref != ffxiLanguages.LANG_ALL)
                {
                    SplashScreen.ShowSplashScreen(LanguagePreference);
                    if (this.LoadAutoTranslatePhrases || this.LoadKeyItems || this.LoadItems)
                    {
                        SplashScreen.SetStatus("Loading Information...");
                        SplashScreen.ClearProgress();
                        Thread.Sleep(600);
                        LoadFileIDs(s);
                        SplashScreen.ProgressVisibility = true;
                        if (this.LoadAutoTranslatePhrases)
                        {
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Job Lists...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadJobList(); });
                            LoadJobList();
                            SplashScreen.EndProgress();
                            Thread.Sleep(250);
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Area Lists...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadAreaList(); });
                            LoadAreaList();
                            SplashScreen.EndProgress();
                            Thread.Sleep(100);
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Ability Info...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadAbilityInfo(); });
                            LoadAbilityInfo();
                            SplashScreen.EndProgress();
                            Thread.Sleep(100);
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Spell Info...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadSpellInfo(); });
                            LoadSpellInfo();
                            SplashScreen.EndProgress();
                            Thread.Sleep(100);
                        }
                        if (this.LoadItems)
                        {
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Item Info...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadItemInfo(); });
                            LoadItemInfo();
                            SplashScreen.EndProgress();
                            Thread.Sleep(100);
                        }
                        if (this.LoadKeyItems)
                        {
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Key Item Info...");
                            //ThreadPool.QueueUserWorkItem((WaitCallback)delegate { LoadKeyItemInfo(); });
                            LoadKeyItemInfo();
                            SplashScreen.EndProgress();
                            Thread.Sleep(100);
                        }
                        if (this.LoadAutoTranslatePhrases)
                        {
                            SplashScreen.ClearProgress();
                            SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Auto-Translate Phrases...");
                            LoadAutoTranslateFile();
                            SplashScreen.EndProgress();
                        }
                        SplashScreen.SetStatus("Loading " + Languages[LanguagePreference] + " Information Completed.");
                    }
                    else Thread.Sleep(5000); // if none are set, still show splashscreen for 5s
                }
                else
                {
                    SplashScreen.ShowSplashScreen(ffxiLanguages.LANG_ALL);
                    if (this.LoadAutoTranslatePhrases || this.LoadKeyItems || this.LoadItems)
                    {
                        SplashScreen.SetStatus("Loading Information...");
                        SplashScreen.ClearProgress();
                        Thread.Sleep(600);
                        LoadFileIDs(s);
                        SplashScreen.ProgressVisibility = true;
                        for (int i = ffxiLanguages.NUM_LANG_MIN; i <= ffxiLanguages.NUM_LANG_MAX; i++)
                        {
                            SplashScreen.PictureBox = SplashScreen.Icons[i];
                            if (this.LoadAutoTranslatePhrases)
                            {
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Job Lists...");
                                LoadJobList(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(250);
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Area Lists...");
                                LoadAreaList(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(100);
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Ability Info...");
                                LoadAbilityInfo(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(100);
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Spell Info...");
                                LoadSpellInfo(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(100);
                            }
                            if (this.LoadItems)
                            {
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Item Info...");
                                LoadItemInfo(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(100);
                            }
                            if (this.LoadKeyItems)
                            {
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Key Item Info...");
                                LoadKeyItemInfo(i);
                                SplashScreen.EndProgress();
                                Thread.Sleep(100);
                            }
                            if (this.LoadAutoTranslatePhrases)
                            {
                                SplashScreen.ClearProgress();
                                SplashScreen.SetStatus("Loading " + Languages[i] + " Auto-Translate Phrases...");
                                LoadAutoTranslateFile(i);
                                SplashScreen.EndProgress();
                            }
                            SplashScreen.SetStatus("Loading " + Languages[i] + " Information Completed.");
                        }
                    }
                    else Thread.Sleep(5000); // if none are set, still show splashscreen for 5s

                    SplashScreen.ProgressVisibility = false;
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Error while loading Files:\r\n{0} -- {1}", e.FileName, e.Message), "Error found!");
                _ATPhrases = null;
                _ATKeys_Items = null;
                // Do Not Rethrow this exception. It doesn't work out well.
            }
            finally
            {
                JobList = null;
                AreaList = null;
                AbilityInfo = null;
                SpellInfo = null;
                _fileNumberArrayList = null;
                SplashScreen.CloseForm();
            }
        }
        #endregion

        #region Additional Classes used in FFXIATPhraseLoader
        #region ATPhraseCompare???? classes for IComparer-style interface.
        /// <summary>
        /// For comparing the numeric values of FFXIATPhrases for purposes of sorting by group and message ids, similar to FFXI.
        /// </summary>
        public class ATPhraseCompareByID : System.Collections.IComparer
        {
            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                FFXIATPhrase xs = x as FFXIATPhrase;
                FFXIATPhrase ys = y as FFXIATPhrase;
                if (xs.Language == ys.Language)
                {
                    if (xs.GroupID == ys.GroupID) // if same group
                    {
                        if (xs.MessageID == ys.MessageID) // Zero if x equals y.
                            return 0;
                        else if (xs.MessageID < ys.MessageID) // Less than zero if x is less than y.
                            return -1;
                        else if (xs.MessageID > ys.MessageID) // Greater than zero if x is greater than y.
                            return 1;
                    }
                    else if (xs.GroupID < ys.GroupID) // Less than zero if x is less than y.
                        return -1;
                    else if (xs.GroupID > ys.GroupID) // Greater than zero if x is greater than y.
                        return 1;
                }
                else if (xs.Language < ys.Language)
                    return -1;
                else if (xs.Language > ys.Language)
                    return 1;

                return 0; // Should never reach here really.
            }
        }

        /// <summary>
        /// For comparing the text values of FFXIATPhrases for purposes of alphabetic menu creation.
        /// </summary>
        public class ATPhraseCompareByValue : System.Collections.IComparer
        {

            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                FFXIATPhrase xs = x as FFXIATPhrase;
                FFXIATPhrase ys = y as FFXIATPhrase;
                return (String.Compare(xs.value, ys.value));
            }

        }
        #endregion

        #region Auto-Translate Phrase specific classes (EntryFormat, GroupFormat, FileFormat)
        public class atphraseEntryFormat : Object
        {
            #region atphraseEntryFormat Variables
            private Byte _StringResourceType;
            private Byte _LanguageCode;
            private Byte _GroupID;
            private Byte _MessageID; // always 0, actually MessageID
            private Byte _TextLength;
            private Byte[] _Text;  // size 32 bytes
            private Byte _CompletionTextLength;
            private Byte[] _CompletionText; // 32 bytes
            #endregion

            #region atphraseEntryFormat Properties
            public Byte StringResourceType
            {
                get { return _StringResourceType; }
                set { _StringResourceType = value; }
            }
            public Byte LanguageCode
            {
                get { return _LanguageCode; }
                set { _LanguageCode = value; }
            }
            public Byte GroupID
            {
                get { return _GroupID; }
                set { _GroupID = value; }
            }
            public Byte MessageID
            {
                get { return _MessageID; }
                set { _MessageID = value; }
            }
            public Byte TextLength
            {
                get { return _TextLength; }
                set { _TextLength = value; }
            }
            public Byte[] Text
            {
                get { return _Text; }
                set { _Text = value; }
            }
            public Byte CompletionTextLength
            {
                get { return _CompletionTextLength; }
                set { _CompletionTextLength = value; }
            }
            public Byte[] CompletionText
            {
                get { return _CompletionText; }
                set { _CompletionText = value; }
            }
            #endregion

            #region atphraseEntryFormat Methods
            public override string ToString()
            {
                return String.Format("{0}{1}{2}{3} - '{4}'",
                    _StringResourceType, _LanguageCode, _GroupID, _MessageID,
                    System.Text.Encoding.ASCII.GetString(_Text));
            }
            #endregion

            #region atphraseEntryFormat Constructor
            public atphraseEntryFormat(BinaryReader br)
            {
                try
                {
                    StringResourceType = br.ReadByte();
                    LanguageCode = br.ReadByte();
                    GroupID = br.ReadByte();
                    MessageID = br.ReadByte();
                    TextLength = br.ReadByte();
                    Text = br.ReadBytes((int)TextLength);
                    if ((LanguageCode == ffxiLanguages.LANG_JAPANESE) || (LanguageCode == ffxiLanguages.LANG_FRENCH)) // Japanese only has Completion Text
                    {
                        CompletionTextLength = br.ReadByte();
                        CompletionText = br.ReadBytes((int)CompletionTextLength);
                    }
                    else
                    {
                        CompletionText = new Byte[0];
                        CompletionTextLength = 0;
                    }
                }
                catch (EndOfStreamException)
                {
                    StringResourceType = 0;
                    LanguageCode = 0;
                    GroupID = 0;
                    MessageID = 0;
                    TextLength = 0;
                    Text = new Byte[0];
                    CompletionText = new Byte[0];
                    CompletionTextLength = 0;
                }
                finally
                {
                }
            }
            #endregion
        }
        public class atphraseGroupFormat : Object
        {
            #region atphraseGroupFormat Variables
            private Byte _StringResourceType;
            private Byte _LanguageCode;
            private Byte _GroupID;
            private Byte _MessageID; // always 0, actually MessageID
            private Byte[] _Text;  // size 32 bytes
            private Byte[] _CompletionText; // 32 bytes
            private UInt32 _MessageCount;
            private UInt32 _MessageBytes;
            #endregion

            #region atphraseGroupFormat Properties
            public Byte StringResourceType
            {
                get { return _StringResourceType; }
                set { _StringResourceType = value; }
            }
            public Byte LanguageCode
            {
                get { return _LanguageCode; }
                set { _LanguageCode = value; }
            }
            public Byte GroupID
            {
                get { return _GroupID; }
                set { _GroupID = value; }
            }
            public Byte MessageID
            {
                get { return _MessageID; }
                set { _MessageID = value; }
            }
            public Byte[] Text
            {
                get { return _Text; }
                set { _Text = value; }
            }
            public Byte[] CompletionText
            {
                get { return _CompletionText; }
                set { _CompletionText = value; }
            }
            public UInt32 MessageCount
            {
                get { return _MessageCount; }
                set { _MessageCount = value; }
            }
            public UInt32 MessageBytes
            {
                get { return _MessageBytes; }
                set { _MessageBytes = value; }
            }
            #endregion

            #region atphraseGroupFormat Methods
            public override string ToString()
            {
                return String.Format("{0}{1}{2}{3} - '{4}'",
                    _StringResourceType, _LanguageCode, _GroupID, _MessageID,
                    System.Text.Encoding.ASCII.GetString(_Text));
            }
            #endregion

            #region atphraseGroupFormat Constructor
            public atphraseGroupFormat(BinaryReader br)
            {
                /* BYTE   String Resource Type
                 * BYTE   Language Code
                 * BYTE   Group ID
                 * BYTE   Unused (always 0)
                 * BYTE   Text[32]
                 * BYTE   CompletionText[32]
                 * UINT32 MessageCount
                 * UINT32 MessageBytes 
                 */
                try
                {

                    _StringResourceType = br.ReadByte();
                    _LanguageCode = br.ReadByte();
                    _GroupID = br.ReadByte();
                    _MessageID = br.ReadByte();
                    _Text = br.ReadBytes(32);
                    _CompletionText = br.ReadBytes(32);
                    _MessageCount = br.ReadUInt32();
                    _MessageBytes = br.ReadUInt32();
                }
                catch (EndOfStreamException)
                {
                    _StringResourceType = 0;
                    _LanguageCode = 0;
                    _GroupID = 0;
                    _MessageID = 0;
                    _Text = new Byte[0];
                    _CompletionText = new Byte[0];
                    _MessageCount = 0;
                    _MessageBytes = 0;
                }
                finally
                {
                }
            }
            #endregion
        }
        public class atphraseFileFormat : Object
        {
            #region atphraseFileFormat Variables
            private atphraseGroupFormat[] _atGroup;
            private atphraseEntryFormat[] _atEntry;
            #endregion

            #region atphraseFileFormat Properties
            public atphraseGroupFormat[] AtGroup
            {
                get { return _atGroup; }
                set { _atGroup = value; }
            }

            public atphraseEntryFormat[] AtEntry
            {
                get { return _atEntry; }
                set { _atEntry = value; }
            }
            #endregion

            #region atphraseFileFormat Methods
            private static BinaryReader GetBinaryReader(String filename)
            {
                // Get a File Info on the Spell Info Files, en ROM\119\56.DAT jp ROM\0\11.DAT
                BinaryReader fi_br = null;
                if (File.Exists(filename))
                {
                    FileStream fs = null;
                    try
                    {
                        fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        if (fs != null)
                        {
                            fi_br = new BinaryReader(fs);
                        }
                    }
                    catch
                    {
                        // ignore errors and return null
                    }
                }
                return fi_br;
            }
            #endregion

            #region atphraseFileFormat Constructors
            public atphraseFileFormat(String filename) : this(GetBinaryReader(filename)) { }
            public atphraseFileFormat() { _atGroup = null; _atEntry = null; }
            public atphraseFileFormat(BinaryReader br)
                : this()
            {
                if (br == null)
                    return;

                int e_cnt = 0;
                while ((br.BaseStream.Position + 10) < br.BaseStream.Length)
                {
                    if (_atGroup == null)
                        _atGroup = new atphraseGroupFormat[1];
                    else Array.Resize(ref _atGroup, _atGroup.Length + 1);

                    _atGroup[_atGroup.Length - 1] = new atphraseGroupFormat(br);

                    byte xLC = _atGroup[_atGroup.Length - 1].LanguageCode;

                    if ((xLC >= ffxiLanguages.NUM_LANG_MIN) && (xLC <= ffxiLanguages.NUM_LANG_MAX) && (_atGroup[_atGroup.Length - 1].MessageCount < 100000))
                    {
                        // Do Nothing
                    }
                    else
                    {
                        _atGroup = null;
                        _atEntry = null;
                        break;
                    }

                    int old_length = 0;

                    if (_atEntry == null)
                    {
                        old_length = 0;
                        _atEntry = new atphraseEntryFormat[_atGroup[AtGroup.Length - 1].MessageCount];
                    }
                    else
                    {
                        old_length = _atEntry.Length;
                        Array.Resize(ref _atEntry, (int)_atEntry.Length + (int)_atGroup[AtGroup.Length - 1].MessageCount);
                    }

                    for (e_cnt = old_length; e_cnt < (old_length + _atGroup[_atGroup.Length - 1].MessageCount); e_cnt++)
                    {
                        _atEntry[e_cnt] = new atphraseEntryFormat(br);
                        if (_atEntry[e_cnt].LanguageCode == 0)
                        {
                            _atEntry = null;
                            break;
                        }
                    }
                }
                if (br != null)
                    br.Close();
            }
            #endregion
        }
        #endregion

        #region d_msg specific classes (HeaderFormat, EntryFormat, FileFormat)
        private class d_msgHeaderFormat : Object
        {
            #region d_msgHeaderFormat Variables
            private String _marker;
            private UInt16 _unknown_1; // (always 1)
            private UInt16 _unknown_2; // either 1 or 0
            private UInt32 _unknown_3; // should always be 3
            private UInt32 _unknown_4; // should always be 3

            // Version 1 (Flipped Bits)    Version 2 = Unflipped Bits
            // Common Variables
            private UInt32 _file_size;
            private UInt32 _header_size; // always 64
            private UInt32 _toc_size;           // Version 1 only
            private UInt32 _entry_size;         // Version 2 only
            // Common Variables again
            private UInt32 _data_size; // Version 1 (DataSize = FileSize - TocSize - Header Size) & 2 (Data Size = Entry Count * Entry Size) 
            private UInt32 _entry_count; // Version 1 (ToC Entry Count = ToC Size / 8) & 2 (actual Entry Counts)
            private UInt32 _unknown_6; // Unknown (always 1)
            private UInt64 _unknown_7; // Unknown (always 0)
            private UInt64 _unknown_8; // Unknown (always 0)
            #endregion

            #region d_msgHeaderFormat Properties
            public String Marker
            {
                get { return _marker; }
                set { _marker = value; }
            }
            public Boolean AreBitsFlipped
            {
                get { if (_unknown_2 == 1) return true; else return false; }
            }
            public UInt16 Unknown_1
            {
                get { return _unknown_1; }
                set { _unknown_1 = value; }
            }
            public UInt16 Unknown_2
            {
                get { return _unknown_2; }
                set { _unknown_2 = value; }
            }
            public UInt32 Unknown_3
            {
                get { return _unknown_3; }
                set { _unknown_3 = value; }
            }
            public UInt32 Unknown_4
            {
                get { return _unknown_4; }
                set { _unknown_4 = value; }
            }

            public UInt32 Unknown_6
            {
                get { return _unknown_6; }
                set { _unknown_6 = value; }
            }
            public UInt64 Unknown_7
            {
                get { return _unknown_7; }
                set { _unknown_7 = value; }
            }
            public UInt64 Unknown_8
            {
                get { return _unknown_8; }
                set { _unknown_8 = value; }
            }
            public UInt32 FileSize
            {
                get { return _file_size; }
                set { _file_size = value; }
            }
            public UInt32 HeaderSize
            {
                get { return _header_size; }
                set { _header_size = value; }
            }
            public UInt32 ToCSize
            {
                get { return _toc_size; }
                set { _toc_size = value; }
            }
            public UInt32 EntrySize
            {
                get { return _entry_size; }
                set { _entry_size = value; }
            }
            public UInt32 DataSize
            {
                get { return _data_size; }
                set { _data_size = value; }
            }
            public UInt32 EntryCount
            {
                get { return _entry_count; }
                set { _entry_count = value; }
            }
            #endregion

            #region d_msgHeaderFormat Methods
            public override string ToString()
            {
                return String.Format("Ver{0} EntrSz: {1}b Num:{4} HdrSz {2}",
                            this.Unknown_2, HeaderSize, (this.AreBitsFlipped) ? ToCSize : EntrySize);
            }
            #endregion

            #region d_msgHeaderFormat Constructor
            public d_msgHeaderFormat(BinaryReader _bfile)
            {
                _marker = String.Empty;
                byte[] b = _bfile.ReadBytes(8);
                foreach (byte a in b)
                    _marker += (char)a;
                _marker = _marker.Trim('\0').Trim();
                _unknown_1 = _bfile.ReadUInt16();
                _unknown_2 = _bfile.ReadUInt16();
                _unknown_3 = _bfile.ReadUInt32();
                _unknown_4 = _bfile.ReadUInt32();
                _file_size = _bfile.ReadUInt32();
                _header_size = _bfile.ReadUInt32();
                //_header.Unknown_5 = _bfile.ReadUInt32();
                _toc_size = _bfile.ReadUInt32();
                _entry_size = _bfile.ReadUInt32();
                _data_size = _bfile.ReadUInt32();
                _entry_count = _bfile.ReadUInt32();
                _unknown_6 = _bfile.ReadUInt32();
                _unknown_7 = _bfile.ReadUInt64();
                _unknown_8 = _bfile.ReadUInt64();
            }
            #endregion
        }
        private class d_msgEntryFormat : Object
        {
            #region d_msgEntryFormat Variables
            public UInt32 offset;
            public UInt32 length;
            public String[] data;
            public Byte MessageID;
            public Byte GroupID;
            #endregion

            #region d_msgEntryFormat Methods
            public override string ToString()
            {
                int data_cnt = 0;
                for (; data_cnt < data.Length; data_cnt++)
                {
                    if ((data[data_cnt] == null) ||
                        (data[data_cnt] == String.Empty) ||
                        (data[data_cnt].Length <= 0))
                        continue;
                    else break;
                }
                return String.Format("'{0}'", (data_cnt >= data.Length) ? "(Empty)" : data[data_cnt]);
            }

            // this is necessary as of the Dec 18th, 2006 patch
            // which causes all job/ability/area lists to be NOT'd bitwise
            // as part of the cryptography.
            private byte[] Fix(byte[] b)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    b[i] = (byte)(~((uint)b[i]));
                }
                return b;
            }
            #endregion

            #region d_msgEntryFormat Constructor
            public d_msgEntryFormat(BinaryReader _bfile, d_msgHeaderFormat _header, FFXIEncoding FFXIConvert)
            {
                if (_bfile.BaseStream.Position == _bfile.BaseStream.Length)
                    return;

                // Loader for the new way as of April 2007
                long start_of_data_block = _bfile.BaseStream.Position;
                long saved_pos = _bfile.BaseStream.Position;
                UInt32 string_count;

                if (_header.ToCSize == 0) // no ToC, use EntrySize
                {
                    #region if _header.ToCSize == 0 (Use EntrySize)
                    start_of_data_block = _bfile.BaseStream.Position;
                    if (_header.AreBitsFlipped)
                        string_count = ~(_bfile.ReadUInt32());
                    else string_count = _bfile.ReadUInt32();
                    if ((string_count < 1) || (string_count > 100))
                    {
                        _bfile.BaseStream.Position = start_of_data_block + _header.EntrySize;
                        return;
                    }
                    data = new String[string_count];
                    byte[] b;
                    UInt32 type = 0;
                    for (int str_cnt = 0; str_cnt < string_count; str_cnt++)
                    {
                        data[str_cnt] = String.Empty;
                        if (_header.AreBitsFlipped)
                        {
                            offset = ~(_bfile.ReadUInt32());
                            length = ~(_bfile.ReadUInt32()); // Use "length" for flags
                        }
                        else
                        {
                            offset = _bfile.ReadUInt32();
                            length = _bfile.ReadUInt32(); // Use "length" for flags
                        }
                        saved_pos = _bfile.BaseStream.Position;
                        _bfile.BaseStream.Position = start_of_data_block + offset;
                        type = _bfile.ReadUInt32();
                        if (_header.AreBitsFlipped)
                            type = ~type;
                        b = new byte[4];
                        if ((length == 1) && (str_cnt == 0) && (string_count > 3))  // It's not a String.
                        {
                            // Hack right now for Key Items.
                            MessageID = (byte)(type & 0x00FF);  // already converted.
                            GroupID = (byte)((type & 0xFF00) >> 8);
                            _bfile.BaseStream.Position = saved_pos;
                            continue;
                        }
                        else if (length == 1)
                        {
                            _bfile.BaseStream.Position = saved_pos;
                            continue;
                        }

                        _bfile.BaseStream.Position += 24; // add 24 bytes to get to actual text.
                        byte[] b_xfer = null;
                        do
                        {
                            b[0] = _bfile.ReadByte();
                            b[1] = _bfile.ReadByte();
                            b[2] = _bfile.ReadByte();
                            b[3] = _bfile.ReadByte();
                            if (_header.AreBitsFlipped)
                            {
                                b[0] = (byte)(~((uint)b[0]));
                                b[1] = (byte)(~((uint)b[1]));
                                b[2] = (byte)(~((uint)b[2]));
                                b[3] = (byte)(~((uint)b[3]));
                            }
                            if (b_xfer == null)
                                b_xfer = new byte[4];
                            else Array.Resize(ref b_xfer, b_xfer.Length + 4);
                            b_xfer[b_xfer.Length - 4] = b[0];
                            b_xfer[b_xfer.Length - 3] = b[1];
                            b_xfer[b_xfer.Length - 2] = b[2];
                            b_xfer[b_xfer.Length - 1] = b[3];
                            if (b[3] == 0x00)
                                break;
                        } while (true);

                        data[str_cnt] = FFXIConvert.GetString(b_xfer).Trim('\0');

                        _bfile.BaseStream.Position = saved_pos;
                    }
                    _bfile.BaseStream.Position = start_of_data_block + _header.EntrySize;
                    #endregion
                }
                else if (_header.EntrySize == 0) // use ToCSize
                {
                    #region if _header.EntrySize == 0 (Use ToCSize)
                    start_of_data_block = _header.HeaderSize + _header.ToCSize;

                    offset = _bfile.ReadUInt32();
                    length = _bfile.ReadUInt32();
                    if (_header.AreBitsFlipped)
                    {
                        offset = ~offset;
                        length = ~length;
                    }
                    saved_pos = _bfile.BaseStream.Position;
                    _bfile.BaseStream.Position = start_of_data_block + offset + 40;
                    data = new String[1];

                    if (_header.AreBitsFlipped)
                        data[0] = FFXIConvert.GetString(Fix(_bfile.ReadBytes((int)length - 40))).Trim('\0');
                    else data[0] = FFXIConvert.GetString(_bfile.ReadBytes((int)length - 40)).Trim('\0');
                    _bfile.BaseStream.Position = saved_pos;
                    #endregion
                }
                else return;
            }
            #endregion
        }
        private class d_msgFile : Object
        {
            #region d_msgFile Variables
            private d_msgHeaderFormat _header;
            private d_msgEntryFormat[] _entry_list;
            #endregion

            #region d_msgFile Properties
            public d_msgHeaderFormat Header
            {
                get { return _header; }
            }
            public d_msgEntryFormat[] EntryList
            {
                get { return _entry_list; }
            }
            #endregion

            #region d_msgFile Methods
            public override string ToString()
            {
                if (_header == null)
                    return "Header Not Loaded";
                return String.Format("Ver{0} {1} {2} Entries at {3}b each",
                            _header.Unknown_2, (this._header.AreBitsFlipped) ? "Flipped Bits" : "Non-Flipped", _header.EntryCount,
                            (_header.ToCSize != 0) ? _header.ToCSize : _header.EntrySize);
            }

            private static BinaryReader GetBinaryReader(String filename)
            {
                // Get a File Info on the Spell Info Files, en ROM\119\56.DAT jp ROM\0\11.DAT
                BinaryReader fi_br = null;
                if (File.Exists(filename))
                {
                    FileStream fs = null;
                    try
                    {
                        fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                        if (fs != null)
                        {
                            fi_br = new BinaryReader(fs);
                        }
                    }
                    catch
                    {
                        // ignore errors and return null
                    }
                }
                return fi_br;
            }
            #endregion

            #region d_msgFile Constructors
            public d_msgFile(String fi, FFXIEncoding FFXIConvert) : this(GetBinaryReader(fi), FFXIConvert) { }
            public d_msgFile(BinaryReader _bfile, FFXIEncoding FFXIConvert)
                : this()
            {
                if (_bfile == null)
                    return;

                _header = new d_msgHeaderFormat(_bfile);

                if (_bfile.BaseStream.Length != _header.FileSize)
                {
                    _header = null;
                    return;
                }

                _entry_list = new d_msgEntryFormat[_header.EntryCount];

                for (int counter = 0; counter < _header.EntryCount; counter++)
                    _entry_list[counter] = new d_msgEntryFormat(_bfile, _header, FFXIConvert);

                _bfile.Close();
                _bfile = null;
            }
            public d_msgFile() { _header = null; _entry_list = null; }
            #endregion
        }
        #endregion

        #region item specific classes (itemHeader, itemFormat)
        private class itemHeaderFormat : Object
        {
            #region itemHeaderFormat Variables
            public UInt32 ID;
            public UInt16 Flags;
            public UInt16 StackSize;
            public UInt16 Type;
            public UInt16 ResourceID;
            public UInt16 ValidTargets;
            public UInt16 HeaderSize;
            #endregion

            #region itemHeaderFormat Constructor
            public itemHeaderFormat(BinaryReader br)
            {
                ID = br.ReadUInt32();
                Flags = br.ReadUInt16();
                StackSize = br.ReadUInt16();
                Type = br.ReadUInt16();
                ResourceID = br.ReadUInt16();
                ValidTargets = br.ReadUInt16();
                HeaderSize = 0x0E;  // 14 bytes, Common
            }
            #endregion
        }
        private class itemFormat : Object
        {
            #region itemFormat Variables
            public itemHeaderFormat itemHeader;
            public String text;
            public String logtext;
            #endregion

            #region itemFormat Constructor
            public itemFormat(BinaryReader br, FFXIEncoding FFXIConvert)
            {
                String[] ItemTexts = new String[9];

                itemHeader = new itemHeaderFormat(br);
                long data_pos = 0;
                UInt32 num_strings = 0, offset = 0, flags = 0;
                // Objects (General Items)  skip 6 bytes
                if ((itemHeader.ID <= 0x0FFF) && (itemHeader.ID >= 0x0000))
                    br.BaseStream.Position = itemHeader.HeaderSize + 6;
                // Usable items skip 2 bytes
                // Usable Items skip 6 bytes as of March 10, 2008 Update (new UINT32)
                else if ((itemHeader.ID <= 0x1FFF) && (itemHeader.ID >= 0x1000))
                    br.BaseStream.Position = itemHeader.HeaderSize + 6;
                // Gil skip just 2 bytes
                else if (itemHeader.ID == 0xFFFF)
                    br.BaseStream.Position = itemHeader.HeaderSize + 2;
                // Puppet Items, skip 8 bytes
                else if ((itemHeader.ID <= 0x2BFF) && (itemHeader.ID >= 0x2000))
                    br.BaseStream.Position = itemHeader.HeaderSize + 10;  // Unknown is 0x04 bytes not 0x02
                // Armor Specific Info, 22 bytes to skip to get to text
                // 26 in March 10, 2008 Update (new UINT32)
                else if ((itemHeader.ID <= 0x3FFF) && (itemHeader.ID >= 0x2C00))
                    br.BaseStream.Position = itemHeader.HeaderSize + 26;
                // Weapon Specific Info, 30 bytes to skip
                // 34 bytes in March 10, 2008 Update (new UINT32)
                else if ((itemHeader.ID <= 0x6FFF) && (itemHeader.ID >= 0x4000))
                    br.BaseStream.Position = itemHeader.HeaderSize + 34;
                // Unknown, should not have anything in the way...
                else br.BaseStream.Position = itemHeader.HeaderSize + 2;
                data_pos = br.BaseStream.Position;
                num_strings = br.ReadUInt32();

                long fallback_pos = 0;
                for (int i = 0; (i < 1); i++)
                {
//                    if (num_strings >= 5)
                        //{ int x = i; }

                    offset = br.ReadUInt32();
                    flags = br.ReadUInt32();
                    fallback_pos = br.BaseStream.Position;
                    // Indicator (UInt32) + UInt32 x 6 Padding before text
                    br.BaseStream.Position = data_pos + offset + 28;
                    byte[] b = new byte[4];
                    int counter = 0;
                    do
                    {
                        if (br.BaseStream.Position >= br.BaseStream.Length) break;
                        if (b == null)
                            b = new byte[4];
                        else if ((counter + 4) > b.Length)
                            Array.Resize(ref b, (int)(counter + 4));
                        b[counter++] = br.ReadByte();
                        b[counter++] = br.ReadByte();
                        b[counter++] = br.ReadByte();
                        b[counter++] = br.ReadByte();
                        if (b[counter - 1] == 0x00)
                            break;
                    } while (true);
                    /*if (i > ItemTexts.Length)
                    {
                        i = i+0;
                        break;
                    }*/
                    ItemTexts[i] = FFXIConvert.GetString(b).Trim().Trim("\0\u0001.".ToCharArray());
                    br.BaseStream.Position = fallback_pos;
                }
                text = ItemTexts[0];
                if (num_strings <= 4) // Japanese (no log name, same as shortname)
                    logtext = text;
                else if (num_strings <= 5) // English (shortname, logname is position 3)
                    logtext = ItemTexts[2];
                else if (num_strings <= 6) // French (shortname, logname is position 4)
                    logtext = ItemTexts[3];
                else if (num_strings <= 9)
                    logtext = ItemTexts[4];
                else logtext = text;
            }
            #endregion
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Represents a UTF-16 Encoding of FFXI Characters.
    /// </summary>
    public class FFXIEncoding : System.Text.Encoding
    {
        #region FFXIEncoding Variables
        /// <summary>
        /// Character map for converting from UTF-16 to FFXI-Encoding.
        /// </summary>
        private BinaryReader encoding_br = new BinaryReader(new MemoryStream(FFXIEncodingResources.encoding));

        /// <summary>
        /// Character map for converting from FFXI-Encoding to UTF-16.
        /// </summary>
        private BinaryReader decoding_br = new BinaryReader(new MemoryStream(FFXIEncodingResources.decoding));

        //F     B           A       S       T       W       L       D       G       R
        //Fire  Blizzard    Aero    Stone   Thunder Water   Light   Dark    Green   Red
        private readonly char[] _0xEFmap = { 'F', 'B', 'A', 'S', 'T', 'W', 'L', 'D', 'G', 'R' };

        /// <summary>
        /// FFXI Encoding Marker to indicate start of an auto-translate phrase or special character.
        /// </summary>
        public static readonly char StartMarker = '<';     // <

        /// <summary>
        /// FFXI Encoding Marker to indicate the middle of an auto-translate phrase.
        /// </summary>
        public static readonly char MiddleMarker = '|';    // |

        /// <summary>
        /// FFXI Encoding Marker to indicate end of an auto-translate phrase or special character.
        /// </summary>
        public static readonly char EndMarker = '>';       // >
        #endregion

        #region FFXIEncoding Properties
        public override bool IsSingleByte
        {
            get
            {
                return false;
            }
        }

        public override string BodyName
        {
            get
            {
                return String.Empty;
            }
        }

        public override int CodePage
        {
            get
            {
                return 0;
            }
        }

        public override string EncodingName
        {
            get { return "Yekyaa's FFXI Encoding"; }
        }
        #endregion

        #region FFXIEncoding Methods
        #region Other Byte/Character functions used heavily in this class.
        /// <summary>
        /// I've yet to figure out how I came up with this. Basically if byte given is 0x80 >= b >= 0x9F, returns true.
        /// </summary>
        /// <param name="b">Byte to evaluate.</param>
        /// <returns>true if 0x80 >= b >= 0x9F, false otherwise.</returns>
        private bool IsSurrogate(byte b)
        {
            return ((b >= 0x80) && (b <= 0x9F));
        }

        /// <summary>
        /// Converts char given to it's 1 or 2-byte equivalent.
        /// </summary>
        /// <param name="c">The character to convert to a byte(s).</param>
        /// <returns>Byte array of length 1 or 2 depending on if it's a surrogate character or not.</returns>
        private byte[] ConvertToByte(char c)
        {
            byte b1 = (byte)((c & 0xFF00) >> 8); // high byte
            byte b2 = (byte)(c & 0x00FF); // low byte
            byte[] ReturnValue = new byte[2];
            if ((b1 == 0xFF) && (b2 == 0xFF))
            {
                Array.Resize(ref ReturnValue, 1);
                ReturnValue[0] = (byte)'?';
            }
            else if (b1 == 0x00)
            {
                Array.Resize(ref ReturnValue, 1);
                ReturnValue[0] = b2;
            }
            else
            {
                ReturnValue[0] = b1;
                ReturnValue[1] = b2;
            }
            return ReturnValue;
        }
        #endregion

        #region ////// Conversion from UTF-16 to FFXI-Encoding and back. //////
        /// <summary>Convert FFXI Encoding (single-byte with surrogates) character to UTF16 (double-byte) character.</summary>
        /// <param name="convertChar">The character in FFXI Encoding format.</param>
        /// <returns>UTF-16 converted character.</returns>
        private char FFXIToUTF16(char convertChar)
        {
            Byte b1, b2;
            // Decoding file in program (Resource)
            // is a 2-byte per character lookup table
            // Lookup the address * 2, read 2 bytes,
            // That's the UTF-16 version.
            decoding_br.BaseStream.Position = (long)((UInt32)convertChar * 2);
            b1 = decoding_br.ReadByte();
            b2 = decoding_br.ReadByte();
            return ((char)((uint)((b1 << 8) + b2)));
        }

        /// <summary>Convert UTF16 (double-byte) char to FFXI Encoding (single-byte with surrogates) character.</summary>
        /// <param name="convertChar">The character in UTF16 format.</param>
        /// <returns>Converted character in FFXI Encoding.</returns>
        private char UTF16ToFFXI(char convertChar)
        {
            //UInt32 value = (UInt32)x;
            Byte b1 = 0x00, b2 = 0x00;
            encoding_br.BaseStream.Position = (long)((UInt32)((UInt32)convertChar * 2));
            b1 = encoding_br.ReadByte();
            b2 = encoding_br.ReadByte();
            return ((char)((uint)((b1 << 8) + b2))); // encoding_br.ReadChar();
        }
        #endregion

        #region Get????Count() overloads
        #region GetByteCount() overloads
        /// <summary>
        /// Calculates the number of bytes produced by encoding all the characters in the specified character array.
        /// </summary>
        /// <param name="chars">The character array containing the characters to encode.</param>
        /// <returns>The number of bytes produced by encoding all the characters in the specified character array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if chars is null.</exception>
        public override int GetByteCount(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "chars is null.");

            if (chars.Length <= 0)
                return 0;

            int ReturnValue = 0;

            try
            {
                ReturnValue = GetByteCount(chars, 0, chars.Length);
            }
            catch
            {
            }
            return ReturnValue;
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the specified String.
        /// </summary>
        /// <param name="s">The String containing the set of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if s is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if The resulting number of bytes is greater than the maximum number that can be returned as an integer.</exception>
        public override int GetByteCount(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s", "s is null.");

            if (s == String.Empty)
                return 0;

            int ReturnValue = 0;

            try
            {
                ReturnValue = this.GetByteCount(s.ToCharArray());
            }
            catch (ArgumentOutOfRangeException err)
            {
                throw err;
            }
            catch (ArgumentException err)
            {
                throw err;
            }
            return ReturnValue;
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        /// <exception cref="ArgumentNullException">Thrown when chars is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index or count is less than zero or index and count do not denote a valid range in chars or The resulting number of bytes is greater than the maximum number that can be returned as an integer.</exception>
        /// <exception cref="ArgumentException">Error detection is enabled, and chars contains an invalid sequence of characters. </exception>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null)
                throw new ArgumentNullException("chars");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            int maxCharCount = index + count;

            if (maxCharCount > chars.Length)
                throw new ArgumentOutOfRangeException("index and count", "index and count does not denote a valid range in chars.");

            long ReturnValue = 0;

            // Starting at charIndex (min 0), and going to charIndex + charCount (max chars.Length)
            for (; index < maxCharCount; index++)
            {
                // if it's null, skip it until end of line,
                // we only want the bytes, not the 0x00's
                // so skip it.
                if (chars[index] == '\u0000')
                    continue;
                // Assuming this char BEGINS the Auto-Translate Phrase.
                else if (chars[index] == StartMarker)
                {
                    // if it's <?> (3 bytes) 
                    // (ie <F> for fire element or <G> or <R> for ATPhrase arrows
                    if (((index + 2) < chars.Length) && ((chars[index + 2]) == EndMarker))
                    {
                        #region if it's 3 bytes <F> Fire, <G> Green ATPhrase arrow, etc
                        // Go to the F, G, R, etc to access the character
                        index++;

                        // locate said character in the MapIndex
                        int charMapIndex = Array.IndexOf(_0xEFmap, chars[index]);

                        // If unable to find in the 0xEF mapIndex
                        if (charMapIndex == -1)
                        {
                            // Copy it char for char <Z> would be <Z> in-game
                            byte[] start = ConvertToByte(UTF16ToFFXI(StartMarker));
                            byte[] mid = ConvertToByte(UTF16ToFFXI(chars[index]));
                            byte[] end = ConvertToByte(UTF16ToFFXI(EndMarker));

                            ReturnValue += start.Length;
                            ReturnValue += mid.Length;
                            ReturnValue += end.Length;
                        }
                        else  // else, convert it to the 0xEF byte for Elemental and Green/Red Arrows.
                        {
                            ReturnValue += 2; // 0xEF 0x1F etc
                        }
                        // Skip the EndMarker by going to it in the index
                        index++;
                        #endregion
                    }
                    // if it's an unknown UTF-16 character <####> (<0000> - <FFFF>)
                    else if (((index + 5) < chars.Length) && ((chars[index + 5]) == EndMarker))
                    {
                        #region If it's an unknown character (Undecodable)
                        ReturnValue += 2;
                        index += 5;
                        #endregion
                    }
                    // if it's <########> (10 bytes total) or <########|blahblah>
                    else if (((index + 9) < chars.Length) && (((chars[index + 9]) == EndMarker) || (chars[index + 9] == MiddleMarker)))
                    {
                        #region If it's an unknown byte group (Undecodable) or an AT Phrase
                        ReturnValue += 6;
                        index += 9;

                        // skip the rest, AT Phrases can be an unknown length
                        for (; ((chars[index] != EndMarker) && (index < maxCharCount)); index++) ;
                        #endregion
                    }
                    else  // if it's none of the above, just copy it, char for char.
                    {
                        ReturnValue += ConvertToByte(UTF16ToFFXI(chars[index])).Length;
                    }
                }
                else if ((chars[index] == '\u000a') || (chars[index] == '\u000d')) // shouldn't have \r\n's
                {
                    continue;
                }
                else
                {
                    ReturnValue += ConvertToByte(UTF16ToFFXI(chars[index])).Length;
                }
            }
            if (ReturnValue > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("return value", "The resulting number of bytes is greater than the maximum number that can be returned as an integer.");
            return (int)(ReturnValue);
        }
        #endregion

        #region GetCharCount() overloads
        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence of bytes.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        public override int GetCharCount(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "bytes is null.");

            if (bytes.Length <= 0)
                return 0;

            int ReturnValue = 0;

            try
            {
                ReturnValue = this.GetCharCount(bytes, 0, bytes.Length);
            }
            catch
            {
            }
            return ReturnValue;
        }

        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence of bytes.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index or count is less than zero or index and count do not denote a valid range in bytes.</exception>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "Parameter to FFXIEncoding.GetString cannot be null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index is less than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count is less than zero.");

            int maxByteCount = index + count;

            if (maxByteCount > bytes.Length)
                throw new ArgumentOutOfRangeException("index and count", "index and count do not denote a valid range in bytes.");

            if (bytes.Length <= 0)
                return 0;

            int ReturnValue = 0;

            string s = String.Empty;
            char nextChar = '\uFFFF';

            #region "Loop Through bytes[] Array, Translating 0x85 codes or Shift-JIS codes"
            for ( ; index < maxByteCount; index++)
            {
                if (bytes[index] == 0x00) break;
                switch (bytes[index])
                {
                    case 0xEF:
                        #region If it's a Weather or such Special Character
                        index++; // Skip this byte and the next for now.
                        if (index >= maxByteCount)
                            break;
                        else if ((bytes[index] >= 0x1F) && (bytes[index] <= 0x28))
                            ReturnValue += 3;
                            //s += String.Format("{0}{1}{2}", StartMarker, _0xEFmap[(uint)(bytes[index] - 0x1F)], EndMarker);
                        #endregion
                        break;
                    case 0xFD:
                        #region If it's an Auto-Translate Phrase Marker
                        index++;  // Skip this char.
                        // Must have enough bytes to support Phrase otherwise skip the char
                        if (index >= maxByteCount)
                            break;
                        else if ((index + 4 + 1) >= maxByteCount)
                            break;
                        // Must continue the phrase, if not, skip the whole attempt... FFXI would ;;
                        else if (bytes[index + 4] != 0xFD)
                        {
                            // item_counter will be incremented the fifth time upon returning to the loop
                            index += 4;
                            break;
                        }
                        else
                        {
                            ReturnValue += 2;
                            for (int i = 0; i < 4; i++, index++)
                            //for ( ; bytes[index] != 0xFD; index++)
                            {
                                //s += String.Format("{0:X2}", (uint)bytes[index]);
                                ReturnValue += 2;
                            }
                            if (bytes[index] != 0xFD)
                                { /* ERROR ?!?!?!?! */ }
                        }
                        #endregion
                        break;
                    default:
                        #region If it's any regular character
                        if (this.IsSurrogate(bytes[index]))
                        {
                            nextChar = FFXIToUTF16((char)(((UInt32)(bytes[index] << 8)) + bytes[index + 1]));
                            if (nextChar == 0xFFFF)
                                ReturnValue += 6;//String.Format("{0}{1:X2}{2:X2}{3}", StartMarker,
                            //(UInt16)bytes[index], (UInt16)bytes[index + 1], EndMarker).Length;
                            else ReturnValue += 1; // String.Format("{0}", nextChar).Length;     // Else copy char
                            index++;
                        }
                        else
                        {
                            nextChar = FFXIToUTF16((char)(bytes[index]));
                            if (nextChar == 0xFFFF)
                                ReturnValue += 6; //String.Format("{0}00{1:X2}{2}", StartMarker,
                                    //(UInt16)bytes[index], EndMarker).Length;
                            else ReturnValue += 1; // String.Format("{0}", nextChar).Length;     // Else copy char
                        }
                        #endregion
                        break;
                }
            }
            #endregion

            return (ReturnValue);
        }
        #endregion
        #endregion

        #region GetMax????Count() overloads
        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <returns>The maximum number of bytes produced by encoding the specified number of characters.</returns>
        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw new ArgumentOutOfRangeException("charCount", "charCount is less than zero.");

            long ret = charCount * 6;
            if (ret > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("return value", "The resulting number of bytes is greater than the maximum number that can be returned as an integer.");

            return (int)ret;
        }

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <returns>The maximum number of characters produced by decoding the specified number of bytes.</returns>
        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount", "byteCount is less than zero.");
            long ret = byteCount * 6;
            if (ret > Int32.MaxValue)
                throw new ArgumentOutOfRangeException("return value", "The resulting number of bytes is greater than the maximum number that can be returned as an integer.");
            return (int)ret;
        }
        #endregion

        #region GetBytes() overloads
        /// <summary>
        /// Encodes all the characters in the specified character array into a sequence of bytes.
        /// </summary>
        /// <param name="chars">The character array containing the characters to encode. </param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        /// <exception cref="ArgumentNullException">If chars is null, throws ArgumentNullException.</exception>
        public override byte[] GetBytes(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "chars is null.");

            byte[] returnValue = new byte[0];

            if (chars.Length > 0)
            {
                try
                {
                    returnValue = GetBytes(chars, 0, chars.Length);
                }
                catch
                {
                    // ignore all.
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Encodes all the characters in the specified String into a sequence of bytes.
        /// </summary>
        /// <param name="s">The string containing the set of characters to encode.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        /// <exception cref="ArgumentNullException">If s is null, throws ArgumentNullException.</exception>
        public override byte[] GetBytes(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s", "s is null.");

            byte[] returnBytes = new byte[0];

            if (s != String.Empty)
            {
                try
                {
                    char[] charArray = s.ToCharArray();
                    returnBytes = this.GetBytes(charArray, 0, charArray.Length);
                }
                catch
                {
                    // ignore all others.
                }
            }
            return returnBytes;
        }

        /// <summary>
        /// Encodes a set of characters from the specified character array into a sequence of bytes.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="index">The index of the first character to encode.</param>
        /// <param name="count">The number of characters to encode.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters.</returns>
        /// <exception cref="ArgumentNullException">If chars is null, throws ArgumentNullException.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If index or count is less than zero, or if index and count does not denote a valid range in chars, throws ArgumentOutOfRangeException.</exception>
        public override byte[] GetBytes(char[] chars, int index, int count)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "chars is null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index is less than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count is less than zero.");
            if ((index + count) > chars.Length)
                throw new ArgumentOutOfRangeException("index + count", "index + count does not denote a valid range in chars.");

            byte[] bytes = new byte[GetByteCount(chars, index, count)];
            int newSize = -1;

            if (chars.Length > 0)
            {
                try
                {
                    newSize = GetBytes(chars, index, count, bytes, 0);
                }
                catch
                {

                }
                if (newSize < 0) // if GetBytes fail, send back a non-null bytes var
                    Array.Resize(ref bytes, 0);
                else if (newSize < bytes.Length) // else if < estimated Length, send back newSize
                    Array.Resize(ref bytes, newSize);
            }
            return bytes;
        }

        /// <summary>
        /// Encodes a set of characters from the specified character array into the specified byte array.
        /// </summary>
        /// <param name="chars">The character array containing the set of characters to encode.</param>
        /// <param name="charIndex">The index of the first character to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
        /// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
        /// <returns>The actual number of bytes written into bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when bytes or chars is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when charIndex or charCount do not denote a valid range in chars, or when byteIndex does not denote a valid range in bytes.</exception>
        /// <exception cref="ArgumentException">Thrown when bytes does not have enough capacity from byteIndex to the end of the array to accommodate the resulting bytes.</exception>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if ((chars == null) || (bytes == null))
                throw new ArgumentNullException("bytes or chars");
            if ((charIndex < 0) || (byteIndex < 0) || (charCount < 0))
                throw new ArgumentOutOfRangeException("charIndex, byteIndex, or charCount", "value is less than zero.");
            if (byteIndex > (bytes.Length - 1))
                throw new ArgumentOutOfRangeException("byteIndex", "byteIndex is not a valid index in bytes.");

            if (charIndex > (chars.Length - 1)) // index can't be equal to the length
                throw new ArgumentOutOfRangeException("charIndex", "charIndex does not denote a valid range in chars.");
            if (charCount > chars.Length)
                throw new ArgumentOutOfRangeException("charCount", "charCount does not denote a valid range in chars.");

            int maxCharCount = charIndex + charCount;

            if (maxCharCount > chars.Length)
                throw new ArgumentOutOfRangeException("charIndex + charCount", "charIndex + charCount does not denote a valid range in chars.");

            int ReturnValue = 0;

            byte[] convertedBytes;

            // Starting at charIndex (min 0), and going to charIndex + charCount (max chars.Length)
            for (; charIndex < maxCharCount; charIndex++)
            {
                // if it's null, skip it until end of line,
                // we only want the bytes, not the 0x00's
                // so skip it.
                if (chars[charIndex] == '\u0000')
                    continue;
                // Assuming this char BEGINS the Auto-Translate Phrase.
                else if (chars[charIndex] == StartMarker)
                {
                    // if it's <?> (3 bytes) 
                    // (ie <F> for fire element or <G> or <R> for ATPhrase arrows
                    if (((charIndex + 2) < chars.Length) && ((chars[charIndex + 2]) == EndMarker))
                    {
                        #region if it's 3 bytes <F> Fire, <G> Green ATPhrase arrow, etc
                        // Go to the F, G, R, etc to access the character
                        charIndex++;

                        // locate said character in the MapIndex
                        int charMapIndex = Array.IndexOf(_0xEFmap, chars[charIndex]);

                        // If unable to find in the 0xEF mapIndex
                        if (charMapIndex == -1)
                        {
                            // Copy it char for char <Z> would be <Z> in-game
                            byte[] start = ConvertToByte(UTF16ToFFXI(StartMarker));
                            byte[] mid = ConvertToByte(UTF16ToFFXI(chars[charIndex]));
                            byte[] end = ConvertToByte(UTF16ToFFXI(EndMarker));

                            convertedBytes = new byte[start.Length + mid.Length + end.Length];

                            int convertedBytesIndex = 0;

                            // Copy the byte value for the StartMarker
                            convertedBytes[convertedBytesIndex++] = start[0];
                            if (start.Length > 1)
                                convertedBytes[convertedBytesIndex++] = start[1];
                            // Copy the byte value for the Middle character
                            convertedBytes[convertedBytesIndex++] = mid[0];
                            if (mid.Length > 1)
                                convertedBytes[convertedBytesIndex++] = mid[1];
                            // Copy the byte value for the EndMarker
                            convertedBytes[convertedBytesIndex++] = end[0];
                            if (end.Length > 1)
                                convertedBytes[convertedBytesIndex++] = end[1];
                            // If the total characters copied are less than the Length
                            // Resize the array. (We really should never get here)
                            if (convertedBytesIndex < convertedBytes.Length)
                                Array.Resize(ref convertedBytes, convertedBytesIndex);
                        }
                        else  // else, convert it to the 0xEF byte for Elemental and Green/Red Arrows.
                        {
                            convertedBytes = new byte[2];
                            convertedBytes[0] = 0xEF;
                            convertedBytes[1] = (byte)(0x1F + charMapIndex);
                        }
                        // Skip the EndMarker by going to it in the index
                        charIndex++;
                        #endregion
                    }
                    // if it's an unknown UTF-16 character <####> (<0000> - <FFFF>)
                    else if (((charIndex + 5) < chars.Length) && ((chars[charIndex + 5]) == EndMarker))
                    {
                        #region If it's an unknown character (Undecodable)
                        string s;
                        convertedBytes = new byte[2];
                        // it's stored as a hex code <0000> - <FFFF>
                        s = String.Format("0x{0}{1}", chars[charIndex + 1], chars[charIndex + 2]);
                        convertedBytes[0] = System.Convert.ToByte(s, 16);
                        s = String.Format("0x{0}{1}", chars[charIndex + 3], chars[charIndex + 4]);
                        convertedBytes[1] = System.Convert.ToByte(s, 16);
                        charIndex += 5;
                        #endregion
                    }
                    // if it's <########> (10 bytes total) or <########|blahblah>
                    else if (((charIndex + 9) < chars.Length) && (((chars[charIndex + 9]) == EndMarker) || (chars[charIndex + 9] == MiddleMarker)))
                    {
                        #region If it's an unknown byte group (Undecodable) or an AT Phrase
                        string s;
                        convertedBytes = new byte[6];

                        convertedBytes[0] = 0xFD;
                        s = String.Format("0x{0}{1}", chars[charIndex + 1], chars[charIndex + 2]);
                        convertedBytes[1] = System.Convert.ToByte(s, 16);
                        s = String.Format("0x{0}{1}", chars[charIndex + 3], chars[charIndex + 4]);
                        convertedBytes[2] = System.Convert.ToByte(s, 16);
                        s = String.Format("0x{0}{1}", chars[charIndex + 5], chars[charIndex + 6]);
                        convertedBytes[3] = System.Convert.ToByte(s, 16);
                        s = String.Format("0x{0}{1}", chars[charIndex + 7], chars[charIndex + 8]);
                        convertedBytes[4] = System.Convert.ToByte(s, 16);
                        convertedBytes[5] = 0xFD;
                        charIndex += 9;

                        // skip the rest, AT Phrases can be an unknown length
                        for (; ((chars[charIndex] != EndMarker) && (charIndex < maxCharCount)); charIndex++) ;
                        #endregion
                    }
                    else  // if it's none of the above, just copy it, char for char.
                    {
                        convertedBytes = ConvertToByte(UTF16ToFFXI(chars[charIndex]));
                    }
                }
                else if ((chars[charIndex] == '\u000a') || (chars[charIndex] == '\u000d')) // shouldn't have \r\n's
                {
                    continue;
                }
                else
                {
                    convertedBytes = ConvertToByte(UTF16ToFFXI(chars[charIndex]));
                }

                if ((byteIndex + convertedBytes.Length) > bytes.Length)
                    throw new ArgumentException("bytes", "bytes does not have enough capacity from byteIndex to the end of the array to accommodate the resulting bytes.");
                else // if ((byteIndex + convertedBytes.Length) < bytes.Length)
                {
                    foreach (byte b in convertedBytes)
                        bytes[byteIndex++] = b;
                    ReturnValue += convertedBytes.Length;
                }
            }
            return (ReturnValue);
        }

        /// <summary>
        /// Encodes a set of characters from the specified String into the specified byte array.
        /// </summary>
        /// <param name="s">The string containing the set of characters to encode.</param>
        /// <param name="charIndex">The index of the first character to encode.</param>
        /// <param name="charCount">The number of characters to encode.</param>
        /// <param name="bytes">The byte array to contain the resulting sequence of bytes.</param>
        /// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes.</param>
        /// <returns>The actual number of bytes written into bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when s is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when charIndex or charCount do not denote a valid range in chars, or when byteIndex does not denote a valid range in bytes.</exception>
        /// <exception cref="ArgumentException">Thrown when bytes does not have enough capacity from byteIndex to the end of the array to accommodate the resulting bytes.</exception>
        public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            int returnValue = 0;

            if (s == null)
                throw new ArgumentNullException("s", "s is null.");

            if (s != String.Empty)
            {
                try
                {
                    returnValue = GetBytes(s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
                }
                catch
                {
                    throw;
                }
            }
            return returnValue;
        }
        #endregion

        #region GetChars() overloads
        /// <summary>
        /// Decodes a sequence of bytes into a set of characters.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index in bytes to starting decoding at.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>A character array containing the results of decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index or count is less than zero or index and count do not denote a valid range in bytes.</exception>
        public override char[] GetChars(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "bytes is null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index is less than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count is less than zero.");
            if ((index + count) > bytes.Length)
                throw new ArgumentOutOfRangeException("index and count", "index and count does not denote a valid range in bytes.");

            string s = String.Empty;

            if ((bytes[0] == 0x00) || (bytes.Length == 0))
                return s.ToCharArray();

            try
            {
                s = this.GetString(bytes, index, count);
            }
            catch
            {
            }

            return (s.ToCharArray());
        }

        /// <summary>
        /// Decodes a sequence of bytes into a set of characters.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <returns>A character array containing the results of decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        public override char[] GetChars(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "bytes is null.");

            string s = String.Empty;

            if ((bytes[0] == 0x00) || (bytes.Length == 0))
                return s.ToCharArray();

            try
            {
                s = this.GetString(bytes, 0, bytes.Length);
            }
            catch
            {
            }

            return (s.ToCharArray());
        }

        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="byteIndex">The index of the first byte to decode.</param>
        /// <param name="byteCount">The number of bytes to decode.</param>
        /// <param name="chars">The character array to contain the resulting set of characters.</param>
        /// <param name="charIndex">The index at which to start writing the resulting set of characters.</param>
        /// <returns>The actual number of characters written into chars.</returns>
        /// <exception cref="ArgumentNullException">Thrown if chars or bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when byteIndex or byteCount or charIndex is less than zero, byteindex and byteCount do not denote a valid range in bytes, or charIndex is not a valid index in chars.</exception>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "chars is null.");
            if (bytes == null)
                throw new ArgumentNullException("bytes", "bytes is null.");
            if (byteIndex < 0)
                throw new ArgumentOutOfRangeException("byteIndex", "byteIndex is less than zero.");
            if (charIndex < 0)
                throw new ArgumentOutOfRangeException("charIndex", "charIndex is less than zero.");
            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount", "byteCount is less than zero.");
            if (charIndex >= chars.Length)
                throw new ArgumentOutOfRangeException("charIndex", "charIndex is not a valid index in chars.");

            int maxByteCount = byteIndex + byteCount;

            if (maxByteCount > bytes.Length)
                throw new ArgumentOutOfRangeException("byteIndex and byteCount", "byteindex and byteCount do not denote a valid range in bytes.");

            string s = String.Empty;

            try
            {
                s = this.GetString(bytes, byteIndex, byteCount);
            }
            catch
            {
            }

            char[] charArray = s.ToCharArray();
            if ((charIndex + charArray.Length) > chars.Length)
                throw new ArgumentException("chars", "chars does not have enough capacity from charIndex to the end of the array to accommodate the resulting characters.");

            int ReturnValue = 0;

            foreach (char c in charArray)
            {
                chars[ReturnValue + charIndex] = c;
                ReturnValue++;
            }
            return ReturnValue;
        }
        #endregion

        #region GetString() overloads
        /// <summary>
        /// Decodes a sequence of bytes into a string.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="index">The index of the first byte to decode.</param>
        /// <param name="count">The number of bytes to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index or count is less than zero or index and count do not denote a valid range in bytes.</exception>
        public override string GetString(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "Parameter to FFXIEncoding.GetString cannot be null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index is less than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count is less than zero.");

            int maxByteCount = index + count;

            if (maxByteCount > bytes.Length)
                throw new ArgumentOutOfRangeException("index and count", "index and count do not denote a valid range in bytes.");

            string s = String.Empty;

            if (bytes.Length <= 0)
                return s;

            char nextChar = '\uFFFF';

            #region "Loop Through bytes[] Array, Translating 0x85 codes or Shift-JIS codes"
            for (; index < maxByteCount; index++)
            {
                if (bytes[index] == 0x00) break;
                switch (bytes[index])
                {
                    case 0xEF:
                        #region If it's a Weather or such Special Character
                        index++; // Skip this byte and the next for now.
                        if (index >= maxByteCount)
                            break;
                        else if ((bytes[index] >= 0x1F) && (bytes[index] <= 0x28))
                            s += String.Format("{0}{1}{2}", StartMarker, _0xEFmap[(uint)(bytes[index] - 0x1F)], EndMarker);
                        #endregion
                        break;
                    case 0xFD:
                        #region If it's an Auto-Translate Phrase Marker
                        index++;  // Skip this char.
                        // Must have enough bytes to support Phrase otherwise skip the char
                        if (index >= maxByteCount)
                            break;
                        else if ((index + 4 + 1) >= maxByteCount)
                            break;
                        // Must continue the phrase, if not, skip the whole attempt... FFXI would ;;
                        else if (bytes[index + 4] != 0xFD)
                        {
                            // item_counter will be incremented the fifth time upon returning to the loop
                            index += 4;
                            break;
                        }
                        else
                        {
                            //throw InvalidAutoTranslatePhraseException("Auto-Translate Phrase Not Found, Skipping.");
                            s += StartMarker;
                            /*
                            byte language_byte = bytes[index + 1];
                            byte resource_byte = bytes[index];
                            if (bytes[index] == 0x04)
                                bytes[index] = 0x02;
                            FFXIATPhrase atp = this.GetPhraseByID(bytes[index], (byte)this.LanguagePreference, bytes[index + 2], bytes[index + 3]);
                            */
                            for (int i = 0; i < 4; i++, index++)
                            //for ( ; bytes[index] != 0xFD; index++)
                            {
                                //s += String.Format("{0:X2}", (uint)bytes[index]);
                                s += String.Format("{0:X2}", (uint)bytes[index]);
                            }
                            if (bytes[index] != 0xFD)
                            { /* ERROR ?!?!?!?! */ }

                            /*
                            s += MidMarker;
                            if (atp == null) // not found
                            {
                                s += "UNKNOWN"; // Add autotranslate support here.
                            }
                            else
                            {
                                //if ((language_byte == 0x01) && ((atp.shortvalue != String.Empty) &&
                                //    (atp.shortvalue != null)))
                                //    s += atp.shortvalue.Trim('\0');
                                //else
                                s += atp.value.Trim('\0');
                            }*/
                            s += EndMarker;
                        }
                        #endregion
                        break;
                    default:
                        #region If it's any regular character
                        if (this.IsSurrogate(bytes[index]))
                        {
                            nextChar = FFXIToUTF16((char)(((UInt32)(bytes[index] << 8)) + bytes[index + 1]));
                            if (nextChar == 0xFFFF)
                                s += String.Format("{0}{1:X2}{2:X2}{3}", StartMarker,
                                 (UInt16)bytes[index], (UInt16)bytes[index + 1], EndMarker);           // Setup for conversion BACK later
                            else s += nextChar;     // Else copy char
                            // Add 1 to i to skip the second character
                            index++;
                        }
                        else
                        {
                            nextChar = FFXIToUTF16((char)(bytes[index]));
                            if (nextChar == 0xFFFF)
                                s += String.Format("{0}00{1:X2}{2}", StartMarker,
                                    (UInt16)bytes[index], EndMarker);
                            else s += nextChar;     // Else copy char
                        }
                        #endregion
                        break;
                }
            }

            #endregion

            return (s);
        }
        
        /// <summary>
        /// Decodes a sequence of bytes into a string. 
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if bytes is null.</exception>
        public override string GetString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes", "Parameter to FFXIEncoding.GetString cannot be null.");

            string s = String.Empty;

            if (bytes.Length <= 0)
                return s;

            try
            {
                s = GetString(bytes, 0, bytes.Length);
            }
            catch
            {
            }

            return (s);
        }
        
        #endregion
        #endregion

        #region FFXIEncoding Constructor
        /// <summary>
        /// Initializes a new instance of the Yekyaa.FFXIEncoding.FFXIEncoding class.
        /// </summary>
        public FFXIEncoding()
        {
        }
        #endregion
    }
}
