# ScrapeGaSos
This program scrapes the Georgia Secretary of State site of the list of registered campaigns at https://elections.sos.ga.gov/GAElection/CandidateDetails.

The program is a Microsoft Windows Forms program written in C#.  The development solution is divided into two projects: a console and worker library.

After selecting search parameters a POST gets a listing of the candidates.  The single scroll page data is formatted into CSV data.  Once all candidates are formatted the UI offers options to save the data to a CSV file.
