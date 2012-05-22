using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
namespace Smoothget.Mkv {
  public enum CodecID {
    V_AVC,
    V_MS,
    A_AAC,
    A_MS
  }

  // Integer values correspond to the return values of GetVIntForTrackType.
  public enum TrackType {
    Video = 1,
    Audio = 2,
    Complex = 3,
    Logo = 16,
    Subtitle = 17,
    Buttons = 18,
    Control = 32,
  }

  public struct CuePoint {
    private ulong CueTime;
    private ulong CueTrack;
    public ulong CueClusterPosition;
    public CuePoint(ulong cueTime, ulong cueTrack, ulong cueClusterPosition) {
      this.CueTime = cueTime;
      this.CueTrack = cueTrack;
      this.CueClusterPosition = cueClusterPosition;
    }
    public byte[] GetBytes() {
      // TODO: Do this with fewer temporary arrays.
      return MkvUtils.GetEEBytes(ID.CuePoint, Utils.CombineBytes(
          MkvUtils.GetEEBytes(ID.CueTime, MkvUtils.GetVintBytes(this.CueTime)),
          MkvUtils.GetEEBytes(ID.CueTrackPositions, Utils.CombineBytes(
              MkvUtils.GetEEBytes(ID.CueTrack, MkvUtils.GetVintBytes(this.CueTrack)),
              MkvUtils.GetEEBytes(ID.CueClusterPosition, MkvUtils.GetVintBytes(this.CueClusterPosition))))));
    }
  }

  // Integer values correspond to GetBytesForID: MkvUtils.GetDataSizeBytes((ulong)id).
  public enum ID {
    EBML = 172351395,
    EBMLVersion = 646,
    EBMLReadVersion = 759,
    EBMLMaxIDLength = 754,
    EBMLMaxSizeLength = 755,
    DocType = 642,
    DocTypeVersion = 647,
    DocTypeReadVersion = 645,
    Void = 108,
    Segment = 139690087,
    SeekHead = 21863284,
    Seek = 3515,
    SeekID = 5035,
    SeekPosition = 5036,
    Info = 88713574,
    SegmentUID = 13220,
    TimecodeScale = 710577,
    Duration = 1161,
    DateUTC = 1121,
    MuxingApp = 3456,
    WritingApp = 5953,
    Cluster = 256095861,
    Timecode = 103,
    SimpleBlock = 35,
    Tracks = 106212971,
    TrackEntry = 46,
    TrackNumber = 87,
    TrackUID = 13253,
    TrackType = 3,
    FlagEnabled = 57,
    FlagDefault = 8,
    FlagForced = 5546,
    FlagLacing = 28,
    Name = 4974,
    Language = 177564,
    CodecID = 6,
    CodecPrivate = 9122,
    Video = 96,
    FlagInterlaced = 26,
    PixelWidth = 48,
    PixelHeight = 58,
    DisplayWidth = 5296,
    DisplayHeight = 5306,
    Audio = 97,
    SamplingFrequency = 53,
    Channels = 31,
    BitDepth = 8804,
    Cues = 206814059,
    CuePoint = 59,
    CueTime = 51,
    CueTrackPositions = 55,
    CueTrack = 119,
    CueClusterPosition = 113,
  }

  // See also TrackEntry.LanguageCodes.
  // Please don't change the order of the names here, because TrackEntry.LanguageCodes corresponds to it.
  public enum LanguageID {
    Abkhazian,
    Achinese,
    Acoli,
    Adangme,
    Adygei,
    Adyghe,
    Afar,
    Afrihili,
    Afrikaans,
    AfroAsiaticLanguages,
    Ainu,
    Akan,
    Akkadian,
    Albanian,
    Alemannic,
    Aleut,
    AlgonquianLanguages,
    Alsatian,
    AltaicLanguages,
    Amharic,
    Angika,
    ApacheLanguages,
    Arabic,
    Aragonese,
    Arapaho,
    Arawak,
    Armenian,
    Aromanian,
    ArtificialLanguages,
    Arumanian,
    Assamese,
    Asturian,
    Asturleonese,
    AthapascanLanguages,
    AustralianLanguages,
    AustronesianLanguages,
    Avaric,
    Avestan,
    Awadhi,
    Aymara,
    Azerbaijani,
    Bable,
    Balinese,
    BalticLanguages,
    Baluchi,
    Bambara,
    BamilekeLanguages,
    BandaLanguages,
    BantuLanguages,
    Basa,
    Bashkir,
    Basque,
    BatakLanguages,
    Bedawiyet,
    Beja,
    Belarusian,
    Bemba,
    Bengali,
    BerberLanguages,
    Bhojpuri,
    BihariLanguages,
    Bikol,
    Bilin,
    Bini,
    Bislama,
    Blin,
    Bliss,
    Blissymbolics,
    Blissymbols,
    BokmålNorwegian,
    Bosnian,
    Braj,
    Breton,
    Buginese,
    Bulgarian,
    Buriat,
    Burmese,
    Caddo,
    Castilian,
    Catalan,
    CaucasianLanguages,
    Cebuano,
    CelticLanguages,
    CentralAmericanIndianLanguages,
    CentralKhmer,
    Chagatai,
    ChamicLanguages,
    Chamorro,
    Chechen,
    Cherokee,
    Chewa,
    Cheyenne,
    Chibcha,
    Chichewa,
    Chinese,
    Chinookjargon,
    Chipewyan,
    Choctaw,
    Chuang,
    ChurchSlavic,
    ChurchSlavonic,
    Chuukese,
    Chuvash,
    ClassicalNepalBhasa,
    ClassicalNewari,
    ClassicalSyriac,
    CookIslandsMaori,
    Coptic,
    Cornish,
    Corsican,
    Cree,
    Creek,
    CreolesAndPidgins,
    CreolesAndPidginsEnglishBased,
    CreolesAndPidginsFrenchBased,
    CreolesAndPidginsPortugueseBased,
    CrimeanTatar,
    CrimeanTurkish,
    Croatian,
    CushiticLanguages,
    Czech,
    Dakota,
    Danish,
    Dargwa,
    Delaware,
    DeneSuline,
    Dhivehi,
    Dimili,
    Dimli,
    Dinka,
    Divehi,
    Dogri,
    Dogrib,
    DravidianLanguages,
    Duala,
    Dutch,
    DutchMiddle,
    Dyula,
    Dzongkha,
    EasternFrisian,
    Edo,
    Efik,
    Egyptian,
    Ekajuk,
    Elamite,
    English,
    EnglishMiddle,
    EnglishOld,
    Erzya,
    Esperanto,
    Estonian,
    Ewe,
    Ewondo,
    Fang,
    Fanti,
    Faroese,
    Fijian,
    Filipino,
    Finnish,
    FinnoUgrianLanguages,
    Flemish,
    Fon,
    French,
    FrenchMiddle,
    FrenchOld,
    Friulian,
    Fulah,
    Ga,
    Gaelic,
    GalibiCarib,
    Galician,
    Ganda,
    Gayo,
    Gbaya,
    Geez,
    Georgian,
    German,
    GermanLow,
    GermanMiddleHigh,
    GermanOldHigh,
    GermanicLanguages,
    Gikuyu,
    Gilbertese,
    Gondi,
    Gorontalo,
    Gothic,
    Grebo,
    GreekAncient,
    GreekModern,
    Greenlandic,
    Guarani,
    Gujarati,
    Gwichin,
    Haida,
    Haitian,
    HaitianCreole,
    Hausa,
    Hawaiian,
    Hebrew,
    Herero,
    Hiligaynon,
    HimachaliLanguages,
    Hindi,
    HiriMotu,
    Hittite,
    Hmong,
    Hungarian,
    Hupa,
    Iban,
    Icelandic,
    Ido,
    Igbo,
    IjoLanguages,
    Iloko,
    ImperialAramaic,
    InariSami,
    IndicLanguages,
    IndoEuropeanLanguages,
    Indonesian,
    Ingush,
    Interlingua,
    Interlingue,
    Inuktitut,
    Inupiaq,
    IranianLanguages,
    Irish,
    IrishMiddle,
    IrishOld,
    IroquoianLanguages,
    Italian,
    Japanese,
    Javanese,
    Jingpho,
    JudeoArabic,
    JudeoPersian,
    Kabardian,
    Kabyle,
    Kachin,
    Kalaallisut,
    Kalmyk,
    Kamba,
    Kannada,
    Kanuri,
    Kapampangan,
    KaraKalpak,
    KarachayBalkar,
    Karelian,
    KarenLanguages,
    Kashmiri,
    Kashubian,
    Kawi,
    Kazakh,
    Khasi,
    KhoisanLanguages,
    Khotanese,
    Kikuyu,
    Kimbundu,
    Kinyarwanda,
    Kirdki,
    Kirghiz,
    Kirmanjki,
    Klingon,
    Komi,
    Kongo,
    Konkani,
    Korean,
    Kosraean,
    Kpelle,
    KruLanguages,
    Kuanyama,
    Kumyk,
    Kurdish,
    Kurukh,
    Kutenai,
    Kwanyama,
    Kyrgyz,
    Ladino,
    Lahnda,
    Lamba,
    LandDayakLanguages,
    Lao,
    Latin,
    Latvian,
    Leonese,
    Letzeburgesch,
    Lezghian,
    Limburgan,
    Limburger,
    Limburgish,
    Lingala,
    Lithuanian,
    Lojban,
    LowGerman,
    LowSaxon,
    LowerSorbian,
    Lozi,
    LubaKatanga,
    LubaLulua,
    Luiseno,
    LuleSami,
    Lunda,
    Luo,
    Lushai,
    Luxembourgish,
    MacedoRomanian,
    Macedonian,
    Madurese,
    Magahi,
    Maithili,
    Makasar,
    Malagasy,
    Malay,
    Malayalam,
    Maldivian,
    Maltese,
    Manchu,
    Mandar,
    Mandingo,
    Manipuri,
    ManoboLanguages,
    Manx,
    Maori,
    Mapuche,
    Mapudungun,
    Marathi,
    Mari,
    Marshallese,
    Marwari,
    Masai,
    MayanLanguages,
    Mende,
    Mikmaq,
    Micmac,
    Minangkabau,
    Mirandese,
    Mohawk,
    Moksha,
    Moldavian,
    Moldovan,
    MonKhmerLanguages,
    Mong,
    Mongo,
    Mongolian,
    Mossi,
    MultipleLanguages,
    MundaLanguages,
    NKo,
    NahuatlLanguages,
    Nauru,
    Navaho,
    Navajo,
    NdebeleNorth,
    NdebeleSouth,
    Ndonga,
    Neapolitan,
    NepalBhasa,
    Nepali,
    Newari,
    Nias,
    NigerKordofanianLanguages,
    NiloSaharanLanguages,
    Niuean,
    Nolinguisticcontent,
    Nogai,
    NorseOld,
    NorthAmericanIndianLanguages,
    NorthNdebele,
    NorthernFrisian,
    NorthernSami,
    NorthernSotho,
    Norwegian,
    NorwegianBokmål,
    NorwegianNynorsk,
    Notapplicable,
    NubianLanguages,
    Nuosu,
    Nyamwezi,
    Nyanja,
    Nyankole,
    NynorskNorwegian,
    Nyoro,
    Nzima,
    Occidental,
    Occitan,
    OccitanOld,
    OfficialAramaic,
    Oirat,
    Ojibwa,
    OldBulgarian,
    OldChurchSlavonic,
    OldNewari,
    OldSlavonic,
    Oriya,
    Oromo,
    Osage,
    Ossetian,
    Ossetic,
    OtomianLanguages,
    Pahlavi,
    Palauan,
    Pali,
    Pampanga,
    Pangasinan,
    Panjabi,
    Papiamento,
    PapuanLanguages,
    Pashto,
    Pedi,
    Persian,
    PersianOld,
    PhilippineLanguages,
    Phoenician,
    Pilipino,
    Pohnpeian,
    Polish,
    Portuguese,
    PrakritLanguages,
    ProvençalOld,
    Punjabi,
    Pushto,
    Quechua,
    Rajasthani,
    Rapanui,
    Rarotongan,
    ReservedForLocalUse,
    RomanceLanguages,
    Romanian,
    Romansh,
    Romany,
    Rundi,
    Russian,
    Sakan,
    SalishanLanguages,
    SamaritanAramaic,
    SamiLanguages,
    Samoan,
    Sandawe,
    Sango,
    Sanskrit,
    Santali,
    Sardinian,
    Sasak,
    SaxonLow,
    Scots,
    ScottishGaelic,
    Selkup,
    SemiticLanguages,
    Sepedi,
    Serbian,
    Serer,
    Shan,
    Shona,
    SichuanYi,
    Sicilian,
    Sidamo,
    SignLanguages,
    Siksika,
    Sindhi,
    Sinhala,
    Sinhalese,
    SinoTibetanLanguages,
    SiouanLanguages,
    SkoltSami,
    Slave,
    SlavicLanguages,
    Slovak,
    Slovenian,
    Sogdian,
    Somali,
    SonghaiLanguages,
    Soninke,
    SorbianLanguages,
    SothoNorthern,
    SothoSouthern,
    SouthAmericanIndianLanguages,
    SouthNdebele,
    SouthernAltai,
    SouthernSami,
    Spanish,
    SrananTongo,
    Sukuma,
    Sumerian,
    Sundanese,
    Susu,
    Swahili,
    Swati,
    Swedish,
    SwissGerman,
    Syriac,
    Tagalog,
    Tahitian,
    TaiLanguages,
    Tajik,
    Tamashek,
    Tamil,
    Tatar,
    Telugu,
    Tereno,
    Tetum,
    Thai,
    Tibetan,
    Tigre,
    Tigrinya,
    Timne,
    Tiv,
    tlhInganHol,
    Tlingit,
    TokPisin,
    Tokelau,
    TongaNyasa,
    TongaTongaIslands,
    Tsimshian,
    Tsonga,
    Tswana,
    Tumbuka,
    TupiLanguages,
    Turkish,
    TurkishOttoman,
    Turkmen,
    Tuvalu,
    Tuvinian,
    Twi,
    Udmurt,
    Ugaritic,
    Uighur,
    Ukrainian,
    Umbundu,
    UncodedLanguages,
    Undetermined,
    UpperSorbian,
    Urdu,
    Uyghur,
    Uzbek,
    Vai,
    Valencian,
    Venda,
    Vietnamese,
    Volapük,
    Votic,
    WakashanLanguages,
    Walloon,
    Waray,
    Washo,
    Welsh,
    WesternFrisian,
    WesternPahariLanguages,
    Wolaitta,
    Wolaytta,
    Wolof,
    Xhosa,
    Yakut,
    Yao,
    Yapese,
    Yiddish,
    Yoruba,
    YupikLanguages,
    ZandeLanguages,
    Zapotec,
    Zaza,
    Zazaki,
    Zenaga,
    Zhuang,
    Zulu,
    Zuni
  }

  public class TrackEntry {
    // `private const string' would increase the .exe size by 5 kB here.
    private static readonly string LanguageCodes = "abkaceachadaadyadyaarafhafrafaainakaakkalbgswalealggswtutamhanpapaaraargarparwarmrupartrupasmastastathausmapavaaveawaaymazeastbanbatbalbambaibadbntbasbakbaqbtkbejbejbelbembenberbhobihbikbynbinbisbynzblzblzblnobbosbrabrebugbulbuaburcadspacatcaucebcelcaikhmchgcmcchachechrnyachychbnyachichnchpchozhachuchuchkchvnwcnwcsycrarcopcorcoscremuscrpcpecpfcppcrhcrhhrvcusczedakdandardelchpdivzzazzadindivdoidgrdraduadutdumdyudzofrsbinefiegyekaelxengenmangmyvepoesteweewofanfatfaofijfilfinfiudutfonfrefrmfrofurfulgaaglacarglgluggaygbagezgeogerndsgmhgohgemkikgilgongorgotgrbgrcgrekalgrngujgwihaihathathauhawhebherhilhimhinhmohithmnhunhupibaiceidoiboijoiloarcsmnincineindinhinaileikuipkiraglemgasgairoitajpnjavkacjrbjprkbdkabkackalxalkamkankaupamkaakrckrlkarkascsbkawkazkhakhikhokikkmbkinzzakirzzatlhkomkonkokkorkoskpekrokuakumkurkrukutkuakirladlahlamdaylaolatlavastltzlezlimlimlimlinlitjbondsndsdsblozlublualuismjlunluolusltzrupmacmadmagmaimakmlgmaymaldivmltmncmdrmanmnimnoglvmaoarnarnmarchmmahmwrmasmynmenmicmicminmwlmohmdfrumrummkhhmnlolmonmosmulmunnqonahnaunavnavndenblndonapnewnepnewnianicssaniuzxxnognonnaindefrrsmensonornobnnozxxnubiiinymnyanynnnonyonziileociproarcxalojichuchunwcchuoriormosaossossotopalpauplipampagpanpappaapusnsoperpeophiphnfilponpolporprapropanpusquerajraprarqaaroarumrohromrunruskhosalsamsmismosadsagsansatsrdsasndsscoglaselsemnsosrpsrrshnsnaiiiscnsidsgnblasndsinsinsitsiosmsdenslasloslvsogsomsonsnkwennsosotsainblaltsmaspasrnsuksuxsunsusswasswswegswsyrtgltahtaitgktmhtamtatteltertetthatibtigtirtemtivtlhtlitpitkltogtontsitsotsntumtupturotatuktvltyvtwiudmugauigukrumbmisundhsburduiguzbvaicatvenvievolvotwakwlnwarwaswelfryhimwalwalwolxhosahyaoyapyidyorypkzndzapzzazzazenzhazulzun";
    private static string GetLanguageCode(LanguageID id) {
      int i = (int)id;
      int i3 = i * 3;
      if (i < 0 || i3 >= LanguageCodes.Length) {
        throw new Exception(string.Format("LanguageID '{0}' is unsupported!", id));
      }
      return LanguageCodes.Substring(i3, 3);
    }

    // TODO: Change public to private.
    public ulong TrackNumber;
    public TrackType TrackType;
    public string Name;
    public LanguageID Language = LanguageID.English;
    public CodecID CodecID;
    private byte[] CodecPrivate;
    private byte[] InfoBytes;
    public TrackEntry(TrackType trackType, byte[] infoBytes, CodecID codecID, byte[] codecPrivate) {
      this.TrackType = trackType;
      this.InfoBytes = infoBytes;
      this.CodecID = codecID;
      this.CodecPrivate = codecPrivate;
    }
    public byte[] GetBytes() {
      if (this.TrackNumber == 0uL) {
        throw new Exception("TrackNumber must be greater than 0!");
      }
      ulong trackUID = this.TrackNumber;
      if (trackUID == 0uL) {
        throw new Exception("TrackUID must be greater than 0!");
      }
      List<byte[]> list = new List<byte[]>();
      list.Add(MkvUtils.GetEEBytes(ID.TrackNumber, MkvUtils.GetVintBytes(this.TrackNumber)));
      list.Add(MkvUtils.GetEEBytes(ID.TrackUID, MkvUtils.GetVintBytes(trackUID)));
      list.Add(MkvUtils.GetEEBytes(ID.TrackType, MkvUtils.GetVintBytes((ulong)this.TrackType)));
      list.Add(MkvUtils.GetEEBytes(ID.FlagEnabled, MkvUtils.GetVIntForFlag(true)));
      list.Add(MkvUtils.GetEEBytes(ID.FlagDefault, MkvUtils.GetVIntForFlag(true)));
      list.Add(MkvUtils.GetEEBytes(ID.FlagForced, MkvUtils.GetVIntForFlag(false)));
      list.Add(MkvUtils.GetEEBytes(ID.FlagLacing, MkvUtils.GetVIntForFlag(true)));
      if (!string.IsNullOrEmpty(this.Name)) {
        list.Add(MkvUtils.GetEEBytes(ID.Name, Encoding.UTF8.GetBytes(this.Name)));
      }
      if (this.Language != LanguageID.English) {
        list.Add(MkvUtils.GetEEBytes(ID.Language, Encoding.ASCII.GetBytes(GetLanguageCode(this.Language))));
      }
      list.Add(MkvUtils.GetEEBytes(ID.CodecID, Encoding.ASCII.GetBytes(MkvUtils.GetStringForCodecID(this.CodecID))));
      if (this.CodecPrivate != null) {
        list.Add(MkvUtils.GetEEBytes(ID.CodecPrivate, this.CodecPrivate));
      }
      list.Add(this.InfoBytes);
      return MkvUtils.GetEEBytes(ID.TrackEntry, Utils.CombineByteArrays(list));
    }
  }

  // This is a pure data class, please don't add logic.
  public class MediaDataBlock {
    // The media sample data bytes (i.e. a frame from the media file).
    public ArraySegment<byte> Bytes;
    public ulong StartTime;
    public bool IsKeyFrame;

    public MediaDataBlock(ArraySegment<byte> bytes, ulong startTime, bool isKeyFrame) {
      this.Bytes = bytes;
      this.StartTime = startTime;
      this.IsKeyFrame = isKeyFrame;
    }
  }

  public interface IChunkStartTimeReceiver {
    // Returns ulong.MaxValue if the information is not available (i.e. chunkIndex is too large).
    ulong GetChunkStartTime(int trackIndex, int chunkIndex);
    void SetChunkStartTime(int trackIndex, int chunkIndex, ulong chunkStartTime);
  }

  // A source (forward-iterator) of MediaDataBlock objects in a fixed number of parallel tracks.
  public interface IMediaDataSource {
    int GetTrackCount();
    // Called only when all chunks have been read (using ConsumeBlock).
    ulong GetTrackEndTime(int trackIndex);
    void StartChunks(IChunkStartTimeReceiver chunkStartTimeReceiver);
    // Returns the first MediaDataBlock (i.e. with smallest StartTime) unconsumed MediaDataBlock on the specified track, or
    // null on EOF on the specified track. The ownership of the returned
    // block is shared between the MediaDataSource and the caller until ConsumeBlock(trackIndex) is called. Afterwards the
    // the MediaDataSource releases ownership. The MediaDataSource never modifies the fields of the MediaDataBlock or the
    // contents of its .Bytes array it returns, and
    // it returns the same reference until ConsumeSample(trackIndex) is called, and a different reference afterwards.
    MediaDataBlock PeekBlock(int trackIndex);
    // Consumes the first unconsumed data block on the specified track. It's illegal to call this method if there are no
    // unconsumed MediaDataBlock objects left on the specified track.
    void ConsumeBlock(int trackIndex);
    // Consume all blocks with .StartTime <= startTime from the specified track.
    void ConsumeBlocksUntil(int trackIndex, ulong startTime);
  }

  public class MuxStateWriter {
    private Stream Stream;
    public MuxStateWriter(Stream stream) {
      this.Stream = stream;
    }
    public void Close() {
      this.Stream.Close();
    }
    public void Flush() {
      this.Stream.Flush();
    }
    // TODO: Use a binary format to save space (maybe 50% of the mux state file size).
    public void WriteUlong(char key, ulong num) {
      // TODO: Speed this up if necessary.
      byte[] outputBytes = Encoding.ASCII.GetBytes(("" + key) + num + '\n');
      this.Stream.Write(outputBytes, 0, outputBytes.Length);
    }
    public void WriteBytes(char key, byte[] bytes) {
      // TODO: Speed this up if necessary.
      byte[] outputBytes = Encoding.ASCII.GetBytes(key + ":" + Utils.HexEncodeString(bytes) + '\n');
      this.Stream.Write(outputBytes, 0, outputBytes.Length);
    }
    public void WriteRaw(byte[] bytes, int start, int end) {
      this.Stream.Write(bytes, start, end - start);
    }
  }

  public class MkvUtils {
    private const ulong DATA_SIZE_MAX_VALUE = 72057594037927934uL;
    public static byte[] GetDataSizeBytes(ulong value) {
      if (value > DATA_SIZE_MAX_VALUE) {
        throw new Exception(string.Format("Data size '{0}' is greater than its max value!", value));
      }
      byte[] bytes = BitConverter.GetBytes(value);
      Array.Reverse(bytes);
      int b = 1;
      while (value > (1uL << (7 * b)) - 2) {
        ++b;
      }
      byte[] array = new byte[b];
      Buffer.BlockCopy(bytes, 8 - b, array, 0, b);
      array[0] += (byte)(1 << (8 - b));
      return array;
    }
    // Like GetDataSizeBytes, but always returns the longest possible byte array (of size 8).
    private static byte[] GetDataSizeEightBytes(ulong value) {
      if (value > DATA_SIZE_MAX_VALUE) {
        throw new Exception(string.Format("Data size '{0}' is greater than its max value!", value));
      }
      byte[] bytes = BitConverter.GetBytes(value);
      Array.Reverse(bytes);
      bytes[0] = 1;
      return bytes;
    }
    private const ulong VINT_MAX_VALUE = 18446744073709551615uL;
    public static byte[] GetVintBytes(ulong value) {
      byte[] bytes = BitConverter.GetBytes(value);
      Array.Reverse(bytes);
      int b = 0;
      while (bytes[b] == 0 && b + 1 < bytes.Length) {
        ++b;
      }
      byte[] array = new byte[bytes.Length - b];
      for (int i = 0; i < array.Length; i++) {
        array[i] = bytes[b + i];
      }
      return array;
    }
    public static byte[] GetFloatBytes(float value) {
      return Utils.InplaceReverseBytes(BitConverter.GetBytes(value));
    }
    private static readonly DateTime MinDateTimeValue = DateTime.Parse("2001-01-01").ToUniversalTime();
    public static byte[] GetDateTimeBytes(DateTime dateTime) {
      DateTime dateTime2 = dateTime.ToUniversalTime();
      if (dateTime2 < MinDateTimeValue) {
        throw new Exception(string.Format("Date '{0}' is lower than its min value!", dateTime.ToShortDateString()));
      }
      return Utils.InplaceReverseBytes(BitConverter.GetBytes(Convert.ToUInt64(
          dateTime2.Subtract(MinDateTimeValue).TotalMilliseconds * 1000000.0)));
    }
    // Get EBML element bytes.
    public static byte[] GetEEBytes(ID id, byte[] contents) {
      return Utils.CombineBytes(GetDataSizeBytes((ulong)id),
                                GetDataSizeBytes((ulong)contents.Length),
                                contents);
    }
    private static byte[] GetEbmlHeaderBytes() {
      List<byte[]> list = new List<byte[]>();
      list.Add(GetEEBytes(ID.EBMLVersion, GetVintBytes(1uL)));
      list.Add(GetEEBytes(ID.EBMLReadVersion, GetVintBytes(1uL)));
      list.Add(GetEEBytes(ID.EBMLMaxIDLength, GetVintBytes(4uL)));
      list.Add(GetEEBytes(ID.EBMLMaxSizeLength, GetVintBytes(8uL)));
      list.Add(GetEEBytes(ID.DocType, Encoding.ASCII.GetBytes("matroska")));
      list.Add(GetEEBytes(ID.DocTypeVersion, GetVintBytes(1uL)));
      list.Add(GetEEBytes(ID.DocTypeReadVersion, GetVintBytes(1uL)));
      return GetEEBytes(ID.EBML, Utils.CombineByteArrays(list));
    }
    public static string GetStringForCodecID(CodecID codecID) {
      switch (codecID) {
        case CodecID.V_AVC: { return "V_MPEG4/ISO/AVC"; }
        case CodecID.V_MS: { return "V_MS/VFW/FOURCC"; }
        case CodecID.A_AAC: { return "A_AAC"; }
        case CodecID.A_MS: { return "A_MS/ACM"; }
        default: { throw new Exception(string.Format("CodecID '{0}' is invalid!", codecID)); }
      }
    }
    public static byte[] GetVideoInfoBytes(ulong pixelWidth, ulong pixelHeight, ulong displayWidth, ulong displayHeight) {
      if (pixelWidth == 0uL) {
        throw new Exception("PixelWidth must be greater than 0!");
      }
      if (pixelHeight == 0uL) {
        throw new Exception("PixelHeight must be greater than 0!");
      }
      if (displayWidth == 0uL) {
        throw new Exception("DisplayWidth must be greater than 0!");
      }
      if (displayHeight == 0uL) {
        throw new Exception("DisplayHeight must be greater than 0!");
      }
      List<byte[]> list = new List<byte[]>();
      list.Add(GetEEBytes(ID.FlagInterlaced, GetVIntForFlag(false)));
      list.Add(GetEEBytes(ID.PixelWidth, GetVintBytes(pixelWidth)));
      list.Add(GetEEBytes(ID.PixelHeight, GetVintBytes(pixelHeight)));
      if (displayWidth != pixelWidth) {
        list.Add(GetEEBytes(ID.DisplayWidth, GetVintBytes(displayWidth)));
      }
      if (displayHeight != pixelHeight) {
        list.Add(GetEEBytes(ID.DisplayHeight, GetVintBytes(displayHeight)));
      }
      return GetEEBytes(ID.Video, Utils.CombineByteArrays(list));
    }
    public static byte[] GetAudioInfoBytes(float samplingFrequency, ulong channels, ulong bitDepth) {
      if (samplingFrequency <= 0f) {
        throw new Exception("SamplingFrequency must be greater than 0!");
      }
      if (channels == 0uL) {
        throw new Exception("Channels cannot be 0!");
      }
      List<byte[]> list = new List<byte[]>();
      list.Add(GetEEBytes(ID.SamplingFrequency, GetFloatBytes(samplingFrequency)));
      list.Add(GetEEBytes(ID.Channels, GetVintBytes(channels)));
      if (bitDepth != 0uL) {
        list.Add(GetEEBytes(ID.BitDepth, GetVintBytes(bitDepth)));
      }
      return GetEEBytes(ID.Audio, Utils.CombineByteArrays(list));
    }
    private static readonly byte[] VINT_FALSE = new byte[] { 0 };
    private static readonly byte[] VINT_TRUE = new byte[] { 1 };
    public static byte[] GetVIntForFlag(bool flag) {
      return flag ? VINT_TRUE : VINT_FALSE;
    }
    private static byte[] GetDurationBytes(ulong duration, ulong timeScale) {
      float floatDuration = Convert.ToSingle(duration * 1000.0 / timeScale);
      if (floatDuration <= 0f) floatDuration = 0.125f;  // .mkv requires a positive duration.
      byte[] bytes = BitConverter.GetBytes(floatDuration);  // 4 bytes.
      Array.Reverse(bytes);
      return bytes;
    }

    private static byte[] GetSegmentInfoBytes(ulong duration, ulong timeScale, bool isDeterministic) {
      AssemblyName name = Assembly.GetEntryAssembly().GetName();
      string muxingApp = name.Name + " v" + name.Version;
      string writingApp = muxingApp;
      byte[] segmentUid;
      if (isDeterministic) {
        // 16 bytes; seemingly random, but deterministic.
        segmentUid = new byte[] {110, 104, 17, 204, 142, 130, 251, 240, 218, 112, 216, 160, 143, 114, 2, 237};
      } else {
        segmentUid = new byte[16];
        new Random().NextBytes(segmentUid);
      }
      List<byte[]> list = new List<byte[]>();
      // ID.Duration must be the first in the list so FindDurationOffset can find it.
      list.Add(GetEEBytes(ID.Duration, GetDurationBytes(duration, timeScale)));
      if (!string.IsNullOrEmpty(muxingApp)) {
        list.Add(GetEEBytes(ID.MuxingApp, Encoding.ASCII.GetBytes(muxingApp)));
      }
      if (!string.IsNullOrEmpty(writingApp)) {
        list.Add(GetEEBytes(ID.WritingApp, Encoding.ASCII.GetBytes(writingApp)));
      }
      list.Add(GetEEBytes(ID.SegmentUID, segmentUid));
      // The deterministic date was a few minutes before Tue Apr 17 21:14:22 CEST 2012.
      byte[] dateBytes = isDeterministic ? new byte[] {4, 242, 35, 97, 249, 143, 0, 192}
        : GetDateTimeBytes(DateTime.UtcNow);
      list.Add(GetEEBytes(ID.DateUTC, dateBytes));
      list.Add(GetEEBytes(ID.TimecodeScale, GetVintBytes(timeScale / 10uL)));
      return GetEEBytes(ID.Info, Utils.CombineByteArrays(list));
    }

    private static byte[] GetTrackEntriesBytes(IList<TrackEntry> trackEntries) {
      byte[][] byteArrays = new byte[trackEntries.Count][];
      for (int i = 0; i < trackEntries.Count; i++) {
        byteArrays[i] = trackEntries[i].GetBytes();
      }
      return GetEEBytes(ID.Tracks, Utils.CombineByteArrays(byteArrays));
    }

    // This is a pure data struct. Please don't add functionality.
    private struct SeekBlock {
      public ID ID;
      public ulong Offset;
      public SeekBlock(ID id, ulong offset) {
        this.ID = id;
        this.Offset = offset;
      }
    }

    private static byte[] GetVoidBytes(ulong length) {
      if (length < 9uL) {
        // >=9 == 1 byte for ID.Void, 8 bytes for the fixed length and >=0 bytes for the data.
        throw new Exception("Void must be greater than or equal to 9 bytes.");
      }
      length -= 9;
      return Utils.CombineBytes(GetDataSizeBytes((ulong)ID.Void),
                                GetDataSizeEightBytes(length),
                                new byte[length]);
    }

    private static byte[] GetSeekBytes(IList<SeekBlock> seekBlocks, int desiredSize) {
      int seekBlockCount = seekBlocks.Count;
      byte[][] byteArrays = new byte[4 * seekBlockCount + 3][];
      byteArrays[0] = GetDataSizeBytes((ulong)ID.SeekHead);
      for (int i = 0, j = 2; i < seekBlockCount; ++i, j += 4) {
        byteArrays[j] = GetDataSizeBytes((ulong)ID.Seek);
        byteArrays[j + 2] = GetEEBytes(ID.SeekID, GetDataSizeBytes((ulong)seekBlocks[i].ID));
        byteArrays[j + 3] = GetEEBytes(ID.SeekPosition, GetVintBytes(seekBlocks[i].Offset));
        byteArrays[j + 1] = GetDataSizeBytes((ulong)(byteArrays[j + 2].Length + byteArrays[j + 3].Length));
      }
      int dataSize = 0;
      int voidIndex = byteArrays.Length - 1;
      for (int i = 2; i < voidIndex; ++i) {
        dataSize += byteArrays[i].Length;
      }
      byteArrays[1] = GetDataSizeBytes((ulong)dataSize);
      byteArrays[voidIndex] = new byte[] {};
      if (desiredSize >= 0) {
        dataSize += byteArrays[0].Length + byteArrays[1].Length;
        if (desiredSize != dataSize) {
          if (desiredSize <= dataSize + 9) {
            throw new Exception("dataSize too small, got " + dataSize + ", expected <=" + (desiredSize - 9));
          }
          byteArrays[voidIndex] = GetVoidBytes((ulong)(desiredSize - dataSize));
        }
      }
      return Utils.CombineByteArrays(byteArrays);
    }

    private const int DESIRED_SEEK_SIZE = 90;

    private static byte[] GetSegmentBytes(ulong duration, ulong mediaEndOffsetMS,
                                    ulong seekHeadOffsetMS, ulong cuesOffsetMS,
                                          ulong timeScale, IList<TrackEntry> trackEntries, bool isDeterministic) {
      byte[][] byteArrays = new byte[5][];
      byteArrays[0] = GetDataSizeBytes((ulong)ID.Segment);  // 4 bytes.
      // Segment data size.
      byteArrays[1] = GetDataSizeEightBytes(mediaEndOffsetMS);  // 1 byte header (== 1) + 7 bytes of size.
      // byteArrays[2][0] is at segmentOffset.
      // byteArrays[2] will be an ID.SeekHead + ID.Void at a total size of DESIRED_SEEK_SIZE.
      byteArrays[3] = GetSegmentInfoBytes(duration, timeScale, isDeterministic);
      byteArrays[4] = GetTrackEntriesBytes(trackEntries);

      IList<SeekBlock> seekBlocks = new List<SeekBlock>();
      seekBlocks.Add(new SeekBlock(ID.Info, DESIRED_SEEK_SIZE));
      seekBlocks.Add(new SeekBlock(ID.Tracks, (ulong)(DESIRED_SEEK_SIZE + byteArrays[3].Length)));
      if (seekHeadOffsetMS > 0) seekBlocks.Add(new SeekBlock(ID.SeekHead, seekHeadOffsetMS));
      if (cuesOffsetMS > 0) seekBlocks.Add(new SeekBlock(ID.Cues, cuesOffsetMS));
      byteArrays[2] = GetSeekBytes(seekBlocks, DESIRED_SEEK_SIZE);

      return Utils.CombineByteArrays(byteArrays);
      // * The first 4 bytes of the return value are from GetDataSizeBytes((ulong)ID.Segment).
      // * The next 1 byte of the return value is 1, the prefix of the 7-byte data size in datasize.GetUInt64().
      // * The next 7 bytes of the return value the total size of `list', but that doesn't matter, because it would be
      //   overwritten just after WriteMkv has written all media data and cues to the file (so the total file size is known).
      // return GetEEBytes(ID.Segment, GetEBMLBytes(list), true);
    }

    // Can't be larger, because the datasize class cannot serialize much larger values than that.
    private const ulong INITIAL_MEDIA_END_OFFSET_MS = ulong.MaxValue >> 9;
    private const ulong INITIAL_SEEK_HEAD_OFFSET_MS = 0;
    private const ulong INITIAL_CUES_OFFSET_MS = 0;
    private const ulong KEEP_ORIGINAL_DURATION = ulong.MaxValue - 1;

    // Returns the first offset not updated.
    // timeScale is ignored if duration == KEEP_ORIGINAL_DURATION.
    private static int UpdatePrefix(byte[] prefix, int prefixSize,
                                    ulong segmentOffset, ulong mediaEndOffsetMS, ulong seekHeadOffsetMS, ulong cuesOffsetMS,
                                    ulong duration, ulong timeScale) {
      Buffer.BlockCopy(Utils.InplaceReverseBytes(BitConverter.GetBytes(mediaEndOffsetMS)), 1,
                       prefix, (int)segmentOffset - 7, 7);
      int durationOffset;
      int afterInfoOffset;
      FindDurationAndAfterInfoOffset(prefix, (int)segmentOffset, prefixSize, out durationOffset, out afterInfoOffset);
      if (duration != KEEP_ORIGINAL_DURATION) {
        Buffer.BlockCopy(GetDurationBytes(duration, timeScale), 0, prefix, durationOffset, 4);
      }
      IList<SeekBlock> seekBlocks = new List<SeekBlock>();
      seekBlocks.Add(new SeekBlock(ID.Info, DESIRED_SEEK_SIZE));
      seekBlocks.Add(new SeekBlock(ID.Tracks, (ulong)afterInfoOffset - segmentOffset));
      if (seekHeadOffsetMS > 0) seekBlocks.Add(new SeekBlock(ID.SeekHead, seekHeadOffsetMS));
      if (cuesOffsetMS > 0) seekBlocks.Add(new SeekBlock(ID.Cues, cuesOffsetMS));
      byte[] seekBytes = GetSeekBytes(seekBlocks, DESIRED_SEEK_SIZE);
      Buffer.BlockCopy(seekBytes, 0, prefix, (int)segmentOffset, seekBytes.Length);
      return durationOffset + 4;
    }

    private static int GetEbmlElementDataSize(byte[] bytes, ref int i) {
      // Width  Size  Representation
      //   1    2^7   1xxx xxxx
      //   2    2^14  01xx xxxx  xxxx xxxx
      //   3    2^21  001x xxxx  xxxx xxxx  xxxx xxxx
      //   4    2^28  0001 xxxx  xxxx xxxx  xxxx xxxx  xxxx xxxx
      // ..7.
      if (bytes.Length <= i) {
        throw new Exception("EOF in EBML length.");
      }
      if ((bytes[i] & 0x80) != 0) {
        return bytes[i++] & 0x7f;
      } else if ((bytes[i] & 0x40) != 0) {
        i += 2;
        if (bytes.Length < i) {
          throw new Exception("EOF in EBML length 2.");
        }
        return (bytes[i - 2] & 0x3f) << 8 | bytes[i - 1];
      } else if (bytes[i] == 1) {
        i += 8;
        if (bytes.Length < i) {
          throw new Exception("EOF in EBML length 8.");
        }
        if (bytes[i - 5] != 0 || bytes[i - 6] != 0 || bytes[i - 7] != 0 || (bytes[i - 4] & 0x80) != 0) {
          throw new Exception("EBML length 8 too large for an int.");
        }
        return bytes[i - 1] | bytes[i - 2] << 8 | bytes[i - 3] << 16 | bytes[i - 4] << 24;
      } else {
        throw new Exception("Long EBML elements not implemented.");
      }
    }

    // Sets durationOffset to the 4 bytes in `bytes' containing the floatDuration field.
    // Sets afterInfoOffset to the offset right after the ID.Info element.
    // `bytes' is the prefix on an .mkv file written by us, with ID.Segment starting at segmentOffset or 0.
    // `j' is the end offset in bytes.
    private static void FindDurationAndAfterInfoOffset(byte[] bytes, int segmentOffset, int j,
                                                       out int durationOffset, out int afterInfoOffset) {
      int i = segmentOffset;
      // Skip ID.EBML if present.
      // if (i + 4 <= j && bytes[i] == 26 && bytes[i + 1] == 69 && bytes[i + 2] == 223 && bytes[i + 3] == 163) {
      // 	i += 4; i += ... GetEbmlElementDataSize(bytes, ref i);
      // }
      // Skip ID.SeekHead if present.
      if (i + 4 <= j && bytes[i] == 17 && bytes[i + 1] == 77 && bytes[i + 2] == 155 && bytes[i + 3] == 116) {
        i += 4;
        int n = GetEbmlElementDataSize(bytes, ref i);  // Doesn't work (i becomes 68 instead of 76) without a helper.
        i += n;
      }
      // Skip ID.Void if present.
      if (i < j && bytes[i] == 236) {
        ++i;
        int n = GetEbmlElementDataSize(bytes, ref i);  // Doesn't work (i becomes 68 instead of 76) without a helper.
        i += n;
      }
      // Detect ID.Info.
      if (!(i + 4 <= j && bytes[i] == 21 && bytes[i + 1] == 73 && bytes[i + 2] == 169 && bytes[i + 3] == 102)) {
        throw new Exception("Expected ID.Info.");
      }
      i += 4;
      int infoSize = GetEbmlElementDataSize(bytes, ref i);
      afterInfoOffset = i + infoSize;
      if (j > i + infoSize) j = i + infoSize;
      // Detect ID.Duration.
      if (!(i + 2 <= j && bytes[i] == 68 && bytes[i + 1] == 137)) {
        throw new Exception("Expected ID.Duration.");
      }
      i += 2;
      int durationSize = GetEbmlElementDataSize(bytes, ref i);
      if (durationSize != 4) {
        throw new Exception("Bad durationSize.");
      }
      durationOffset = i;
    }

    private static int GetVideoTrackIndex(IList<TrackEntry> trackEntries, int defaultIndex) {
      int videoTrackIndex = 0;
      while (videoTrackIndex < trackEntries.Count && trackEntries[videoTrackIndex].TrackType != TrackType.Video) {
        ++videoTrackIndex;
      }
      return (videoTrackIndex == trackEntries.Count) ? defaultIndex : videoTrackIndex;
    }

    private static IList<bool> GetIsAmsCodecs(IList<TrackEntry> trackEntries) {
      IList<bool> isAmsCodecs = new List<bool>();
      for (int i = 0; i < trackEntries.Count; ++i) {
        isAmsCodecs.Add(trackEntries[i].CodecID == CodecID.A_MS);
      }
      return isAmsCodecs;
    }

    private static byte[] GetSimpleBlockBytes(ulong trackNumber, short timeCode, bool IsKeyFrame, bool isAmsCodec,
                                              int mediaDataBlockTotalSize) {
      // Was: LacingID lacingId = isAmsCodec ? LacingID.FixedSize : LacingID.No;
      byte b = isAmsCodec ? (byte)4 : (byte)0;
      if (IsKeyFrame) {
        b += 128;
      }
      // Originally b was always initialized to 0, and then incremented like this:
      // switch (lacingId) {
      //	case LacingID.No: { break; }
      //	case LacingID.Xiph: { b += 2; break; }
      //	case LacingID.EBML: { b += 6; break; }
      //	case LacingID.FixedSize: { b += 4; break; }
      // }
      List<byte[]> output = new List<byte[]>();
      output.Add(GetDataSizeBytes((ulong)ID.SimpleBlock));
      output.Add(null);  // Reserved for the return value of GetDataSizeBytes.
      output.Add(GetDataSizeBytes(trackNumber));
      output.Add(Utils.InplaceReverseBytes(BitConverter.GetBytes(timeCode)));
      output.Add(new byte[] { b });
      // Was: if (lacingId != LacingID.No) output.Add(new byte[] { (byte)(sampleData.Count - 1) });
      if (isAmsCodec) output.Add(new byte[] { (byte)1 });
      int totalSize = 0;
      for (int i = 2; i < output.Count; ++i) {
        totalSize += output[i].Length;
      }
      output[1] = GetDataSizeBytes((ulong)(totalSize + mediaDataBlockTotalSize));
      // Usually output[0].Length == 3, and the length of the rest of output (without sampleData) is 4.
      return Utils.CombineByteArrays(output);
    }

    public static byte[] GetCueBytes(IList<CuePoint> cuePoints) {
      byte[][] output = new byte[cuePoints.Count][];
      for (int i = 0; i < cuePoints.Count; i++) {
        output[i] = cuePoints[i].GetBytes();
      }
      // TODO: Avoid unnecessary copies, also in GetEEBytes.
      return GetEEBytes(ID.Cues, Utils.CombineByteArrays(output));
    }

    private class StateChunkStartTimeReceiver : IChunkStartTimeReceiver {
      private MuxStateWriter MuxStateWriter;
      private IList<ulong>[] TrackChunkStartTimes;
      private int[] TrackChunkWrittenCounts;
      // Takes ownership of trackChunkStartTimes (and will append to its items).
      public StateChunkStartTimeReceiver(MuxStateWriter muxStateWriter, IList<ulong>[] trackChunkStartTimes) {
        this.MuxStateWriter = muxStateWriter;
        this.TrackChunkStartTimes = trackChunkStartTimes;
        this.TrackChunkWrittenCounts = new int[trackChunkStartTimes.Length];  // Initializes items to 0.
        for (int trackIndex = 0; trackIndex < trackChunkStartTimes.Length; ++trackIndex) {
          IList<ulong> chunkStartTimes = trackChunkStartTimes[trackIndex];
          if (chunkStartTimes == null) {
            trackChunkStartTimes[trackIndex] = chunkStartTimes = new List<ulong>();
          } else {
            int chunkCount = chunkStartTimes.Count;
            for (int chunkIndex = 1; chunkIndex < chunkCount; ++chunkIndex) {
              if (chunkStartTimes[chunkIndex - 1] >= chunkStartTimes[chunkIndex]) {
                throw new Exception(string.Concat(new object[] {
                    "Chunk StartTimes not increasing: track=", trackIndex,
                    " chuunk=", chunkIndex }));
              }
            }
          }
          this.TrackChunkWrittenCounts[trackIndex] = trackChunkStartTimes[trackIndex].Count;
        }
      }
      /*implements*/ public ulong GetChunkStartTime(int trackIndex, int chunkIndex) {
        IList<ulong> chunkStartTimes = this.TrackChunkStartTimes[trackIndex];
        return chunkIndex >= chunkStartTimes.Count ? ulong.MaxValue : chunkStartTimes[chunkIndex];
      }
      /*implements*/ public void SetChunkStartTime(int trackIndex, int chunkIndex, ulong chunkStartTime) {
        int chunkCount = this.TrackChunkStartTimes[trackIndex].Count;
        if (chunkIndex == chunkCount) {  // A simple append.
          IList<ulong> chunkStartTimes = this.TrackChunkStartTimes[trackIndex];
          if (chunkCount > 0) {
            ulong lastChunkStartTime = chunkStartTimes[chunkCount - 1];
            if (lastChunkStartTime >= chunkStartTime) {
              throw new Exception(string.Concat(new object[] {
                  "New chunk StartTime not larger: track=", trackIndex, " chunk=", chunkIndex,
                  " last=", lastChunkStartTime, " new=", chunkStartTime }));
            }
          }
          chunkStartTimes.Add(chunkStartTime);
          ++chunkCount;
          // Flush all chunk StartTimes not written to the .muxstate yet. Usually we write only one item
          // (chunkStartTime) here.
          int i = this.TrackChunkWrittenCounts[trackIndex];
          char key = (char)('n' + trackIndex);
          if (i == 0) this.MuxStateWriter.WriteUlong(key, chunkStartTimes[i++]);
          for (; i < chunkCount; ++i) {
            this.MuxStateWriter.WriteUlong(key, chunkStartTimes[i] - chunkStartTimes[i - 1]);
          }
          // There is no need to call this.MuxStateWriter.Flush(); here. it's OK to flush that later.
          this.TrackChunkWrittenCounts[trackIndex] = i;
        } else if (chunkIndex < chunkCount) {
          ulong oldChunkStartTime = this.TrackChunkStartTimes[trackIndex][chunkIndex];
          if (chunkStartTime != oldChunkStartTime) {
            throw new Exception(string.Concat(new object[] {
                "Chunk StartTime mismatch: track=", trackIndex, " chunk=", chunkIndex,
                " old=", oldChunkStartTime, " new=", chunkStartTime }));
          }
        } else {
          throw new Exception(string.Concat(new object[] {
              "Chunk StartTime set too far: track=", trackIndex, " chunk=", chunkIndex,
              " chunkCount=" + chunkCount }));
        }
      }
    }

    // Calls fileStream.Position and fileStream.Write only.
    //
    // Usually trackSamples has 2 elements: a video track and an audio track.
    //
    // Uses trackEntries and trackSamples only as a read-only argument, doesn't modify their contents.
    //
    // Starts with the initial cue points specified in cuePoints, and appends subsequent cue points in place.
    private static void WriteClustersAndCues(FileStream fileStream,
                                                         ulong segmentOffset,
                                                         int videoTrackIndex,
                                                         IList<bool> isAmsCodecs,
                                                         IMediaDataSource mediaDataSource,
                                                         MuxStateWriter muxStateWriter,
                                                         IList<CuePoint> cuePoints,
                                                         ref ulong minStartTime,
                                                         ulong timePosition,
                                                         out ulong seekHeadOffsetMS,
                                                         out ulong cuesOffsetMS) {
      int trackCount = mediaDataSource.GetTrackCount();
      if (isAmsCodecs.Count != trackCount) {
        throw new Exception("ASSERT: isAmsCodecs vs mediaDataSource length mismatch.");
      }
      if (trackCount > 13) {  // 13 is because a..m and n..z in MuxStateWriter checkpointing.
        throw new Exception("Too many tracks to mux.");
      }
      // For each track, contains the data bytes of a media sample ungot (i.e. pushed back) after reading.
      // Initializes items to null (good).
      MediaDataBlock[] ungetBlocks = new MediaDataBlock[trackCount];
      ulong minStartTime0 = minStartTime;
      if (timePosition == ulong.MaxValue) {
        timePosition = 0;
        ulong maxStartTime = ulong.MaxValue;
        for (int i = 0; i < trackCount; ++i) {
          if ((ungetBlocks[i] = mediaDataSource.PeekBlock(i)) != null) {
            if (maxStartTime == ulong.MaxValue || maxStartTime < ungetBlocks[i].StartTime) {
              maxStartTime = ungetBlocks[i].StartTime;
            }
            mediaDataSource.ConsumeBlock(i);  // Since it was moved to ungetBlocks[i].
          }
        }
        for (int i = 0; i < trackCount; ++i) {
          MediaDataBlock block = mediaDataSource.PeekBlock(i);
          while (block != null && block.StartTime <= maxStartTime) {
            ungetBlocks[i] = block;  // Takes ownership.
            mediaDataSource.ConsumeBlock(i);
          }
          // We'll start each track (in ungetMediaSample[i]) from the furthest sample within maxStartTime.
        }
        int trackIndex2;
        if ((trackIndex2 = GetNextTrackIndex(mediaDataSource, ungetBlocks)) < 0) {
          throw new Exception("ASSERT: Empty media file, no samples.");
        }
        minStartTime = minStartTime0 = ungetBlocks[trackIndex2] != null ? ungetBlocks[trackIndex2].StartTime :
                  mediaDataSource.PeekBlock(trackIndex2).StartTime;
              muxStateWriter.WriteUlong('A', minStartTime0);
      }
      List<ArraySegment<byte>> output = new List<ArraySegment<byte>>();
      ulong[] lastOutputStartTimes = new ulong[trackCount];  // Items initialized to zero.
      int trackIndex;
      // timePosition is the beginning StartTime of the last output block written by fileStream.Write.
      while ((trackIndex = GetNextTrackIndex(mediaDataSource, ungetBlocks)) >= 0) {
        ulong timeCode;  // Will be set below.
        bool isKeyFrame;  // Will be set below.
        MediaDataBlock block0;  // Will be set below.
        MediaDataBlock block1 = null;  // May be set below.
        int mediaDataBlockTotalSize;  // Will be set below.
        {
          if ((block0 = ungetBlocks[trackIndex]) == null &&
              (block0 = mediaDataSource.PeekBlock(trackIndex)) == null) {
            throw new Exception("ASSERT: Reading from a track already at EOF.");
          }
          // Some kind of time delta for this sample.
          timeCode = block0.StartTime - timePosition - minStartTime0;
          if (block0.StartTime < timePosition + minStartTime0) {
            throw new Exception("Bad start times: block0.StartTime=" + block0.StartTime +
                                " timePosition=" + timePosition + " minStartTime=" + minStartTime0);
          }
          isKeyFrame = block0.IsKeyFrame;
          mediaDataBlockTotalSize = block0.Bytes.Count;
          if (ungetBlocks[trackIndex] != null) {
            ungetBlocks[trackIndex] = null;
          } else {
            mediaDataSource.ConsumeBlock(trackIndex);
          }
        }
        if (timeCode > 327670000uL) {
          throw new Exception("timeCode too large: " + timeCode);  // Maybe that's not fatal?
        }
        if (isAmsCodecs[trackIndex]) {  // Copy one more MediaSample if available.
          // TODO: Test this.
          block1 = ungetBlocks[trackIndex];
          if (block1 != null) {
            mediaDataBlockTotalSize += block1.Bytes.Count;
            ungetBlocks[trackIndex] = null;
          } else if ((block1 = mediaDataSource.PeekBlock(trackIndex)) != null) {
            mediaDataBlockTotalSize += block1.Bytes.Count;
            mediaDataSource.ConsumeBlock(trackIndex);
          }
        }
        // TODO: How can be timeCode so large at this point?
        if ((output.Count != 0 && trackIndex == videoTrackIndex && isKeyFrame) || timeCode > 327670000uL) {
          ulong outputOffset = (ulong)fileStream.Position - segmentOffset;
          cuePoints.Add(new CuePoint(timePosition / 10000uL, (ulong)(videoTrackIndex + 1), outputOffset));
          muxStateWriter.WriteUlong('C', timePosition);
          muxStateWriter.WriteUlong('D', outputOffset);
          int totalSize = 0;
          for (int i = 0; i < output.Count; ++i) {
            totalSize += output[i].Count;
          }
          // We do a single copy of the media stream data bytes here. That copy is inevitable, because it's
          // faster to save to file that way.
          byte[] bytes = Utils.CombineByteArraysAndArraySegments(
              new byte[][]{GetDataSizeBytes((ulong)ID.Cluster), GetDataSizeBytes((ulong)totalSize)}, output);
          output.Clear();
          // The average bytes.Length is 286834 bytes here, that's large enough (>8 kB), and it doesn't warrant a
          // a buffered output stream for speedup.
          fileStream.Write(bytes, 0, bytes.Length);
          fileStream.Flush();
          for (int i = 0; i < trackCount; ++i) {
            muxStateWriter.WriteUlong((char)('a' + i), lastOutputStartTimes[i]);
          }
          muxStateWriter.WriteUlong('P', (ulong)bytes.Length);
          muxStateWriter.Flush();
        }
        if (output.Count == 0) {
          timePosition += timeCode;
          timeCode = 0uL;
          output.Add(new ArraySegment<byte>(
              GetEEBytes(ID.Timecode, GetVintBytes(timePosition / 10000uL))));
        }
        output.Add(new ArraySegment<byte>(GetSimpleBlockBytes(
            (ulong)(trackIndex + 1), (short)(timeCode / 10000uL), isKeyFrame, isAmsCodecs[trackIndex],
            mediaDataBlockTotalSize)));
        output.Add(block0.Bytes);
        if (block1 != null) output.Add(block1.Bytes);
        lastOutputStartTimes[trackIndex] = block1 != null ? block1.StartTime : block0.StartTime;
      }

      // Write remaining samples (from output to fileStream), and write cuePoints.
      {
        ulong outputOffset = (ulong)fileStream.Position - segmentOffset;
        cuePoints.Add(new CuePoint(timePosition / 10000uL, (ulong)(videoTrackIndex + 1), outputOffset));
        muxStateWriter.WriteUlong('C', timePosition);
        muxStateWriter.WriteUlong('D', outputOffset);
        if (output.Count == 0) {
          throw new Exception("ASSERT: Expecting non-empty output at end of mixing.");
        }
        int totalSize = 0;
        for (int i = 0; i < output.Count; ++i) {
          totalSize += output[i].Count;
        }
        byte[] bytes = Utils.CombineByteArraysAndArraySegments(
            new byte[][]{GetDataSizeBytes((ulong)ID.Cluster), GetDataSizeBytes((ulong)totalSize)}, output);
        output.Clear();  // Save memory.
        cuesOffsetMS = outputOffset + (ulong)bytes.Length;
        byte[] bytes2 = GetCueBytes(cuePoints);  // cues are about 1024 bytes per 2 minutes.
        seekHeadOffsetMS = cuesOffsetMS + (ulong)bytes2.Length;
        SeekBlock[] seekBlocks = new SeekBlock[cuePoints.Count];
        for (int i = 0; i < cuePoints.Count; ++i) {
          seekBlocks[i] = new SeekBlock(ID.Cluster, cuePoints[i].CueClusterPosition);
        }
        byte[] bytes3 = GetSeekBytes(seekBlocks, -1);
        bytes = Utils.CombineBytes(bytes, bytes2, bytes3);
        fileStream.Write(bytes, 0, bytes.Length);
      }
    }

    // Returns trackIndex with the smallest StartTime, or -1.
    private static int GetNextTrackIndex(IMediaDataSource mediaDataSource, MediaDataBlock[] ungetBlocks) {
      int trackCount = ungetBlocks.Length;  // == mediaDataSource.GetTrackCount().
      ulong minUnconsumedStartTime = 0;  // No real need to initialize it here.
      int trackIndex = -1;
      for (int i = 0; i < trackCount; ++i) {
        MediaDataBlock block = ungetBlocks[i];
        if (block == null) block = mediaDataSource.PeekBlock(i);
        if (block != null && (trackIndex == -1 || minUnconsumedStartTime > block.StartTime)) {
          trackIndex = i;
          minUnconsumedStartTime = block.StartTime;
        }
      }
      return trackIndex;
    }

    private const ulong MUX_STATE_VERSION = 923840374526694867;

    // This is a pure data class, please don't add logic.
    private class ParsedMuxState {
      public string status;
      public bool hasZ;
      public ulong vZ;
      public bool hasM;
      public ulong vM;
      public bool hasS;
      public ulong vS;
      public bool hasA;
      public ulong vA;
      public bool isXGood;
      public bool hasX;
      public ulong vX;
      public bool hasV;
      public ulong vV;
      public bool hasH;
      public byte[] vH;
      public IList<CuePoint> cuePoints;
      public bool isComplete;
      public bool isContinuable;
      public ulong lastOutOfs;
      public bool hasC;
      public ulong lastC;
      // this.trackLastStartTimes[trackIndex] is a StartTime lower limit. When muxing is continued, MediaDataBlock()s with
      // .StartTime <= the limit must be ignored (consumed).
      public ulong[] trackLastStartTimes;
      public IList<ulong>[] trackChunkStartTimes;
      public int endOffset;
      public ParsedMuxState() {
        this.isXGood = false;
        this.hasZ = this.hasM = this.hasS = this.hasX = this.hasV = this.hasH = this.hasC = this.hasA = false;
        this.isComplete = this.isContinuable = false;
        this.vZ = this.vM = this.vS = this.vX = this.vV = this.vA = 0;
        this.vH = null;
        this.status = "unparsed";
        this.cuePoints = null;
        this.lastOutOfs = 0;
        this.trackLastStartTimes = null;
        this.trackChunkStartTimes = null;
        this.endOffset = 0;
        this.lastC = 0;
      }
      public override String ToString() {
        StringBuilder buf = new StringBuilder();
        buf.Append("ParsedMuxState(status=" + Utils.EscapeString(this.status));
        if (this.isComplete) {
          buf.Append(", Complete");
        } else if (this.isContinuable) {
          buf.Append(", Continuable");
        } else {
          buf.Append(", Unusable");
        }
        if (this.isXGood) {
          buf.Append(", XGood");
        } else if (this.hasX) {
          buf.Append(", X=" + this.vX);
        }
        if (this.hasS) {
          buf.Append(", S=" + this.vS);
        }
        if (this.hasH) {
          buf.Append(", H.size=" + this.vH.Length);
        }
        if (this.hasA) {
          buf.Append(", A=" + this.vA);
        }
        if (this.hasV) {
          buf.Append(", V=" + this.vV);
        }
        if (this.hasC) {
          buf.Append(", lastC=" + this.lastC);
        }
        if (this.hasM) {
          buf.Append(", M=" + this.vM);
        }
        if (this.hasZ) {
          buf.Append(", Z=" + this.vZ);
        }
        if (this.cuePoints != null) {
          buf.Append(", cuePoints.size=" + this.cuePoints.Count);
        }
        if (this.lastOutOfs > 0) {
          buf.Append(", lastOutOfs=" + this.lastOutOfs);
        }
        if (this.trackLastStartTimes != null) {
          for (int i = 0; i < this.trackLastStartTimes.Length; ++i) {
            buf.Append(", lastStartTime[" + i + "]=" + this.trackLastStartTimes[i]);
          }
        }
        if (this.trackChunkStartTimes != null) {
          for (int i = 0; i < this.trackChunkStartTimes.Length; ++i) {
            buf.Append(", chunkStartTime[" + i + "].Size=" + this.trackChunkStartTimes[i].Count);
          }
        }
        buf.Append(")");
        return buf.ToString();
      }
    }

    private static ParsedMuxState ParseMuxState(byte[] muxState, ulong oldSize, byte[] prefix, int prefixSize,
                                                int videoTrackIndex, int trackCount) {
      ParsedMuxState parsedMuxState = new ParsedMuxState();
      if (muxState == null) {
        parsedMuxState.status = "no mux state";
        return parsedMuxState;
      }
      if (muxState.Length == 0) {
        parsedMuxState.status = "empty mux state";
        return parsedMuxState;
      }
      if (oldSize == 0) {
        parsedMuxState.status = "empty old file";
        return parsedMuxState;
      }
      if (prefixSize == 0) {
        parsedMuxState.status = "empty old prefix";
        return parsedMuxState;
      }

      // muxState might be truncated, so we find a sensible end offset to parse until.
      int end = 0;
      byte b;
      int i = muxState.Length;
      int j;
      if (i > 0 && (b = muxState[i - 1]) != '\n' && b != '\r') {  // Ignore the last, incomplete line.
        while (i > 0 && (b = muxState[i - 1]) != '\n' && b != '\r') {
          --i;
        }
        if (i > 0) --i;
      }
      for (;;) {  // Traverse the lines backwards.
        while (i > 0 && ((b = muxState[i - 1]) == '\n' || b == '\r')) {
          --i;
        }
        if (i == 0) break;
        j = i;
        while (i > 0 && (b = muxState[i - 1]) != '\n' && b != '\r') {
          --i;
        }
        // Found non-empty line muxState[i : j] (without trailing newlines).
        // Console.WriteLine("(" + Encoding.ASCII.GetString(Utils.GetSubBytes(muxState, i, j)) + ")");
        if (muxState[i] == 'Z' || muxState[i] == 'P') {  // Stop just after the last line starting with Z or P.
          end = j + 1;  // +1 for the trailing newline.
          break;
        }
      }
      if (end == 0) {
        parsedMuxState.status = "truncated to useless";
        return parsedMuxState;
      }
      parsedMuxState.endOffset = end;

      // Parse muxState[:end].
      // Output block state. Values:
      // * -5: expecting Z or after Z
      // * -4: expecting X, S, H or V
      // * -3: expecting A
      // * -2: expecting C
      // * -1: expecting D
      // * 0: expecting a (trackIndex == 0) or M
      // * 1: expecting b (trackIndex == 1)
      // * ...
      // * trackCount: expecting P
      int outState = -4;  // Output block state: -1 before V,
      parsedMuxState.lastC = ulong.MaxValue;
      ulong lastD = ulong.MaxValue;
      i = 0;
      if (i >= end || muxState[i] != 'X') {
        parsedMuxState.status = "expected key X in the beginning";
        return parsedMuxState;
      }
      while (i < end) {
        byte key = muxState[i++];
        if (key == '\r' || key == '\n') continue;
        bool doCheckDup = false;
        if (key == 'X' || key == 'S' || key == 'V' || key == 'A' || key == 'M' || key == 'Z' ||
            key == 'C' || key == 'D' || key == 'P' || (uint)(key - 'a') < 26) {
          ulong v = 0;
          while (i < end && (b = muxState[i]) != '\n' && b != '\r') {
            if (((uint)b - '0') > 9) {
              parsedMuxState.status = "expected ulong for key " + (char)key;
              return parsedMuxState;
            }
            if (v > (ulong.MaxValue - (ulong)(b - '0')) / 10) {
              parsedMuxState.status = "ulong overflow for key " + (char)key;
              return parsedMuxState;
            }
            v = 10 * v + (ulong)(b - '0');
            ++i;
          }
          if (i == end) {
            parsedMuxState.status = "EOF in key " + (char)key;
            return parsedMuxState;
          }
          if (key == 'X' && outState == -4) {
            doCheckDup = parsedMuxState.hasX; parsedMuxState.hasX = true; parsedMuxState.vX = v;
            parsedMuxState.isXGood = (v == MUX_STATE_VERSION);
            if (!parsedMuxState.isXGood) {
              parsedMuxState.status = "unsupported format version (X)";
              return parsedMuxState;
            }
          } else if (key == 'S' && outState == -4) {
            doCheckDup = parsedMuxState.hasS; parsedMuxState.hasS = true; parsedMuxState.vS = v;
          } else if (key == 'V' && outState == -4) {
            doCheckDup = parsedMuxState.hasV; parsedMuxState.hasV = true; parsedMuxState.vV = v;
            outState = -3;
          } else if (key == 'A' && outState == -3) {
            doCheckDup = parsedMuxState.hasA; parsedMuxState.hasA = true; parsedMuxState.vA = v;
            outState = -2;
          } else if (key == 'M' && outState == 0) {
            doCheckDup = parsedMuxState.hasM; parsedMuxState.hasM = true; parsedMuxState.vM = v;
            outState = -5;
          } else if (key == 'Z' && outState == -5) {
            doCheckDup = parsedMuxState.hasZ; parsedMuxState.hasZ = true; parsedMuxState.vZ = v;
          } else if (key == 'C' && outState == -2) {
            outState = -1;
            parsedMuxState.lastC = v;
          } else if (key == 'D' && outState == -1) {
            outState = 0;
            if (parsedMuxState.cuePoints == null) {
              parsedMuxState.cuePoints = new List<CuePoint>();
            }
            parsedMuxState.cuePoints.Add(new CuePoint(
                parsedMuxState.lastC / 10000uL, (ulong)(videoTrackIndex + 1), v));
            lastD = v;
            if (parsedMuxState.trackLastStartTimes == null) {
              parsedMuxState.trackLastStartTimes = new ulong[trackCount];  // Initialized to 0. Good.
            }
          } else if ((uint)(key - 'a') < 13 && outState < trackCount && outState == key - 'a') {
            if (v <= parsedMuxState.trackLastStartTimes[outState]) {
              parsedMuxState.status = "trackLastStart time values must increase, got " + v +
                  ", expected > " + parsedMuxState.trackLastStartTimes[outState];
              return parsedMuxState;
            }
            parsedMuxState.trackLastStartTimes[outState] = v;
            ++outState;
          } else if ((uint)(key - 'n') < 13 && outState >= -3) {
            if (parsedMuxState.trackChunkStartTimes == null) {
              parsedMuxState.trackChunkStartTimes = new IList<ulong>[trackCount];
              for (int ti = 0; ti < trackCount; ++ti) {
                parsedMuxState.trackChunkStartTimes[ti] = new List<ulong>();
              }
            }
            int trackIndex = key - 'n';
            int chunkCount = parsedMuxState.trackChunkStartTimes[trackIndex].Count;
            if (chunkCount > 0) {
              ulong lastChunkStartTime =
                  parsedMuxState.trackChunkStartTimes[trackIndex][chunkCount - 1];
              v += lastChunkStartTime;
              if (lastChunkStartTime >= v) {
                parsedMuxState.status = string.Concat(new object[] {
                    "trackChunkStartTime values must increase, got ", v, ", expected > ",
                    lastChunkStartTime, " for track ", trackIndex });
                return parsedMuxState;
              }
            }
            parsedMuxState.trackChunkStartTimes[trackIndex].Add(v);
          } else if (key == 'P' && outState == trackCount) {
            outState = -2;
            parsedMuxState.lastOutOfs = v + parsedMuxState.vS + lastD;
            lastD = ulong.MaxValue;  // A placeholder to expose future bugs.
          } else {
            parsedMuxState.status = "unexpected key " + (char)key + " in outState " + outState;
            return parsedMuxState;
          }
        } else if (key == 'H') {
          if (i == end || muxState[i] != ':') {
            parsedMuxState.status = "expected colon after hex key " + (char)key;
            return parsedMuxState;
          }
          j = ++i;
          while (i > 0 && (b = muxState[i]) != '\n' && b != '\r') {
            ++i;
          }
          byte[] bytes = Utils.HexDecodeBytes(muxState, j, i);
          if (bytes == null) {
            parsedMuxState.status = "parse error in hex key " + (char)key;
            return parsedMuxState;
          }
          if (key == 'H' && outState == -4) {
            doCheckDup = parsedMuxState.hasH; parsedMuxState.hasH = true; parsedMuxState.vH = bytes;
          } else {
            parsedMuxState.status = "unexpected key " + (char)key + " in outState " + outState;
            return parsedMuxState;
          }
        } else {
          parsedMuxState.status = "unknown key " + (char)key;
          return parsedMuxState;
        }
        if (doCheckDup) {
          parsedMuxState.status = "duplicate key " + (char)key;
          return parsedMuxState;
        }
      }
      if (outState != -5 && outState != -2) {
        parsedMuxState.status = "unexpected final outState " + outState;
        return parsedMuxState;
      }
      if (!parsedMuxState.hasV) {
        parsedMuxState.status = "missing video track index (V)";
        return parsedMuxState;
      }
      if (parsedMuxState.vV != (ulong)videoTrackIndex) {
        parsedMuxState.status = "video track index (V) mismatch, expected " + videoTrackIndex;
        return parsedMuxState;
      }
      if (!parsedMuxState.hasH) {
        parsedMuxState.status = "missing hex file prefix (H)";
        return parsedMuxState;
      }
      if (parsedMuxState.vH.Length < 10) {
        // This shouldn't happen, because we read 4096 bytes below, and the header is usually just 404 bytes long.
        parsedMuxState.status = "hex file prefix (H) too short";
        return parsedMuxState;
      }
      if (parsedMuxState.vH.Length > prefixSize) {
        // This shouldn't happen, because we read 4096 bytes below, and the header is usually just 404 bytes long.
        parsedMuxState.status = "hex file prefix (H) too long, maximum prefix size is " + prefixSize;
        return parsedMuxState;
      }
      if (!parsedMuxState.hasS) {
        parsedMuxState.status = "missing segmentOffset (S)";
        return parsedMuxState;
      }
      if (parsedMuxState.vS < 10 || parsedMuxState.vS > oldSize) {
        parsedMuxState.status = "bad video track index (V) range";
        return parsedMuxState;
      }
      if (!parsedMuxState.hasA) {
        parsedMuxState.status = "missing minStartTime (A)";
        return parsedMuxState;
      }
      if (parsedMuxState.hasZ) {
        if (parsedMuxState.vZ != 1) {
          parsedMuxState.status = "bad end marker (Z) value, expected 1";
          return parsedMuxState;
        }
        if (!parsedMuxState.hasM) {
          parsedMuxState.status = "missing key M";
          return parsedMuxState;
        }
        if (parsedMuxState.vM != oldSize) {
          parsedMuxState.status = "old file size (M) mismatch, expected " + oldSize;
          return parsedMuxState;
        }
      }
      // Console.WriteLine("H(" + Utils.HexEncodeString(Utils.GetSubBytes(prefix, 0, parsedMuxState.vH.Length)) + ")");
      if (!Utils.ArePrefixBytesEqual(parsedMuxState.vH, prefix, parsedMuxState.vH.Length)) {
        // We repeat the comparison with the mediaEndOffsetMS, seekHeadOffsetMS, cuesOffsetMS and duration fields
        // ignored. (So compare e.g. the .mkv format version and the track codec parameters.)
        //
        // If not complete yet (!parsedMuxState.hasZ), then the duration may be different; the other fields may be
        // different as well if WriteMkv has written the output of UpdatePrefix, but not the 'Z' value yet. If complete,
        // all the fields may be different (usually the duration is the same, and the other fields are different,
        // because they don't contain their INITIAL_* value anymore).
        //
        // We ignore the duration by copying it from one array to the other (it could work the other way around as well).
        // We ignore the other fields by setting them back to their INITIAL_* values before the comparison.
        int prefixCompareSize = parsedMuxState.vH.Length;  // Shortness is checked above.
        byte[] prefix1 = new byte[prefixCompareSize];
        Buffer.BlockCopy(parsedMuxState.vH, 0, prefix1, 0, prefixCompareSize);
        byte[] prefix2 = new byte[prefixCompareSize];
        Buffer.BlockCopy(prefix, 0, prefix2, 0, prefixCompareSize);
        UpdatePrefix(  // TODO: Catch exception if this fails.
            prefix2, prefixCompareSize, parsedMuxState.vS,
            INITIAL_MEDIA_END_OFFSET_MS, INITIAL_SEEK_HEAD_OFFSET_MS, INITIAL_CUES_OFFSET_MS,
            KEEP_ORIGINAL_DURATION, /*timeScale:*/0);
        int durationOffset;
        int afterInfoOffset;  // Ignored, dummy.
        // TODO: Catch exception if this fails.
        FindDurationAndAfterInfoOffset(prefix1, (int)parsedMuxState.vS, prefixCompareSize,
                                       out durationOffset, out afterInfoOffset);
        Buffer.BlockCopy(prefix1, durationOffset, prefix2, durationOffset, 4);
        if (!Utils.ArePrefixBytesEqual(prefix1, prefix2, prefixCompareSize)) {
          // Console.WriteLine("P(" + Utils.HexEncodeString(Utils.GetSubBytes(prefix, 0, parsedMuxState.vH.Length)) + ")");
          // Console.WriteLine("V(" + Utils.HexEncodeString(Utils.GetSubBytes(parsedMuxState.vH, 0, parsedMuxState.vH.Length)) + ")");
          parsedMuxState.status = "hex file prefix (H) mismatch";
          return parsedMuxState;
        }
      }
      if (parsedMuxState.hasZ) {
        parsedMuxState.isComplete = true;
        parsedMuxState.status = "complete";
      } else if (parsedMuxState.cuePoints == null || parsedMuxState.cuePoints.Count == 0) {
        parsedMuxState.status = "no cue points";
      } else if (parsedMuxState.lastOutOfs <= parsedMuxState.vS) {
        parsedMuxState.status = "no downloaded media data";
      } else if (parsedMuxState.lastOutOfs > oldSize) {
        parsedMuxState.status = "file shorter than lastOutOfs";
      } else if (parsedMuxState.trackChunkStartTimes == null) {
        parsedMuxState.status = "no chunk start times";
      } else {
        if (parsedMuxState.trackLastStartTimes == null) {
          throw new Exception("ASSERT: expected trackLastStartTimes.");
        }
        for (i = 0; i < trackCount; ++i) {
          if (parsedMuxState.trackLastStartTimes[i] == 0) {
            throw new Exception("ASSERT: expected positive trackLastStartTimes value.");
          }
        }
        parsedMuxState.isContinuable = true;
        parsedMuxState.status = "continuable";
      }
      return parsedMuxState;
    }

    // This function may modify trackSamples in a destructive way, to save memory.
    public static void WriteMkv(string mkvPath,
                                IList<TrackEntry> trackEntries,
                                IMediaDataSource mediaDataSource,
                                ulong maxTrackEndTimeHint,
                                ulong timeScale,
                                bool isDeterministic,
                                byte[] oldMuxState,
                                MuxStateWriter muxStateWriter) {
      if (trackEntries.Count != mediaDataSource.GetTrackCount()) {
        throw new Exception("ASSERT: trackEntries vs mediaDataSource length mismatch.");
      }
      bool doParseOldMuxState = oldMuxState != null && oldMuxState.Length > 0;
      FileMode fileMode = doParseOldMuxState ? FileMode.OpenOrCreate : FileMode.Create;
      using (FileStream fileStream = new FileStream(mkvPath, fileMode)) {
        ulong oldSize = doParseOldMuxState ? (ulong)fileStream.Length : 0uL;
        int videoTrackIndex = GetVideoTrackIndex(trackEntries, 0);
        bool isComplete = false;
        bool isContinuable = false;
        ulong lastOutOfs = 0;
        ulong segmentOffset = 0;  // Will be overwritten below.
        IList<CuePoint> cuePoints = null;
        ulong minStartTime = 0;
        ulong timePosition = ulong.MaxValue;
        byte[] prefix = null; // Well be overwritten below.
        if (doParseOldMuxState && oldSize > 0) {
          Console.WriteLine("Trying to use the old mux state to continue downloading.");
          prefix = new byte[4096];
          int prefixSize = fileStream.Read(prefix, 0, prefix.Length);
          ParsedMuxState parsedMuxState = ParseMuxState(
              oldMuxState, oldSize, prefix, prefixSize, videoTrackIndex, trackEntries.Count);
          if (parsedMuxState.isComplete) {
            Console.WriteLine("The .mkv file is already fully downloaded.");
            isComplete = true;
            // TODO: Don't even temporarily modify the .muxstate file.
            muxStateWriter.WriteRaw(oldMuxState, 0, oldMuxState.Length);
          } else if (parsedMuxState.isContinuable) {
            Console.WriteLine("Continuing the .mkv file download.");
            lastOutOfs = parsedMuxState.lastOutOfs;
            segmentOffset = parsedMuxState.vS;
            cuePoints = parsedMuxState.cuePoints;
            minStartTime = parsedMuxState.vA;
            timePosition = parsedMuxState.lastC;
            // We may save memory after this by trucating prefix to durationOffset + 4 -- but we don't care.
            muxStateWriter.WriteRaw(oldMuxState, 0, parsedMuxState.endOffset);
            mediaDataSource.StartChunks(new StateChunkStartTimeReceiver(
                muxStateWriter, parsedMuxState.trackChunkStartTimes));
            for (int i = 0; i < trackEntries.Count; ++i) {
              // Skip downloading most of the chunk files already in the .mkv (up to lastOutOfs).
              mediaDataSource.ConsumeBlocksUntil(i, parsedMuxState.trackLastStartTimes[i]);
            }
            isContinuable = true;
          } else {
            Console.WriteLine("Could not use old mux state: " + parsedMuxState);
          }
        }
        if (!isComplete) {
          fileStream.SetLength((long)lastOutOfs);
          fileStream.Seek((long)lastOutOfs, 0);
          if (!isContinuable) {  // Not continuing from previous state, writing an .mkv from scratch.
            // EBML: http://matroska.org/technical/specs/rfc/index.html
            // http://matroska.org/technical/specs/index.html
            prefix = GetEbmlHeaderBytes();
            segmentOffset = (ulong)prefix.Length + 12;
            muxStateWriter.WriteUlong('X', MUX_STATE_VERSION);  // Unique ID and version number.
            muxStateWriter.WriteUlong('S', segmentOffset);  // About 52.
            prefix = Utils.CombineBytes(prefix, GetSegmentBytes(
                /*duration:*/maxTrackEndTimeHint,
                INITIAL_MEDIA_END_OFFSET_MS, INITIAL_SEEK_HEAD_OFFSET_MS, INITIAL_CUES_OFFSET_MS,
                timeScale, trackEntries, isDeterministic));
            fileStream.Write(prefix, 0, prefix.Length);  // Write the MKV header.
            fileStream.Flush();
            muxStateWriter.WriteBytes('H', prefix);  // About 405 bytes long.
            muxStateWriter.WriteUlong('V', (ulong)videoTrackIndex);
            cuePoints = new List<CuePoint>();
            mediaDataSource.StartChunks(new StateChunkStartTimeReceiver(
                muxStateWriter, new IList<ulong>[trackEntries.Count]));
          }
          ulong seekHeadOffsetMS;  // Will be set by WriteClustersAndCues below.
          ulong cuesOffsetMS;  // Will be set by WriteClustersAndCues below.
          WriteClustersAndCues(
              fileStream, segmentOffset, videoTrackIndex, GetIsAmsCodecs(trackEntries),
              mediaDataSource, muxStateWriter, cuePoints, ref minStartTime, timePosition,
              out seekHeadOffsetMS, out cuesOffsetMS);
          fileStream.Flush();
          // Update the MKV header with the file size.
          ulong mediaEndOffset = (ulong)fileStream.Position;
          muxStateWriter.WriteUlong('M', mediaEndOffset);
          // Usually this seek position is 45.
          ulong maxTrackEndTime = 0;  // TODO: mkvmerge calculates this differently (<0.5s -- rounding?)
          for (int i = 0; i < mediaDataSource.GetTrackCount(); ++i) {
            ulong trackEndTime = mediaDataSource.GetTrackEndTime(i);
            if (maxTrackEndTime < trackEndTime) maxTrackEndTime = trackEndTime;
          }
          // Update the ID.Segment size and ID.Duration with their final values.
          int seekOffset = (int)segmentOffset - 7;
          // We update the final duration and some offsets in the .mkv header so mplayer (and possibly other
          // media players) will be able to seek in the file without additional tricks. More specifically:
          //
          //   			play-before	play-after	seek-before	seek-after
          //   mplayer		yes     	yes		no		yes
          //   mplayer -idx	yes     	yes		yes		yes
          //   mplayer2		yes     	yes		yes		yes
          //   mplayer2 -idx	yes     	yes		yes		yes
          //   VLC 1.0.x		no		no		no		no
          //   VLC 1.1.x		no		no		no		no
          //   VLC 2.0.x		?		yes		?		yes
          //   SMPlayer 0.6.9	?		yes		?		yes
          //
          // Legend:
          //
          // * mplayer: MPlayer SVN-r1.0~rc3+svn20090426-4.4.3 on Ubuntu Lucid
          // * mplayer2: MPlayer2 2.0 from http://ftp.mplayer2.org/pub/release/ , mtime 2011-03-26
          // * -idx; The -idx command-line flag of mplayer and mplayer2.
          // * play: playing the video sequentially from beginning to end
          // * seek: jumping back and forth within the video upon user keypress (e.g. the <Up> key),
          //   including jumping to regions of the .mkv which haven't been downloaded when playback started
          // * before: before running UpdatePrefix below, i.e. while the .mkv is being downloaded
          // * after: after running UpdatePrefix below
          //
          // VLC 1.0.x and VLC 1.1.x problems: audio is fine, but the video is jumping back and forth fraction of
          // a second.
          int updateOffset = UpdatePrefix(
               prefix, prefix.Length, segmentOffset,
                     mediaEndOffset - segmentOffset,
                     /*seekHeadOffsetMS:*/seekHeadOffsetMS,
                     /*cuesOffsetMS:*/cuesOffsetMS,
                     /*duration:*/maxTrackEndTime - minStartTime, timeScale);
          fileStream.Seek(seekOffset, 0);
          fileStream.Write(prefix, seekOffset, updateOffset - seekOffset);
          fileStream.Flush();
          muxStateWriter.WriteUlong('Z', 1);
          muxStateWriter.Flush();
        }
      }
    }
  }
}
