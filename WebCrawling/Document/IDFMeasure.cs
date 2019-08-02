using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawling.Controllers.Document
{
    public class IDFMeasure
    {
        private readonly string[] _docs;
        private readonly int _numDocs;
        private readonly int _numTerms;
        private readonly List<string> _terms;
        private float[][] _inverseDocFreq;
        private readonly int[] _docFreq;

        private readonly Dictionary<string, int> _wordIndex = new Dictionary<string, int>();
        public IDFMeasure(string[] documents)
        {
            _docs = documents;
            _numDocs = documents.Length;

            _terms = GenerateTerms(_docs).ToList();
            _numTerms = _terms.Count;

            _docFreq = new int[_numTerms];
            _wordIndex = new Dictionary<string, int>();

            FillWordIndexWithTerms();
            GenerateDocumentFrequency();
        }

        private static IList<string> GenerateTerms(IEnumerable<string> docs)
        {
            return docs.SelectMany(ProcessDocument).Distinct().ToList();
        }

        private void FillWordIndexWithTerms()
        {
            for (var termIndex = 0; termIndex < _terms.Count; termIndex++)
            {
                _wordIndex.Add(_terms[termIndex], termIndex);
            }
        }

        private void GenerateDocumentFrequency()
        {
            _inverseDocFreq = new float[_numDocs][];

            for (var i = 0; i < _numDocs; i++)
            {
                _inverseDocFreq[i] = new float[_numTerms];

                var curDoc = _docs[i];
                var freq = GetWordFrequency(curDoc);

                CalculateTermFrequency(freq);
            }
        }
        private void CalculateTermFrequency(Dictionary<string, int> freq)
        {
            foreach (var termIndex in freq.Select(entry => GetTermIndex(entry.Key)))
            {
                _docFreq[termIndex]++;
            }
        }
        private int GetTermIndex(string term)
        {
            return _wordIndex[term];
        }


        private static IEnumerable<string> ProcessDocument(string doc)
        {
            return doc.Split(' ')
             .GroupBy(word => word)
             .OrderByDescending(g => g.Count())
             .Select(g => g.Key);
        }
        private static Dictionary<string, int> GetWordFrequency(string input)
        {
            return input.Split(' ').GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}

