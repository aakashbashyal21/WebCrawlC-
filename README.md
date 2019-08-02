# WebCrawl in C#
Webcrawl and scrapping assignment implementation using C# and HtmlAgilityPack

This project contains the source code for scrapping the five nepali news portal. 
The nepali news portals are:

1.	https://nayapatrikadaily.com/
2.	https://www.kantipurdaily.com/
3.	https://www.onlinekhabar.com/
4.	https://www.setopati.com/
5.	https://www.nepalaaja.com

All the extracted article will be saved to the database:

And further it will calculate the term frequency of the word from the scrapped article.

Also, I have scanned the links from the hamropatro.com, for identifying as the hub or authority.

External Libary:

1. Hangfire (for scheduling extracting in background)
2. DateConversion (converting Bikramsabat to A.D)
3. EntityFramework (saving to sqlserver)
4. HtmlAgilityPack (HTML parser written in C# to read/write DOM)
 
