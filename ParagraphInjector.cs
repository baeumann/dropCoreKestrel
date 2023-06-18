using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace dropCoreKestrel
{
    public class ParagraphInjector
    {
        public const string INJECTION_MARK = "$(paragraphsMark)";
        public const string PARAGRAPH_ID_MARK = "$(paragraphId)";
        public const string PARAGRAPH_TITLE_MARK = "$(paragraphTitle)";
        public const string PARAGRAPH_CONTENT_MARK = "$(paragraphContent)";
        public const string IMAGE_NAME_MARK = "$(imageName)";
        public const string VIDEO_NAME_MARK = "$(videoName)";
        private string renderedParagraphs;

        public ParagraphInjector(string paragraphFile) {
            StringBuilder stringBuilder = new StringBuilder();
            string[] paragraphs = File.ReadAllLines(paragraphFile);
            int currentCount = 0;

            foreach(var currentParagraph in paragraphs) {
                string[] splitParagraph = currentParagraph.Split("|");

                stringBuilder.Append(CreateParagraph(splitParagraph[0], splitParagraph[1], currentCount));
                currentCount++;
            }

            renderedParagraphs = stringBuilder.ToString();
        }

        public string InjectInto(string input) {
            return input.Replace(INJECTION_MARK, renderedParagraphs);
        }

        private string CreateParagraph(string title, string content, int id) {
            string idString = "paragraphId" + id;

            string paragraphTemplate = File.ReadAllText("paragraph.template");
            string decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(content));
            decodedContent = Regex.Replace(decodedContent, @"\r\n?|\n", "<br>");
            decodedContent = InjectMedia(decodedContent, VIDEO_NAME_MARK, "video", File.ReadAllText("video.template"));
            decodedContent = InjectMedia(decodedContent, IMAGE_NAME_MARK, "image", File.ReadAllText("image.template"));


            return paragraphTemplate.Replace(PARAGRAPH_ID_MARK, idString)
                                    .Replace(PARAGRAPH_TITLE_MARK, title) 
                                    .Replace(PARAGRAPH_CONTENT_MARK, decodedContent);
        }

        private string InjectMedia(string sourceText, string mediaMark, string markPayload, string mediaTemplate) {
            var injectedText = sourceText;

            string imageTemplate = string.Empty;    

            foreach (var currentMatch in ExtractMarksWithPayload(sourceText, markPayload)) {
                string matchAsString = currentMatch.ToString();
                var payloads = ExtractPayloadsFromMarks(matchAsString); 
                var mediaName = payloads[0];

                injectedText = injectedText.Replace(matchAsString, mediaTemplate.Replace(mediaMark, mediaName));

            }

            return injectedText;
        }

        private MatchCollection ExtractMarksWithPayload(string sourceText, string mark) {
            string pattern = "(\\$\\(" + mark + "\\|).+?(\\))";

            return Regex.Matches(sourceText, pattern);
        }

        private string[] ExtractPayloadsFromMarks(string markWithPayload) {
            List<string> payloads = new List<string>();
            var splitString = markWithPayload.Split('|');

            for(int i= 1; i< splitString.Length; i++) {

                if(i == (splitString.Length-1)) {
                    payloads.Add(splitString[i].Substring(0, splitString[i].Length - 1));
                } else {
                    payloads.Add(splitString[i]);
                }

            }

            return payloads.ToArray();
        }
    }
}