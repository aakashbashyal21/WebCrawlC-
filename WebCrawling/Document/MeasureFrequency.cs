using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawling.Controllers.Document
{
    public class MeasureFrequency
    {
        public string TermWords { get; set; }

        IEnumerable<string> stopWords = System.IO.File.ReadAllLines("c:\\stopwords01.txt");
        public Dictionary<string, int> dictionary = new Dictionary<string, int>();
        public string ProcessDocument(string[] content)
        {
            foreach (var doc in content)
            {
                string inputString = doc;

                string[] stripChars = { ";", ",", ".", "-", "_", "^", "(", ")",
                    "भन्दै","हुन्","गरे","भन्ने","समेत","दिन","वर्ष","सय","लाख","गर्दा","बाट",
                    "मात्रै","सुरु","अघि","रुपमा","दिएको","आएको","कारण","आएका","क्रममा",
                    "बताएका"," का ","भनाइ","थप","बढी","परेको","गते","जना","वटा"," एको ",
                    "भएपछि","लागेको","गत","विषय", "भएकाले","दिएका","कार्य","विभिन्न","पर्ने",
                    "उल्लेख","गर्नु","बनाउन","नभएको","परेका","क्षेत्रमा","हुँदा","अवस्था",
                    "सक्ने","लिएको","हुँदै","[", "]","|","।","०", "१", "२","३", "४","५","६",
                    "७", "८","९","—","’", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                    "\n", "\t", "\r",":","×" };

                string[] stringArray = { "सरकारले", "प्रहरीले", "प्रदेशले", "सरकारको" };

                foreach (string character in stripChars)
                {
                    try
                    {

                        inputString = WebUtility.HtmlDecode(inputString).Replace(character, "");
                    }
                    catch (Exception ex)
                    {

                    }
                }

                StringBuilder filterContent = new StringBuilder();

                var words = inputString.Split();
                //var newWords = words.Except(stripChars);

                var newWords = words.Except(stopWords);
                filterContent.Append(string.Join(" ", newWords));

                List<string> wordList = filterContent.ToString()
                                            .Split(' ').ToList();


                foreach (string word1 in wordList)
                {
                    string word = word1;

                    if (word.Length >= 5)
                    {

                        if (stringArray.Any(s => word1.Contains(s)) && (word.EndsWith("ले") || word.EndsWith("को")))
                        {
                            word = word.Substring(0, word.Length - 2);

                        }

                        if (dictionary.ContainsKey(word))
                        {
                            dictionary[word]++;
                        }
                        else
                        {
                            // Otherwise, if it's a new word then add it to the dictionary with an initial count of 1
                            dictionary[word] = 1;
                        }
                    }
                }
            }




            var sortedDict = (from entry in dictionary
                              orderby entry.Value descending
                              select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            // Loop through the sorted dictionary and output the top N most frequently occurring words
            int count = 1;


            foreach (KeyValuePair<string, int> pair in sortedDict)
            {

                TermWords += pair.Key.ToString() + "\t\t" + pair.Value.ToString() + "\r\n";
                count++;

                // Only display the top 10 words then break out of the loop!
                if (count > 20)
                {
                    break;
                }
            }

            return TermWords;
        }
    }
}