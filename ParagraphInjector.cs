using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dropCoreKestrel
{
    public class ParagraphInjector
    {
        public const string INJECTION_MARK = "$(paragraphsMark)";
        public const string PARAGRAPH_ID_MARK = "$(paragraphId)";
        public const string PARAGRAPH_TITLE_MARK = "$(paragraphTitle)";
        public const string PARAGRAPH_CONTENT_MARK = "$(paragraphContent)";
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

        private string CreateParagraph(string title, string content, int id) {
            string idString = "paragraphId" + id;

            string paragraphTemplate = File.ReadAllText("paragraph.template");

            return paragraphTemplate.Replace(PARAGRAPH_ID_MARK, idString)
                                    .Replace(PARAGRAPH_TITLE_MARK, title)
                                    .Replace(PARAGRAPH_CONTENT_MARK, content);
        }

        public string InjectInto(string input) {

            return input.Replace(INJECTION_MARK, renderedParagraphs);
        }
    }
}