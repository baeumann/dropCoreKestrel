using System.Text;

namespace dropCoreKestrel
{
    public class RequestStatistics {

        public long[] RequestsThisWeek {get; private set;}
        public long RequeustsThisDay {get; private set;}
        public long RequestRate {get; private set;}
        public long PeakRequestsPerSecond {get; private set;}

        private int requestsThisSecond;
        private int currentDay;

        private DateTime startTime;

  

        public RequestStatistics() {
            startTime = DateTime.Now;
            RequestsThisWeek = new long[7];
            PeakRequestsPerSecond = 0;
            currentDay = DateTime.Now.DayOfYear;

            var statisticsThread = new Thread(Run);
            statisticsThread.Start();
        }

        private void Run() {
            while(true) {
                Thread.Sleep(1000);

                RequestRate = requestsThisSecond;
                requestsThisSecond = 0;

                if(RequestRate > PeakRequestsPerSecond) {
                    PeakRequestsPerSecond = RequestRate;
                }

                if(currentDay != DateTime.Now.DayOfYear) {
                    PushWeekStack();
                    PeakRequestsPerSecond = 0;

                    currentDay = DateTime.Now.DayOfYear;
                }
            }
        }

        public string UptimeAsString() {
            var currentTime = DateTime.Now;
            TimeSpan uptime = currentTime.Subtract(startTime);
        
            return "Days " + string.Format("{0:N2}", uptime.TotalDays);
        }

        public string RequestsOfLastSevenDaysAsString() {
            StringBuilder stringBuilder = new StringBuilder();

            for(int i = 0; i < RequestsThisWeek.Length; i++) {
                if(i == RequestsThisWeek.Length-1) {
                    stringBuilder.Append(RequestsThisWeek[i]);
                } else {
                    stringBuilder.Append(RequestsThisWeek[i] + "|");
                }
            }

            return stringBuilder.ToString();
        }

        public void RequestIncoming() {
            RequeustsThisDay++;
            requestsThisSecond++;
        }

        private void PushWeekStack() {
            //push everything backwards (remove the last day)
            for(int i = (RequestsThisWeek.Length-1); i > 0; i--) {
                RequestsThisWeek[i] = RequestsThisWeek[i-1];
            }
            //fill the new day into the first element
            RequestsThisWeek[0] = RequeustsThisDay;
            //reset the request counter
            RequeustsThisDay = 0;
        }
    }
}
