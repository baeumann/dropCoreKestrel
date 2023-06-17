using System.Text;

namespace dropCoreKestrel
{
    public class RequestStatistics {

        public long[] requestsThisWeek {get; private set;}
        public long requeustsThisDay {get; private set;}
        public long RequestRate {get; private set;}

        private int requestsThisSecond;
        private int currentDay;

        public RequestStatistics() {
            requestsThisWeek = new long[7];
            currentDay = DateTime.Now.DayOfYear;

            var statisticsThread = new Thread(Run);
            statisticsThread.Start();
        }

        private void Run() {
            while(true) {
                Thread.Sleep(1000);

                RequestRate = requestsThisSecond;
                requestsThisSecond = 0;

                if(currentDay != DateTime.Now.DayOfYear) {
                    PushWeekStack();

                    currentDay = DateTime.Now.DayOfYear;
                }
            }
        }

        public string RequestsOfLastSevenDaysAsString() {
            StringBuilder stringBuilder = new StringBuilder();

            for(int i = 0; i < requestsThisWeek.Length; i++) {
                if(i == requestsThisWeek.Length-1) {
                    stringBuilder.Append(requestsThisWeek[i]);
                } else {
                    stringBuilder.Append(requestsThisWeek[i] + "|");
                }
            }

            return stringBuilder.ToString();
        }

        public void RequestIncoming() {
            requeustsThisDay++;
            requestsThisSecond++;
        }

        private void PushWeekStack() {
            //push everything backwards (remove the last day)
            for(int i = (requestsThisWeek.Length-1); i > 0; i--) {
                requestsThisWeek[i] = requestsThisWeek[i-1];
            }
            //fill the new day into the first element
            requestsThisWeek[0] = requeustsThisDay;
            //reset the request counter
            requeustsThisDay = 0;
        }
    }
}
