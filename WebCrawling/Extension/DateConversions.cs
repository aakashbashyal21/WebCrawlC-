using NepaliDateConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawling.Extension
{
    public static class DateConversions
    {
        public static string ParseSetoPatiDate(this string date)
        {
            var aStrs = date.Replace(",", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();

            DateConverter converter = DateConverter.ConvertToEnglish(
                aStrs[3].Conversion(), //yy
                aStrs[1].MonthConversion(), //mm
                aStrs[2].Conversion() //dd
            );

            string adDate = $"{converter.Month}/{converter.Day}/{converter.Year}";

            return adDate;
        }
        public static string ParseNagarikDate(this string date)
        {
            var aStrs = date.Replace(",", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
            ///शनिबार, ११ साउन २०७६
            DateConverter converter = DateConverter.ConvertToEnglish(
                aStrs[3].Conversion(),
                aStrs[2].MonthConversion(),
                aStrs[1].Conversion()
            );

            string adDate = $"{converter.Month}/{converter.Day}/{converter.Year}";

            return adDate;
        }
        public static string ParseOnlineKhabarDate(this string date)
        {
            var aStrs = date.Replace(",", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
            ///शनिबार, ११ साउन २०७६
            DateConverter converter = DateConverter.ConvertToEnglish(
                aStrs[0].Conversion(),
                aStrs[1].MonthConversion(),
                aStrs[2].Conversion()
            );

            string adDate = $"{converter.Month}/{converter.Day}/{converter.Year}";

            return adDate;
        }
        public static string ParseKantipurDate(this string date)
        {
            var aStrs = date.Replace(",", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
            //श्रावण १२, २०७६
            DateConverter converter = DateConverter.ConvertToEnglish(
                aStrs[2].Conversion(),
                aStrs[0].MonthConversion(),
                aStrs[1].Conversion()
            );

            string adDate = $"{converter.Month}/{converter.Day}/{converter.Year}";

            return adDate;
        }
        public static int MonthConversion(this string str)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"वैशाख", "1"},
                {"बैशाख","1" },
                {"जेठ", "2"},
                {"जेष्ठ","2" },
                {"असार", "3"},
                {"साउन", "4"},
                {"श्रावण","4" },
                {"भदौ", "5"},
                {"भाद्र","5" },
                {"आश्विन","6" },
                {"असोज", "6"},
                {"कात्तिक", "7"},
                {"कार्तिक","7" },
                {"मंसिर", "8"},
                {"मङ्सिर","8" },
                {"मङि्सर","8" },
                {"पुस", "9"},
                {"पौष","9" },
                {"पुष","9" },
                {"माघ", "10"},
                {"फागुन", "11"},
                {"फाल्गुन","11" },
                {"फाल्गुण","11"},
                {"चैत", "12"},
                {"चैत्र","12" }
           };

            var newstr = dict[str];
            //dict.Aggregate(str, (current, value) =>
            //                current.Replace(value.Key, value.Value));

            return int.Parse(newstr);
        }
        public static int YearConversion(this string str)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
               {"२०७३","2073" },
               {"२०७४","2074"},
               {"२०७५","2075"},
               {"२०७६","2076"}
           };

            var newstr = dict.Aggregate(str, (current, value) =>
                                current.Replace(value.Key, value.Value));

            return int.Parse(newstr);
        }
        public static int Conversion(this string str)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"०","0" },
                {"१","1"},
                {"२","2"},
                {"३","3"},
                {"४","4"},
                {"५","5"},
                {"६","6"},
                {"७","7"},
                {"८","8"},
                {"९","9"}
           };

            var newstr = dict.Aggregate(str, (current, value) =>
            current.Replace(
                value.Key,
                value.Value
            ));


            return int.Parse(newstr);
        }

    }
}
